<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 10-I | UI Polish & Accessibility — Transitions, Hover States, Settings Menu
Status: Stage 10-H complete. All audio wired.
Task: Polish the UI so it feels finished:
  1. Settlement tab transitions — fade/slide between tabs instead of instant swap
  2. Button hover and press visual states across all scenes
  3. Settings menu — fully functional: master/music/SFX volume, fullscreen toggle,
     input remapping stub (display only), font size option
  4. Accessibility options — high-contrast mode toggle, larger font size option
  Both settings and accessibility choices must persist via PlayerPrefs.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_10/STAGE_10_I.md
- Assets/_Game/Scripts/Core.UI/SettlementScreenController.cs
- Assets/_Game/Scripts/Core.Systems/AudioManager.cs
- Assets/_Game/Scripts/Core.UI/MainMenuController.cs
- Assets/_Game/Scripts/Core.UI/CampaignSelectController.cs

Then confirm:
- All UI is UIToolkit (no Canvas/uGUI) — confirmed throughout
- AudioManager already has PlayerPrefs volume persistence (Stage 8-A)
- Tab switching in Settlement is an instant display swap in SettlementScreenController
- There is currently NO settings scene or settings VisualElement in any scene
- What you will NOT do (credits sequence — that is Stage 10-J)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 10-I: UI Polish & Accessibility — Transitions, Hover States, Settings Menu

**Resuming from:** Stage 10-H complete — all audio wired
**Done when:** Settlement tabs fade between each other; all buttons have hover/press states; settings menu is fully functional; accessibility options apply and persist
**Commit:** `"10I: UI polish — tab transitions, hover states, settings menu, accessibility options"`
**Next session:** STAGE_10_J.md

---

## Part 1 — Settlement Tab Transitions

Currently `SettlementScreenController.ShowTab(string tabName)` instantly hides the old panel and shows the new one. Replace with a cross-fade.

### 1a — Tab Fade Coroutine

```csharp
// In SettlementScreenController.cs

private VisualElement _currentTabPanel;
private Coroutine     _tabTransition;

public void SwitchToTab(string tabName)
{
    var nextPanel = _uiDocument.rootVisualElement.Q(tabName + "-panel");
    if (nextPanel == null || nextPanel == _currentTabPanel) return;

    if (_tabTransition != null) StopCoroutine(_tabTransition);
    _tabTransition = StartCoroutine(CrossFadeTabs(_currentTabPanel, nextPanel));
}

private IEnumerator CrossFadeTabs(VisualElement outPanel, VisualElement inPanel)
{
    // Fade out old panel
    if (outPanel != null)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.12f;
            outPanel.style.opacity = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }
        outPanel.style.display = DisplayStyle.None;
        outPanel.style.opacity = 1f;
    }

    // Show and fade in new panel
    inPanel.style.opacity = 0f;
    inPanel.style.display = DisplayStyle.Flex;

    float t2 = 0f;
    while (t2 < 1f)
    {
        t2 += Time.deltaTime / 0.14f;
        inPanel.style.opacity = Mathf.Lerp(0f, 1f, t2);
        yield return null;
    }
    inPanel.style.opacity = 1f;
    _currentTabPanel = inPanel;
    _tabTransition   = null;
}
```

Replace all `ShowTab()` calls with `SwitchToTab()`. On initial load, set `_currentTabPanel` to the default tab panel (Hunters) and display it immediately (no fade on first show).

### 1b — Active Tab Button Visual State

When a tab is selected, its button should look visually distinct. Add a USS class:

In `SettlementScreen.uss`:
```css
.tab-button {
    background-color: rgba(20, 16, 10, 0.0);
    border-bottom-width: 1px;
    border-bottom-color: rgba(100, 85, 60, 0.3);
    color: rgb(110, 105, 95);
    transition-property: background-color, color, border-bottom-color;
    transition-duration: 0.1s;
}

.tab-button:hover {
    background-color: rgba(255, 200, 80, 0.08);
    color: rgb(180, 170, 150);
    border-bottom-color: rgba(180, 160, 80, 0.5);
}

.tab-button.active {
    background-color: rgba(255, 200, 80, 0.12);
    color: rgb(212, 204, 186);
    border-bottom-color: rgb(184, 133, 10);
    border-bottom-width: 2px;
}
```

In `SettlementScreenController.SwitchToTab()`, after the fade:
```csharp
// Remove active class from all tab buttons
_uiDocument.rootVisualElement.Query<Button>(className: "tab-button")
    .ForEach(b => b.RemoveFromClassList("active"));

// Add active class to the clicked button
var activeBtn = _uiDocument.rootVisualElement.Q<Button>(tabName + "-tab-btn");
activeBtn?.AddToClassList("active");
```

---

## Part 2 — Button Hover & Press Visual States (Global)

Create a global USS stylesheet that adds hover and active states to every button in the game.

**Path:** `Assets/_Game/Art/UI/USS/GlobalButtons.uss`

```css
/* Applied to all Button elements game-wide */

Button {
    background-color: rgba(20, 16, 10, 0.0);
    border-top-width: 1px;
    border-bottom-width: 1px;
    border-left-width: 1px;
    border-right-width: 1px;
    border-top-color: rgba(100, 85, 60, 0.4);
    border-bottom-color: rgba(100, 85, 60, 0.4);
    border-left-color: rgba(100, 85, 60, 0.4);
    border-right-color: rgba(100, 85, 60, 0.4);
    color: rgb(180, 170, 150);
    padding-top: 6px;
    padding-bottom: 6px;
    padding-left: 12px;
    padding-right: 12px;
    transition-property: background-color, border-top-color, color;
    transition-duration: 0.08s;
}

Button:hover {
    background-color: rgba(255, 200, 80, 0.10);
    border-top-color: rgba(184, 133, 10, 0.7);
    border-bottom-color: rgba(184, 133, 10, 0.7);
    border-left-color: rgba(184, 133, 10, 0.7);
    border-right-color: rgba(184, 133, 10, 0.7);
    color: rgb(212, 204, 186);
}

Button:active {
    background-color: rgba(255, 200, 80, 0.20);
    border-top-color: rgb(184, 133, 10);
    border-bottom-color: rgb(184, 133, 10);
    border-left-color: rgb(184, 133, 10);
    border-right-color: rgb(184, 133, 10);
    color: rgb(230, 225, 210);
}

Button:disabled {
    background-color: rgba(20, 16, 10, 0.0);
    border-top-color: rgba(60, 55, 50, 0.3);
    border-bottom-color: rgba(60, 55, 50, 0.3);
    border-left-color: rgba(60, 55, 50, 0.3);
    border-right-color: rgba(60, 55, 50, 0.3);
    color: rgb(80, 78, 72);
    opacity: 0.5;
}
```

Apply `GlobalButtons.uss` to every PanelSettings asset in the project (there should be one from Stage 8-A). In Unity: select the PanelSettings asset → Inspector → Style Sheets → add `GlobalButtons.uss` to the list.

UIToolkit CSS transitions are supported from Unity 2022.2+. If transitions don't work in your Unity version, implement hover with `MouseEnterEvent` / `MouseLeaveEvent` callbacks in a utility class:

```csharp
// Assets/_Game/Scripts/Core.UI/ButtonHoverEffect.cs
using UnityEngine;
using UnityEngine.UIElements;

namespace MnM.Core.UI
{
    public static class ButtonHoverEffect
    {
        private static readonly Color _hoverBg    = new Color(1f, 0.78f, 0.31f, 0.10f);
        private static readonly Color _normalBg   = new Color(0f, 0f, 0f, 0f);
        private static readonly Color _hoverBorder = new Color(0.72f, 0.52f, 0.04f, 0.7f);
        private static readonly Color _normalBorder = new Color(0.39f, 0.33f, 0.24f, 0.4f);

        public static void Apply(VisualElement root)
        {
            root.Query<Button>().ForEach(btn =>
            {
                btn.RegisterCallback<MouseEnterEvent>(_ =>
                {
                    btn.style.backgroundColor = _hoverBg;
                    btn.style.borderTopColor = btn.style.borderBottomColor =
                    btn.style.borderLeftColor = btn.style.borderRightColor = _hoverBorder;
                });
                btn.RegisterCallback<MouseLeaveEvent>(_ =>
                {
                    btn.style.backgroundColor = _normalBg;
                    btn.style.borderTopColor = btn.style.borderBottomColor =
                    btn.style.borderLeftColor = btn.style.borderRightColor = _normalBorder;
                });
            });
        }
    }
}
```

Call `ButtonHoverEffect.Apply(_uiDocument.rootVisualElement)` at the end of every controller's `OnEnable()`.

---

## Part 3 — Settings Menu

The settings menu is a modal overlay accessible from the Main Menu and from a gear icon in the Settlement and Combat scenes. It does NOT load a new scene — it overlays the current one.

### 3a — SettingsMenuController

**Path:** `Assets/_Game/Scripts/Core.UI/SettingsMenuController.cs`

```csharp
using UnityEngine;
using UnityEngine.UIElements;

namespace MnM.Core.UI
{
    public class SettingsMenuController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;

        private VisualElement _settingsPanel;

        private const string KEY_MASTER   = "vol_master";
        private const string KEY_MUSIC    = "vol_music";
        private const string KEY_SFX      = "vol_sfx";
        private const string KEY_FULLSCREEN = "fullscreen";
        private const string KEY_FONT_SIZE  = "font_size";
        private const string KEY_HIGH_CONTRAST = "high_contrast";

        private void OnEnable()
        {
            var root = _uiDocument.rootVisualElement;
            _settingsPanel = root.Q("settings-panel");
            if (_settingsPanel == null) return;

            _settingsPanel.style.display = DisplayStyle.None;

            // Wire open button (gear icon in various scenes)
            root.Q<Button>("settings-open-btn")?.RegisterCallback<ClickEvent>(_ => OpenSettings());
            root.Q<Button>("settings-close-btn")?.RegisterCallback<ClickEvent>(_ => CloseSettings());

            // Volume sliders
            WireSlider("slider-master", KEY_MASTER,   1f,  v => AudioManager.Instance.SetMasterVolume(v));
            WireSlider("slider-music",  KEY_MUSIC,    0.8f, v => AudioManager.Instance.SetMusicVolume(v));
            WireSlider("slider-sfx",    KEY_SFX,      1f,  v => AudioManager.Instance.SetSFXVolume(v));

            // Fullscreen toggle
            var fsToggle = root.Q<Toggle>("toggle-fullscreen");
            if (fsToggle != null)
            {
                fsToggle.value = PlayerPrefs.GetInt(KEY_FULLSCREEN, 1) == 1;
                fsToggle.RegisterValueChangedCallback(evt =>
                {
                    Screen.fullScreen = evt.newValue;
                    PlayerPrefs.SetInt(KEY_FULLSCREEN, evt.newValue ? 1 : 0);
                    PlayerPrefs.Save();
                });
            }

            // Font size toggle (Normal / Large)
            var fontToggle = root.Q<Toggle>("toggle-large-font");
            if (fontToggle != null)
            {
                fontToggle.value = PlayerPrefs.GetInt(KEY_FONT_SIZE, 0) == 1;
                fontToggle.RegisterValueChangedCallback(evt =>
                {
                    PlayerPrefs.SetInt(KEY_FONT_SIZE, evt.newValue ? 1 : 0);
                    PlayerPrefs.Save();
                    AccessibilityManager.Instance.SetLargeFont(evt.newValue);
                });
            }

            // High contrast toggle
            var hcToggle = root.Q<Toggle>("toggle-high-contrast");
            if (hcToggle != null)
            {
                hcToggle.value = PlayerPrefs.GetInt(KEY_HIGH_CONTRAST, 0) == 1;
                hcToggle.RegisterValueChangedCallback(evt =>
                {
                    PlayerPrefs.SetInt(KEY_HIGH_CONTRAST, evt.newValue ? 1 : 0);
                    PlayerPrefs.Save();
                    AccessibilityManager.Instance.SetHighContrast(evt.newValue);
                });
            }

            // Input remapping — display-only stub
            root.Q<Button>("btn-remap-keys")?.RegisterCallback<ClickEvent>(_ =>
                root.Q<Label>("remap-notice")?.style.SetProperty(
                    "display", DisplayStyle.Flex));
        }

        private void WireSlider(string elementName, string key, float defaultVal,
                                  System.Action<float> onChange)
        {
            var slider = _uiDocument.rootVisualElement.Q<Slider>(elementName);
            if (slider == null) return;
            slider.lowValue   = 0f;
            slider.highValue  = 1f;
            slider.value      = PlayerPrefs.GetFloat(key, defaultVal);
            slider.RegisterValueChangedCallback(evt =>
            {
                onChange(evt.newValue);
                PlayerPrefs.SetFloat(key, evt.newValue);
                PlayerPrefs.Save();
            });
        }

        public void OpenSettings()
        {
            _settingsPanel.style.display = DisplayStyle.Flex;
            _settingsPanel.style.opacity = 0f;
            var t = 0f;
            _settingsPanel.schedule.Execute(() =>
            {
                t += Time.deltaTime / 0.15f;
                _settingsPanel.style.opacity = Mathf.Min(t, 1f);
            }).Until(() => t >= 1f);
        }

        public void CloseSettings()
        {
            _settingsPanel.style.display = DisplayStyle.None;
        }
    }
}
```

### 3b — Settings Panel UXML

**Path:** `Assets/_Game/Art/UI/UXML/SettingsPanel.uxml`

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements" xmlns:uie="UnityEditor.UIElements">
  <ui:VisualElement name="settings-panel"
    style="position: absolute; left: 50%; top: 50%;
           translate: -50% -50%;
           width: 480px; min-height: 520px;
           background-color: rgba(8,6,4,0.97);
           border-color: rgba(184,133,10,1);
           border-width: 1px;
           padding: 32px;">

    <ui:Label text="SETTINGS"
      style="color: rgb(184,133,10); font-size: 14px;
             -unity-font-style: bold; margin-bottom: 24px;" />

    <!-- AUDIO -->
    <ui:Label text="AUDIO" style="color: rgb(110,105,95); font-size: 9px; margin-bottom: 8px;" />

    <ui:VisualElement style="flex-direction: row; align-items: center; margin-bottom: 10px;">
      <ui:Label text="MASTER" style="color: rgb(180,170,150); font-size: 9px; width: 100px;" />
      <ui:Slider name="slider-master" low-value="0" high-value="1" style="flex-grow: 1;" />
    </ui:VisualElement>

    <ui:VisualElement style="flex-direction: row; align-items: center; margin-bottom: 10px;">
      <ui:Label text="MUSIC" style="color: rgb(180,170,150); font-size: 9px; width: 100px;" />
      <ui:Slider name="slider-music" low-value="0" high-value="1" style="flex-grow: 1;" />
    </ui:VisualElement>

    <ui:VisualElement style="flex-direction: row; align-items: center; margin-bottom: 24px;">
      <ui:Label text="SFX" style="color: rgb(180,170,150); font-size: 9px; width: 100px;" />
      <ui:Slider name="slider-sfx" low-value="0" high-value="1" style="flex-grow: 1;" />
    </ui:VisualElement>

    <!-- DISPLAY -->
    <ui:Label text="DISPLAY" style="color: rgb(110,105,95); font-size: 9px; margin-bottom: 8px;" />

    <ui:VisualElement style="flex-direction: row; align-items: center; margin-bottom: 24px;">
      <ui:Label text="FULLSCREEN" style="color: rgb(180,170,150); font-size: 9px; flex-grow: 1;" />
      <ui:Toggle name="toggle-fullscreen" />
    </ui:VisualElement>

    <!-- ACCESSIBILITY -->
    <ui:Label text="ACCESSIBILITY" style="color: rgb(110,105,95); font-size: 9px; margin-bottom: 8px;" />

    <ui:VisualElement style="flex-direction: row; align-items: center; margin-bottom: 10px;">
      <ui:Label text="LARGE TEXT" style="color: rgb(180,170,150); font-size: 9px; flex-grow: 1;" />
      <ui:Toggle name="toggle-large-font" />
    </ui:VisualElement>

    <ui:VisualElement style="flex-direction: row; align-items: center; margin-bottom: 24px;">
      <ui:Label text="HIGH CONTRAST" style="color: rgb(180,170,150); font-size: 9px; flex-grow: 1;" />
      <ui:Toggle name="toggle-high-contrast" />
    </ui:VisualElement>

    <!-- INPUT REMAPPING (stub) -->
    <ui:Label text="INPUT" style="color: rgb(110,105,95); font-size: 9px; margin-bottom: 8px;" />
    <ui:Button name="btn-remap-keys" text="REMAP KEYS"
      style="margin-bottom: 4px;" />
    <ui:Label name="remap-notice"
      text="Key remapping will be available in a future update."
      style="display: none; color: rgb(110,105,95); font-size: 8px; margin-bottom: 24px;" />

    <ui:Button name="settings-close-btn" text="CLOSE"
      style="margin-top: 8px; align-self: flex-end;" />
  </ui:VisualElement>
</ui:UXML>
```

Add this UXML as an `AdditionalTemplates` reference in the BootstrapManager or instantiate it in each scene that needs the settings icon.

### 3c — Settings Gear Icon

Add a small gear button to the top-right corner of: Main Menu, Settlement, and Combat HUD. In each scene's UXML, add:

```xml
<ui:Button name="settings-open-btn" text="⚙"
  style="position: absolute; top: 8px; right: 8px;
         width: 32px; height: 32px; font-size: 14px;
         background-color: rgba(20,16,10,0.5);
         border-color: rgba(100,85,60,0.4); border-width: 1px;" />
```

Attach `SettingsMenuController` as a component on the UIDocument GameObject in each scene (or make it a DontDestroyOnLoad persistent manager — whichever matches your architecture). The simplest approach: add it as a component alongside each scene's UIDocument, pointing to the same UIDocument.

---

## Part 4 — AccessibilityManager

**Path:** `Assets/_Game/Scripts/Core.Systems/AccessibilityManager.cs`

```csharp
using UnityEngine;
using UnityEngine.UIElements;

namespace MnM.Core.Systems
{
    public class AccessibilityManager : MonoBehaviour
    {
        public static AccessibilityManager Instance { get; private set; }

        private const string KEY_FONT_SIZE     = "font_size";
        private const string KEY_HIGH_CONTRAST = "high_contrast";

        private const float NORMAL_FONT_SCALE = 1.0f;
        private const float LARGE_FONT_SCALE  = 1.4f;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // Apply saved preferences on boot
            SetLargeFont(PlayerPrefs.GetInt(KEY_FONT_SIZE, 0) == 1);
            SetHighContrast(PlayerPrefs.GetInt(KEY_HIGH_CONTRAST, 0) == 1);
        }

        public void SetLargeFont(bool large)
        {
            // UIToolkit: set DynamicAtlas scale or root VisualElement font-size multiplier
            // The simplest implementation: scale the entire UI panel
            var roots = FindObjectsOfType<UIDocument>();
            float scale = large ? LARGE_FONT_SCALE : NORMAL_FONT_SCALE;
            foreach (var doc in roots)
            {
                var root = doc.rootVisualElement;
                if (root != null)
                    root.style.fontSize = new StyleLength(
                        new Length(large ? 11 : 9, LengthUnit.Pixel));
            }
            Debug.Log($"[Accessibility] Large font: {large}");
        }

        public void SetHighContrast(bool enabled)
        {
            // High contrast: add/remove a USS class on all roots that boosts contrast
            var roots = FindObjectsOfType<UIDocument>();
            foreach (var doc in roots)
            {
                var root = doc.rootVisualElement;
                if (root == null) continue;
                if (enabled)
                    root.AddToClassList("high-contrast");
                else
                    root.RemoveFromClassList("high-contrast");
            }
            Debug.Log($"[Accessibility] High contrast: {enabled}");
        }
    }
}
```

Add to `GlobalButtons.uss`:

```css
.high-contrast Button {
    color: rgb(255, 255, 255);
    border-top-color: rgb(255, 255, 255);
    border-bottom-color: rgb(255, 255, 255);
    border-left-color: rgb(255, 255, 255);
    border-right-color: rgb(255, 255, 255);
}

.high-contrast Label {
    color: rgb(255, 255, 255);
}

.high-contrast .tab-button.active {
    background-color: rgba(255, 255, 100, 0.3);
    color: rgb(255, 255, 255);
}
```

Add `AccessibilityManager` to the Bootstrap scene's Managers GameObject (DontDestroyOnLoad alongside GameStateManager, AudioManager, etc.).

### 3d — AudioManager Volume Methods

If `SetMasterVolume`, `SetMusicVolume`, `SetSFXVolume` don't already exist on `AudioManager`, add:

```csharp
public void SetMasterVolume(float v)
{
    AudioListener.volume = v;
    PlayerPrefs.SetFloat("vol_master", v);
}

public void SetMusicVolume(float v)
{
    _musicSource.volume = v;
    PlayerPrefs.SetFloat("vol_music", v);
}

public void SetSFXVolume(float v)
{
    _sfxSource.volume = v;
    PlayerPrefs.SetFloat("vol_sfx", v);
}
```

---

## Verification Checklist

- [ ] Switching tabs in Settlement → 0.12s fade-out / 0.14s fade-in plays (not instant swap)
- [ ] Active tab button shows gold bottom border and brighter text
- [ ] All buttons in Main Menu have hover state (background brightens on mouse-over)
- [ ] All buttons in Settlement have hover state
- [ ] Button press (click-down) shows a further brightened state
- [ ] Disabled buttons are visually muted (grey, low opacity)
- [ ] Settings panel opens from gear icon in Main Menu → panel fades in
- [ ] Settings panel opens from gear icon in Settlement → same panel over settlement
- [ ] Master volume slider → AudioListener.volume changes immediately
- [ ] Music slider → background music volume changes immediately
- [ ] SFX slider → play a card → card SFX at new volume
- [ ] Fullscreen toggle → Screen.fullScreen toggles
- [ ] Large Text toggle → all Label font sizes increase noticeably
- [ ] High Contrast toggle → all text becomes white, borders become white
- [ ] All settings persist after quit and reload (PlayerPrefs)
- [ ] Remap Keys button → notice label appears below it
- [ ] AccessibilityManager is DontDestroyOnLoad; settings apply to newly loaded scenes

---

## Next Session

**File:** `_Docs/Stage_10/STAGE_10_J.md`
**Covers:** Credits sequence — building the full credits scene with scrolling text (team, tools, music, inspiration), a final image or animation, a "Return to Main Menu" button, and wiring the credits button from the Main Menu to the scene
