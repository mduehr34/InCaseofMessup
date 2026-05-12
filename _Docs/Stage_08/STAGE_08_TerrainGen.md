<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 8 Extension | Random Terrain Generation
Status: Stage 8-Q complete. Fixed terrain via TerrainSetupSO works.
Task: Implement the random terrain placement pass. Each monster can
carry a TerrainSpawnTableSO that defines which terrain types spawn in
its arena, how many, and where they may appear. The random pass runs
AFTER the hand-authored TerrainSetupSO layout so fixed landmarks are
always guaranteed.

Read these files before doing anything:
- .cursorrules
- CLAUDE.md
- _Docs/Stage_08/STAGE_08_TerrainGen.md
- Assets/_Game/Scripts/Core.Data/TerrainSpawnTableSO.cs
- Assets/_Game/Scripts/Core.Data/TerrainSetupSO.cs
- Assets/_Game/Scripts/Core.Data/MonsterSO.cs
- Assets/_Game/Scripts/Core.Systems/CombatManager.cs   (StartCombat TODO stub)
- Assets/_Game/Scripts/Core.Systems/IGridManager.cs

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 8 Extension: Random Terrain Generation

**Prerequisite:** Stage 8-Q complete — `TerrainSpawnTableSO` stub exists; `CombatManager.StartCombat` has a TODO comment for this pass
**Done when:** Each monster can carry a `TerrainSpawnTableSO`; at combat start the random terrain pass places the correct terrain count within the distance constraints; terrain survives save/load; Debug.Log confirms each randomly placed cell
**Commit:** `"8-TerrainGen: random terrain placement from monster spawn table"`

---

## What Already Exists

`TerrainSpawnTableSO` is fully stubbed at `Assets/_Game/Scripts/Core.Data/TerrainSpawnTableSO.cs`:

```csharp
public TerrainSpawnRule[] rules;

public struct TerrainSpawnRule
{
    public TerrainCellSO terrain;
    public int count;
    public int minDistFromMonster;
    public int maxDistFromMonster;
    public bool avoidSpawnZones;
    public bool avoidOtherTerrain;
}
```

`CombatManager._terrainSpawnTable` field exists (`[SerializeField]`). The TODO stub in `StartCombat` marks exactly where the random pass fires (after the fixed `TerrainSetupSO` pass).

---

## Part 1: Wire TerrainSpawnTableSO to MonsterSO

Add to `MonsterSO`:

```csharp
[Header("Terrain")]
public TerrainSetupSO  terrainLayout;      // Hand-authored fixed landmarks (optional)
public TerrainSpawnTableSO terrainSpawnTable; // Random terrain pool (optional)
```

In `CombatManager.InitializeMonsterAI` (or `StartCombat`), read these from the selected `MonsterSO` and assign them to `_terrainSetup` / `_terrainSpawnTable` respectively. This removes the Inspector-only wiring and lets the monster drive its own arena.

---

## Part 2: Implement ApplyRandomTerrain

Add private method to `CombatManager`:

```csharp
private void ApplyRandomTerrain(
    IGridManager grid, CombatState state, TerrainSpawnTableSO table)
{
    if (table?.rules == null) return;

    var monster    = state.monster;
    var placed     = new HashSet<string>(); // already-placed terrain keys from fixed layout
    if (state.grid?.terrainCells != null)
        foreach (var t in state.grid.terrainCells)
            placed.Add($"{t.x},{t.y}");

    var newCells = new List<TerrainCellState>();

    foreach (var rule in table.rules)
    {
        if (rule.terrain == null || rule.count <= 0) continue;

        // Build candidate cells that satisfy all constraints
        var candidates = new List<Vector2Int>();
        for (int x = 0; x < grid.GridWidth;  x++)
        for (int y = 0; y < grid.GridHeight; y++)
        {
            var pos = new Vector2Int(x, y);
            if (!grid.IsInBounds(pos))    continue;
            if (grid.IsOccupied(pos))     continue;
            if (grid.IsDenied(pos))       continue;
            if (placed.Contains($"{x},{y}")) continue;

            // Distance from monster footprint (Chebyshev)
            int distToMonster = MinDistToMonsterFootprint(pos, monster);
            if (distToMonster < rule.minDistFromMonster) continue;
            if (distToMonster > rule.maxDistFromMonster) continue;

            // Spawn zone avoidance — requires SpawnZoneSO refs (pass as param if needed)
            // if (rule.avoidSpawnZones && CellInAnySpawnZone(pos, spawnZones)) continue;

            candidates.Add(pos);
        }

        // Shuffle candidates and pick 'count' of them
        ShuffleList(candidates);
        int toPlace = Mathf.Min(rule.count, candidates.Count);
        for (int i = 0; i < toPlace; i++)
        {
            var pos  = candidates[i];
            var cell = new TerrainCellState
            {
                x             = pos.x,
                y             = pos.y,
                terrainId     = rule.terrain.terrainId,
                terrainType   = rule.terrain.terrainType,
                accuracyBonus = rule.terrain.accuracyBonus,
                defenseBonus  = rule.terrain.defenseBonus,
                movementCost  = rule.terrain.movementCost,
                cssClass      = rule.terrain.cssClass,
            };
            grid.PlaceTerrain(cell);
            placed.Add($"{pos.x},{pos.y}");
            newCells.Add(cell);
        }
    }

    // Append to GridState for save/load
    if (state.grid != null)
    {
        var all = new List<TerrainCellState>(state.grid.terrainCells ?? new TerrainCellState[0]);
        all.AddRange(newCells);
        state.grid.terrainCells = all.ToArray();
    }

    Debug.Log($"[Terrain] Random pass placed {newCells.Count} cells from spawn table");
}
```

Replace the TODO stub in `StartCombat` with a call to this method.

---

## Part 3: Save / Load Round-Trip

Random terrain is appended to `GridState.terrainCells` before the state is serialised, so no extra work is needed — the existing JSON save/load path already persists the full array.

On load, `StartCombat` receives the already-populated `GridState.terrainCells` (from the saved JSON). Add a replay pass at the start of `StartCombat` that iterates over any pre-existing `grid.terrainCells` and calls `grid.PlaceTerrain` for each, so the `GridManager` dictionary is populated before gameplay begins.

---

## Part 4: Mock Spawn Table — Gaunt Standard

Create `Assets/_Game/Data/Terrain/TerrainSpawnTable_Gaunt.asset`:

| Rule | Terrain | Count | minDist | maxDist | avoidSpawnZones |
|---|---|---|---|---|---|
| 0 | StickyMud | 2 | 2 | 6 | true |
| 1 | HighGround | 1 | 2 | 5 | false |

Assign to `Monster_Gaunt.terrainSpawnTable`.

---

## Verification Test

- [ ] Enter combat vs Gaunt Standard — console shows `[Terrain] Random pass placed N cells from spawn table` with the expected count
- [ ] Random cells respect distance constraints (no terrain inside the monster footprint)
- [ ] High-ground and mud CSS tints appear on randomly placed cells
- [ ] Save during combat, reload → terrain cells survive; tints visible immediately
- [ ] Running twice with the same seed produces a different layout (randomness confirmed)
- [ ] Fixed `TerrainSetupSO` cells (High Ground at 11,7 etc.) always appear regardless of random roll

---

## Notes

- The spawn zone avoidance (`avoidSpawnZones`) requires the `SpawnZoneSO[]` array to be accessible in `CombatManager`. Either pass it as a parameter to `ApplyRandomTerrain` or read it from `MonsterSO.hunterSpawnZones`.
- If `count` exceeds the number of valid candidates after filtering, the system gracefully places fewer cells — it does not error. Log how many were actually placed vs. requested.
- Difficulty scaling: `TerrainSpawnRule.count` can be overridden per difficulty tier by adding a `countHardened` / `countApex` pair to the struct if needed later.
