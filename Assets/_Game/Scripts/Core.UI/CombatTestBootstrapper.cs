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
        [SerializeField] private MnM.Core.Data.MonsterSO _mockMonsterSO;

        private void Start()
        {
            if (_combatManager == null)
            {
                Debug.LogError("[Bootstrapper] CombatManager not assigned");
                return;
            }

            var gsm = GameStateManager.Instance;
            var realState = gsm?.CombatState;

            if (realState != null && realState.hunters != null && realState.hunters.Length > 0)
            {
                _combatManager.StartCombat(realState);
                if (gsm.SelectedMonster != null)
                    _combatManager.InitializeMonsterAI(gsm.SelectedMonster, gsm.SelectedDifficulty ?? "Standard");
                else
                    Debug.LogWarning("[Bootstrapper] Real state found but SelectedMonster is null — MonsterAI not initialized");
                Debug.Log($"[Bootstrapper] Real combat started — " +
                          $"{realState.hunters.Length} hunters vs {realState.monster.monsterName}");
            }
            else
            {
                var mockState = CombatStateFactory.BuildMockCombatState();
                _combatManager.StartCombat(mockState);
                if (_mockMonsterSO != null)
                    _combatManager.InitializeMonsterAI(_mockMonsterSO, "Standard");
                else
                    Debug.LogWarning("[Bootstrapper] _mockMonsterSO not assigned — MonsterAI not initialized. Assign Monster_Gaunt in Inspector.");
                Debug.Log("[Bootstrapper] Mock combat started — Aldric vs The Gaunt Standard " +
                          "(no real CombatState found — enter via Settlement→Hunt for real data)");
            }
        }
    }
}
