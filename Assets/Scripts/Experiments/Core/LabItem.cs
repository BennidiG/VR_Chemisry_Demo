using System.Collections.Generic;
using UnityEngine;

public static class LabItemRegistry
{
    private static readonly List<LabItem> items = new List<LabItem>();

    public static LabItem[] ActiveItems => items.ToArray();

    public static void Register(LabItem item)
    {
        if (!items.Contains(item))
            items.Add(item);
    }

    public static void Unregister(LabItem item)
    {
        items.Remove(item);
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
/// Identifies an interactable laboratory item and provides per-material-slot
/// property-block highlights without creating material instances.
/// </summary>
public class LabItem : MonoBehaviour
{
    public LabItemType itemType;
    public bool trackHomePosition;
    [Min(0.01F)] public float homeRadius = 0.15F;

    private Vector3 homePosition;
    private Renderer[] renderers;
    private Material[][] sharedMaterials;
    private Color[][] baseColors;
    private MaterialPropertyBlock[][] originalBlocks;
    private MaterialPropertyBlock[][] workingBlocks;
    private bool highlighted;
    private Color highlightColor;

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
        baseColors = new Color[renderers.Length][];
        originalBlocks = new MaterialPropertyBlock[renderers.Length][];
        workingBlocks = new MaterialPropertyBlock[renderers.Length][];

        for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
        {
            Renderer renderer = renderers[rendererIndex];
            Material[] materials = renderer.sharedMaterials;
            sharedMaterials[rendererIndex] = materials;
            baseColors[rendererIndex] = new Color[materials.Length];
            originalBlocks[rendererIndex] = new MaterialPropertyBlock[materials.Length];
            workingBlocks[rendererIndex] = new MaterialPropertyBlock[materials.Length];

            for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
            {
                Material material = materials[materialIndex];
                baseColors[rendererIndex][materialIndex] = GetBaseColor(material);
                originalBlocks[rendererIndex][materialIndex] = new MaterialPropertyBlock();
                workingBlocks[rendererIndex][materialIndex] = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(originalBlocks[rendererIndex][materialIndex], materialIndex);
            }
        }
    }

    private void Update()
    {
        if (!highlighted || renderers == null)
            return;

        float pulse = 0.5F + 0.5F * Mathf.Sin(Time.unscaledTime * 4F);
        for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
        {
            for (int materialIndex = 0; materialIndex < sharedMaterials[rendererIndex].Length; materialIndex++)
            {
                MaterialPropertyBlock block = workingBlocks[rendererIndex][materialIndex];
                renderers[rendererIndex].GetPropertyBlock(block, materialIndex);
                Color baseColor = Color.Lerp(baseColors[rendererIndex][materialIndex], highlightColor, 0.25F + pulse * 0.35F);
                baseColor.a = baseColors[rendererIndex][materialIndex].a;
                block.SetColor("_BaseColor", baseColor);
                block.SetColor("_Color", baseColor);
                block.SetColor("_EmissionColor", highlightColor * (0.3F + pulse * 0.8F));
                renderers[rendererIndex].SetPropertyBlock(block, materialIndex);
            }
        }
    }

    private void RestoreOriginalAppearance()
    {
        if (renderers == null)
            return;
        for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
        {
            for (int materialIndex = 0; materialIndex < sharedMaterials[rendererIndex].Length; materialIndex++)
                renderers[rendererIndex].SetPropertyBlock(originalBlocks[rendererIndex][materialIndex], materialIndex);
        }
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
        if (objectName.StartsWith("Powder") || objectName.StartsWith("Disslpate") || objectName.Contains("Socket"))
            return false;
        if (objectName == "Drug" || objectName == "Water" || objectName == "Mars" || objectName == "OnFire" || objectName == "LampCapT")
            return false;
        if (objectName.EndsWith("Coll") || objectName.EndsWith("Trigger") || objectName.EndsWith("Tirgger"))
            return false;
        return true;
    }

    private static Color GetBaseColor(Material material)
    {
        if (material.HasProperty("_BaseColor"))
            return material.GetColor("_BaseColor");
        if (material.HasProperty("_Color"))
            return material.GetColor("_Color");
        return Color.white;
    }
}
