using UnityEngine;

public class RandomiseYRotation : MonoBehaviour
{
    void Awake()
    {
        float randomY = Random.Range(0, 360f);
        Vector3 currentEuler = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(currentEuler.x, randomY, currentEuler.z);
    }
}
