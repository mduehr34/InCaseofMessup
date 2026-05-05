<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 8-S | Combat Terrain — Obstacles and Bonus Squares
Status: Stage 8-K complete. Hunter movement, occupancy, and
once-per-turn movement limit all working.
Task: Extend GridManager with terrain features: static obstacle
cells that block movement, and bonus terrain cells that grant
stat modifiers (accuracy, defense) to hunters standing in them.
Define terrain via a new TerrainCellSO ScriptableObject, place
terrain at combat-start via CombatStateFactory or a
TerrainSetupSO on the monster, and render terrain tints on the
grid via two new CSS classes.

Read these files before doing anything:
- .cursorrules
- CLAUDE.md
- _Docs/Stage_08/STAGE_08_S.md
- Assets/_Game/Scripts/Core.Systems/GridManager.cs
- Assets/_Game/Scripts/Core.Systems/IGridManager.cs
- Assets/_Game/Scripts/Core.Data/CombatState.cs
- Assets/_Game/Scripts/Core.Data/DataStructs.cs
- Assets/_Game/Scripts/Core.Systems/CombatManager.cs
- Assets/_Game/Scripts/Core.UI/CombatScreenController.cs

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 8-S: Combat Terrain — Obstacles and Bonus Squares

**Resuming from:** Stage 8-K complete — hunter movement working
**Done when:** Static obstacle cells block movement; bonus terrain cells grant hunter stat modifiers; both types render as distinct grid tints; terrain is defined in a ScriptableObject and applied at combat start
**Commit:** `"8S: Combat terrain — obstacle cells and bonus terrain squares"`
**Next session:** STAGE_08_L.md (or next in sequence)

---

## What Already Exists

`GridManager` already supports two special cell types:
- `IsDenied(cell)` — blocks movement entirely; used for Spear zones (duration-based)
- `IsMarrowSink(cell)` — permanent special cells with their own mechanic

The new terrain system adds two **static, combat-wide** cell types:
- **Obstacle** — permanently impassable; blocks movement same as denied, but defined by level layout not card effects
- **Bonus terrain** — passable; grants a stat modifier to any hunter standing in the cell

Obstacles use the existing `IsDenied` infrastructure. Bonus terrain needs a new query and resolution path.

---

## Part 1: TerrainCellSO — Data Definition

Create `Assets/_Game/Scripts/Core.Data/TerrainCellSO.cs`:

```csharp
using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/Terrain/TerrainCell")]
    public class TerrainCellSO : ScriptableObject
    {
        [Header("Identity")]
        public string terrainId;            // Unique key e.g. "HighGround", "BoneAsh"
        public string displayName;

        [Header("Type")]
        public TerrainType terrainType;     // Obstacle or Bonus

        [Header("Bonus Modifiers (Bonus type only)")]
        public int accuracyBonus;           // Added to hunter accuracy while standing here
        public int defenseBonus;            // Subtracted from incoming Force Check rolls vs this hunter

        [Header("Visual")]
        public string cssClass;             // e.g. "grid-cell--terrain-high" or "grid-cell--terrain-ash"
        [TextArea] public string flavourText;
    }

    public enum TerrainType { Obstacle, Bonus }
}
```

---

## Part 2: TerrainSetupSO — Per-Combat Layout

Create `Assets/_Game/Scripts/Core.Data/TerrainSetupSO.cs`:

```csharp
using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/Terrain/TerrainSetup")]
    public class TerrainSetupSO : ScriptableObject
    {
        public TerrainEntry[] entries;

        [System.Serializable]
        public struct TerrainEntry
        {
            public TerrainCellSO terrain;
            public int gridX;
            public int gridY;
        }
    }
}
```

A `TerrainSetupSO` asset is assigned per-monster (or per-arena) in the Inspector and applied at combat start.

---

## Part 3: TerrainCell — Runtime State

Add to `CombatState.cs` (inside the `GridState` class, or as a new top-level serializable struct):

```csharp
[Serializable]
public struct TerrainCellState : IJsonSerializable
{
    public int x;
    public int y;
    public string terrainId;    // Matches TerrainCellSO.terrainId
    public TerrainType terrainType;
    public int accuracyBonus;
    public int defenseBonus;
    public string cssClass;
}
```

Add to `GridState`:

```csharp
public TerrainCellState[] terrainCells;
```

---

## Part 4: IGridManager — New Terrain Queries

**Stop and confirm before adding these methods** — adding to `IGridManager` requires interface approval per the clarification protocol. Expected additions:

```csharp
void PlaceTerrain(TerrainCellState cell);
bool IsTerrain(Vector2Int cell);
TerrainCellState? GetTerrain(Vector2Int cell);
```

`PlaceTerrain` for Obstacle type should also call `SetDenied` internally so movement blocking is automatic.

---

## Part 5: GridManager — Terrain Storage and Queries

Add a `Dictionary<string, TerrainCellState> _terrainCells` field (key: `"x,y"`).

Implement the three interface methods:

```csharp
public void PlaceTerrain(TerrainCellState cell)
{
    _terrainCells[Key(cell.x, cell.y)] = cell;
    if (cell.terrainType == TerrainType.Obstacle)
        SetDenied(new Vector2Int(cell.x, cell.y), true, int.MaxValue);
    Debug.Log($"[Grid] Terrain placed: {cell.terrainId} at ({cell.x},{cell.y})");
}

public bool IsTerrain(Vector2Int cell) =>
    _terrainCells.ContainsKey(Key(cell));

public TerrainCellState? GetTerrain(Vector2Int cell) =>
    _terrainCells.TryGetValue(Key(cell), out var t) ? t : (TerrainCellState?)null;
```

---

## Part 6: Apply Terrain Bonuses in CardResolver

In `CardResolver.Resolve()`, before computing the Precision Check roll:

```csharp
// Terrain bonus — hunter standing on bonus terrain gets accuracy modifier
var terrain = gridManager?.GetTerrain(new Vector2Int(hunter.gridX, hunter.gridY));
int terrainAccuracyBonus = terrain?.terrainType == TerrainType.Bonus
    ? terrain.Value.accuracyBonus : 0;
int terrainDefenseBonus  = 0; // Applied when hunter is the target — see TryPlayCard
```

And pass `terrainAccuracyBonus` into the d10 roll.

For defense bonus: in `CombatManager.TryPlayCard`, when a monster card deals damage to a hunter, check `GetTerrain` on the hunter's cell and reduce incoming damage by `defenseBonus`.

---

## Part 7: Wire Terrain at Combat Start

Add `[SerializeField] private TerrainSetupSO _terrainSetup;` to `CombatTestBootstrapper` (or to `CombatManager`).

In `CombatManager.StartCombat`, after occupant registration:

```csharp
if (_terrainSetup != null && grid != null)
{
    foreach (var entry in _terrainSetup.entries)
    {
        var cell = new TerrainCellState
        {
            x             = entry.gridX,
            y             = entry.gridY,
            terrainId     = entry.terrain.terrainId,
            terrainType   = entry.terrain.terrainType,
            accuracyBonus = entry.terrain.accuracyBonus,
            defenseBonus  = entry.terrain.defenseBonus,
            cssClass      = entry.terrain.cssClass,
        };
        grid.PlaceTerrain(cell);
    }
    // Persist terrain in GridState for save/load round-trips
    initialState.grid.terrainCells = System.Array.ConvertAll(
        _terrainSetup.entries, e => /* same construction above */);
}
```

---

## Part 8: UI — Render Terrain Tints

In `CombatScreenController.RefreshGrid()`, inside the per-cell loop after the movable class:

```csharp
var terrain = (_gridManager as IGridManager)?.GetTerrain(pos);
if (terrain.HasValue)
    cell.EnableInClassList(terrain.Value.cssClass, true);
```

In `CombatScreenController.BuildGrid()`, reset terrain classes per-cell in `RefreshGrid` (add the CSS class name to the reset block at the top of the loop, or use `EnableInClassList(cssClass, false)` via the terrain state).

---

## Part 9: CSS — Terrain Tints

Add to `combat-screen.uss`:

```css
/* Obstacle — impassable rock / rubble */
.grid-cell--terrain-obstacle {
    background-color: rgba(80, 55, 30, 0.45);
    border-color:     rgba(120, 85, 45, 0.70);
    border-width:     2px;
}

/* High ground / vantage — accuracy bonus */
.grid-cell--terrain-high {
    background-color: rgba(60, 80, 110, 0.30);
    border-color:     rgba(90, 130, 180, 0.55);
    border-width:     1px;
}

/* Ash / bone field — defense bonus */
.grid-cell--terrain-ash {
    background-color: rgba(90, 75, 60, 0.25);
    border-color:     rgba(140, 120, 90, 0.50);
    border-width:     1px;
}
```

---

## Mock Assets to Create

After implementation, create two mock `TerrainCellSO` assets for testing:
- `Assets/_Game/Data/Terrain/Terrain_HighGround.asset` — accuracyBonus=1, cssClass="grid-cell--terrain-high"
- `Assets/_Game/Data/Terrain/Terrain_BoneAsh.asset` — defenseBonus=1, cssClass="grid-cell--terrain-ash"

And one `TerrainSetupSO`:
- `Assets/_Game/Data/Terrain/TerrainSetup_GauntArena.asset` — place HighGround at (5,4) and (16,4), BoneAsh at (11,8)

Assign `TerrainSetup_GauntArena` to the `CombatTestBootstrapper._terrainSetup` field.

---

## Verification Test

- [ ] Obstacle cell at defined coordinates shows brown tint; hunter movement range excludes it
- [ ] Hunter cannot move onto an obstacle cell (TryMoveHunter rejects it)
- [ ] Bonus terrain cell shows correct tint (blue for high ground, tan for ash)
- [ ] Hunter standing on high ground gets +1 accuracy on Precision Check — confirm via d10 log
- [ ] Hunter standing on ash terrain reduces incoming Force Check damage by defenseBonus — confirm via d10 log
- [ ] Terrain persists across RefreshGrid calls (tints survive damage events)
- [ ] Console: `[Grid] Terrain placed: HighGround at (5,4)` at combat start
- [ ] No terrain tints appear during Monster Phase or Settlement transitions
- [ ] Save/load round-trip: terrain cells survive `GridState` JSON serialization

---

## Interface Approval Required

Before implementing Part 4, post the full `IGridManager` diff for review. The three new methods (`PlaceTerrain`, `IsTerrain`, `GetTerrain`) must be approved before writing implementation code.
