<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 5-E | Grid Rendering, Card→Cell Resolution & Stage 5 Final
Status: Stage 5-D complete. Card hand renders correctly.
Card click sets pending state. Monster panel shows all parts.
Task: Build the 22×16 grid procedurally, handle grid cell
clicks to resolve pending cards, wire End Turn button fully,
and run the Stage 5 final playthrough to confirm the combat
screen is fully playable.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_05/STAGE_05_E.md
- Assets/_Game/Scripts/Core.UI/CombatScreenController.cs
- Assets/_Game/Scripts/Core.Systems/ICombatManager.cs
- Assets/_Game/Scripts/Core.Systems/IGridManager.cs
- Assets/_Game/Scripts/Core.Data/CombatState.cs
- Assets/_Game/UI/USS/combat-screen.uss

Then confirm:
- That you will ADD to CombatScreenController.cs
- That grid cells are VisualElements, not uGUI objects
- That clicking a cell with a pending card calls
  CombatManager.TryPlayCard() then RefreshAll()
- That clicking a cell with no pending card and a
  hunter present selects that hunter
- What you will NOT touch this session

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 5-E: Grid Rendering, Card→Cell Resolution & Stage 5 Final

**Resuming from:** Stage 5-D complete — card hand and monster panel verified  
**Done when:** 22×16 grid renders in the centre panel; clicking a card then a grid cell calls `TryPlayCard()`; End Turn button advances the phase; Stage 5 Definition of Done fully checked off  
**Commit:** `"5E: Grid rendering, card→cell resolution, End Turn — Stage 5 complete"`  
**Next session:** STAGE_06_A.md (Stage 6 begins)  

---

## Step 1: Add Grid Rendering to CombatScreenController.cs

Add these fields and methods to the **existing** `CombatScreenController` class:

### New Fields

```csharp
// ── Grid ─────────────────────────────────────────────────────
private VisualElement _gridContainer;
private VisualElement[,] _gridCells;    // [x, y] — 22×16

[SerializeField] private GridManager _gridManager;

// ── Keyboard / Grid Cursor ───────────────────────────────────
private Vector2Int _gridCursor = new Vector2Int(-1, -1); // -1 = no selection
```

### BuildGrid() — call once after CombatManager.StartCombat()

```csharp
public void BuildGrid()
{
    _gridContainer = _root.Q<VisualElement>("grid-container");
    if (_gridContainer == null)
    {
        Debug.LogError("[CombatUI] grid-container not found in UXML");
        return;
    }

    _gridContainer.Clear();
    _gridCells = new VisualElement[22, 16];

    for (int y = 0; y < 16; y++)
    {
        var row = new VisualElement();
        row.AddToClassList("grid-row");

        for (int x = 0; x < 22; x++)
        {
            var cell = new VisualElement();
            cell.AddToClassList("grid-cell");

            // Capture for lambda
            int cx = x, cy = y;
            cell.RegisterCallback<ClickEvent>(_ => OnGridCellClicked(cx, cy));

            _gridCells[x, y] = cell;
            row.Add(cell);
        }

        _gridContainer.Add(row);
    }

    Debug.Log("[CombatUI] Grid built: 22×16 cells");
    RefreshGrid();
}

public void RefreshGrid()
{
    if (_gridCells == null || _combatManager?.CurrentState == null) return;

    var state = _combatManager.CurrentState;
    IGridManager grid = _gridManager;

    for (int y = 0; y < 16; y++)
    for (int x = 0; x < 22; x++)
    {
        var cell = _gridCells[x, y];
        if (cell == null) continue;

        var pos = new Vector2Int(x, y);

        // Reset classes
        cell.EnableInClassList("grid-cell--denied",   false);
        cell.EnableInClassList("grid-cell--marrow",   false);
        cell.EnableInClassList("grid-cell--hunter",   false);
        cell.EnableInClassList("grid-cell--monster",  false);
        cell.EnableInClassList("grid-cell--selected", false);
        cell.EnableInClassList("grid-cell--valid",    false);
        cell.style.backgroundColor = StyleKeyword.Null; // Reset inline colour

        // Tile type
        if (grid != null)
        {
            if (grid.IsDenied(pos))     cell.AddToClassList("grid-cell--denied");
            if (grid.IsMarrowSink(pos)) cell.AddToClassList("grid-cell--marrow");
        }

        // Occupants
        bool isHunterCell  = IsHunterAtCell(state.hunters, x, y);
        bool isMonsterCell = IsMonsterAtCell(state.monster, x, y);
        if (isHunterCell)  cell.AddToClassList("grid-cell--hunter");
        if (isMonsterCell) cell.AddToClassList("grid-cell--monster");

        // Cursor
        if (_gridCursor.x == x && _gridCursor.y == y)
            cell.AddToClassList("grid-cell--selected");

        // Pending card — highlight valid targets (monster cells)
        if (_pendingCardName != null && isMonsterCell)
            cell.AddToClassList("grid-cell--valid");
    }
}

private bool IsHunterAtCell(HunterCombatState[] hunters, int x, int y)
{
    foreach (var h in hunters)
        if (!h.isCollapsed && h.gridX == x && h.gridY == y) return true;
    return false;
}

private bool IsMonsterAtCell(MonsterCombatState monster, int x, int y)
{
    return x >= monster.gridX && x < monster.gridX + monster.footprintW &&
           y >= monster.gridY && y < monster.gridY + monster.footprintH;
}
```

### Grid Cell Click Handler

```csharp
private void OnGridCellClicked(int x, int y)
{
    _gridCursor = new Vector2Int(x, y);
    Debug.Log($"[CombatUI] Grid cell clicked: ({x},{y})");

    if (_pendingCardName != null)
    {
        // Card is pending — attempt to resolve it at this cell
        ResolveCardAtCell(x, y);
    }
    else
    {
        // No card pending — check if a hunter is here (future: select them)
        var state = _combatManager?.CurrentState;
        if (state != null)
        {
            var hunter = System.Array.Find(
                state.hunters, h => !h.isCollapsed && h.gridX == x && h.gridY == y);
            if (hunter != null)
                Debug.Log($"[CombatUI] Hunter at ({x},{y}): {hunter.hunterName}");
        }
    }

    RefreshGrid();
}

private void ResolveCardAtCell(int x, int y)
{
    var state = _combatManager?.CurrentState;
    if (state == null || _pendingCardName == null) return;

    // Find the active hunter
    var activeHunter = System.Array.Find(
        state.hunters, h => !h.hasActedThisPhase && !h.isCollapsed);
    if (activeHunter == null)
    {
        Debug.LogWarning("[CombatUI] No active hunter to play card");
        return;
    }

    var targetCell = new Vector2Int(x, y);
    bool success = _combatManager.TryPlayCard(
        activeHunter.hunterId, _pendingCardName, targetCell);

    if (success)
    {
        Debug.Log($"[CombatUI] Card played: {_pendingCardName} → ({x},{y})");
    }
    else
    {
        Debug.Log($"[CombatUI] TryPlayCard failed: {_pendingCardName} → ({x},{y}) — invalid target or insufficient AP");
    }

    // Always clear selection after attempt
    _pendingCardName = null;
    if (_selectedCardEl != null)
    {
        _selectedCardEl.EnableInClassList("card--selected", false);
        _selectedCardEl = null;
    }

    RefreshAll();
    RefreshGrid();
}
```

### Wire BuildGrid into Start()

```csharp
private void Start()
{
    if (_combatManager?.CurrentState != null)
    {
        RefreshAll();
        BuildGrid(); // Build grid after state is available
    }
}
```

---

## Step 2: Keyboard & Controller Input

Add a basic `Update()` for keyboard support:

```csharp
private void Update()
{
    HandleKeyboardInput();
}

private void HandleKeyboardInput()
{
    // Number keys 1–6: select card by index
    for (int i = 0; i < 6; i++)
    {
        if (Input.GetKeyDown(KeyCode.Alpha1 + i))
        {
            SelectCardByIndex(i);
            return;
        }
    }

    // Escape: cancel card selection
    if (Input.GetKeyDown(KeyCode.Escape))
    {
        if (_pendingCardName != null)
        {
            _pendingCardName = null;
            if (_selectedCardEl != null)
            {
                _selectedCardEl.EnableInClassList("card--selected", false);
                _selectedCardEl = null;
            }
            RefreshGrid();
            Debug.Log("[CombatUI] Card selection cancelled");
        }
        return;
    }

    // WASD / Arrow: move grid cursor
    Vector2Int delta = Vector2Int.zero;
    if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))    delta.y = -1;
    if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))  delta.y =  1;
    if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))  delta.x = -1;
    if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) delta.x =  1;

    if (delta != Vector2Int.zero)
    {
        int nx = Mathf.Clamp((_gridCursor.x < 0 ? 11 : _gridCursor.x) + delta.x, 0, 21);
        int ny = Mathf.Clamp((_gridCursor.y < 0 ? 8  : _gridCursor.y) + delta.y, 0, 15);
        _gridCursor = new Vector2Int(nx, ny);
        RefreshGrid();
        return;
    }

    // Enter / Space: confirm grid cursor selection
    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
    {
        if (_gridCursor.x >= 0)
            OnGridCellClicked(_gridCursor.x, _gridCursor.y);
    }
}

private void SelectCardByIndex(int index)
{
    var state = _combatManager?.CurrentState;
    if (state == null) return;

    var activeHunter = System.Array.Find(
        state.hunters, h => !h.hasActedThisPhase && !h.isCollapsed);
    if (activeHunter == null || index >= activeHunter.handCardNames.Length) return;

    string cardName = activeHunter.handCardNames[index];
    var cardEl = _handCards?.ElementAt(index) as VisualElement;

    Debug.Log($"[CombatUI] Keyboard card select [{index+1}]: {cardName}");
    if (cardEl != null) OnCardClicked(cardName, cardEl, true);
}
```

---

## Stage 5 Final Playthrough Verification

Play the combat scene and work through this checklist manually:

**Grid:**
- [ ] 22×16 grid of cells renders in the centre panel
- [ ] Cells have dark background with visible borders
- [ ] Hunter cell (Aldric at 5,8) shows hunter border colour
- [ ] Monster cell (Gaunt at 12,7) shows monster border colour

**Card Flow:**
- [ ] Click "Brace" in the card hand → card highlights gold
- [ ] Click a monster grid cell → `TryPlayCard()` called, Debug.Log confirms
- [ ] Card deselects after play attempt
- [ ] RefreshAll() fires after card play — panels update

**Phase Flow:**
- [ ] Phase label starts "VITALITY PHASE"
- [ ] Click End Turn → phase advances (may cycle through phases)
- [ ] Phase label updates correctly on each advance

**Keyboard:**
- [ ] Press 1 → first card in hand selected
- [ ] Press Escape → card deselected
- [ ] Press WASD → grid cursor moves, cell highlights
- [ ] Press Enter on a monster cell with card pending → TryPlayCard() called

**Monster Panel:**
- [ ] Monster name shows "The Gaunt"
- [ ] All 7 parts visible with Shell/Flesh tracks
- [ ] Reducing a part's shell in mock data shows bar at partial fill

**No uGUI:**
- [ ] Confirm Hierarchy has NO Canvas, Image, or Text components

---

## Stage 5 Complete — What You Now Have

- `tokens.uss` and `stone-panel.uss` — full design token system
- `combat-screen.uxml` and `combat-screen.uss` — 4-zone layout verified
- `CombatScreenController.cs` — complete:
  - Hunter panels (name, Shell/Flesh bars, aggro, status badges, AP, Grit pips, collapsed state)
  - Phase label and round counter
  - Card hand (card elements from ActionCardSO, Loud tag, AP cost, playability dimming)
  - Card click → pending state → grid click → `TryPlayCard()`
  - Monster panel (all parts, Shell/Flesh bars, broken/exposed tags, deck count)
  - 22×16 grid (procedural cells, denied/marrow/hunter/monster/valid highlighting)
  - Keyboard input (number keys, WASD cursor, Enter confirm, Escape cancel)

No settlement UI. No modals. No scene transitions. A fully playable combat screen.

---

## Next Session

**File:** `_Docs/Stage_06/STAGE_06_A.md`  
**First task of Stage 6:** Main menu screen and GameStateManager scaffold — the scene management layer that connects all screens
