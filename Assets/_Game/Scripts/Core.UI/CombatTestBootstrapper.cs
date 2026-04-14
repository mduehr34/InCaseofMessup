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

            var mockState = CombatStateFactory.BuildMockCombatState();
            _combatManager.StartCombat(mockState);
            Debug.Log("[Bootstrapper] Mock combat started — Aldric vs The Gaunt Standard");
        }
    }
}
