using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class LabProjectValidation
{
    private static readonly List<string> Errors = new List<string>();
    private static readonly List<string> Warnings = new List<string>();

    [MenuItem("VR Chemistry/Validation/Validate Project")]
    public static void ValidateProject()
    {
        Validate(false, false);
    }

    public static bool ValidateForBuild(bool requireSigning)
    {
        return Validate(requireSigning, true);
    }

    private static bool Validate(bool requireSigning, bool requireFont)
    {
        Errors.Clear();
        Warnings.Clear();

        CheckScene();
        CheckRuntimeAssembly();
        CheckChemistryDefinitions();
        CheckDynamicCollidersAndLights();
        CheckShadersAndMaterials();
        CheckFont(requireFont);
        CheckLodAssets();
        CheckQuestAndAndroid(requireSigning);

        Directory.CreateDirectory("Logs");
        List<string> report = new List<string>
        {
            "VR Chemistry project validation",
            "Unity: " + Application.unityVersion,
            "Errors: " + Errors.Count,
            "Warnings: " + Warnings.Count,
            string.Empty
        };
        report.AddRange(Errors.Select(error => "ERROR: " + error));
        report.AddRange(Warnings.Select(warning => "WARNING: " + warning));
        File.WriteAllLines("Logs/LabProjectValidation.txt", report);

        foreach (string warning in Warnings)
            Debug.LogWarning(warning);
        foreach (string error in Errors)
            Debug.LogError(error);
        Debug.Log("Project validation finished. Errors=" + Errors.Count + ", warnings=" + Warnings.Count + ". Report: Logs/LabProjectValidation.txt");
        return Errors.Count == 0;
    }

    private static void CheckScene()
    {
        if (!EditorBuildSettings.scenes.Any(scene => scene.enabled && scene.path == "Assets/Scenes/SampleScene.unity"))
            Errors.Add("SampleScene is not enabled in Build Settings.");
        if (!File.Exists("Assets/Scenes/SampleScene.unity"))
            Errors.Add("SampleScene.unity is missing.");

        string[] requiredGuids =
        {
            "ba1d1234567890abcdef0000000007ab",
            "ba1d1234567890abcdef0000000008ab",
            "ba1d1234567890abcdef0000000009ab",
            "ba1d1234567890abcdef000000000dab",
            "ba1d1234567890abcdef000000000cab",
            "ba1d1234567890abcdef0000000011ab",
            "ba1d1234567890abcdef0000000012ab",
            "ba1d1234567890abcdef0000000013ab",
            "ba1d1234567890abcdef0000000014ab",
            "ba1d1234567890abcdef0000000015ab",
            "ba1d1234567890abcdef0000000016ab",
            "ba1d1234567890abcdef0000000017ab",
            "ba1d1234567890abcdef000000001aab",
            "ba1d1234567890abcdef0000000018ab",
            "ba1d1234567890abcdef0000000019ab",
            "ba1d1234567890abcdef000000001bab",
            "ba1d1234567890abcdef000000001cab",
            "ba1d1234567890abcdef000000001dab",
            "ba1d1234567890abcdef0000000025ab"
        };
        foreach (string guid in requiredGuids)
        {
            if (string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(guid)))
                Errors.Add("Required project script is missing for GUID " + guid);
        }
    }

    private static void CheckRuntimeAssembly()
    {
        string runtime = File.ReadAllText("Assets/Scripts/VRChemistry.Runtime.asmdef");
        foreach (string reference in new[] { "VRChemistry.Chemistry.Core", "VRChemistry.Experiments.Core", "Unity.XR.Interaction.Toolkit", "UnityEngine.UI", "Unity.InputSystem", "Unity.VisualEffectGraph", "Unity.TextMeshPro", "Unity.RenderPipelines.Universal.Runtime" })
        {
            if (!runtime.Contains("\"" + reference + "\""))
                Errors.Add("VRChemistry.Runtime is missing assembly reference " + reference);
        }
        string playMode = File.ReadAllText("Assets/Tests/PlayMode/VRChemistry.PlayModeTests.asmdef");
        if (!playMode.Contains("VRChemistry.Runtime"))
            Errors.Add("PlayMode tests do not reference VRChemistry.Runtime.");

        string score = File.ReadAllText("Assets/Scripts/InitialDefinition/Score.cs");
        if (score.Contains("runner.SetStepCompletedFromLegacy") || score.Contains("runner.SetCleanupCompletedFromLegacy"))
            Errors.Add("Score still writes Runner directly; use LegacyExperimentAdapter.");
    }

    private static void CheckChemistryDefinitions()
    {
        foreach (string path in new[] { "Assets/Resources/Chemistry/Reactions/make_oxygen.asset", "Assets/Resources/Chemistry/Reactions/match_refire.asset", "Assets/Resources/Chemistry/Reactions/neutralization.asset" })
        {
            ReactionDefinitionAsset asset = AssetDatabase.LoadAssetAtPath<ReactionDefinitionAsset>(path);
            if (asset == null)
            {
                Errors.Add("Required reaction asset is missing: " + path);
                continue;
            }
            foreach (string error in asset.ToDefinition().ValidateConfiguration())
                Errors.Add(path + ": " + error);
        }

        foreach (string guid in AssetDatabase.FindAssets("t:SubstanceDefinitionAsset"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SubstanceDefinitionAsset asset = AssetDatabase.LoadAssetAtPath<SubstanceDefinitionAsset>(path);
            if (asset == null || string.IsNullOrWhiteSpace(asset.id) || asset.molarMass <= 0F)
                Errors.Add("Invalid chemical substance asset: " + path);
        }
    }

    private static void CheckDynamicCollidersAndLights()
    {
        foreach (Rigidbody body in UnityEngine.Object.FindObjectsOfType<Rigidbody>(true))
        {
            if (body.isKinematic)
                continue;
            foreach (MeshCollider collider in body.GetComponentsInChildren<MeshCollider>(true))
            {
                if (!collider.convex)
                    Errors.Add("Dynamic non-convex MeshCollider: " + body.name);
            }
        }
        foreach (Light light in UnityEngine.Object.FindObjectsOfType<Light>(true))
        {
            if (light.type != LightType.Directional && light.shadows != LightShadows.None)
                Warnings.Add("Local light still casts shadows: " + light.name);
        }
    }

    private static void CheckShadersAndMaterials()
    {
        foreach (string path in Directory.GetFiles("Assets/Shaders/Lab", "*.shader"))
        {
            string source = File.ReadAllText(path);
            if (!source.Contains("CBUFFER_START(UnityPerMaterial)") || !source.Contains("CBUFFER_END"))
                Errors.Add("Shader is missing UnityPerMaterial: " + path);
            if (source.Contains("ShaderLibrary/Unlit.hlsl"))
                Errors.Add("Shader uses an incompatible URP include: " + path);
        }
        string equipment = File.ReadAllText("Assets/Shaders/Lab/LabEquipment.shader");
        if (equipment.Contains("GetMainLight(output.shadowCoord)"))
            Errors.Add("LabEquipment uses output.shadowCoord in fragment code.");
        if (!equipment.Contains("GetMainLight(input.shadowCoord)") || !equipment.Contains("Name \"ShadowCaster\""))
            Errors.Add("LabEquipment shadow path is incomplete.");

        foreach (string path in new[]
        {
            "Assets/Materials/Lab/LabGlass.mat",
            "Assets/Materials/Lab/LabGlass_Frosted.mat",
            "Assets/Materials/Lab/LabLiquid_Water.mat",
            "Assets/Materials/Lab/LabLiquid_Alcohol.mat",
            "Assets/Materials/Lab/LabEquipment_Metal.mat",
            "Assets/Materials/Lab/LabEquipment_Ceramic.mat",
            "Assets/Materials/Lab/LabBubble.mat",
            "Assets/Materials/Stick.mat"
        })
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null || material.shader == null)
                Errors.Add("Material or shader is missing: " + path);
        }
    }

    private static void CheckFont(bool required)
    {
        const string path = "Assets/Fonts/ChemistryChineseSDF.asset";
        if (!File.Exists("Assets/Fonts/NotoSansSC.ttf"))
            Errors.Add("NotoSansSC.ttf is missing.");
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        if (font == null)
        {
            string message = "Chinese TMP font is not generated. Run VR Chemistry/Repair Chinese Font.";
            if (required)
                Errors.Add(message);
            else
                Warnings.Add(message);
        }
        else if (AssetDatabase.AssetPathToGUID(path) != "d43e48d21e6f5b245bd50c421a0e3044")
        {
            Errors.Add("Chinese TMP font GUID does not match the scene contract.");
        }
        else if (font.characterTable == null || font.characterTable.Count == 0)
        {
            Errors.Add("Chinese TMP font contains no pre-generated glyphs.");
        }

        string scene = File.ReadAllText("Assets/Scenes/SampleScene.unity");
        if (required && scene.Contains("m_sharedMaterial: {fileID: -3525995633598858701"))
            Errors.Add("Old TMP sharedMaterial references remain. Rebind and save SampleScene after font repair.");
    }

    private static void CheckLodAssets()
    {
        foreach (string name in new[] { "a_LOD1.fbx", "a_LOD2.fbx", "a_LOD3.fbx", "a_PROXY.fbx", "basin_LOD1.fbx", "basin_LOD2.fbx", "basin_LOD3.fbx", "rack_LOD1.fbx", "rack_LOD2.fbx", "rack_LOD3.fbx", "rack_PROXY.fbx" })
        {
            string path = "Assets/Modle/Optimized/" + name;
            if (!File.Exists(path) || !File.Exists(path + ".meta"))
                Errors.Add("LOD asset or meta is missing: " + path);
            else if (!AssetDatabase.LoadAllAssetsAtPath(path).OfType<Mesh>().Any())
                Errors.Add("LOD asset contains no imported Mesh: " + path);
        }
    }

    private static void CheckQuestAndAndroid(bool requireSigning)
    {
        UniversalRenderPipelineAsset quest = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/UniversalRP-Quest.asset");
        if (quest == null)
            Errors.Add("UniversalRP-Quest.asset is missing.");
        else
        {
            if (quest.renderScale <= 0F || quest.renderScale > 1F)
                Warnings.Add("Quest renderScale should be in (0, 1]. Current: " + quest.renderScale);
            if (quest.additionalLightsRenderingMode != LightRenderingMode.PerVertex)
                Warnings.Add("Quest additional lights are not using the vertex path.");
        }

        if (PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARM64)
            Errors.Add("Android target architecture is not ARM64.");
        if (AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/UniversalRP-QuestLow.asset") == null)
            Warnings.Add("QuestLow profile is missing; F8 low-budget comparison will be unavailable.");
        if (PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) != ScriptingImplementation.IL2CPP)
            Errors.Add("Android scripting backend is not IL2CPP.");

        string packageId = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android);
        if (packageId.StartsWith("com.DefaultCompany", StringComparison.OrdinalIgnoreCase))
        {
            string message = "Android package identifier is still the default: " + packageId;
            if (requireSigning)
                Errors.Add(message);
            else
                Warnings.Add(message);
        }
        if (PlayerSettings.GetGraphicsAPIs(BuildTarget.Android).Length == 0)
            Errors.Add("No Android graphics API is configured.");
        if (PlayerSettings.GetGraphicsAPIs(BuildTarget.Android).All(api => api != GraphicsDeviceType.Vulkan && api != GraphicsDeviceType.OpenGLES3))
            Warnings.Add("Android graphics APIs contain neither Vulkan nor OpenGLES3.");

        if (requireSigning)
        {
            if (!PlayerSettings.Android.useCustomKeystore || string.IsNullOrEmpty(PlayerSettings.Android.keystoreName) || string.IsNullOrEmpty(PlayerSettings.Android.keyaliasName))
                Errors.Add("Signed build requires a configured Android keystore and key alias. Passwords are not stored by this tool.");
        }
    }
}
