using UnityEngine;
using MnM.Core.Data;

namespace MnM.Core.Systems
{
    public interface IGridManager
    {
        // Constants
        int GridWidth { get; }          // 22
        int GridHeight { get; }         // 16

        // Cell queries
        bool IsOccupied(Vector2Int cell);
        bool IsInBounds(Vector2Int cell);
        bool IsDenied(Vector2Int cell);             // Spear card movement denial
        bool IsMarrowSink(Vector2Int cell);         // Hazard tile
        GridOccupant GetOccupant(Vector2Int cell);

        // Placement
        void PlaceOccupant(GridOccupant occupant, Vector2Int cell);
        void RemoveOccupant(string occupantId);
        void MoveOccupant(string occupantId, Vector2Int destination);

        // Arc / facing
        // Returns which arc the attacker is in relative to the target's facing
        FacingArc GetArcFromAttackerToTarget(
            Vector2Int attackerCell,
            Vector2Int targetCell,
            Vector2Int targetFacing);

        // Range & sight
        bool HasLineOfSight(Vector2Int from, Vector2Int to);
        Vector2Int[] GetCellsInRange(Vector2Int origin, int range);
        int GetDistance(Vector2Int a, Vector2Int b); // Chebyshev: diagonal = 1

        // Denial (Spear zone control cards)
        void SetDenied(Vector2Int cell, bool denied, int durationRounds);
        void TickDeniedCells();     // Called once per round end

        // Terrain (static per-combat layout)
        void PlaceTerrain(TerrainCellState cell);
        bool IsTerrain(Vector2Int cell);
        TerrainCellState? GetTerrain(Vector2Int cell);
    }
}
