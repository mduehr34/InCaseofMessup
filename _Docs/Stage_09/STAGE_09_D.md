<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 9-D | Gear Overlay Sprite System
Status: Stage 9-C complete. Animator controllers working.
Task: Build a layered sprite overlay system so equipped gear
is visually shown on top of the hunter token's base sprite.
Each gear piece (head, chest, arms, weapon, off-hand) has its
own SpriteRenderer child object on the hunter token, drawn
over the base sprite using sorting order. Equipping or removing
gear in the settlement automatically updates the overlay sprites
on the combat token.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_09/STAGE_09_D.md
- Assets/_Game/Scripts/Core.Data/HunterState.cs
- Assets/_Game/Scripts/Core.UI/CombatScreenController.cs

Then confirm:
- Overlays are child SpriteRenderer GameObjects on the token
- Layer order: Base (0), Arms (1), Chest (2), Head (3),
  Weapon (4), OffHand (5)
- GearSO already has a spritePath field (or add one now)
- Overlays update whenever gear is equipped in Settlement
  AND when the combat token is first spawned
- What you will NOT build (per-animation overlay frames — all
  overlays are static, non-animated for MVP)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 9-D: Gear Overlay Sprite System

**Resuming from:** Stage 9-C complete — animator controllers built and working
**Done when:** Equipping a helmet in Settlement shows the helmet sprite on the hunter token; removing it removes the overlay; all 5 gear slots function correctly
**Commit:** `"9D: Gear overlay sprite system — layered SpriteRenderer children on hunter tokens"`
**Next session:** STAGE_09_E.md

---

## How the Overlay System Works

Each hunter token is a GameObject with multiple SpriteRenderer child objects stacked at the same world position. The base hunter sprite is at sorting order 0. Each gear slot adds a child at a higher sorting order so it draws on top.

```
Token_Aldric (GameObject)
├── SpriteRenderer [Base sprite, order 0]   ← hunter_aethel_south.png
├── GearOverlay_Arms (child GO)             [SpriteRenderer, order 1]
├── GearOverlay_Chest (child GO)            [SpriteRenderer, order 2]
├── GearOverlay_Head (child GO)             [SpriteRenderer, order 3]
├── GearOverlay_Weapon (child GO)           [SpriteRenderer, order 4]
└── GearOverlay_OffHand (child GO)          [SpriteRenderer, order 5]
```

When gear is equipped, the corresponding child's sprite is set. When it's removed, the sprite is set to null (invisible).

---

## Part 1: Add `overlaySprite` to GearSO

Open `Assets/_Game/Scripts/Core.Data/GearSO.cs` and add:

```csharp
[Header("Visuals")]
public Sprite overlaySprite;         // The sprite drawn on top of the hunter token
public string gearSlot;              // "Head" | "Chest" | "Arms" | "Weapon" | "OffHand"
```

If `gearSlot` already exists as a string field, leave it. Just add `overlaySprite`.

---

## Part 2: Generate Gear Overlay Sprites

**New developer note:** Gear overlays are transparent sprites the same size as the hunter token (32×48) with only the gear piece drawn in the correct position. Everything else is transparent. When placed exactly on top of the base sprite, they look like the hunter is wearing the gear.

**Save path:** `Assets/_Game/Art/Generated/Gear/Overlays/`

**Import settings:** Sprite (2D and UI), Point (No Filter), PPU 16, Compression: None

### Prompt Template

```
Pixel art gear overlay sprite. 32×48 pixels. Transparent background
except for the gear piece itself. The gear should be positioned
correctly to overlay on a {SLOT_POSITION} of a 32×48 hunter figure.
No hunter body — only the gear item drawn at the correct location.
Dark fantasy style. {GEAR_DESCRIPTION}. Point art, no anti-aliasing.
Palette: bone white, iron grey, dark leather brown, aged metal.
```

### Gear Overlay Assets to Generate

For MVP, generate one overlay per gear type in the game's craft system. Use the GDD craft tables for reference. Create at least these representative pieces:

**Head slot overlays** (top third of sprite):

| Filename | Gear Description |
|---|---|
| `overlay_helm_carapace.png` | Dark chitin half-helm, bone-plated brow |
| `overlay_helm_membrane.png` | Stretched hide hood, wrapped chin |
| `overlay_helm_auric.png` | Gold-tinted scale visor, open face |

**Chest slot overlays** (middle of sprite):

| Filename | Gear Description |
|---|---|
| `overlay_chest_carapace.png` | Layered chitin chest plate, irregular edges |
| `overlay_chest_ichor.png` | Hardened resin cuirass, dark amber colour |
| `overlay_chest_rot.png` | Decayed organic matter shaped into armour, deep green-grey |

**Arms slot overlays** (arm regions of sprite):

| Filename | Gear Description |
|---|---|
| `overlay_arms_membrane.png` | Wrapped hide vambraces, sinew-laced |
| `overlay_arms_carapace.png` | Chitin gauntlets, clawed fingertips |

**Weapon slot overlays** (right-side, weapon-carrying region):

| Filename | Gear Description |
|---|---|
| `overlay_weapon_boneblade.png` | Rough carved bone short blade |
| `overlay_weapon_clawspear.png` | Elongated claw mounted on a pole |
| `overlay_weapon_maulhook.png` | Heavy curved hook-maul, iron-headed |

**OffHand slot overlays** (left arm / shield side):

| Filename | Gear Description |
|---|---|
| `overlay_offhand_boneshield.png` | Bone and hide round shield, strapped |
| `overlay_offhand_ichorflask.png` | Small flask of ichor at the belt |

---

## Part 3: GearOverlayController.cs

**Path:** `Assets/_Game/Scripts/Core.UI/GearOverlayController.cs`

Attach this to each hunter token. It manages the 5 overlay child GameObjects.

```csharp
using System.Collections.Generic;
using UnityEngine;
using MnM.Core.Data;

namespace MnM.Core.UI
{
    public class GearOverlayController : MonoBehaviour
    {
        // Slot order → sorting order
        private static readonly Dictionary<string, int> SlotOrder = new()
        {
            { "Arms",    1 },
            { "Chest",   2 },
            { "Head",    3 },
            { "Weapon",  4 },
            { "OffHand", 5 },
        };

        private readonly Dictionary<string, SpriteRenderer> _overlays = new();

        private void Awake()
        {
            foreach (var kvp in SlotOrder)
                _overlays[kvp.Key] = CreateOverlayChild(kvp.Key, kvp.Value);
        }

        private SpriteRenderer CreateOverlayChild(string slot, int sortingOrder)
        {
            var child   = new GameObject($"Overlay_{slot}");
            child.transform.SetParent(transform);
            child.transform.localPosition = Vector3.zero;
            child.transform.localScale    = Vector3.one;

            var sr = child.AddComponent<SpriteRenderer>();
            sr.sortingOrder = sortingOrder;
            return sr;
        }

        /// <summary>
        /// Apply all currently equipped gear overlays from a HunterState.
        /// </summary>
        public void ApplyFromHunterState(HunterState hunter, GearSO[] allGear)
        {
            // Clear all slots first
            foreach (var sr in _overlays.Values)
                sr.sprite = null;

            if (hunter.equippedGearIds == null || allGear == null) return;

            foreach (var gearId in hunter.equippedGearIds)
            {
                GearSO gear = null;
                foreach (var g in allGear)
                    if (g != null && g.gearId == gearId) { gear = g; break; }

                if (gear == null || gear.overlaySprite == null) continue;

                SetSlot(gear.gearSlot, gear.overlaySprite);
            }
        }

        /// <summary>Set one gear slot's overlay sprite directly.</summary>
        public void SetSlot(string slot, Sprite sprite)
        {
            if (_overlays.TryGetValue(slot, out var sr))
                sr.sprite = sprite;
            else
                Debug.LogWarning($"[GearOverlay] Unknown slot: {slot}");
        }

        /// <summary>Clear one slot.</summary>
        public void ClearSlot(string slot)
        {
            if (_overlays.TryGetValue(slot, out var sr))
                sr.sprite = null;
        }

        /// <summary>Clear all slots.</summary>
        public void ClearAll()
        {
            foreach (var sr in _overlays.Values)
                sr.sprite = null;
        }

        /// <summary>
        /// Call this when facing direction changes — flip all overlays with the base sprite.
        /// </summary>
        public void SetFlipX(bool flipX)
        {
            foreach (var sr in _overlays.Values)
                sr.flipX = flipX;
        }
    }
}
```

---

## Part 4: Update HunterFacingController to Sync Overlays

In `HunterFacingController.cs`, add support for flipping overlays when facing west:

```csharp
private GearOverlayController _gearOverlay;

private void Awake()
{
    _renderer    = GetComponent<SpriteRenderer>();
    _gearOverlay = GetComponent<GearOverlayController>(); // on same GO
}

private void ApplySprite()
{
    if (_sprites == null || _renderer == null) return;

    bool flip = false;
    switch (_currentFacing)
    {
        case FacingDirection.South:
            _renderer.sprite = _sprites.south; break;
        case FacingDirection.North:
            _renderer.sprite = _sprites.north; break;
        case FacingDirection.East:
            _renderer.sprite = _sprites.east;  break;
        case FacingDirection.West:
            _renderer.sprite = _sprites.east;
            flip             = true;            break;
    }

    _renderer.flipX = flip;
    _gearOverlay?.SetFlipX(flip);  // Flip overlays too
}
```

---

## Part 5: Wiring into CombatScreenController Token Spawn

In `SpawnHunterToken()`:

```csharp
private GameObject SpawnHunterToken(HunterState hunter, Vector2Int gridPos)
{
    var tokenGO      = new GameObject($"Token_{hunter.hunterName}");
    var renderer     = tokenGO.AddComponent<SpriteRenderer>();
    var facing       = tokenGO.AddComponent<HunterFacingController>();
    var gearOverlay  = tokenGO.AddComponent<GearOverlayController>();
    var animTrigger  = tokenGO.AddComponent<HunterAnimationTrigger>();
    var animator     = tokenGO.AddComponent<Animator>();

    // ... (facing and animator wiring from 9-B and 9-C)

    // Apply gear overlays
    gearOverlay.ApplyFromHunterState(hunter, _allGear);

    tokenGO.transform.position = GridToWorldPos(gridPos);
    return tokenGO;
}
```

Add `[SerializeField] private GearSO[] _allGear;` to `CombatScreenController` and assign all gear SOs in Inspector.

---

## Part 6: Settlement Gear Panel — Live Preview

In the settlement gear panel, show the currently equipped overlays as a preview image. Since the settlement screen is UIToolkit (2D), composite the layers as stacked UIToolkit `VisualElement` background images.

Add a gear preview area in `SettlementScreenController`:

```csharp
private void RefreshGearPreview(HunterState hunter, GearSO[] allGear)
{
    var root    = _uiDocument.rootVisualElement;
    var preview = root.Q("gear-preview");
    if (preview == null) return;
    preview.Clear();

    // Base sprite
    var baseEl = MakePreviewLayer(GetBaseSprite(hunter.buildName));
    preview.Add(baseEl);

    if (hunter.equippedGearIds == null || allGear == null) return;

    // Gear overlays in slot order
    string[] slotOrder = { "Arms", "Chest", "Head", "Weapon", "OffHand" };
    foreach (var slot in slotOrder)
    {
        foreach (var gearId in hunter.equippedGearIds)
        {
            GearSO gear = null;
            foreach (var g in allGear)
                if (g != null && g.gearId == gearId) { gear = g; break; }

            if (gear == null || gear.gearSlot != slot || gear.overlaySprite == null) continue;

            var overlayEl = MakePreviewLayer(gear.overlaySprite);
            preview.Add(overlayEl);
        }
    }
}

private VisualElement MakePreviewLayer(Sprite sprite)
{
    var el = new VisualElement();
    el.style.position  = Position.Absolute;
    el.style.width     = 64;   // Scale up 2× for visibility (32px × 2)
    el.style.height    = 96;   // 48px × 2
    if (sprite != null)
        el.style.backgroundImage = new StyleBackground(sprite);
    el.style.backgroundScaleMode = new StyleEnum<ScaleMode>(ScaleMode.ScaleToFit);
    el.pickingMode     = PickingMode.Ignore;
    return el;
}

private Sprite GetBaseSprite(string buildName)
{
    // Load the south-facing base sprite from Resources
    string path = $"Art/Generated/Hunters/{buildName}/hunter_{buildName.ToLower()}_south";
    return Resources.Load<Sprite>(path);
}
```

Add in the settlement UXML gear panel:

```xml
<!-- Gear Preview -->
<ui:VisualElement name="gear-preview"
    style="position:relative; width:64px; height:96px;
           margin-right:20px; flex-shrink:0;" />
```

---

## Verification Test

- [ ] Spawn Aldric (Aethel build) with no gear → base south sprite only, no overlays
- [ ] Equip `overlay_helm_carapace.png` in Head slot → helmet overlay appears on token
- [ ] Equip chest piece → chest overlay appears independently
- [ ] Equip weapon → weapon overlay appears on correct (right) side
- [ ] Remove helmet → Head overlay disappears, chest and weapon remain
- [ ] Move Aldric left (West) → base sprite and all overlays flip together
- [ ] Move Aldric right (East) → flip resets on both base and overlays
- [ ] Equip gear in Settlement → gear preview updates immediately
- [ ] Open Combat with gear equipped → overlays appear from initial spawn
- [ ] Two hunters with different gear → overlays don't cross between tokens
- [ ] GearSO with no `overlaySprite` → no overlay drawn, no null ref error

---

## Next Session

**File:** `_Docs/Stage_09/STAGE_09_E.md`
**Covers:** Gear Grid Adjacency & Link Bonus Logic — detecting when gear pieces from the same craft set are placed in adjacent slots in the hunter's gear grid, calculating the adjacency bonus, and displaying the bonus in the settlement gear panel
