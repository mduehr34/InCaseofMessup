<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 7-A | Art Pipeline — Anthropic Image API Editor Window
Status: Stage 6 complete. Full year loop verified.
Task: Build the Unity Editor Window that calls the
Anthropic image API to generate game sprites. This is an
Editor-only tool — it runs in the Unity Editor, not at
runtime. Build the window, the API call, the preview panel,
and the save-to-disk system. Do NOT generate any images yet
— just build the tool.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_07/STAGE_07_A.md
- Assets/_Game/Scripts/Core.Data/Enums.cs

Then confirm:
- That this is an Editor script (Editor folder, not runtime)
- That images are saved to Assets/_Game/Art/Generated/
- That the tool has a preview panel before saving
- That no runtime code is touched this session

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 7-A: Art Pipeline — Anthropic Image API Editor Window

**Resuming from:** Stage 6 complete  
**Done when:** The Art Generator window opens in Unity (Window menu), prompts can be typed, a mock API call runs without error, and the save path is correct  
**Commit:** `"7A: Art Generator Editor Window — API integration, preview, save-to-disk"`  
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
│       └── ArtGeneratorWindow.cs    ← Create this session
```

---

## GDD Appendix B — Style Constants

These are locked. Every generation prompt must begin with the style prefix.

```
Style:    Dark pixel art, 16-bit era detail level, high contrast, desaturated palette
Palette:  Ash grey (#8A8A8A), bone white (#D4CCBA), dried blood brown (#4A2020),
          Marrow gold (#B8860B), shadow black (#0A0A0C), cold blue-green ambient
Lighting: Torchlight and fire primary. Moonlight for outdoor. NEVER warm sunlight.
Linework: Bold pixel outlines on characters and monsters. Thinner on environment.
UI:       Stone carving texture — chisel marks, relief grooves, worn edges.
```

---

## ArtGeneratorWindow.cs

**Path:** `Assets/_Game/Editor/ArtGeneratorWindow.cs`

```csharp
using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace MnM.Editor
{
    public class ArtGeneratorWindow : EditorWindow
    {
        // ── Style Prefix — locked per GDD Appendix B ────────────
        private const string StylePrefix =
            "Dark pixel art, 16-bit era detail level, high contrast, " +
            "desaturated palette, ash grey, bone white, dried blood brown, " +
            "Marrow gold (#B8860B), bold pixel outlines on characters, " +
            "torchlight primary lighting, never warm sunlight, SNES RPG style — ";

        // ── Prompt Templates (Appendix B) ───────────────────────
        private static readonly string[] QuickPrompts = new[]
        {
            "CHARACTER: 32x64 sprite, male warrior survivor, lean build, primitive leather wraps, bone jewelry, idle frame",
            "CHARACTER: 32x64 sprite, female warrior survivor, athletic build, hide wraps, bone ornaments, idle frame",
            "MONSTER: 64x64 sprite, enormous blind wolf, Marrow-starved, no eyes, massive jaw, facing left",
            "UI: stone panel texture, carved tablet surface, chisel marks, relief grooves, worn edges, tileable 64x64",
            "SETTLEMENT: Boneworks structure, bone-and-sinew workshop building, primitive construction, top-down isometric",
        };

        // ── Window State ─────────────────────────────────────────
        private string  _promptSuffix    = "";
        private string  _selectedQuick   = "";
        private string  _saveName        = "new_sprite";
        private string  _saveSubfolder   = "Characters";
        private Texture2D _previewTex    = null;
        private bool    _isGenerating    = false;
        private string  _statusMessage   = "Ready";
        private Vector2 _scrollPos;

        private static readonly string[] Subfolders =
            { "Characters", "Monsters", "UI", "Settlement" };

        [MenuItem("MnM/Art Generator")]
        public static void ShowWindow()
        {
            var window = GetWindow<ArtGeneratorWindow>("MnM Art Generator");
            window.minSize = new Vector2(600, 700);
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            GUILayout.Label("MARROW & MYTH — Art Generator", EditorStyles.boldLabel);
            GUILayout.Label("All prompts auto-prepend the GDD Appendix B style prefix.",
                EditorStyles.helpBox);
            EditorGUILayout.Space(8);

            // ── Style prefix preview ─────────────────────────────
            EditorGUILayout.LabelField("Style Prefix (locked):", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(StylePrefix, MessageType.None);
            EditorGUILayout.Space(8);

            // ── Quick prompts ────────────────────────────────────
            EditorGUILayout.LabelField("Quick Templates:", EditorStyles.boldLabel);
            foreach (var qp in QuickPrompts)
            {
                if (GUILayout.Button(qp, GUILayout.Height(30)))
                {
                    _selectedQuick = qp;
                    _promptSuffix  = qp;
                }
            }
            EditorGUILayout.Space(8);

            // ── Custom prompt ────────────────────────────────────
            EditorGUILayout.LabelField("Custom Prompt Suffix:", EditorStyles.boldLabel);
            _promptSuffix = EditorGUILayout.TextArea(_promptSuffix, GUILayout.Height(80));
            EditorGUILayout.Space(4);

            // Full prompt preview
            string fullPrompt = StylePrefix + _promptSuffix;
            EditorGUILayout.LabelField("Full Prompt (sent to API):", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(fullPrompt, MessageType.None);
            EditorGUILayout.Space(8);

            // ── Save settings ────────────────────────────────────
            EditorGUILayout.LabelField("Save Settings:", EditorStyles.boldLabel);
            _saveName      = EditorGUILayout.TextField("File Name (no ext)", _saveName);
            int folderIdx  = System.Array.IndexOf(Subfolders, _saveSubfolder);
            folderIdx      = EditorGUILayout.Popup("Subfolder", folderIdx < 0 ? 0 : folderIdx, Subfolders);
            _saveSubfolder = Subfolders[folderIdx];
            string savePath = $"Assets/_Game/Art/Generated/{_saveSubfolder}/{_saveName}.png";
            EditorGUILayout.LabelField("Save path:", savePath);
            EditorGUILayout.Space(8);

            // ── Generate button ──────────────────────────────────
            GUI.enabled = !_isGenerating && !string.IsNullOrEmpty(_promptSuffix);
            if (GUILayout.Button(_isGenerating ? "Generating..." : "GENERATE", GUILayout.Height(48)))
                GenerateImage(fullPrompt, savePath);
            GUI.enabled = true;

            // ── Status ───────────────────────────────────────────
            EditorGUILayout.HelpBox(_statusMessage,
                _isGenerating ? MessageType.Info : MessageType.None);

            // ── Preview ──────────────────────────────────────────
            if (_previewTex != null)
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.LabelField("Preview:", EditorStyles.boldLabel);

                // Scale preview — max 256px wide
                float aspect = (float)_previewTex.height / _previewTex.width;
                float previewW = Mathf.Min(256f, position.width - 32f);
                float previewH = previewW * aspect;
                var rect = GUILayoutUtility.GetRect(previewW, previewH);
                EditorGUI.DrawPreviewTexture(rect, _previewTex, null, ScaleMode.ScaleToFit);
                EditorGUILayout.Space(8);

                if (GUILayout.Button("SAVE TO PROJECT", GUILayout.Height(36)))
                    SaveTextureToDisk(_previewTex, savePath);

                if (GUILayout.Button("Discard & Re-generate", GUILayout.Height(28)))
                {
                    DestroyImmediate(_previewTex);
                    _previewTex    = null;
                    _statusMessage = "Ready";
                }
            }

            EditorGUILayout.EndScrollView();
        }

        // ── API Call ─────────────────────────────────────────────
        private void GenerateImage(string prompt, string savePath)
        {
            _isGenerating  = true;
            _statusMessage = "Calling Anthropic image API...";
            _previewTex    = null;
            Repaint();

            // Run as coroutine via EditorApplication.update
            var routine = CallImageAPI(prompt, savePath);
            EditorCoroutineRunner.Run(routine, () => {
                _isGenerating = false;
                Repaint();
            });
        }

        private IEnumerator CallImageAPI(string prompt, string savePath)
        {
            // Anthropic image generation endpoint
            // Model: claude-opus-4-6 with tool_use for image generation
            // ⚑ API key is handled by the environment — do not hardcode

            var requestBody = new ImageRequestBody
            {
                model      = "claude-opus-4-6",
                max_tokens = 1024,
                messages   = new[] {
                    new Message {
                        role    = "user",
                        content = new[] {
                            new Content {
                                type = "text",
                                text = $"Generate a pixel art sprite with this description: {prompt}\n\n" +
                                       "Return ONLY the image, no text."
                            }
                        }
                    }
                }
            };

            string json = JsonUtility.ToJson(requestBody);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            using var request = new UnityWebRequest(
                "https://api.anthropic.com/v1/messages", "POST");
            request.uploadHandler   = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type",         "application/json");
            request.SetRequestHeader("anthropic-version",    "2023-06-01");
            // API key from environment variable — set in Unity preferences
            string apiKey = System.Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY") ?? "";
            request.SetRequestHeader("x-api-key", apiKey);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                _statusMessage = $"API Error: {request.error}\n{request.downloadHandler.text}";
                Debug.LogError($"[ArtGen] {_statusMessage}");
                yield break;
            }

            // Parse response — look for image content block
            string responseText = request.downloadHandler.text;
            Debug.Log($"[ArtGen] Response received ({responseText.Length} chars)");

            // Extract base64 image data from response
            byte[] imageBytes = ParseImageFromResponse(responseText);
            if (imageBytes == null || imageBytes.Length == 0)
            {
                _statusMessage = "No image in response. Check prompt or API response format.";
                Debug.LogWarning($"[ArtGen] Raw response: {responseText}");
                yield break;
            }

            // Load into Texture2D for preview
            var tex = new Texture2D(2, 2);
            if (tex.LoadImage(imageBytes))
            {
                _previewTex    = tex;
                _statusMessage = $"Image generated! ({tex.width}×{tex.height}px) — Review and save.";
                Debug.Log($"[ArtGen] Image ready: {tex.width}×{tex.height}");
            }
            else
            {
                _statusMessage = "Failed to decode image bytes.";
            }
        }

        private static byte[] ParseImageFromResponse(string responseJson)
        {
            // Look for base64 image data in Anthropic API response
            // Response format: {"content":[{"type":"image","source":{"type":"base64","data":"..."}}]}
            const string dataMarker = "\"data\":\"";
            int dataStart = responseJson.IndexOf(dataMarker);
            if (dataStart < 0) return null;

            dataStart += dataMarker.Length;
            int dataEnd = responseJson.IndexOf("\"", dataStart);
            if (dataEnd < 0) return null;

            string base64 = responseJson.Substring(dataStart, dataEnd - dataStart);
            try { return System.Convert.FromBase64String(base64); }
            catch { return null; }
        }

        // ── Save ─────────────────────────────────────────────────
        private void SaveTextureToDisk(Texture2D tex, string assetPath)
        {
            // Ensure directory exists
            string dir = Path.GetDirectoryName(assetPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            byte[] png = tex.EncodeToPNG();
            string fullPath = Path.Combine(
                Application.dataPath.Replace("Assets", ""), assetPath);
            File.WriteAllBytes(fullPath, png);
            AssetDatabase.ImportAsset(assetPath);
            AssetDatabase.Refresh();

            _statusMessage = $"Saved: {assetPath}";
            Debug.Log($"[ArtGen] Saved sprite to {assetPath}");
        }

        // ── JSON helper structs ──────────────────────────────────
        [System.Serializable] private class ImageRequestBody
        {
            public string model;
            public int max_tokens;
            public Message[] messages;
        }
        [System.Serializable] private class Message
        {
            public string role;
            public Content[] content;
        }
        [System.Serializable] private class Content
        {
            public string type;
            public string text;
        }
    }

    // ── Minimal EditorCoroutine runner ───────────────────────────
    public static class EditorCoroutineRunner
    {
        private static IEnumerator _current;
        private static System.Action _onComplete;

        public static void Run(IEnumerator routine, System.Action onComplete = null)
        {
            _current    = routine;
            _onComplete = onComplete;
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            if (_current == null || !_current.MoveNext())
            {
                EditorApplication.update -= Update;
                _onComplete?.Invoke();
                _current    = null;
                _onComplete = null;
            }
        }
    }
}
```

---

## API Key Setup

Before generating any images:

1. Set your Anthropic API key as an environment variable:
   - **Windows:** `setx ANTHROPIC_API_KEY "your-key-here"`
   - **Mac/Linux:** Add `export ANTHROPIC_API_KEY="your-key-here"` to `~/.zshrc`
2. Restart Unity after setting the variable
3. Verify with a test: open Window → MnM → Art Generator, type a prompt, click GENERATE

> ⚑ Never hardcode the API key in source files. The environment variable approach keeps it out of version control.

---

## Verification Test

1. Open Unity → Window → MnM → Art Generator
2. Confirm the window opens with style prefix displayed
3. Click a Quick Template — confirm it populates the prompt field
4. Confirm save path updates correctly when file name and subfolder change
5. Click GENERATE — confirm status changes to "Calling Anthropic image API..."
6. Confirm `Assets/_Game/Art/Generated/` folder structure exists (create manually if needed)
7. If API key is set correctly, image should preview after ~5–15 seconds

---

## Next Session

**File:** `_Docs/Stage_07/STAGE_07_B.md`  
**Covers:** Style lock — generate character variants, review, lock the canonical prompt template
