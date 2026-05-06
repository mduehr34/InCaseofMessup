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

        // ── Grid Reference ───────────────────────────────────────
        private IGridManager _grid;
        public void InjectGrid(IGridManager grid) => _grid = grid;

        // ── Cached Stat Block ────────────────────────────────────
        private MonsterStatBlock _statBlock;

        // ── Events ───────────────────────────────────────────────
        // Fires immediately mid-turn when the last Removable card is removed
        public event System.Action OnMonsterDefeated;
        // Fires whenever a card is successfully removed — UI uses this to rebuild the deck panel
        public event System.Action OnBehaviorCardRemoved;

        // ── State ────────────────────────────────────────────────
        private bool _apexTriggered = false;
        private BehaviorCardSO _lastDrawnCard; // Prevents same card landing first after reshuffle

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

            // Cache the stat block so ExecuteCard can read accuracy/strength
            int statIdx = difficulty == "Hardened" ? 1 : difficulty == "Apex" ? 2 : 0;
            if (monster.statBlocks != null && monster.statBlocks.Length > statIdx)
                _statBlock = monster.statBlocks[statIdx];

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
            // Permanent cards pre-empt the normal draw when their trigger fires.
            // EvaluateTrigger requires CombatState — wired in a future stage.
            // Until then, EvaluateTrigger(string) always returns false.
            foreach (var perm in _permanentCards)
            {
                if (EvaluateTrigger(perm.triggerCondition))
                {
                    Debug.Log($"[MonsterAI] Permanent card triggered: {perm.cardName}");
                    return perm;
                }
            }

            // If active deck empty: rebuild, reshuffle, then prevent same card landing first
            if (_activeDeck.Count == 0)
            {
                Debug.Log("[MonsterAI] Active deck exhausted — rebuilding and reshuffling");
                RebuildActiveDeck();
                ShuffleDeck(_activeDeck);

                // Don't let the just-drawn card appear first again after a reshuffle
                if (_lastDrawnCard != null && _activeDeck.Count > 1 &&
                    _activeDeck[0] == _lastDrawnCard)
                {
                    int swapIdx = Random.Range(1, _activeDeck.Count);
                    (_activeDeck[0], _activeDeck[swapIdx]) = (_activeDeck[swapIdx], _activeDeck[0]);
                    Debug.Log($"[MonsterAI] Anti-repeat swap — moved \"{_lastDrawnCard.cardName}\" " +
                              $"away from top of reshuffled deck");
                }
            }

            // Draw from top
            var card = _activeDeck[0];
            _activeDeck.RemoveAt(0);
            _lastDrawnCard = card;

            Debug.Log($"[MonsterAI] Drew: \"{card.cardName}\" " +
                      $"({card.group}/{card.cardType}) " +
                      $"Draw pile remaining: {_activeDeck.Count}");
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

        // Returns a random removable card name, or null if none remain
        public string GetRandomRemovableCardName()
        {
            var all = new List<BehaviorCardSO>();
            all.AddRange(_openingCards);
            all.AddRange(_escalationCards);
            all.AddRange(_apexCards);
            if (all.Count == 0) return null;
            return all[Random.Range(0, all.Count)].cardName;
        }

        // ── Card Removal ─────────────────────────────────────────
        public bool RemoveCard(string cardName)
        {
            bool removed = false;

            removed |= RemoveFromList(_openingCards,    cardName);
            removed |= RemoveFromList(_escalationCards, cardName);
            removed |= RemoveFromList(_apexCards,       cardName);

            // Also evict from the active draw pile if it's sitting there
            var inActive = _activeDeck.FirstOrDefault(c =>
                (c.cardName == cardName ||
                 string.Equals(c.name, cardName, System.StringComparison.OrdinalIgnoreCase))
                && c.cardType != BehaviorCardType.Permanent);
            if (inActive != null) _activeDeck.Remove(inActive);

            if (removed)
            {
                Debug.Log($"[Combat] Monster behavior card discarded: \"{cardName}\" " +
                          $"(removable remaining: {RemainingRemovableCount})");
                OnBehaviorCardRemoved?.Invoke();

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

            return removed;
        }

        private bool RemoveFromList(List<BehaviorCardSO> list, string cardName)
        {
            // Match by cardName field first, then fall back to Unity asset name (c.name)
            // so MonsterSO breakRemovesCardNames can use either the display name or the asset ID
            var card = list.FirstOrDefault(c =>
                c.cardName == cardName ||
                string.Equals(c.name, cardName, System.StringComparison.OrdinalIgnoreCase));
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

        // ── ExecuteCard ──────────────────────────────────────────
        public BehaviorCardResult ExecuteCard(BehaviorCardSO card, CombatState state)
        {
            var result = new BehaviorCardResult();
            Debug.Log($"[MonsterAI] ExecuteCard: {card.cardName}");

            if (!EvaluateTrigger(card, state))
            {
                Debug.Log($"[MonsterAI] Trigger not met — {card.cardName} skipped");
                return result;
            }

            // ── Movement ──────────────────────────────────────────
            if (card.movementPattern != MovementPattern.None && card.movementDistance > 0)
                result = ApplyMovement(card, state, result);

            // ── Attack ────────────────────────────────────────────
            if (card.attackTargetType != AttackTargetType.None && card.attackDamage > 0)
                result = ApplyAttack(card, state, result);

            // ── Special ───────────────────────────────────────────
            if (!string.IsNullOrEmpty(card.specialTag))
                result = ApplySpecial(card, state, result);

            // ── Reshuffle ─────────────────────────────────────────
            if (card.isShuffle)
            {
                ShuffleDeck(_activeDeck);
                Debug.Log("[MonsterAI] Deck reshuffled after card");
            }

            return result;
        }

        // ── Trigger Evaluation ───────────────────────────────────
        // String-only overload — used by DrawNextCard for permanent card pre-check.
        // Requires CombatState to evaluate properly; returns false until wired in a future stage.
        private bool EvaluateTrigger(string triggerCondition) => false;

        // Full evaluation — used by ExecuteCard
        private bool EvaluateTrigger(BehaviorCardSO card, CombatState state)
        {
            string cond = (card.triggerCondition ?? "Always").Trim().ToLower();

            if (cond == "" || cond == "always")
            {
                Debug.Log($"[MonsterAI] EvaluateTrigger: Always — met");
                return true;
            }

            var monster      = state.monster;
            var aggroHunter  = FindHunter(state, state.aggroHolderId);
            var monCell      = new Vector2Int(monster.gridX, monster.gridY);
            int distToAggro  = aggroHunter != null && _grid != null
                ? _grid.GetDistance(monCell, new Vector2Int(aggroHunter.gridX, aggroHunter.gridY))
                : 99;

            // "any hunter is adjacent" / "no hunter is adjacent"
            bool anyAdjacent = AnyHunterWithinRange(state, monCell, 1);
            if (cond.Contains("no hunter") && cond.Contains("adjacent"))
            {
                bool met = !anyAdjacent;
                Debug.Log($"[MonsterAI] EvaluateTrigger: no hunter adjacent — {met}");
                return met;
            }
            if (cond.Contains("any hunter") && cond.Contains("adjacent"))
            {
                Debug.Log($"[MonsterAI] EvaluateTrigger: any hunter adjacent — {anyAdjacent}");
                return anyAdjacent;
            }

            // "aggro target is adjacent"
            if (cond.Contains("aggro target") && cond.Contains("adjacent"))
            {
                bool met = distToAggro <= 1;
                Debug.Log($"[MonsterAI] EvaluateTrigger: aggro adjacent (dist={distToAggro}) — {met}");
                return met;
            }

            // "aggro target is N+ spaces away"
            if (cond.Contains("spaces away") || cond.Contains("space away"))
            {
                var m = System.Text.RegularExpressions.Regex.Match(cond, @"(\d+)\+?\s+space");
                if (m.Success && int.TryParse(m.Groups[1].Value, out int minDist))
                {
                    bool met = distToAggro >= minDist;
                    Debug.Log($"[MonsterAI] EvaluateTrigger: aggro {minDist}+ spaces away " +
                              $"(dist={distToAggro}) — {met}");
                    return met;
                }
            }

            // "any hunter is behind" (rear arc)
            if (cond.Contains("behind"))
            {
                var behind = GetHuntersInArc(state, monster, FacingArc.Rear);
                bool met = behind.Count > 0;
                Debug.Log($"[MonsterAI] EvaluateTrigger: any hunter behind — {met}");
                return met;
            }

            // "[partName] shell is broken" / "[partName] is broken"
            if (cond.Contains("broken"))
            {
                foreach (var part in monster.parts)
                {
                    string pname = part.partName.ToLower();
                    if (!cond.Contains(pname)) continue;
                    bool shellCheck = cond.Contains("shell") ? part.shellCurrent <= 0 : part.isBroken;
                    Debug.Log($"[MonsterAI] EvaluateTrigger: {part.partName} broken check — {shellCheck}");
                    return shellCheck;
                }
            }

            // "below 50%" / "below half"
            if (cond.Contains("below 50%") || cond.Contains("below half"))
            {
                int totalFlesh = 0, maxFlesh = 0;
                foreach (var p in monster.parts) { totalFlesh += p.fleshCurrent; maxFlesh += p.fleshMax; }
                bool met = maxFlesh > 0 && totalFlesh < maxFlesh / 2;
                Debug.Log($"[MonsterAI] EvaluateTrigger: below half HP ({totalFlesh}/{maxFlesh}) — {met}");
                return met;
            }

            // Unhandled — treat as Always and warn
            Debug.LogWarning($"[MonsterAI] EvaluateTrigger: unhandled condition " +
                             $"'{card.triggerCondition}' — treating as Always");
            return true;
        }

        // ── Movement ─────────────────────────────────────────────
        private BehaviorCardResult ApplyMovement(BehaviorCardSO card, CombatState state,
                                                  BehaviorCardResult result)
        {
            if (_grid == null)
            {
                Debug.LogWarning("[MonsterAI] IGridManager not injected — movement skipped");
                return result;
            }

            var monster     = state.monster;
            var aggroHunter = FindHunter(state, state.aggroHolderId);

            if (aggroHunter == null)
            {
                Debug.LogWarning("[MonsterAI] No aggro target — movement skipped");
                return result;
            }

            if (card.movementPattern == MovementPattern.Pivot)
            {
                // Pivot: face lowest-flesh hunter, no position change
                var target = FindHunterWithLowestFlesh(state);
                if (target != null)
                {
                    int dx = target.gridX - monster.gridX;
                    int dy = target.gridY - monster.gridY;
                    monster.facingX = dx == 0 ? 0 : (dx > 0 ? 1 : -1);
                    monster.facingY = dy == 0 ? 0 : (dy > 0 ? 1 : -1);
                    Debug.Log($"[MonsterAI] Pivot — monster now facing ({monster.facingX},{monster.facingY}) " +
                              $"toward {target.hunterName}");
                }
                return result;
            }

            // Approach / Charge — step toward aggro target
            var targetCell  = new Vector2Int(aggroHunter.gridX, aggroHunter.gridY);
            var currentCell = new Vector2Int(monster.gridX, monster.gridY);
            var bestCell    = currentCell;

            for (int step = 0; step < card.movementDistance; step++)
            {
                var next = StepToward(bestCell, targetCell);
                if (next == bestCell) break; // Already adjacent or no progress

                bool occupied = _grid.IsOccupied(next);
                if (occupied && card.movementPattern != MovementPattern.Charge) break;
                if (!_grid.IsInBounds(next)) break;

                bestCell = next;
            }

            if (bestCell != currentCell)
            {
                int dx = bestCell.x - currentCell.x;
                int dy = bestCell.y - currentCell.y;
                if (dx != 0 || dy != 0)
                {
                    monster.facingX = dx == 0 ? 0 : (dx > 0 ? 1 : -1);
                    monster.facingY = dy == 0 ? 0 : (dy > 0 ? 1 : -1);
                }

                monster.gridX = bestCell.x;
                monster.gridY = bestCell.y;
                result.monsterMoved   = true;
                result.newMonsterCell = bestCell;

                Debug.Log($"[MonsterAI] Monster moved to ({bestCell.x},{bestCell.y}) " +
                          $"facing ({monster.facingX},{monster.facingY})");
            }

            return result;
        }

        // ── Attack ────────────────────────────────────────────────
        private BehaviorCardResult ApplyAttack(BehaviorCardSO card, CombatState state,
                                               BehaviorCardResult result)
        {
            var monster = state.monster;
            var monCell = new Vector2Int(monster.gridX, monster.gridY);
            var targets = new List<HunterCombatState>();

            switch (card.attackTargetType)
            {
                case AttackTargetType.AggroTarget:
                    var aggro = FindHunter(state, state.aggroHolderId);
                    if (aggro != null) targets.Add(aggro);
                    break;

                case AttackTargetType.AllAdjacent:
                    targets = GetHuntersWithinRange(state, monCell, 1);
                    break;

                case AttackTargetType.AllBehind:
                    targets = GetHuntersInArc(state, monster, FacingArc.Rear);
                    break;

                case AttackTargetType.AllInFront:
                    targets = GetHuntersInArc(state, monster, FacingArc.Front);
                    break;

                case AttackTargetType.AllInRange:
                    targets = GetHuntersWithinRange(state, monCell, card.attackRange);
                    break;
            }

            foreach (var target in targets)
            {
                if (target.isCollapsed) continue;
                string zone = DamageRandomZone(target, card.attackDamage);
                result.hits.Add(new BehaviorCardResult.HitRecord
                {
                    hunterId = target.hunterId,
                    zone     = zone,
                    damage   = card.attackDamage,
                });
            }

            if (targets.Count == 0)
                Debug.Log($"[MonsterAI] Attack ({card.attackTargetType}) — no valid targets");

            return result;
        }

        // ── Special ───────────────────────────────────────────────
        private BehaviorCardResult ApplySpecial(BehaviorCardSO card, CombatState state,
                                                BehaviorCardResult result)
        {
            result.specialFired = true;
            result.specialTag   = card.specialTag;
            var monster = state.monster;
            var monCell = new Vector2Int(monster.gridX, monster.gridY);

            // STANCE:tagname — set the monster's current stance
            if (card.specialTag.StartsWith("STANCE:"))
            {
                string tag = card.specialTag.Substring(7);
                monster.currentStanceTag = tag;
                Debug.Log($"[MonsterAI] Special: stance set to '{tag}'");
                return result;
            }

            // REGEN:N — restore N flesh to most-damaged non-broken part
            if (card.specialTag.StartsWith("REGEN:") &&
                int.TryParse(card.specialTag.Substring(6), out int regenAmount))
            {
                int lowestFlesh = int.MaxValue;
                int targetIdx   = -1;
                for (int i = 0; i < monster.parts.Length; i++)
                {
                    if (monster.parts[i].isBroken) continue;
                    if (monster.parts[i].fleshCurrent < lowestFlesh)
                    {
                        lowestFlesh = monster.parts[i].fleshCurrent;
                        targetIdx   = i;
                    }
                }
                if (targetIdx >= 0)
                {
                    var p = monster.parts[targetIdx];
                    p.fleshCurrent = Mathf.Min(p.fleshMax, p.fleshCurrent + regenAmount);
                    monster.parts[targetIdx] = p;
                    Debug.Log($"[MonsterAI] Special: REGEN {regenAmount} on {p.partName} " +
                              $"→ {p.fleshCurrent}/{p.fleshMax}");
                }
                return result;
            }

            // PINNED — apply Pinned to all adjacent hunters
            if (card.specialTag == "PINNED")
            {
                var adjacent = GetHuntersWithinRange(state, monCell, 1);
                foreach (var h in adjacent)
                {
                    var effects = new List<string>(h.activeStatusEffects ?? new string[0]);
                    if (!effects.Contains("Pinned")) effects.Add("Pinned");
                    h.activeStatusEffects = effects.ToArray();
                    Debug.Log($"[MonsterAI] Special: {h.hunterName} gains Pinned");
                }
                return result;
            }

            // AGGRO:LOWEST — move aggro to hunter with lowest total flesh
            if (card.specialTag == "AGGRO:LOWEST")
            {
                var lowest = FindHunterWithLowestFlesh(state);
                if (lowest != null)
                {
                    state.aggroHolderId = lowest.hunterId;
                    Debug.Log($"[MonsterAI] Special: aggro moved to {lowest.hunterName} (lowest flesh)");
                }
                return result;
            }

            // STUN_SELF — monster skips next card (CombatManager checks currentStanceTag)
            if (card.specialTag == "STUN_SELF")
            {
                monster.currentStanceTag = "STUNNED";
                Debug.Log("[MonsterAI] Special: STUN_SELF — monster enters STUNNED stance");
                return result;
            }

            Debug.LogWarning($"[MonsterAI] Special tag '{card.specialTag}' unhandled");
            return result;
        }

        // ── Targeting Helpers ────────────────────────────────────
        private static HunterCombatState FindHunter(CombatState state, string id)
        {
            if (state?.hunters == null || id == null) return null;
            return System.Array.Find(state.hunters, h => h.hunterId == id && !h.isCollapsed);
        }

        private static HunterCombatState FindHunterWithLowestFlesh(CombatState state)
        {
            HunterCombatState best = null;
            int lowestFlesh = int.MaxValue;
            foreach (var h in state.hunters)
            {
                if (h.isCollapsed) continue;
                int total = 0;
                foreach (var z in h.bodyZones) total += z.fleshCurrent;
                if (total < lowestFlesh) { lowestFlesh = total; best = h; }
            }
            return best;
        }

        private bool AnyHunterWithinRange(CombatState state, Vector2Int origin, int range)
        {
            if (_grid == null) return false;
            foreach (var h in state.hunters)
            {
                if (h.isCollapsed) continue;
                if (_grid.GetDistance(origin, new Vector2Int(h.gridX, h.gridY)) <= range) return true;
            }
            return false;
        }

        private List<HunterCombatState> GetHuntersWithinRange(CombatState state, Vector2Int origin, int range)
        {
            var result = new List<HunterCombatState>();
            if (_grid == null) return result;
            foreach (var h in state.hunters)
            {
                if (h.isCollapsed) continue;
                if (_grid.GetDistance(origin, new Vector2Int(h.gridX, h.gridY)) <= range) result.Add(h);
            }
            return result;
        }

        private List<HunterCombatState> GetHuntersInArc(CombatState state, MonsterCombatState monster,
                                                          FacingArc arc)
        {
            var result = new List<HunterCombatState>();
            if (_grid == null) return result;
            var monCell   = new Vector2Int(monster.gridX, monster.gridY);
            var monFacing = new Vector2Int(monster.facingX, monster.facingY);
            foreach (var h in state.hunters)
            {
                if (h.isCollapsed) continue;
                var hunterCell = new Vector2Int(h.gridX, h.gridY);
                if (_grid.GetArcFromAttackerToTarget(hunterCell, monCell, monFacing) == arc)
                    result.Add(h);
            }
            return result;
        }

        // Step one cell toward target, preferring the axis with greater distance
        private static Vector2Int StepToward(Vector2Int from, Vector2Int to)
        {
            int dx    = to.x - from.x;
            int dy    = to.y - from.y;
            int stepX = dx == 0 ? 0 : (dx > 0 ? 1 : -1);
            int stepY = dy == 0 ? 0 : (dy > 0 ? 1 : -1);
            if (Mathf.Abs(dx) >= Mathf.Abs(dy))
                return new Vector2Int(from.x + stepX, from.y);
            return new Vector2Int(from.x, from.y + stepY);
        }

        // Apply flat flesh damage to a weighted-random body zone; returns zone name
        private static string DamageRandomZone(HunterCombatState hunter, int damage)
        {
            if (hunter.bodyZones == null || hunter.bodyZones.Length == 0) return "Unknown";

            var weights = new (string zone, int weight)[]
            {
                ("Head",     1),
                ("Torso",    3),
                ("LeftArm",  2),
                ("RightArm", 2),
                ("LeftLeg",  2),
                ("RightLeg", 2),
            };

            int total = 0;
            foreach (var w in weights) total += w.weight;
            int roll = Random.Range(0, total);

            string chosen = "Torso";
            int running = 0;
            foreach (var w in weights)
            {
                running += w.weight;
                if (roll < running) { chosen = w.zone; break; }
            }

            for (int i = 0; i < hunter.bodyZones.Length; i++)
            {
                if (hunter.bodyZones[i].zone != chosen) continue;
                var zone = hunter.bodyZones[i];
                zone.fleshCurrent = Mathf.Max(0, zone.fleshCurrent - damage);
                hunter.bodyZones[i] = zone;
                Debug.Log($"[MonsterAI] {hunter.hunterName} takes {damage} flesh to {chosen} " +
                          $"({zone.fleshCurrent}/{zone.fleshMax})");
                return chosen;
            }
            return chosen;
        }
    }
}
