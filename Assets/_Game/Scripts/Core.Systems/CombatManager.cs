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
        private AggroManager  _aggroManager  = new AggroManager();
        private ComboTracker  _comboTracker  = new ComboTracker();
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

                // Trap zone check — if triggered, skip normal damage resolution
                bool trapFired = HandleTrapZone(targetPart.partName, hunterId);

                if (!trapFired)
                {
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
            }

            // Collapse check — runs after every card resolution in case damage was dealt
            foreach (var h in CurrentState.hunters)
                CheckHunterCollapse(h);

            // Combo tracking
            _comboTracker.NotifyCardPlayed(hunterId, card.category);

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
            var hunter = GetHunter(hunterId);
            if (hunter == null)
            {
                Debug.LogWarning($"[Combat] TryMoveHunter: {hunterId} not found");
                return false;
            }
            if (hunter.isCollapsed)
            {
                Debug.LogWarning($"[Combat] TryMoveHunter: {hunter.hunterName} is collapsed");
                return false;
            }
            if (!(_gridManager as IGridManager).IsInBounds(destination))
            {
                Debug.LogWarning($"[Combat] TryMoveHunter: destination out of bounds");
                return false;
            }
            if ((_gridManager as IGridManager).IsOccupied(destination))
            {
                Debug.LogWarning($"[Combat] TryMoveHunter: destination occupied");
                return false;
            }
            if ((_gridManager as IGridManager).IsDenied(destination))
            {
                Debug.LogWarning($"[Combat] TryMoveHunter: destination denied by Spear zone");
                return false;
            }

            // Movement cost check (Slowed = half movement)
            int effectiveMovement = hunter.movement; // Base movement from CharacterSO (wired in Stage 4)
            int accuracy = hunter.accuracy;
            StatusEffectResolver.ApplyStatusPenalties(hunter, ref accuracy, ref effectiveMovement);

            var from = new Vector2Int(hunter.gridX, hunter.gridY);
            int dist = (_gridManager as IGridManager).GetDistance(from, destination);
            if (dist > effectiveMovement)
            {
                Debug.LogWarning($"[Combat] TryMoveHunter: distance {dist} exceeds movement {effectiveMovement}");
                return false;
            }

            // Execute move
            (_gridManager as IGridManager).MoveOccupant(hunterId, destination);
            hunter.gridX = destination.x;
            hunter.gridY = destination.y;

            // Check which arc the hunter is in relative to the monster — may trigger Flank Sense
            // EvaluateTrigger handles this in MonsterAI (wired when full content added Stage 7)
            var monsterCell   = new Vector2Int(CurrentState.monster.gridX, CurrentState.monster.gridY);
            var monsterFacing = new Vector2Int(CurrentState.monster.facingX, CurrentState.monster.facingY);
            var arc = (_gridManager as IGridManager).GetArcFromAttackerToTarget(
                destination, monsterCell, monsterFacing);

            Debug.Log($"[Combat] {hunter.hunterName} moved to ({destination.x},{destination.y}) — Arc: {arc}");
            return true;
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
            _comboTracker.OnHunterTurnEnd(hunterId);
            Debug.Log($"[Combat] {hunter.hunterName} ended turn");
            AdvancePhase(); // Check if all hunters done
        }

        public void ExecuteBehaviorCard(string behaviorCardName)
        {
            Debug.Log($"[Combat] ExecuteBehaviorCard: {behaviorCardName} — stub, implement Stage 3");
        }

        // ── Injury / Scar Application ─────────────────────────────
        // Stub — scar card draw happens post-combat during settlement.
        // TODO: 7R — EyePendant scar intercept
        // When applying an injury/scar card to a hunter:
        //   1. Check HunterCombatState.equippedItemNames contains "Gaunt Eye Pendant"
        //   2. Check spentHuntAbilities does NOT contain "Gaunt Eye Pendant"
        //   3. If both: present discard option in UI; on confirm, skip card application
        //      and add "Gaunt Eye Pendant" to spentHuntAbilities.
        // TODO: 7R — handle GAUNT_3PC_LOUD_SUPPRESS
        // When a Loud behavior card resolves, check activeGearEffectTags for
        // "GAUNT_3PC_LOUD_SUPPRESS" and reduce that card's movement effect by 2 squares.

        // ── Collapse / Hunt Loss ──────────────────────────────────
        private void CheckHunterCollapse(HunterCombatState hunter)
        {
            if (hunter.isCollapsed) return;

            var head  = System.Array.Find(hunter.bodyZones, z => z.zone == "Head");
            var torso = System.Array.Find(hunter.bodyZones, z => z.zone == "Torso");

            bool headDead  = head.fleshCurrent  <= 0;
            bool torsoDead = torso.fleshCurrent <= 0;

            if (headDead || torsoDead)
            {
                // TODO: 7R — GAUNT_5PC_DEATH_CHEAT collapse intercept
                // Check activeGearEffectTags contains "GAUNT_5PC_DEATH_CHEAT" AND
                // spentHuntAbilities does NOT contain "GAUNT_5PC_DEATH_CHEAT".
                // If both: set struck zone fleshCurrent = 1, add tag to spentHuntAbilities, return early.
                hunter.isCollapsed = true;
                string cause = headDead ? "Head Flesh = 0" : "Torso Flesh = 0";
                Debug.Log($"[Combat] *** {hunter.hunterName} COLLAPSED ({cause}) ***");
                OnEntityCollapsed?.Invoke(hunter.hunterId);

                // Remove from grid — collapsed hunters don't block movement
                (_gridManager as IGridManager)?.RemoveOccupant(hunter.hunterId);

                CheckHuntLoss();
            }
        }

        private void CheckHuntLoss()
        {
            if (System.Array.TrueForAll(CurrentState.hunters, h => h.isCollapsed))
            {
                var result = new CombatResult
                {
                    isVictory          = false,
                    roundsElapsed      = CurrentState.currentRound,
                    collapsedHunterIds = System.Array.ConvertAll(
                        CurrentState.hunters, h => h.hunterId),
                };
                Debug.Log("[Combat] *** HUNT LOST — All hunters collapsed ***");
                OnCombatEnded?.Invoke(result);
            }
        }

        // ── Trap Zone Handling ────────────────────────────────────
        // Returns true if a trap was triggered (caller should skip normal attack resolution)
        private bool HandleTrapZone(string partName, string hunterId)
        {
            var part = System.Array.Find(
                CurrentState.monster.parts, p => p.partName == partName);

            if (part.partName == null) return false;

            if (!part.isRevealed)
            {
                var monsterSO = GetMonsterSO();
                bool isTrap = monsterSO != null &&
                    System.Array.IndexOf(monsterSO.trapZoneParts, partName) >= 0;

                if (isTrap)
                {
                    int idx = System.Array.FindIndex(
                        CurrentState.monster.parts, p => p.partName == partName);
                    if (idx >= 0)
                    {
                        var mutablePart = CurrentState.monster.parts[idx];
                        mutablePart.isRevealed = true;
                        CurrentState.monster.parts[idx] = mutablePart;
                    }

                    Debug.Log($"[Combat] *** TRAP TRIGGERED — {partName} was a Trap Zone! ***");
                    Debug.Log($"[Combat] Counter-attack fires. No damage applied this hit.");

                    // Full trigger evaluation wired in Stage 7 with real behavior cards
                    return true; // trap triggered — skip normal attack
                }
            }
            return false; // not a trap — proceed normally
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
            // Event already fired via CheckHuntLoss() — polling only, does NOT re-fire OnCombatEnded
            if (CurrentState.hunters.All(h => h.isCollapsed))
            {
                result.isVictory     = false;
                result.roundsElapsed = CurrentState.currentRound;
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
