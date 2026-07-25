using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pig : MonoBehaviour
{
    static readonly int ColorPropertyId = Shader.PropertyToID("_Color");
    static readonly int BaseColorPropertyId = Shader.PropertyToID("_BaseColor");

    // -- SYSTEM -- //

    GameController gameController;
    bool isAlive = true;
    float originalGroundY;

    void Awake()
    {
        gameController = GameObject.FindFirstObjectByType<GameController>();
        originalGroundY = transform.position.y;
    }

    void Update()
    {
        if (!isAlive)
            return;

        CheckGround();
        UpdateAnimationState();
        UpdateSpeedProgression();
        UpdateForwardMovement();
        UpdateSideMovement();
        UpdateModelRotation();
        UpdateJump();
        UpdateDash();
        UpdateJumpHeight();
        UpdateFalling();
        UpdateCooldowns();
    }

    // -- MOVEMENT -- //

    [Header("MOVEMENT")]
    public float forwardSpeed = 5f;
    public float sideSpeed = 5f;
    public float jumpHeight = 2f;
    public float jumpDuration = 0.6f;
    public float groundCheckDistance = 0.1f;
    public float jumpCooldownDuration = 0.2f;
    public float speedIncreasePerSecond = 0.1f;
    public float dashForce = 3f;
    public float dashDuration = 0.1f;
    public float dashCooldownDuration = 0.5f;
    public float tiltAngle = 15f;
    public float tiltSpeed = 5f;
    public float rotationAngle = 15f;
    public Transform pigModelTransform;
    public Animator pigAnimator;
    public ParticleSystem dashParticleSystem;
    public GameObject waterSplashParticlesPrefab;
    public GameObject bushExplodeParticlesPrefab;

    public float fallGravity = 20f;
    public float maxFallSpeed = 25f;
    public float fallAccelerationMultiplier = 2f;
    public bool isGrounded;

    [Header("DEATH")]
    public float crashBackwardDistance = 3f;
    public float crashUpwardDistance = 4f;
    public float crashEndYOffset = -1f;
    public float crashSpinRevolutions = 2f;
    public float crashRandomZTiltMax = 20f;
    public float crashSidewaysDistance = 1.5f;
    public float waterDeathSinkDistance = 2f;

    float jumpCooldown;
    float dashCooldown;
    float dashSpeedTimer;
    float fallSpeed;
    float currentForwardSpeed;
    float currentSideSpeed;

    bool isJumping;
    bool hasJumpedSinceLastGrounded;
    string currentAnimationName;
    float jumpTimer;
    Vector3 jumpStartPosition;

    void Start()
    {
        currentForwardSpeed = forwardSpeed;
        currentSideSpeed = sideSpeed;
    }

    void UpdateSpeedProgression()
    {
        if (speedIncreasePerSecond <= 0f)
            return;

        currentForwardSpeed += speedIncreasePerSecond * Time.deltaTime;
        currentSideSpeed += speedIncreasePerSecond * Time.deltaTime;
    }

    void UpdateForwardMovement()
    {
        float currentSpeed = currentForwardSpeed;
        if (dashSpeedTimer > 0)
        {
            currentSpeed += dashForce;
        }

        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
    }

    void UpdateSideMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        transform.Translate(Vector3.right * horizontalInput * currentSideSpeed * Time.deltaTime);
    }

    void UpdateModelRotation()
    {
        if (pigModelTransform == null)
            return;

        float horizontalInput = Input.GetAxis("Horizontal");
        float targetTilt = -horizontalInput * tiltAngle;
        float targetRotation = horizontalInput * rotationAngle;

        Quaternion targetQuaternion = Quaternion.Euler(0f, targetRotation, targetTilt);
        pigModelTransform.localRotation = Quaternion.Lerp(pigModelTransform.localRotation, targetQuaternion, tiltSpeed * Time.deltaTime);
    }

    void UpdateAnimationState()
    {
        if (pigAnimator == null)
            return;

        string targetAnimation = isGrounded ? "Run" : "Jump";
        if (currentAnimationName == targetAnimation)
            return;

        pigAnimator.Play(targetAnimation);
        currentAnimationName = targetAnimation;
    }

    void UpdateJump()
    {
        bool canGroundJump = isGrounded && jumpCooldown <= 0f && !isJumping;
        bool canFallingJump = !isGrounded && !isJumping && !hasJumpedSinceLastGrounded && jumpCooldown <= 0f;

        if (Input.GetKey(KeyCode.Z) && (canGroundJump || canFallingJump))
        {
            isJumping = true;
            hasJumpedSinceLastGrounded = true;
            jumpTimer = 0f;
            jumpStartPosition = transform.position;
            jumpCooldown = jumpCooldownDuration;
            fallSpeed = 0f;

            if (AudioController.Instance != null)
            {
                AudioController.Instance.PlayJump();
            }
        }
    }

    void UpdateJumpHeight()
    {
        if (!isJumping)
            return;

        // Pause gravity while dashing
        if (dashSpeedTimer > 0)
            return;

        jumpTimer += Time.deltaTime;

        if (jumpTimer >= jumpDuration)
        {
            // Jump finished
            isJumping = false;
            jumpTimer = 0f;
            Vector3 pos = transform.position;
            pos.y = jumpStartPosition.y;
            transform.position = pos;
            return;
        }

        // Jump in progress - use sine curve for smooth arc
        float jumpProgress = jumpTimer / jumpDuration;
        float height = Mathf.Sin(jumpProgress * Mathf.PI) * jumpHeight;

        Vector3 jumpPos = transform.position;
        jumpPos.y = jumpStartPosition.y + height;
        transform.position = jumpPos;
    }

    void UpdateFalling()
    {
        if (isGrounded || isJumping)
            return;

        fallSpeed += fallGravity * fallAccelerationMultiplier * Time.deltaTime;
        if (fallSpeed > maxFallSpeed)
        {
            fallSpeed = maxFallSpeed;
        }

        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);
    }

    void UpdateDash()
    {
        if (Input.GetKeyDown(KeyCode.X) && dashCooldown <= 0f)
        {
            dashSpeedTimer = dashDuration;
            dashCooldown = dashCooldownDuration;

            if (dashParticleSystem != null)
            {
                dashParticleSystem.Play();
            }

            if (AudioController.Instance != null)
            {
                AudioController.Instance.PlayDash();
            }
        }
    }

    void CheckGround()
    {
        Vector3 checkPosition = transform.position + Vector3.down * (groundCheckDistance / 2f);
        Collider[] colliders = Physics.OverlapSphere(checkPosition, groundCheckDistance);
        isGrounded = false;

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject == gameObject)
                continue;

            if (collider.CompareTag("Ground"))
            {
                isGrounded = true;
                break;
            }
        }

        if (isGrounded)
        {
            fallSpeed = 0f;

            if (!isJumping)
            {
                hasJumpedSinceLastGrounded = false;
                SnapToOriginalGroundHeight();
            }
        }
    }

    void SnapToOriginalGroundHeight()
    {
        Vector3 pos = transform.position;
        if (!Mathf.Approximately(pos.y, originalGroundY))
        {
            pos.y = originalGroundY;
            transform.position = pos;
        }
    }

    void UpdateCooldowns()
    {
        if (jumpCooldown > 0f)
        {
            jumpCooldown -= Time.deltaTime;
        }

        if (dashCooldown > 0f)
        {
            dashCooldown -= Time.deltaTime;
        }

        if (dashSpeedTimer > 0f)
        {
            dashSpeedTimer -= Time.deltaTime;
        }
    }

    // -- COLLISIONS -- //

    void OnTriggerEnter(Collider other)
    {
        if (!isAlive)
            return;

        if (other.CompareTag("Bush"))
        {
            SpawnBushExplodeParticles(other.transform.position);

            if (dashSpeedTimer > 0f)
            {
                Destroy(other.gameObject);
                gameController.AddScorePoints(1);

                if (AudioController.Instance != null)
                {
                    AudioController.Instance.PlayBushBreak();
                }
            }
            else
            {
                TriggerCrashDeath("Pig hit a bush without dashing!");
            }

            return;
        }

        if (other.CompareTag("Water"))
        {
            isAlive = false;

            if (waterSplashParticlesPrefab != null)
            {
                Instantiate(waterSplashParticlesPrefab, transform.position, Quaternion.identity);
            }

            float shrinkDuration = gameController != null ? Mathf.Max(0f, gameController.gameOverScreenDelay) : 1f;
            StartCoroutine(ShrinkOnWaterDeath(shrinkDuration));

            if (gameController != null)
            {
                gameController.GameOver();
            }
            Debug.Log("Pig fell into water!");

            if (AudioController.Instance != null)
            {
                AudioController.Instance.PlayWaterDeath();
            }

            return;
        }

        if (other.CompareTag("Obstacle"))
        {
            TriggerCrashDeath("Pig hit an obstacle!");
        }

        if (other.CompareTag("Apple"))
        {
            Destroy(other.gameObject);
            gameController.AddTime(10f);
            gameController.AddScorePoints(1);

            if (AudioController.Instance != null)
            {
                AudioController.Instance.PlayApplePickup();
            }
        }
    }

    void TriggerCrashDeath(string debugMessage)
    {
        if (!isAlive)
            return;

        isAlive = false;

        if (gameController != null)
        {
            gameController.GameOver();
        }

        Debug.Log(debugMessage);

        if (AudioController.Instance != null)
        {
            AudioController.Instance.PlayCrashDeath();
        }

        float crashDuration = gameController != null ? Mathf.Max(0f, gameController.gameOverScreenDelay) : 1f;
        StartCoroutine(PlayCrashDeathAnimation(crashDuration));
    }

    void SpawnBushExplodeParticles(Vector3 spawnPosition)
    {
        if (bushExplodeParticlesPrefab == null)
            return;

        GameObject particles = Instantiate(bushExplodeParticlesPrefab, spawnPosition, Quaternion.identity);
        Destroy(particles, 1f);
    }

    IEnumerator ShrinkOnWaterDeath(float duration)
    {
        Vector3 startScale = transform.localScale;
        Vector3 startPosition = transform.position;
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        List<Material> fadeMaterials = new List<Material>();
        List<int> fadeColorPropertyIds = new List<int>();
        List<Color> startColors = new List<Color>();

        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            foreach (Material material in materials)
            {
                if (material == null)
                    continue;

                int colorPropertyId;
                if (material.HasProperty(ColorPropertyId))
                {
                    colorPropertyId = ColorPropertyId;
                }
                else if (material.HasProperty(BaseColorPropertyId))
                {
                    colorPropertyId = BaseColorPropertyId;
                }
                else
                {
                    continue;
                }

                fadeMaterials.Add(material);
                fadeColorPropertyIds.Add(colorPropertyId);
                startColors.Add(material.GetColor(colorPropertyId));
            }
        }

        if (duration <= 0f)
        {
            transform.localScale = Vector3.zero;
            transform.position = startPosition + Vector3.down * waterDeathSinkDistance;

            for (int i = 0; i < fadeMaterials.Count; i++)
            {
                Color color = startColors[i];
                color.a = 0f;
                fadeMaterials[i].SetColor(fadeColorPropertyIds[i], color);
            }

            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            transform.position = Vector3.Lerp(startPosition, startPosition + Vector3.down * waterDeathSinkDistance, t);

            for (int i = 0; i < fadeMaterials.Count; i++)
            {
                Color color = startColors[i];
                color.a = Mathf.Lerp(startColors[i].a, 0f, t);
                fadeMaterials[i].SetColor(fadeColorPropertyIds[i], color);
            }

            yield return null;
        }

        transform.localScale = Vector3.zero;
    transform.position = startPosition + Vector3.down * waterDeathSinkDistance;

        for (int i = 0; i < fadeMaterials.Count; i++)
        {
            Color color = startColors[i];
            color.a = 0f;
            fadeMaterials[i].SetColor(fadeColorPropertyIds[i], color);
        }
    }

    IEnumerator PlayCrashDeathAnimation(float duration)
    {
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        Vector3 backwardDirection = -transform.forward;
        Vector3 sidewaysDirection = transform.right;
        float peakHeight = Mathf.Max(0f, crashUpwardDistance);
        float zTiltDirection = Random.value < 0.5f ? -1f : 1f;
        float zTiltMagnitude = Random.Range(0f, Mathf.Max(0f, crashRandomZTiltMax));
        float targetZTilt = zTiltDirection * zTiltMagnitude;

        if (duration <= 0f)
        {
            transform.position = startPosition
                + backwardDirection * crashBackwardDistance
                + sidewaysDirection * (crashSidewaysDistance * zTiltDirection)
                + Vector3.up * crashEndYOffset;
            transform.rotation = startRotation * Quaternion.Euler(-360f * crashSpinRevolutions, 0f, targetZTilt);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Parabolic hop that peaks mid-flight, plus an end offset for the final downward drop.
            float yOffset = (4f * peakHeight * t * (1f - t)) + (crashEndYOffset * t);

            transform.position = startPosition
                + backwardDirection * (crashBackwardDistance * t)
                + sidewaysDirection * (crashSidewaysDistance * zTiltDirection * t)
                + Vector3.up * yOffset;

            float spinAngle = -360f * crashSpinRevolutions * t;
            float zTiltAngle = targetZTilt * t;
            transform.rotation = startRotation * Quaternion.Euler(spinAngle, 0f, zTiltAngle);

            yield return null;
        }

        transform.position = startPosition
            + backwardDirection * crashBackwardDistance
            + sidewaysDirection * (crashSidewaysDistance * zTiltDirection)
            + Vector3.up * crashEndYOffset;
        transform.rotation = startRotation * Quaternion.Euler(-360f * crashSpinRevolutions, 0f, targetZTilt);
    }
}
