<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 8-F | Status Effect Visuals — Icons & Display System
Status: Stage 8-E complete. Settings and pause menu working.
Task: Generate 8 status effect icon sprites. Build a
StatusEffectDisplay component that shows active effects on
combat tokens. Wire to CombatManager events so icons appear
and disappear as effects are applied/removed in combat.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_08/STAGE_08_F.md
- Assets/_Game/Scripts/Core.Systems/CombatManager.cs
- Assets/_Game/Scripts/Core.Data/Enums.cs

Then confirm:
- All icons are 16×16 pixel art, transparent background
- StatusEffectDisplay is a MonoBehaviour on each combat token
- Icons appear as a horizontal strip below the token sprite
- Duration counter shows as a small number overlaid on the icon
- Shaken, Pinned, Slowed, Exposed, Bleeding all have unique icons
- What you will NOT build this session (status effect logic
  is already in CombatManager — we're only adding visuals)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 8-F: Status Effect Visuals — Icons & Display System

**Resuming from:** Stage 8-E complete — settings and pause working
**Done when:** All 8 status icons generated and imported; StatusEffectDisplay updates correctly when effects are applied/removed in a test combat; duration numbers display on icons
**Commit:** `"8F: Status effect icons and display system — combat tokens updated"`
**Next session:** STAGE_08_G.md

---

## Status Effects in Marrow & Myth

| Effect | Meaning |
|---|---|
| **Shaken** | Hunter is rattled — next action costs +1 AP |
| **Pinned** | Cannot move; must pass Force Check to remove |
| **Slowed** | Movement reduced by half |
| **Exposed** | A specific part has 0 Shell; attacks bypass Shell |
| **Bleeding** | Lose 1 Flesh at the start of each round |
| **Marked** | +1 Accuracy against this target for all hunters |
| **Broken** | Applied to a part when Shell reaches 0 permanently |
| **Inspired** | Gain 1 AP at the start of next turn |

---

## Step 1: Generate Status Icons via CoPlay

Use `generate_or_edit_images` for each. All icons: **16×16 px**, transparent background, pixel art, dark palette.

Save all to: `Assets/_Game/Art/Generated/UI/StatusIcons/`
Import settings: Sprite (2D and UI), Point (No Filter), PPU 16, Compression None

| Filename | Prompt |
|---|---|
| `status_shaken.png` | Tiny pixel art icon: jagged lightning bolt, yellow-white, suggesting flinch or alarm. 16x16. Transparent bg. |
| `status_pinned.png` | Tiny pixel art icon: downward-pointing spike or nail piercing a flat surface. Dark red. 16x16. Transparent bg. |
| `status_slowed.png` | Tiny pixel art icon: a snail or two overlapping boot prints with an X. Ash grey. 16x16. Transparent bg. |
| `status_exposed.png` | Tiny pixel art icon: a shield split in half / cracked open, revealing interior. Marrow gold outline. 16x16. Transparent bg. |
| `status_bleeding.png` | Tiny pixel art icon: three blood drops falling, dried blood brown (#4A2020) turning red. 16x16. Transparent bg. |
| `status_marked.png` | Tiny pixel art icon: a crosshair / target reticle, bone white. 16x16. Transparent bg. |
| `status_broken.png` | Tiny pixel art icon: a cracked bone or shattered piece, grey and dark. 16x16. Transparent bg. |
| `status_inspired.png` | Tiny pixel art icon: small upward flame or star burst, marrow gold. 16x16. Transparent bg. |

---

## Step 2: StatusEffectDisplay.cs

**Path:** `Assets/_Game/Scripts/Core.UI/StatusEffectDisplay.cs`

This component sits on each hunter/monster token GameObject in the combat scene. It manages a row of icon images below the sprite.

```csharp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace MnM.Core.UI
{
    /// <summary>
    /// Manages the visual status effect icon strip on a combat token.
    /// Attach to each Hunter and Monster token GameObject.
    /// </summary>
    public class StatusEffectDisplay : MonoBehaviour
    {
        // Key: effect name (matches Enums.StatusEffect.ToString())
        // Value: remaining duration (-1 = permanent)
        private readonly Dictionary<string, int> _activeEffects = new();

        // The icon strip — a row of VisualElements in the parent UIDocument
        private VisualElement _iconStrip;

        // Sprites loaded by name
        private static readonly Dictionary<string, Sprite> _iconCache = new();

        private static readonly string[] KnownEffects =
            { "Shaken","Pinned","Slowed","Exposed","Bleeding","Marked","Broken","Inspired" };

        /// <summary>
        /// Call once from the token setup code with the icon strip VisualElement
        /// from the combat UIDocument.
        /// </summary>
        public void Initialise(VisualElement iconStrip)
        {
            _iconStrip = iconStrip;
            PreloadIcons();
            Refresh();
        }

        // ── Public API ──────────────────────────────────────────────────

        public void ApplyEffect(string effectName, int duration)
        {
            _activeEffects[effectName] = duration;
            Debug.Log($"[Status] Applied {effectName} ({duration} rounds)");
            Refresh();
        }

        public void RemoveEffect(string effectName)
        {
            _activeEffects.Remove(effectName);
            Debug.Log($"[Status] Removed {effectName}");
            Refresh();
        }

        /// <summary>Call at the start of each new round to tick down durations.</summary>
        public void TickDurations()
        {
            var toRemove = new List<string>();
            foreach (var key in new List<string>(_activeEffects.Keys))
            {
                if (_activeEffects[key] == -1) continue; // permanent
                _activeEffects[key]--;
                if (_activeEffects[key] <= 0) toRemove.Add(key);
            }
            foreach (var key in toRemove)
                _activeEffects.Remove(key);

            Refresh();
        }

        public bool HasEffect(string effectName) => _activeEffects.ContainsKey(effectName);

        // ── Private ──────────────────────────────────────────────────────

        private void Refresh()
        {
            if (_iconStrip == null) return;
            _iconStrip.Clear();

            foreach (var kvp in _activeEffects)
            {
                var container = new VisualElement();
                container.style.width  = 18;
                container.style.height = 18;
                container.style.marginRight = 2;
                container.style.position   = Position.Relative;

                // Icon
                var icon = new VisualElement();
                icon.style.width  = 16;
                icon.style.height = 16;
                icon.style.backgroundScaleMode = ScaleMode.ScaleToFit;
                if (_iconCache.TryGetValue(kvp.Key.ToLower(), out var sprite))
                    icon.style.backgroundImage = new StyleBackground(sprite);
                container.Add(icon);

                // Duration counter (top-right corner)
                if (kvp.Value > 0)
                {
                    var dur = new Label(kvp.Value.ToString());
                    dur.style.position   = Position.Absolute;
                    dur.style.right      = 0;
                    dur.style.bottom     = 0;
                    dur.style.fontSize   = 7;
                    dur.style.color      = Color.white;
                    dur.style.backgroundColor = new Color(0, 0, 0, 0.6f);
                    container.Add(dur);
                }

                _iconStrip.Add(container);
            }
        }

        private void PreloadIcons()
        {
            foreach (string effect in KnownEffects)
            {
                string key  = effect.ToLower();
                string path = $"Art/Generated/UI/StatusIcons/status_{key}";
                if (!_iconCache.ContainsKey(key))
                {
                    var sprite = Resources.Load<Sprite>(path);
                    if (sprite != null) _iconCache[key] = sprite;
                }
            }
        }
    }
}
```

---

## Step 3: Wire StatusEffectDisplay to Combat Tokens

In the combat scene, each hunter and monster has a token (a GameObject with a SpriteRenderer or UI element). Add `StatusEffectDisplay` as a component to each.

**In the token setup code** (wherever tokens are instantiated, e.g., `CombatScreenController` or `GridManager`):

```csharp
// After creating a hunter token:
var statusDisplay = tokenGO.AddComponent<StatusEffectDisplay>();

// Find or create the icon strip VisualElement in the UIDocument
var iconStrip = _uiDocument.rootVisualElement.Q($"status-strip-{hunter.characterId}");
if (iconStrip == null)
{
    iconStrip = new VisualElement();
    iconStrip.name = $"status-strip-{hunter.characterId}";
    iconStrip.style.flexDirection = FlexDirection.Row;
    // Position it below the token (exact position depends on your layout)
    _uiDocument.rootVisualElement.Q("combat-grid").Add(iconStrip);
}
statusDisplay.Initialise(iconStrip);
```

**In CombatManager**, when an effect is applied:
```csharp
// Find the StatusEffectDisplay for the affected entity and call:
statusDisplay.ApplyEffect("Shaken", 2);

// When a round ends:
foreach (var display in allStatusDisplays)
    display.TickDurations();
```

---

## Verification Test

- [ ] All 8 icon PNG files exist in `Assets/_Game/Art/Generated/UI/StatusIcons/`
- [ ] All icons are 16×16 with transparent backgrounds
- [ ] Run a combat — Apply Shaken to Aldric → Shaken icon appears below his token
- [ ] Duration number "2" visible in corner of Shaken icon
- [ ] After 2 rounds → Shaken icon disappears automatically
- [ ] Applying Pinned + Bleeding shows two icons side by side
- [ ] Removing Pinned manually removes only the Pinned icon
- [ ] No null reference errors when a token has no active effects

---

## Next Session

**File:** `_Docs/Stage_08/STAGE_08_G.md`
**Covers:** Combat UI polish — monster part health bars (Shell + Flesh per part), AP counter, Grit counter, round number display, and the VITALITY PHASE / MONSTER PHASE transition banner
