using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Strips original and LOD1 mesh references only for the Android build, then
/// restores them after the build or after an exception. The Editor scene is
/// never saved in stripped form.
/// </summary>
public class LabQuestBuildPreprocessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    private sealed class MeshBackup
    {
        public MeshFilter Filter;
        public MeshCollider Collider;
        public Mesh Mesh;
    }

    private static readonly List<MeshBackup> MeshBackups = new List<MeshBackup>();
    private static readonly Dictionary<LODGroup, LOD[]> OriginalLods = new Dictionary<LODGroup, LOD[]>();

    public int callbackOrder => 100;

    public void OnPreprocessBuild(BuildReport report)
    {
        RestoreBuildState();
        try
        {
            if (report.summary.platform != BuildTarget.Android)
                return;

            foreach (LabQuestLodController controller in Object.FindObjectsOfType<LabQuestLodController>(true))
            {
                LODGroup group = controller.GetComponent<LODGroup>();
                if (group == null)
                    continue;
                LOD[] levels = group.GetLODS();
                if (levels.Length < 4)
                    continue;

                OriginalLods[group] = levels;
                controller.ApplyQuestLods();
                StripRendererReferences(levels[0].renderers);
                StripRendererReferences(levels[1].renderers);
            }
        }
        catch (Exception)
        {
            RestoreBuildState();
            throw;
        }
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        RestoreBuildState();
    }

    private static void RestoreBuildState()
    {
        foreach (KeyValuePair<LODGroup, LOD[]> pair in OriginalLods)
        {
            pair.Key.SetLODS(pair.Value);
            pair.Key.RecalculateBounds();
        }

        foreach (MeshBackup backup in MeshBackups)
        {
            if (backup.Filter != null)
                backup.Filter.sharedMesh = backup.Mesh;
            if (backup.Collider != null)
                backup.Collider.sharedMesh = backup.Mesh;
        }
        MeshBackups.Clear();
        OriginalLods.Clear();
    }

    private static void StripRendererReferences(Renderer[] renderers)
    {
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
                continue;

            MeshFilter filter = renderer.GetComponent<MeshFilter>();
            if (filter != null && filter.sharedMesh != null)
            {
                MeshBackups.Add(new MeshBackup { Filter = filter, Mesh = filter.sharedMesh });
                filter.sharedMesh = null;
            }

            foreach (MeshCollider collider in renderer.GetComponentsInChildren<MeshCollider>(true))
            {
                if (collider.sharedMesh != null && !collider.enabled)
                {
                    MeshBackups.Add(new MeshBackup { Collider = collider, Mesh = collider.sharedMesh });
                    collider.sharedMesh = null;
                }
            }
        }
    }
}
