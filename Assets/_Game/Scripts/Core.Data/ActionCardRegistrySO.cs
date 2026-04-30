using System.Collections.Generic;
using UnityEngine;

namespace MnM.Core.Data
{
    /// <summary>
    /// Maps card name strings (used in HunterCombatState.handCardNames) to ActionCardSO assets.
    /// Place one instance at Assets/_Game/Data/Resources/ActionCardRegistry.asset.
    /// Populate the 'cards' array in the Inspector — drag all ActionCardSOs in.
    /// </summary>
    [CreateAssetMenu(menuName = "MnM/Cards/ActionCardRegistry", fileName = "ActionCardRegistry")]
    public class ActionCardRegistrySO : ScriptableObject
    {
        public ActionCardSO[] cards;

        private Dictionary<string, ActionCardSO> _lookup;

        public ActionCardSO Get(string cardName)
        {
            if (_lookup == null)
                BuildLookup();

            _lookup.TryGetValue(cardName, out var result);
            return result;
        }

        private void BuildLookup()
        {
            _lookup = new Dictionary<string, ActionCardSO>(System.StringComparer.OrdinalIgnoreCase);
            if (cards == null) return;
            foreach (var card in cards)
            {
                if (card == null) continue;
                if (!_lookup.ContainsKey(card.cardName))
                    _lookup[card.cardName] = card;
                else
                    Debug.LogWarning($"[CardRegistry] Duplicate card name: '{card.cardName}' — second entry ignored");
            }
        }

        private void OnValidate() => _lookup = null; // Rebuild on Inspector change
    }
}
