using UnityEditor;
using UnityEngine;

public class FixBgSpriteImport
{
    public static void Execute()
    {
        string bgPath   = "Assets/_Game/Art/Generated/UI/ui_main_menu_bg.png";
        string logoPath = "Assets/_Game/Art/Generated/UI/ui_title_logo.png";

        // Background — smooth scaling, native PPU so UI Toolkit fills the container
        FixSprite(bgPath,   filterMode: FilterMode.Bilinear, ppu: 1,  wrapMode: TextureWrapMode.Clamp);

        // Logo — keep pixel-art crisp but fix PPU so it doesn't render tiny
        FixSprite(logoPath, filterMode: FilterMode.Point,    ppu: 1,  wrapMode: TextureWrapMode.Clamp);

        AssetDatabase.Refresh();
        Debug.Log("[FixBgImport] Done — reimport complete.");
    }

    static void FixSprite(string path, FilterMode filterMode, int ppu, TextureWrapMode wrapMode)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) { Debug.LogError($"[FixBgImport] Not found: {path}"); return; }

        importer.textureType        = TextureImporterType.Sprite;
        importer.filterMode         = filterMode;
        importer.spritePixelsPerUnit = ppu;
        importer.wrapMode           = wrapMode;
        importer.mipmapEnabled      = false;
        importer.SaveAndReimport();
        Debug.Log($"[FixBgImport] Reimported: {path} | filter={filterMode} ppu={ppu}");
    }
}
