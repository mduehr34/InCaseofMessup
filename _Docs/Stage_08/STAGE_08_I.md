<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 8-I | Screen Transition Animations
Status: Stage 8-H complete. Card rendering working.
Task: Build SceneTransitionManager — a singleton that plays
a fade-to-black animation between all scene loads. Replace
all bare SceneManager.LoadScene() calls with
SceneTransitionManager.LoadScene(). Also add slide-in
animations for modal panels (event modals, result modals).

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_08/STAGE_08_I.md

Then confirm:
- SceneTransitionManager is a DontDestroyOnLoad singleton
- Fade out takes 0.3s, scene loads, fade in takes 0.3s
- The black overlay is a UIDocument overlay on top of everything
- Modal panels slide in from off-screen bottom over 0.25s
- All existing SceneManager.LoadScene() calls will be replaced
- What you will NOT change (SceneTransitionManager does not
  handle loading screens — scenes load fast enough currently)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 8-I: Screen Transition Animations

**Resuming from:** Stage 8-H complete — card rendering working
**Done when:** All scene changes fade to black and back; modal panels slide in from bottom; no jarring instant cuts between scenes
**Commit:** `"8I: SceneTransitionManager — fade-to-black transitions and modal slide-in animations"`
**Next session:** STAGE_08_J.md

---

## Part 1: SceneTransitionManager

### Step 1: Create Persistent Scene

Create a small "bootstrap" scene that holds managers that must persist across scenes:

1. **File → New Scene → Empty** → save as `Assets/Bootstrap.unity`
2. Move `AudioManager` from Settlement to this scene
3. Make Bootstrap scene **index 0** in Build Settings — push MainMenu to index 1
4. Bootstrap will load MainMenu automatically on startup

In Bootstrap scene, create a GameObject named `SceneTransitionManager`.

### Step 2: Transition Overlay UXML

Create `Assets/_Game/UI/SceneTransition.uxml`:
```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
  <ui:VisualElement name="fade-overlay" style="position:absolute; left:0; top:0;
      right:0; bottom:0; background-color:rgba(0,0,0,0);
      display:none; pointer-events:none;" />
</ui:UXML>
```

### Step 3: SceneTransitionManager.cs

**Path:** `Assets/_Game/Scripts/Core.Systems/SceneTransitionManager.cs`

```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace MnM.Core.Systems
{
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        [SerializeField] private UIDocument _uiDocument;

        private VisualElement _overlay;
        private bool          _isTransitioning = false;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            _overlay = _uiDocument.rootVisualElement.Q("fade-overlay");
            // Start with a fade-in (game just launched)
            StartCoroutine(FadeIn(0.4f));
        }

        // ── Public API ──────────────────────────────────────────────────

        /// <summary>Use this everywhere instead of SceneManager.LoadScene()</summary>
        public void LoadScene(string sceneName)
        {
            if (_isTransitioning) return;
            StartCoroutine(TransitionTo(sceneName));
        }

        /// <summary>Slide a modal panel into view from the bottom.</summary>
        public IEnumerator SlideIn(VisualElement panel, float duration = 0.25f)
        {
            panel.style.display = DisplayStyle.Flex;
            float startY = Screen.height;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float y = Mathf.Lerp(startY, 0f, t / duration);
                panel.style.bottom = -y;
                yield return null;
            }
            panel.style.bottom = 0;
        }

        /// <summary>Slide a modal panel back off-screen.</summary>
        public IEnumerator SlideOut(VisualElement panel, float duration = 0.2f)
        {
            float t = 0f;
            float startBottom = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float y = Mathf.Lerp(0f, Screen.height, t / duration);
                panel.style.bottom = -y;
                yield return null;
            }
            panel.style.display = DisplayStyle.None;
        }

        // ── Private ──────────────────────────────────────────────────────

        private IEnumerator TransitionTo(string sceneName)
        {
            _isTransitioning = true;
            yield return FadeOut(0.3f);
            SceneManager.LoadScene(sceneName);
            // One frame for the scene to initialise
            yield return null;
            yield return FadeIn(0.3f);
            _isTransitioning = false;
        }

        private IEnumerator FadeOut(float duration)
        {
            if (_overlay == null) yield break;
            _overlay.style.display = DisplayStyle.Flex;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(0f, 1f, t / duration);
                _overlay.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, alpha));
                yield return null;
            }
            _overlay.style.backgroundColor = new StyleColor(Color.black);
        }

        private IEnumerator FadeIn(float duration)
        {
            if (_overlay == null) yield break;
            _overlay.style.display = DisplayStyle.Flex;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(1f, 0f, t / duration);
                _overlay.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, alpha));
                yield return null;
            }
            _overlay.style.display = DisplayStyle.None;
        }
    }
}
```

### Step 4: Bootstrap Scene Setup

Create `Assets/_Game/Scripts/Core.Systems/BootstrapManager.cs`:

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MnM.Core.Systems
{
    /// <summary>
    /// Runs once when the game starts. Loads the MainMenu after
    /// persistent managers are initialised.
    /// </summary>
    public class BootstrapManager : MonoBehaviour
    {
        private void Start()
        {
            // GameStateManager, AudioManager, SceneTransitionManager
            // are all in this scene and marked DontDestroyOnLoad.
            // Now load the main menu.
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Additive);
        }
    }
}
```

### Step 5: Replace LoadScene Calls

Search all controllers for `SceneManager.LoadScene(` and replace with:
```csharp
SceneTransitionManager.Instance.LoadScene("SceneName");
```

Files to update:
- `MainMenuController.cs`
- `CampaignSelectController.cs`
- `CharacterCreationController.cs`
- `PauseMenuController.cs`
- `SettlementScreenController.cs`
- Any other controller that navigates between scenes

---

## Part 2: Modal Slide-In Helper

In any controller that shows a modal panel, replace instant show/hide with:

```csharp
// Show modal:
StartCoroutine(SceneTransitionManager.Instance.SlideIn(_eventModal));

// Hide modal:
StartCoroutine(SceneTransitionManager.Instance.SlideOut(_eventModal));
```

Apply this to:
- Event modal in SettlementScreenController
- Combat result modal in CombatScreenController
- Guiding Principal modal in SettlementScreenController
- Hunt selection modal in SettlementScreenController

---

## Verification Test

- [ ] Bootstrap scene is index 0 in Build Settings; MainMenu loads after it
- [ ] Launching game: fade in from black over 0.4s
- [ ] Clicking NEW GAME: screen fades to black (0.3s) → CampaignSelect fades in (0.3s)
- [ ] No jarring instant cuts between any two scenes
- [ ] Event modal slides up from the bottom smoothly
- [ ] ESC to pause: pause overlay appears without a scene transition (no fade)
- [ ] `_isTransitioning` guard prevents double-click from firing two transitions
- [ ] AudioManager and GameStateManager still exist across scene transitions (DontDestroyOnLoad)

---

## Next Session

**File:** `_Docs/Stage_08/STAGE_08_J.md`
**Covers:** Card play and draw animations — cards slide up from deck on draw, cards slide to the field on play, discarded cards flip and fade out, activated behavior cards glow briefly
