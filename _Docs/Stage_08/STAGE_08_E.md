<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 8-E | Settings Screen & In-Game Pause Menu
Status: Stage 8-D complete. All audio wired.
Task: Build a Settings scene (volume sliders, fullscreen
toggle, back button). Build a PauseMenuController that
overlays on top of any scene when Escape is pressed.
Settings changes must persist across sessions via PlayerPrefs.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_08/STAGE_08_E.md
- Assets/_Game/Scripts/Core.Systems/AudioManager.cs

Then confirm:
- Settings are saved with PlayerPrefs (Unity's built-in pref storage)
- Pause menu is an overlay — it does NOT load a new scene
- PauseMenuController is added to BOTH CombatScene and Settlement
- Pressing Escape toggles the pause overlay
- Resume button hides the overlay and resumes Time.timeScale
- What you will NOT touch (keybinding remapping — post-MVP)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 8-E: Settings Screen & In-Game Pause Menu

**Resuming from:** Stage 8-D complete — all audio clips wired
**Done when:** Settings scene saves volume preferences; pause overlay works in both combat and settlement; ESC toggles pause; Resume resumes game
**Commit:** `"8E: Settings scene and pause menu overlay — volume sliders, fullscreen, ESC pause"`
**Next session:** STAGE_08_F.md

---

## Part 1: Settings Scene

### Step 1: Create Scene
**File → New Scene → Empty** → save as `Assets/Settings.unity`
Add to Build Settings after CharacterCreation.

### Step 2: UXML — Settings.uxml
```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
  <ui:VisualElement name="root" style="width:100%; height:100%; flex-direction:column;
      align-items:center; justify-content:center; background-color:#0A0A0C;">

    <ui:Label text="SETTINGS" style="color:#D4CCBA; font-size:20px; margin-bottom:40px;" />

    <ui:VisualElement style="width:400px; flex-direction:column; gap:24px; margin-bottom:48px;">

      <ui:VisualElement class="setting-row">
        <ui:Label text="MASTER VOLUME" class="setting-label" />
        <ui:Slider name="slider-master" low-value="0" high-value="1" value="1"
                   style="flex-grow:1;" />
        <ui:Label name="val-master" text="100" class="setting-value" />
      </ui:VisualElement>

      <ui:VisualElement class="setting-row">
        <ui:Label text="MUSIC" class="setting-label" />
        <ui:Slider name="slider-music" low-value="0" high-value="1" value="0.8"
                   style="flex-grow:1;" />
        <ui:Label name="val-music" text="80" class="setting-value" />
      </ui:VisualElement>

      <ui:VisualElement class="setting-row">
        <ui:Label text="SFX" class="setting-label" />
        <ui:Slider name="slider-sfx" low-value="0" high-value="1" value="1"
                   style="flex-grow:1;" />
        <ui:Label name="val-sfx" text="100" class="setting-value" />
      </ui:VisualElement>

      <ui:VisualElement class="setting-row">
        <ui:Label text="FULLSCREEN" class="setting-label" />
        <ui:Toggle name="toggle-fullscreen" label="" />
      </ui:VisualElement>

    </ui:VisualElement>

    <ui:Button name="btn-back" text="← BACK" class="mnm-btn-secondary" />
  </ui:VisualElement>
</ui:UXML>
```

Add to CSS (reuse or extend MainMenu.uss, or create Settings.uss):
```css
.setting-row { flex-direction:row; align-items:center; gap:16px; }
.setting-label { color:rgb(138,138,138); font-size:11px; width:140px; }
.setting-value { color:rgb(212,204,186); font-size:11px; width:32px; -unity-text-align:middle-right; }
```

### Step 3: SettingsController.cs

**Path:** `Assets/_Game/Scripts/Core.UI/SettingsController.cs`

```csharp
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace MnM.Core.UI
{
    public class SettingsController : MonoBehaviour
    {
        [SerializeField] private UIDocument  _uiDocument;
        [SerializeField] private AudioMixer  _masterMixer;

        private const string KeyMaster     = "vol_master";
        private const string KeyMusic      = "vol_music";
        private const string KeySfx        = "vol_sfx";
        private const string KeyFullscreen = "fullscreen";

        private void OnEnable()
        {
            var root = _uiDocument.rootVisualElement;

            // Load saved prefs
            float master = PlayerPrefs.GetFloat(KeyMaster, 1f);
            float music  = PlayerPrefs.GetFloat(KeyMusic,  0.8f);
            float sfx    = PlayerPrefs.GetFloat(KeySfx,    1f);
            bool  fs     = PlayerPrefs.GetInt(KeyFullscreen, 1) == 1;

            SetupSlider(root, "slider-master", "val-master", master, v =>
            {
                PlayerPrefs.SetFloat(KeyMaster, v);
                _masterMixer?.SetFloat("MasterVol", Mathf.Log10(Mathf.Max(v, 0.001f)) * 20);
            });

            SetupSlider(root, "slider-music", "val-music", music, v =>
            {
                PlayerPrefs.SetFloat(KeyMusic, v);
                _masterMixer?.SetFloat("MusicVol", Mathf.Log10(Mathf.Max(v, 0.001f)) * 20);
            });

            SetupSlider(root, "slider-sfx", "val-sfx", sfx, v =>
            {
                PlayerPrefs.SetFloat(KeySfx, v);
                _masterMixer?.SetFloat("SFXVol", Mathf.Log10(Mathf.Max(v, 0.001f)) * 20);
            });

            var fsToggle = root.Q<Toggle>("toggle-fullscreen");
            fsToggle.value = fs;
            Screen.fullScreen = fs;
            fsToggle.RegisterValueChangedCallback(evt =>
            {
                Screen.fullScreen = evt.newValue;
                PlayerPrefs.SetInt(KeyFullscreen, evt.newValue ? 1 : 0);
            });

            // Back — return to whatever loaded us (check if from main menu or in-game)
            root.Q<Button>("btn-back").RegisterCallback<ClickEvent>(_ =>
                SceneManager.LoadScene("MainMenu"));

            // Apply saved audio settings immediately
            ApplyAudioPrefs(master, music, sfx);
        }

        private void SetupSlider(VisualElement root, string sliderId, string labelId,
                                  float initial, System.Action<float> onChange)
        {
            var slider = root.Q<Slider>(sliderId);
            var label  = root.Q<Label>(labelId);
            slider.value = initial;
            label.text   = Mathf.RoundToInt(initial * 100).ToString();
            slider.RegisterValueChangedCallback(evt =>
            {
                label.text = Mathf.RoundToInt(evt.newValue * 100).ToString();
                onChange(evt.newValue);
                PlayerPrefs.Save();
            });
        }

        private void ApplyAudioPrefs(float master, float music, float sfx)
        {
            if (_masterMixer == null) return;
            _masterMixer.SetFloat("MasterVol", Mathf.Log10(Mathf.Max(master, 0.001f)) * 20);
            _masterMixer.SetFloat("MusicVol",  Mathf.Log10(Mathf.Max(music,  0.001f)) * 20);
            _masterMixer.SetFloat("SFXVol",    Mathf.Log10(Mathf.Max(sfx,   0.001f)) * 20);
        }
    }
}
```

---

## Part 2: In-Game Pause Menu

The pause menu is an **overlay panel** — it sits on top of the game but doesn't change the scene.

### Step 1: Add Pause UXML Overlay

In `CombatScene.unity` and `Settlement.unity`, add a second UIDocument GameObject named `PauseMenuOverlay`.
Create `Assets/_Game/UI/PauseMenu.uxml`:

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
  <ui:VisualElement name="pause-bg" style="position:absolute; left:0; top:0; right:0; bottom:0;
      background-color:rgba(0,0,0,0.7); display:none; align-items:center; justify-content:center;">

    <ui:VisualElement style="width:280px; background-color:#0A0A0C; border-color:#B8860B;
        border-width:2px; padding:32px; flex-direction:column; align-items:center; gap:16px;">
      <ui:Label text="PAUSED" style="color:#D4CCBA; font-size:20px; margin-bottom:16px;" />
      <ui:Button name="btn-resume"   text="RESUME"         class="mnm-btn" style="width:200px;" />
      <ui:Button name="btn-settings" text="SETTINGS"       class="mnm-btn" style="width:200px;" />
      <ui:Button name="btn-save"     text="SAVE GAME"      class="mnm-btn" style="width:200px;" />
      <ui:Button name="btn-menu"     text="MAIN MENU"      class="mnm-btn" style="width:200px;" />
    </ui:VisualElement>

  </ui:VisualElement>
</ui:UXML>
```

### Step 2: PauseMenuController.cs

**Path:** `Assets/_Game/Scripts/Core.UI/PauseMenuController.cs`

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using MnM.Core.Systems;

namespace MnM.Core.UI
{
    public class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;

        private VisualElement _pauseBg;
        private bool          _isPaused = false;

        private void OnEnable()
        {
            var root  = _uiDocument.rootVisualElement;
            _pauseBg  = root.Q("pause-bg");

            root.Q<Button>("btn-resume")  .RegisterCallback<ClickEvent>(_ => SetPause(false));
            root.Q<Button>("btn-settings").RegisterCallback<ClickEvent>(_ => OpenSettings());
            root.Q<Button>("btn-save")    .RegisterCallback<ClickEvent>(_ => SaveAndClose());
            root.Q<Button>("btn-menu")    .RegisterCallback<ClickEvent>(_ => ReturnToMenu());
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                SetPause(!_isPaused);
        }

        public void SetPause(bool paused)
        {
            _isPaused             = paused;
            Time.timeScale        = paused ? 0f : 1f;
            _pauseBg.style.display = paused ? DisplayStyle.Flex : DisplayStyle.None;
            Debug.Log($"[Pause] {(paused ? "Paused" : "Resumed")}");
        }

        private void OpenSettings()
        {
            // TODO Stage 8-E: push settings panel as overlay or load scene
            Debug.Log("[Pause] Settings — not yet implemented as overlay");
        }

        private void SaveAndClose()
        {
            SaveSystem.SaveCurrent();
            SetPause(false);
            Debug.Log("[Pause] Game saved");
        }

        private void ReturnToMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }
    }
}
```

Add `PauseMenuController` to a GameObject in both `CombatScene.unity` and `Settlement.unity`.

---

## Verification Test

**Settings:**
- [ ] Settings scene opens from MainMenu → SETTINGS button
- [ ] Moving Master Volume slider → AudioMixer master volume changes
- [ ] Moving Music slider → music volume changes
- [ ] Closing Settings and reopening → sliders remember last position
- [ ] Fullscreen toggle → screen changes to/from fullscreen

**Pause Menu:**
- [ ] Press ESC in Settlement → pause overlay appears, game halts
- [ ] Press ESC again → overlay disappears, game resumes
- [ ] RESUME button dismisses pause
- [ ] SAVE GAME calls SaveSystem.SaveCurrent() and closes pause
- [ ] MAIN MENU returns to MainMenu scene
- [ ] Time.timeScale returns to 1 after unpausing

---

## Next Session

**File:** `_Docs/Stage_08/STAGE_08_F.md`
**Covers:** Status effect visual icons — generate sprites for all 8 status effects (Shaken, Pinned, Slowed, Exposed, Bleeding, Marked, Inspired, Broken), StatusEffectDisplay component to show them on combat tokens, and wiring to CombatManager events
