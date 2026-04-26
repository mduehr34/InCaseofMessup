<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 8-N | Tutorial Tooltip & Onboarding System
Status: Stage 8-M complete. Travel scene working.
Task: Build TutorialTooltipManager — a singleton that shows
sequential step-by-step overlay tooltips guiding a new player
through their first settlement and first combat. Each step
highlights a UI element with a glowing ring and shows an
explanatory text box. Player clicks NEXT or the highlighted
element to advance. Tutorial only runs on Tutorial Campaign.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_08/STAGE_08_N.md
- Assets/_Game/Scripts/Core.Systems/GameStateManager.cs

Then confirm:
- Tutorial only fires when CampaignState.isTutorialCampaign == true
- Each step targets a named UIToolkit element by element name
- A semi-transparent overlay dims everything EXCEPT the target
- Player can skip the entire tutorial with a SKIP button
- Tutorial progress is saved to PlayerPrefs so it doesn't repeat
  on "Continue" saves
- What you will NOT build (full voiced tutorial — out of scope)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 8-N: Tutorial Tooltip & Onboarding System

**Resuming from:** Stage 8-M complete — travel scene working
**Done when:** Tutorial campaign shows sequential tooltips in Settlement and Combat; player can advance or skip; tooltips never show on Standard Campaign or after completion
**Commit:** `"8N: Tutorial tooltip system — step sequencer, highlight overlay, skip"`
**Next session:** STAGE_08_O.md

---

## Tutorial Step Design

Steps are defined as data — no hardcoded scene logic needed.

### Settlement Steps (Year 1)

| Step | Target Element | Tooltip Text |
|---|---|---|
| 1 | `tab-bar` | "This is your settlement. Each tab shows a different part of your camp. Start with HUNTERS." |
| 2 | `hunter-roster-list` | "These are your hunters. Each has a name, build type, and stats. They grow over years of hunting." |
| 3 | `tab-innovations` | "Click INNOVATIONS to see your settlement's collective knowledge." |
| 4 | `innovations-pool` | "Adopt an Innovation to unlock a new Grit Skill for all hunters. Start with Desperate Sprint." |
| 5 | `btn-send-hunting-party` | "Ready? Click SEND HUNTING PARTY to choose your team and begin the hunt." |

### Combat Steps (Round 1)

| Step | Target Element | Tooltip Text |
|---|---|---|
| 6 | `phase-banner` | "This is the VITALITY PHASE — your hunters act first each round." |
| 7 | `hand-container` | "Your hand of cards. Each card is an action your hunter can take. Hover to read the effect." |
| 8 | `aggro-token` | "The AGGRO TOKEN marks who the monster is targeting. The monster attacks this hunter." |
| 9 | `monster-parts-panel` | "The monster's body parts. Break the Shell (gold bar) to reach the Flesh (red bar) beneath." |
| 10 | `hand-container` | "Play a card to act. Click a card in your hand to play it. Try SHOVE to push the monster back." |

---

## TutorialStep Data Class

Create `Assets/_Game/Scripts/Core.Data/TutorialStep.cs`:

```csharp
namespace MnM.Core.Data
{
    [System.Serializable]
    public class TutorialStep
    {
        public string targetElementName;  // UIToolkit element name to highlight
        public string tooltipText;        // Explanation shown to the player
        public string sceneName;          // Which scene this step belongs to
    }
}
```

---

## TutorialTooltipManager.cs

**Path:** `Assets/_Game/Scripts/Core.Systems/TutorialTooltipManager.cs`

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace MnM.Core.Systems
{
    public class TutorialTooltipManager : MonoBehaviour
    {
        public static TutorialTooltipManager Instance { get; private set; }

        [SerializeField] private UIDocument _overlayDocument;

        private const string PrefKey = "tutorial_step";

        private VisualElement _dimOverlay;
        private VisualElement _highlightRing;
        private VisualElement _tooltipBox;
        private Label         _tooltipLabel;
        private Button        _nextBtn;
        private Button        _skipBtn;

        private int           _currentStep = 0;
        private bool          _active      = false;
        private bool          _stepComplete = false;

        private static readonly List<TutorialStep> Steps = new()
        {
            new() { sceneName="Settlement", targetElementName="tab-bar",
                    tooltipText="This is your settlement. Each tab shows a different part of your camp." },
            new() { sceneName="Settlement", targetElementName="hunter-roster-list",
                    tooltipText="These are your hunters. Each has a name, build type, and stats. They grow over years of hunting." },
            new() { sceneName="Settlement", targetElementName="tab-innovations",
                    tooltipText="Click INNOVATIONS to see your settlement's collective knowledge." },
            new() { sceneName="Settlement", targetElementName="innovations-pool",
                    tooltipText="Adopt an Innovation to unlock a new Grit Skill for all hunters. Start with Desperate Sprint." },
            new() { sceneName="Settlement", targetElementName="btn-send-hunting-party",
                    tooltipText="Ready? Click SEND HUNTING PARTY to choose your team and begin the hunt." },
            new() { sceneName="CombatScene", targetElementName="phase-banner",
                    tooltipText="VITALITY PHASE — your hunters act first each round." },
            new() { sceneName="CombatScene", targetElementName="hand-container",
                    tooltipText="Your hand of cards. Each card is an action. Hover to read the effect." },
            new() { sceneName="CombatScene", targetElementName="aggro-token-display",
                    tooltipText="The AGGRO TOKEN marks who the monster is targeting this round." },
            new() { sceneName="CombatScene", targetElementName="monster-parts-panel",
                    tooltipText="The monster's body parts. Break the Shell (gold) to expose the Flesh (red)." },
            new() { sceneName="CombatScene", targetElementName="hand-container",
                    tooltipText="Play a card to act. Click SHOVE in your hand to push the monster back." },
        };

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            _currentStep = PlayerPrefs.GetInt(PrefKey, 0);
            BuildOverlay();
        }

        private void BuildOverlay()
        {
            if (_overlayDocument == null) return;
            var root = _overlayDocument.rootVisualElement;

            _dimOverlay = new VisualElement();
            _dimOverlay.style.position       = Position.Absolute;
            _dimOverlay.style.left = _dimOverlay.style.top =
            _dimOverlay.style.right = _dimOverlay.style.bottom = 0;
            _dimOverlay.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, 0.7f));
            _dimOverlay.style.display         = DisplayStyle.None;
            _dimOverlay.pickingMode           = PickingMode.Ignore;
            root.Add(_dimOverlay);

            _highlightRing = new VisualElement();
            _highlightRing.style.position     = Position.Absolute;
            _highlightRing.style.borderTopColor = _highlightRing.style.borderBottomColor =
            _highlightRing.style.borderLeftColor = _highlightRing.style.borderRightColor =
                new StyleColor(new Color(0.72f, 0.52f, 0.04f));
            _highlightRing.style.borderTopWidth = _highlightRing.style.borderBottomWidth =
            _highlightRing.style.borderLeftWidth = _highlightRing.style.borderRightWidth = 3;
            _highlightRing.style.display         = DisplayStyle.None;
            _highlightRing.pickingMode           = PickingMode.Ignore;
            root.Add(_highlightRing);

            _tooltipBox = new VisualElement();
            _tooltipBox.style.position        = Position.Absolute;
            _tooltipBox.style.width           = 360;
            _tooltipBox.style.backgroundColor = new StyleColor(new Color(0.05f, 0.04f, 0.03f, 0.97f));
            _tooltipBox.style.borderTopColor  = _tooltipBox.style.borderBottomColor =
            _tooltipBox.style.borderLeftColor = _tooltipBox.style.borderRightColor =
                new StyleColor(new Color(0.72f, 0.52f, 0.04f));
            _tooltipBox.style.borderTopWidth  = _tooltipBox.style.borderBottomWidth =
            _tooltipBox.style.borderLeftWidth = _tooltipBox.style.borderRightWidth = 2;
            _tooltipBox.style.paddingTop      = _tooltipBox.style.paddingBottom =
            _tooltipBox.style.paddingLeft     = _tooltipBox.style.paddingRight    = 16;
            _tooltipBox.style.display         = DisplayStyle.None;
            root.Add(_tooltipBox);

            _tooltipLabel = new Label();
            _tooltipLabel.style.color      = new Color(0.83f, 0.80f, 0.73f);
            _tooltipLabel.style.fontSize   = 11;
            _tooltipLabel.style.whiteSpace = WhiteSpace.Normal;
            _tooltipLabel.style.marginBottom = 12;
            _tooltipBox.Add(_tooltipLabel);

            var btnRow = new VisualElement();
            btnRow.style.flexDirection = FlexDirection.Row;
            btnRow.style.justifyContent = Justify.SpaceBetween;
            _tooltipBox.Add(btnRow);

            _skipBtn = new Button { text = "SKIP TUTORIAL" };
            _skipBtn.style.color    = new StyleColor(new Color(0.54f, 0.54f, 0.54f));
            _skipBtn.style.fontSize = 9;
            _skipBtn.RegisterCallback<ClickEvent>(_ => CompleteTutorial());
            btnRow.Add(_skipBtn);

            _nextBtn = new Button { text = "NEXT →" };
            _nextBtn.style.backgroundColor = new StyleColor(new Color(0.72f, 0.52f, 0.04f, 0.2f));
            _nextBtn.style.borderTopColor  = _nextBtn.style.borderBottomColor =
            _nextBtn.style.borderLeftColor = _nextBtn.style.borderRightColor  =
                new StyleColor(new Color(0.72f, 0.52f, 0.04f));
            _nextBtn.style.color    = new StyleColor(new Color(0.83f, 0.80f, 0.73f));
            _nextBtn.style.fontSize = 11;
            _nextBtn.RegisterCallback<ClickEvent>(_ => AdvanceStep());
            btnRow.Add(_nextBtn);
        }

        /// <summary>Call this from SettlementScreenController.OnEnable() and CombatScreenController.OnEnable()</summary>
        public void TryShowStepsForScene(string sceneName, UIDocument sceneDocument)
        {
            if (!GameStateManager.Instance.CampaignState.isTutorialCampaign) return;
            if (_currentStep >= Steps.Count) return;

            StartCoroutine(RunStepsForScene(sceneName, sceneDocument));
        }

        private IEnumerator RunStepsForScene(string sceneName, UIDocument sceneDoc)
        {
            yield return new WaitForSeconds(0.6f); // Let scene settle

            while (_currentStep < Steps.Count &&
                   Steps[_currentStep].sceneName == sceneName)
            {
                var step   = Steps[_currentStep];
                var target = sceneDoc.rootVisualElement.Q(step.targetElementName);

                ShowStep(step, target);
                _stepComplete = false;
                yield return new WaitUntil(() => _stepComplete);
            }

            HideOverlay();
        }

        private void ShowStep(TutorialStep step, VisualElement target)
        {
            _dimOverlay.style.display  = DisplayStyle.Flex;
            _tooltipBox.style.display  = DisplayStyle.Flex;
            _tooltipLabel.text         = step.tooltipText;

            if (target != null)
            {
                var rect = target.worldBound;
                _highlightRing.style.display = DisplayStyle.Flex;
                _highlightRing.style.left    = rect.x - 4;
                _highlightRing.style.top     = rect.y - 4;
                _highlightRing.style.width   = rect.width + 8;
                _highlightRing.style.height  = rect.height + 8;

                // Position tooltip below or above target
                bool below = rect.yMax + 120 < Screen.height;
                _tooltipBox.style.left = Mathf.Clamp(rect.x, 8f, Screen.width - 376f);
                _tooltipBox.style.top  = below ? rect.yMax + 12 : rect.y - 120;
            }
            else
            {
                _highlightRing.style.display = DisplayStyle.None;
                _tooltipBox.style.left       = (Screen.width - 360) / 2f;
                _tooltipBox.style.top        = Screen.height / 2f - 60f;
            }

            _nextBtn.text = (_currentStep == Steps.Count - 1) ? "DONE ✓" : "NEXT →";
        }

        private void AdvanceStep()
        {
            _currentStep++;
            PlayerPrefs.SetInt(PrefKey, _currentStep);
            PlayerPrefs.Save();

            if (_currentStep >= Steps.Count)
                CompleteTutorial();
            else
                _stepComplete = true;
        }

        private void HideOverlay()
        {
            _dimOverlay.style.display    = DisplayStyle.None;
            _highlightRing.style.display = DisplayStyle.None;
            _tooltipBox.style.display    = DisplayStyle.None;
        }

        private void CompleteTutorial()
        {
            _currentStep = Steps.Count;
            PlayerPrefs.SetInt(PrefKey, _currentStep);
            PlayerPrefs.Save();
            HideOverlay();
            _stepComplete = true;
            Debug.Log("[Tutorial] Complete");
        }
    }
}
```

---

## Integration

In `SettlementScreenController.OnEnable()`:
```csharp
TutorialTooltipManager.Instance?.TryShowStepsForScene("Settlement", _uiDocument);
```

In `CombatScreenController.OnEnable()`:
```csharp
TutorialTooltipManager.Instance?.TryShowStepsForScene("CombatScene", _uiDocument);
```

Add `isTutorialCampaign` bool to `CampaignState` — set true when `Campaign_Tutorial.asset` is selected.

---

## Verification Test

- [ ] Start Tutorial Campaign → Settlement loads → tooltip step 1 appears over tab-bar
- [ ] Gold highlight ring surrounds the tab-bar element
- [ ] Background is dimmed behind the tooltip
- [ ] Click NEXT → step 2 appears on hunter-roster-list
- [ ] Click SKIP TUTORIAL → all tooltips disappear immediately
- [ ] Start Standard Campaign → NO tooltips appear at all
- [ ] Continue a saved Tutorial Campaign that finished step 3 → starts at step 4
- [ ] After completing all 10 steps → tutorial never shows again (PlayerPrefs saved)

---

## Next Session

**File:** `_Docs/Stage_08/STAGE_08_O.md`
**Covers:** Chronicle Log and Codex UI — CodexEntrySO data class, tabbed log in the settlement showing chronicle entries and codex entries, entries unlocked by events
