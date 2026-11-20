using UnityEngine;

/// <summary>
/// Fits LabLiquid's world-space height range to the current renderer bounds.
/// A cached material instance is used for persistent per-liquid height values,
/// so Wobble and short-lived highlights cannot overwrite them through a block.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class LabLiquidVolume : MonoBehaviour
{
    public bool autoFitToRenderer = true;
    public bool generateSurfaceCap;
    public Vector2 heightOverride = new Vector2(-0.05F, 0.05F);

    private Renderer targetRenderer;
    private Quaternion lastRotation;
    private Vector3 lastScale;
    private Vector3 lastLossyScale;
    private Wobble wobble;

    private void Start()
    {
        targetRenderer = GetComponent<Renderer>();
        wobble = GetComponent<Wobble>();
        CaptureTransform();
        ApplyHeightRange();
        if (generateSurfaceCap)
            LabLiquidCap.Create(this);
    }

    private void Update()
    {
        // Pure translation does not change the height range relative to the
        // container origin. Rotation and parent/local scale can change bounds.
        if (transform.rotation != lastRotation || transform.localScale != lastScale || transform.lossyScale != lastLossyScale)
        {
            CaptureTransform();
            ApplyHeightRange();
        }
    }

    private void CaptureTransform()
    {
        lastRotation = transform.rotation;
        lastScale = transform.localScale;
        lastLossyScale = transform.lossyScale;
    }

    private Renderer TargetRenderer
    {
        get
        {
            if (targetRenderer == null)
                targetRenderer = GetComponent<Renderer>();
            return targetRenderer;
        }
    }

    public Material RuntimeMaterial => TargetRenderer.material;
    public bool RuntimeRendererEnabled => TargetRenderer != null && TargetRenderer.enabled && TargetRenderer.gameObject.activeInHierarchy;
    public Vector2 CurrentWobble => wobble != null ? wobble.CurrentWobble : Vector2.zero;

    public float GetSurfaceHeight()
    {
        Material material = RuntimeMaterial;
        float minimum = material != null ? material.GetFloat("_MinHeight") : heightOverride.x;
        float maximum = material != null ? material.GetFloat("_MaxHeight") : heightOverride.y;
        float fill = material != null ? material.GetFloat("_FillAmount") : 0.65F;
        return transform.position.y + Mathf.Lerp(minimum, maximum, fill);
    }

    public Vector3 GetSurfaceCenter()
    {
        Vector3 center = TargetRenderer.bounds.center;
        center.y = GetSurfaceHeight();
        return center;
    }

    public Vector2 GetSurfaceRadii()
    {
        if (TargetRenderer == null)
            return Vector2.one * 0.01F;
        Bounds bounds = TargetRenderer.bounds;
        return new Vector2(Mathf.Max(0.001F, bounds.extents.x * 0.95F), Mathf.Max(0.001F, bounds.extents.z * 0.95F));
    }

    public void ApplyHeightRange()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();
        if (targetRenderer == null)
            return;

        Material material = targetRenderer.material;
        if (material == null || !material.HasProperty("_MinHeight") || !material.HasProperty("_MaxHeight"))
            return;

        if (autoFitToRenderer)
        {
            Bounds bounds = targetRenderer.bounds;
            float originY = transform.position.y;
            float minimum = bounds.min.y - originY;
            float maximum = bounds.max.y - originY;
            if (maximum - minimum < 0.0001F)
            {
                minimum = heightOverride.x;
                maximum = heightOverride.y;
            }
            material.SetFloat("_MinHeight", minimum);
            material.SetFloat("_MaxHeight", maximum);
        }
        else
        {
            material.SetFloat("_MinHeight", heightOverride.x);
            material.SetFloat("_MaxHeight", heightOverride.y);
        }
    }
}
