<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 8-G | Combat UI Polish — HP Bars, Counters & Phase Banner
Status: Stage 8-F complete. Status effect icons working.
Task: Add visual HP bars for each monster body part (Shell
shown in gold, Flesh in red). Add AP counter, Grit counter,
and round number display to the hunter HUD. Build the
animated phase transition banner that slides in when the
phase changes (VITALITY PHASE / MONSTER PHASE).

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_08/STAGE_08_G.md
- Assets/_Game/Scripts/Core.UI/CombatScreenController.cs
- Assets/_Game/Scripts/Core.Data/MonsterSO.cs

Then confirm:
- Shell bars show as gold (#B8860B), Flesh bars as red (#4A2020)
- AP and Grit counters update whenever CombatManager fires events
- Phase banner slides in from off-screen top, holds 1.5s, slides out
- Part bars update immediately when damage is applied — no delay
- What you will NOT change (combat logic — visuals only this session)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 8-G: Combat UI Polish — HP Bars, Counters & Phase Banner

**Resuming from:** Stage 8-F complete — status effect icons working
**Done when:** Monster part HP bars display and update on damage; AP and Grit counters update correctly; phase banner animates in/out on phase change
**Commit:** `"8G: Combat UI polish — part HP bars, AP/Grit counters, phase transition banner"`
**Next session:** STAGE_08_H.md

---

## Part 1: Monster Part HP Bars

Each monster part (e.g., Gaunt's Head, Throat, Torso) needs two progress bars:
- **Gold bar** = remaining Shell durability
- **Red bar** = remaining Flesh durability

These appear in the monster's part list panel on the right side of the combat UI.

### PartHealthBar.cs

**Path:** `Assets/_Game/Scripts/Core.UI/PartHealthBar.cs`

```csharp
using UnityEngine;
using UnityEngine.UIElements;

namespace MnM.Core.UI
{
    /// <summary>
    /// Manages the Shell and Flesh progress bars for one monster body part.
    /// Add via code when building the monster part panel in CombatScreenController.
    /// </summary>
    public class PartHealthBar
    {
        private readonly Label         _partNameLabel;
        private readonly VisualElement _shellBar;
        private readonly VisualElement _fleshBar;
        private readonly Label         _shellVal;
        private readonly Label         _fleshVal;

        private int _maxShell;
        private int _maxFlesh;

        public PartHealthBar(VisualElement container, string partName, int maxShell, int maxFlesh)
        {
            _maxShell = maxShell;
            _maxFlesh = maxFlesh;

            // Part name
            _partNameLabel = new Label(partName.ToUpper());
            _partNameLabel.style.color    = new Color(0.83f, 0.80f, 0.73f);
            _partNameLabel.style.fontSize = 9;
            _partNameLabel.style.marginBottom = 2;
            container.Add(_partNameLabel);

            // Shell bar row
            var shellRow = MakeBarRow("SHELL", new Color(0.72f, 0.52f, 0.04f), // Marrow gold
                                      out _shellBar, out _shellVal);
            container.Add(shellRow);

            // Flesh bar row
            var fleshRow = MakeBarRow("FLESH", new Color(0.29f, 0.13f, 0.13f), // Dried blood
                                      out _fleshBar, out _fleshVal);
            container.Add(fleshRow);

            // Spacer
            var spacer = new VisualElement();
            spacer.style.height = 6;
            container.Add(spacer);

            SetValues(maxShell, maxFlesh);
        }

        public void SetValues(int currentShell, int currentFlesh)
        {
            float shellPct = _maxShell > 0 ? (float)currentShell / _maxShell : 0f;
            float fleshPct = _maxFlesh > 0 ? (float)currentFlesh / _maxFlesh : 0f;

            _shellBar.style.width = new StyleLength(new Length(shellPct * 100f, LengthUnit.Percent));
            _fleshBar.style.width = new StyleLength(new Length(fleshPct * 100f, LengthUnit.Percent));

            _shellVal.text = currentShell.ToString();
            _fleshVal.text = currentFlesh.ToString();

            // Flash red if shell just broke
            if (currentShell == 0)
                _partNameLabel.style.color = new Color(0.72f, 0.2f, 0.2f);
        }

        private VisualElement MakeBarRow(string label, Color barColor,
                                          out VisualElement fill, out Label valLabel)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems    = Align.Center;
            row.style.marginBottom  = 2;

            var lbl = new Label(label);
            lbl.style.color    = new Color(0.54f, 0.54f, 0.54f);
            lbl.style.fontSize = 7;
            lbl.style.width    = 30;
            row.Add(lbl);

            var track = new VisualElement();
            track.style.flexGrow        = 1;
            track.style.height          = 6;
            track.style.backgroundColor = new Color(0.12f, 0.10f, 0.08f);
            track.style.marginRight     = 4;

            fill = new VisualElement();
            fill.style.height          = 6;
            fill.style.backgroundColor = new StyleColor(barColor);
            fill.style.width           = new StyleLength(new Length(100f, LengthUnit.Percent));
            fill.style.transitionDuration = new List<TimeValue> { new TimeValue(0.2f, TimeUnit.Second) };
            track.Add(fill);
            row.Add(track);

            valLabel = new Label("?");
            valLabel.style.color    = new Color(0.83f, 0.80f, 0.73f);
            valLabel.style.fontSize = 8;
            valLabel.style.width    = 14;
            row.Add(valLabel);

            return row;
        }
    }
}
```

**In CombatScreenController** — when building the monster part panel, create one `PartHealthBar` per part:

```csharp
// In BuildMonsterPartPanel() or equivalent:
_partBars.Clear();
foreach (var part in monster.standardParts)
{
    var partContainer = new VisualElement();
    _partPanel.Add(partContainer);
    var bar = new PartHealthBar(partContainer, part.partName, part.shellDurability, part.fleshDurability);
    _partBars[part.partName] = bar;
}

// When damage is dealt, call:
if (_partBars.TryGetValue(partName, out var bar))
    bar.SetValues(currentShell, currentFlesh);
```

---

## Part 2: Hunter HUD — AP, Grit, Round Counter

These elements already exist in the combat UXML from earlier stages. This session ensures they update correctly and look polished.

### CombatHUDUpdater.cs

**Path:** `Assets/_Game/Scripts/Core.UI/CombatHUDUpdater.cs`

```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace MnM.Core.UI
{
    public class CombatHUDUpdater : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;

        private Label _roundLabel;
        private Label _phaseLabel;
        private VisualElement _phaseBanner;

        private void OnEnable()
        {
            var root    = _uiDocument.rootVisualElement;
            _roundLabel  = root.Q<Label>("round-number");
            _phaseBanner = root.Q("phase-banner");
            _phaseLabel  = root.Q<Label>("phase-label");

            // Start off-screen
            if (_phaseBanner != null)
                _phaseBanner.style.top = -80;
        }

        public void SetRound(int round)
        {
            if (_roundLabel != null)
                _roundLabel.text = $"ROUND {round}";
        }

        public void SetHunterStats(string hunterId, int ap, int maxAp, int grit, int maxGrit)
        {
            var root = _uiDocument.rootVisualElement;
            root.Q<Label>($"ap-{hunterId}")   .text = $"{ap}/{maxAp} AP";
            root.Q<Label>($"grit-{hunterId}") .text = $"{grit} GRIT";
        }

        public void ShowPhaseBanner(string phaseName)
        {
            if (_phaseLabel  != null) _phaseLabel.text = phaseName;
            StartCoroutine(AnimatePhaseBanner());
        }

        private IEnumerator AnimatePhaseBanner()
        {
            if (_phaseBanner == null) yield break;

            // Slide down from top
            float t = 0f;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                _phaseBanner.style.top = Mathf.Lerp(-80f, 20f, t / 0.3f);
                yield return null;
            }
            _phaseBanner.style.top = 20f;

            yield return new WaitForSeconds(1.5f);

            // Slide back up
            t = 0f;
            while (t < 0.25f)
            {
                t += Time.deltaTime;
                _phaseBanner.style.top = Mathf.Lerp(20f, -80f, t / 0.25f);
                yield return null;
            }
            _phaseBanner.style.top = -80f;
        }
    }
}
```

### Add Phase Banner to UXML

In your existing combat UXML, add inside the root element:

```xml
<!-- Phase transition banner — animated from script, starts off-screen -->
<ui:VisualElement name="phase-banner" style="position:absolute; left:0; right:0; top:-80px;
    height:64px; background-color:rgba(10,10,12,0.9); border-color:#B8860B;
    border-bottom-width:2px; align-items:center; justify-content:center;">
  <ui:Label name="phase-label" text="VITALITY PHASE"
            style="color:#D4CCBA; font-size:20px; letter-spacing:4px;" />
</ui:VisualElement>

<!-- Round counter -->
<ui:Label name="round-number" text="ROUND 1"
          style="position:absolute; top:8px; right:16px; color:#8A8A8A; font-size:12px;" />
```

**Call from CombatManager** when phase changes:
```csharp
_hudUpdater.ShowPhaseBanner("VITALITY PHASE");
_hudUpdater.ShowPhaseBanner("MONSTER PHASE");
_hudUpdater.SetRound(currentRound);
```

---

## Verification Test

- [ ] Start a combat — part list panel shows one Shell bar and one Flesh bar per part
- [ ] Shell bars are gold, Flesh bars are dark red
- [ ] Deal Shell damage to Gaunt Head — gold bar shortens immediately
- [ ] Break Gaunt Throat Shell — part name turns red
- [ ] "ROUND 1" displays top-right, increments to "ROUND 2" at round end
- [ ] Phase banner slides down smoothly at start of Vitality Phase
- [ ] Banner shows "VITALITY PHASE" text, holds 1.5 seconds, slides back up
- [ ] Same animation fires for "MONSTER PHASE"
- [ ] AP and Grit labels update when Aldric spends AP or uses Grit
- [ ] No frame drop during banner animation

---

## Next Session

**File:** `_Docs/Stage_08/STAGE_08_H.md`
**Covers:** Card visual rendering system — generate card frame art, build CardRenderer component that dynamically displays action card name, category, AP cost, weapon type, and effect text on a stone-carved card frame
