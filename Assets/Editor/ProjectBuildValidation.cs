using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ProjectBuildValidation
{
    [MenuItem("VR Chemistry/Validation/Build Android APK")]
    public static void BuildAndroidApk()
    {
        BuildInternal(false);
    }

    [MenuItem("VR Chemistry/Validation/Build Signed Android APK")]
    public static void BuildSignedAndroidApk()
    {
        BuildInternal(true);
    }

    public static void ValidateForCI()
    {
        PrepareAssetsForBuild();
        if (!LabProjectValidation.ValidateForBuild(false))
            throw new InvalidOperationException("Project validation failed. See Logs/LabProjectValidation.txt.");
    }

    private static void BuildInternal(bool signed)
    {
        PrepareAssetsForBuild();
        if (!LabProjectValidation.ValidateForBuild(signed))
            throw new InvalidOperationException("Build blocked by project validation. See Logs/LabProjectValidation.txt.");

        string[] scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
        if (scenes.Length == 0)
            throw new InvalidOperationException("No scenes are enabled in Build Settings.");

        Directory.CreateDirectory("Build/Android");
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = signed ? "Build/Android/VR_Chemistry-signed.apk" : "Build/Android/VR_Chemistry.apk",
            target = BuildTarget.Android,
            options = BuildOptions.StrictMode
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result == BuildResult.Succeeded)
        {
            WriteBuildInfo(report, signed);
            Debug.Log("Android build succeeded: " + report.summary.outputPath + " (" + report.summary.totalSize + " bytes)");
        }
        else
            throw new InvalidOperationException("Android build failed. Review the Editor log for C# and Shader errors.");
    }

    private static void WriteBuildInfo(BuildReport report, bool signed)
    {
        LabBuildInfo info = new LabBuildInfo
        {
            timestampUtc = DateTime.UtcNow.ToString("O"),
            unityVersion = Application.unityVersion,
            applicationIdentifier = PlayerSettings.GetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android),
            bundleVersion = PlayerSettings.bundleVersion,
            signedBuild = signed,
            outputPath = report.summary.outputPath,
            sizeBytes = report.summary.totalSize,
            architectures = PlayerSettings.Android.targetArchitectures.ToString()
        };
        string directory = Path.GetDirectoryName(report.summary.outputPath);
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "build-info.json"), JsonUtility.ToJson(info, true));
    }

    [Serializable]
    private sealed class LabBuildInfo
    {
        public string timestampUtc;
        public string unityVersion;
        public string applicationIdentifier;
        public string bundleVersion;
        public bool signedBuild;
        public string outputPath;
        public ulong sizeBytes;
        public string architectures;
    }

    private static void PrepareAssetsForBuild()
    {
        if (SceneManager.GetActiveScene().name != "SampleScene")
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                throw new InvalidOperationException("Build cancelled because the current scene was not saved.");
            EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Single);
        }

        if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/ChemistryChineseSDF.asset") == null)
            ChineseFontRepair.Repair();
        ModelOptimizationTools.ApplyLodAssets();
        LabPhysicsOptimizationTools.ApplyPhysicsProxies();
        LabTextureOptimization.ApplyAndroidBudgets();
    }
}
