<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude Code to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 8-P | Active Hunter Selection — Player Chooses Who Acts
Status: Stage 8-O complete. Hunter deployment phase working — hunters
are placed via spawn zone before combat begins.
Task: Allow the player to select which hunter is active during
Hunter Phase by clicking that hunter's grid token. Currently
GetActiveHunter() always returns the first non-acted hunter in
array order. After this session the player clicks any non-acted
hunter token to activate them, see their hand, and move or play
cards as that hunter. End Turn marks only that hunter done; the
others remain selectable.

Read these files before doing anything:
- CLAUDE.md
- _Docs/Stage_08/STAGE_08_P.md
- Assets/_Game/Scripts/Core.UI/CombatScreenController.cs
- Assets/_Game/Scripts/Core.Systems/CombatManager.cs
- Assets/_Game/Scripts/Core.Data/CombatState.cs

Then confirm:
- GetActiveHunter() currently does: Array.Find(hunters, h => !h.hasActedThisPhase && !h.isCollapsed)
- TryMoveHunter(hunterId, dest) and TryPlayCard(hunterId, cardName, cell) already take explicit hunterId
- EndHunterTurn(hunterId) already marks only the specified hunter's hasActedThisPhase = true
- _selectedHunterId does NOT exist yet — you will add it
- Hunter tokens are rendered via grid-cell--hunter CSS class; clicking a hunter cell currently
  triggers movement code rather than selection

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 8-P: Active Hunter Selection — Player Chooses Who Acts

**Resuming from:** Stage 8-O complete — hunter deployment working
**Done when:** During Hunter Phase the player clicks any non-acted hunter token to select them; movement range, hand, and AP display all update for the selected hunter; clicking a different non-acted hunter switches selection; End Turn ends only the selected hunter's turn; the UI clearly marks the active hunter
**Commit:** `"8P: Active hunter selection — click token to switch active hunter"`
**Next session:** STAGE_08_Q.md

---

## What's Changing and Why

Currently `GetActiveHunter()` returns whichever hunter comes first in the `hunters[]` array that hasn't acted. With two hunters — say Aldric (index 0) and a second hunter (index 1) — you're forced to act with Aldric first, always. There is no way to choose.

The fix is entirely in `CombatScreenController`. `CombatManager` already supports acting on any hunter by `hunterId` — `TryPlayCard`, `TryMoveHunter`, and `EndHunterTurn` all take an explicit ID. The only thing that needs changing is which hunter the UI treats as "active."

No changes to `CombatManager`, `ICombatManager`, or `CombatState` are needed.

---

## Part 1: _selectedHunterId Field

Open `Assets/_Game/Scripts/Core.UI/CombatScreenController.cs`.

Add one field in the `// ── Card Selection State` block:

```csharp
// ── Active Hunter Selection ────────────────────────────────────────
private string _selectedHunterId = null;   // null = fall back to first non-acted hunter
```

---

## Part 2: Update GetActiveHunter()

Replace the existing `GetActiveHunter()` implementation:

```csharp
private HunterCombatState GetActiveHunter()
{
    var state = _combatManager?.CurrentState;
    if (state == null) return null;

    // If a specific hunter was selected and they haven't acted yet, keep them active
    if (_selectedHunterId != null)
    {
        var selected = System.Array.Find(
            state.hunters,
            h => h.hunterId == _selectedHunterId && !h.hasActedThisPhase && !h.isCollapsed);
        if (selected != null) return selected;

        // Selection is stale (selected hunter just ended their turn) — clear it
        _selectedHunterId = null;
    }

    // Default: first non-acted, non-collapsed hunter
    return System.Array.Find(state.hunters, h => !h.hasActedThisPhase && !h.isCollapsed);
}
```

---

## Part 3: Hunter Token Click → Selection

In `OnGridCellClicked()`, add a selection branch inside the `HunterPhase` movement block. Currently the movement block's else clause only logs a hunter name. Replace it with:

```csharp
else
{
    // Clicked a non-reachable cell — check if it's another non-acted hunter token
    var state  = _combatManager.CurrentState;
    var hunter = System.Array.Find(
        state.hunters,
        h => !h.isCollapsed && !h.hasActedThisPhase &&
             h.gridX == x && h.gridY == y);

    if (hunter != null && hunter.hunterId != GetActiveHunter()?.hunterId)
    {
        // Switch active selection to this hunter
        _selectedHunterId = hunter.hunterId;
        Debug.Log($"[CombatUI] Active hunter switched to: {hunter.hunterName}");

        ClearMovementRange();
        _pendingCardName = null;
        if (_selectedCardEl != null)
        {
            _selectedCardEl.EnableInClassList("card--selected", false);
            _selectedCardEl = null;
        }
        ShowMovementRange(hunter);
        RefreshAll();
    }
    else if (hunter != null)
    {
        Debug.Log($"[CombatUI] Hunter at ({x},{y}): {hunter.hunterName} (already selected)");
    }
}
```

---

## Part 4: Clear Selection on Phase Change

In `OnPhaseChanged()`, clear the selection whenever a new Hunter Phase starts (or any non-hunter phase):

```csharp
// After ClearMovementRange() near the top of OnPhaseChanged:
if (phase != CombatPhase.HunterPhase)
    _selectedHunterId = null;
```

This ensures a fresh round always starts with the player choosing, rather than carrying a stale selection from the previous round.

---

## Part 5: End Turn Ends Only the Selected Hunter

`OnEndTurnClicked()` currently finds the first non-acted hunter. Update it to use `GetActiveHunter()` (which already respects `_selectedHunterId`):

```csharp
private void OnEndTurnClicked()
{
    if (_combatManager?.CurrentState == null) return;

    var activeHunter = GetActiveHunter();
    if (activeHunter != null)
    {
        _selectedHunterId = null;   // Clear selection before ending turn
        _combatManager.EndHunterTurn(activeHunter.hunterId);
        ClearMovementRange();
        Debug.Log($"[CombatUI] End Turn: {activeHunter.hunterName} done");

        // Auto-select next non-acted hunter if exactly one remains
        var state = _combatManager.CurrentState;
        var remaining = System.Array.FindAll(
            state.hunters, h => !h.hasActedThisPhase && !h.isCollapsed);
        if (remaining.Length == 1)
        {
            _selectedHunterId = remaining[0].hunterId;
            ShowMovementRange(remaining[0]);
            Debug.Log($"[CombatUI] Auto-selected last remaining hunter: {remaining[0].hunterName}");
        }

        RefreshAll();
    }
    else
    {
        _combatManager.AdvancePhase();
        Debug.Log("[CombatUI] All hunters acted — phase advanced");
    }
}
```

---

## Part 6: Visual — Active Hunter Highlight

The hunter panel for the active hunter should have a visible highlight so the player knows who they're acting as. Extend `RefreshHunterPanel()`:

```csharp
private void RefreshHunterPanel(int index, HunterCombatState hunter, bool hasAggro)
{
    // ... existing code ...

    // Active hunter highlight
    bool isActive = !hunter.hasActedThisPhase &&
                    !hunter.isCollapsed &&
                    hunter.hunterId == GetActiveHunter()?.hunterId;
    _hunterPanels[index].EnableInClassList("hunter-panel--active", isActive);
}
```

Add to `RefreshGrid()` — mark the active hunter's grid cell with a distinct CSS class:

```csharp
var activeHunter = GetActiveHunter();
// Inside the per-cell loop, after the hunter class:
bool isActiveHunterCell = activeHunter != null &&
                          !activeHunter.isCollapsed &&
                          x == activeHunter.gridX && y == activeHunter.gridY;
cell.EnableInClassList("grid-cell--active-hunter", isActiveHunterCell);
```

---

## Part 7: CSS — Selection Highlight

Add to `combat-screen.uss`:

```css
/* Hunter panel — currently active (their turn, selected by player) */
.hunter-panel--active {
    border-color: rgba(200, 170, 60, 0.85);
    border-width: 2px;
}

/* Grid token for the currently active hunter — gold outline, distinct from green movable */
.grid-cell--active-hunter {
    background-color: rgba(180, 140, 30, 0.45);
    border-color:     rgba(230, 190, 60, 0.90);
    border-width:     2px;
}
```

---

## Part 8: ResolveCardAtCell — Use GetActiveHunter()

`ResolveCardAtCell()` currently has its own `Array.Find` for the active hunter. Replace it with a call to `GetActiveHunter()` so card plays also respect the selection:

```csharp
private void ResolveCardAtCell(int x, int y)
{
    var state = _combatManager?.CurrentState;
    if (state == null || _pendingCardName == null) return;

    // Use GetActiveHunter() to respect _selectedHunterId
    var activeHunter = GetActiveHunter();
    if (activeHunter == null)
    {
        Debug.LogWarning("[CombatUI] No active hunter to play card");
        return;
    }

    // ... rest of method unchanged ...
}
```

---

## Edge Cases to Handle

**Two hunters, one collapsed:** `GetActiveHunter()` must never return a collapsed hunter. The `!h.isCollapsed` guard in the find already handles this.

**Selected hunter ends turn via card exhaust (no AP):** If `EndHunterTurn` is called automatically by a system event (not via button), `_selectedHunterId` may be stale. The stale-selection guard in `GetActiveHunter()` clears it.

**Player selects a hunter who has already acted:** Hunter tokens with `hasActedThisPhase=true` should not be selectable. The `!h.hasActedThisPhase` guard in the click handler prevents this.

**All hunters acted — phase should advance:** `EndHunterTurn` in `CombatManager` already calls `AdvancePhase()` internally when `AllHuntersActed()` is true. No extra handling needed here.

---

## Verification Test

- [ ] Two hunters in combat (extend CombatStateFactory to add a second mock hunter at a different unplaced position)
- [ ] Hunter Phase begins — first non-acted hunter's panel glows gold; their movement range shows
- [ ] Click the other hunter's grid token → selection switches; gold panel moves to that hunter; movement range redraws
- [ ] Card hand updates to show the newly selected hunter's cards (hand is based on `GetActiveHunter()`)
- [ ] Play a card as Hunter B → it comes from Hunter B's hand, not Hunter A's
- [ ] Click End Turn → Hunter B's `hasActedThisPhase=true`; Hunter A is auto-selected if they're the last remaining
- [ ] Click End Turn for Hunter A → both acted; phase advances to BehaviorRefresh
- [ ] Gold panel highlight on hunter panel matches the selected hunter
- [ ] Gold grid cell highlight appears on selected hunter's token
- [ ] Clicking a hunter who has already acted does nothing (no selection change, no log spam)
- [ ] After round ends (VitalityPhase fires), selection resets — both panels lose gold highlight
- [ ] Pressing Escape while a card is selected deselects the card but does NOT change hunter selection

---

## Next Session

**File:** `_Docs/Stage_08/STAGE_08_Q.md`
**Covers:** Combat Terrain — obstacle cells that block movement and bonus terrain cells that grant stat modifiers; defined via `TerrainCellSO` and `TerrainSetupSO`, applied at combat start, rendered as grid tints
