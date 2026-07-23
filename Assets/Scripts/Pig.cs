using UnityEngine;

public class Pig : MonoBehaviour
{
    // -- SYSTEM -- //

    void Start()
    {

    }

    void Update()
    {
        CheckGround();
        UpdateForwardMovement();
        UpdateSideMovement();
        UpdateModelRotation();
        UpdateJump();
        UpdateDash();
        UpdateJumpHeight();
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
    public float dashForce = 3f;
    public float dashDuration = 0.1f;
    public float dashCooldownDuration = 0.5f;
    public float tiltAngle = 15f;
    public float tiltSpeed = 5f;
    public Transform pigModelTransform;

    public bool isGrounded;
    float jumpCooldown;
    float dashCooldown;
    float dashSpeedTimer;
    
    bool isJumping;
    float jumpTimer;
    Vector3 jumpStartPosition;

    void UpdateForwardMovement()
    {
        float currentSpeed = forwardSpeed;
        if (dashSpeedTimer > 0)
        {
            currentSpeed += dashForce;
        }
        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
    }

    void UpdateSideMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        transform.Translate(Vector3.right * horizontalInput * sideSpeed * Time.deltaTime);
    }

    void UpdateModelRotation()
    {
        if (pigModelTransform == null)
            return;

        float horizontalInput = Input.GetAxis("Horizontal");
        float targetTilt = -horizontalInput * tiltAngle;
        
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetTilt);
        pigModelTransform.localRotation = Quaternion.Lerp(pigModelTransform.localRotation, targetRotation, tiltSpeed * Time.deltaTime);
    }

    void UpdateJump()
    {
        CheckGround();
        
        if (Input.GetKey(KeyCode.Z) && isGrounded && jumpCooldown <= 0 && !isJumping)
        {
            isJumping = true;
            jumpTimer = 0f;
            jumpStartPosition = transform.position;
            jumpCooldown = jumpCooldownDuration;
        }
    }

    void UpdateJumpHeight()
    {
        if (isJumping)
        {
            jumpTimer += Time.deltaTime;
            
            if (jumpTimer >= jumpDuration)
            {
                // Jump finished
                isJumping = false;
                jumpTimer = 0f;
                Vector3 pos = transform.position;
                pos.y = jumpStartPosition.y;
                transform.position = pos;
            }
            else
            {
                // Jump in progress - use sine curve for smooth arc
                float jumpProgress = jumpTimer / jumpDuration;
                float height = Mathf.Sin(jumpProgress * Mathf.PI) * jumpHeight;
                
                Vector3 pos = transform.position;
                pos.y = jumpStartPosition.y + height;
                transform.position = pos;
            }
        }
    }

    void UpdateDash()
    {
        if (Input.GetKeyDown(KeyCode.X) && dashCooldown <= 0)
        {
            dashSpeedTimer = dashDuration;
            dashCooldown = dashCooldownDuration;
        }
    }

    void CheckGround()
    {
        Vector3 checkPosition = transform.position + Vector3.down * (groundCheckDistance / 2f);
        Collider[] colliders = Physics.OverlapSphere(checkPosition, groundCheckDistance);
        isGrounded = colliders.Length > 1; // More than 1 because the pig itself is included
    }

    void UpdateCooldowns()
    {
        if (jumpCooldown > 0)
        {
            jumpCooldown -= Time.deltaTime;
        }
        if (dashCooldown > 0)
        {
            dashCooldown -= Time.deltaTime;
        }
        if (dashSpeedTimer > 0)
        {
            dashSpeedTimer -= Time.deltaTime;
        }
    }
}
