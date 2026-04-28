using UnityEditor;
using UnityEngine;

namespace MnM.Editor
{
    /// <summary>
    /// Automatically applies correct pixel-art import settings to every PNG saved
    /// under Assets/_Game/Art/Generated/.
    ///
    /// Settings enforced:
    ///   Texture Type  → Sprite (2D and UI)
    ///   Filter Mode   → Point (No Filter)   ← critical — prevents blurring
    ///   Pixels/Unit   → 16
    ///   Compression   → None
    ///   Max Size      → 64
    ///
    /// No manual Inspector edits required — every sprite in the pipeline inherits
    /// these settings on first import and on reimport.
    /// </summary>
    public class SpriteImportPostprocessor : AssetPostprocessor
    {
        private const string GeneratedPath    = "Assets/_Game/Art/Generated/";
        private const string OverlordsSubpath = "Assets/_Game/Art/Generated/Overlords/";
        private const string UISubpath        = "Assets/_Game/Art/Generated/UI/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(GeneratedPath))
                return;

            var importer = (TextureImporter)assetImporter;

            // UI background/logo images need full resolution + bilinear — skip pixel-art enforcement
            if (assetPath.StartsWith(UISubpath))
            {
                importer.textureType         = TextureImporterType.Sprite;
                importer.spriteImportMode    = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 1f;
                importer.filterMode          = FilterMode.Bilinear;
                importer.mipmapEnabled       = false;
                importer.maxTextureSize      = 2048;
                var uiSettings = importer.GetDefaultPlatformTextureSettings();
                uiSettings.format            = TextureImporterFormat.RGBA32;
                uiSettings.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SetPlatformTextureSettings(uiSettings);
                Debug.Log($"[SpritePostprocessor] Applied UI-art settings to: {assetPath}");
                return;
            }

            importer.textureType         = TextureImporterType.Sprite;
            importer.spriteImportMode    = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 16f;
            importer.filterMode          = FilterMode.Point;
            importer.mipmapEnabled       = false;
            // Overlords are 96×96 — need maxTextureSize 128; standard sprites are 64 or smaller
            importer.maxTextureSize      = assetPath.StartsWith(OverlordsSubpath) ? 128 : 64;

            var settings = importer.GetDefaultPlatformTextureSettings();
            settings.format      = TextureImporterFormat.RGBA32;
            settings.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SetPlatformTextureSettings(settings);

            Debug.Log($"[SpritePostprocessor] Applied pixel-art settings to: {assetPath}");
        }
    }
}
