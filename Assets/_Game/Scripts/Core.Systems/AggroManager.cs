using UnityEngine;
using MnM.Core.Data;

namespace MnM.Core.Systems
{
    public class AggroManager
    {
        private CombatState _state;

        public string AggroHolderId => _state?.aggroHolderId;

        public void Initialize(CombatState state)
        {
            _state = state;
            Debug.Log($"[Aggro] Initialized. Holder: {_state.aggroHolderId}");
        }

        public void TransferAggro(string newHolderId)
        {
            string previous = _state.aggroHolderId;
            _state.aggroHolderId = newHolderId;
            Debug.Log($"[Aggro] Token transferred: {previous} → {newHolderId}");
        }

        // Special Pack rule — called by CombatManager when a wolf is killed
        public void OnWolfKilled(string killingBlowHunterId)
        {
            Debug.Log($"[Aggro] Wolf killed — aggro transfers immediately to killing blow holder");
            TransferAggro(killingBlowHunterId);
        }

        // Certain behavior cards target the most-damaged hunter (most Shell damage)
        public string GetMostExposedHunterId(HunterCombatState[] hunters)
        {
            string mostExposedId = null;
            int lowestShell = int.MaxValue;

            foreach (var hunter in hunters)
            {
                if (hunter.isCollapsed) continue;
                int totalShell = 0;
                foreach (var zone in hunter.bodyZones)
                    totalShell += zone.shellCurrent;

                if (totalShell < lowestShell)
                {
                    lowestShell   = totalShell;
                    mostExposedId = hunter.hunterId;
                }
            }

            Debug.Log($"[Aggro] Most exposed hunter: {mostExposedId} (shell total: {lowestShell})");
            return mostExposedId;
        }
    }
}
