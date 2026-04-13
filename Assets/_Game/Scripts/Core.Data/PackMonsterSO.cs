using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/PackMonster", fileName = "New PackMonster")]
    public class PackMonsterSO : MonsterSO
    {
        [Header("Pack-Specific")]
        public int wolfCount;               // Always 3 for The Pack
        // Shared deck defined in base MonsterSO openingCards/escalationCards/apexCards
        // Each wolf's health tracked at runtime — not stored here
        // Aggro rule on kill: KILLING_BLOW_HOLDER (constant, no data field needed)
    }
}
