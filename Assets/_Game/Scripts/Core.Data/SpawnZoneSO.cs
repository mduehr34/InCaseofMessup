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
