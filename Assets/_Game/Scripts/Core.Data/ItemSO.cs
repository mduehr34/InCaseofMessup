using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/Item", fileName = "New Item")]
    public class ItemSO : ScriptableObject
    {
        public string itemName;
        public int materialTier;                // 1–4
        public Vector2Int gridDimensions;       // e.g. (2,2) for a 2×2 gear item
        public bool isConsumable;
        public string setNameTag;               // Empty if not part of a set

        [Header("Stat Modifiers")]
        public int accuracyMod;
        public int strengthMod;
        public int toughnessMod;
        public int evasionMod;
        public int luckMod;
        public int movementMod;

        [Header("Links & Affinity")]
        public string[] affinityTags;
        public LinkPoint[] linkPoints;

        [Header("Effect & Crafting")]
        [TextArea] public string specialEffect;
        public ResourceSO[] craftingCost;
        public int[] craftingCostAmounts;       // Parallel array with craftingCost
    }
}
