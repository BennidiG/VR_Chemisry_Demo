using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Replaces the expensive physics portion of test tubes and the oxygen bottle
/// with capsules. Original mesh colliders remain disabled for rollback, while
/// trigger-specific meshes and all other collider types are preserved.
/// </summary>
[InitializeOnLoad]
public static class LabPhysicsOptimizationTools
{
    static LabPhysicsOptimizationTools()
    {
        EditorApplication.delayCall += AutoApply;
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        AutoApply();
    }

    private static void AutoApply()
    {
        if (EditorApplication.isPlaying || SessionState.GetBool("VRChemistry.PhysicsProxiesApplied", false))
            return;
        if (SceneManager.GetActiveScene().name != "SampleScene")
            return;
        ApplyPhysicsProxies();
        SessionState.SetBool("VRChemistry.PhysicsProxiesApplied", true);
    }

    [MenuItem("VR Chemistry/Optimization/Apply Physics Proxies")]
    public static void ApplyPhysicsProxies()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name != "SampleScene")
        {
            Debug.LogWarning("Open SampleScene before applying physics proxies.");
            return;
        }

        int converted = 0;
        int disabled = 0;
        foreach (Rigidbody body in Object.FindObjectsOfType<Rigidbody>(true))
        {
            if (body.isKinematic || body.gameObject.scene != scene)
                continue;
            if (!body.name.StartsWith("TestTube") && body.name != "GlassBottle")
                continue;
            if (body.GetComponent<CapsuleCollider>() != null)
                continue;

            MeshCollider[] meshColliders = body.GetComponentsInChildren<MeshCollider>(true);
            MeshCollider source = meshColliders.FirstOrDefault(collider => collider.gameObject == body.gameObject && !collider.isTrigger);
            if (source == null)
                source = meshColliders.Where(collider => !collider.isTrigger).OrderByDescending(collider => collider.bounds.size.sqrMagnitude).FirstOrDefault();
            if (source == null)
                continue;

            Bounds sourceBounds = source.sharedMesh != null ? source.sharedMesh.bounds : source.bounds;
            Vector3 center = body.transform.InverseTransformPoint(source.transform.TransformPoint(sourceBounds.center));
            Vector3 extents = body.transform.InverseTransformVector(source.transform.TransformVector(sourceBounds.extents));
            float radius = Mathf.Max(Mathf.Abs(extents.x), Mathf.Abs(extents.z));
            float height = Mathf.Max(Mathf.Abs(extents.y) * 2F, radius * 2F);
            if (radius <= 0.00001F || height <= 0.00001F)
                continue;

            Undo.RegisterCompleteObjectUndo(body.gameObject, "Apply laboratory physics proxy");
            foreach (MeshCollider collider in meshColliders)
            {
                if (collider.isTrigger || collider.gameObject.name.Contains("Trigger") || collider.gameObject.name.Contains("Tirgger"))
                    continue;
                collider.enabled = false;
                disabled++;
            }

            CapsuleCollider capsule = Undo.AddComponent<CapsuleCollider>(body.gameObject);
            capsule.center = center;
            capsule.radius = radius;
            capsule.height = height;
            capsule.direction = 1;
            capsule.isTrigger = false;
            capsule.contactOffset = source.contactOffset;
            capsule.sharedMaterial = source.sharedMaterial;
            body.solverIterations = 4;
            body.solverVelocityIterations = 1;
            converted++;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Physics proxies applied to " + converted + " bodies; disabled " + disabled + " original mesh colliders. Trigger meshes and other collider types were preserved.");
    }
}
