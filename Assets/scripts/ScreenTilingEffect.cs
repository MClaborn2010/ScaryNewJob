using UnityEngine;

public class ScreenTilingEffect : MonoBehaviour
{
    [Tooltip("The material to animate (drag your screen material here, or leave empty to auto-use the renderer's material)")]
    public Material targetMaterial;

    [Tooltip("Minimum X tiling value")]
    public float minTileX = 1f;

    [Tooltip("Maximum X tiling value")]
    public float maxTileX = 30f;

    [Tooltip("Speed of the oscillation (higher = faster)")]
    public float rate = 1f;

    private Material materialInstance;
    private Vector2 originalTiling;

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();

        if (renderer == null)
        {
            Debug.LogError("No Renderer found on this GameObject!");
            return;
        }

        // Use dragged-in material or the one on the renderer
        if (targetMaterial != null)
        {
            materialInstance = targetMaterial;
        }
        else
        {
            // Create a unique instance so we don't modify the original asset
            materialInstance = renderer.material;
        }

        // Store the original Y tiling so we don't change it
        originalTiling = materialInstance.mainTextureScale;
    }

    void Update()
    {
        if (materialInstance == null) return;

        // Smooth easing back and forth using sine
        float t = (Mathf.Sin(Time.time * rate) + 1f) * 0.5f; // 0 to 1 and back

        // Interpolate X tiling
        float currentTileX = Mathf.Lerp(minTileX, maxTileX, t);

        // Apply new tiling (keep original Y)
        materialInstance.mainTextureScale = new Vector2(currentTileX, originalTiling.y);
    }

    // Optional: Reset on disable/stop (good for editor)
    void OnDisable()
    {
        if (materialInstance != null)
        {
            materialInstance.mainTextureScale = originalTiling;
        }
    }
}