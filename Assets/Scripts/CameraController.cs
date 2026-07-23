using UnityEngine;

public class CameraController : MonoBehaviour
{
    // -- SYSTEM -- //

    GameObject pig;

    void Awake()
    {
        pig = GameObject.FindGameObjectWithTag("Pig");
        offset = transform.position - pig.transform.position;
    }

    void Update()
    {
        UpdateCameraPosition();
    }

    // -- CAMERA -- //

    public Vector3 offset = Vector3.zero;

    void UpdateCameraPosition()
    {
        Vector3 targetPos = new Vector3(
            pig.transform.position.x + offset.x,
            transform.position.y,
            pig.transform.position.z + offset.z
        );
        transform.position = targetPos;
    }
}
