using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/GuidingPrincipal", fileName = "New GuidingPrincipal")]
    public class GuidingPrincipalSO : ScriptableObject
    {
        public string principalId;          // e.g. "GP-01"
        public string principalName;
        [TextArea] public string triggerCondition;
        public EventChoice choiceA;
        public EventChoice choiceB;
    }
}
