using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/Weapon", fileName = "New Weapon")]
    public class WeaponSO : ScriptableObject
    {
        public string weaponName;
        public WeaponType weaponType;
        public ElementTag elementTag;
        public int accuracyMod;
        public int strengthMod;
        public int attacksPerTurn;          // 1–3
        public int range;                   // 0 = adjacent; 2 = 2 tiles
        public bool isAlwaysLoud;
        public ActionCardSO signatureCard;

        [Header("Proficiency Deck Unlocks")]
        // Index 0 = Tier 1 cards, index 1 = Tier 2, etc.
        public ActionCardSO[] tier1Cards;
        public ActionCardSO[] tier2Cards;
        public ActionCardSO[] tier3Cards;
        public ActionCardSO[] tier4Cards;
        public ActionCardSO[] tier5Cards;

        [Header("Crafting")]
        public ResourceSO[] craftingCost;
        public int[] craftingCostAmounts;       // Parallel array with craftingCost

        [Header("Identity")]
        [TextArea] public string uniqueCapability;
        [TextArea] public string genuineCost;
    }
}
