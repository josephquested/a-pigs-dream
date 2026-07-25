using UnityEngine;

public class ScrollMaterialX : MonoBehaviour
{
    public float scrollSpeed = 0.25f;

    Renderer cachedRenderer;
    Material cachedMaterial;

    void Awake()
    {
        cachedRenderer = GetComponent<Renderer>();
        if (cachedRenderer != null)
        {
            cachedMaterial = cachedRenderer.material;
        }
    }

    void Update()
    {
        if (cachedMaterial == null)
            return;

        Vector2 offset = cachedMaterial.mainTextureOffset;
        offset.x = Mathf.Repeat(-Time.time * scrollSpeed, 1f);
        cachedMaterial.mainTextureOffset = offset;
    }
}
