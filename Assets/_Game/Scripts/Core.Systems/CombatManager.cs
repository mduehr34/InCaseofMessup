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

        // Visual events — not on ICombatManager; consumed by UI components directly
        public event System.Action<string, string, int>         OnEffectApplied;        // entityId, effectName, duration
        public event System.Action<string, string>              OnEffectRemoved;         // entityId, effectName
        public event System.Action<string>                      OnBehaviorCardActivated; // cardName
        public event System.Action                              OnBehaviorCardRemoved;   // deck changed — UI should rebuild
        public event System.Action<string, WoundOutcome, string> OnWoundResolved;        // hunterId, outcome, locationName

        // ── Lifecycle ────────────────────────────────────────────
        public void StartCombat(CombatState initialState)
        {
            CurrentState = initialState;
            _aggroManager.Initialize(initialState);

            // Register initial grid positions so IsOccupied() works from turn 1.
            // Unplaced hunters are not yet on the grid — skip them.
            var grid = _gridManager as IGridManager;
            if (grid != null)
            {
                foreach (var hunter in initialState.hunters)
                {
                    if (hunter.isCollapsed || hunter.isUnplaced) continue;
                    grid.PlaceOccupant(new GridOccupant
                    {
                        occupantId = hunter.hunterId,
                        isHunter   = true,
                        footprintW = 1,
                        footprintH = 1,
                        gridX      = hunter.gridX,
                        gridY      = hunter.gridY,
                    }, new Vector2Int(hunter.gridX, hunter.gridY));
                }

                var m = initialState.monster;
                grid.PlaceOccupant(new GridOccupant
                {
                    occupantId = m.monsterName,
                    isHunter   = false,
                    footprintW = m.footprintW,
                    footprintH = m.footprintH,
                    gridX      = m.gridX,
                    gridY      = m.gridY,
                }, new Vector2Int(m.gridX, m.gridY));
            }
            else
            {
                Debug.LogWarning("[Combat] StartCombat: _gridManager not assigned — occupancy will not block movement");
            }

            bool anyUnplaced = System.Array.Exists(initialState.hunters, h => h.isUnplaced);
            CurrentPhase = anyUnplaced ? CombatPhase.DeploymentPhase : CombatPhase.VitalityPhase;
            CurrentState.currentPhase = CurrentPhase.ToString();

            Debug.Log($"[Combat] Started. Year:{initialState.campaignYear} " +
                      $"Monster:{initialState.monster.monsterName} " +
                      $"Hunters:{initialState.hunters.Length} " +
                      $"Phase:{CurrentPhase}");
            OnPhaseChanged?.Invoke(CurrentPhase);
        }

        // ── Phase Machine ────────────────────────────────────────
        public void AdvancePhase()
        {
            switch (CurrentPhase)
            {
                case CombatPhase.DeploymentPhase:
                    Debug.LogWarning("[Combat] AdvancePhase called during DeploymentPhase — use TryPlaceHunter");
                    break;

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
                hunter.hasActedThisPhase  = false;
                hunter.hasMovedThisPhase  = false;
                hunter.apRemaining        = 2;
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
            // AdvanceGroupIfExhausted removed — no escalation logic in Stage 8-M model
            (_gridManager as IGridManager)?.TickDeniedCells();
            int remaining = _monsterAI?.RemainingRemovableCount ?? -1;
            Debug.Log($"[BehaviorRefresh] Removable cards remaining: {remaining}");
        }

        private void RunMonsterPhase()
        {
            if (_monsterAI == null) { Debug.LogWarning("[MonsterPhase] IMonsterAI not assigned"); return; }

            if (CurrentState.monster.currentStanceTag == "STUNNED")
            {
                CurrentState.monster.currentStanceTag = "";
                Debug.Log("[MonsterPhase] Monster STUNNED — skipping, clearing stun");
                return;
            }

            var card = _monsterAI.DrawNextCard();
            if (card == null) return;

            Debug.Log($"[MonsterPhase] Executing: {card.cardName}");
            OnBehaviorCardActivated?.Invoke(card.cardName);

            var result = _monsterAI.ExecuteCard(card, CurrentState);

            // Process movement
            if (result.monsterMoved && _gridManager != null)
            {
                (_gridManager as IGridManager)?.MoveOccupant(
                    CurrentState.monster.monsterName, result.newMonsterCell);
                Debug.Log($"[MonsterPhase] GridManager updated — monster at " +
                          $"({result.newMonsterCell.x},{result.newMonsterCell.y})");
            }

            // Process hits
            foreach (var hit in result.hits)
            {
                OnDamageDealt?.Invoke(hit.hunterId, hit.damage, DamageType.Flesh);
                var hunter = GetHunter(hit.hunterId);
                if (hunter != null) CheckHunterCollapse(hunter);
            }

            // Sync health counts to CombatState for UI
            if (_monsterAI is MonsterAI ai)
            {
                CurrentState.monster.behaviorDeckCount    = ai._behaviorDeckPublic.DeckCount;
                CurrentState.monster.behaviorDiscardCount = ai._behaviorDeckPublic.DiscardCount;
                CurrentState.monster.moodCardsInPlayCount = ai._behaviorDeckPublic.MoodInPlayCount;
                CurrentState.monster.woundDeckCount       = ai._woundDeckPublic.DeckCount;
                CurrentState.monster.woundDiscardCount    = ai._woundDeckPublic.DiscardCount;
            }
        }

        // ── Card Registry (mirrors UI pattern — cards not in a Resources sub-path) ────
        private static ActionCardRegistrySO _cardRegistry;
        private static ActionCardSO LoadCardSO(string name)
        {
            if (_cardRegistry == null)
                _cardRegistry = Resources.Load<ActionCardRegistrySO>("ActionCardRegistry");
            if (_cardRegistry == null)
            {
                Debug.LogError("[Combat] ActionCardRegistry not found — " +
                               "create it at Assets/_Game/Data/Resources/ActionCardRegistry.asset");
                return null;
            }
            return _cardRegistry.Get(name);
        }

        // ── Deployment ────────────────────────────────────────────
        public bool TryPlaceHunter(string hunterId, Vector2Int cell, SpawnZoneSO[] zones)
        {
            var grid = _gridManager as IGridManager;
            if (CurrentPhase != CombatPhase.DeploymentPhase)
            {
                Debug.LogWarning("[Combat] TryPlaceHunter called outside DeploymentPhase");
                return false;
            }

            var hunter = GetHunter(hunterId);
            if (hunter == null || !hunter.isUnplaced)
            {
                Debug.LogWarning($"[Combat] TryPlaceHunter: {hunterId} not found or already placed");
                return false;
            }

            if (zones != null && zones.Length > 0 && !CellInAnyZone(cell, zones))
            {
                Debug.LogWarning($"[Combat] TryPlaceHunter: ({cell.x},{cell.y}) is outside all spawn zones");
                return false;
            }

            if (grid != null && grid.IsOccupied(cell))
            {
                Debug.LogWarning($"[Combat] TryPlaceHunter: ({cell.x},{cell.y}) is occupied");
                return false;
            }

            hunter.gridX      = cell.x;
            hunter.gridY      = cell.y;
            hunter.isUnplaced = false;

            grid?.PlaceOccupant(new GridOccupant
            {
                occupantId = hunter.hunterId,
                isHunter   = true,
                footprintW = 1,
                footprintH = 1,
                gridX      = cell.x,
                gridY      = cell.y,
            }, cell);

            Debug.Log($"[Combat] {hunter.hunterName} placed at ({cell.x},{cell.y})");

            if (!System.Array.Exists(CurrentState.hunters, h => h.isUnplaced))
            {
                Debug.Log("[Combat] All hunters placed — advancing to VitalityPhase");
                CurrentPhase = CombatPhase.VitalityPhase;
                CurrentState.currentPhase = CurrentPhase.ToString();
                OnPhaseChanged?.Invoke(CurrentPhase);
            }
            else
            {
                OnPhaseChanged?.Invoke(CurrentPhase); // Refresh UI to show next unplaced hunter
            }

            return true;
        }

        private static bool CellInAnyZone(Vector2Int cell, SpawnZoneSO[] zones)
        {
            foreach (var z in zones)
                if (z != null && z.ContainsCell(cell)) return true;
            return false;
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

            var card = LoadCardSO(cardName);
            if (card == null)
            {
                Debug.LogWarning($"[Combat] TryPlayCard: ActionCardSO not found for \"{cardName}\" — check ActionCardRegistry");
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

                    // Act on removed cards — CardResolver is acyclic, so callers drive removal.
                    // Track only cards that MonsterAI confirms were still present and removed.
                    // Card removal only fires when the MonsterSO part data explicitly configures it
                    // via breakRemovesCardNames / woundRemovesCardNames. No fallback removal.
                    var confirmedRemovals = new List<string>();
                    foreach (var removedName in result.removedCardNames)
                    {
                        if (_monsterAI != null && _monsterAI.RemoveCard(removedName))
                            confirmedRemovals.Add(removedName);
                    }

                    // ── Wound Resolution (Stage 8-N) ──────────────
                    if (!result.wasMiss && !result.reactionApplied)
                    {
                        CurrentState.lastAttackerId = hunterId;
                        ResolveWound(hunterId);
                    }

                    // ── Combat Log ────────────────────────────────
                    string outcome = result.wasMiss         ? "MISS"
                                   : result.reactionApplied ? "REACTION"
                                   : result.isCritical      ? "CRITICAL HIT"
                                   : result.damageDealt > 0 ? "HIT"
                                   :                          "HIT — Force check failed";
                    string removed = confirmedRemovals.Count > 0
                        ? string.Join(", ", confirmedRemovals)
                        : "none";
                    Debug.Log(
                        $"[Combat Log] ──────────────────────────────────────────\n" +
                        $"  {hunter.hunterName} played {card.cardName}\n" +
                        $"  Target  : {targetPart.partName} ({CurrentState.monster.monsterName})\n" +
                        $"  Outcome : {outcome}\n" +
                        $"  Part HP : Shell {targetPart.shellCurrent}/{targetPart.shellMax} " +
                                    $"| Flesh {targetPart.fleshCurrent}/{targetPart.fleshMax}\n" +
                        $"  AP left : {hunter.apRemaining}\n" +
                        $"  Removed : {removed} (remaining: {_monsterAI?.RemainingRemovableCount ?? -1})\n" +
                        $"────────────────────────────────────────────────────────");

                    // apexShouldTrigger / TriggerApex removed — no escalation logic in Stage 8-M model

                    // Monster-side damage event removed — wound resolution via WoundDeck in Stage 8-N
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
            var m = CurrentState.monster;
            bool inFootprint =
                cell.x >= m.gridX && cell.x < m.gridX + m.footprintW &&
                cell.y >= m.gridY && cell.y < m.gridY + m.footprintH;
            if (!inFootprint) return -1;

            // Prefer unbroken revealed parts; fall back to broken parts (still have flesh)
            var eligible = new List<int>();
            for (int i = 0; i < m.parts.Length; i++)
            {
                var p = m.parts[i];
                if (p.isRevealed) eligible.Add(i);
            }

            if (eligible.Count == 0)
            {
                Debug.LogWarning("[Combat] No revealed monster parts — no eligible target");
                return -1;
            }

            // Prefer unbroken parts so shell damage is still possible; if all broken, any revealed part
            var preferred = eligible.Where(i => !m.parts[i].isBroken).ToList();
            var pool      = preferred.Count > 0 ? preferred : eligible;

            int chosen = pool[Random.Range(0, pool.Count)];
            Debug.Log($"[Combat] Part targeted: {m.parts[chosen].partName} " +
                      $"broken={m.parts[chosen].isBroken} " +
                      $"(rolled from {pool.Count} eligible parts)");
            return chosen;
        }

        private void RemoveCardFromHand(HunterCombatState hunter, string cardName)
        {
            var hand    = new List<string>(hunter.handCardNames);
            var discard = new List<string>(hunter.discardCardNames);
            hand.Remove(cardName);
            discard.Add(cardName);
            hunter.handCardNames    = hand.ToArray();
            hunter.discardCardNames = discard.ToArray();
            Debug.Log($"[Combat] {hunter.hunterName} discarded: {cardName} " +
                      $"(discard pile: {hunter.discardCardNames.Length})");
        }

        private MonsterSO _cachedMonsterSO;
        private MonsterSO GetMonsterSO() => _cachedMonsterSO;

        public bool TryMoveHunter(string hunterId, Vector2Int destination)
        {
            var grid = _gridManager as IGridManager;
            if (grid == null)
            {
                Debug.LogError("[Combat] TryMoveHunter: _gridManager not assigned on CombatManager");
                return false;
            }

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
            if (!grid.IsInBounds(destination))
            {
                Debug.LogWarning($"[Combat] TryMoveHunter: destination out of bounds");
                return false;
            }
            if (grid.IsOccupied(destination))
            {
                Debug.LogWarning($"[Combat] TryMoveHunter: destination occupied");
                return false;
            }
            if (grid.IsDenied(destination))
            {
                Debug.LogWarning($"[Combat] TryMoveHunter: destination denied by Spear zone");
                return false;
            }

            // Movement cost check (Slowed = half movement)
            int effectiveMovement = hunter.movement;
            int accuracy = hunter.accuracy;
            StatusEffectResolver.ApplyStatusPenalties(hunter, ref accuracy, ref effectiveMovement);

            var from = new Vector2Int(hunter.gridX, hunter.gridY);
            int dist = grid.GetDistance(from, destination);
            if (dist > effectiveMovement)
            {
                Debug.LogWarning($"[Combat] TryMoveHunter: distance {dist} exceeds movement {effectiveMovement}");
                return false;
            }

            // Execute move
            grid.MoveOccupant(hunterId, destination);
            hunter.gridX             = destination.x;
            hunter.gridY             = destination.y;
            hunter.hasMovedThisPhase = true;

            // Update facing toward direction of movement
            int dx = destination.x - from.x;
            int dy = destination.y - from.y;
            if (dx != 0 || dy != 0)
            {
                hunter.facingX = dx == 0 ? 0 : (dx > 0 ? 1 : -1);
                hunter.facingY = dy == 0 ? 0 : (dy > 0 ? 1 : -1);
            }

            // Check which arc the hunter is in relative to the monster — may trigger Flank Sense
            // EvaluateTrigger handles this in MonsterAI (wired when full content added Stage 7)
            var monsterCell   = new Vector2Int(CurrentState.monster.gridX, CurrentState.monster.gridY);
            var monsterFacing = new Vector2Int(CurrentState.monster.facingX, CurrentState.monster.facingY);
            var arc = grid.GetArcFromAttackerToTarget(destination, monsterCell, monsterFacing);

            Debug.Log($"[Combat] {hunter.hunterName} moved to ({destination.x},{destination.y}) " +
                      $"facing ({hunter.facingX},{hunter.facingY}) — Arc: {arc}");
            return true;
        }

        public void SkipHunterMove(string hunterId)
        {
            var hunter = GetHunter(hunterId);
            if (hunter == null) return;
            hunter.hasMovedThisPhase = true;
            Debug.Log($"[Combat] {hunter.hunterName} skipped move — staying at ({hunter.gridX},{hunter.gridY})");
        }

        public void EndHunterTurn(string hunterId)
        {
            var hunter = GetHunter(hunterId);
            if (hunter == null) return;
            hunter.hasActedThisPhase = true;
            // Discard remaining hand
            var discard = new List<string>(hunter.discardCardNames);
            foreach (var cardName in hunter.handCardNames)
            {
                discard.Add(cardName);
                Debug.Log($"[Combat] {hunter.hunterName} end-of-turn discard: {cardName}");
            }
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
            // Stage 8-M: trapZoneParts removed from MonsterSO.
            // Trap zones are now WoundLocationSO entries with isTrap == true,
            // drawn from the wound deck on a successful hit — see Stage 8-N.
            return false;
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

        // ── Status Effect API ─────────────────────────────────────
        // Call these instead of StatusEffectResolver directly so the display stays in sync.
        public void ApplyStatusEffect(string entityId, StatusEffect effect, int visualDuration = 2)
        {
            var hunter = GetHunter(entityId);
            if (hunter == null)
            {
                Debug.LogWarning($"[Combat] ApplyStatusEffect: entity {entityId} not found");
                return;
            }
            StatusEffectResolver.Apply(ref hunter.activeStatusEffects, effect);
            OnEffectApplied?.Invoke(entityId, effect.ToString(), visualDuration);
        }

        public void RemoveStatusEffect(string entityId, StatusEffect effect)
        {
            var hunter = GetHunter(entityId);
            if (hunter == null)
            {
                Debug.LogWarning($"[Combat] RemoveStatusEffect: entity {entityId} not found");
                return;
            }
            StatusEffectResolver.Remove(ref hunter.activeStatusEffects, effect);
            OnEffectRemoved?.Invoke(entityId, effect.ToString());
        }

        /// <summary>Returns all remaining BehaviorCardSOs — UI only, not on the interface.</summary>
        public BehaviorCardSO[] GetActiveBehaviorCards()
        {
            return _monsterAI?.GetActiveBehaviorCards() ?? new BehaviorCardSO[0];
        }

        /// <summary>Direct read from MonsterAI — always matches the console log count.</summary>
        public int MonsterRemainingRemovableCount => _monsterAI?.RemainingRemovableCount ?? 0;

        public void SetMonsterAI(IMonsterAI ai) => _monsterAI = ai;

        public void InitializeMonsterAI(MonsterSO monster, string difficulty)
        {
            _cachedMonsterSO = monster;

            var ai = new MonsterAI();
            ai.InitializeDeck(monster, difficulty);

            // Subscribe before SetMonsterAI so the event is wired before any draws
            ai.OnMonsterDefeated     += HandleMonsterDefeated;
            ai.OnBehaviorCardRemoved += () => OnBehaviorCardRemoved?.Invoke();
            // OnGritWindow: UI phase (Stage 9+) subscribes here to pause for player input
            ai.OnGritWindow          += (phase, card) =>
                Debug.Log($"[GritWindow] Phase: {phase} | Card: {card?.cardName}");

            ai.InjectGrid(_gridManager as IGridManager);
            SetMonsterAI(ai);
            Debug.Log($"[Combat] MonsterAI initialized for {monster.monsterName} ({difficulty})");
        }

        // ── Wound Resolution ──────────────────────────────────────────
        /// <summary>
        /// Called when a hunter successfully hits the monster (to-hit roll passed).
        /// Draws a wound location, runs the force roll, and removes a behavior card on wound/critical.
        /// </summary>
        public WoundOutcome ResolveWound(string hunterId)
        {
            var hunter = GetHunter(hunterId);
            if (hunter == null || _monsterAI == null)
            {
                Debug.LogWarning("[Combat] ResolveWound: hunter or AI null");
                return WoundOutcome.Failure;
            }

            var ai = _monsterAI as MonsterAI;
            if (ai == null)
            {
                Debug.LogWarning("[Combat] ResolveWound: MonsterAI cast failed");
                return WoundOutcome.Failure;
            }

            // ── Draw wound location ───────────────────────────────────
            var location = ai._woundDeckPublic.Draw();
            if (location == null)
            {
                Debug.LogWarning("[Combat] ResolveWound: wound deck empty");
                return WoundOutcome.Failure;
            }

            Debug.Log($"[Wound] Drew: {location.locationName} (target: {location.woundTarget}, " +
                      $"trap: {location.isTrap}, impervious: {location.isImpervious})");

            // ── Trap ──────────────────────────────────────────────────
            if (location.isTrap)
            {
                Debug.Log($"[Wound] TRAP: {location.trapEffect}");
                OnWoundResolved?.Invoke(hunterId, WoundOutcome.Trap, location.locationName);
                ai._woundDeckPublic.SendToDiscard(location);
                ai._woundDeckPublic.ReshuffleDiscardIntoDeck();
                return WoundOutcome.Trap;
            }

            // ── Force Roll ────────────────────────────────────────────
            int roll     = Random.Range(1, 11);  // d10
            int strength = GetHunterStat(hunterId, "strength");
            bool woundPassed = (roll + strength) > location.woundTarget;

            Debug.Log($"[Wound] Force roll: d10={roll} + STR={strength} = {roll + strength} vs target {location.woundTarget} " +
                      $"→ {(woundPassed ? "WOUND CHECK PASSES" : "FAILURE")}");

            if (!woundPassed)
            {
                if (!string.IsNullOrEmpty(location.failureEffect))
                    Debug.Log($"[Wound] Failure effect: {location.failureEffect}");
                OnWoundResolved?.Invoke(hunterId, WoundOutcome.Failure, location.locationName);
                ai._woundDeckPublic.SendToDiscard(location);
                return WoundOutcome.Failure;
            }

            // ── Critical Sub-Check (only when wound check passed) ─────
            int luck          = GetHunterStat(hunterId, "luck");
            int critThreshold = 10 - luck;  // Luck 2 → crit on d10 ≥ 8
            bool isCritical   = roll >= critThreshold;

            Debug.Log($"[Wound] Critical check: d10 natural={roll} vs threshold {critThreshold} " +
                      $"→ {(isCritical ? "CRITICAL" : "standard wound")}");

            WoundOutcome outcome = isCritical ? WoundOutcome.Critical : WoundOutcome.Wound;

            if (!string.IsNullOrEmpty(location.woundEffect))
                Debug.Log($"[Wound] Wound effect: {location.woundEffect}");
            if (isCritical && !string.IsNullOrEmpty(location.criticalEffect))
                Debug.Log($"[Wound] Critical effect: {location.criticalEffect}");

            // Set critical wound tag
            if (isCritical && !string.IsNullOrEmpty(location.criticalWoundTag))
            {
                ai.AddCriticalWoundTag(location.criticalWoundTag);
                var monState = CurrentState.monster;
                var tags     = new List<string>(monState.criticalWoundTags ?? new string[0]);
                if (!tags.Contains(location.criticalWoundTag)) tags.Add(location.criticalWoundTag);
                monState.criticalWoundTags = tags.ToArray();
                Debug.Log($"[Wound] Critical tag set: {location.criticalWoundTag}");
            }

            // Grant resources (placeholder — wire to ResourceManager in Stage 9-E)
            if (location.woundResources != null && location.woundResources.Length > 0)
                Debug.Log($"[Wound] Resources: {location.woundResources.Length} entries (wire to ResourceManager in 9-E)");

            // ── Impervious: effects fire but no behavior card removed ──
            if (location.isImpervious)
            {
                Debug.Log("[Wound] Location is IMPERVIOUS — no behavior card removed");
                OnWoundResolved?.Invoke(hunterId, outcome, location.locationName);
                ai._woundDeckPublic.SendToDiscard(location);
                return outcome;
            }

            // ── Remove behavior card (default: top of deck) ───────────
            var removedCard = ai._behaviorDeckPublic.RemoveTopCard();
            if (removedCard != null)
                Debug.Log($"[Wound] '{removedCard.cardName}' removed from monster health pool");

            OnBehaviorCardRemoved?.Invoke();
            OnWoundResolved?.Invoke(hunterId, outcome, location.locationName);
            ai._woundDeckPublic.SendToDiscard(location);

            // Sync deck counts
            CurrentState.monster.behaviorDeckCount    = ai._behaviorDeckPublic.DeckCount;
            CurrentState.monster.behaviorDiscardCount = ai._behaviorDeckPublic.DiscardCount;
            CurrentState.monster.woundDeckCount       = ai._woundDeckPublic.DeckCount;
            CurrentState.monster.woundDiscardCount    = ai._woundDeckPublic.DiscardCount;

            // ── Defeat check ──────────────────────────────────────────
            if (ai._behaviorDeckPublic.IsDefeated)
            {
                Debug.Log("[Combat] *** MONSTER DEFEATED — last behavior card removed by wound ***");
                HandleMonsterDefeated();
            }

            return outcome;
        }

        private int GetHunterStat(string hunterId, string stat)
        {
            var hunter = GetHunter(hunterId);
            if (hunter != null)
            {
                return stat switch
                {
                    "strength" => hunter.strength,
                    "luck"     => hunter.luck,
                    "accuracy" => hunter.accuracy,
                    _          => 0,
                };
            }
            // Fallback defaults for testing
            return stat switch
            {
                "strength" => 3,
                "luck"     => 1,
                _          => 0,
            };
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
