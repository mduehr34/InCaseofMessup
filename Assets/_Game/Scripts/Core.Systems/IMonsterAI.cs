using MnM.Core.Data;
using UnityEngine;

namespace MnM.Core.Systems
{
    public interface IMonsterAI
    {
        BehaviorGroup CurrentGroup          { get; }
        int           RemainingRemovableCount { get; }

        event System.Action OnMonsterDefeated;

        void               InitializeDeck(MonsterSO monster, string difficulty);
        BehaviorCardSO     DrawNextCard();
        BehaviorCardResult ExecuteCard(BehaviorCardSO card, CombatState state); // changed: was void
        bool               RemoveCard(string cardName); // returns true if the card was found and removed
        void               TriggerApex();
        void               AdvanceGroupIfExhausted();
        bool               HasRemovableCards();
        BehaviorCardSO[]   GetActiveBehaviorCards();
        void               InjectGrid(IGridManager grid);
    }
}
