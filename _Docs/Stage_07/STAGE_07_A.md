<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 7-A | Art Pipeline — Sprite Importer Editor Window
Status: Stage 6 complete. Full year loop verified.
Task: Build the Unity Editor Window that lets you pick
an existing PNG file from disk, preview it, set the target
subfolder and filename, and copy it into the correct
Assets/_Game/Art/Generated/ folder. This is an Editor-only
tool — no runtime code. No API calls.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_07/STAGE_07_A.md
- Assets/_Game/Scripts/Core.Data/Enums.cs

Then confirm:
- That this is an Editor script (Editor folder, not runtime)
- That the PNG picker uses EditorUtility.OpenFilePanel
- That images are copied to Assets/_Game/Art/Generated/
- That the tool shows a preview before saving
- That no runtime code is touched this session

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 7-A: Art Pipeline — Sprite Importer Editor Window

> ✅ **COMPLETE** — Committed as `"7A: Art Generator Editor Window — PNG file picker, preview, save-to-disk"`

**Resuming from:** Stage 6 complete  
**Done when:** The Sprite Importer window opens in Unity (Window → MnM → Art Importer), lets you browse for a PNG, previews it, and copies it to the correct subfolder under `Assets/_Game/Art/Generated/`  
**Commit:** `"7A: Art Generator Editor Window — PNG file picker, preview, save-to-disk"`  
**Next session:** STAGE_07_B.md  

---

## Folder Structure

```
Assets/
├── _Game/
│   ├── Art/
│   │   └── Generated/
│   │       ├── Characters/
│   │       ├── Monsters/
│   │       ├── UI/
│   │       └── Settlement/
│   └── Editor/
│       └── ArtGeneratorWindow.cs    ← Built this session
```

---

## GDD Appendix B — Style Reference

All sprites must match this style. Use these constants when creating art externally.

```
Style:    Dark pixel art, 16-bit era detail level, high contrast, desaturated palette
Palette:  Ash grey (#8A8A8A), bone white (#D4CCBA), dried blood brown (#4A2020),
          Marrow gold (#B8860B), shadow black (#0A0A0C), cold blue-green ambient
Lighting: Torchlight and fire primary. Moonlight for outdoor. NEVER warm sunlight.
Linework: Bold pixel outlines on characters and monsters. Thinner on environment.
UI:       Stone carving texture — chisel marks, relief grooves, worn edges.
```

---

## ArtGeneratorWindow.cs (Sprite Importer)

**Path:** `Assets/_Game/Editor/ArtGeneratorWindow.cs`

```csharp
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MnM.Editor
{
    public class ArtGeneratorWindow : EditorWindow
    {
        // ── Window State ─────────────────────────────────────────
        private string    _sourcePath      = "";
        private string    _saveName        = "new_sprite";
        private string    _saveSubfolder   = "Characters";
        private Texture2D _previewTex      = null;
        private string    _statusMessage   = "Pick a PNG to import.";
        private Vector2   _scrollPos;

        private static readonly string[] Subfolders =
            { "Characters", "Monsters", "UI", "Settlement" };

        [MenuItem("MnM/Art Importer")]
        public static void ShowWindow()
        {
            var window = GetWindow<ArtGeneratorWindow>("MnM Art Importer");
            window.minSize = new Vector2(500, 600);
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            GUILayout.Label("MARROW & MYTH — Sprite Importer", EditorStyles.boldLabel);
            GUILayout.Label("Pick a PNG from disk, preview it, then save to the project.",
                EditorStyles.helpBox);
            EditorGUILayout.Space(8);

            // ── File picker ──────────────────────────────────────
            EditorGUILayout.LabelField("Source PNG:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(
                string.IsNullOrEmpty(_sourcePath) ? "(none)" : _sourcePath,
                EditorStyles.miniLabel);
            if (GUILayout.Button("Browse...", GUILayout.Width(80)))
                PickFile();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(8);

            // ── Save settings ────────────────────────────────────
            EditorGUILayout.LabelField("Save Settings:", EditorStyles.boldLabel);
            _saveName      = EditorGUILayout.TextField("File Name (no ext)", _saveName);
            int folderIdx  = System.Array.IndexOf(Subfolders, _saveSubfolder);
            folderIdx      = EditorGUILayout.Popup("Subfolder", folderIdx < 0 ? 0 : folderIdx, Subfolders);
            _saveSubfolder = Subfolders[folderIdx];
            string savePath = $"Assets/_Game/Art/Generated/{_saveSubfolder}/{_saveName}.png";
            EditorGUILayout.LabelField("Target path:", savePath);
            EditorGUILayout.Space(8);

            // ── Preview ──────────────────────────────────────────
            if (_previewTex != null)
            {
                EditorGUILayout.LabelField("Preview:", EditorStyles.boldLabel);
                float aspect   = (float)_previewTex.height / _previewTex.width;
                float previewW = Mathf.Min(256f, position.width - 32f);
                float previewH = previewW * aspect;
                var rect       = GUILayoutUtility.GetRect(previewW, previewH);
                EditorGUI.DrawPreviewTexture(rect, _previewTex, null, ScaleMode.ScaleToFit);
                EditorGUILayout.Space(8);
            }

            // ── Import button ─────────────────────────────────────
            GUI.enabled = !string.IsNullOrEmpty(_sourcePath) && _previewTex != null;
            if (GUILayout.Button("IMPORT TO PROJECT", GUILayout.Height(48)))
                ImportFile(savePath);
            GUI.enabled = true;

            EditorGUILayout.HelpBox(_statusMessage, MessageType.None);
            EditorGUILayout.EndScrollView();
        }

        // ── File picker ──────────────────────────────────────────
        private void PickFile()
        {
            string path = EditorUtility.OpenFilePanel("Select PNG", "", "png");
            if (string.IsNullOrEmpty(path)) return;

            _sourcePath = path;

            // Auto-fill save name from filename (no extension)
            _saveName = Path.GetFileNameWithoutExtension(path);

            // Load preview
            if (_previewTex != null) { DestroyImmediate(_previewTex); _previewTex = null; }
            byte[] bytes = File.ReadAllBytes(path);
            var tex = new Texture2D(2, 2);
            if (tex.LoadImage(bytes))
            {
                _previewTex    = tex;
                _statusMessage = $"Ready — {tex.width}×{tex.height}px";
            }
            else
            {
                _statusMessage = "Failed to load image. Ensure file is a valid PNG.";
            }
            Repaint();
        }

        // ── Import ───────────────────────────────────────────────
        private void ImportFile(string assetPath)
        {
            string dir = Path.GetDirectoryName(
                Path.Combine(Application.dataPath.Replace("Assets", ""), assetPath));
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            string fullDest = Path.Combine(
                Application.dataPath.Replace("Assets", ""), assetPath);
            File.Copy(_sourcePath, fullDest, overwrite: true);
            AssetDatabase.ImportAsset(assetPath);
            AssetDatabase.Refresh();

            _statusMessage = $"Imported: {assetPath}";
            Debug.Log($"[ArtImporter] Imported sprite → {assetPath}");
        }
    }
}
```

---

## Verification Test

1. Open Unity → Window → MnM → Art Importer
2. Click **Browse...** — OS file picker opens, select any PNG
3. Confirm preview appears in the window
4. Set file name and subfolder, confirm target path updates
5. Click **IMPORT TO PROJECT** — file copied to `Assets/_Game/Art/Generated/<subfolder>/`
6. Confirm the asset appears in the Project window with no errors

---

## Next Session

**File:** `_Docs/Stage_07/STAGE_07_B.md`  
**Covers:** Import pipeline verification — import first two sprites, confirm folder structure and naming conventions before batch work begins
