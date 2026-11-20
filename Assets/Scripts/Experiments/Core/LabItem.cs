using System.Collections.Generic;
using UnityEngine;

public static class LabItemRegistry
{
    private static readonly List<LabItem> items = new List<LabItem>();
    private static LabItem[] cachedItems = new LabItem[0];
    private static bool cacheDirty = true;

    public static LabItem[] ActiveItems
    {
        get
        {
            if (cacheDirty)
            {
                cachedItems = items.ToArray();
                cacheDirty = false;
            }
            return cachedItems;
        }
    }

    public static void Register(LabItem item)
    {
        if (!items.Contains(item))
        {
            items.Add(item);
            cacheDirty = true;
        }
    }

    public static void Unregister(LabItem item)
    {
        if (items.Remove(item))
            cacheDirty = true;
    }

    public static bool IsAtHome(LabItemType itemType)
    {
        foreach (LabItem item in items)
        {
            if (item.itemType == itemType)
                return item.IsAtHome;
        }
        return false;
    }

    public static void DebugReturnHome(LabItemType itemType)
    {
        foreach (LabItem item in items)
        {
            if (item.itemType == itemType)
            {
                item.DebugReturnHome();
                return;
            }
        }
    }
}

/// <summary>
/// Identifies an interactable laboratory item and delegates shared material
/// state to LabMaterialState, including per-slot highlight ownership.
/// </summary>
public class LabItem : MonoBehaviour
{
    public LabItemType itemType;
    public bool trackHomePosition;
    [Min(0.01F)] public float homeRadius = 0.15F;

    private Vector3 homePosition;
    private Renderer[] renderers;
    private Material[][] sharedMaterials;
    private bool highlighted;
    private Color highlightColor;
    private float nextHighlightUpdate;

    private void Awake()
    {
        homePosition = transform.position;
    }

    public bool IsAtHome => !trackHomePosition || Vector3.Distance(transform.position, homePosition) <= homeRadius;

    public void DebugReturnHome()
    {
        transform.position = homePosition;
        Rigidbody body = GetComponent<Rigidbody>();
        if (body != null)
        {
            body.position = homePosition;
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
    }

    private void OnEnable()
    {
        LabItemRegistry.Register(this);
    }

    private void OnDisable()
    {
        RestoreOriginalAppearance();
        highlighted = false;
        LabItemRegistry.Unregister(this);
    }

    private void OnDestroy()
    {
        if (renderers == null)
            return;
        foreach (Renderer renderer in renderers)
            LabMaterialState.ClearRenderer(renderer);
    }

    public void SetHighlight(Color? color)
    {
        if (!color.HasValue)
        {
            highlighted = false;
            RestoreOriginalAppearance();
            return;
        }

        EnsureRendererState();
        highlightColor = color.Value;
        highlighted = true;
    }

    private void EnsureRendererState()
    {
        if (renderers != null)
            return;

        List<Renderer> meshRenderers = new List<Renderer>();
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
        {
            if ((renderer is MeshRenderer || renderer is SkinnedMeshRenderer) && ShouldUseRenderer(renderer))
                meshRenderers.Add(renderer);
        }

        renderers = meshRenderers.ToArray();
        sharedMaterials = new Material[renderers.Length][];
        for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            sharedMaterials[rendererIndex] = renderers[rendererIndex].sharedMaterials;
    }

    private void Update()
    {
        if (!highlighted || renderers == null || Time.unscaledTime < nextHighlightUpdate)
            return;
        nextHighlightUpdate = Time.unscaledTime + 1F / 30F;

        float pulse = 0.5F + 0.5F * Mathf.Sin(Time.unscaledTime * 4F);
        for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
        {
            for (int materialIndex = 0; materialIndex < sharedMaterials[rendererIndex].Length; materialIndex++)
                LabMaterialState.ApplyHighlight(renderers[rendererIndex], materialIndex, this, highlightColor, pulse);
        }
    }

    private void RestoreOriginalAppearance()
    {
        if (renderers == null)
            return;
        foreach (Renderer renderer in renderers)
            LabMaterialState.ClearHighlight(renderer, this);
    }

    private bool ShouldUseRenderer(Renderer renderer)
    {
        if (renderer.gameObject.layer == 5 || renderer.GetComponentInParent<Canvas>() != null)
            return false;

        // The rack has its own highlight, but a separately registered test tube
        // must own its materials instead of being overwritten by the rack.
        if (itemType == LabItemType.TestTubeRack)
        {
            LabItem nestedItem = renderer.GetComponentInParent<LabItem>();
            if (nestedItem != null && nestedItem != this && nestedItem.itemType == LabItemType.TestTube)
                return false;
        }

        string objectName = renderer.gameObject.name;
        if (objectName.StartsWith("Powder") || objectName.StartsWith("Disslpate") || objectName.StartsWith("LiquidCap") || objectName.Contains("Socket"))
            return false;
        if (objectName == "Drug" || objectName == "Water" || objectName == "Mars" || objectName == "OnFire" || objectName == "LampCapT")
            return false;
        if (objectName.EndsWith("Coll") || objectName.EndsWith("Trigger") || objectName.EndsWith("Tirgger"))
            return false;
        return true;
    }
}
