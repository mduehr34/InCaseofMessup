<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 9-B | 4-Way Directional Sprites & Facing Logic
Status: Stage 9-A complete. Injury/Scar/Disorder/FightingArt SOs done.
Task: Generate directional sprites for all 8 hunter build types
(North, South, East — West is East flipped). Wire a facing system
into HunterTokenController so tokens automatically face the correct
direction when placed or moved. Implement the same facing system
for the monster token. The combat grid already exists — this
session only adds sprite direction logic on top of it.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_09/STAGE_09_B.md
- Assets/_Game/Scripts/Core.UI/CombatScreenController.cs

Then confirm:
- Hunter tokens are SpriteRenderer GameObjects on the combat grid
- Each build type needs 3 sprite assets: _south, _north, _east
  (west = east with flipX = true)
- Facing updates on every move, including the initial placement
- Monster facing is tracked separately (always faces the aggro target)
- What you will NOT build (diagonal facing, run animation — post-MVP)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 9-B: 4-Way Directional Sprites & Facing Logic

**Resuming from:** Stage 9-A complete — all lifecycle card SOs and assets created
**Done when:** All 8 hunter builds have N/S/E/W sprites; tokens face the correct direction on the grid; monster token faces the aggro target
**Commit:** `"9B: 4-way directional sprites and facing logic for all hunter builds"`
**Next session:** STAGE_09_C.md

---

## The 8 Hunter Build Types

From the GDD (confirmed in Stage 8-C character creation):

| Build | Sex | Description |
|---|---|---|
| Aethel | M | Lean, light armour, hood, carrying a short blade |
| Beorn | M | Stocky, fur-lined coat, axe across back |
| Cyne | M | Tall, wrapped cloth, dual short weapons at hips |
| Duna | M | Broad, layered armour, greatspear in hand |
| Eira | F | Slight, long bow across back, hunter's cloak |
| Freya | F | Medium build, twin daggers at belt, braided hair |
| Gerd | F | Heavy-set, shield strapped to back, shortsword |
| Hild | F | Wiry, long spear, leather wraps on arms |

---

## Part 1: Generate Sprites

Use CoPlay `generate_or_edit_images` for each hunter build × 3 directions.

**Save path:** `Assets/_Game/Art/Generated/Hunters/{BuildName}/`

**Standard prompt template** (fill in `{BUILD_DESCRIPTION}` and `{DIRECTION}`):

```
16-bit pixel art hunter token. {DIRECTION} view.
{BUILD_DESCRIPTION}.
Dark muted palette: ash grey armour, bone white highlights,
dried blood accent on weapon. Transparent background.
Size: 32×48 pixels. Point art, no anti-aliasing.
Single character, full body visible, standing idle pose.
Style consistent with dark survival settlement game.
```

**Direction-specific pose guidance:**

- **South** (facing camera): front view, face visible, weapon at side or in front
- **North** (facing away): back view, weapon holstered/on back
- **East** (facing right): side view, profile, weapon toward right side

### Aethel (M)

| Filename | Direction | Prompt Note |
|---|---|---|
| `hunter_aethel_south.png` | South | Lean male, grey hood up, short blade held at side |
| `hunter_aethel_north.png` | North | Hood seen from behind, blade across back |
| `hunter_aethel_east.png` | East | Profile left-to-right, blade pointing right |

### Beorn (M)

| Filename | Direction | Prompt Note |
|---|---|---|
| `hunter_beorn_south.png` | South | Stocky male, fur collar, axe held in right hand |
| `hunter_beorn_north.png` | North | Broad shoulders from behind, axe across back |
| `hunter_beorn_east.png` | East | Profile, axe swung forward, fur visible on sides |

### Cyne (M)

| Filename | Direction | Prompt Note |
|---|---|---|
| `hunter_cyne_south.png` | South | Tall male, cloth wraps, both short weapons visible at hips |
| `hunter_cyne_north.png` | North | Tall silhouette from behind, wraps trailing |
| `hunter_cyne_east.png` | East | Profile, one weapon visible at hip |

### Duna (M)

| Filename | Direction | Prompt Note |
|---|---|---|
| `hunter_duna_south.png` | South | Broad male, layered armour, greatspear planted in front |
| `hunter_duna_north.png` | North | Wide armoured back, spear tip visible above shoulder |
| `hunter_duna_east.png` | East | Profile, spear horizontal pointing right |

### Eira (F)

| Filename | Direction | Prompt Note |
|---|---|---|
| `hunter_eira_south.png` | South | Slight female, hunter's cloak, longbow across back |
| `hunter_eira_north.png` | North | Cloak from behind, bow stave visible |
| `hunter_eira_east.png` | East | Profile, cloak edge trailing, bow tip pointing up-right |

### Freya (F)

| Filename | Direction | Prompt Note |
|---|---|---|
| `hunter_freya_south.png` | South | Medium female, braids, both daggers at belt |
| `hunter_freya_north.png` | North | Braids seen from behind, daggers at sides |
| `hunter_freya_east.png` | East | Profile, one dagger visible, braids trailing |

### Gerd (F)

| Filename | Direction | Prompt Note |
|---|---|---|
| `hunter_gerd_south.png` | South | Heavy-set female, round shield on back strap, shortsword at hip |
| `hunter_gerd_north.png` | North | Shield prominent from behind, stocky figure |
| `hunter_gerd_east.png` | East | Profile, shield edge visible on back, sword at side |

### Hild (F)

| Filename | Direction | Prompt Note |
|---|---|---|
| `hunter_hild_south.png` | South | Wiry female, long spear held upright, leather arm wraps |
| `hunter_hild_north.png` | North | Spear tip above head from behind, lean silhouette |
| `hunter_hild_east.png` | East | Profile, spear angled right and forward |

**Import settings for all sprites:**
- Texture Type: Sprite (2D and UI)
- Filter Mode: Point (No Filter)
- Pixels Per Unit: 16
- Sprite Mode: Single
- Compression: None (small files, no loss needed)

---

## Part 2: DirectionalSprite Helper

**Path:** `Assets/_Game/Scripts/Core.Data/DirectionalSprite.cs`

This data class holds the three sprites for one build direction set. It is used as a serialized field in inspector-assignable arrays.

```csharp
using UnityEngine;

namespace MnM.Core.Data
{
    [System.Serializable]
    public class DirectionalSprite
    {
        public string buildName;     // e.g. "Aethel"
        public Sprite south;         // Facing camera
        public Sprite north;         // Facing away
        public Sprite east;          // Facing right (west = east + flipX)
    }
}
```

---

## Part 3: FacingDirection Enum

**Path:** `Assets/_Game/Scripts/Core.Data/FacingDirection.cs`

```csharp
namespace MnM.Core.Data
{
    public enum FacingDirection
    {
        South,   // Default / toward camera
        North,
        East,
        West     // Mirror of East
    }
}
```

---

## Part 4: HunterFacingController.cs

**Path:** `Assets/_Game/Scripts/Core.UI/HunterFacingController.cs`

Attach this to each hunter token GameObject on the combat grid.

```csharp
using UnityEngine;
using MnM.Core.Data;

namespace MnM.Core.UI
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class HunterFacingController : MonoBehaviour
    {
        [SerializeField] private string _buildName;
        // Assigned at runtime by CombatScreenController

        private SpriteRenderer _renderer;
        private DirectionalSprite _sprites;

        private FacingDirection _currentFacing = FacingDirection.South;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// Called by CombatScreenController when spawning this token.
        /// Provide the build's directional sprite set.
        /// </summary>
        public void Initialize(DirectionalSprite sprites, FacingDirection initialFacing = FacingDirection.South)
        {
            _sprites = sprites;
            _buildName = sprites.buildName;
            SetFacing(initialFacing);
        }

        /// <summary>
        /// Call this whenever the hunter moves to a new grid cell.
        /// Provide the old and new grid positions to determine direction.
        /// </summary>
        public void UpdateFacingFromMove(Vector2Int from, Vector2Int to)
        {
            if (to == from) return;

            Vector2Int delta = to - from;

            FacingDirection facing;
            if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
            {
                // Horizontal movement dominates
                facing = delta.x > 0 ? FacingDirection.East : FacingDirection.West;
            }
            else
            {
                // Vertical movement dominates
                facing = delta.y < 0 ? FacingDirection.South : FacingDirection.North;
                // Note: in Unity grid, positive Y = up = North.
                // If your grid uses screen coords (positive Y = down), invert this.
            }

            SetFacing(facing);
        }

        public void SetFacing(FacingDirection dir)
        {
            _currentFacing = dir;
            ApplySprite();
        }

        public FacingDirection CurrentFacing => _currentFacing;

        private void ApplySprite()
        {
            if (_sprites == null || _renderer == null) return;

            _renderer.flipX = false;

            switch (_currentFacing)
            {
                case FacingDirection.South:
                    _renderer.sprite = _sprites.south;
                    break;
                case FacingDirection.North:
                    _renderer.sprite = _sprites.north;
                    break;
                case FacingDirection.East:
                    _renderer.sprite = _sprites.east;
                    break;
                case FacingDirection.West:
                    _renderer.sprite = _sprites.east;   // Reuse east
                    _renderer.flipX  = true;             // Mirror it
                    break;
            }
        }
    }
}
```

---

## Part 5: Monster Facing Logic

The monster always faces the hunter it is currently targeting (the aggro token holder).

Add to `CombatScreenController` or `CombatManager`:

```csharp
private void UpdateMonsterFacing(Vector2Int monsterPos, Vector2Int aggroTargetPos)
{
    if (_monsterFacingController == null) return;

    Vector2Int delta = aggroTargetPos - monsterPos;

    FacingDirection facing;
    if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
        facing = delta.x > 0 ? FacingDirection.East : FacingDirection.West;
    else
        facing = delta.y < 0 ? FacingDirection.South : FacingDirection.North;

    _monsterFacingController.SetFacing(facing);
}
```

The monster token uses the same `HunterFacingController` component (rename it `TokenFacingController` if cleaner) with its own `DirectionalSprite` set.

---

## Part 6: Wiring into CombatScreenController

In `CombatScreenController`, add a serialized array of all directional sprite sets:

```csharp
[SerializeField] private DirectionalSprite[] _hunterSpriteSets;
// Assign all 8 build types in the Inspector
```

When spawning a hunter token:

```csharp
private GameObject SpawnHunterToken(HunterState hunter, Vector2Int gridPos)
{
    var tokenGO  = new GameObject($"Token_{hunter.hunterName}");
    var renderer = tokenGO.AddComponent<SpriteRenderer>();
    var facing   = tokenGO.AddComponent<HunterFacingController>();

    // Find the matching sprite set
    DirectionalSprite sprites = null;
    foreach (var set in _hunterSpriteSets)
        if (set.buildName == hunter.buildName) { sprites = set; break; }

    if (sprites != null)
        facing.Initialize(sprites, FacingDirection.South);
    else
        Debug.LogWarning($"[Facing] No sprite set found for build: {hunter.buildName}");

    // Position in world space
    tokenGO.transform.position = GridToWorldPos(gridPos);
    return tokenGO;
}
```

When a hunter moves:

```csharp
private void MoveHunterToken(GameObject tokenGO, Vector2Int from, Vector2Int to)
{
    var facing = tokenGO.GetComponent<HunterFacingController>();
    facing?.UpdateFacingFromMove(from, to);

    // Then animate movement (from Stage 8-K)
    _combatAnim.AnimateMove(tokenGO, GridToWorldPos(to));
}
```

---

## Part 7: GridToWorldPos Helper

If not already present in `CombatScreenController`:

```csharp
private const float CellSize = 1.0f;   // World units per grid cell

private Vector3 GridToWorldPos(Vector2Int gridPos)
{
    // Adjust the offset so (0,0) maps to the bottom-left of your grid
    return new Vector3(gridPos.x * CellSize, gridPos.y * CellSize, 0f);
}
```

Adjust `CellSize` and the base offset to match your actual grid layout.

---

## Verification Test

- [ ] All 8 × 3 = 24 directional sprites exist in `Assets/_Game/Art/Generated/Hunters/`
- [ ] Sprites are 32×48, Point filter, PPU 16
- [ ] West sprites are NOT separate assets — they reuse East with flipX in code
- [ ] Place Aethel on grid facing South → south sprite visible, correct colours
- [ ] Move Aethel right (East) → east sprite shows, no flipX
- [ ] Move Aethel left (West) → east sprite shows with flipX = true (mirrored)
- [ ] Move Aethel up (North) → north sprite shows (back view)
- [ ] Move Aethel down (South) → south sprite shows (front view)
- [ ] Do the same for at least one female build (Eira or Freya)
- [ ] Monster token faces Aldric when Aldric holds the aggro token
- [ ] Aggro token changes to Beorn → monster visually turns to face Beorn
- [ ] Collapsed hunter token stays on its last facing (no facing resets on collapse)
- [ ] No NullReferenceException if a build name doesn't match any sprite set

---

## Next Session

**File:** `_Docs/Stage_09/STAGE_09_C.md`
**Covers:** Character Animator Controllers — building Unity Animator state machines for all 8 hunter builds with idle, walk, attack, and hit-react animation states driven by code triggers (not input)
