<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 8-K | Hunter Movement UI — Grid Click to Move & Facing
Status: Stage 8-J complete. Card animations working.
Task: Wire hunter movement to the combat grid. TryMoveHunter()
is fully implemented in CombatManager and GridManager but
nothing in CombatScreenController calls it. Clicking an empty
cell during Hunter Phase does nothing. Fix this: clicking a
reachable empty cell moves the active hunter. Show valid
movement cells in green. Update hunter facing after each move.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_08/STAGE_08_K.md
- Assets/_Game/Scripts/Core.UI/CombatScreenController.cs
- Assets/_Game/Scripts/Core.Systems/CombatManager.cs
- Assets/_Game/Scripts/Core.Data/CombatState.cs

Then confirm:
- TryMoveHunter() in CombatManager.cs is complete (lines ~261-317)
- HunterCombatState already has facingX and facingY int fields
- OnGridCellClicked() has no movement code — only card targeting
- _validMoveCells will be a new HashSet<Vector2Int> added to the controller
- StatusEffectResolver.ApplyStatusPenalties already handles Slowed movement reduction
- What you will NOT build: token lerp animations (Stage 10-M)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 8-K: Hunter Movement UI — Grid Click to Move & Facing

**Resuming from:** Stage 8-J complete — card animations working
**Done when:** Active hunter moves on grid click; reachable cells highlight green; hunter facing updates on every move; movement and card-targeting highlights never appear simultaneously
**Commit:** `"8K: Hunter movement UI — grid click-to-move, range highlight, facing update"`
**Next session:** STAGE_08_L.md

---

## What's Missing

`CombatManager.TryMoveHunter()` validates bounds, occupancy, denied cells, and movement range — it is fully implemented. `HunterCombatState` already has `facingX`/`facingY`. But `CombatScreenController.OnGridCellClicked()` only handles card targeting. Clicking any empty cell during Hunter Phase does nothing. This stage wires the two together and adds movement range highlighting.

---

## Part 1: Facing Update in TryMoveHunter

Open `Assets/_Game/Scripts/Core.Systems/CombatManager.cs`.

Find the successful-move block in `TryMoveHunter()` — the section that calls `MoveOccupant` and updates `hunter.gridX`/`gridY`. Immediately after setting those fields, add the facing update:

```csharp
// After: hunter.gridY = destination.y;

// Update facing toward direction of movement
int dx = destination.x - from.x;
int dy = destination.y - from.y;
if (dx != 0 || dy != 0)
{
    hunter.facingX = dx == 0 ? 0 : (dx > 0 ? 1 : -1);
    hunter.facingY = dy == 0 ? 0 : (dy > 0 ? 1 : -1);
}
Debug.Log($"[Combat] {hunter.hunterName} moved to ({destination.x},{destination.y}) " +
          $"facing ({hunter.facingX},{hunter.facingY})");
```

---

## Part 2: Movement Range Fields and Helpers

Open `Assets/_Game/Scripts/Core.UI/CombatScreenController.cs`.

Add one field alongside the existing card-selection state fields:

```csharp
// ── Movement State ────────────────────────────────────────────
private HashSet<Vector2Int> _validMoveCells = new();
```

Add a helper to avoid repeating the active-hunter lookup (used in several places below):

```csharp
private HunterCombatState GetActiveHunter()
{
    var state = _combatManager?.CurrentState;
    if (state == null) return null;
    return System.Array.Find(state.hunters, h => !h.hasActedThisPhase && !h.isCollapsed);
}
```

---

## Part 3: ShowMovementRange and ClearMovementRange

Add these two methods immediately after `RefreshGrid()`:

```csharp
private void ShowMovementRange(HunterCombatState hunter)
{
    _validMoveCells.Clear();
    if (_gridManager == null || _gridCells == null) return;

    int effectiveMove = hunter.movement;
    int effectiveAcc  = hunter.accuracy;
    StatusEffectResolver.ApplyStatusPenalties(hunter, ref effectiveAcc, ref effectiveMove);

    var origin = new Vector2Int(hunter.gridX, hunter.gridY);

    for (int x = 0; x < 22; x++)
    for (int y = 0; y < 16; y++)
    {
        var cell = new Vector2Int(x, y);
        if (!_gridManager.IsInBounds(cell)) continue;
        if (_gridManager.IsOccupied(cell))  continue;
        if (_gridManager.IsDenied(cell))    continue;

        int dist = _gridManager.GetDistance(origin, cell);
        if (dist > 0 && dist <= effectiveMove)
            _validMoveCells.Add(cell);
    }

    // Apply CSS class to each reachable cell
    foreach (var pos in _validMoveCells)
    {
        var el = _gridCells[pos.x, pos.y];
        if (el != null) el.EnableInClassList("grid-cell--movable", true);
    }

    Debug.Log($"[CombatUI] Movement range: {_validMoveCells.Count} cells reachable from " +
              $"({hunter.gridX},{hunter.gridY}) move={effectiveMove}");
}

private void ClearMovementRange()
{
    foreach (var pos in _validMoveCells)
    {
        var el = _gridCells?[pos.x, pos.y];
        if (el != null) el.EnableInClassList("grid-cell--movable", false);
    }
    _validMoveCells.Clear();
}
```

---

## Part 4: Wire ShowMovementRange into Phase Changes

In `OnPhaseChanged()`, after the `RefreshAll()` call at the bottom, add:

```csharp
// Clear stale highlights first, then show movement range if entering Hunter Phase
ClearMovementRange();
if (phase == CombatPhase.HunterPhase && _pendingCardName == null)
{
    var active = GetActiveHunter();
    if (active != null) ShowMovementRange(active);
}
```

---

## Part 5: Wire ShowMovementRange into Card Selection

In `OnCardClicked()`, find the section that sets `_pendingCardName`. After the toggle-off path (card deselected, `_pendingCardName = null`) and after the select path (`_pendingCardName = cardName`), add range management at both branches:

```csharp
// ── Card deselected (toggle off) ─────────────────────────────
if (_pendingCardName == cardName)
{
    _pendingCardName = null;
    _selectedCardEl  = null;
    Debug.Log($"[CombatUI] Card deselected: {cardName}");

    // Restore movement range when no card is pending
    ClearMovementRange();
    var active = GetActiveHunter();
    if (active != null && _combatManager.CurrentPhase == CombatPhase.HunterPhase)
        ShowMovementRange(active);

    RefreshGrid();
    return;
}

// ── Card selected ─────────────────────────────────────────────
_pendingCardName = cardName;
_selectedCardEl  = cardEl;
cardEl.EnableInClassList("card--selected", true);

// Hide movement highlights — attack targets take priority visually
ClearMovementRange();

Debug.Log($"[CombatUI] Card selected: {cardName} — click a grid cell to target");
RefreshGrid();
```

---

## Part 6: Update OnGridCellClicked to Handle Movement

Replace the full body of `OnGridCellClicked()` with:

```csharp
private void OnGridCellClicked(int x, int y)
{
    _gridCursor = new Vector2Int(x, y);
    Debug.Log($"[CombatUI] Grid cell clicked: ({x},{y})");

    // ── Card targeting mode ───────────────────────────────────
    if (_pendingCardName != null)
    {
        ResolveCardAtCell(x, y);
        return;
    }

    // ── Movement mode (Hunter Phase, no card selected) ────────
    if (_combatManager?.CurrentPhase == CombatPhase.HunterPhase)
    {
        var activeHunter = GetActiveHunter();
        if (activeHunter == null) { RefreshGrid(); return; }

        var destination = new Vector2Int(x, y);

        // Clicking own cell: no-op
        if (destination.x == activeHunter.gridX && destination.y == activeHunter.gridY)
        {
            RefreshGrid();
            return;
        }

        if (_validMoveCells.Contains(destination))
        {
            bool moved = _combatManager.TryMoveHunter(activeHunter.hunterId, destination);
            if (moved)
            {
                ClearMovementRange();
                ShowMovementRange(activeHunter); // Recalculate from new position
                RefreshAll();
            }
            else
            {
                Debug.Log($"[CombatUI] TryMoveHunter rejected: ({x},{y})");
            }
        }
        else
        {
            // Clicked a non-reachable occupied cell — log hunter name if present
            var state = _combatManager.CurrentState;
            var hunter = System.Array.Find(
                state.hunters, h => !h.isCollapsed && h.gridX == x && h.gridY == y);
            if (hunter != null)
                Debug.Log($"[CombatUI] Hunter at ({x},{y}): {hunter.hunterName}");
        }
    }

    RefreshGrid();
}
```

---

## Part 7: Keep Movable Class Alive Through RefreshGrid

In `RefreshGrid()`, inside the per-cell loop alongside the other `EnableInClassList` calls, add:

```csharp
cell.EnableInClassList("grid-cell--movable",
    _validMoveCells.Contains(new Vector2Int(x, y)));
```

This ensures movement highlights survive damage events, phase labels, and other UI refreshes that call `RefreshGrid`.

---

## Part 8: CSS — Movement Highlight

Open the combat screen USS file (whichever `.uss` file defines `grid-cell`).

Add:

```css
.grid-cell--movable {
    background-color: rgba(40, 110, 60, 0.30);
    border-color: rgba(70, 180, 90, 0.55);
    border-width: 1px;
}
```

Muted green — visually distinct from the gold `grid-cell--valid` used for card targeting.

---

## Verification Test

- [ ] Enter Hunter Phase → green cells appear within active hunter's movement range
- [ ] Card selected → green movement tint disappears; monster cells show gold (valid target)
- [ ] Card deselected (click same card again) → movement range reappears
- [ ] Click a green cell → hunter token repositions; range redraws from new position
- [ ] Click hunter's own cell → nothing happens, no error
- [ ] Click an occupied cell (another hunter or monster footprint) → no move
- [ ] Click a denied cell → no move (`TryMoveHunter` rejects it)
- [ ] Movement range respects grid edges — no cells outside 22×16
- [ ] Slowed status applied → movement range shrinks (check movement halved)
- [ ] After move: Console shows `[Combat] Aldric moved to (X,Y) facing (1,0)` (or appropriate)
- [ ] After move: arc log appears `[Grid] Arc check: attacker... → Front/Flank/Rear`
- [ ] Hunter can move multiple times per turn (movement does not end turn)
- [ ] End Turn → movement highlights clear; if a second hunter exists, their range appears
- [ ] Monster Phase → no green highlights visible anywhere

---

## Next Session

**File:** `_Docs/Stage_08/STAGE_08_L.md`
**Covers:** Monster action execution engine — implement `MonsterAI.ExecuteCard()` and `EvaluateTrigger()` so the monster moves toward hunters, deals damage to body zones, and respects trigger conditions when resolving behavior cards each round
