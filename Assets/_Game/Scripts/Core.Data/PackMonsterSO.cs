using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/PackMonster", fileName = "New PackMonster")]
    public class PackMonsterSO : MonsterSO
    {
        [Header("Herd-Specific")]
        public int unitCount;               // Always 3 for The Ivory Stampede
        // Shared deck defined in base MonsterSO openingCards/escalationCards/apexCards
        // Each elephant's health tracked at runtime — not stored here
        // Aggro rule on kill: KILLING_BLOW_HOLDER (constant, no data field needed)
    }
}
