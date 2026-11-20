using System;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable]
public class LabPerformanceReport
{
    public string timestampUtc;
    public string unityVersion;
    public string platform;
    public string deviceModel;
    public string graphicsDevice;
    public string qualityLevel;
    public float renderScale;
    public int frames;
    public float elapsedSeconds;
    public float averageFrameMs;
    public float minimumFrameMs;
    public float maximumFrameMs;
    public float estimatedFps;
    public long gcStartBytes;
    public long gcEndBytes;
    public long gcDeltaBytes;
    public long totalAllocatedBytes;
    public long totalReservedBytes;
    public long monoUsedBytes;
    public long monoHeapBytes;
    public int editorDrawCalls;
    public int editorTriangles;
    public int editorBatches;
    public bool editorStatsAvailable;
}

/// <summary>
/// Opt-in performance sampling. It reports frame-time and memory evidence, not
/// a Quest GPU guarantee. JSON is written only after an explicit sample.
/// </summary>
public class LabPerformanceMonitor : MonoBehaviour
{
    public bool autoStart;
    [Min(0.5F)] public float sampleDuration = 10F;
    public string reportFileName = "lab_performance.json";

    private bool sampling;
    private float elapsed;
    private int frames;
    private float minimumFrameMs;
    private float maximumFrameMs;
    private long gcStartBytes;

    public string ReportPath => Path.Combine(Application.persistentDataPath, reportFileName);

    private void Start()
    {
        if (autoStart)
            BeginSample();
    }

    public void BeginSample()
    {
        elapsed = 0F;
        frames = 0;
        minimumFrameMs = float.MaxValue;
        maximumFrameMs = 0F;
        gcStartBytes = GC.GetTotalMemory(false);
        sampling = true;
        Debug.Log("Lab performance sampling started for " + sampleDuration + " seconds.");
    }

    private void Update()
    {
        if (!sampling)
            return;

        float frameMs = Time.unscaledDeltaTime * 1000F;
        elapsed += Time.unscaledDeltaTime;
        frames++;
        minimumFrameMs = Mathf.Min(minimumFrameMs, frameMs);
        maximumFrameMs = Mathf.Max(maximumFrameMs, frameMs);

        if (elapsed >= sampleDuration)
        {
            sampling = false;
            WriteReport();
        }
    }

    public void WriteReport()
    {
        if (frames <= 0 || elapsed <= 0F)
            return;

        UniversalRenderPipelineAsset pipeline = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
        if (pipeline == null)
            pipeline = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
        LabPerformanceReport report = new LabPerformanceReport
        {
            timestampUtc = DateTime.UtcNow.ToString("O"),
            unityVersion = Application.unityVersion,
            platform = Application.platform.ToString(),
            deviceModel = SystemInfo.deviceModel,
            graphicsDevice = SystemInfo.graphicsDeviceName,
            qualityLevel = QualitySettings.names[QualitySettings.GetQualityLevel()],
            renderScale = pipeline != null ? pipeline.renderScale : 0F,
            frames = frames,
            elapsedSeconds = elapsed,
            averageFrameMs = elapsed * 1000F / frames,
            minimumFrameMs = minimumFrameMs,
            maximumFrameMs = maximumFrameMs,
            estimatedFps = frames / elapsed,
            gcStartBytes = gcStartBytes,
            gcEndBytes = GC.GetTotalMemory(false),
            gcDeltaBytes = GC.GetTotalMemory(false) - gcStartBytes,
            totalAllocatedBytes = Profiler.GetTotalAllocatedMemoryLong(),
            totalReservedBytes = Profiler.GetTotalReservedMemoryLong(),
            monoUsedBytes = Profiler.GetMonoUsedSizeLong(),
            monoHeapBytes = Profiler.GetMonoHeapSizeLong()
        };
#if UNITY_EDITOR
        report.editorStatsAvailable = true;
        report.editorDrawCalls = UnityEditor.UnityStats.drawCalls;
        report.editorTriangles = UnityEditor.UnityStats.triangles;
        report.editorBatches = UnityEditor.UnityStats.batches;
#endif

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
            File.WriteAllText(ReportPath, JsonUtility.ToJson(report, true));
            Debug.Log("Lab performance report written: " + ReportPath);
        }
        catch (Exception exception)
        {
            Debug.LogError("Could not write lab performance report: " + exception.Message);
        }
    }
}
