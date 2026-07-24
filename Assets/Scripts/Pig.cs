using UnityEngine;

public class Pig : MonoBehaviour
{
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

    public float fallGravity = 20f;
    public float maxFallSpeed = 25f;
    public float fallAccelerationMultiplier = 2f;
    public bool isGrounded;

    float jumpCooldown;
    float dashCooldown;
    float dashSpeedTimer;
    float fallSpeed;
    float currentForwardSpeed;
    float currentSideSpeed;

    bool isJumping;
    bool hasJumpedSinceLastGrounded;
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
        if (other.CompareTag("Obstacle"))
        {
            isAlive = false;
            gameController.GameOver();
            Debug.Log("Pig hit an obstacle!");
        }

        if (other.CompareTag("Apple"))
        {
            Destroy(other.gameObject);
            gameController.AddTime(10f);
        }
    }
}
