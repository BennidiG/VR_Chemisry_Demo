using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class ModelOptimizationTools
{
    static ModelOptimizationTools()
    {
        EditorApplication.delayCall += AutoApplyLodAssets;
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        AutoApplyLodAssets();
    }

    private static void AutoApplyLodAssets()
    {
        if (EditorApplication.isPlaying)
            return;
        if (SessionState.GetBool("VRChemistry.LODAssetsApplied", false))
            return;
        if (SceneManager.GetActiveScene().name != "SampleScene")
            return;
        if (!File.Exists("Assets/Modle/Optimized/a_LOD1.fbx"))
            return;
        if (AssetDatabase.LoadAllAssetsAtPath("Assets/Modle/Optimized/a_LOD1.fbx").Length == 0)
            return;

        ApplyLodAssets();
        SessionState.SetBool("VRChemistry.LODAssetsApplied", true);
    }

    private sealed class ModelSpec
    {
        public string Source;
        public string Lod1;
        public string Lod2;
        public string Lod3;
        public string Proxy;
    }

    private static readonly ModelSpec[] Models =
    {
        new ModelSpec { Source = "Assets/Modle/a.fbx", Lod1 = "Assets/Modle/Optimized/a_LOD1.fbx", Lod2 = "Assets/Modle/Optimized/a_LOD2.fbx", Lod3 = "Assets/Modle/Optimized/a_LOD3.fbx", Proxy = "Assets/Modle/Optimized/a_PROXY.fbx" },
        new ModelSpec { Source = "Assets/Modle/18中式陶瓷洗手盆.fbx", Lod1 = "Assets/Modle/Optimized/basin_LOD1.fbx", Lod2 = "Assets/Modle/Optimized/basin_LOD2.fbx", Lod3 = "Assets/Modle/Optimized/basin_LOD3.fbx", Proxy = null },
        new ModelSpec { Source = "Assets/Modle/TestTubeRack.fbx", Lod1 = "Assets/Modle/Optimized/rack_LOD1.fbx", Lod2 = "Assets/Modle/Optimized/rack_LOD2.fbx", Lod3 = "Assets/Modle/Optimized/rack_LOD3.fbx", Proxy = "Assets/Modle/Optimized/rack_PROXY.fbx" }
    };

    [MenuItem("VR Chemistry/Optimization/Apply LOD Assets")]
    public static void ApplyLodAssets()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name != "SampleScene")
        {
            Debug.LogWarning("Open SampleScene before applying laboratory LOD assets.");
            return;
        }

        int lodCount = 0;
        int proxyCount = 0;
        int skipped = 0;
        foreach (ModelSpec spec in Models)
        {
            Mesh[] lod1Meshes = LoadMeshes(spec.Lod1);
            Mesh[] lod2Meshes = LoadMeshes(spec.Lod2);
            Mesh[] lod3Meshes = LoadMeshes(spec.Lod3);
            Mesh[] proxyMeshes = spec.Proxy == null ? Array.Empty<Mesh>() : LoadMeshes(spec.Proxy);
            if (lod1Meshes.Length == 0 || lod2Meshes.Length == 0 || lod3Meshes.Length == 0)
            {
                Debug.LogWarning("LOD assets are missing for " + spec.Source + ". Run Tools/optimize_models.py with Blender first.");
                skipped++;
                continue;
            }

            foreach (MeshFilter filter in UnityEngine.Object.FindObjectsOfType<MeshFilter>(true))
            {
                if (filter.gameObject.scene != scene || filter.sharedMesh == null)
                    continue;
                if (!string.Equals(AssetDatabase.GetAssetPath(filter.sharedMesh), spec.Source, StringComparison.OrdinalIgnoreCase))
                    continue;

                MeshRenderer sourceRenderer = filter.GetComponent<MeshRenderer>();
                if (sourceRenderer == null)
                    continue;

                Mesh lod1 = FindMatchingMesh(lod1Meshes, filter.sharedMesh.name);
                Mesh lod2 = FindMatchingMesh(lod2Meshes, filter.sharedMesh.name);
                Mesh lod3 = FindMatchingMesh(lod3Meshes, filter.sharedMesh.name);
                if (lod1 == null || lod2 == null || lod3 == null)
                {
                    Debug.LogWarning("Could not match LOD mesh " + filter.sharedMesh.name + " in " + spec.Source);
                    skipped++;
                    continue;
                }

                MeshCollider collider = filter.GetComponent<MeshCollider>();
                Rigidbody body = filter.GetComponentInParent<Rigidbody>();
                if (collider != null && collider.convex && body != null && !body.isKinematic && proxyMeshes.Length > 0)
                {
                    Mesh proxy = FindMatchingMesh(proxyMeshes, filter.sharedMesh.name);
                    if (proxy != null)
                    {
                        Undo.RecordObject(collider, "Assign laboratory collision proxy");
                        collider.sharedMesh = proxy;
                        proxyCount++;
                    }
                }

                LODGroup group = filter.GetComponent<LODGroup>();
                LOD[] oldLods = group != null ? group.GetLODS() : new LOD[0];
                if (oldLods.Length >= 4)
                {
                    if (group.GetComponent<LabQuestLodController>() == null)
                        Undo.AddComponent<LabQuestLodController>(filter.gameObject);
                    continue;
                }

                Renderer lod1Renderer = oldLods.Length >= 2 && oldLods[1].renderers.Length > 0 ? oldLods[1].renderers[0] : CreateLodRenderer(filter.transform, sourceRenderer, lod1, "_LOD1");
                Renderer lod2Renderer = oldLods.Length >= 3 && oldLods[2].renderers.Length > 0 ? oldLods[2].renderers[0] : CreateLodRenderer(filter.transform, sourceRenderer, lod2, "_LOD2");
                Renderer lod3Renderer = CreateLodRenderer(filter.transform, sourceRenderer, lod3, "_LOD3");
                if (group == null)
                    group = Undo.AddComponent<LODGroup>(filter.gameObject);
                Undo.RecordObject(group, "Update laboratory LOD chain");

                if (oldLods.Length >= 3)
                {
                    group.SetLODS(new[]
                    {
                        new LOD(0.65F, oldLods[0].renderers),
                        new LOD(0.25F, oldLods[1].renderers),
                        new LOD(0.12F, oldLods[2].renderers),
                        new LOD(0.01F, new[] { lod3Renderer })
                    });
                }
                else
                {
                    group.SetLODS(new[]
                    {
                        new LOD(0.65F, new[] { sourceRenderer }),
                        new LOD(0.25F, new[] { lod1Renderer }),
                        new LOD(0.12F, new[] { lod2Renderer }),
                        new LOD(0.01F, new[] { lod3Renderer })
                    });
                }
                group.RecalculateBounds();
                lodCount++;
                if (group.GetComponent<LabQuestLodController>() == null)
                    Undo.AddComponent<LabQuestLodController>(filter.gameObject);
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Applied " + lodCount + " LOD groups and " + proxyCount + " convex collision proxies; skipped " + skipped + ".");
    }

    private static Mesh[] LoadMeshes(string path)
    {
        if (!File.Exists(path))
            return Array.Empty<Mesh>();
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Mesh>().ToArray();
    }

    private static Mesh FindMatchingMesh(Mesh[] meshes, string sourceName)
    {
        Mesh exact = meshes.FirstOrDefault(mesh => mesh.name == sourceName);
        if (exact != null)
            return exact;
        string normalized = NormalizeName(sourceName);
        Mesh[] matches = meshes.Where(mesh => NormalizeName(mesh.name) == normalized).ToArray();
        if (matches.Length > 1)
        {
            Debug.LogWarning("Ambiguous LOD mesh name " + sourceName + "; refusing to guess.");
            return null;
        }
        return matches.FirstOrDefault();
    }

    private static string NormalizeName(string value)
    {
        return value.Replace(" ", string.Empty).Replace("_", string.Empty).Replace(".", string.Empty);
    }

    private static Renderer CreateLodRenderer(Transform parent, MeshRenderer source, Mesh mesh, string suffix)
    {
        GameObject child = new GameObject(source.gameObject.name + suffix);
        Undo.RegisterCreatedObjectUndo(child, "Create laboratory LOD renderer");
        child.transform.SetParent(parent, false);
        child.transform.localPosition = Vector3.zero;
        child.transform.localRotation = Quaternion.identity;
        child.transform.localScale = Vector3.one;

        MeshFilter filter = child.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;
        MeshRenderer renderer = child.AddComponent<MeshRenderer>();
        renderer.sharedMaterials = source.sharedMaterials;
        renderer.shadowCastingMode = source.shadowCastingMode;
        renderer.receiveShadows = source.receiveShadows;
        renderer.lightProbeUsage = source.lightProbeUsage;
        renderer.reflectionProbeUsage = source.reflectionProbeUsage;

        Wobble sourceWobble = source.GetComponent<Wobble>();
        if (sourceWobble != null)
        {
            Wobble wobble = child.AddComponent<Wobble>();
            wobble.MaxWobble = sourceWobble.MaxWobble;
            wobble.WobbleSpeed = sourceWobble.WobbleSpeed;
            wobble.Recovery = sourceWobble.Recovery;
        }

        LabLiquidVolume sourceVolume = source.GetComponent<LabLiquidVolume>();
        if (sourceVolume != null)
        {
            LabLiquidVolume volume = child.AddComponent<LabLiquidVolume>();
            volume.autoFitToRenderer = sourceVolume.autoFitToRenderer;
            volume.generateSurfaceCap = sourceVolume.generateSurfaceCap;
            volume.heightOverride = sourceVolume.heightOverride;
        }
        return renderer;
    }
}
