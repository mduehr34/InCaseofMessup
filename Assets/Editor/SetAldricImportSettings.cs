using UnityEditor;
using UnityEngine;

public class SetAldricImportSettings
{
    public static void Execute()
    {
        string path = "Assets/_Game/Art/Generated/Characters/Aldric/aldric_sheet.aseprite";
        var importer = AssetImporter.GetAtPath(path);

        if (importer == null)
        {
            Debug.LogError("[Setup] Could not find importer at: " + path);
            return;
        }

        Debug.Log("[Setup] Importer type: " + importer.GetType().Name);

        // Use SerializedObject to set properties regardless of importer type
        var so = new SerializedObject(importer);

        var ppu = so.FindProperty("m_PixelsPerUnit");
        if (ppu != null) ppu.floatValue = 16f;

        var filter = so.FindProperty("m_TextureSettings.m_FilterMode");
        if (filter != null) filter.intValue = (int)FilterMode.Point;

        var compression = so.FindProperty("m_TextureSettings.m_TextureCompression");
        if (compression != null) compression.intValue = 0; // None

        so.ApplyModifiedProperties();
        importer.SaveAndReimport();

        Debug.Log("[Setup] Aldric import settings applied — PPU:16, Point filter, No compression.");
    }
}
