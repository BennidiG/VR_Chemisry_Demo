using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Generates a world-horizontal liquid cap for a LabLiquidVolume. The cap is
/// deliberately separate from the source renderer, so parent scale and liquid
/// clipping do not distort its size or create recursive bounds.
/// </summary>
public class LabLiquidCap : MonoBehaviour
{
    [Range(16, 96)] public int segments = 48;

    private LabLiquidVolume source;
    private GameObject capObject;
    private Mesh capMesh;
    private Material capMaterial;
    private Material sourceMaterial;
    private static readonly Dictionary<Material, Material> sharedCapMaterials = new Dictionary<Material, Material>();
    private static readonly Dictionary<Material, int> capMaterialUsers = new Dictionary<Material, int>();
    private Vector2 initialRadii;
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private Vector3 lastScale;
    private Vector3 lastLossyScale;
    private float lastFill = -1F;
    private Vector2 lastWobble;
    private bool lastVisible;
    private bool initialized;

    public static LabLiquidCap Create(LabLiquidVolume source)
    {
        if (source == null || !source.generateSurfaceCap)
            return null;
        LabLiquidCap existing = source.GetComponent<LabLiquidCap>();
        if (existing != null)
            return existing;
        return source.gameObject.AddComponent<LabLiquidCap>();
    }

    private void Start()
    {
        source = GetComponent<LabLiquidVolume>();
        if (source == null || !source.generateSurfaceCap || source.RuntimeMaterial == null)
            return;

        initialRadii = source.GetSurfaceRadii();
        capObject = new GameObject("LiquidCap_" + source.name);
        capObject.hideFlags = HideFlags.DontSave;
        capObject.layer = source.gameObject.layer;
        MeshFilter filter = capObject.AddComponent<MeshFilter>();
        int actualSegments = Application.platform == RuntimePlatform.Android ? Mathf.Min(segments, 24) : segments;
        capMesh = CreateCapMesh(actualSegments);
        filter.sharedMesh = capMesh;

        MeshRenderer renderer = capObject.AddComponent<MeshRenderer>();
        sourceMaterial = source.RuntimeMaterial;
        capMaterial = AcquireCapMaterial(sourceMaterial);
        renderer.sharedMaterial = capMaterial;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.lightProbeUsage = LightProbeUsage.Off;
        renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        UpdateCapTransform();
    }

    private void Update()
    {
        if (source == null || capObject == null)
            return;
        if (source.RuntimeRendererEnabled != lastVisible || NeedsGeometryUpdate())
            UpdateCapTransform();
    }

    private bool NeedsGeometryUpdate()
    {
        if (!initialized)
            return true;
        if (source.transform.position != lastPosition || source.transform.rotation != lastRotation || source.transform.localScale != lastScale || source.transform.lossyScale != lastLossyScale)
            return true;
        if (!Mathf.Approximately(source.RuntimeMaterial.GetFloat("_FillAmount"), lastFill))
            return true;
        return source.CurrentWobble != lastWobble;
    }

    private void UpdateCapTransform()
    {
        if (capObject == null || source == null)
            return;
        if (!source.RuntimeRendererEnabled)
        {
            capObject.SetActive(false);
            lastVisible = false;
            return;
        }
        capObject.SetActive(true);
        lastVisible = true;

        Vector3 center = source.GetSurfaceCenter();
        center.y -= 0.0002F;
        capObject.transform.position = center;
        capObject.transform.rotation = Quaternion.identity;
        if (capMaterial != null)
        {
            capMaterial.SetFloat("_WobbleX", source.CurrentWobble.x);
            capMaterial.SetFloat("_WobbleZ", source.CurrentWobble.y);
        }
        Vector2 radii = source.GetSurfaceRadii();
        capObject.transform.localScale = new Vector3(
            radii.x / Mathf.Max(0.001F, initialRadii.x),
            1F,
            radii.y / Mathf.Max(0.001F, initialRadii.y));
        lastPosition = source.transform.position;
        lastRotation = source.transform.rotation;
        lastScale = source.transform.localScale;
        lastLossyScale = source.transform.lossyScale;
        lastFill = source.RuntimeMaterial.GetFloat("_FillAmount");
        lastWobble = source.CurrentWobble;
        initialized = true;
    }

    private void OnDisable()
    {
        if (capObject != null)
            capObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (capObject != null)
            capObject.SetActive(source == null || source.RuntimeRendererEnabled);
    }

    private void OnDestroy()
    {
        if (capObject != null)
            Destroy(capObject);
        ReleaseCapMaterial(sourceMaterial);
        if (capMesh != null)
            Destroy(capMesh);
    }

    private static Material AcquireCapMaterial(Material sourceMaterial)
    {
        if (sharedCapMaterials.TryGetValue(sourceMaterial, out Material existing))
        {
            capMaterialUsers[sourceMaterial]++;
            return existing;
        }

        Material cap = new Material(sourceMaterial)
        {
            name = sourceMaterial.name + "_SharedCap",
            renderQueue = 3001
        };
        sharedCapMaterials.Add(sourceMaterial, cap);
        capMaterialUsers.Add(sourceMaterial, 1);
        return cap;
    }

    private static void ReleaseCapMaterial(Material sourceMaterial)
    {
        if (sourceMaterial == null || !capMaterialUsers.TryGetValue(sourceMaterial, out int users))
            return;
        if (users > 1)
        {
            capMaterialUsers[sourceMaterial] = users - 1;
            return;
        }

        capMaterialUsers.Remove(sourceMaterial);
        if (sharedCapMaterials.TryGetValue(sourceMaterial, out Material cap))
        {
            sharedCapMaterials.Remove(sourceMaterial);
            if (cap != null)
                Destroy(cap);
        }
    }

    private static Mesh CreateCapMesh(int segments)
    {
        int count = Mathf.Clamp(segments, 16, 96);
        Vector3[] vertices = new Vector3[count + 1];
        Vector3[] normals = new Vector3[count + 1];
        Vector2[] uv = new Vector2[count + 1];
        int[] triangles = new int[count * 3];
        vertices[0] = Vector3.zero;
        normals[0] = Vector3.up;
        uv[0] = new Vector2(0.5F, 0.5F);

        for (int index = 0; index < count; index++)
        {
            float angle = index * Mathf.PI * 2F / count;
            float x = Mathf.Cos(angle);
            float z = Mathf.Sin(angle);
            vertices[index + 1] = new Vector3(x, 0F, z);
            normals[index + 1] = Vector3.up;
            uv[index + 1] = new Vector2(x * 0.5F + 0.5F, z * 0.5F + 0.5F);

            int next = (index + 1) % count;
            int triangle = index * 3;
            triangles[triangle] = 0;
            triangles[triangle + 1] = next + 1;
            triangles[triangle + 2] = index + 1;
        }

        Mesh mesh = new Mesh { name = "LabLiquidCapMesh" };
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        return mesh;
    }
}
