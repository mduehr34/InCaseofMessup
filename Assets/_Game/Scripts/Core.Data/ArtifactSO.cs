using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/Artifact", fileName = "New Artifact")]
    public class ArtifactSO : ScriptableObject
    {
        public string artifactId;
        public string artifactName;
        public CodexCategory codexCategory;
        [TextArea] public string loreText;
        public string unlockCondition;
        // yearFound set at runtime — not stored in SO
    }
}
