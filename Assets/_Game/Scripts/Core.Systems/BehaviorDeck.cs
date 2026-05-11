using System.Collections.Generic;
using UnityEngine;
using MnM.Core.Data;

namespace MnM.Core.Systems
{
    /// <summary>
    /// Position-aware ordered list of BehaviorCardSOs.
    /// Index 0 = top of deck (next to draw).
    /// All deck manipulation goes through this class — no raw List access at call sites.
    /// </summary>
    public class BehaviorDeck
    {
        private List<BehaviorCardSO> _deck              = new();
        private List<BehaviorCardSO> _discard           = new();
        private List<BehaviorCardSO> _moodInPlay        = new();
        private List<BehaviorCardSO> _permanentlyRemoved = new();

        // ── Read-only counts (for CombatState sync) ─────────────────
        public int DeckCount               => _deck.Count;
        public int DiscardCount            => _discard.Count;
        public int MoodInPlayCount         => _moodInPlay.Count;
        public int PermanentlyRemovedCount => _permanentlyRemoved.Count;

        /// <summary>
        /// Health pool = deck + discard + moodInPlay.
        /// PermanentlyRemoved and SingleTrigger-fired cards do NOT count.
        /// </summary>
        public int HealthPool => _deck.Count + _discard.Count + _moodInPlay.Count;

        // ── Build ────────────────────────────────────────────────────

        /// <summary>
        /// Construct the starting deck from pool arrays and a composition.
        /// Shuffles each pool independently (Fisher-Yates), takes the first N cards,
        /// then combines and shuffles the combined list.
        /// </summary>
        public void Build(MonsterSO monster, int difficultyIndex)
        {
            _deck.Clear();
            _discard.Clear();
            _moodInPlay.Clear();
            _permanentlyRemoved.Clear();

            if (difficultyIndex < 0 || difficultyIndex >= monster.deckCompositions.Length)
            {
                Debug.LogError($"[BehaviorDeck] Invalid difficulty index {difficultyIndex} for {monster.monsterName}");
                return;
            }

            var comp     = monster.deckCompositions[difficultyIndex];
            var combined = new List<BehaviorCardSO>();

            combined.AddRange(DrawFromPool(monster.baseCardPool,         comp.baseCardCount));
            combined.AddRange(DrawFromPool(monster.advancedCardPool,     comp.advancedCardCount));
            combined.AddRange(DrawFromPool(monster.overwhelmingCardPool, comp.overwhelmingCardCount));

            Shuffle(combined);
            _deck.AddRange(combined);

            Debug.Log($"[BehaviorDeck] Built for {monster.monsterName} (difficulty {difficultyIndex}): " +
                      $"{comp.baseCardCount} base + {comp.advancedCardCount} advanced + " +
                      $"{comp.overwhelmingCardCount} overwhelming = {_deck.Count} cards (health)");
        }

        private static List<BehaviorCardSO> DrawFromPool(BehaviorCardSO[] pool, int count)
        {
            if (pool == null || pool.Length == 0 || count <= 0) return new List<BehaviorCardSO>();
            var shuffled = new List<BehaviorCardSO>(pool);
            Shuffle(shuffled);
            int take = Mathf.Min(count, shuffled.Count);
            return shuffled.GetRange(0, take);
        }

        // ── Draw & Peek ──────────────────────────────────────────────

        /// <summary>Removes and returns the top card. Reshuffles discard if deck is empty.</summary>
        public BehaviorCardSO Draw()
        {
            if (_deck.Count == 0)
            {
                if (_discard.Count == 0)
                {
                    Debug.LogWarning("[BehaviorDeck] Both deck and discard are empty — monster should be defeated");
                    return null;
                }
                ReshuffleDiscardIntoDeck();
            }

            var card = _deck[0];
            _deck.RemoveAt(0);
            return card;
        }

        public BehaviorCardSO PeekTop()
            => _deck.Count > 0 ? _deck[0] : null;

        public List<BehaviorCardSO> PeekTop(int n)
        {
            int take = Mathf.Min(n, _deck.Count);
            return _deck.GetRange(0, take);
        }

        // ── Card Resolution ──────────────────────────────────────────

        /// <summary>After resolving: Removable cards go to discard.</summary>
        public void SendToDiscard(BehaviorCardSO card)
        {
            _discard.Add(card);
        }

        /// <summary>Mood cards enter the in-play zone (ongoing effect active).</summary>
        public void SendToMoodInPlay(BehaviorCardSO card)
        {
            _moodInPlay.Add(card);
        }

        /// <summary>
        /// SingleTrigger (and cards removed by wounds) go here — permanently out of the health pool.
        /// </summary>
        public void SendToPermanentlyRemoved(BehaviorCardSO card)
        {
            _permanentlyRemoved.Add(card);
            Debug.Log($"[BehaviorDeck] '{card.cardName}' permanently removed. " +
                      $"Health pool: deck={_deck.Count} discard={_discard.Count} mood={_moodInPlay.Count}");
        }

        /// <summary>
        /// Remove a Mood card from the in-play zone — its removalCondition was met.
        /// Card goes to discard (re-enters health pool; can be reshuffled and drawn again).
        /// </summary>
        public void RemoveMoodCard(BehaviorCardSO card)
        {
            if (_moodInPlay.Remove(card))
            {
                _discard.Add(card);
                Debug.Log($"[BehaviorDeck] Mood card '{card.cardName}' removed from play → discard. " +
                          $"Health pool: {HealthPool}");
            }
        }

        // ── Wound Removal ────────────────────────────────────────────

        /// <summary>
        /// Default wound removal: remove top card of deck → permanentlyRemoved.
        /// If deck is empty, shuffles discard first.
        /// </summary>
        public BehaviorCardSO RemoveTopCard()
        {
            if (_deck.Count == 0)
            {
                if (_discard.Count == 0) return null;
                ReshuffleDiscardIntoDeck();
            }
            var card = _deck[0];
            _deck.RemoveAt(0);
            _permanentlyRemoved.Add(card);
            Debug.Log($"[BehaviorDeck] Wound removal: '{card.cardName}' permanently removed. " +
                      $"Health pool: deck={_deck.Count} discard={_discard.Count} mood={_moodInPlay.Count}");
            return card;
        }

        /// <summary>
        /// Remove a specific card by name (for RemoveCard(string) backward compat and Grit spends by name).
        /// Searches deck first, then discard. Returns true if found and removed.
        /// </summary>
        public bool RemoveByName(string cardName)
        {
            var inDeck = _deck.Find(c => c.cardName == cardName || c.name == cardName);
            if (inDeck != null)
            {
                _deck.Remove(inDeck);
                _permanentlyRemoved.Add(inDeck);
                Debug.Log($"[BehaviorDeck] RemoveByName: '{cardName}' removed from deck. Health pool: {HealthPool}");
                return true;
            }
            var inDiscard = _discard.Find(c => c.cardName == cardName || c.name == cardName);
            if (inDiscard != null)
            {
                _discard.Remove(inDiscard);
                _permanentlyRemoved.Add(inDiscard);
                Debug.Log($"[BehaviorDeck] RemoveByName: '{cardName}' removed from discard. Health pool: {HealthPool}");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Grit spend: choose which behavior card is removed on this wound.
        /// </summary>
        public bool RemoveSpecific(BehaviorCardSO card)
        {
            if (_deck.Remove(card) || _discard.Remove(card))
            {
                _permanentlyRemoved.Add(card);
                Debug.Log($"[BehaviorDeck] Specific removal: '{card.cardName}' permanently removed (Grit spend). " +
                          $"Health pool: {HealthPool}");
                return true;
            }
            return false;
        }

        // ── Deck Operations (for Hunter Abilities / Grit) ────────────

        public void MoveTopToBottom()
        {
            if (_deck.Count < 2) return;
            var top = _deck[0];
            _deck.RemoveAt(0);
            _deck.Add(top);
        }

        public void ReorderTop(int n, List<BehaviorCardSO> newOrder)
        {
            int take = Mathf.Min(n, _deck.Count);
            _deck.RemoveRange(0, take);
            for (int i = Mathf.Min(take, newOrder.Count) - 1; i >= 0; i--)
                _deck.Insert(0, newOrder[i]);
        }

        // ── Active Mood Cards (read for UI and condition checks) ─────

        public IReadOnlyList<BehaviorCardSO> GetMoodCardsInPlay()
            => _moodInPlay.AsReadOnly();

        // ── Defeat Check ─────────────────────────────────────────────

        /// <summary>
        /// Monster is defeated when deck + discard are both empty.
        /// Mood cards in play do NOT block defeat — their removal re-enters them into discard,
        /// so defeat is checked AFTER each mood removal.
        /// </summary>
        public bool IsDefeated => _deck.Count == 0 && _discard.Count == 0;

        // ── Reshuffle ────────────────────────────────────────────────

        public void ReshuffleDiscardIntoDeck()
        {
            Shuffle(_discard);
            _deck.AddRange(_discard);
            _discard.Clear();
            Debug.Log($"[BehaviorDeck] Discard reshuffled into deck. Deck size: {_deck.Count}");
        }

        // ── Fisher-Yates Shuffle ─────────────────────────────────────

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
