using MnM.Core.Data;
using UnityEngine;

namespace MnM.Core.Systems
{
    public enum GritWindowPhase
    {
        AfterDraw,
        AfterTargetIdentification,
        AfterMovement,
        BeforeDamageApplied,
        AfterDamageApplied,
        EndOfMonsterTurn,
    }

    public interface IMonsterAI
    {
        // CurrentGroup removed — BehaviorGroup enum removed in Stage 8-M (no escalation logic)
        int RemainingRemovableCount { get; }

        event System.Action OnMonsterDefeated;

        // Fires at each of the 6 Grit windows during monster turn execution.
        // Phase 9+ UI subscribes to pause and prompt the player.
        event System.Action<GritWindowPhase, BehaviorCardSO> OnGritWindow;

        void               InitializeDeck(MonsterSO monster, string difficulty);
        BehaviorCardSO     DrawNextCard();
        BehaviorCardResult ExecuteCard(BehaviorCardSO card, CombatState state);
        bool               RemoveCard(string cardName);
        bool               HasRemovableCards();
        BehaviorCardSO[]   GetActiveBehaviorCards();
        void               InjectGrid(IGridManager grid);
    }
}
