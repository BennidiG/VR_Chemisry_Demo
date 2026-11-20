using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// Shared per-renderer/material-slot state for short-lived material effects.
/// Each effect writes only its own fields, and highlight ownership is tracked.
/// </summary>
public static class LabMaterialState
{
    private struct SlotKey : IEquatable<SlotKey>
    {
        public readonly Renderer Renderer;
        public readonly int MaterialIndex;

        public SlotKey(Renderer renderer, int materialIndex)
        {
            Renderer = renderer;
            MaterialIndex = materialIndex;
        }

        public bool Equals(SlotKey other)
        {
            return ReferenceEquals(Renderer, other.Renderer) && MaterialIndex == other.MaterialIndex;
        }

        public override bool Equals(object obj)
        {
            return obj is SlotKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return RuntimeHelpers.GetHashCode(Renderer) * 397 ^ MaterialIndex;
        }
    }

    private sealed class SlotState
    {
        public readonly MaterialPropertyBlock Block = new MaterialPropertyBlock();
        public readonly HashSet<LabItem> HighlightOwners = new HashSet<LabItem>();
        public Color BaseColor;
        public Color EmissionColor;
    }

    private static readonly Dictionary<SlotKey, SlotState> States = new Dictionary<SlotKey, SlotState>();

    private static SlotState GetState(Renderer renderer, int materialIndex)
    {
        if (renderer == null || materialIndex < 0)
            return null;
        Material[] materials = renderer.sharedMaterials;
        if (materialIndex >= materials.Length)
            return null;

        SlotKey key = new SlotKey(renderer, materialIndex);
        if (States.TryGetValue(key, out SlotState state))
            return state;

        state = new SlotState();
        Material material = materials[materialIndex];
        state.BaseColor = GetBaseColor(material);
        state.EmissionColor = material != null && material.HasProperty("_EmissionColor") ? material.GetColor("_EmissionColor") : Color.black;
        renderer.GetPropertyBlock(state.Block, materialIndex);
        States.Add(key, state);
        return state;
    }

    public static void ApplyHighlight(Renderer renderer, int materialIndex, LabItem owner, Color color, float pulse)
    {
        SlotState state = GetState(renderer, materialIndex);
        if (state == null)
            return;

        if (owner != null)
            state.HighlightOwners.Add(owner);
        renderer.GetPropertyBlock(state.Block, materialIndex);
        Color baseColor = Color.Lerp(state.BaseColor, color, 0.25F + pulse * 0.35F);
        baseColor.a = state.BaseColor.a;
        state.Block.SetColor("_BaseColor", baseColor);
        state.Block.SetColor("_Color", baseColor);
        state.Block.SetColor("_EmissionColor", color * (0.3F + pulse * 0.8F));
        renderer.SetPropertyBlock(state.Block, materialIndex);
    }

    public static void ClearHighlight(Renderer renderer, LabItem owner)
    {
        if (renderer == null)
            return;

        foreach (KeyValuePair<SlotKey, SlotState> pair in States)
        {
            if (!ReferenceEquals(pair.Key.Renderer, renderer))
                continue;
            SlotState state = pair.Value;
            if (owner != null)
                state.HighlightOwners.Remove(owner);
            if (state.HighlightOwners.Count != 0)
                continue;

            renderer.GetPropertyBlock(state.Block, pair.Key.MaterialIndex);
            state.Block.SetColor("_BaseColor", state.BaseColor);
            state.Block.SetColor("_Color", state.BaseColor);
            state.Block.SetColor("_EmissionColor", state.EmissionColor);
            renderer.SetPropertyBlock(state.Block, pair.Key.MaterialIndex);
        }
    }

    public static void SetWobble(Renderer renderer, float wobbleX, float wobbleZ)
    {
        SlotState state = GetState(renderer, 0);
        if (state == null)
            return;
        renderer.GetPropertyBlock(state.Block, 0);
        state.Block.SetFloat("_WobbleX", wobbleX);
        state.Block.SetFloat("_WobbleZ", wobbleZ);
        renderer.SetPropertyBlock(state.Block, 0);
    }

    public static void SetDissolve(Renderer renderer, float offset, float dissolve)
    {
        SlotState state = GetState(renderer, 0);
        if (state == null)
            return;
        renderer.GetPropertyBlock(state.Block, 0);
        state.Block.SetFloat("Offest", offset);
        state.Block.SetFloat("Disslpate", dissolve);
        renderer.SetPropertyBlock(state.Block, 0);
    }

    public static void ClearRenderer(Renderer renderer)
    {
        List<SlotKey> keys = new List<SlotKey>();
        foreach (SlotKey key in States.Keys)
        {
            if (ReferenceEquals(key.Renderer, renderer))
                keys.Add(key);
        }
        foreach (SlotKey key in keys)
            States.Remove(key);
    }

    private static Color GetBaseColor(Material material)
    {
        if (material == null)
            return Color.white;
        if (material.HasProperty("_BaseColor"))
            return material.GetColor("_BaseColor");
        if (material.HasProperty("_Color"))
            return material.GetColor("_Color");
        return Color.white;
    }
}
