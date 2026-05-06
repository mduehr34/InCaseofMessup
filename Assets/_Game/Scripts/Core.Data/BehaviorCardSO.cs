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

        [Header("Execution — Movement")]
        public MovementPattern movementPattern = MovementPattern.None;
        public int             movementDistance = 0;   // Cells to move (0 = no movement)

        [Header("Execution — Attack")]
        public AttackTargetType attackTargetType = AttackTargetType.None;
        public int              attackDamage     = 0;  // Base flesh damage per target (0 = no attack)
        public int              attackRange      = 1;  // Max cells to reach target (1 = adjacent/melee)

        [Header("Execution — Special")]
        public string specialTag = "";
        // Simple effect tags — resolved in MonsterAI.ApplySpecial():
        //   "PINNED"          — apply Pinned status to all adjacent hunters
        //   "REGEN:N"         — restore N flesh to the most-damaged part
        //   "STANCE:tagname"  — set MonsterCombatState.currentStanceTag
        //   "STUN_SELF"       — skip next card draw (no action next monster phase)
        //   "AGGRO:LOWEST"    — move aggro token to hunter with lowest flesh total

        [Header("Execution — Deck")]
        public bool isShuffle = false;  // Reshuffle active deck after this card resolves
    }
}
