using UnityEngine;

/// <summary>
/// Uses the lower-cost LOD chain on Android while keeping the original mesh as
/// LOD0 for desktop inspection. This component is inert in the Editor.
/// </summary>
public class LabQuestLodController : MonoBehaviour
{
    public bool applyOnAndroid = true;

    private void Start()
    {
        if (applyOnAndroid && Application.platform == RuntimePlatform.Android)
            ApplyQuestLods();
    }

    public void ApplyQuestLods()
    {
        LODGroup group = GetComponent<LODGroup>();
        if (group == null)
            return;

        LOD[] levels = group.GetLODS();
        if (levels.Length < 4)
            return;

        group.SetLODS(new[]
        {
            new LOD(0.65F, levels[2].renderers),
            new LOD(0.01F, levels[3].renderers)
        });
        group.RecalculateBounds();
    }
}
