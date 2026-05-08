<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 10-N | Settlement UI Animations — Craft, Gear, Year Banner
Status: Stage 10-M complete. Combat action animations done.
Task: Add feedback animations to the Settlement screen. When a
craft succeeds, pulse the gear slot. When gear is equipped,
flash the equipment row. When innovations are adopted, briefly
highlight the innovation panel. When the year advances, show
the year banner reveal sequence.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_10/STAGE_10_N.md
- Assets/_Game/Scripts/Core.UI/SettlementScreenController.cs
- Assets/_Game/Scripts/Core.Systems/GameStateManager.cs

Then confirm:
- SettlementScreenController already fires craft/equip/innovate
  events through delegates (check OnCraftComplete, OnGearEquipped,
  OnInnovationAdopted, OnYearAdvanced)
- All USS transitions target the existing panel class names
- No logic changes — animations are cosmetic hooks only
- What you will NOT build: animated settlement map or
  character portrait transitions

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 10-N: Settlement UI Animations — Craft, Gear, Year Banner

**Resuming from:** Stage 10-M complete — combat action animations done
**Done when:** Craft success, gear equip, innovation adoption, and year advance all have distinct visual feedback in the Settlement UI; year banner plays as a full-panel reveal; all effects are USS transitions
**Commit:** `"10N: Settlement UI animations — craft pulse, gear flash, year banner, innovation highlight"`
**Next session:** STAGE_10_L.md

---

## What You Are Building

The settlement screen is functional but silent — crafting, equipping, and year advance happen with no visual acknowledgment. This stage adds feedback animations so every meaningful action registers clearly.

---

## Part 1: SettlementAnimationController.cs

Create `Assets/_Game/Scripts/Core.UI/SettlementAnimationController.cs`:

```csharp
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class SettlementAnimationController : MonoBehaviour
{
    private UIDocument _doc;
    private VisualElement _yearBanner;

    public void Init(UIDocument doc)
    {
        _doc = doc;
        _yearBanner = doc.rootVisualElement.Q<VisualElement>("year-banner");
    }

    // ── Craft Success ────────────────────────────────────────────

    public void AnimateCraftSuccess(string gearSlotId)
    {
        var slotEl = _doc.rootVisualElement.Q<VisualElement>(gearSlotId);
        if (slotEl == null)
        {
            Debug.LogWarning($"[SettlementAnim] Gear slot not found: {gearSlotId}");
            return;
        }
        FlashClass(slotEl, "gear-slot--craft-success", 500);
        Debug.Log($"[SettlementAnim] Craft success pulse: {gearSlotId}");
    }

    // ── Gear Equip ───────────────────────────────────────────────

    public void AnimateGearEquip(string hunterId, string gearName)
    {
        var hunterRow = _doc.rootVisualElement.Q<VisualElement>($"hunter-row--{hunterId}");
        if (hunterRow == null) return;
        FlashClass(hunterRow, "hunter-row--equip-flash", 400);
        Debug.Log($"[SettlementAnim] Gear equip flash: {hunterId} → {gearName}");
    }

    // ── Innovation Adopt ─────────────────────────────────────────

    public void AnimateInnovationAdopt(string innovationId)
    {
        var innovEl = _doc.rootVisualElement.Q<VisualElement>($"innovation--{innovationId}");
        if (innovEl == null) return;
        FlashClass(innovEl, "innovation--adopted-pulse", 700);
        Debug.Log($"[SettlementAnim] Innovation adopted: {innovationId}");
    }

    // ── Year Banner ──────────────────────────────────────────────

    public void ShowYearBanner(int year, string subtitle = "")
    {
        if (_yearBanner == null) return;

        // Update text
        var yearLabel = _yearBanner.Q<Label>("year-banner-label");
        var subtitleLabel = _yearBanner.Q<Label>("year-banner-subtitle");
        if (yearLabel != null) yearLabel.text = $"YEAR {year}";
        if (subtitleLabel != null) subtitleLabel.text = subtitle;

        // Reveal sequence: fade in, hold, fade out
        StartCoroutine(YearBannerSequence());
        Debug.Log($"[SettlementAnim] Year banner: Year {year} — {subtitle}");
    }

    private IEnumerator YearBannerSequence()
    {
        _yearBanner.EnableInClassList("year-banner--hidden", false);
        _yearBanner.EnableInClassList("year-banner--visible", true);

        yield return new WaitForSeconds(2.0f);

        _yearBanner.EnableInClassList("year-banner--visible", false);
        _yearBanner.EnableInClassList("year-banner--hidden", true);
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

## Part 2: Wire Into SettlementScreenController

Open `Assets/_Game/Scripts/Core.UI/SettlementScreenController.cs`.

**Step 1 — Field:**
```csharp
private SettlementAnimationController _animController;
```

**Step 2 — Init (in Awake or OnEnable, after `_doc` is assigned):**
```csharp
_animController = gameObject.AddComponent<SettlementAnimationController>();
_animController.Init(_doc);
```

**Step 3 — Wire craft success:**

Find the method that handles a successful craft action. After updating the gear slot's displayed value, add:
```csharp
_animController?.AnimateCraftSuccess($"gear-slot--{craftedItem.slotId}");
```

**Step 4 — Wire gear equip:**

Find the method that handles equipment assignment to a hunter. After updating the hunter panel, add:
```csharp
_animController?.AnimateGearEquip(hunter.hunterId, gear.gearName);
```

**Step 5 — Wire innovation adoption:**

Find the method that marks an innovation as adopted. After updating the innovation panel, add:
```csharp
_animController?.AnimateInnovationAdopt(innovation.innovationId);
```

**Step 6 — Wire year advance:**

Find the method that triggers year advancement (OnYearAdvanced delegate or equivalent). Add before refreshing the full screen:
```csharp
string subtitle = newYear <= 5 ? "The hunt begins." :
                  newYear <= 15 ? "The cost grows." : "The end draws near.";
_animController?.ShowYearBanner(newYear, subtitle);
```

---

## Part 3: Year Banner UXML

Add this to the Settlement UXML file, as a direct child of the root element (so it renders over the full screen):

```xml
<ui:VisualElement name="year-banner" class="year-banner year-banner--hidden">
    <ui:Label name="year-banner-label" class="year-banner-label" text="YEAR 1" />
    <ui:Label name="year-banner-subtitle" class="year-banner-subtitle" text="" />
</ui:VisualElement>
```

---

## Part 4: USS — Animation Classes

Open the settlement screen USS file. Add at the bottom:

```css
/* ── Craft success ─────────────────────────────────────── */

.gear-slot--craft-success {
    background-color: rgba(40, 110, 60, 0.50);
    border-color: rgba(70, 180, 90, 0.80);
    border-width: 2px;
    transition: background-color 0.5s ease-out, border-color 0.5s ease-out;
}

/* ── Gear equip ────────────────────────────────────────── */

.hunter-row--equip-flash {
    background-color: rgba(60, 90, 140, 0.50);
    border-color: rgba(100, 150, 210, 0.70);
    border-width: 1px;
    transition: background-color 0.4s ease-out, border-color 0.4s ease-out;
}

/* ── Innovation adopted ────────────────────────────────── */

.innovation--adopted-pulse {
    background-color: rgba(80, 50, 110, 0.60);
    border-color: rgba(150, 100, 200, 0.80);
    border-width: 2px;
    transition: background-color 0.7s ease-out, border-color 0.7s ease-out;
}

/* ── Year banner ───────────────────────────────────────── */

.year-banner {
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    align-items: center;
    justify-content: center;
    background-color: rgba(0, 0, 0, 0.70);
    flex-direction: column;
    transition: opacity 0.5s ease-in-out;
}

.year-banner--hidden {
    opacity: 0;
    display: none;
}

.year-banner--visible {
    opacity: 1;
    display: flex;
}

.year-banner-label {
    font-size: 48px;
    color: rgb(220, 190, 130);
    -unity-font-style: bold;
    letter-spacing: 6px;
    margin-bottom: 12px;
}

.year-banner-subtitle {
    font-size: 18px;
    color: rgb(160, 140, 100);
    letter-spacing: 2px;
    -unity-font-style: italic;
}
```

---

## Verification Test

- [ ] Craft a gear item → the target gear slot flashes green for ~500ms then returns to normal
- [ ] Equip gear to a hunter → that hunter's row flashes blue for ~400ms
- [ ] Adopt an innovation → the innovation panel pulses purple for ~700ms
- [ ] Year advances → full-screen dark overlay appears with "YEAR N" and subtitle text, holds 2 seconds, fades out
- [ ] Year banner subtitle changes correctly: ≤5 "The hunt begins.", ≤15 "The cost grows.", >15 "The end draws near."
- [ ] No animation leaves a permanent CSS class (verify via UIToolkit Debugger)
- [ ] Year banner does not block Settlement UI interaction during fade-in or fade-out
- [ ] Rapidly crafting multiple items does not stack flash classes permanently
- [ ] All effects visible at both 1920×1080 and 1280×720

---

## Next Session

**File:** `_Docs/Stage_10/STAGE_10_L.md`
**Covers:** Final Integration & Ship — complete DoD, Windows standalone build, performance check, debug cleanup, v1.0-gold tag
