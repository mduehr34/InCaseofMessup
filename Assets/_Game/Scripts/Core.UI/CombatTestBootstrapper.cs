using UnityEngine;
using MnM.Core.Data;
using MnM.Core.Systems;

namespace MnM.Core.UI
{
    /// <summary>
    /// Temporary test bootstrapper — seeds CombatManager with mock data on Start.
    /// Remove or disable before shipping Stage 6.
    /// </summary>
    public class CombatTestBootstrapper : MonoBehaviour
    {
        [SerializeField] private CombatManager _combatManager;

        private void Start()
        {
            if (_combatManager == null)
            {
                Debug.LogError("[Bootstrapper] CombatManager not assigned");
                return;
            }

            // Prefer the real CombatState built by GameStateManager (normal hunt flow).
            // Fall back to mock only when entering CombatScene directly (editor testing).
            var realState = GameStateManager.Instance?.CombatState;
            if (realState != null && realState.hunters != null && realState.hunters.Length > 0)
            {
                _combatManager.StartCombat(realState);
                Debug.Log($"[Bootstrapper] Real combat started — " +
                          $"{realState.hunters.Length} hunters vs {realState.monster.monsterName}");
            }
            else
            {
                var mockState = CombatStateFactory.BuildMockCombatState();
                _combatManager.StartCombat(mockState);
                Debug.Log("[Bootstrapper] Mock combat started — Aldric vs The Gaunt Standard " +
                          "(no real CombatState found — enter via Settlement→Hunt for real data)");
            }
        }
    }
}
