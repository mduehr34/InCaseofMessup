using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/Cards/InjuryCard", fileName = "New InjuryCard")]
    public class InjuryCardSO : ScriptableObject
    {
        public string injuryName;
        public BodyPartTag bodyPartTag;
        public InjurySeverity severity;
        [TextArea] public string effect;
        public string removalCondition;     // e.g. "Settlement healing action"
    }
}
