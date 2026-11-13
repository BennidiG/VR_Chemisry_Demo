using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(AudioSource))]
public class ExperimentFeedbackController : MonoBehaviour
{
    private ExperimentRunner runner;
    private AudioSource audioSource;
    private AudioClip stepClip;
    private AudioClip completedClip;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0F;
        audioSource.volume = 0.35F;
        stepClip = CreateTone("StepComplete", 520F, 0F, 0.1F);
        completedClip = CreateTone("ExperimentComplete", 660F, 880F, 0.22F);
    }

    private void Start()
    {
        runner = ExperimentRunner.GetOrCreate();
        runner.StepChanged += OnStepChanged;
        runner.PhaseChanged += OnPhaseChanged;
    }

    private void OnDestroy()
    {
        if (runner != null)
        {
            runner.StepChanged -= OnStepChanged;
            runner.PhaseChanged -= OnPhaseChanged;
        }
        if (stepClip != null)
            Destroy(stepClip);
        if (completedClip != null)
            Destroy(completedClip);
    }

    private void OnStepChanged(ExperimentStepDefinition step, bool completed)
    {
        if (!completed || runner.CurrentMode == ExperimentMode.Assessment)
            return;
        audioSource.PlayOneShot(stepClip);
        SendHapticImpulse(0.2F, 0.08F);
    }

    private void OnPhaseChanged(ExperimentPhase phase)
    {
        if (phase != ExperimentPhase.Completed)
            return;
        audioSource.PlayOneShot(completedClip);
        SendHapticImpulse(0.45F, 0.18F);
    }

    private static AudioClip CreateTone(string clipName, float primaryFrequency, float secondaryFrequency, float duration)
    {
        const int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];
        for (int index = 0; index < sampleCount; index++)
        {
            float time = index / (float)sampleRate;
            float envelope = 1F - index / (float)sampleCount;
            float primary = Mathf.Sin(2F * Mathf.PI * primaryFrequency * time);
            float secondary = secondaryFrequency > 0F ? Mathf.Sin(2F * Mathf.PI * secondaryFrequency * time) * 0.5F : 0F;
            samples[index] = (primary + secondary) * envelope * 0.3F;
        }
        AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static void SendHapticImpulse(float amplitude, float duration)
    {
        SendHapticImpulse(XRNode.LeftHand, amplitude, duration);
        SendHapticImpulse(XRNode.RightHand, amplitude, duration);
    }

    private static void SendHapticImpulse(XRNode node, float amplitude, float duration)
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(node, devices);
        foreach (InputDevice device in devices)
        {
            if (!device.isValid)
                continue;
            if (device.TryGetHapticCapabilities(out HapticCapabilities capabilities) && capabilities.supportsImpulse)
                device.SendHapticImpulse(0U, amplitude, duration);
        }
    }
}
