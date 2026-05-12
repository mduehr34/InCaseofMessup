using System.Collections.Generic;
using UnityEngine;
using MnM.Core.Data;

namespace MnM.Core.Systems
{
    public class MonsterAI : IMonsterAI
    {
        // ── Dependencies ─────────────────────────────────────────────
        private IGridManager _grid;
        public void InjectGrid(IGridManager grid) => _grid = grid;

        private MonsterSO _monster;

        // ── Events ───────────────────────────────────────────────────
        public event System.Action OnMonsterDefeated;
        public event System.Action OnBehaviorCardRemoved;
        public event System.Action<GritWindowPhase, BehaviorCardSO> OnGritWindow;

        // ── Deck Wrappers ────────────────────────────────────────────
        private BehaviorDeck _behaviorDeck = new();
        private WoundDeck    _woundDeck    = new();

        // Public accessors for CombatManager wound resolution
        public BehaviorDeck _behaviorDeckPublic => _behaviorDeck;
        public WoundDeck    _woundDeckPublic    => _woundDeck;

        // ── Critical Wound Tags ──────────────────────────────────────
        private HashSet<string> _criticalWoundTags = new();
        public void AddCriticalWoundTag(string tag) => _criticalWoundTags.Add(tag);

        // ── IMonsterAI: Health Count ─────────────────────────────────
        public int RemainingRemovableCount => _behaviorDeck.HealthPool;

        public bool HasRemovableCards() => !_behaviorDeck.IsDefeated;

        // ── InitializeDeck ───────────────────────────────────────────
        public void InitializeDeck(MonsterSO monster, string difficulty)
        {
            _monster = monster;

            int diffIndex = difficulty switch
            {
                "Hardened" => 1,
                "Apex"     => 2,
                _          => 0,  // Standard
            };

            _behaviorDeck.Build(monster, diffIndex);

            var woundPool = diffIndex == 0 ? monster.standardWoundDeck
                          : diffIndex == 1 ? monster.hardenedWoundDeck
                          :                  monster.apexWoundDeck;
            _woundDeck.Build(woundPool);

            _criticalWoundTags.Clear();

            Debug.Log($"[MonsterAI] Deck initialized for {monster.monsterName} ({difficulty}). " +
                      $"Health: {_behaviorDeck.HealthPool} | Wound deck: {_woundDeck.DeckCount}");
        }

        // ── DrawNextCard ─────────────────────────────────────────────
        public BehaviorCardSO DrawNextCard()
        {
            return _behaviorDeck.Draw();
        }

        // ── GetActiveBehaviorCards ───────────────────────────────────
        /// <summary>Returns deck top (up to 5) + mood cards in play for the UI panel.</summary>
        public BehaviorCardSO[] GetActiveBehaviorCards()
        {
            var list = new List<BehaviorCardSO>(_behaviorDeck.PeekTop(5));
            list.AddRange(_behaviorDeck.GetMoodCardsInPlay());
            return list.ToArray();
        }

        // ── RemoveCard (backward compat for old CardResolver path) ───
        public bool RemoveCard(string cardName)
        {
            bool removed = _behaviorDeck.RemoveByName(cardName);
            if (removed)
            {
                OnBehaviorCardRemoved?.Invoke();
                if (_behaviorDeck.IsDefeated) OnMonsterDefeated?.Invoke();
            }
            else
            {
                Debug.LogWarning($"[MonsterAI] RemoveCard: '{cardName}' not found");
            }
            return removed;
        }

        // ── ExecuteCard — New Sub-Phase Flow ─────────────────────────
        public BehaviorCardResult ExecuteCard(BehaviorCardSO card, CombatState state)
        {
            var result = new BehaviorCardResult();
            if (card == null) return result;

            // Check if critical wound condition alters this card's behavior
            if (!string.IsNullOrEmpty(card.criticalWoundCondition) &&
                _criticalWoundTags.Contains(card.criticalWoundCondition))
            {
                Debug.Log($"[MonsterAI] '{card.cardName}' using ALTERNATE behavior " +
                          $"(tag: {card.criticalWoundCondition})");
            }

            Debug.Log($"[MonsterAI] ExecuteCard: {card.cardName} | " +
                      $"Target:{card.hasTargetIdentification} Move:{card.hasMovement} " +
                      $"Damage:{card.hasDamage} | Type:{card.cardType}");

            // ── Grit Window 1: after draw, before anything ────────────
            Debug.Log($"[MonsterAI] GRIT WINDOW: {GritWindowPhase.AfterDraw}");
            OnGritWindow?.Invoke(GritWindowPhase.AfterDraw, card);

            // ── Target Identification ─────────────────────────────────
            HunterCombatState target = null;
            if (card.hasTargetIdentification)
            {
                target = IdentifyTarget(card.targetRule, state);
                if (target != null)
                    Debug.Log($"[MonsterAI] Target identified: {target.hunterName} (rule: {card.targetRule})");
            }

            // ── Grit Window 2: after target identified ────────────────
            Debug.Log($"[MonsterAI] GRIT WINDOW: {GritWindowPhase.AfterTargetIdentification}");
            OnGritWindow?.Invoke(GritWindowPhase.AfterTargetIdentification, card);

            // ── Movement ──────────────────────────────────────────────
            if (card.hasMovement)
            {
                var moveTarget = target ?? FindHunter(state, state.aggroHolderId);
                result = ApplySubPhaseMovement(state, moveTarget, result);
            }

            // ── Grit Window 3: after movement ─────────────────────────
            Debug.Log($"[MonsterAI] GRIT WINDOW: {GritWindowPhase.AfterMovement}");
            OnGritWindow?.Invoke(GritWindowPhase.AfterMovement, card);

            // ── Damage — Determine ────────────────────────────────────
            HunterCombatState damageTarget = null;
            if (card.hasDamage)
            {
                damageTarget = target ?? FindHunter(state, state.aggroHolderId);
                if (damageTarget != null)
                {
                    string zoneName = !string.IsNullOrEmpty(card.forcedHunterBodyPart)
                        ? card.forcedHunterBodyPart
                        : DetermineRandomBodyPart();

                    Debug.Log($"[MonsterAI] Damage sub-phase: {damageTarget.hunterName}, zone: {zoneName}");
                    result.pendingDamageHunterId = damageTarget.hunterId;
                    result.pendingDamageZone     = zoneName;
                }
            }

            // ── Grit Window 4: after damage determined, before applied ─
            Debug.Log($"[MonsterAI] GRIT WINDOW: {GritWindowPhase.BeforeDamageApplied}");
            OnGritWindow?.Invoke(GritWindowPhase.BeforeDamageApplied, card);

            // ── Damage — Apply ────────────────────────────────────────
            if (card.hasDamage && damageTarget != null)
            {
                string zone   = result.pendingDamageZone;
                int    damage = 1;  // Default; scale with MonsterStatBlock in Stage 9

                ApplyDamageToZone(damageTarget, zone, damage);
                result.hits.Add(new BehaviorCardResult.HitRecord
                {
                    hunterId = damageTarget.hunterId,
                    zone     = zone,
                    damage   = damage,
                });
                Debug.Log($"[MonsterAI] Applied {damage} flesh to {damageTarget.hunterName} {zone}");
            }

            // ── Grit Window 5: after damage applied ───────────────────
            Debug.Log($"[MonsterAI] GRIT WINDOW: {GritWindowPhase.AfterDamageApplied}");
            OnGritWindow?.Invoke(GritWindowPhase.AfterDamageApplied, card);

            // ── Card Type Resolution ──────────────────────────────────
            switch (card.cardType)
            {
                case BehaviorCardType.Removable:
                    _behaviorDeck.SendToDiscard(card);
                    Debug.Log($"[MonsterAI] '{card.cardName}' → discard. Health: {_behaviorDeck.HealthPool}");
                    break;

                case BehaviorCardType.Mood:
                    _behaviorDeck.SendToMoodInPlay(card);
                    Debug.Log($"[MonsterAI] '{card.cardName}' → mood in play. Health: {_behaviorDeck.HealthPool}");
                    break;

                case BehaviorCardType.SingleTrigger:
                    _behaviorDeck.SendToPermanentlyRemoved(card);
                    Debug.Log($"[MonsterAI] '{card.cardName}' → permanently removed (SingleTrigger).");
                    break;
            }

            // ── Mood Card Removal Check ───────────────────────────────
            CheckMoodCardRemovals(state);

            // ── Defeat Check ──────────────────────────────────────────
            if (_behaviorDeck.IsDefeated)
            {
                Debug.Log("[MonsterAI] *** MONSTER DEFEATED — behavior deck exhausted ***");
                OnMonsterDefeated?.Invoke();
            }

            // ── Grit Window 6: end of monster turn ────────────────────
            Debug.Log($"[MonsterAI] GRIT WINDOW: {GritWindowPhase.EndOfMonsterTurn}");
            OnGritWindow?.Invoke(GritWindowPhase.EndOfMonsterTurn, card);

            result.monsterDefeated = _behaviorDeck.IsDefeated;
            return result;
        }

        // ── Target Identification ─────────────────────────────────────
        private HunterCombatState IdentifyTarget(string targetRule, CombatState state)
        {
            return (targetRule ?? "aggro").ToLower() switch
            {
                "aggro"         => FindHunter(state, state.aggroHolderId),
                "nearest"       => FindNearestHunter(state),
                "mostinjured"   => FindHunterWithLowestFlesh(state),
                "last_attacker" => FindHunter(state, state.lastAttackerId ?? state.aggroHolderId),
                _               => FindHunter(state, state.aggroHolderId),
            };
        }

        // ── Movement Sub-Phase ────────────────────────────────────────
        private BehaviorCardResult ApplySubPhaseMovement(CombatState state, HunterCombatState moveTarget,
                                                          BehaviorCardResult result)
        {
            if (_grid == null || moveTarget == null) return result;

            var monster    = state.monster;
            var targetCell = new Vector2Int(moveTarget.gridX, moveTarget.gridY);
            var current    = new Vector2Int(monster.gridX, monster.gridY);

            var next = StepToward(current, targetCell);
            // VERIFY-FIX: check full W×H footprint — single-cell IsOccupied missed IsDenied terrain cells
            if (next != current && IsFootprintClear(next, current, monster.footprintW, monster.footprintH))
            {
                int dx = next.x - current.x;
                int dy = next.y - current.y;
                monster.facingX = dx == 0 ? 0 : (dx > 0 ? 1 : -1);
                monster.facingY = dy == 0 ? 0 : (dy > 0 ? 1 : -1);
                monster.gridX   = next.x;
                monster.gridY   = next.y;
                result.monsterMoved   = true;
                result.newMonsterCell = next;
                Debug.Log($"[MonsterAI] Moved to ({next.x},{next.y}) facing ({monster.facingX},{monster.facingY})");
            }
            return result;
        }

        private bool IsFootprintClear(Vector2Int newTopLeft, Vector2Int oldTopLeft, int fpW, int fpH)
        {
            for (int dy = 0; dy < fpH; dy++)
            {
                for (int dx = 0; dx < fpW; dx++)
                {
                    var cell = new Vector2Int(newTopLeft.x + dx, newTopLeft.y + dy);
                    if (!_grid.IsInBounds(cell)) return false;
                    if (_grid.IsDenied(cell)) return false;
                    // Skip cells the monster already occupies in its current footprint
                    bool inOldFootprint = cell.x >= oldTopLeft.x && cell.x < oldTopLeft.x + fpW &&
                                         cell.y >= oldTopLeft.y && cell.y < oldTopLeft.y + fpH;
                    if (!inOldFootprint && _grid.IsOccupied(cell)) return false;
                }
            }
            return true;
        }

        // ── Mood Card Removal ─────────────────────────────────────────
        private void CheckMoodCardRemovals(CombatState state)
        {
            var moodCards = new List<BehaviorCardSO>(_behaviorDeck.GetMoodCardsInPlay());
            foreach (var mood in moodCards)
            {
                if (EvaluateMoodRemoval(mood, state))
                    _behaviorDeck.RemoveMoodCard(mood);
            }
        }

        private bool EvaluateMoodRemoval(BehaviorCardSO card, CombatState state)
        {
            string cond = (card.removalCondition ?? "").Trim().ToLower();
            if (string.IsNullOrEmpty(cond)) return false;

            // "N turns" countdown — full implementation in Stage 9
            if (cond.Contains("turns")) return false;

            // "hunter inflicts a wound" and "hunter spends N grit" — wired in Stage 9
            return false;
        }

        // ── Hunter Lookup Helpers ─────────────────────────────────────
        private static HunterCombatState FindHunter(CombatState state, string hunterId)
        {
            if (string.IsNullOrEmpty(hunterId) || state.hunters == null) return null;
            return System.Array.Find(state.hunters, h => h.hunterId == hunterId && !h.isCollapsed);
        }

        private HunterCombatState FindNearestHunter(CombatState state)
        {
            if (_grid == null) return FindHunter(state, state.aggroHolderId);
            HunterCombatState nearest = null;
            int minDist = int.MaxValue;
            var monCell = new Vector2Int(state.monster.gridX, state.monster.gridY);
            foreach (var h in state.hunters)
            {
                if (h.isCollapsed) continue;
                int dist = _grid.GetDistance(monCell, new Vector2Int(h.gridX, h.gridY));
                if (dist < minDist) { minDist = dist; nearest = h; }
            }
            return nearest;
        }

        private static HunterCombatState FindHunterWithLowestFlesh(CombatState state)
        {
            HunterCombatState result = null;
            int lowestFlesh = int.MaxValue;
            foreach (var h in state.hunters)
            {
                if (h.isCollapsed || h.bodyZones == null) continue;
                int total = 0;
                foreach (var z in h.bodyZones) total += z.fleshCurrent;
                if (total < lowestFlesh) { lowestFlesh = total; result = h; }
            }
            return result;
        }

        public List<HunterCombatState> GetHuntersWithinRange(CombatState state, Vector2Int origin, int range)
        {
            var result = new List<HunterCombatState>();
            foreach (var h in state.hunters)
            {
                if (h.isCollapsed) continue;
                int dist = _grid != null
                    ? _grid.GetDistance(origin, new Vector2Int(h.gridX, h.gridY))
                    : Mathf.Abs(h.gridX - origin.x) + Mathf.Abs(h.gridY - origin.y);
                if (dist <= range) result.Add(h);
            }
            return result;
        }

        public List<HunterCombatState> GetHuntersInArc(CombatState state, MonsterCombatState monster, FacingArc arc)
        {
            if (_grid == null) return new List<HunterCombatState>();
            var result    = new List<HunterCombatState>();
            var monCell   = new Vector2Int(monster.gridX, monster.gridY);
            var monFacing = new Vector2Int(monster.facingX, monster.facingY);
            foreach (var h in state.hunters)
            {
                if (h.isCollapsed) continue;
                var hunterCell = new Vector2Int(h.gridX, h.gridY);
                var hunterArc  = _grid.GetArcFromAttackerToTarget(hunterCell, monCell, monFacing);
                if (hunterArc == arc) result.Add(h);
            }
            return result;
        }

        // ── Body Part Helpers ─────────────────────────────────────────
        private static string DetermineRandomBodyPart()
        {
            var parts = new (string name, int weight)[]
            {
                ("Head",     1), ("Torso",    3),
                ("LeftArm",  2), ("RightArm", 2),
                ("LeftLeg",  2), ("RightLeg", 2),
            };
            int total = 0;
            foreach (var p in parts) total += p.weight;
            int roll    = Random.Range(0, total);
            int running = 0;
            foreach (var p in parts)
            {
                running += p.weight;
                if (roll < running) return p.name;
            }
            return "Torso";
        }

        private static void ApplyDamageToZone(HunterCombatState hunter, string zoneName, int damage)
        {
            if (hunter.bodyZones == null) return;
            for (int i = 0; i < hunter.bodyZones.Length; i++)
            {
                if (hunter.bodyZones[i].zone != zoneName) continue;
                var zone = hunter.bodyZones[i];
                zone.fleshCurrent       = Mathf.Max(0, zone.fleshCurrent - damage);
                hunter.bodyZones[i]     = zone;
                return;
            }
        }

        // ── Movement Geometry ─────────────────────────────────────────
        private static Vector2Int StepToward(Vector2Int from, Vector2Int to)
        {
            if (from == to) return from;
            int dx = to.x - from.x;
            int dy = to.y - from.y;
            // Prefer the axis with the greater delta
            if (Mathf.Abs(dx) >= Mathf.Abs(dy))
                return new Vector2Int(from.x + (dx > 0 ? 1 : -1), from.y);
            return new Vector2Int(from.x, from.y + (dy > 0 ? 1 : -1));
        }
    }
}
