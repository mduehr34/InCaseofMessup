using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/Innovation", fileName = "New Innovation")]
    public class InnovationSO : ScriptableObject
    {
        public string innovationId;         // e.g. "INN-01"
        public string innovationName;
        [TextArea] public string effect;
        // Cards added to the Innovation Deck pool when this is adopted
        public InnovationSO[] addsToDeck;
        public string gritSkillUnlocked;    // Empty if none
    }
}
