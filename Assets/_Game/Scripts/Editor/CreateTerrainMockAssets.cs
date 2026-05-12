using UnityEngine;
using UnityEditor;
using MnM.Core.Data;

namespace MnM.Editor
{
    public static class CreateTerrainMockAssets
    {
        // Monster mock spawn: gridX=12 gridY=7 footprint 2x2 -> occupies (12-13, 7-8).
        // All terrain placed adjacent to or within 2 cells of the monster for testability.
        [MenuItem("MnM/Create Mock Terrain Assets")]
        public static void Execute()
        {
            const string terrainDir = "Assets/_Game/Data/Terrain";
            System.IO.Directory.CreateDirectory(Application.dataPath + "/../" + terrainDir);
            AssetDatabase.Refresh();

            // ── TerrainCellSO assets ──────────────────────────────
            var highGround = CreateTerrain(terrainDir, "Terrain_HighGround",
                "HighGround", "High Ground", TerrainType.Bonus,
                accuracyBonus: 1, defenseBonus: 0, movementCost: 1,
                cssClass: "grid-cell--terrain-high",
                flavour: "Hunter elevated on rock or rubble — easier to strike from above.");

            var boneAsh = CreateTerrain(terrainDir, "Terrain_BoneAsh",
                "BoneAsh", "Bone Ash", TerrainType.Bonus,
                accuracyBonus: 0, defenseBonus: 1, movementCost: 1,
                cssClass: "grid-cell--terrain-ash",
                flavour: "Soft ash and ground-down bone absorb the impact of incoming blows.");

            var stickyMud = CreateTerrain(terrainDir, "Terrain_StickyMud",
                "StickyMud", "Sticky Mud", TerrainType.Bonus,
                accuracyBonus: -1, defenseBonus: 0, movementCost: 2,
                cssClass: "grid-cell--terrain-mud",
                flavour: "Hunter stuck in mud — accuracy suffers and costs 2 movement to enter.");

            var rockObstacle = CreateTerrain(terrainDir, "Terrain_Rock",
                "Rock", "Rocky Outcrop", TerrainType.Obstacle,
                accuracyBonus: 0, defenseBonus: 0, movementCost: 1,
                cssClass: "grid-cell--terrain-obstacle",
                flavour: "Impassable rock formation — blocks movement entirely.");

            // ── TerrainSetupSO: Gaunt Arena ───────────────────────
            // (11,7)  HighGround — left flank, adjacent to monster left edge  → attack with +1 accuracy
            // (14,7)  HighGround — right flank, adjacent to monster right edge → attack with +1 accuracy
            // (12,9)  BoneAsh   — 1 step below monster bottom edge            → +1 defense vs hits
            // (10,7)  StickyMud — 2 steps left (passable but slow, costs 2)  → -1 accuracy, movement tax
            // (15,8)  StickyMud — 2 steps right (passable but slow, costs 2) → -1 accuracy, movement tax
            // (11,6)  Rock      — blocks upper-left approach to monster        → impassable
            const string assetPath = terrainDir + "/TerrainSetup_GauntArena.asset";
            var setup = AssetDatabase.LoadAssetAtPath<TerrainSetupSO>(assetPath);
            if (setup == null)
            {
                setup = ScriptableObject.CreateInstance<TerrainSetupSO>();
                AssetDatabase.CreateAsset(setup, assetPath);
            }

            setup.entries = new TerrainSetupSO.TerrainEntry[]
            {
                new TerrainSetupSO.TerrainEntry { terrain = highGround,   gridX = 11, gridY = 7, footprintW = 1, footprintH = 1 },
                new TerrainSetupSO.TerrainEntry { terrain = highGround,   gridX = 14, gridY = 7, footprintW = 1, footprintH = 1 },
                new TerrainSetupSO.TerrainEntry { terrain = boneAsh,      gridX = 12, gridY = 9, footprintW = 1, footprintH = 1 },
                new TerrainSetupSO.TerrainEntry { terrain = stickyMud,    gridX = 10, gridY = 7, footprintW = 1, footprintH = 1 },
                new TerrainSetupSO.TerrainEntry { terrain = stickyMud,    gridX = 15, gridY = 8, footprintW = 1, footprintH = 1 },
                // 2×1 rock wall — demonstrates multi-cell footprint; expands to (11,6) and (12,6)
                new TerrainSetupSO.TerrainEntry { terrain = rockObstacle, gridX = 11, gridY = 6, footprintW = 2, footprintH = 1 },
            };

            EditorUtility.SetDirty(setup);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[MnM] Terrain mock assets rebuilt:");
            Debug.Log("[MnM]   Terrain_HighGround — Bonus +1 acc,  at (11,7)+(14,7) adjacent to Gaunt");
            Debug.Log("[MnM]   Terrain_BoneAsh    — Bonus +1 def,  at (12,9) below Gaunt");
            Debug.Log("[MnM]   Terrain_StickyMud  — Bonus -1 acc, moveCost=2, at (10,7)+(15,8)");
            Debug.Log("[MnM]   Terrain_Rock        — Obstacle (impassable), at (11,6)");
        }

        private static TerrainCellSO CreateTerrain(
            string dir, string assetName, string id, string displayName,
            TerrainType type, int accuracyBonus, int defenseBonus, int movementCost,
            string cssClass, string flavour)
        {
            string path = $"{dir}/{assetName}.asset";
            var so = AssetDatabase.LoadAssetAtPath<TerrainCellSO>(path);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<TerrainCellSO>();
                AssetDatabase.CreateAsset(so, path);
            }
            so.terrainId     = id;
            so.displayName   = displayName;
            so.terrainType   = type;
            so.accuracyBonus = accuracyBonus;
            so.defenseBonus  = defenseBonus;
            so.movementCost  = movementCost;
            so.cssClass      = cssClass;
            so.flavourText   = flavour;
            EditorUtility.SetDirty(so);
            return so;
        }
    }
}
