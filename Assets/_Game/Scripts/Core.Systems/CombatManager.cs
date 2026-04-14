using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MnM.Core.Data;

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

        // ── Hunter Actions — Stubs (implemented fully in Stage 3) ─
        public bool TryPlayCard(string hunterId, string cardName, Vector2Int targetCell)
        {
            Debug.LogWarning("[Combat] TryPlayCard stub — implement in Stage 3");
            return false;
        }

        public bool TryMoveHunter(string hunterId, Vector2Int destination)
        {
            Debug.LogWarning("[Combat] TryMoveHunter stub — implement in Stage 3");
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

        // ── Win / Loss — Stub (implemented in Stage 3) ───────────
        public bool IsCombatOver(out CombatResult result)
        {
            result = default;
            // Stub — Stage 3 implements real win/loss detection
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
            SetMonsterAI(ai);
            Debug.Log($"[Combat] MonsterAI initialized for {monster.monsterName} ({difficulty})");
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
