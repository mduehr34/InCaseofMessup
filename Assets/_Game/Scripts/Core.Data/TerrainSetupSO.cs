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
            [Min(1)] public int footprintW;     // Cells wide  — default 1
            [Min(1)] public int footprintH;     // Cells tall  — default 1
        }
    }
}
