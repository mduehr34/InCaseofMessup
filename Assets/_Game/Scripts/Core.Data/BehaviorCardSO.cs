using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/Cards/BehaviorCard", fileName = "New BehaviorCard")]
    public class BehaviorCardSO : ScriptableObject
    {
        public string cardName;
        public BehaviorCardType cardType;
        public BehaviorGroup group;
        [TextArea] public string triggerCondition;
        [TextArea] public string effectDescription;
        public string removalCondition;     // e.g. "Throat Shell break"
        public string stanceTag;
        public string groupTag;
        // Logic resolved by MnM.Core.Systems — no logic in this class
    }
}
