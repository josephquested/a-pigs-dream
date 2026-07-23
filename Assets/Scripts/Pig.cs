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
    }

    // -- MOVEMENT -- //

    [Header("MOVEMENT")]
    public float forwardSpeed = 5f;
    public float sideSpeed = 5f;
    public float jumpForce = 5f;
    public float groundCheckDistance = 0.1f;
    public float jumpCooldownDuration = 0.2f;

    Rigidbody rb;
    public bool isGrounded;
    float jumpCooldown;

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
        if (jumpCooldown > 0)
        {
            jumpCooldown -= Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.Z) && isGrounded && jumpCooldown <= 0)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpCooldown = jumpCooldownDuration;
        }
    }

    void CheckGround()
    {
        Vector3 checkPosition = transform.position + Vector3.down * (groundCheckDistance / 2f);
        Collider[] colliders = Physics.OverlapSphere(checkPosition, groundCheckDistance);
        isGrounded = colliders.Length > 1; // More than 1 because the pig itself is included
    }
}
