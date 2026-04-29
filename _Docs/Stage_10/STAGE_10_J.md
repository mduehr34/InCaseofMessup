<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 10-J | Credits Sequence — Full Credits Scene
Status: Stage 10-I complete. UI polish and settings done.
Task: Build the Credits scene. It currently exists as an empty
stub from Stage 8-R. This session fills it completely:
  - Scrolling credits text (developer, tools, audio, inspiration)
  - A generated title card image at the top
  - Warm atmospheric music playing through the scroll
  - "RETURN TO MAIN MENU" button at the end of the scroll
  - The Credits scene is reachable from both the Main Menu
    and from the Victory Epilogue screen (after the epilogue fades)

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_10/STAGE_10_J.md
- Assets/_Game/Scripts/Core.UI/VictoryEpilogueController.cs
- Assets/_Game/Scripts/Core.UI/MainMenuController.cs
- Assets/_Game/Scripts/Core.Systems/AudioManager.cs

Then confirm:
- Credits scene exists in Build Settings at index 9
- VictoryEpilogueController has a "CONTINUE" button that currently goes to Main Menu
- Main Menu has or should have a CREDITS button
- SceneTransitionManager.LoadScene("Credits") works the same as any other scene load
- What you will NOT do (balance pass — that is Stage 10-K)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 10-J: Credits Sequence — Full Credits Scene

**Resuming from:** Stage 10-I complete — UI polish and settings menu done
**Done when:** Credits scene scrolls fully from title to final card; music plays; return button works; reachable from Main Menu and Victory Epilogue
**Commit:** `"10J: Credits scene — scrolling credits, title card, music, navigation wired"`
**Next session:** STAGE_10_K.md

---

## Part 1 — Credits Title Card Image

Generate a title card image for the top of the credits scroll.

**File:** `Credits_TitleCard.png` in `Assets/_Game/Art/Backgrounds/Generated/`

**Generation prompt (use `mcp__coplay-mcp__generate_or_edit_images`):**
> "Dark fantasy digital painting, 960×400. A lone lantern sitting on cracked stone in an empty dark space, warm amber glow, deep shadows, wisps of smoke or mist at the edges. Below the lantern, faintly visible: a small settlement in the extreme background. Quiet, earned, final. No text, no UI."

Import as: Sprite (2D and UI), Point filter, no compression.

---

## Part 2 — CreditsController

**Path:** `Assets/_Game/Scripts/Core.UI/CreditsController.cs`

```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using MnM.Core.Systems;

namespace MnM.Core.UI
{
    public class CreditsController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private Texture2D  _titleCardTexture;

        private VisualElement _scrollRoot;
        private bool          _scrollComplete;

        private void OnEnable()
        {
            var root = _uiDocument.rootVisualElement;

            // Background
            root.style.backgroundColor = new Color(0.03f, 0.02f, 0.01f, 1f);

            // Title card at top
            var titleCard = root.Q("credits-title-card");
            if (titleCard != null && _titleCardTexture != null)
                titleCard.style.backgroundImage = new StyleBackground(_titleCardTexture);

            // Return button (hidden until scroll finishes)
            var returnBtn = root.Q<Button>("credits-return-btn");
            if (returnBtn != null)
            {
                returnBtn.style.display = DisplayStyle.None;
                returnBtn.RegisterCallback<ClickEvent>(_ =>
                    SceneTransitionManager.Instance.LoadScene("MainMenu"));
            }

            _scrollRoot = root.Q("credits-scroll-content");

            // Set music context
            AudioManager.Instance.SetMusicContext(AudioContext.Credits);

            // Start scroll
            StartCoroutine(AutoScroll(returnBtn));
        }

        private IEnumerator AutoScroll(Button returnBtn)
        {
            // Wait a beat before starting
            yield return new WaitForSeconds(1.5f);

            var scrollView = _uiDocument.rootVisualElement.Q<ScrollView>("credits-scroll-view");
            if (scrollView == null) yield break;

            float scrollHeight     = scrollView.contentContainer.resolvedStyle.height;
            float viewportHeight   = scrollView.resolvedStyle.height;
            float totalScrollable  = Mathf.Max(0f, scrollHeight - viewportHeight);
            float scrollDuration   = Mathf.Max(30f, totalScrollable / 40f); // ~40 px/sec
            float t                = 0f;

            while (t < 1f)
            {
                // Allow player to skip with any key or click
                if (Input.anyKeyDown)
                {
                    scrollView.scrollOffset = new Vector2(0f, totalScrollable);
                    break;
                }

                t += Time.deltaTime / scrollDuration;
                scrollView.scrollOffset = new Vector2(0f, Mathf.Lerp(0f, totalScrollable, t));
                yield return null;
            }

            yield return new WaitForSeconds(1.2f);

            if (returnBtn != null)
            {
                returnBtn.style.display = DisplayStyle.Flex;
                returnBtn.style.opacity = 0f;
                float ft = 0f;
                while (ft < 1f)
                {
                    ft += Time.deltaTime / 0.5f;
                    returnBtn.style.opacity = Mathf.Lerp(0f, 1f, ft);
                    yield return null;
                }
            }

            _scrollComplete = true;
        }
    }
}
```

Add `AudioContext.Credits` to the `AudioContext` enum in `AudioManager.cs` if not present. Wire it to the main menu music (or a dedicated credits track — the settlement late music works well here).

---

## Part 3 — Credits UXML

**Path:** `Assets/_Game/Art/UI/UXML/Credits.uxml`

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
  <ui:VisualElement name="credits-root"
    style="width: 100%; height: 100%;
           background-color: rgb(8,6,4);
           align-items: center;">

    <!-- Title card image -->
    <ui:VisualElement name="credits-title-card"
      style="width: 960px; height: 400px;
             margin-top: 0px;
             background-size: cover;" />

    <!-- Scrollable credits content -->
    <ui:ScrollView name="credits-scroll-view"
      style="width: 700px; flex-grow: 1; overflow: hidden;"
      vertical-scroller-visibility="Hidden"
      horizontal-scroller-visibility="Hidden">

      <ui:VisualElement name="credits-scroll-content"
        style="align-items: center; padding-bottom: 120px;">

        <!-- Game title -->
        <ui:Label text="MARROW &amp; MYTH"
          style="color: rgb(184,133,10); font-size: 22px;
                 -unity-font-style: bold;
                 margin-top: 48px; margin-bottom: 8px;
                 -unity-text-align: middle-center;" />
        <ui:Label text="A 30-Year Dark Fantasy Campaign"
          style="color: rgb(110,105,95); font-size: 10px;
                 margin-bottom: 64px;
                 -unity-text-align: middle-center;" />

        <!-- Development -->
        <ui:Label text="DEVELOPMENT"
          style="color: rgb(184,133,10); font-size: 11px;
                 -unity-font-style: bold;
                 margin-bottom: 16px;
                 -unity-text-align: middle-center;" />
        <ui:Label text="Design, Code &amp; Direction"
          style="color: rgb(110,105,95); font-size: 9px;
                 margin-bottom: 4px; -unity-text-align: middle-center;" />
        <ui:Label text="Matt Duehr"
          style="color: rgb(212,204,186); font-size: 12px;
                 -unity-font-style: bold;
                 margin-bottom: 48px; -unity-text-align: middle-center;" />

        <!-- AI Assistance -->
        <ui:Label text="AI ASSISTANCE"
          style="color: rgb(184,133,10); font-size: 11px;
                 -unity-font-style: bold;
                 margin-bottom: 16px; -unity-text-align: middle-center;" />
        <ui:Label text="Implementation Partner"
          style="color: rgb(110,105,95); font-size: 9px;
                 margin-bottom: 4px; -unity-text-align: middle-center;" />
        <ui:Label text="Claude (Anthropic)"
          style="color: rgb(212,204,186); font-size: 12px;
                 margin-bottom: 8px; -unity-text-align: middle-center;" />
        <ui:Label text="Art Generation"
          style="color: rgb(110,105,95); font-size: 9px;
                 margin-bottom: 4px; -unity-text-align: middle-center;" />
        <ui:Label text="CoPlay MCP"
          style="color: rgb(212,204,186); font-size: 12px;
                 margin-bottom: 48px; -unity-text-align: middle-center;" />

        <!-- Engine & Tools -->
        <ui:Label text="BUILT WITH"
          style="color: rgb(184,133,10); font-size: 11px;
                 -unity-font-style: bold;
                 margin-bottom: 16px; -unity-text-align: middle-center;" />
        <ui:Label text="Unity 2022 LTS"
          style="color: rgb(212,204,186); font-size: 10px;
                 margin-bottom: 4px; -unity-text-align: middle-center;" />
        <ui:Label text="UI Toolkit"
          style="color: rgb(212,204,186); font-size: 10px;
                 margin-bottom: 4px; -unity-text-align: middle-center;" />
        <ui:Label text="Newtonsoft.Json"
          style="color: rgb(212,204,186); font-size: 10px;
                 margin-bottom: 48px; -unity-text-align: middle-center;" />

        <!-- Music & Audio -->
        <ui:Label text="MUSIC &amp; AUDIO"
          style="color: rgb(184,133,10); font-size: 11px;
                 -unity-font-style: bold;
                 margin-bottom: 16px; -unity-text-align: middle-center;" />
        <ui:Label text="All music and SFX generated via CoPlay MCP audio tools"
          style="color: rgb(110,105,95); font-size: 9px;
                 white-space: normal; -unity-text-align: middle-center;
                 max-width: 500px; margin-bottom: 48px;" />

        <!-- Inspirations -->
        <ui:Label text="INSPIRED BY"
          style="color: rgb(184,133,10); font-size: 11px;
                 -unity-font-style: bold;
                 margin-bottom: 16px; -unity-text-align: middle-center;" />
        <ui:Label text="Kingdom Death: Monster"
          style="color: rgb(212,204,186); font-size: 10px;
                 margin-bottom: 4px; -unity-text-align: middle-center;" />
        <ui:Label text="Darkest Dungeon"
          style="color: rgb(212,204,186); font-size: 10px;
                 margin-bottom: 4px; -unity-text-align: middle-center;" />
        <ui:Label text="Slay the Spire"
          style="color: rgb(212,204,186); font-size: 10px;
                 margin-bottom: 48px; -unity-text-align: middle-center;" />

        <!-- Closing message -->
        <ui:Label text="Thank you for surviving."
          style="color: rgb(184,133,10); font-size: 13px;
                 -unity-font-style: italic;
                 margin-bottom: 16px; -unity-text-align: middle-center;" />
        <ui:Label text="The settlement remembers."
          style="color: rgb(80,75,65); font-size: 9px;
                 margin-bottom: 80px; -unity-text-align: middle-center;" />

      </ui:VisualElement>
    </ui:ScrollView>

    <!-- Return button — hidden until scroll completes -->
    <ui:Button name="credits-return-btn" text="RETURN TO MAIN MENU"
      style="display: none;
             margin-bottom: 32px;
             padding-left: 24px; padding-right: 24px;
             color: rgb(180,170,150); font-size: 10px;" />

  </ui:VisualElement>
</ui:UXML>
```

---

## Part 4 — Credits Scene Setup

Open the Credits scene (already in Build Settings index 9). Set it up:

1. Create a `UIDocument` GameObject named "CreditsUI"
2. Assign `Credits.uxml` as the Source Asset
3. Add `CreditsController` component to the same GameObject
4. Assign the `_titleCardTexture` field in the Inspector to `Credits_TitleCard.png`
5. Assign `PanelSettings` (same one used throughout the project)

---

## Part 5 — Navigation Wiring

### From Victory Epilogue

In `VictoryEpilogueController`, the existing "CONTINUE" button currently goes to Main Menu. Change it to go to Credits instead:

```csharp
// In VictoryEpilogueController — wire the continue button:
root.Q<Button>("epilogue-continue-btn")?.RegisterCallback<ClickEvent>(_ =>
    SceneTransitionManager.Instance.LoadScene("Credits"));
```

### From Main Menu

In `MainMenuController`, add a CREDITS button wiring:

```csharp
root.Q<Button>("main-menu-credits-btn")?.RegisterCallback<ClickEvent>(_ =>
    SceneTransitionManager.Instance.LoadScene("Credits"));
```

Add the button to `MainMenu.uxml` if not already present:
```xml
<ui:Button name="main-menu-credits-btn" text="CREDITS"
  style="margin-top: 8px; align-self: center; font-size: 9px;
         color: rgb(80,75,65);" />
```

Place it below the QUIT button — visually de-emphasised.

---

## Verification Checklist

- [ ] Credits scene loads without errors from Main Menu → CREDITS button
- [ ] Credits scene loads from Victory Epilogue → CONTINUE button
- [ ] Title card image displays at top of credits (not a black square)
- [ ] Credits scroll begins automatically after 1.5 second delay
- [ ] Text scrolls slowly upward at ~40 px/sec
- [ ] Pressing any key skips to the bottom of the scroll
- [ ] "RETURN TO MAIN MENU" button fades in after scroll completes (or skip)
- [ ] Clicking RETURN TO MAIN MENU → fade transition to Main Menu
- [ ] Credits music context plays (not silent)
- [ ] All text is readable — no truncation, no overlap
- [ ] High-contrast mode (from Settings) correctly makes all credits text white
- [ ] Credits scene is indexed correctly in Build Settings (no scene load errors)

---

## Next Session

**File:** `_Docs/Stage_10/STAGE_10_K.md`
**Covers:** Balance pass — a dedicated session to tune the campaign economy using the debug cheat panel. Covers monster HP vs expected round counts, resource economy (too fast vs too slow craft unlock), encounter frequency, overlord year gates, and lifecycle card distribution. Documents recommended changes as permanent edits to MonsterSO and GameStateManager data.
