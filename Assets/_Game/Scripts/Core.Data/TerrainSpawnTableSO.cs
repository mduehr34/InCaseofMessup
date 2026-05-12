using UnityEngine;

namespace MnM.Core.Data
{
    // ── Deferred: STAGE_08_TerrainGen ────────────────────────────────────────
    // Random terrain placement system. Each MonsterSO references a TerrainSpawnTableSO
    // to define which terrain types can appear in its arena and how many.
    // CombatManager.StartCombat resolves this AFTER the hand-authored TerrainSetupSO pass,
    // so fixed landmarks (starting rocks, key zones) can coexist with randomised flavour.
    //
    // See _Docs/Stage_08/STAGE_08_TerrainGen.md for the full implementation spec.
    // ─────────────────────────────────────────────────────────────────────────
    [CreateAssetMenu(menuName = "MnM/Terrain/TerrainSpawnTable")]
    public class TerrainSpawnTableSO : ScriptableObject
    {
        public TerrainSpawnRule[] rules;

        [System.Serializable]
        public struct TerrainSpawnRule
        {
            public TerrainCellSO terrain;

            [Tooltip("How many instances to place (each is 1×1 unless footprintW/H added here later).")]
            [Min(0)] public int count;

            [Tooltip("Minimum Chebyshev distance from any monster footprint cell. Prevents terrain spawning inside or directly on top of the monster.")]
            [Min(0)] public int minDistFromMonster;

            [Tooltip("Maximum Chebyshev distance from the nearest monster footprint cell. Keeps terrain in the fight zone.")]
            [Min(1)] public int maxDistFromMonster;

            [Tooltip("If true, candidate cells inside any SpawnZoneSO are excluded so terrain doesn't block hunter deployment.")]
            public bool avoidSpawnZones;

            [Tooltip("If true, candidate cells that are already occupied by another terrain piece are skipped.")]
            public bool avoidOtherTerrain;
        }
    }
}
