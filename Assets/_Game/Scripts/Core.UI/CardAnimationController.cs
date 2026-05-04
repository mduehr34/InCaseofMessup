using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace MnM.Core.UI
{
    /// <summary>
    /// Coroutine-based animations for card VisualElements in the combat hand.
    /// Call from CombatScreenController — no animation blocks game logic.
    /// </summary>
    public class CardAnimationController : MonoBehaviour
    {
        // ── Draw ─────────────────────────────────────────────────────

        /// <summary>
        /// Slides a card up from below the hand area over 0.22s.
        /// Call after adding the card to the hand container.
        /// </summary>
        public void AnimateDraw(VisualElement card, float delay = 0f)
        {
            StartCoroutine(DrawRoutine(card, delay));
        }

        private IEnumerator DrawRoutine(VisualElement card, float delay)
        {
            // Hide immediately so the card doesn't flash at its natural position during the delay
            card.style.opacity   = 0;
            card.style.translate = new Translate(80, 0, 0);

            if (delay > 0f) yield return new WaitForSeconds(delay);

            float t = 0f, dur = 0.22f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float p = Mathf.SmoothStep(0f, 1f, t / dur);
                card.style.opacity   = p;
                card.style.translate = new Translate(Mathf.Lerp(80f, 0f, p), 0, 0);
                yield return null;
            }
            card.style.opacity   = 1;
            card.style.translate = new Translate(0, 0, 0);
        }

        // ── Play ──────────────────────────────────────────────────────

        /// <summary>
        /// Card shrinks and fades upward over 0.18s.
        /// Remove the card from the hand container and reparent to an overlay
        /// BEFORE calling — the routine removes it from the overlay when done.
        /// </summary>
        public void AnimatePlay(VisualElement card, VisualElement overlay)
        {
            StartCoroutine(PlayRoutine(card, overlay));
        }

        private IEnumerator PlayRoutine(VisualElement card, VisualElement overlay)
        {
            float t = 0f, dur = 0.18f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float p = t / dur;
                card.style.opacity   = Mathf.Lerp(1f, 0f, p);
                card.style.scale     = new Scale(new Vector3(Mathf.Lerp(1f, 0.5f, p),
                                                              Mathf.Lerp(1f, 0.5f, p), 1f));
                card.style.translate = new Translate(0, Mathf.Lerp(0f, -30f, p), 0);
                yield return null;
            }
            if (card.parent != null) card.parent.Remove(card);
        }

        // ── Discard ───────────────────────────────────────────────────

        /// <summary>
        /// Card fades out and drops 20px over 0.15s.
        /// Remove from hand container and reparent to an overlay BEFORE calling.
        /// </summary>
        public void AnimateDiscard(VisualElement card)
        {
            StartCoroutine(DiscardRoutine(card));
        }

        private IEnumerator DiscardRoutine(VisualElement card)
        {
            float t = 0f, dur = 0.15f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float p = t / dur;
                card.style.opacity   = Mathf.Lerp(1f, 0f, p);
                card.style.translate = new Translate(0, Mathf.Lerp(0f, 20f, p), 0);
                yield return null;
            }
            if (card.parent != null) card.parent.Remove(card);
        }

        // ── Behavior Card Activation Pulse ────────────────────────────

        /// <summary>
        /// Flashes a gold border on the card for 0.35s, then fades back to the
        /// default behavior-red over 0.25s. Card stays in the monster panel.
        /// </summary>
        public void AnimateBehaviorActivation(VisualElement card)
        {
            StartCoroutine(BehaviorPulse(card));
        }

        private IEnumerator BehaviorPulse(VisualElement card)
        {
            var gold = new StyleColor(new Color(0.72f, 0.52f, 0.04f));
            var red  = new StyleColor(new Color(0.5f, 0.1f, 0.1f));

            SetBorderColor(card, gold);
            SetBorderWidth(card, 3);

            yield return new WaitForSeconds(0.35f);

            float t = 0f, dur = 0.25f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float p = t / dur;
                var c = Color.Lerp(new Color(0.72f, 0.52f, 0.04f), new Color(0.5f, 0.1f, 0.1f), p);
                SetBorderColor(card, new StyleColor(c));
                yield return null;
            }

            SetBorderColor(card, red);
            SetBorderWidth(card, 2);
        }

        // ── Hover Lift ────────────────────────────────────────────────

        /// <summary>
        /// Registers mouse-enter/leave callbacks to lift the card 12px on hover.
        /// Call once per card element when building the hand.
        /// </summary>
        public static void RegisterHoverLift(VisualElement card)
        {
            card.RegisterCallback<MouseEnterEvent>(_ =>
                card.style.translate = new Translate(0, -12, 0));
            card.RegisterCallback<MouseLeaveEvent>(_ =>
                card.style.translate = new Translate(0, 0, 0));
        }

        // ── Helpers ───────────────────────────────────────────────────

        private static void SetBorderColor(VisualElement el, StyleColor c)
        {
            el.style.borderTopColor    = c;
            el.style.borderBottomColor = c;
            el.style.borderLeftColor   = c;
            el.style.borderRightColor  = c;
        }

        private static void SetBorderWidth(VisualElement el, float w)
        {
            el.style.borderTopWidth    = w;
            el.style.borderBottomWidth = w;
            el.style.borderLeftWidth   = w;
            el.style.borderRightWidth  = w;
        }
    }
}
