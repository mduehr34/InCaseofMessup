using UnityEngine;

namespace MnM.Core.Logic
{
    public class ComboTracker
    {
        private bool   _comboActive = false;
        private string _hunterId;

        public bool IsComboActive => _comboActive;

        // Called when an Opener card is played
        public void OnOpenerPlayed(string hunterId)
        {
            _comboActive = true;
            _hunterId    = hunterId;
            Debug.Log($"[Combo] Combo started by {hunterId}");
        }

        // Called when a Linker card is played — combo must already be active
        public void OnLinkerPlayed(string hunterId)
        {
            if (!_comboActive)
                Debug.LogWarning($"[Combo] Linker played by {hunterId} outside of active combo");
            else
                Debug.Log($"[Combo] Combo continued by {hunterId}");
        }

        // Called when a Finisher card is played — ends the combo
        public void OnFinisherPlayed(string hunterId)
        {
            _comboActive = false;
            Debug.Log($"[Combo] Combo ended with Finisher by {hunterId}");
        }

        // Called when a hunter's turn ends — any unfinished combo breaks
        public void OnHunterTurnEnd(string hunterId)
        {
            if (_comboActive)
                Debug.Log($"[Combo] Combo BROKEN — {hunterId} ended turn without Finisher");
            _comboActive = false;
            _hunterId    = null;
        }

        // Notify combo tracker of card category — called from CombatManager.TryPlayCard
        public void NotifyCardPlayed(string hunterId, MnM.Core.Data.CardCategory category)
        {
            switch (category)
            {
                case MnM.Core.Data.CardCategory.Opener:
                    OnOpenerPlayed(hunterId);
                    break;
                case MnM.Core.Data.CardCategory.Linker:
                    OnLinkerPlayed(hunterId);
                    break;
                case MnM.Core.Data.CardCategory.Finisher:
                    OnFinisherPlayed(hunterId);
                    break;
                // BasicAttack, Reaction, Signature don't affect combo state
            }
        }
    }
}
