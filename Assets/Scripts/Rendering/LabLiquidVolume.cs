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
    public Vector2 heightOverride = new Vector2(-0.05F, 0.05F);

    private Renderer targetRenderer;
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private Vector3 lastScale;
    private Vector3 lastLossyScale;

    private void Start()
    {
        targetRenderer = GetComponent<Renderer>();
        CaptureTransform();
        ApplyHeightRange();
    }

    private void Update()
    {
        if (transform.position != lastPosition || transform.rotation != lastRotation || transform.localScale != lastScale || transform.lossyScale != lastLossyScale)
        {
            CaptureTransform();
            ApplyHeightRange();
        }
    }

    private void CaptureTransform()
    {
        lastPosition = transform.position;
        lastRotation = transform.rotation;
        lastScale = transform.localScale;
        lastLossyScale = transform.lossyScale;
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
