using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/Resource", fileName = "New Resource")]
    public class ResourceSO : ScriptableObject
    {
        public string resourceName;
        public ResourceType type;
        public int tier;                // 1–4
        public float conversionRate;    // e.g. 2 UniqueCommon = 1 UniqueUncommon
    }
}
