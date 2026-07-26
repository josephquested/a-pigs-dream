using UnityEngine;

public class CameraController : MonoBehaviour
{
    // -- SYSTEM -- //

    GameObject pig;
    Vector3 startPosition;
    Quaternion startRotation;
    bool isFollowingPig = true;

    void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;

        pig = GameObject.FindGameObjectWithTag("Pig");
        if (pig != null)
        {
            offset = transform.position - pig.transform.position;
        }
    }

    void Update()
    {
        if (!isFollowingPig)
            return;

        UpdateCameraPosition();
    }

    // -- CAMERA -- //

    public Vector3 offset = Vector3.zero;
    public float smoothSpeed = 0.1f;

    public void ResetToStartPosition()
    {
        isFollowingPig = false;
        transform.position = startPosition;
        transform.rotation = startRotation;
    }

    public void ResumeFollowingPig()
    {
        isFollowingPig = true;
    }

    void UpdateCameraPosition()
    {
        if (pig == null)
            return;

        Vector3 targetPos = new Vector3(
            pig.transform.position.x + offset.x,
            transform.position.y,
            pig.transform.position.z + offset.z
        );
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed);
    }
}
