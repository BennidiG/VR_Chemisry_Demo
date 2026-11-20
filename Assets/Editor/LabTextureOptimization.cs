using UnityEditor;
using UnityEngine;

public static class LabTextureOptimization
{
    [MenuItem("VR Chemistry/Optimization/Apply Android Texture Budgets")]
    public static void ApplyAndroidBudgets()
    {
        int changed = 0;
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Texture", "Assets/Stick_M", "Assets/Resources/tiankonghe" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                continue;
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null || Mathf.Max(texture.width, texture.height) <= 2048)
                continue;

            TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings("Android");
            android.overridden = true;
            android.maxTextureSize = 2048;
            android.format = TextureImporterFormat.ETC2_RGBA8;
            android.compressionQuality = 50;
            importer.SetPlatformTextureSettings(android);
            EditorUtility.SetDirty(importer);
            changed++;
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Android texture budgets applied to " + changed + " large textures (2048 max, ETC2_RGBA8).");
    }
}
