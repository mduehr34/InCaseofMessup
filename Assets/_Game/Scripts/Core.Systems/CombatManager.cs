using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MnM.Core.Data;
using MnM.Core.Logic;

namespace MnM.Core.Systems
{
    public class CombatManager : MonoBehaviour, ICombatManager
    {
        // ── Injected Dependencies ────────────────────────────────
        [SerializeField] private GridManager _gridManager;
        // IMonsterAI injected at runtime — stub reference for now
        private IMonsterAI _monsterAI;

        // ── State ────────────────────────────────────────────────
        public CombatState CurrentState { get; private set; }
        public CombatPhase CurrentPhase { get; private set; }

        // ── Sub-Systems ──────────────────────────────────────────
        private AggroManager _aggroManager = new AggroManager();
        private bool _firstPartBreakOccurred = false;
        public AggroManager AggroManager => _aggroManager;

        // ── Events ───────────────────────────────────────────────
        public event System.Action<CombatPhase>             OnPhaseChanged;
        public event System.Action<string, int, DamageType> OnDamageDealt;
        public event System.Action<string>                  OnEntityCollapsed;
        public event System.Action<CombatResult>            OnCombatEnded;

        // ── Lifecycle ────────────────────────────────────────────
        public void StartCombat(CombatState initialState)
        {
            CurrentState = initialState;
            CurrentPhase = CombatPhase.VitalityPhase;
            _aggroManager.Initialize(initialState);
            Debug.Log($"[Combat] Started. Year:{initialState.campaignYear} " +
                      $"Monster:{initialState.monster.monsterName} " +
                      $"Hunters:{initialState.hunters.Length}");
            OnPhaseChanged?.Invoke(CurrentPhase);
        }

        // ── Phase Machine ────────────────────────────────────────
        public void AdvancePhase()
        {
            switch (CurrentPhase)
            {
                case CombatPhase.VitalityPhase:
                    RunVitalityPhase();
                    CurrentPhase = CombatPhase.HunterPhase;
                    break;

                case CombatPhase.HunterPhase:
                    if (AllHuntersActed())
                    {
                        CurrentPhase = CombatPhase.BehaviorRefresh;
                        Debug.Log("[Combat] All hunters acted — advancing to BehaviorRefresh");
                    }
                    else
                    {
                        Debug.Log("[Combat] HunterPhase — waiting for remaining hunters");
                    }
                    break;

                case CombatPhase.BehaviorRefresh:
                    RunBehaviorRefresh();
                    CurrentPhase = CombatPhase.MonsterPhase;
                    break;

                case CombatPhase.MonsterPhase:
                    RunMonsterPhase();
                    CurrentState.currentRound++;
                    CurrentPhase = CombatPhase.VitalityPhase;
                    Debug.Log($"[Combat] Round {CurrentState.currentRound} complete");
                    break;
            }

            CurrentState.currentPhase = CurrentPhase.ToString();
            OnPhaseChanged?.Invoke(CurrentPhase);
            Debug.Log($"[Combat] Phase → {CurrentPhase}");
        }

        // ── Phase Implementations ────────────────────────────────
        private void RunVitalityPhase()
        {
            foreach (var hunter in CurrentState.hunters)
            {
                if (hunter.isCollapsed) continue;
                hunter.hasActedThisPhase = false;
                hunter.apRemaining       = 2;
                DrawCardsForHunter(hunter);
                Debug.Log($"[Vitality] {hunter.hunterName} hand: [{string.Join(", ", hunter.handCardNames)}]");
            }
        }

        private void DrawCardsForHunter(HunterCombatState hunter)
        {
            // Hand size = 2 (bare fist default) — weapon proficiency may increase this in Stage 3
            const int handSize = 2;
            var deck    = new List<string>(hunter.deckCardNames);
            var discard = new List<string>(hunter.discardCardNames);
            var hand    = new List<string>();

            for (int i = 0; i < handSize; i++)
            {
                if (deck.Count == 0)
                {
                    // Reshuffle discard into deck
                    deck.AddRange(discard);
                    discard.Clear();
                    ShuffleList(deck);
                    Debug.Log($"[Vitality] {hunter.hunterName} reshuffled discard into deck");
                }
                if (deck.Count > 0)
                {
                    hand.Add(deck[0]);
                    deck.RemoveAt(0);
                }
            }

            hunter.handCardNames    = hand.ToArray();
            hunter.deckCardNames    = deck.ToArray();
            hunter.discardCardNames = discard.ToArray();
        }

        private void RunBehaviorRefresh()
        {
            _monsterAI?.AdvanceGroupIfExhausted();
            (_gridManager as IGridManager)?.TickDeniedCells();
            int remaining = _monsterAI?.RemainingRemovableCount ?? -1;
            Debug.Log($"[BehaviorRefresh] Removable cards remaining: {remaining}");
        }

        private void RunMonsterPhase()
        {
            if (_monsterAI == null)
            {
                Debug.LogWarning("[MonsterPhase] IMonsterAI not yet assigned — stub phase");
                return;
            }
            var card = _monsterAI.DrawNextCard();
            Debug.Log($"[MonsterPhase] Executing: {card.cardName} — {card.effectDescription}");
            _monsterAI.ExecuteCard(card, CurrentState);
        }

        // ── Hunter Actions ────────────────────────────────────────
        public bool TryPlayCard(string hunterId, string cardName, Vector2Int targetCell)
        {
            var hunter = GetHunter(hunterId);
            if (hunter == null)
            {
                Debug.LogWarning($"[Combat] TryPlayCard: hunter {hunterId} not found");
                return false;
            }

            if (!System.Array.Exists(hunter.handCardNames, n => n == cardName))
            {
                Debug.LogWarning($"[Combat] TryPlayCard: \"{cardName}\" not in {hunter.hunterName}'s hand");
                return false;
            }

            // Load card SO via Resources — Stage 5 will use a proper registry
            var card = Resources.Load<ActionCardSO>($"Data/Cards/Action/{cardName}");
            if (card == null)
            {
                Debug.LogWarning($"[Combat] TryPlayCard: ActionCardSO not found for \"{cardName}\"");
                return false;
            }

            int netCost = card.apCost - card.apRefund;
            if (hunter.apRemaining < netCost && card.category != CardCategory.Reaction)
            {
                Debug.LogWarning($"[Combat] TryPlayCard: insufficient AP. " +
                                 $"Have:{hunter.apRemaining} Need:{netCost}");
                return false;
            }

            int targetPartIndex = FindMonsterPartAtCell(targetCell);
            if (targetPartIndex < 0 && card.category != CardCategory.Reaction)
            {
                Debug.LogWarning($"[Combat] TryPlayCard: no monster part at ({targetCell.x},{targetCell.y})");
                return false;
            }

            if (targetPartIndex >= 0)
            {
                var targetPart = CurrentState.monster.parts[targetPartIndex];
                var result = CardResolver.Resolve(
                    card, hunter, CurrentState.monster,
                    ref targetPart, GetMonsterSO(),
                    _firstPartBreakOccurred);

                CurrentState.monster.parts[targetPartIndex] = targetPart;

                // Act on removed cards — CardResolver is acyclic, so callers drive removal
                foreach (var removedName in result.removedCardNames)
                    _monsterAI?.RemoveCard(removedName);

                if (result.apexShouldTrigger && !_firstPartBreakOccurred)
                {
                    _firstPartBreakOccurred = true;
                    _monsterAI?.TriggerApex();
                }

                if (result.damageDealt > 0)
                    OnDamageDealt?.Invoke(CurrentState.monster.monsterName,
                        result.damageDealt, result.damageType);
            }

            RemoveCardFromHand(hunter, cardName);
            return true;
        }

        private int FindMonsterPartAtCell(Vector2Int cell)
        {
            // Returns index 0 if cell is within the monster's footprint.
            // Stage 5 UI will map cells to specific named parts.
            var m = CurrentState.monster;
            bool inFootprint =
                cell.x >= m.gridX && cell.x < m.gridX + m.footprintW &&
                cell.y >= m.gridY && cell.y < m.gridY + m.footprintH;
            return inFootprint ? 0 : -1;
        }

        private void RemoveCardFromHand(HunterCombatState hunter, string cardName)
        {
            var hand    = new List<string>(hunter.handCardNames);
            var discard = new List<string>(hunter.discardCardNames);
            hand.Remove(cardName);
            discard.Add(cardName);
            hunter.handCardNames    = hand.ToArray();
            hunter.discardCardNames = discard.ToArray();
        }

        // Placeholder — Stage 5 will use a SO registry
        private MonsterSO GetMonsterSO() =>
            Resources.Load<MonsterSO>(
                $"Data/Monsters/{CurrentState.monster.monsterName.Replace(" ", "")}");

        public bool TryMoveHunter(string hunterId, Vector2Int destination)
        {
            Debug.LogWarning("[Combat] TryMoveHunter stub — implement in Stage 3-D");
            return false;
        }

        public void EndHunterTurn(string hunterId)
        {
            var hunter = GetHunter(hunterId);
            if (hunter == null) return;
            hunter.hasActedThisPhase = true;
            // Discard remaining hand
            var discard = new List<string>(hunter.discardCardNames);
            discard.AddRange(hunter.handCardNames);
            hunter.discardCardNames = discard.ToArray();
            hunter.handCardNames    = new string[0];
            Debug.Log($"[Combat] {hunter.hunterName} ended turn");
            AdvancePhase(); // Check if all hunters done
        }

        public void ExecuteBehaviorCard(string behaviorCardName)
        {
            Debug.Log($"[Combat] ExecuteBehaviorCard: {behaviorCardName} — stub, implement Stage 3");
        }

        // ── Win / Loss ────────────────────────────────────────────
        // Monster defeat fires via OnMonsterDefeated event (immediate, mid-turn).
        // This polling path exists for cases where callers need to check synchronously.
        public bool IsCombatOver(out CombatResult result)
        {
            result = default;

            // Monster defeated — event already fired via HandleMonsterDefeated,
            // but allow polling too (does NOT re-fire OnCombatEnded)
            if (_monsterAI != null && !_monsterAI.HasRemovableCards())
            {
                result.isVictory     = true;
                result.roundsElapsed = CurrentState.currentRound;
                return true;
            }

            // Hunt lost — all hunters collapsed
            if (CurrentState.hunters.All(h => h.isCollapsed))
            {
                result.isVictory     = false;
                result.roundsElapsed = CurrentState.currentRound;
                Debug.Log("[Combat] *** HUNT LOST — All hunters collapsed ***");
                OnCombatEnded?.Invoke(result);
                return true;
            }

            return false;
        }

        // ── Helpers ──────────────────────────────────────────────
        private bool AllHuntersActed() =>
            CurrentState.hunters.All(h => h.isCollapsed || h.hasActedThisPhase);

        private HunterCombatState GetHunter(string hunterId) =>
            System.Array.Find(CurrentState.hunters, h => h.hunterId == hunterId);

        public void SetMonsterAI(IMonsterAI ai) => _monsterAI = ai;

        public void InitializeMonsterAI(MonsterSO monster, string difficulty)
        {
            var ai = new MonsterAI();
            ai.InitializeDeck(monster, difficulty);

            // Subscribe before SetMonsterAI so the event is wired before any draws
            ai.OnMonsterDefeated += HandleMonsterDefeated;

            SetMonsterAI(ai);
            Debug.Log($"[Combat] MonsterAI initialized for {monster.monsterName} ({difficulty})");
        }

        private void HandleMonsterDefeated()
        {
            var result = new CombatResult
            {
                isVictory          = true,
                roundsElapsed      = CurrentState.currentRound,
                collapsedHunterIds = CurrentState.hunters
                    .Where(h => h.isCollapsed)
                    .Select(h => h.hunterId)
                    .ToArray(),
            };
            Debug.Log($"[Combat] *** HUNT WON *** Round:{result.roundsElapsed}");
            OnCombatEnded?.Invoke(result);
        }

        private static void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
