using UnityEngine;

public class Pig : MonoBehaviour
{
    // -- SYSTEM -- //

    void Start()
    {
        
    }

    void Update()
    {
        UpdateForwardMovement();
    }

    // -- MOVEMENT -- //

    [Header("MOVEMENT")]
    public float forwardSpeed = 5f;

    void UpdateForwardMovement()
    {
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);
    }
}
