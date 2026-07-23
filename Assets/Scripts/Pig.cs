using UnityEngine;

public class Pig : MonoBehaviour
{
    // -- SYSTEM -- //

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        CheckGround();
        UpdateForwardMovement();
        UpdateSideMovement();
        UpdateJump();
        UpdateDash();
        ApplyGravity();
        ClampVelocity();
        
        // Decrease cooldowns
        if (jumpCooldown > 0)
        {
            jumpCooldown -= Time.deltaTime;
        }
        if (dashCooldown > 0)
        {
            dashCooldown -= Time.deltaTime;
        }
    }

    // -- MOVEMENT -- //

    [Header("MOVEMENT")]
    public float forwardSpeed = 5f;
    public float sideSpeed = 5f;
    public float jumpForce = 5f;
    public float groundCheckDistance = 0.1f;
    public float jumpCooldownDuration = 0.2f;
    public float downwardForce = 5f;
    public float dashForce = 10f;
    public float dashCooldownDuration = 0.5f;
    public float maxSpeed = 20f;

    Rigidbody rb;
    public bool isGrounded;
    float jumpCooldown;
    float dashCooldown;

    void UpdateForwardMovement()
    {
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);
    }

    void UpdateSideMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        transform.Translate(Vector3.right * horizontalInput * sideSpeed * Time.deltaTime);
    }

    void UpdateJump()
    {
        if (Input.GetKey(KeyCode.Z) && isGrounded && jumpCooldown <= 0)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpCooldown = jumpCooldownDuration;
        }
    }

    void UpdateDash()
    {
        if (Input.GetKeyDown(KeyCode.X) && dashCooldown <= 0)
        {
            rb.AddForce(Vector3.forward * dashForce, ForceMode.Impulse);
            dashCooldown = dashCooldownDuration;
        }
    }

    void CheckGround()
    {
        Vector3 checkPosition = transform.position + Vector3.down * (groundCheckDistance / 2f);
        Collider[] colliders = Physics.OverlapSphere(checkPosition, groundCheckDistance);
        isGrounded = colliders.Length > 1; // More than 1 because the pig itself is included
    }

    void ApplyGravity()
    {
        if (!isGrounded)
        {
            rb.AddForce(Vector3.down * downwardForce, ForceMode.Acceleration);
        }
    }

    void ClampVelocity()
    {
        Vector3 velocity = rb.linearVelocity;
        if (velocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = velocity.normalized * maxSpeed;
        }
    }
}
