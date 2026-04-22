using System.IO;
using UnityEditor;
using UnityEngine;

namespace MnM.Editor
{
    public class ArtGeneratorWindow : EditorWindow
    {
        // ── Style Guide — locked per GDD Appendix B (reference for your art tool) ──
        private const string StyleGuide =
            "Dark pixel art, 16-bit era detail level, high contrast, desaturated palette.\n" +
            "Colours: ash grey (#8A8A8A), bone white (#D4CCBA), dried blood brown (#4A2020),\n" +
            "         Marrow gold (#B8860B), shadow black (#0A0A0C), cold blue-green ambient.\n" +
            "Lighting: torchlight / fire primary. Moonlight for outdoor. NEVER warm sunlight.\n" +
            "Linework: bold pixel outlines on characters & monsters. Thinner on environment.\n" +
            "UI: stone carving texture — chisel marks, relief grooves, worn edges.";

        // ── Suggested names per category (canonical convention: see STAGE_07_B.md) ──
        // Characters:  char_[buildname]_idle.png
        // Monsters:    monster_[name].png
        // UI:          ui_[descriptor].png
        // Settlement:  building_[name].png
        private static readonly string[][] NameSuggestions =
        {
            // Characters — char_[buildname]_idle_[dir]  (_s = south/default)
            new[] { "char_aethel_idle_s", "char_beorn_idle_s", "char_eira_idle_s", "char_freya_idle_s" },
            // Monsters — monster_[name]_[dir]  (_s = south/default)
            new[] { "monster_gaunt_s", "monster_thornback_s", "monster_ivory_stampede_s", "monster_spite_s" },
            // Overlords — overlord_[name]_[dir]  (_s = south/default)
            new[] { "overlord_siltborn_s", "overlord_penitent_s", "overlord_pale_stag_ascendant_s", "overlord_suture_s" },
            // UI — ui_[descriptor]
            new[] { "ui_stone_panel_bg", "ui_card_frame", "ui_button_normal", "ui_icon_bone" },
            // Settlement — building_[name]
            new[] { "building_boneworks", "building_herbalist", "building_lantern", "building_wall" },
        };

        // ── Subfolder options (must match Generated/ subfolders) ────────────────────
        private static readonly string[] Subfolders = { "Characters", "Monsters", "Overlords", "UI", "Settlement" };

        // ── Window State ─────────────────────────────────────────────────────────────
        private string    _saveName      = "new_sprite";
        private string    _saveSubfolder = "Characters";
        private Texture2D _previewTex    = null;
        private string    _statusMessage = "Browse for a PNG to get started.";
        private Vector2   _scrollPos;

        // ─────────────────────────────────────────────────────────────────────────────

        [MenuItem("MnM/Art Generator")]
        public static void ShowWindow()
        {
            var window = GetWindow<ArtGeneratorWindow>("MnM Art Generator");
            window.minSize = new Vector2(520, 640);
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            GUILayout.Label("MARROW & MYTH — Sprite Importer", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            // ── Style reference ───────────────────────────────────────────────────
            EditorGUILayout.LabelField("Style Guide (Appendix B — for your art tool):",
                EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(StyleGuide, MessageType.None);
            EditorGUILayout.Space(10);

            // ── Save settings ─────────────────────────────────────────────────────
            EditorGUILayout.LabelField("Save Settings:", EditorStyles.boldLabel);

            _saveName = EditorGUILayout.TextField("File Name (no ext)", _saveName);

            int folderIdx  = System.Array.IndexOf(Subfolders, _saveSubfolder);
            folderIdx      = EditorGUILayout.Popup("Subfolder", folderIdx < 0 ? 0 : folderIdx, Subfolders);
            _saveSubfolder = Subfolders[folderIdx];

            string savePath = $"Assets/_Game/Art/Generated/{_saveSubfolder}/{_saveName}.png";
            EditorGUILayout.LabelField("Save path:", savePath);
            EditorGUILayout.Space(6);

            // ── Name suggestions for selected subfolder ───────────────────────────
            EditorGUILayout.LabelField("Quick Name Suggestions:", EditorStyles.miniLabel);
            EditorGUILayout.BeginHorizontal();
            foreach (var suggestion in NameSuggestions[folderIdx])
            {
                if (GUILayout.Button(suggestion, EditorStyles.miniButton))
                    _saveName = suggestion;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);

            // ── Browse button ─────────────────────────────────────────────────────
            if (GUILayout.Button("BROWSE FOR PNG...", GUILayout.Height(48)))
                BrowseForPng();

            // ── Status bar ────────────────────────────────────────────────────────
            EditorGUILayout.HelpBox(_statusMessage, MessageType.None);

            // ── Preview + save / discard ──────────────────────────────────────────
            if (_previewTex != null)
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.LabelField(
                    $"Preview: {_previewTex.width}x{_previewTex.height}px",
                    EditorStyles.boldLabel);

                float aspect   = (float)_previewTex.height / _previewTex.width;
                float previewW = Mathf.Min(256f, position.width - 32f);
                float previewH = previewW * aspect;
                Rect  rect     = GUILayoutUtility.GetRect(previewW, previewH);
                EditorGUI.DrawPreviewTexture(rect, _previewTex, null, ScaleMode.ScaleToFit);
                EditorGUILayout.Space(8);

                if (GUILayout.Button("SAVE TO PROJECT", GUILayout.Height(36)))
                    SaveTextureToDisk(_previewTex, savePath);

                if (GUILayout.Button("Discard", GUILayout.Height(26)))
                    ClearPreview();
            }

            EditorGUILayout.EndScrollView();
        }

        // ── Browse ────────────────────────────────────────────────────────────────

        private void BrowseForPng()
        {
            string picked = EditorUtility.OpenFilePanel(
                "Select sprite PNG", "", "png");

            if (string.IsNullOrEmpty(picked))
                return; // user cancelled

            byte[] bytes = File.ReadAllBytes(picked);
            var tex = new Texture2D(2, 2);

            if (!tex.LoadImage(bytes))
            {
                DestroyImmediate(tex);
                _statusMessage = $"Could not load image: {Path.GetFileName(picked)}";
                Debug.LogWarning("[ArtGen] " + _statusMessage);
                Repaint();
                return;
            }

            // Auto-fill the file name from the picked file (without extension)
            _saveName = Path.GetFileNameWithoutExtension(picked);

            ClearPreview();
            _previewTex    = tex;
            _statusMessage = $"Loaded \"{Path.GetFileName(picked)}\" — " +
                             $"{tex.width}x{tex.height}px. Adjust name/subfolder then SAVE.";
            Debug.Log($"[ArtGen] Preview loaded: {picked} ({tex.width}x{tex.height})");
            Repaint();
        }

        // ── Save to project ───────────────────────────────────────────────────────

        private void SaveTextureToDisk(Texture2D tex, string assetPath)
        {
            string dir = Path.GetDirectoryName(assetPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            byte[] png = tex.EncodeToPNG();

            string projectRoot = Application.dataPath.Substring(
                0, Application.dataPath.Length - "Assets".Length);
            string fullPath = Path.Combine(projectRoot, assetPath);

            File.WriteAllBytes(fullPath, png);
            AssetDatabase.ImportAsset(assetPath);
            AssetDatabase.Refresh();

            _statusMessage = $"Saved: {assetPath}";
            Debug.Log($"[ArtGen] Sprite saved → {assetPath}");
            Repaint();
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private void ClearPreview()
        {
            if (_previewTex != null)
            {
                DestroyImmediate(_previewTex);
                _previewTex = null;
            }
            _statusMessage = "Browse for a PNG to get started.";
        }

        private void OnDestroy() => ClearPreview();
    }
}
