<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 8-K | Combat Action Animations
Status: Stage 8-J complete. Card animations working.
Task: Add visual feedback to combat actions. Attack: flash
the target part red briefly. Hit: show a tiny pixel burst
particle on the token. Miss: show a "MISS" floating text.
Monster move: smoothly lerp the token across grid cells.
Part break: show a crack flash and part name highlights red.
Hunter collapse: token shakes then fades to a "collapsed" tint.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_08/STAGE_08_K.md
- Assets/_Game/Scripts/Core.UI/CombatScreenController.cs
- Assets/_Game/Scripts/Core.Systems/CombatManager.cs

Then confirm:
- Token movement uses Transform.position lerp in world space
  (not UIToolkit — tokens are SpriteRenderer GameObjects on the grid)
- Hit/Miss visuals are UIToolkit overlays (world-space to screen-space)
- Part flash uses UIToolkit (the part list panel is already UIToolkit)
- All animations are non-blocking — game state advances immediately
- Part break is permanent — the bar turns grey and stays red-named

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 8-K: Combat Action Animations

**Resuming from:** Stage 8-J complete — card animations working
**Done when:** Attack hits show flash + impact; misses show floating "MISS" text; monster moves smoothly on grid; part breaks trigger crack effect; hunter collapse shakes and dims token
**Commit:** `"8K: Combat action animations — hit flash, miss text, monster move lerp, part break, collapse"`
**Next session:** STAGE_08_L.md

---

## Part 1: Generate Impact Sprites

Use CoPlay `generate_or_edit_images`. Save to `Assets/_Game/Art/Generated/UI/Combat/`

| Filename | Size | Description |
|---|---|---|
| `fx_hit_shell.png` | 32×32 | Pixel burst — white/gold sparks radiating outward. Shell impact. Transparent bg. |
| `fx_hit_flesh.png` | 32×32 | Dark red splatter dots. Flesh impact. Transparent bg. |
| `fx_part_break.png` | 48×48 | Crack lines radiating from centre. Bone-white and grey. Transparent bg. |
| `fx_miss.png` | 32×16 | The word "MISS" in grey pixel font, slightly diagonal. Transparent bg. |

Import: Sprite (2D and UI), Point (No Filter), PPU 16

---

## Part 2: CombatAnimationController.cs

**Path:** `Assets/_Game/Scripts/Core.UI/CombatAnimationController.cs`

```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace MnM.Core.UI
{
    public class CombatAnimationController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;

        // ── Hit Flash ───────────────────────────────────────────────────

        /// <summary>Show impact sprite at a world-space position.</summary>
        public void ShowHitImpact(Vector3 worldPos, bool isShell)
        {
            StartCoroutine(ImpactRoutine(worldPos, isShell));
        }

        private IEnumerator ImpactRoutine(Vector3 worldPos, bool isShell)
        {
            string path = isShell ? "Art/Generated/UI/Combat/fx_hit_shell"
                                  : "Art/Generated/UI/Combat/fx_hit_flesh";
            var sprite  = Resources.Load<Sprite>(path);
            if (sprite == null) yield break;

            var el = new VisualElement();
            el.style.width  = 48;
            el.style.height = 48;
            el.style.position = Position.Absolute;
            el.style.backgroundImage     = new StyleBackground(sprite);
            el.style.backgroundScaleMode = ScaleMode.ScaleToFit;
            el.pickingMode = PickingMode.Ignore;

            // Convert world → screen → UIToolkit coords
            Vector2 screen = Camera.main.WorldToScreenPoint(worldPos);
            float uiX = screen.x - 24f;
            float uiY = Screen.height - screen.y - 24f;
            el.style.left = uiX;
            el.style.top  = uiY;

            _uiDocument.rootVisualElement.Add(el);

            // Animate: scale up then fade
            float t = 0f, duration = 0.3f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = t / duration;
                el.style.opacity = Mathf.Lerp(1f, 0f, p);
                float scale = Mathf.Lerp(0.5f, 1.5f, p);
                el.style.scale = new Scale(new Vector3(scale, scale, 1f));
                yield return null;
            }
            _uiDocument.rootVisualElement.Remove(el);
        }

        // ── Miss Text ───────────────────────────────────────────────────

        public void ShowMiss(Vector3 worldPos)
        {
            StartCoroutine(MissRoutine(worldPos));
        }

        private IEnumerator MissRoutine(Vector3 worldPos)
        {
            Vector2 screen = Camera.main.WorldToScreenPoint(worldPos);
            float uiX = screen.x - 20f;
            float uiY = Screen.height - screen.y;

            var label = new Label("MISS");
            label.style.position  = Position.Absolute;
            label.style.left      = uiX;
            label.style.top       = uiY;
            label.style.color     = new Color(0.54f, 0.54f, 0.54f);
            label.style.fontSize  = 14;
            label.pickingMode     = PickingMode.Ignore;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            _uiDocument.rootVisualElement.Add(label);

            float t = 0f, duration = 0.6f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = t / duration;
                label.style.opacity = Mathf.Lerp(1f, 0f, p * p);
                label.style.top     = uiY - 30f * p;
                yield return null;
            }
            _uiDocument.rootVisualElement.Remove(label);
        }

        // ── Part Break Flash ────────────────────────────────────────────

        /// <summary>Flash a crack effect on the part panel entry and lock bar to grey.</summary>
        public void ShowPartBreak(VisualElement partContainer, string partName)
        {
            StartCoroutine(PartBreakRoutine(partContainer));
            Debug.Log($"[Combat] PART BREAK: {partName}");
        }

        private IEnumerator PartBreakRoutine(VisualElement partContainer)
        {
            // Flash red
            for (int i = 0; i < 3; i++)
            {
                partContainer.style.backgroundColor = new StyleColor(new Color(0.5f, 0.1f, 0.1f, 0.5f));
                yield return new WaitForSeconds(0.08f);
                partContainer.style.backgroundColor = StyleKeyword.Initial;
                yield return new WaitForSeconds(0.06f);
            }
            // Lock shell bar to grey
            // (PartHealthBar.SetValues() with shell=0 already turns part name red — see Stage 8-G)
        }

        // ── Monster Move ────────────────────────────────────────────────

        /// <summary>Smoothly move a token from its current position to targetPos.</summary>
        public void AnimateMove(GameObject token, Vector3 targetPos, float duration = 0.25f)
        {
            StartCoroutine(MoveRoutine(token, targetPos, duration));
        }

        private IEnumerator MoveRoutine(GameObject token, Vector3 target, float duration)
        {
            Vector3 start = token.transform.position;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                token.transform.position = Vector3.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t / duration));
                yield return null;
            }
            token.transform.position = target;
        }

        // ── Hunter Collapse ─────────────────────────────────────────────

        public void AnimateCollapse(GameObject hunterToken)
        {
            StartCoroutine(CollapseRoutine(hunterToken));
        }

        private IEnumerator CollapseRoutine(GameObject token)
        {
            var renderer = token.GetComponent<SpriteRenderer>();
            if (renderer == null) yield break;

            // Shake
            Vector3 origin = token.transform.position;
            for (int i = 0; i < 8; i++)
            {
                token.transform.position = origin + new Vector3(
                    Random.Range(-0.06f, 0.06f), Random.Range(-0.04f, 0.04f), 0);
                yield return new WaitForSeconds(0.04f);
            }
            token.transform.position = origin;

            // Fade to dark collapsed tint
            float t = 0f, duration = 0.4f;
            Color startColor = renderer.color;
            Color endColor   = new Color(0.3f, 0.15f, 0.15f, 0.5f);
            while (t < duration)
            {
                t += Time.deltaTime;
                renderer.color = Color.Lerp(startColor, endColor, t / duration);
                yield return null;
            }
            renderer.color = endColor;
            Debug.Log($"[Combat] {token.name} collapsed");
        }
    }
}
```

---

## Integration

Add `CombatAnimationController` component to the same GameObject as `CombatScreenController`.

In `CombatScreenController` / `CombatManager` event handlers:

```csharp
// After resolving an attack:
if (hit && isShell)
    _combatAnim.ShowHitImpact(targetToken.transform.position, isShell: true);
else if (hit)
    _combatAnim.ShowHitImpact(targetToken.transform.position, isShell: false);
else
    _combatAnim.ShowMiss(targetToken.transform.position);

// After a part breaks:
_combatAnim.ShowPartBreak(partContainer, partName);

// After monster moves:
_combatAnim.AnimateMove(monsterToken, targetWorldPos);

// After hunter collapses:
_combatAnim.AnimateCollapse(hunterToken);
```

---

## Verification Test

- [ ] Hit a shell part → gold/white burst appears and fades at impact point
- [ ] Hit a flesh wound → dark red splatter appears and fades
- [ ] Miss → grey "MISS" text floats up and fades
- [ ] Break Gaunt Throat shell → container flashes red 3 times, shell bar turns grey
- [ ] Monster plays Creeping Advance → monster token slides to new cell smoothly (0.25s)
- [ ] Aldric reaches 0 Flesh → token shakes then fades to dark tint
- [ ] Multiple effects can run simultaneously (attack on one part while another moves)
- [ ] No NullReferenceException when Camera.main is called

---

## Next Session

**File:** `_Docs/Stage_08/STAGE_08_L.md`
**Covers:** Settlement UI animations — item crafting forge flash, gear equip highlight in the gear grid, innovation adoption glow, and year advance banner
