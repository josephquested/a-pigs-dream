using UnityEngine;

public class BobAndSpin : MonoBehaviour
{
    public float rotationSpeed = 30f;
    public float bobSpeed = 1f;
    public float bobHeight = 0.25f;

    float baseY;

    void Start()
    {
        baseY = transform.position.y;
    }

    void Update()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);

        Vector3 position = transform.position;
        position.y = baseY + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = position;
    }
}
