using MnM.Core.Data;

namespace MnM.Core.Systems
{
    public interface IMonsterAI
    {
        // Deck state
        BehaviorGroup CurrentGroup { get; }
        int RemainingRemovableCount { get; }
        bool HasRemovableCards();

        // Draw and execute
        BehaviorCardSO DrawNextCard();
        void ExecuteCard(BehaviorCardSO card, CombatState state);

        // Deck manipulation — called mid-turn by CombatManager on part break/wound
        void RemoveCard(string cardName);

        // Group progression — called during Behavior Refresh phase
        void AdvanceGroupIfExhausted();

        // Apex trigger — called by CombatManager on first part break (not automatic)
        void TriggerApex();

        // Initialization
        void InitializeDeck(MonsterSO monster, string difficulty);
    }
}
