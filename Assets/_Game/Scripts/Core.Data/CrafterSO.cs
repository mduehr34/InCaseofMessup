using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/Crafter", fileName = "New Crafter")]
    public class CrafterSO : ScriptableObject
    {
        public string crafterName;
        public string monsterTag;               // Monster materials that unlock this
        public int materialTier;
        public ItemSO[] recipeList;
        public ResourceSO[] unlockCost;
        public int[] unlockCostAmounts;         // Parallel array with unlockCost
        // Settlement scene placement
        public Vector2 settlementScenePosition;
        public Sprite structureSprite;
    }
}
