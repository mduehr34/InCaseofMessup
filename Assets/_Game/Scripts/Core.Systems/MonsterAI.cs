using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MnM.Core.Data;

namespace MnM.Core.Systems
{
    public class MonsterAI : IMonsterAI
    {
        // ── Deck Lists ───────────────────────────────────────────
        // These shrink as cards are removed via part breaks/wounds
        private List<BehaviorCardSO> _openingCards    = new();
        private List<BehaviorCardSO> _escalationCards = new();
        private List<BehaviorCardSO> _apexCards       = new();
        private List<BehaviorCardSO> _permanentCards  = new();

        // The shuffled draw pile for the current round
        // Rebuilt each time it empties or group advances
        private List<BehaviorCardSO> _activeDeck = new();

        // ── Events ───────────────────────────────────────────────
        // Fires immediately mid-turn when the last Removable card is removed
        public event System.Action OnMonsterDefeated;

        // ── State ────────────────────────────────────────────────
        private bool _apexTriggered = false;

        public BehaviorGroup CurrentGroup { get; private set; } = BehaviorGroup.Opening;

        public int RemainingRemovableCount =>
            _openingCards.Count + _escalationCards.Count + _apexCards.Count;

        public bool HasRemovableCards() => RemainingRemovableCount > 0;

        // ── Initialization ───────────────────────────────────────
        public void InitializeDeck(MonsterSO monster, string difficulty)
        {
            // Difficulty selects which stat block to use — behavior deck is
            // defined on the MonsterSO and shared across difficulties
            // (Hardened/Apex variants add extra cards in Stage 7 content pass)
            _openingCards    = new List<BehaviorCardSO>(monster.openingCards    ?? new BehaviorCardSO[0]);
            _escalationCards = new List<BehaviorCardSO>(monster.escalationCards ?? new BehaviorCardSO[0]);
            _apexCards       = new List<BehaviorCardSO>(monster.apexCards       ?? new BehaviorCardSO[0]);
            _permanentCards  = new List<BehaviorCardSO>(monster.permanentCards  ?? new BehaviorCardSO[0]);

            _apexTriggered = false;
            CurrentGroup   = BehaviorGroup.Opening;

            RebuildActiveDeck();
            ShuffleDeck(_activeDeck);

            Debug.Log($"[MonsterAI] Deck initialized for {monster.monsterName} ({difficulty}). " +
                      $"Opening:{_openingCards.Count} Escalation:{_escalationCards.Count} " +
                      $"Apex:{_apexCards.Count} Permanent:{_permanentCards.Count} " +
                      $"Total Removable:{RemainingRemovableCount}");
        }

        // ── Deck Building ────────────────────────────────────────
        private void RebuildActiveDeck()
        {
            _activeDeck = new List<BehaviorCardSO>();

            // Opening group cards are always in the active deck
            _activeDeck.AddRange(_openingCards);

            // Escalation cards enter once Opening group is exhausted
            if (CurrentGroup == BehaviorGroup.Escalation ||
                CurrentGroup == BehaviorGroup.Apex)
                _activeDeck.AddRange(_escalationCards);

            // Apex cards only after TriggerApex() called
            if (_apexTriggered)
                _activeDeck.AddRange(_apexCards);

            // Permanent cards are NOT added here — they are checked separately
            // in DrawNextCard() and never enter the shuffled active deck

            Debug.Log($"[MonsterAI] Active deck rebuilt. Size: {_activeDeck.Count} " +
                      $"(Group:{CurrentGroup} Apex:{_apexTriggered})");
        }

        private void ShuffleDeck(List<BehaviorCardSO> deck)
        {
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (deck[i], deck[j]) = (deck[j], deck[i]);
            }
        }

        // ── Drawing ──────────────────────────────────────────────
        public BehaviorCardSO DrawNextCard()
        {
            // Check Permanent cards first — they fire if their trigger is active
            // EvaluateTrigger is a stub in this session (always false)
            foreach (var perm in _permanentCards)
            {
                if (EvaluateTrigger(perm.triggerCondition))
                {
                    Debug.Log($"[MonsterAI] Permanent card triggered: {perm.cardName}");
                    return perm;
                }
            }

            // If active deck empty: rebuild and reshuffle
            if (_activeDeck.Count == 0)
            {
                Debug.Log("[MonsterAI] Active deck exhausted — rebuilding and reshuffling");
                RebuildActiveDeck();
                ShuffleDeck(_activeDeck);
            }

            // Draw from top
            var card = _activeDeck[0];
            _activeDeck.RemoveAt(0);

            Debug.Log($"[MonsterAI] Drew: \"{card.cardName}\" " +
                      $"({card.group}/{card.cardType}) " +
                      $"Active deck remaining: {_activeDeck.Count}");
            return card;
        }

        // ── Group Progression ────────────────────────────────────
        // Called during Behavior Refresh phase by CombatManager
        public void AdvanceGroupIfExhausted()
        {
            // Only advance Opening → Escalation
            // Apex is triggered separately via TriggerApex()
            if (CurrentGroup == BehaviorGroup.Opening && _openingCards.Count == 0)
            {
                CurrentGroup = BehaviorGroup.Escalation;
                RebuildActiveDeck();
                ShuffleDeck(_activeDeck);
                Debug.Log("[MonsterAI] *** Group advanced: Opening → Escalation ***");
            }
        }

        // Called by CombatManager on first part break (or other Apex trigger condition)
        public void TriggerApex()
        {
            if (_apexTriggered) return;
            _apexTriggered = true;
            RebuildActiveDeck();
            ShuffleDeck(_activeDeck);
            Debug.Log("[MonsterAI] *** APEX TRIGGERED — Apex cards entered rotation ***");
        }

        // ── Card Removal ─────────────────────────────────────────
        public void RemoveCard(string cardName)
        {
            bool removed = false;

            removed |= RemoveFromList(_openingCards,    cardName);
            removed |= RemoveFromList(_escalationCards, cardName);
            removed |= RemoveFromList(_apexCards,       cardName);

            // Also evict from the active draw pile if it's sitting there
            var inActive = _activeDeck.FirstOrDefault(
                c => c.cardName == cardName && c.cardType != BehaviorCardType.Permanent);
            if (inActive != null) _activeDeck.Remove(inActive);

            if (removed)
            {
                Debug.Log($"[MonsterAI] Card removed: \"{cardName}\". " +
                          $"Remaining Removable: {RemainingRemovableCount}");

                // Win condition — fires IMMEDIATELY, mid-turn, mid-phase
                if (!HasRemovableCards())
                {
                    Debug.Log("[MonsterAI] *** LAST REMOVABLE CARD REMOVED — MONSTER DEFEATED ***");
                    OnMonsterDefeated?.Invoke();
                }
            }
            else
            {
                Debug.LogWarning($"[MonsterAI] RemoveCard: \"{cardName}\" not found in any removable list");
            }
        }

        private bool RemoveFromList(List<BehaviorCardSO> list, string cardName)
        {
            var card = list.FirstOrDefault(c => c.cardName == cardName);
            if (card == null) return false;
            list.Remove(card);
            return true;
        }

        /// <summary>Returns all remaining behavior cards across every group — used by the UI to render the panel.</summary>
        public BehaviorCardSO[] GetActiveBehaviorCards()
        {
            var all = new List<BehaviorCardSO>();
            all.AddRange(_openingCards);
            all.AddRange(_escalationCards);
            all.AddRange(_apexCards);
            all.AddRange(_permanentCards);
            return all.ToArray();
        }

        public void ExecuteCard(BehaviorCardSO card, CombatState state)
        {
            // STUB — implemented Session 3-C (trigger evaluation)
            Debug.Log($"[MonsterAI] ExecuteCard stub: {card.cardName} — implement in 3-C");
        }

        // ── Trigger Evaluation — Stub ────────────────────────────
        // Implemented in Session 3-C after EvaluateTrigger logic is designed
        private bool EvaluateTrigger(string triggerCondition)
        {
            // STUB — always returns false until 3-C
            return false;
        }
    }
}
