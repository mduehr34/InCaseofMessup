<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 8-L | Settlement UI Animations
Status: Stage 8-K complete. Combat action animations done.
Task: Add visual feedback to the settlement screen.
Craft success: a brief forge-glow flash on the crafted item
slot. Gear equip: the gear grid cell pulses gold when an
item is placed. Innovation adoption: the card glows then
settles into a dimmed "adopted" state. Year advance:
a full-screen banner shows the new year number before
loading the next phase.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_08/STAGE_08_L.md
- Assets/_Game/Scripts/Core.UI/SettlementScreenController.cs

Then confirm:
- All settlement UI is UIToolkit — use VisualElement style changes
- Year advance banner reuses the phase banner approach from 8-G
- Innovation adoption: card dims in place, cascade cards slide in
- Gear grid cell pulse is a border-color transition
- Crafting flash: the item card in the recipe list briefly glows

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 8-L: Settlement UI Animations

**Resuming from:** Stage 8-K complete — combat action animations done
**Done when:** Crafting, gear equip, innovation adoption, and year advance all have polish animations in the settlement scene
**Commit:** `"8L: Settlement UI animations — craft flash, gear pulse, innovation glow, year banner"`
**Next session:** STAGE_08_M.md

---

## SettlementAnimationController.cs

**Path:** `Assets/_Game/Scripts/Core.UI/SettlementAnimationController.cs`

```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace MnM.Core.UI
{
    public class SettlementAnimationController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;

        // ── Craft Success Flash ─────────────────────────────────────────

        /// <summary>
        /// Call after an item is successfully crafted.
        /// itemCardElement: the VisualElement card in the recipe list.
        /// </summary>
        public void AnimateCraftSuccess(VisualElement itemCardElement)
        {
            StartCoroutine(CraftFlashRoutine(itemCardElement));
        }

        private IEnumerator CraftFlashRoutine(VisualElement card)
        {
            if (card == null) yield break;

            // Pulse to marrow gold background
            for (int i = 0; i < 2; i++)
            {
                card.style.backgroundColor = new StyleColor(new Color(0.72f, 0.52f, 0.04f, 0.35f));
                card.style.borderTopColor  = card.style.borderBottomColor =
                card.style.borderLeftColor = card.style.borderRightColor =
                    new StyleColor(new Color(0.72f, 0.52f, 0.04f));
                yield return new WaitForSeconds(0.12f);
                card.style.backgroundColor = StyleKeyword.Initial;
                yield return new WaitForSeconds(0.08f);
            }

            // Final settle: gold border fades back to normal
            float t = 0f, duration = 0.4f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = t / duration;
                var c = Color.Lerp(new Color(0.72f, 0.52f, 0.04f),
                                   new Color(0.31f, 0.27f, 0.20f), p);
                card.style.borderTopColor  = card.style.borderBottomColor =
                card.style.borderLeftColor = card.style.borderRightColor = new StyleColor(c);
                yield return null;
            }
            card.style.borderTopColor  = card.style.borderBottomColor =
            card.style.borderLeftColor = card.style.borderRightColor = StyleKeyword.Initial;
        }

        // ── Gear Grid Cell Equip Pulse ──────────────────────────────────

        /// <summary>
        /// Pulse a gear grid cell gold when an item is placed into it.
        /// cellElement: the VisualElement representing the grid cell.
        /// </summary>
        public void AnimateGearEquip(VisualElement cellElement)
        {
            StartCoroutine(GearEquipPulse(cellElement));
        }

        private IEnumerator GearEquipPulse(VisualElement cell)
        {
            if (cell == null) yield break;

            var gold = new StyleColor(new Color(0.72f, 0.52f, 0.04f));
            cell.style.borderTopColor  = cell.style.borderBottomColor =
            cell.style.borderLeftColor = cell.style.borderRightColor = gold;
            cell.style.borderTopWidth  = cell.style.borderBottomWidth =
            cell.style.borderLeftWidth = cell.style.borderRightWidth = 3;
            cell.style.backgroundColor = new StyleColor(new Color(0.72f, 0.52f, 0.04f, 0.15f));

            yield return new WaitForSeconds(0.3f);

            float t = 0f, duration = 0.35f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = t / duration;
                var c = Color.Lerp(new Color(0.72f, 0.52f, 0.04f),
                                   new Color(0.31f, 0.27f, 0.20f), p);
                cell.style.borderTopColor  = cell.style.borderBottomColor =
                cell.style.borderLeftColor = cell.style.borderRightColor = new StyleColor(c);
                float bg = Mathf.Lerp(0.15f, 0f, p);
                cell.style.backgroundColor = new StyleColor(new Color(0.72f, 0.52f, 0.04f, bg));
                yield return null;
            }
            cell.style.borderTopWidth  = cell.style.borderBottomWidth =
            cell.style.borderLeftWidth = cell.style.borderRightWidth = 1;
            cell.style.borderTopColor  = cell.style.borderBottomColor =
            cell.style.borderLeftColor = cell.style.borderRightColor = StyleKeyword.Initial;
            cell.style.backgroundColor = StyleKeyword.Initial;
        }

        // ── Innovation Adoption Glow ────────────────────────────────────

        /// <summary>
        /// Glow the innovation card gold, then settle it to the dimmed "adopted" state.
        /// cascadeCards: optional list of newly unlocked cascade cards to slide in.
        /// </summary>
        public void AnimateInnovationAdopt(VisualElement innovCard,
                                            VisualElement[] cascadeCards = null)
        {
            StartCoroutine(InnovationAdoptRoutine(innovCard, cascadeCards));
        }

        private IEnumerator InnovationAdoptRoutine(VisualElement card,
                                                    VisualElement[] cascades)
        {
            if (card == null) yield break;

            // Bright gold flash
            card.style.backgroundColor = new StyleColor(new Color(0.72f, 0.52f, 0.04f, 0.6f));
            yield return new WaitForSeconds(0.2f);

            // Fade to adopted dim
            float t = 0f, duration = 0.5f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = t / duration;
                float alpha = Mathf.Lerp(0.6f, 0f, p);
                card.style.backgroundColor = new StyleColor(new Color(0.72f, 0.52f, 0.04f, alpha));
                card.style.opacity = Mathf.Lerp(1f, 0.5f, p); // dim the adopted card
                yield return null;
            }
            card.style.opacity = 0.5f;
            card.style.backgroundColor = StyleKeyword.Initial;

            // Slide in cascade cards
            if (cascades != null)
            {
                foreach (var cascadeCard in cascades)
                {
                    cascadeCard.style.opacity   = 0;
                    cascadeCard.style.translate = new Translate(0, 30, 0);
                    yield return new WaitForSeconds(0.1f);

                    float ct = 0f, cdur = 0.25f;
                    while (ct < cdur)
                    {
                        ct += Time.deltaTime;
                        float cp = ct / cdur;
                        cascadeCard.style.opacity   = Mathf.Lerp(0f, 1f, cp);
                        cascadeCard.style.translate = new Translate(0, Mathf.Lerp(30f, 0f, cp), 0);
                        yield return null;
                    }
                    cascadeCard.style.opacity   = 1;
                    cascadeCard.style.translate = new Translate(0, 0, 0);
                }
            }
        }

        // ── Year Advance Banner ─────────────────────────────────────────

        /// <summary>
        /// Shows a full-screen year-advance banner then fades away.
        /// Call at the start of a new year.
        /// </summary>
        public void ShowYearBanner(int year)
        {
            StartCoroutine(YearBannerRoutine(year));
        }

        private IEnumerator YearBannerRoutine(int year)
        {
            var root = _uiDocument.rootVisualElement;

            var banner = new VisualElement();
            banner.style.position       = Position.Absolute;
            banner.style.left = banner.style.top = banner.style.right = banner.style.bottom = 0;
            banner.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, 0f));
            banner.style.alignItems     = Align.Center;
            banner.style.justifyContent = Justify.Center;
            banner.pickingMode          = PickingMode.Ignore;
            root.Add(banner);

            var yearLabel = new Label($"YEAR {year}");
            yearLabel.style.color    = new Color(0.83f, 0.80f, 0.73f);
            yearLabel.style.fontSize = 48;
            yearLabel.style.opacity  = 0;
            yearLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            banner.Add(yearLabel);

            var subLabel = new Label(year == 1 ? "The settlement begins." : "Another year passes.");
            subLabel.style.color    = new Color(0.54f, 0.54f, 0.54f);
            subLabel.style.fontSize = 14;
            subLabel.style.opacity  = 0;
            subLabel.style.marginTop = 12;
            banner.Add(subLabel);

            // Fade in
            float t = 0f;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                float p = t / 0.5f;
                banner.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, p * 0.85f));
                yearLabel.style.opacity  = p;
                subLabel.style.opacity   = Mathf.Max(0f, p * 2f - 1f);
                yield return null;
            }

            yield return new WaitForSeconds(1.8f);

            // Fade out
            t = 0f;
            while (t < 0.4f)
            {
                t += Time.deltaTime;
                float p = 1f - t / 0.4f;
                banner.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, p * 0.85f));
                yearLabel.style.opacity = p;
                subLabel.style.opacity  = p;
                yield return null;
            }
            root.Remove(banner);
        }
    }
}
```

---

## Integration

Add `SettlementAnimationController` to the `SettlementUI` GameObject.

In `SettlementScreenController`:

```csharp
// After crafting an item successfully:
_settlementAnim.AnimateCraftSuccess(craftedItemCard);

// After placing gear in the grid:
_settlementAnim.AnimateGearEquip(gridCellElement);

// After adopting an innovation:
var cascades = BuildCascadeCardElements(adoptedInn.addsToDeck);
_settlementAnim.AnimateInnovationAdopt(innovationCard, cascades);

// At the start of each new year (called from OnEndYearClicked after advancing):
_settlementAnim.ShowYearBanner(newYear);
```

---

## Verification Test

- [ ] Craft an item → card flashes gold twice, then border fades back to normal
- [ ] Place gear in gear grid → cell border pulses gold then dims back
- [ ] Adopt INN-01 → card dims to 50% opacity; INN-07 and INN-11 slide in below
- [ ] End Year 1 → full-screen "YEAR 2" banner fades in, holds ~1.8s, fades out
- [ ] Year banner shows correct year number each time
- [ ] Animations don't block settlement UI interaction (tabs still work during fade)
- [ ] No null reference errors if animations are called rapidly in sequence

---

## Next Session

**File:** `_Docs/Stage_08/STAGE_08_M.md`
**Covers:** Hunt Travel scene — full TravelController implementation, 0–3 random travel events with card-style display, CONTINUE TO HUNT button, atmospheric scene dressing
