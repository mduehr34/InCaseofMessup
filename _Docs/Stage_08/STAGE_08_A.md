<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 8-A | Main Menu Scene — Art, UI, and Controller
Status: Stage 7 complete. Full Tutorial playthrough verified.
Task: Create the MainMenu Unity scene with atmospheric
background art, title logo, and four navigation buttons.
Build MainMenuController.cs. Wire the scene into Build
Settings as scene index 0.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_08/STAGE_08_A.md

Then confirm:
- The scene uses UIToolkit ONLY — no uGUI components
- MainMenu.unity is scene index 0 in Build Settings
- Background art is generated via CoPlay MCP
  generate_or_edit_images tool using the exact prompts below
- CONTINUE button is disabled when no save file exists
- What you will NOT do this session (audio wiring — that is 8-D)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 8-A: Main Menu Scene — Art, UI, and Controller

**Resuming from:** Stage 7-Q complete — full game verified
**Done when:** Launching the game opens MainMenu.unity first; background and title logo display; all 4 buttons respond correctly; CONTINUE is disabled with no save file
**Commit:** `"8A: Main menu scene — background art, UIToolkit layout, MainMenuController"`
**Next session:** STAGE_08_B.md

---

## What You Are Building

The main menu is the first screen the player sees. It needs:
1. A dark, atmospheric full-screen background image
2. The game title "Marrow & Myth" as a pixel art logo
3. Four buttons: NEW GAME, CONTINUE, SETTINGS, QUIT
4. A `MainMenuController.cs` script that makes buttons work

**New developer note:** A "scene" in Unity is like a separate room or screen. Build Settings controls which scene loads first when the game starts. We want MainMenu to always be scene index 0 (the first one).

---

## Step 1: Create Folders

In the Unity Project window, create these folders if they don't exist:
```
Assets/_Game/UI/              ← UXML and USS files go here
Assets/_Game/Art/Generated/UI/ ← already exists from Stage 7-E
```

Right-click in the Project window → Create → Folder to create them.

---

## Step 2: Generate Background Art via CoPlay

Use the CoPlay MCP `generate_or_edit_images` tool with this exact prompt:

**Background (640×360):**
```
Dark pixel art main menu background. A lone settlement cluster
of stone buildings at night, distant torch lights flickering.
Bone-white crescent moon. Rolling dark fog in the midground.
Foreground silhouettes of dead gnarled trees and crumbled ruins.
Vast, empty wilderness beyond. Style: 16-bit era pixel art,
high contrast, desaturated. Palette: ash grey #8A8A8A,
bone white #D4CCBA, shadow black #0A0A0C, marrow gold #B8860B
for torch glow. Cinematic, haunting, lonely atmosphere.
```
Save to: `Assets/_Game/Art/Generated/UI/ui_main_menu_bg.png`
Import settings: Texture Type = Sprite (2D and UI), Filter Mode = Point (No Filter), PPU = 16

**Title Logo (320×64):**
```
Pixel art title logo reading "MARROW & MYTH". Stone-carved
letters with marrow gold (#B8860B) fill and bone white
highlights on edges. Cracked stone texture on the letters.
Small skull-and-crossed-bone motif beneath the ampersand.
Transparent background. High contrast. 16-bit era detail.
```
Save to: `Assets/_Game/Art/Generated/UI/ui_title_logo.png`
Import settings: same as above

---

## Step 3: Create the MainMenu Scene

1. **File → New Scene** → choose **Empty**
2. **File → Save As** → save as `Assets/MainMenu.unity`
3. In the Hierarchy, right-click → **UI Toolkit → UI Document** — this creates a GameObject with a `UIDocument` component
4. Rename the GameObject to `MainMenuUI`
5. **File → Build Settings** → click **Add Open Scenes** → drag MainMenu.unity to index 0 (above CombatScene)

---

## Step 4: Panel Settings Asset

1. In the Project window: right-click → **Create → UI Toolkit → Panel Settings**
2. Name it `MainMenuPanelSettings`
3. Set **Scale Mode** to `Scale With Screen Size`, Reference Resolution `1280 × 720`
4. Select `MainMenuUI` in Hierarchy → drag `MainMenuPanelSettings` into the **Panel Settings** field on the UIDocument component

---

## Step 5: UXML Layout

Right-click in Project → **Create → UI Toolkit → UI Document** → name it `MainMenu.uxml`
Paste this content into the file (double-click to open in a text editor, or use the code below):

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
  <ui:VisualElement name="root" style="width:100%; height:100%; align-items:center; justify-content:center; background-color:#0A0A0C;">
    <ui:VisualElement name="bg" style="position:absolute; left:0; top:0; right:0; bottom:0; -unity-background-scale-mode:scale-to-fit;" />
    <ui:VisualElement name="title-logo" style="width:320px; height:64px; margin-bottom:16px; -unity-background-scale-mode:scale-to-fit;" />
    <ui:Label name="subtitle" text="A settlement built on bones. A hunt that never ends."
              style="color:#8A8A8A; font-size:11px; margin-bottom:48px; -unity-text-align:middle-center;" />
    <ui:VisualElement name="button-col" style="flex-direction:column; align-items:center;">
      <ui:Button name="btn-new-game"  text="NEW GAME"  class="mnm-btn" />
      <ui:Button name="btn-continue"  text="CONTINUE"  class="mnm-btn" />
      <ui:Button name="btn-settings"  text="SETTINGS"  class="mnm-btn" />
      <ui:Button name="btn-quit"      text="QUIT"       class="mnm-btn" />
    </ui:VisualElement>
    <ui:Label name="version" text="v0.1 — Early Access"
              style="position:absolute; bottom:8px; right:12px; color:#404040; font-size:9px;" />
  </ui:VisualElement>
</ui:UXML>
```

---

## Step 6: USS Styles

Right-click → **Create → UI Toolkit → Style Sheet** → name it `MainMenu.uss`

```css
.mnm-btn {
    width: 240px;
    height: 48px;
    margin-bottom: 12px;
    background-color: rgb(28, 22, 16);
    border-color: rgb(184, 134, 11);
    border-width: 2px;
    color: rgb(212, 204, 186);
    font-size: 14px;
    -unity-text-align: middle-center;
    transition: background-color 0.1s;
}

.mnm-btn:hover {
    background-color: rgb(55, 42, 28);
    border-color: rgb(212, 204, 186);
}

.mnm-btn:disabled {
    opacity: 0.35;
}
```

In the Inspector for the UIDocument on `MainMenuUI`:
- Set **Source Asset** to `MainMenu.uxml`
- On the UXML, add the USS as a **Style Sheet** reference

---

## Step 7: MainMenuController.cs

**Path:** `Assets/_Game/Scripts/Core.UI/MainMenuController.cs`

**What this does:** Wires button clicks to scene navigation. Checks if a save file exists to enable/disable CONTINUE.

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using MnM.Core.Systems;

namespace MnM.Core.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;

        private void OnEnable()
        {
            var root = _uiDocument.rootVisualElement;

            // Wire background image
            var bgSprite = Resources.Load<Sprite>("Art/Generated/UI/ui_main_menu_bg");
            if (bgSprite != null)
                root.Q("bg").style.backgroundImage = new StyleBackground(bgSprite);

            var logoSprite = Resources.Load<Sprite>("Art/Generated/UI/ui_title_logo");
            if (logoSprite != null)
                root.Q("title-logo").style.backgroundImage = new StyleBackground(logoSprite);

            // Wire buttons
            root.Q<Button>("btn-new-game").RegisterCallback<ClickEvent>(_ => OnNewGame());
            root.Q<Button>("btn-continue").RegisterCallback<ClickEvent>(_ => OnContinue());
            root.Q<Button>("btn-settings").RegisterCallback<ClickEvent>(_ => OnSettings());
            root.Q<Button>("btn-quit")    .RegisterCallback<ClickEvent>(_ => OnQuit());

            // Disable Continue if no save exists
            root.Q<Button>("btn-continue").SetEnabled(SaveSystem.HasSave());

            Debug.Log("[MainMenu] Ready");
        }

        private void OnNewGame()
        {
            Debug.Log("[MainMenu] → CampaignSelect");
            SceneManager.LoadScene("CampaignSelect");
        }

        private void OnContinue()
        {
            SaveSystem.LoadMostRecent();
            SceneManager.LoadScene("Settlement");
        }

        private void OnSettings()
        {
            SceneManager.LoadScene("Settings");
        }

        private void OnQuit()
        {
            Debug.Log("[MainMenu] Quit");
            Application.Quit();
        }
    }
}
```

**Attaching the script:**
1. Select `MainMenuUI` in the Hierarchy
2. Inspector → **Add Component** → search `MainMenuController` → click it
3. Drag the `MainMenuUI` GameObject into the **Ui Document** field

**Move sprites to Resources:**
Create `Assets/Resources/Art/Generated/UI/` and copy both PNGs there so `Resources.Load` can find them at runtime.

---

## Verification Test

- [ ] Press Play — MainMenu.unity loads first, not CombatScene
- [ ] Dark background art covers the full screen
- [ ] Title logo displays below the top of the screen
- [ ] CONTINUE button is greyed out (no save file exists yet)
- [ ] Clicking NEW GAME logs "[MainMenu] → CampaignSelect" in the Console
- [ ] QUIT button exits Play Mode in the Editor
- [ ] No errors in the Console on load
- [ ] No uGUI (Canvas, Image, Text) components anywhere in the Hierarchy

---

## Next Session

**File:** `_Docs/Stage_08/STAGE_08_B.md`
**Covers:** Campaign Select scene — Tutorial vs Standard campaign picker, difficulty selector, ironman toggle, CONFIRM button that starts the game
