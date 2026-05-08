<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 10-M | Combat Action Animations — Hit Impact, Move, Collapse
Status: Stage 10-K complete. Balance pass done.
Task: Add action feedback animations to the combat screen.
When a card resolves, flash a hit/miss indicator on the monster
panel. When a hunter collapses, play the collapse pulse on
their panel. When a monster part breaks, flash the break
indicator on that part bar. All animations use UIToolkit
USS transitions — no external animation library.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_10/STAGE_10_M.md
- Assets/_Game/Scripts/Core.UI/CombatScreenController.cs
- Assets/_Game/Scripts/Core.Systems/CombatManager.cs
- Assets/_Game/Scripts/Core.Data/CombatState.cs

Then confirm:
- CombatScreenController already fires OnDamageDealt and
  OnHunterCollapsed events (these are the hooks to wire)
- USS file for the combat screen already defines grid-cell,
  part-bar, hunter-panel classes
- All animations are cosmetic only — no logic changes
- What you will NOT build: token lerp movement (that's 10-N)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 10-M: Combat Action Animations — Hit Impact, Move, Collapse

**Resuming from:** Stage 10-K complete — balance pass done; all systems functional
**Done when:** Shell hits, flesh wounds, misses, part breaks, and collapses all have distinct visual feedback in the combat UI; all USS transitions are smooth at 60 fps
**Commit:** `"10M: Combat action animations — hit flash, part break, collapse pulse"`
**Next session:** STAGE_10_N.md

---

## What You Are Building

The combat screen already has correct data and phase flow. This stage adds the visual juice that makes actions feel impactful. All feedback is implemented as USS class toggling with CSS transitions — no C# coroutines, no external tweening library.

---

## Part 1: CombatAnimationController.cs

Create `Assets/_Game/Scripts/Core.UI/CombatAnimationController.cs`:

```csharp
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class CombatAnimationController : MonoBehaviour
{
    private UIDocument _doc;

    public void Init(UIDocument doc)
    {
        _doc = doc;
    }

    // ── Hit Impact ──────────────────────────────────────────────

    public void ShowHitImpact(string partName, string damageType)
    {
        // damageType: "shell" | "flesh"
        var partEl = _doc.rootVisualElement.Q<VisualElement>($"part-bar--{partName.ToLower()}");
        if (partEl == null) return;

        string cssClass = damageType == "shell" ? "part-bar--hit-shell" : "part-bar--hit-flesh";
        FlashClass(partEl, cssClass, 300);
    }

    public void ShowMiss(string hunterId)
    {
        var hunterEl = _doc.rootVisualElement.Q<VisualElement>($"hunter-panel--{hunterId}");
        if (hunterEl == null) return;
        FlashClass(hunterEl, "hunter-panel--miss", 400);
    }

    public void ShowPartBreak(string partName)
    {
        var partEl = _doc.rootVisualElement.Q<VisualElement>($"part-bar--{partName.ToLower()}");
        if (partEl == null) return;
        FlashClass(partEl, "part-bar--broken", 600);
        Debug.Log($"[CombatAnim] Part break flash: {partName}");
    }

    // ── Collapse ────────────────────────────────────────────────

    public void AnimateCollapse(string hunterId)
    {
        var hunterEl = _doc.rootVisualElement.Q<VisualElement>($"hunter-panel--{hunterId}");
        if (hunterEl == null) return;
        hunterEl.EnableInClassList("hunter-panel--collapsed-flash", true);
        StartCoroutine(RemoveClassAfter(hunterEl, "hunter-panel--collapsed-flash", 800));
        Debug.Log($"[CombatAnim] Collapse pulse: {hunterId}");
    }

    // ── Behavior Pulse ──────────────────────────────────────────

    public void AnimateBehaviorPulse(string cardName)
    {
        // Briefly highlight the active behavior card in the deck panel
        var root = _doc.rootVisualElement;
        root.Query<VisualElement>(className: "behavior-card").ForEach(el =>
        {
            var label = el.Q<Label>("behavior-card-name");
            if (label != null && label.text == cardName)
                FlashClass(el, "behavior-card--active-pulse", 500);
        });
    }

    // ── Internal Helpers ─────────────────────────────────────────

    private void FlashClass(VisualElement el, string cssClass, int durationMs)
    {
        el.EnableInClassList(cssClass, true);
        StartCoroutine(RemoveClassAfter(el, cssClass, durationMs));
    }

    private IEnumerator RemoveClassAfter(VisualElement el, string cssClass, int ms)
    {
        yield return new WaitForSeconds(ms / 1000f);
        el?.EnableInClassList(cssClass, false);
    }
}
```

---

## Part 2: Wire Into CombatScreenController

Open `Assets/_Game/Scripts/Core.UI/CombatScreenController.cs`.

**Step 1 — Field:**
```csharp
private CombatAnimationController _animController;
```

**Step 2 — Init (in Awake or Start, after `_doc` is assigned):**
```csharp
_animController = gameObject.AddComponent<CombatAnimationController>();
_animController.Init(_doc);
```

**Step 3 — Wire OnDamageDealt:**

In the handler that fires when `CombatManager.OnDamageDealt` triggers (the delegate/event you already subscribe to in OnEnable), add after updating bars:
```csharp
// Animate the feedback
if (args.damageType == "shell")
    _animController?.ShowHitImpact(args.partName, "shell");
else if (args.damageType == "flesh")
    _animController?.ShowHitImpact(args.partName, "flesh");
else if (args.damageType == "miss")
    _animController?.ShowMiss(args.hunterId);

if (args.partBroken)
    _animController?.ShowPartBreak(args.partName);
```

**Step 4 — Wire OnHunterCollapsed:**
```csharp
_animController?.AnimateCollapse(args.hunterId);
```

**Step 5 — Wire behavior card pulse in RunMonsterPhase result handler:**
```csharp
if (!string.IsNullOrEmpty(executedCardName))
    _animController?.AnimateBehaviorPulse(executedCardName);
```

---

## Part 3: USS — Animation Classes

Open the combat screen USS file. Add at the bottom:

```css
/* ── Hit feedback ──────────────────────────────────────── */

.part-bar--hit-shell {
    background-color: rgba(180, 140, 40, 0.60);
    transition: background-color 0.3s ease-out;
}

.part-bar--hit-flesh {
    background-color: rgba(180, 50, 50, 0.70);
    transition: background-color 0.3s ease-out;
}

.part-bar--broken {
    background-color: rgba(255, 80, 0, 0.80);
    border-color: rgba(255, 160, 0, 0.90);
    border-width: 2px;
    transition: background-color 0.6s ease-out, border-color 0.6s ease-out;
}

/* ── Miss feedback ─────────────────────────────────────── */

.hunter-panel--miss {
    border-color: rgba(120, 120, 120, 0.70);
    border-width: 1px;
    transition: border-color 0.4s ease-out;
}

/* ── Collapse pulse ────────────────────────────────────── */

.hunter-panel--collapsed-flash {
    background-color: rgba(80, 20, 20, 0.80);
    border-color: rgba(200, 50, 50, 0.90);
    border-width: 2px;
    transition: background-color 0.8s ease-out, border-color 0.8s ease-out;
}

/* ── Behavior card active ──────────────────────────────── */

.behavior-card--active-pulse {
    background-color: rgba(90, 60, 20, 0.70);
    border-color: rgba(200, 150, 40, 0.80);
    border-width: 1px;
    transition: background-color 0.5s ease-out;
}
```

---

## Art Assets Required

The following sprite assets are needed for hit impact overlays. They are **Editor-generated only** using the Anthropic image tool:

| Asset | Filename | Description |
|---|---|---|
| Shell hit spark | `fx_hit_shell.png` | Small amber spark burst, 32×32 px, transparent bg |
| Flesh wound splat | `fx_hit_flesh.png` | Dark red splatter, 32×32 px, transparent bg |
| Part break crack | `fx_part_break.png` | White crack lines radiating outward, 48×48 px |
| Miss puff | `fx_miss.png` | Faint grey smoke puff, 32×32 px, transparent bg |

These sprites are used as background-image overlays on the part-bar element during the flash. Once generated and approved, place them in `Assets/_Game/Art/FX/Combat/`.

**Generation note:** All four should use a dark, desaturated palette that reads well against the combat screen's near-black background. No bright colours other than the amber/red accent on shell/flesh hits.

---

## Verification Test

- [ ] Card resolves with shell damage → amber flash appears on the target part bar for ~300ms
- [ ] Card resolves with flesh damage → red flash appears on the target part bar for ~300ms
- [ ] Card misses → grey border pulse appears on the active hunter panel for ~400ms
- [ ] Monster part reaches shell 0 → orange break flash on part bar, wider border for ~600ms
- [ ] Hunter head or torso reaches flesh 0 → dark red pulse on full hunter panel for ~800ms
- [ ] Monster executes behavior card → that card's row in the behavior deck panel briefly highlights
- [ ] No animation extends longer than 1 second (transitions auto-clear via `RemoveClassAfter`)
- [ ] Rapidly resolving multiple cards in sequence does not stack or leave permanent CSS classes
- [ ] All flashes visible at 1920×1080 and at 1280×720
- [ ] Unity profiler shows no garbage allocation spike during flash (USS class toggle is GC-free)

---

## Next Session

**File:** `_Docs/Stage_10/STAGE_10_N.md`
**Covers:** Settlement UI Animations — craft success pulse, gear equip flash, year banner reveal, innovation adopt effect
