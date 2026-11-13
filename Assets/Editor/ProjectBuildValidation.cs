using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class ProjectBuildValidation
{
    [MenuItem("VR Chemistry/Validation/Build Android APK")]
    public static void BuildAndroidApk()
    {
        string[] scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
        if (scenes.Length == 0)
        {
            Debug.LogError("No scenes are enabled in Build Settings.");
            return;
        }

        Directory.CreateDirectory("Build/Android");
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "Build/Android/VR_Chemistry.apk",
            target = BuildTarget.Android,
            options = BuildOptions.StrictMode
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result == BuildResult.Succeeded)
            Debug.Log("Android build succeeded: " + report.summary.outputPath);
        else
            Debug.LogError("Android build failed. Review the Editor log for C# and Shader errors.");
    }
}
