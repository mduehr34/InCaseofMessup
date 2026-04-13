using UnityEngine;
using MnM.Core.Data;

namespace MnM.Core.Systems
{
    public interface ICombatManager
    {
        // State — full JSON-serializable snapshot at all times
        CombatState CurrentState { get; }
        CombatPhase CurrentPhase { get; }

        // Lifecycle
        void StartCombat(CombatState initialState);
        void AdvancePhase();
        bool IsCombatOver(out CombatResult result);

        // Hunter actions
        bool TryPlayCard(string hunterId, string cardName, Vector2Int targetCell);
        bool TryMoveHunter(string hunterId, Vector2Int destination);
        void EndHunterTurn(string hunterId);

        // Monster actions — called by IMonsterAI, not by UI
        void ExecuteBehaviorCard(string behaviorCardName);

        // Events — UI and other systems subscribe to these
        event System.Action<CombatPhase> OnPhaseChanged;
        event System.Action<string, int, DamageType> OnDamageDealt;   // id, amount, type
        event System.Action<string> OnEntityCollapsed;                  // occupantId
        event System.Action<CombatResult> OnCombatEnded;
    }
}
