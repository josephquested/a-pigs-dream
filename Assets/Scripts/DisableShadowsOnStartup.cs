using UnityEngine;

public class DisableShadowsOnStartup : MonoBehaviour
{
    void Awake()
    {
        QualitySettings.shadows = ShadowQuality.Disable;
        QualitySettings.shadowDistance = 0f;
        QualitySettings.pixelLightCount = 0;

        Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light light in lights)
        {
            if (light == null)
                continue;

            light.enabled = false;
        }
    }
}
