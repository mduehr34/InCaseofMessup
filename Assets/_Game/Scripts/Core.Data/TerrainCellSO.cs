using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/Terrain/TerrainCell")]
    public class TerrainCellSO : ScriptableObject
    {
        [Header("Identity")]
        public string terrainId;            // Unique key e.g. "HighGround", "BoneAsh"
        public string displayName;

        [Header("Type")]
        public TerrainType terrainType;     // Obstacle or Bonus

        [Header("Bonus Modifiers (Bonus type only)")]
        public int accuracyBonus;           // Added to hunter accuracy for to-hit rolls while standing here
                                            // Positive = advantage (elevated position, large object)
                                            // Negative = penalty (sticky mud, unstable ground)
        public int defenseBonus;            // Subtracted from incoming Force Check rolls vs this hunter

        [Header("Movement")]
        [Min(1)] public int movementCost = 1; // BFS entry cost — 2 = slow terrain (mud); 1 = normal

        [Header("Visual")]
        public string cssClass;             // e.g. "grid-cell--terrain-high" or "grid-cell--terrain-ash"
        [TextArea] public string flavourText;

        // ── Deferred: Terrain Buff/Resource System ────────────────
        // See _Docs/Stage_08/STAGE_08_TerrainFX.md for full implementation spec.
        [Header("On-Enter Buff (STAGE_08_TerrainFX)")]
        public string buffOnEnterTag;           // StatusEffect tag applied when hunter enters this cell.
                                                // e.g. "Energized", "Shielded". Empty = none.
        public int    buffDurationRounds;       // How many rounds the buff lasts.

        [Header("Resource Grant (STAGE_08_TerrainFX)")]
        public string resourceGrantTag;         // Resource type granted on first entry per combat.
                                                // Wire to ResourceManager in STAGE_08_TerrainFX.
        public int    resourceGrantAmount;      // Amount of resource granted.
    }

    public enum TerrainType { Obstacle, Bonus }
}
