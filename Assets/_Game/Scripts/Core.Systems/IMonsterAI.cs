using MnM.Core.Data;
using UnityEngine;

namespace MnM.Core.Systems
{
    public interface IMonsterAI
    {
        // CurrentGroup removed — BehaviorGroup enum removed in Stage 8-M (no escalation logic)
        int RemainingRemovableCount { get; }

        event System.Action OnMonsterDefeated;

        void               InitializeDeck(MonsterSO monster, string difficulty);
        BehaviorCardSO     DrawNextCard();
        BehaviorCardResult ExecuteCard(BehaviorCardSO card, CombatState state);
        bool               RemoveCard(string cardName);
        bool               HasRemovableCards();
        BehaviorCardSO[]   GetActiveBehaviorCards();
        void               InjectGrid(IGridManager grid);
    }
}
