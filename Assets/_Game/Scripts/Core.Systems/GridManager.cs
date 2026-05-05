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
        private Dictionary<string, string>       _occupancy   = new();
        private Dictionary<string, GridOccupant> _occupants   = new();
        private Dictionary<string, int>          _deniedCells = new(); // key:"x,y", value:roundsRemaining
        private HashSet<string>                  _marrowSinks = new();

        // ── Helpers ──────────────────────────────────────────────
        private static string Key(int x, int y) => $"{x},{y}";
        private static string Key(Vector2Int v)  => $"{v.x},{v.y}";

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
            if (!_occupants.TryGetValue(occupantId, out _))
            {
                Debug.LogWarning($"[Grid] MoveOccupant: {occupantId} not found");
                return;
            }

            // Cache the occupant before RemoveOccupant clears it from _occupants
            var occupant = _occupants[occupantId];
            RemoveOccupant(occupantId);
            PlaceOccupant(occupant, destination);
            Debug.Log($"[Grid] Moved {occupantId} to ({destination.x},{destination.y})");
        }

        // ── Facing Arc ───────────────────────────────────────────
        // Returns which arc the attacker is in relative to the target's facing
        public FacingArc GetArcFromAttackerToTarget(
            Vector2Int attackerCell, Vector2Int targetCell, Vector2Int targetFacing)
        {
            var toAttacker = new Vector2Int(
                attackerCell.x - targetCell.x,
                attackerCell.y - targetCell.y);

            float dot = Vector2.Dot(
                new Vector2(targetFacing.x, targetFacing.y).normalized,
                new Vector2(toAttacker.x,   toAttacker.y).normalized);

            FacingArc arc;
            if      (dot >= 0.5f)  arc = FacingArc.Front;  // Attacker is in the direction target faces
            else if (dot <= -0.5f) arc = FacingArc.Rear;   // Attacker is opposite to facing direction
            else                   arc = FacingArc.Flank;

            Debug.Log($"[Grid] Arc check: attacker({attackerCell.x},{attackerCell.y}) " +
                      $"vs target({targetCell.x},{targetCell.y}) facing({targetFacing.x},{targetFacing.y}) " +
                      $"dot={dot:F2} → {arc}");
            return arc;
        }

        // ── Range & Sight ────────────────────────────────────────
        public int GetDistance(Vector2Int a, Vector2Int b) =>
            Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y); // Manhattan — each cell step costs 1

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
