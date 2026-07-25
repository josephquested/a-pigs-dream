using UnityEngine;

public class TreeRandomizer : MonoBehaviour
{
    public GameObject tree1;
    public GameObject tree2;

    void Awake()
    {
        Vector3 position = transform.position;
        position.y = Random.Range(-1f, 0f);
        transform.position = position;

        Vector3 scale = transform.localScale;
        scale.x = Random.Range(1f, 1.5f);
        scale.z = Random.Range(1f, 1.5f);
        transform.localScale = scale;

        if (tree1 == null || tree2 == null)
            return;

        bool enableTree1 = Random.value < 0.5f;

        tree1.SetActive(enableTree1);
        tree2.SetActive(!enableTree1);
    }
}
