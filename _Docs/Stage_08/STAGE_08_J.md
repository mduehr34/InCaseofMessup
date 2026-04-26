<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 8-J | Card Play & Draw Animations
Status: Stage 8-I complete. Scene transitions working.
Task: Add animations to the card system in combat.
Cards slide up from a deck position on draw. Cards slide
toward the field area when played, then vanish. Discarded
cards flip-fade out. Monster behavior cards flash briefly
when they activate. All animations run via coroutine on
the UIToolkit VisualElements — no Animator component needed.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_08/STAGE_08_J.md
- Assets/_Game/Scripts/Core.UI/CombatScreenController.cs
- Assets/_Game/Scripts/Core.UI/CardRenderer.cs

Then confirm:
- All animations use UIElement style transitions or coroutines
- Draw animation: 0.2s slide up from deck position
- Play animation: 0.15s scale-down + slide toward field, then remove
- Discard: 0.15s fade out + slight downward slide
- Behavior card activation: 0.3s gold border pulse, then normal
- Animations do NOT block game logic — they fire and the
  game state updates immediately; visuals catch up

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 8-J: Card Play & Draw Animations

**Resuming from:** Stage 8-I complete — scene transitions working
**Done when:** Draw, play, discard, and behavior card activation all have smooth visual animations in combat
**Commit:** `"8J: Card play and draw animations — draw slide, play throw, discard fade, behavior pulse"`
**Next session:** STAGE_08_K.md

---

## Key Design Principles

- **Game logic first:** State updates immediately. Animations are purely cosmetic.
- **Short durations:** 0.15–0.3s. These happen constantly — they must feel snappy, not slow.
- **No blocking:** Animations run in coroutines and never yield for the next player input.

---

## CardAnimationController.cs

**Path:** `Assets/_Game/Scripts/Core.UI/CardAnimationController.cs`

```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace MnM.Core.UI
{
    /// <summary>
    /// Coroutine-based animations for card VisualElements in the combat hand.
    /// Call these from CombatScreenController — they do NOT wait for completion
    /// before returning, so game logic can proceed immediately.
    /// </summary>
    public class CardAnimationController : MonoBehaviour
    {
        // ── Draw Animation ──────────────────────────────────────────────

        /// <summary>
        /// Slides a card VisualElement up from below the hand area.
        /// Call after adding the card to the hand container.
        /// </summary>
        public void AnimateDraw(VisualElement card, float delay = 0f)
        {
            StartCoroutine(DrawRoutine(card, delay));
        }

        private IEnumerator DrawRoutine(VisualElement card, float delay)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);

            card.style.opacity    = 0;
            card.style.translate  = new Translate(0, 60, 0);

            float t = 0f, duration = 0.22f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = Mathf.SmoothStep(0f, 1f, t / duration);
                card.style.opacity   = p;
                card.style.translate = new Translate(0, Mathf.Lerp(60f, 0f, p), 0);
                yield return null;
            }
            card.style.opacity   = 1;
            card.style.translate = new Translate(0, 0, 0);
        }

        // ── Play Animation ──────────────────────────────────────────────

        /// <summary>
        /// Card shrinks and flies toward the field (centre-screen), then fades out.
        /// Remove the card from the hand BEFORE calling this — pass the detached element.
        /// </summary>
        public void AnimatePlay(VisualElement card, VisualElement fieldRoot)
        {
            StartCoroutine(PlayRoutine(card, fieldRoot));
        }

        private IEnumerator PlayRoutine(VisualElement card, VisualElement root)
        {
            // Briefly show the card at its current position shrinking
            float t = 0f, duration = 0.18f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = t / duration;
                card.style.opacity   = Mathf.Lerp(1f, 0f, p);
                card.style.scale     = new Scale(new Vector3(Mathf.Lerp(1f, 0.5f, p),
                                                              Mathf.Lerp(1f, 0.5f, p), 1f));
                card.style.translate = new Translate(0, Mathf.Lerp(0f, -30f, p), 0);
                yield return null;
            }
            // Remove from DOM (already detached from hand — just clean up overlay)
            if (card.parent != null) card.parent.Remove(card);
        }

        // ── Discard Animation ───────────────────────────────────────────

        /// <summary>
        /// Card fades out and drops slightly. Remove from hand BEFORE calling.
        /// </summary>
        public void AnimateDiscard(VisualElement card)
        {
            StartCoroutine(DiscardRoutine(card));
        }

        private IEnumerator DiscardRoutine(VisualElement card)
        {
            float t = 0f, duration = 0.15f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = t / duration;
                card.style.opacity   = Mathf.Lerp(1f, 0f, p);
                card.style.translate = new Translate(0, Mathf.Lerp(0f, 20f, p), 0);
                yield return null;
            }
            if (card.parent != null) card.parent.Remove(card);
        }

        // ── Behavior Card Activation Pulse ──────────────────────────────

        /// <summary>
        /// Flashes the card with a gold border to indicate it just activated.
        /// Does NOT remove the card — it stays in the monster panel.
        /// </summary>
        public void AnimateBehaviorActivation(VisualElement card)
        {
            StartCoroutine(BehaviorPulse(card));
        }

        private IEnumerator BehaviorPulse(VisualElement card)
        {
            var gold = new StyleColor(new Color(0.72f, 0.52f, 0.04f));
            var dim  = new StyleColor(new Color(0.5f, 0.1f, 0.1f));

            // Flash to gold
            card.style.borderTopColor = card.style.borderBottomColor =
            card.style.borderLeftColor = card.style.borderRightColor = gold;
            card.style.borderTopWidth  = card.style.borderBottomWidth =
            card.style.borderLeftWidth = card.style.borderRightWidth = 3;

            yield return new WaitForSeconds(0.35f);

            // Fade back
            float t = 0f, duration = 0.25f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = t / duration;
                var c = Color.Lerp(new Color(0.72f, 0.52f, 0.04f), new Color(0.5f, 0.1f, 0.1f), p);
                card.style.borderTopColor = card.style.borderBottomColor =
                card.style.borderLeftColor = card.style.borderRightColor = new StyleColor(c);
                yield return null;
            }
            card.style.borderTopColor = card.style.borderBottomColor =
            card.style.borderLeftColor = card.style.borderRightColor = dim;
            card.style.borderTopWidth  = card.style.borderBottomWidth =
            card.style.borderLeftWidth = card.style.borderRightWidth = 2;
        }

        // ── Card Hover Lift ─────────────────────────────────────────────

        /// <summary>
        /// Register hover lift on a card so it rises slightly on mouse-over.
        /// Call once when building the hand.
        /// </summary>
        public static void RegisterHoverLift(VisualElement card)
        {
            card.RegisterCallback<MouseEnterEvent>(_ =>
                card.style.translate = new Translate(0, -12, 0));
            card.RegisterCallback<MouseLeaveEvent>(_ =>
                card.style.translate = new Translate(0, 0, 0));
        }
    }
}
```

---

## Integration in CombatScreenController

Add `CardAnimationController` as a component to the same GameObject as `CombatScreenController`.

```csharp
// Declare field:
[SerializeField] private CardAnimationController _cardAnim;

// On drawing a card (in RefreshHandDisplay or DrawCard()):
var cardEl = CardRenderer.BuildActionCard(cardSO);
CardAnimationController.RegisterHoverLift(cardEl);
_handContainer.Add(cardEl);
_cardAnim.AnimateDraw(cardEl, delay: handPosition * 0.05f); // stagger by slot

// On playing a card:
var cardEl = GetCardElement(playedCard); // retrieve the VisualElement
_handContainer.Remove(cardEl);
_cardAnim.AnimatePlay(cardEl, _uiDocument.rootVisualElement);
// [Game logic already processed by CombatManager]

// On discarding a card:
_cardAnim.AnimateDiscard(cardEl);

// On a behavior card activating (from monster AI):
var behaviorCardEl = GetBehaviorCardElement(card);
_cardAnim.AnimateBehaviorActivation(behaviorCardEl);
```

---

## Deck Visual (Pixel Art Pile)

Use CoPlay `generate_or_edit_images` to create:

**Deck stack sprite (48×64):**
```
Pixel art of a small stack of stone-carved cards seen from
a slight 3/4 angle. Dark stone texture. Marrow gold edges
visible on the side. Simple, readable silhouette.
Transparent background. 16-bit style.
```
Save to: `Assets/_Game/Art/Generated/UI/ui_deck_stack.png`
Display in the bottom-left of the combat UI as the "draw pile" indicator with a card count label on top.

---

## Verification Test

- [ ] Start combat — drawing 2 cards: they slide up with a 0.05s stagger between them
- [ ] Mouse over a hand card — it lifts 12px upward
- [ ] Click a card to play it — card shrinks and fades toward field, game resolves effect
- [ ] End of round — remaining hand cards fade-discard
- [ ] Monster phase — active behavior card border pulses gold, then dims back to red
- [ ] No animation lag or stuttering during normal combat
- [ ] Multiple animations can run simultaneously (e.g., draw 3 cards in a row)
- [ ] Removed card elements are cleaned up (no ghost elements in DOM)

---

## Next Session

**File:** `_Docs/Stage_08/STAGE_08_K.md`
**Covers:** Combat action animations — attack flash on target part, hit impact pixel burst, miss whoosh, monster movement animation on the grid, and the part-break crack effect
