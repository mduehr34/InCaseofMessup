using System.Collections.Generic;
using UnityEngine;
using MnM.Core.Data;

namespace MnM.Core.Systems
{
    public class WoundDeck
    {
        private List<WoundLocationSO> _deck    = new();
        private List<WoundLocationSO> _discard = new();

        public int DeckCount    => _deck.Count;
        public int DiscardCount => _discard.Count;

        public void Build(WoundLocationSO[] locations)
        {
            _deck.Clear();
            _discard.Clear();
            if (locations == null || locations.Length == 0) return;
            _deck.AddRange(locations);
            Shuffle(_deck);
            Debug.Log($"[WoundDeck] Built with {_deck.Count} locations");
        }

        /// <summary>Draw top wound location. Reshuffles discard (including traps) if deck empty.</summary>
        public WoundLocationSO Draw()
        {
            if (_deck.Count == 0)
            {
                if (_discard.Count == 0)
                {
                    Debug.LogWarning("[WoundDeck] Both deck and discard empty");
                    return null;
                }
                ReshuffleDiscardIntoDeck();
            }
            var loc = _deck[0];
            _deck.RemoveAt(0);
            return loc;
        }

        /// <summary>
        /// Send location to discard. For trap cards, caller should then immediately
        /// call ReshuffleDiscardIntoDeck() so the trap cycles back in.
        /// </summary>
        public void SendToDiscard(WoundLocationSO location)
        {
            _discard.Add(location);
        }

        public void ReshuffleDiscardIntoDeck()
        {
            _deck.AddRange(_discard);
            _discard.Clear();
            Shuffle(_deck);
            Debug.Log($"[WoundDeck] Discard reshuffled. Deck size: {_deck.Count}");
        }

        private static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
