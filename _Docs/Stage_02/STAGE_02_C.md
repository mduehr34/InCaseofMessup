<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 2-C | GridManager Implementation
Status: Stage 2-B complete. CombatState JSON round-trip
verified. Test script deleted.
Task: Implement GridManager — the concrete class that
implements IGridManager. No UI. No MonsterAI. No CombatManager.
Grid logic only.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_02/STAGE_02_C.md
- Assets/_Game/Scripts/Core.Systems/IGridManager.cs
- Assets/_Game/Scripts/Core.Data/Enums.cs
- Assets/_Game/Scripts/Core.Data/DataStructs.cs

Then confirm:
- What file you will create
- What you will NOT touch this session

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 2-C: GridManager Implementation

**Resuming from:** Stage 2-B complete — CombatState JSON verified  
**Done when:** GridManager compiles, all IGridManager methods implemented, placement/movement/facing/denial all log correctly  
**Commit:** `"2C: GridManager — placement, movement, facing arc, denial"`  
**Next session:** STAGE_02_D.md  

---

## Key Rules for This Implementation

- Grid is **22 wide × 16 high** — constants, never hardcoded elsewhere
- Distance uses **Chebyshev** (diagonal movement = 1, not √2)
- Facing arc logic: Front = attacker in front 3×3 cone, Flank = sides, Rear = behind
- Denied cells have a `roundsRemaining` counter — decremented by `TickDeniedCells()`
- Multi-tile occupants (monster footprint 2×2 or 3×3) occupy multiple cells simultaneously

---

## GridManager.cs

**Path:** `Assets/_Game/Scripts/Core.Systems/GridManager.cs`

```csharp
using System.Collections.Generic;
using UnityEngine;
using MnM.Core.Data;

namespace MnM.Core.Systems
{
    public class GridManager : MonoBehaviour, IGridManager
    {
        // ── Constants ────────────────────────────────────────────
        public int GridWidth  => 22;
        public int GridHeight => 16;

        // ── Internal State ───────────────────────────────────────
        // Key: "x,y" string. Value: occupantId or null
        private Dictionary<string, string>       _occupancy    = new();
        private Dictionary<string, GridOccupant> _occupants    = new();
        private Dictionary<string, int>          _deniedCells  = new(); // key:"x,y", value:roundsRemaining
        private HashSet<string>                  _marrowSinks  = new();

        // ── Helpers ──────────────────────────────────────────────
        private static string Key(int x, int y)           => $"{x},{y}";
        private static string Key(Vector2Int v)            => $"{v.x},{v.y}";

        // ── Cell Queries ─────────────────────────────────────────
        public bool IsInBounds(Vector2Int cell) =>
            cell.x >= 0 && cell.x < GridWidth &&
            cell.y >= 0 && cell.y < GridHeight;

        public bool IsOccupied(Vector2Int cell) =>
            _occupancy.ContainsKey(Key(cell)) && _occupancy[Key(cell)] != null;

        public bool IsDenied(Vector2Int cell) =>
            _deniedCells.ContainsKey(Key(cell));

        public bool IsMarrowSink(Vector2Int cell) =>
            _marrowSinks.Contains(Key(cell));

        public GridOccupant GetOccupant(Vector2Int cell)
        {
            if (!_occupancy.TryGetValue(Key(cell), out var id) || id == null) return null;
            _occupants.TryGetValue(id, out var occupant);
            return occupant;
        }

        // ── Placement ────────────────────────────────────────────
        public void PlaceOccupant(GridOccupant occupant, Vector2Int cell)
        {
            occupant.gridX = cell.x;
            occupant.gridY = cell.y;
            _occupants[occupant.occupantId] = occupant;

            // Occupy all cells in footprint
            for (int dx = 0; dx < occupant.footprintW; dx++)
            for (int dy = 0; dy < occupant.footprintH; dy++)
                _occupancy[Key(cell.x + dx, cell.y + dy)] = occupant.occupantId;

            Debug.Log($"[Grid] Placed {occupant.occupantId} at ({cell.x},{cell.y}) " +
                      $"footprint {occupant.footprintW}x{occupant.footprintH}");
        }

        public void RemoveOccupant(string occupantId)
        {
            if (!_occupants.TryGetValue(occupantId, out var occupant)) return;

            for (int dx = 0; dx < occupant.footprintW; dx++)
            for (int dy = 0; dy < occupant.footprintH; dy++)
                _occupancy.Remove(Key(occupant.gridX + dx, occupant.gridY + dy));

            _occupants.Remove(occupantId);
            Debug.Log($"[Grid] Removed {occupantId}");
        }

        public void MoveOccupant(string occupantId, Vector2Int destination)
        {
            if (!_occupants.TryGetValue(occupantId, out var occupant))
            {
                Debug.LogWarning($"[Grid] MoveOccupant: {occupantId} not found");
                return;
            }

            RemoveOccupant(occupantId);
            PlaceOccupant(occupant, destination);
            Debug.Log($"[Grid] Moved {occupantId} to ({destination.x},{destination.y})");
        }

        // ── Facing Arc ───────────────────────────────────────────
        // Determines which arc the ATTACKER is in relative to the TARGET's facing
        public FacingArc GetArcFromAttackerToTarget(
            Vector2Int attackerCell, Vector2Int targetCell, Vector2Int targetFacing)
        {
            var toAttacker = new Vector2Int(
                attackerCell.x - targetCell.x,
                attackerCell.y - targetCell.y);

            // Dot product with target's facing to determine arc
            float dot = Vector2.Dot(
                new Vector2(targetFacing.x, targetFacing.y).normalized,
                new Vector2(toAttacker.x,   toAttacker.y).normalized);

            FacingArc arc;
            if      (dot >= 0.5f)  arc = FacingArc.Rear;   // Behind target
            else if (dot <= -0.5f) arc = FacingArc.Front;  // In front of target
            else                   arc = FacingArc.Flank;

            Debug.Log($"[Grid] Arc check: attacker({attackerCell.x},{attackerCell.y}) " +
                      $"vs target({targetCell.x},{targetCell.y}) facing({targetFacing.x},{targetFacing.y}) " +
                      $"dot={dot:F2} → {arc}");
            return arc;
        }

        // ── Range & Sight ────────────────────────────────────────
        public int GetDistance(Vector2Int a, Vector2Int b) =>
            Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y)); // Chebyshev

        public Vector2Int[] GetCellsInRange(Vector2Int origin, int range)
        {
            var cells = new List<Vector2Int>();
            for (int x = origin.x - range; x <= origin.x + range; x++)
            for (int y = origin.y - range; y <= origin.y + range; y++)
            {
                var cell = new Vector2Int(x, y);
                if (IsInBounds(cell) && GetDistance(origin, cell) <= range)
                    cells.Add(cell);
            }
            return cells.ToArray();
        }

        public bool HasLineOfSight(Vector2Int from, Vector2Int to)
        {
            // Bresenham line — blocked by occupied cells between from and to
            int dx = Mathf.Abs(to.x - from.x);
            int dy = Mathf.Abs(to.y - from.y);
            int sx = from.x < to.x ? 1 : -1;
            int sy = from.y < to.y ? 1 : -1;
            int err = dx - dy;
            int cx = from.x, cy = from.y;

            while (cx != to.x || cy != to.y)
            {
                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; cx += sx; }
                if (e2 <  dx) { err += dx; cy += sy; }

                // Skip origin and destination — only check cells in between
                if ((cx == from.x && cy == from.y) || (cx == to.x && cy == to.y)) continue;
                if (IsOccupied(new Vector2Int(cx, cy))) return false;
            }
            return true;
        }

        // ── Denial ───────────────────────────────────────────────
        public void SetDenied(Vector2Int cell, bool denied, int durationRounds)
        {
            if (denied)
            {
                _deniedCells[Key(cell)] = durationRounds;
                Debug.Log($"[Grid] Cell ({cell.x},{cell.y}) denied for {durationRounds} rounds");
            }
            else
            {
                _deniedCells.Remove(Key(cell));
                Debug.Log($"[Grid] Cell ({cell.x},{cell.y}) denial cleared");
            }
        }

        public void TickDeniedCells()
        {
            var toRemove = new List<string>();
            foreach (var key in new List<string>(_deniedCells.Keys))
            {
                _deniedCells[key]--;
                if (_deniedCells[key] <= 0) toRemove.Add(key);
            }
            foreach (var key in toRemove)
            {
                _deniedCells.Remove(key);
                Debug.Log($"[Grid] Denial expired: {key}");
            }
        }

        // ── Marrow Sink Setup (called at combat start) ────────────
        public void SetMarrowSink(Vector2Int cell, bool active)
        {
            if (active) _marrowSinks.Add(Key(cell));
            else        _marrowSinks.Remove(Key(cell));
        }
    }
}
```

---

## Verification Test

Create a temporary test MonoBehaviour — delete after test passes:

**Path:** `Assets/_Game/Scripts/Core.Systems/GridManagerTest.cs` *(delete after)*

```csharp
using UnityEngine;
using MnM.Core.Data;
using MnM.Core.Systems;

public class GridManagerTest : MonoBehaviour
{
    private void Start()
    {
        var grid = gameObject.AddComponent<GridManager>();

        // Test 1: Place and query occupant
        var aldric = new GridOccupant { occupantId = "hunter_aldric", isHunter = true, footprintW = 1, footprintH = 1 };
        grid.PlaceOccupant(aldric, new Vector2Int(5, 8));
        Debug.Assert(grid.IsOccupied(new Vector2Int(5, 8)), "FAIL: cell should be occupied");
        Debug.Assert(!grid.IsOccupied(new Vector2Int(6, 8)), "FAIL: adjacent cell should be empty");

        // Test 2: Monster footprint (2×2)
        var gaunt = new GridOccupant { occupantId = "monster", isHunter = false, footprintW = 2, footprintH = 2 };
        grid.PlaceOccupant(gaunt, new Vector2Int(12, 7));
        Debug.Assert(grid.IsOccupied(new Vector2Int(12, 7)), "FAIL: monster cell 0,0 not occupied");
        Debug.Assert(grid.IsOccupied(new Vector2Int(13, 8)), "FAIL: monster cell 1,1 not occupied");

        // Test 3: Chebyshev distance
        int dist = grid.GetDistance(new Vector2Int(5, 8), new Vector2Int(12, 7));
        Debug.Assert(dist == 7, $"FAIL: distance should be 7, got {dist}");

        // Test 4: Facing arc
        var arc = grid.GetArcFromAttackerToTarget(
            new Vector2Int(5, 8),   // Aldric
            new Vector2Int(12, 7),  // Gaunt
            new Vector2Int(-1, 0)); // Gaunt facing West
        Debug.Log($"[GridTest] Arc result: {arc} (expect Front — Aldric is in front of westward-facing Gaunt)");

        // Test 5: Denial
        grid.SetDenied(new Vector2Int(10, 7), true, 2);
        Debug.Assert(grid.IsDenied(new Vector2Int(10, 7)), "FAIL: cell should be denied");
        grid.TickDeniedCells();
        grid.TickDeniedCells();
        Debug.Assert(!grid.IsDenied(new Vector2Int(10, 7)), "FAIL: denial should have expired");

        // Test 6: Move
        grid.MoveOccupant("hunter_aldric", new Vector2Int(6, 8));
        Debug.Assert(!grid.IsOccupied(new Vector2Int(5, 8)), "FAIL: old cell should be empty after move");
        Debug.Assert(grid.IsOccupied(new Vector2Int(6, 8)), "FAIL: new cell should be occupied after move");

        Debug.Log("[GridManagerTest] ✓ All assertions passed");
    }
}
```

Attach to a GameObject, Play, verify all assertions pass, **delete the test script**.

---

## Next Session

**File:** `_Docs/Stage_02/STAGE_02_D.md`  
**Covers:** Turn phase state machine (CombatManager) + DiceResolver with Debug.Log verification
