using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// Applies conservative material limits only on Android. Editor/desktop keeps
/// the authored liquid and bubble appearance for comparison.
/// </summary>
public class LabQuestMaterialTuner : MonoBehaviour
{
    public bool applyOnAndroid = true;
    [Range(0F, 0.01F)] public float maxWaveAmplitude = 0.001F;
    [Range(0F, 2F)] public float maxBubbleEmission = 0.65F;
    [Range(0.1F, 1F)] public float bubblePlayRate = 0.75F;

    private void Start()
    {
        if (applyOnAndroid && Application.platform == RuntimePlatform.Android)
            StartCoroutine(ApplyAfterOneFrame());
    }

    private IEnumerator ApplyAfterOneFrame()
    {
        yield return null;

        foreach (LabLiquidVolume volume in FindObjectsOfType<LabLiquidVolume>())
        {
            Material material = volume.RuntimeMaterial;
            if (material != null && material.HasProperty("_WaveAmplitude"))
                material.SetFloat("_WaveAmplitude", Mathf.Min(material.GetFloat("_WaveAmplitude"), maxWaveAmplitude));
        }

        GameObject bubbleObject = SceneObjectFinder.Find("ParticleBubbles");
        if (bubbleObject == null)
            yield break;
        Renderer renderer = bubbleObject.GetComponent<Renderer>();
        if (renderer != null && renderer.material.HasProperty("_EmissionStrength"))
            renderer.material.SetFloat("_EmissionStrength", Mathf.Min(renderer.material.GetFloat("_EmissionStrength"), maxBubbleEmission));
        VisualEffect bubbleEffect = bubbleObject.GetComponent<VisualEffect>();
        if (bubbleEffect != null)
            bubbleEffect.playRate = Mathf.Min(bubbleEffect.playRate, bubblePlayRate);
    }
}
