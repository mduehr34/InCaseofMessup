using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/Cards/ActionCard", fileName = "New ActionCard")]
    public class ActionCardSO : ScriptableObject
    {
        public string cardName;
        public WeaponType weaponType;
        public CardCategory category;
        public int apCost;
        public int apRefund;
        public bool isLoud;
        public bool isReaction;
        public int proficiencyTierRequired;     // 1–5
        [TextArea] public string flavorText;
        [TextArea] public string effectDescription;
        // Effect resolution handled by MnM.Core.Logic — no logic in this class
    }
}
