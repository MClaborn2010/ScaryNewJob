using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightIntensityPulse : MonoBehaviour
{
    [Tooltip("Minimum intensity value")]
    public float minIntensity = 0.01f;

    [Tooltip("Maximum intensity value")]
    public float maxIntensity = 0.04f;

    [Tooltip("Speed of the oscillation (higher = faster)")]
    public float rate = 1f;

    private Light lightComponent;
    private float originalIntensity;

    void Start()
    {
        lightComponent = GetComponent<Light>();
        if (lightComponent != null)
        {
            originalIntensity = lightComponent.intensity;
        }
        else
        {
            Debug.LogError("No Light component found on this GameObject!");
        }
    }

    void Update()
    {
        if (lightComponent == null) return;

        // Smooth back-and-forth oscillation (0 to 1) using sine for natural easing
        float t = (Mathf.Sin(Time.time * rate) + 1f) * 0.5f;

        // Interpolate between min and max
        float currentIntensity = Mathf.Lerp(minIntensity, maxIntensity, t);

        lightComponent.intensity = currentIntensity;
    }

    // Optional: Restore original intensity when disabled/stopped
    void OnDisable()
    {
        if (lightComponent != null)
        {
            lightComponent.intensity = originalIntensity;
        }
    }
}