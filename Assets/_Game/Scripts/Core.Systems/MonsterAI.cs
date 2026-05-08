// Stage 8-M: MonsterAI stubbed to compile against the new data model.
// Full rewrite in Stage 8-N — pool-based deck construction, sub-phase execution, Grit windows.
using System.Collections.Generic;
using UnityEngine;
using MnM.Core.Data;

namespace MnM.Core.Systems
{
    public class MonsterAI : IMonsterAI
    {
        private IGridManager _grid;
        public void InjectGrid(IGridManager grid) => _grid = grid;

        public event System.Action OnMonsterDefeated;
        public event System.Action OnBehaviorCardRemoved;

        // Runtime deck state — initialized by InitializeDeck, managed by Stage 8-N
        private List<BehaviorCardSO> _behaviorDeck    = new();
        private List<BehaviorCardSO> _behaviorDiscard = new();

        public int RemainingRemovableCount =>
            _behaviorDeck.Count + _behaviorDiscard.Count;

        public bool HasRemovableCards() => RemainingRemovableCount > 0;

        public void InitializeDeck(MonsterSO monster, string difficulty)
        {
            // Stage 8-N: implement pool-based Fisher-Yates draw from
            // monster.baseCardPool / advancedCardPool / overwhelmingCardPool
            // using monster.deckCompositions[difficultyIndex].
            _behaviorDeck    = new List<BehaviorCardSO>();
            _behaviorDiscard = new List<BehaviorCardSO>();
            Debug.Log($"[MonsterAI] InitializeDeck stub — {monster.monsterName} ({difficulty}). " +
                      $"Full implementation in Stage 8-N.");
        }

        public BehaviorCardSO DrawNextCard()
        {
            if (_behaviorDeck.Count == 0)
            {
                _behaviorDeck.AddRange(_behaviorDiscard);
                _behaviorDiscard.Clear();
                ShuffleDeck(_behaviorDeck);
                Debug.Log("[MonsterAI] Deck empty — reshuffled discard");
            }

            if (_behaviorDeck.Count == 0)
            {
                Debug.LogWarning("[MonsterAI] DrawNextCard: no cards available");
                return null;
            }

            var card = _behaviorDeck[0];
            _behaviorDeck.RemoveAt(0);
            Debug.Log($"[MonsterAI] Drew: \"{card?.cardName}\" | Deck:{_behaviorDeck.Count} Discard:{_behaviorDiscard.Count}");
            return card;
        }

        public BehaviorCardResult ExecuteCard(BehaviorCardSO card, CombatState state)
        {
            // Stage 8-N: implement sub-phase flow (target identification → movement → damage)
            // with Grit windows between each phase. Use card.hasTargetIdentification,
            // card.hasMovement, card.hasDamage, card.targetRule, card.forcedHunterBodyPart.
            var result = new BehaviorCardResult();
            Debug.Log($"[MonsterAI] ExecuteCard stub — {card?.cardName}. Full implementation in Stage 8-N.");
            return result;
        }

        public bool RemoveCard(string cardName)
        {
            // Remove from deck first, then discard
            var inDeck = _behaviorDeck.Find(c =>
                c.cardName == cardName ||
                string.Equals(c.name, cardName, System.StringComparison.OrdinalIgnoreCase));
            if (inDeck != null)
            {
                _behaviorDeck.Remove(inDeck);
                Debug.Log($"[MonsterAI] Removed \"{cardName}\" from deck. " +
                          $"Health: deck={_behaviorDeck.Count} discard={_behaviorDiscard.Count}");
                OnBehaviorCardRemoved?.Invoke();
                if (!HasRemovableCards()) OnMonsterDefeated?.Invoke();
                return true;
            }

            var inDiscard = _behaviorDiscard.Find(c =>
                c.cardName == cardName ||
                string.Equals(c.name, cardName, System.StringComparison.OrdinalIgnoreCase));
            if (inDiscard != null)
            {
                _behaviorDiscard.Remove(inDiscard);
                Debug.Log($"[MonsterAI] Removed \"{cardName}\" from discard. " +
                          $"Health: deck={_behaviorDeck.Count} discard={_behaviorDiscard.Count}");
                OnBehaviorCardRemoved?.Invoke();
                if (!HasRemovableCards()) OnMonsterDefeated?.Invoke();
                return true;
            }

            Debug.LogWarning($"[MonsterAI] RemoveCard: \"{cardName}\" not found");
            return false;
        }

        public BehaviorCardSO[] GetActiveBehaviorCards()
        {
            var all = new List<BehaviorCardSO>(_behaviorDeck);
            all.AddRange(_behaviorDiscard);
            return all.ToArray();
        }

        private void ShuffleDeck(List<BehaviorCardSO> deck)
        {
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (deck[i], deck[j]) = (deck[j], deck[i]);
            }
        }

        // TriggerApex and AdvanceGroupIfExhausted removed — no escalation logic in 8-M model
    }
}
