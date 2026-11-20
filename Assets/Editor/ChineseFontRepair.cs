using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;
using UnityEngine.TextCore.LowLevel;

[InitializeOnLoad]
public static class ChineseFontRepair
{
    private const string SourceFontPath = "Assets/Fonts/NotoSansSC.ttf";
    private const string FontAssetPath = "Assets/Fonts/ChemistryChineseSDF.asset";
    private const string RequiredGuid = "d43e48d21e6f5b245bd50c421a0e3044";

    static ChineseFontRepair()
    {
        EditorApplication.delayCall += AutoRepairIfNeeded;
    }

    private static void AutoRepairIfNeeded()
    {
        if (EditorApplication.isPlaying)
            return;
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "SampleScene")
            return;
        if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath) != null)
            return;
        if (AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath) == null)
            return;

        try
        {
            Repair();
        }
        catch (Exception exception)
        {
            Debug.LogError("Automatic Chinese font repair failed: " + exception.Message);
        }
    }

    [MenuItem("VR Chemistry/Repair Chinese Font")]
    public static void Repair()
    {
        Font font = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);
        if (font == null)
        {
            EditorUtility.DisplayDialog("Chinese font missing", "NotoSansSC.ttf was not found in Assets/Fonts.", "OK");
            return;
        }

        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        if (fontAsset == null)
        {
            fontAsset = TMP_FontAsset.CreateFontAsset(font, 90, 9, GlyphRenderMode.SDFAA, 2048, 2048, AtlasPopulationMode.Dynamic, true);
            fontAsset.name = "ChemistryChineseSDF";
            AssetDatabase.CreateAsset(fontAsset, FontAssetPath);
        }
        else
        {
            fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            EditorUtility.SetDirty(fontAsset);
        }

        string meta = "fileFormatVersion: 2\n" +
            "guid: " + RequiredGuid + "\n" +
            "NativeFormatImporter:\n" +
            "  externalObjects: {}\n" +
            "  mainObjectFileID: 11400000\n" +
            "  userData: \n" +
            "  assetBundleName: \n" +
            "  assetBundleVariant: \n";
        File.WriteAllText(FontAssetPath + ".meta", meta);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (fontAsset.material != null && !AssetDatabase.Contains(fontAsset.material))
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        string glyphText = File.ReadAllText("Assets/Resources/Experiments/MakeO2.asset") + File.ReadAllText("Assets/Resources/Experiments/MatchReFire.asset") + "中文 / EN 制取氧气火柴复燃实验器材准备归位得分完成重置实验室添加销毁调整角度位置上移下移前移后退";
        if (!fontAsset.TryAddCharacters(glyphText))
            Debug.LogWarning("Some Chinese glyphs could not be added to the font atlas; dynamic fallback remains enabled.");
        if (fontAsset.atlasTextures != null)
        {
            foreach (Texture2D atlasTexture in fontAsset.atlasTextures)
            {
                if (atlasTexture != null && !AssetDatabase.Contains(atlasTexture))
                    AssetDatabase.AddObjectToAsset(atlasTexture, fontAsset);
            }
        }
        AssetDatabase.SaveAssets();

        TMP_Text[] texts = Object.FindObjectsOfType<TMP_Text>(true);
        foreach (TMP_Text text in texts)
        {
            text.font = fontAsset;
            if (fontAsset.material != null)
                text.fontSharedMaterial = fontAsset.material;
            EditorUtility.SetDirty(text);
        }
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("Chinese TMP font asset created, persisted and rebound in the active scene: " + FontAssetPath);
    }
}
