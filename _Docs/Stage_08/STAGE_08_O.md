<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude Code to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 8-O | Hunter Deployment Phase — Per-Monster Spawn Zones
Status: Stage 8-N complete. New combat runtime wired — BehaviorDeck,
WoundDeck, wound resolution, and MonsterAI sub-phase execution all working.
Task: Replace hardcoded hunter grid positions in
CombatStateFactory with a per-monster spawn zone. Add a
Deployment phase before VitalityPhase where the player clicks
cells inside the highlighted zone to place each hunter one at a
time. Combat does not start until all hunters are placed.

Read these files before doing anything:
- CLAUDE.md
- _Docs/Stage_08/STAGE_08_O.md
- Assets/_Game/Scripts/Core.Data/CombatState.cs
- Assets/_Game/Scripts/Core.Data/CombatStateFactory.cs
- Assets/_Game/Scripts/Core.Systems/CombatManager.cs
- Assets/_Game/Scripts/Core.UI/CombatScreenController.cs
- Assets/_Game/Scripts/Core.Systems/GridManager.cs

Then confirm:
- HunterCombatState has gridX/gridY fields (set to -1 for unplaced)
- CombatPhase enum does NOT yet have a DeploymentPhase value
- GridManager already supports IsDenied() and PlaceOccupant()
- CombatStateFactory hardcodes Aldric at (5,8) — this will be
  replaced with gridX=-1, gridY=-1 (unplaced)
- SpawnZoneSO is a new ScriptableObject (does not exist yet)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 8-O: Hunter Deployment Phase — Per-Monster Spawn Zones

**Resuming from:** Stage 8-N complete — new combat runtime implemented; BehaviorDeck/WoundDeck wrappers in place; MonsterAI uses sub-phase execution flow
**Done when:** A spawn zone is defined per-monster in a ScriptableObject; combat begins with a Deployment phase where the player clicks zone cells to place each hunter; once all hunters are placed VitalityPhase starts normally; hardcoded spawn positions are removed from CombatStateFactory
**Commit:** `"8O: Hunter deployment phase — per-monster spawn zones, player placement"`
**Next session:** STAGE_08_P.md

---

## What's Changing and Why

Currently `CombatStateFactory` hardcodes Aldric at `(5,8)` and the monster at `(12,7)`. This works for a single-hunter test but breaks down as soon as the party has multiple hunters or a different monster is loaded — the team always spawns in a straight line with no gameplay input from the player.

The fix has three parts:
1. A `SpawnZoneSO` ScriptableObject defines a rectangular (or explicit-cell) area where hunters may be placed for a specific monster.
2. `CombatManager` gets a `DeploymentPhase` and a `TryPlaceHunter()` method.
3. `CombatScreenController` shows the spawn zone as a tinted grid region and handles click-to-place during `DeploymentPhase`.

Monster position and footprint stay hardcoded in `CombatStateFactory` / `MonsterSO` for now — this session only changes hunter spawning.

---

## Part 1: SpawnZoneSO — Data Definition

**Stop first** — this adds a new ScriptableObject class. Per the clarification protocol, confirm this doesn't duplicate anything in an existing STAGE file before proceeding.

Create `Assets/_Game/Scripts/Core.Data/SpawnZoneSO.cs`:

```csharp
using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/Combat/SpawnZone")]
    public class SpawnZoneSO : ScriptableObject
    {
        [Header("Zone Definition")]
        public SpawnZoneShape shape;

        [Header("Rectangle (if shape = Rect)")]
        public int rectX;       // Left column (inclusive)
        public int rectY;       // Top row (inclusive)
        public int rectWidth;   // Number of columns
        public int rectHeight;  // Number of rows

        [Header("Explicit Cells (if shape = Explicit)")]
        public Vector2Int[] cells;

        public bool ContainsCell(Vector2Int cell)
        {
            if (shape == SpawnZoneShape.Rect)
                return cell.x >= rectX && cell.x < rectX + rectWidth &&
                       cell.y >= rectY && cell.y < rectY + rectHeight;

            foreach (var c in cells)
                if (c == cell) return true;
            return false;
        }

        public Vector2Int[] GetAllCells()
        {
            if (shape == SpawnZoneShape.Explicit)
                return cells ?? new Vector2Int[0];

            var list = new System.Collections.Generic.List<Vector2Int>();
            for (int x = rectX; x < rectX + rectWidth; x++)
            for (int y = rectY; y < rectY + rectHeight; y++)
                list.Add(new Vector2Int(x, y));
            return list.ToArray();
        }
    }

    public enum SpawnZoneShape { Rect, Explicit }
}
```

---

## Part 2: Add SpawnZone Reference to MonsterSO

Open `Assets/_Game/Scripts/Core.Data/MonsterSO.cs` (or wherever `MonsterSO` is defined).

Add one field to `MonsterSO`:

```csharp
[Header("Combat Setup")]
[SerializeField] public SpawnZoneSO hunterSpawnZone;
```

This field is optional — if null, `CombatManager` falls back to legacy positions (gridX/gridY already set in `CombatState`). This keeps backwards compatibility with the existing mock.

---

## Part 3: HunterCombatState — Unplaced Sentinel

Open `Assets/_Game/Scripts/Core.Data/CombatState.cs`.

Add one field to `HunterCombatState`:

```csharp
public bool isUnplaced;   // true during DeploymentPhase — hunter is not yet on the grid
```

In `CombatStateFactory.BuildMockCombatState()`, change Aldric's initialization:

```csharp
// Before: gridX = 5, gridY = 8
gridX      = -1,
gridY      = -1,
isUnplaced = true,
```

Remove the hunter occupant registration block from `CombatManager.StartCombat` for unplaced hunters:

```csharp
foreach (var hunter in initialState.hunters)
{
    if (hunter.isCollapsed || hunter.isUnplaced) continue;   // ← add isUnplaced guard
    grid.PlaceOccupant(...);
}
```

---

## Part 4: CombatPhase Enum — Add DeploymentPhase

Open wherever `CombatPhase` is defined (likely `CombatState.cs` or `ICombatManager.cs`).

Add the new value **before** `VitalityPhase`:

```csharp
public enum CombatPhase
{
    DeploymentPhase,    // ← new: player places hunters before round 1
    VitalityPhase,
    HunterPhase,
    BehaviorRefresh,
    MonsterPhase,
}
```

**Interface approval required** — `CombatPhase` is used by `ICombatManager`. Confirm no tests or external systems hard-switch on the ordinal values before adding.

---

## Part 5: CombatManager — DeploymentPhase and TryPlaceHunter

In `CombatManager.StartCombat`, change the initial phase:

```csharp
// Before: CurrentPhase = CombatPhase.VitalityPhase;
bool anyUnplaced = System.Array.Exists(initialState.hunters, h => h.isUnplaced);
CurrentPhase = anyUnplaced ? CombatPhase.DeploymentPhase : CombatPhase.VitalityPhase;
```

Add `TryPlaceHunter()` as a new public method on `CombatManager` (not on `ICombatManager` — this is a setup-only call the UI makes directly):

```csharp
public bool TryPlaceHunter(string hunterId, Vector2Int cell, SpawnZoneSO zone)
{
    var grid = _gridManager as IGridManager;
    if (CurrentPhase != CombatPhase.DeploymentPhase)
    {
        Debug.LogWarning("[Combat] TryPlaceHunter called outside DeploymentPhase");
        return false;
    }

    var hunter = GetHunter(hunterId);
    if (hunter == null || !hunter.isUnplaced)
    {
        Debug.LogWarning($"[Combat] TryPlaceHunter: {hunterId} not found or already placed");
        return false;
    }

    if (zone != null && !zone.ContainsCell(cell))
    {
        Debug.LogWarning($"[Combat] TryPlaceHunter: ({cell.x},{cell.y}) is outside spawn zone");
        return false;
    }

    if (grid != null && grid.IsOccupied(cell))
    {
        Debug.LogWarning($"[Combat] TryPlaceHunter: ({cell.x},{cell.y}) is occupied");
        return false;
    }

    hunter.gridX      = cell.x;
    hunter.gridY      = cell.y;
    hunter.isUnplaced = false;

    grid?.PlaceOccupant(new GridOccupant
    {
        occupantId = hunter.hunterId,
        isHunter   = true,
        footprintW = 1,
        footprintH = 1,
        gridX      = cell.x,
        gridY      = cell.y,
    }, cell);

    Debug.Log($"[Combat] {hunter.hunterName} placed at ({cell.x},{cell.y})");

    // If all hunters are placed, advance to VitalityPhase automatically
    if (!System.Array.Exists(CurrentState.hunters, h => h.isUnplaced))
    {
        Debug.Log("[Combat] All hunters placed — advancing to VitalityPhase");
        CurrentPhase = CombatPhase.VitalityPhase;
        CurrentState.currentPhase = CurrentPhase.ToString();
        OnPhaseChanged?.Invoke(CurrentPhase);
    }
    else
    {
        OnPhaseChanged?.Invoke(CurrentPhase); // Refresh UI to show next unplaced hunter
    }

    return true;
}
```

Update `AdvancePhase()` to handle the new enum value (add a no-op or warning case):

```csharp
case CombatPhase.DeploymentPhase:
    Debug.LogWarning("[Combat] AdvancePhase called during DeploymentPhase — use TryPlaceHunter");
    break;
```

---

## Part 6: CombatScreenController — Deployment UI

Add the spawn zone reference:

```csharp
[SerializeField] private SpawnZoneSO _spawnZone;
```

Add a field to track which hunter is currently being placed:

```csharp
private string _deployingHunterId = null;
```

Add a helper to get the current unplaced hunter:

```csharp
private HunterCombatState GetNextUnplacedHunter()
{
    var state = _combatManager?.CurrentState;
    if (state == null) return null;
    return System.Array.Find(state.hunters, h => h.isUnplaced && !h.isCollapsed);
}
```

In `OnPhaseChanged()`, add the deployment case alongside the existing phase handling:

```csharp
if (phase == CombatPhase.DeploymentPhase)
{
    // Disable End Turn during deployment — no hunter turn to end
    if (_endTurnBtn != null) _endTurnBtn.SetEnabled(false);

    var next = GetNextUnplacedHunter();
    _deployingHunterId = next?.hunterId;
    ShowSpawnZone();

    if (_phaseLabel != null)
        _phaseLabel.text = next != null
            ? $"DEPLOY: Place {next.hunterName}"
            : "DEPLOY: All placed";
    return;
}
```

Add `ShowSpawnZone()` and `ClearSpawnZone()`:

```csharp
private void ShowSpawnZone()
{
    if (_spawnZone == null || _gridCells == null) return;
    foreach (var cell in _spawnZone.GetAllCells())
    {
        if (cell.x < 0 || cell.x >= 22 || cell.y < 0 || cell.y >= 16) continue;
        var el = _gridCells[cell.x, cell.y];
        if (el != null) el.EnableInClassList("grid-cell--spawn", true);
    }
}

private void ClearSpawnZone()
{
    if (_gridCells == null) return;
    for (int x = 0; x < 22; x++)
    for (int y = 0; y < 16; y++)
    {
        _gridCells[x, y]?.EnableInClassList("grid-cell--spawn", false);
    }
}
```

In `OnGridCellClicked()`, add a deployment branch **before** the card targeting check:

```csharp
// ── Deployment mode ────────────────────────────────────────────────
if (_combatManager?.CurrentPhase == CombatPhase.DeploymentPhase)
{
    if (_deployingHunterId == null)
    {
        Debug.Log("[CombatUI] No hunter awaiting placement");
        return;
    }

    var dest = new Vector2Int(x, y);
    bool placed = _combatManager.TryPlaceHunter(_deployingHunterId, dest, _spawnZone);
    if (placed)
    {
        ClearSpawnZone();
        var next = GetNextUnplacedHunter();
        _deployingHunterId = next?.hunterId;
        if (next != null) ShowSpawnZone();
        RefreshAll();
    }
    else
    {
        Debug.Log($"[CombatUI] Cannot place here: ({x},{y}) — outside zone or occupied");
    }
    RefreshGrid();
    return;
}
```

In `RefreshGrid()`, keep the spawn zone tint alive through grid refreshes (same pattern as `_validMoveCells`):

```csharp
if (_combatManager?.CurrentPhase == CombatPhase.DeploymentPhase && _spawnZone != null)
    cell.EnableInClassList("grid-cell--spawn",
        _spawnZone.ContainsCell(new Vector2Int(x, y)));
else
    cell.EnableInClassList("grid-cell--spawn", false);
```

---

## Part 7: CSS — Spawn Zone Tint

Add to `combat-screen.uss`:

```css
/* Deployment spawn zone — teal-blue, distinct from movement green */
.grid-cell--spawn {
    background-color: rgba(20, 100, 130, 0.35);
    border-color:     rgba(40, 160, 200, 0.65);
    border-width:     1px;
}
```

---

## Part 8: Mock Asset — SpawnZoneSO for Gaunt Standard

After implementation, create the mock asset in the Editor:

**Path:** `Assets/_Game/Data/Combat/SpawnZone_GauntStandard.asset`

**Settings:**
- Shape: `Rect`
- Rect X: 2, Rect Y: 5
- Rect Width: 4, Rect Height: 6

This places the spawn zone on the left quarter of the 22×16 grid — 24 available cells for up to 4 hunters.

Assign `SpawnZone_GauntStandard` to `CombatScreenController._spawnZone` in the Inspector.

---

## Verification Test

- [ ] Combat opens in DeploymentPhase — phase label shows "DEPLOY: Place Aldric"
- [ ] Spawn zone cells highlight in teal-blue
- [ ] Clicking a teal cell places Aldric there; his token appears
- [ ] Clicking outside the zone does nothing (TryPlaceHunter warns and returns false)
- [ ] Clicking an occupied cell (another hunter or monster footprint) does nothing
- [ ] After all hunters placed, VitalityPhase begins automatically
- [ ] Phase label updates to "VITALITY PHASE" after last placement
- [ ] Spawn zone tint clears when VitalityPhase begins
- [ ] With 2 hunters: placing one shows "DEPLOY: Place [next]"; placing both advances phase
- [ ] Console shows `[Combat] Aldric placed at (X,Y)` per placement
- [ ] Console shows `[Combat] All hunters placed — advancing to VitalityPhase`
- [ ] Legacy fallback: if all hunters have `isUnplaced=false` at StartCombat, DeploymentPhase is skipped

---

## Next Session

**File:** `_Docs/Stage_08/STAGE_08_P.md`
**Covers:** Active hunter selection — player clicks any unhurt, unturn'd hunter token to switch who is active before playing cards or moving
