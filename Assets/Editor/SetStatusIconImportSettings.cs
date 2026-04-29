using UnityEditor;
using UnityEngine;

public class SetStatusIconImportSettings
{
    public static void Execute()
    {
        string folder = "Assets/Resources/Art/Generated/UI/StatusIcons";
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });

        int updated = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;

            importer.textureType         = TextureImporterType.Sprite;
            importer.spriteImportMode    = SpriteImportMode.Single;
            importer.filterMode          = FilterMode.Point;
            importer.spritePixelsPerUnit = 16;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled       = false;

            var settings = importer.GetDefaultPlatformTextureSettings();
            settings.format      = TextureImporterFormat.RGBA32;
            settings.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SetPlatformTextureSettings(settings);

            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
            updated++;
            Debug.Log($"[StatusIcons] Import settings applied: {path}");
        }

        AssetDatabase.Refresh();
        Debug.Log($"[StatusIcons] Done — {updated} sprites configured.");
    }
}
