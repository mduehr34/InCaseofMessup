<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude Code to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 8-N | New Combat Runtime — BehaviorDeck, Wound Resolution, MonsterAI Rebuild
Status: Stage 8-M complete. Data model reworked — shell/flesh HP
removed, BehaviorCardSO uses sub-phase booleans, WoundLocationSO
exists, MonsterSO uses pool arrays and deckCompositions.
Task: Wire up the runtime implementation of the new health system.
CombatState gets per-monster deck/wound tracking. BehaviorDeck and
WoundDeck get wrapper classes. MonsterAI.InitializeDeck is rebuilt
to pull from pools. MonsterAI.ExecuteCard is rebuilt with the new
sub-phase flow and Grit window events. CombatManager gets wound
resolution and defeat condition. Full Gaunt Standard deck is
authored as SO assets.

Read these files before doing anything:
- CLAUDE.md
- .cursorrules
- _Docs/Stage_08/STAGE_08_N.md
- _Docs/Stage_08/STAGE_08_M.md         ← data model reference
- Assets/_Game/Scripts/Core.Data/CombatState.cs
- Assets/_Game/Scripts/Core.Data/MonsterSO.cs
- Assets/_Game/Scripts/Core.Data/BehaviorCardSO.cs
- Assets/_Game/Scripts/Core.Data/WoundLocationSO.cs
- Assets/_Game/Scripts/Core.Systems/MonsterAI.cs
- Assets/_Game/Scripts/Core.Systems/CombatManager.cs
- Assets/_Game/Scripts/Core.Systems/IMonsterAI.cs

Then confirm:
- MonsterSO now has baseCardPool, advancedCardPool, overwhelmingCardPool,
  deckCompositions[], standardWoundDeck[], hardenedWoundDeck[]
- BehaviorCardSO has hasMovement, hasDamage, hasTargetIdentification, targetRule
- WoundLocationSO exists with woundTarget, isTrap, isImpervious, criticalWoundTag
- CombatState does NOT yet have behaviorDeck/woundDeck runtime tracking
- MonsterAI.InitializeDeck still uses the old BehaviorGroup/escalation logic
- MonsterAI.ExecuteCard still uses movementPattern/attackTargetType (8-L fields)
- What you will NOT build: Grit spending UI (Stage 9+), full authored
  deck for all monsters (Stage 9-A onward)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 8-N: New Combat Runtime — BehaviorDeck, Wound Resolution, MonsterAI Rebuild

**Resuming from:** Stage 8-M complete — data model reworked; shell/flesh HP removed; `WoundLocationSO` created; `MonsterSO` now uses pool arrays and `BehaviorDeckComposition`
**Done when:** MonsterAI builds its deck from pool arrays using `deckCompositions`; ExecuteCard runs the correct sub-phase sequence; wound resolution draws from the wound deck, runs force rolls, and removes behavior cards; defeat fires when the deck is empty; full Gaunt Standard and Hardened decks are authored as SO assets
**Commit:** `"8N: New combat runtime — BehaviorDeck wrapper, wound resolution, MonsterAI rebuild for new health model"`
**Next session:** STAGE_08_O.md

---

## Context: What Was Built in 8-L and Why It's Being Replaced

Stage 8-L implemented `ExecuteCard()` and `EvaluateTrigger()` using the old data model — `movementPattern`, `attackTargetType`, `attackDamage`, `BehaviorGroup`. That implementation was a valid proof-of-concept that revealed the system was frustrating to play: the monster felt arbitrary and the round pacing was wrong because health was spread across body parts.

Stage 8-T replaced the data model entirely. This session rebuilds the **runtime** side using the new model. The old 8-L execution code will be removed or replaced. Keep any helper methods that are still valid (e.g., `StepToward`, `FindHunter`, `GetHuntersWithinRange`) — they still apply to movement and targeting.

---

## Part 1: CombatState.cs — Per-Monster Runtime Deck State

Open `Assets/_Game/Scripts/Core.Data/CombatState.cs`.

The `MonsterCombatState` class needs runtime deck tracking. Add these fields alongside the existing `gridX`, `gridY`, `facingX`, `facingY`, `currentStanceTag` fields:

```csharp
// ── Behavior Deck Runtime State ───────────────────────────────────
// Do NOT store BehaviorCardSO references here — these are managed by
// MonsterAI's BehaviorDeck wrapper. CombatState tracks counts only
// so the UI and save/load system can read health without touching MonsterAI.

public int behaviorDeckCount;        // Cards in draw pile
public int behaviorDiscardCount;     // Cards in discard pile
public int moodCardsInPlayCount;     // Active Mood cards (not removable by wounds while active)
public int permanentlyRemovedCount;  // Cards removed from the game — never return

// ── Wound Deck Runtime State ──────────────────────────────────────
public int woundDeckCount;
public int woundDiscardCount;

// ── Critical Wound Tags ───────────────────────────────────────────
// Set when a critical wound lands on a location with a criticalWoundTag.
// Read by MonsterAI when resolving ExecuteCard with criticalWoundCondition.
public string[] criticalWoundTags;   // e.g. ["GauntJaw_Critical"]

// ── Per-Hunter Grit (Runtime) ─────────────────────────────────────
// Grit is a combat resource, NOT a persistent stat. Initialize from
// CampaignSO.startingGrit at combat start; do not read from CharacterSO.
// Tracked here so CombatManager and UI can access it without going through MonsterAI.
```

Add `currentGrit` to `HunterCombatState`:
```csharp
public int currentGrit;     // Combat resource — spent at Grit windows
```

---

## Part 2: BehaviorDeck.cs — Wrapper Class

Create `Assets/_Game/Scripts/Core.Systems/BehaviorDeck.cs`:

```csharp
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
        private List<BehaviorCardSO> _deck    = new();
        private List<BehaviorCardSO> _discard = new();
        private List<BehaviorCardSO> _moodInPlay = new();
        private List<BehaviorCardSO> _permanentlyRemoved = new();

        // ── Read-only counts (for CombatState sync) ─────────────────
        public int DeckCount              => _deck.Count;
        public int DiscardCount           => _discard.Count;
        public int MoodInPlayCount        => _moodInPlay.Count;
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
            _deck.Clear(); _discard.Clear(); _moodInPlay.Clear(); _permanentlyRemoved.Clear();

            if (difficultyIndex < 0 || difficultyIndex >= monster.deckCompositions.Length)
            {
                Debug.LogError($"[BehaviorDeck] Invalid difficulty index {difficultyIndex} for {monster.monsterName}");
                return;
            }

            var comp = monster.deckCompositions[difficultyIndex];
            var combined = new List<BehaviorCardSO>();

            combined.AddRange(DrawFromPool(monster.baseCardPool,        comp.baseCardCount));
            combined.AddRange(DrawFromPool(monster.advancedCardPool,    comp.advancedCardCount));
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
            var top = _deck[0]; _deck.RemoveAt(0); _deck.Add(top);
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
```

---

## Part 3: WoundDeck.cs — Wrapper Class

Create `Assets/_Game/Scripts/Core.Systems/WoundDeck.cs`:

```csharp
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
            _deck.Clear(); _discard.Clear();
            if (locations == null) return;
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
```

---

## Part 4: MonsterAI — Remove Old Deck Fields, Add New Wrappers

Open `Assets/_Game/Scripts/Core.Systems/MonsterAI.cs`.

**Remove** all fields and methods from the old escalation/group deck system:
- `_openingCards`, `_escalationCards`, `_apexCards`, `_permanentCards`, `_activeDeck` (the old Lists)
- Old `InitializeDeck()` body
- Old `ExecuteCard()` body (keep the method signature, replace the body)
- `ApplyMovement()`, `ApplyAttack()`, `ApplySpecial()` (from 8-L — being replaced by new sub-phase methods)
- `ShuffleDeck()`
- `EvaluateTrigger()` will be rebuilt below

**Keep:**
- `StepToward(from, to)` — still used by movement
- `FindHunter(state, id)` — still used
- `FindHunterWithLowestFlesh(state)` — still used
- `GetHuntersWithinRange(state, origin, range)` — still used for targeting
- `GetHuntersInArc(state, monster, arc)` — still used
- `_grid` field and `InjectGrid()` — still used
- `DamageRandomZone()` — still used when `forcedHunterBodyPart` is empty

**Add the new deck instances:**

```csharp
// ── Deck Wrappers ────────────────────────────────────────────────
private BehaviorDeck _behaviorDeck = new();
private WoundDeck    _woundDeck    = new();

// ── Critical Wound Tags ──────────────────────────────────────────
private HashSet<string> _criticalWoundTags = new();
```

---

## Part 5: Rebuild MonsterAI.InitializeDeck

Replace the old `InitializeDeck` body:

```csharp
public void InitializeDeck(MonsterSO monster, string difficulty)
{
    _monster = monster;

    int diffIndex = difficulty switch
    {
        "Hardened" => 1,
        "Apex"     => 2,
        _          => 0,   // Standard
    };

    _behaviorDeck.Build(monster, diffIndex);

    // Build wound deck for this difficulty
    var woundPool = diffIndex == 0 ? monster.standardWoundDeck
                  : diffIndex == 1 ? monster.hardenedWoundDeck
                  :                  monster.apexWoundDeck;
    _woundDeck.Build(woundPool);

    _criticalWoundTags.Clear();

    Debug.Log($"[MonsterAI] Deck initialized for {monster.monsterName} ({difficulty}). " +
              $"Health: {_behaviorDeck.HealthPool} | Wound deck: {_woundDeck.DeckCount}");
}
```

Update `IMonsterAI` to add `IReadOnlyList<BehaviorCardSO> GetMoodCardsInPlay()` and update `HasRemovableCards()` to use the new deck:

```csharp
// HasRemovableCards — used by UI to update defeat possibility display
public bool HasRemovableCards() => !_behaviorDeck.IsDefeated;

// GetActiveBehaviorCards — used by behavior deck panel in combat UI
// Returns deck top (up to 5) + mood cards in play
public BehaviorCardSO[] GetActiveBehaviorCards()
{
    var list = new List<BehaviorCardSO>(_behaviorDeck.PeekTop(5));
    list.AddRange(_behaviorDeck.GetMoodCardsInPlay());
    return list.ToArray();
}
```

---

## Part 6: Rebuild MonsterAI.DrawNextCard

Replace the old `DrawNextCard`:

```csharp
public BehaviorCardSO DrawNextCard()
{
    return _behaviorDeck.Draw();
}
```

---

## Part 7: Rebuild MonsterAI.ExecuteCard — New Sub-Phase Flow

Replace the old `ExecuteCard` entirely:

```csharp
public BehaviorCardResult ExecuteCard(BehaviorCardSO card, CombatState state)
{
    var result = new BehaviorCardResult();
    if (card == null) return result;

    // Check if critical wound condition alters this card's behavior
    string effectiveCondition = card.triggerCondition;
    string effectiveEffect    = card.effectDescription;
    if (!string.IsNullOrEmpty(card.criticalWoundCondition) &&
        _criticalWoundTags.Contains(card.criticalWoundCondition))
    {
        effectiveCondition = card.alternateTriggerCondition;
        effectiveEffect    = card.alternateEffectDescription;
        Debug.Log($"[MonsterAI] '{card.cardName}' using ALTERNATE behavior (tag: {card.criticalWoundCondition})");
    }

    Debug.Log($"[MonsterAI] ExecuteCard: {card.cardName} | " +
              $"Target:{card.hasTargetIdentification} Move:{card.hasMovement} Damage:{card.hasDamage} | " +
              $"Type:{card.cardType}");

    // ── Grit Window 1: after draw, before anything ────────────────
    OnGritWindow?.Invoke(GritWindowPhase.AfterDraw, card);

    // ── Target Identification ─────────────────────────────────────
    HunterCombatState target = null;
    if (card.hasTargetIdentification)
    {
        target = IdentifyTarget(card.targetRule, state);
        if (target != null)
            Debug.Log($"[MonsterAI] Target identified: {target.hunterName} (rule: {card.targetRule})");
    }

    // ── Grit Window 2: after target identified ────────────────────
    OnGritWindow?.Invoke(GritWindowPhase.AfterTargetIdentification, card);

    // ── Movement ─────────────────────────────────────────────────
    if (card.hasMovement)
        result = ApplySubPhaseMovement(state, target ?? FindHunter(state, state.aggroHolderId), result);

    // ── Grit Window 3: after movement ────────────────────────────
    OnGritWindow?.Invoke(GritWindowPhase.AfterMovement, card);

    // ── Damage Resolution ─────────────────────────────────────────
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

    // ── Grit Window 4: after damage determined, before applied ────
    OnGritWindow?.Invoke(GritWindowPhase.BeforeDamageApplied, card);

    // ── Apply Damage ──────────────────────────────────────────────
    if (card.hasDamage && damageTarget != null)
    {
        string zone   = result.pendingDamageZone;
        int    damage = 1;  // Default damage — scale with MonsterStatBlock in Stage 9

        ApplyDamageToZone(damageTarget, zone, damage);
        result.hits.Add(new BehaviorCardResult.HitRecord
        {
            hunterId = damageTarget.hunterId,
            zone     = zone,
            damage   = damage,
        });
        Debug.Log($"[MonsterAI] Applied {damage} flesh to {damageTarget.hunterName} {zone}");
    }

    // ── Grit Window 5: after damage applied ──────────────────────
    OnGritWindow?.Invoke(GritWindowPhase.AfterDamageApplied, card);

    // ── Card Type Resolution ──────────────────────────────────────
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

    // ── Mood Card Removal Check ───────────────────────────────────
    CheckMoodCardRemovals(state);

    // ── Defeat Check ──────────────────────────────────────────────
    if (_behaviorDeck.IsDefeated)
    {
        Debug.Log($"[MonsterAI] *** MONSTER DEFEATED — behavior deck exhausted ***");
        OnMonsterDefeated?.Invoke();
    }

    // ── Grit Window 6: end of monster turn ───────────────────────
    OnGritWindow?.Invoke(GritWindowPhase.EndOfMonsterTurn, card);

    result.monsterDefeated = _behaviorDeck.IsDefeated;
    return result;
}
```

---

## Part 8: Target Identification and Movement Helpers

Add these methods:

```csharp
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

private BehaviorCardResult ApplySubPhaseMovement(CombatState state, HunterCombatState moveTarget,
                                                  BehaviorCardResult result)
{
    if (_grid == null || moveTarget == null) return result;

    var monster    = state.monster;
    var targetCell = new Vector2Int(moveTarget.gridX, moveTarget.gridY);
    var current    = new Vector2Int(monster.gridX, monster.gridY);

    // Step one cell toward target (same logic as before, 1 step default)
    var next = StepToward(current, targetCell);
    if (next != current && !_grid.IsOccupied(next) && _grid.IsInBounds(next))
    {
        int dx = next.x - current.x;
        int dy = next.y - current.y;
        monster.facingX = dx == 0 ? 0 : (dx > 0 ? 1 : -1);
        monster.facingY = dy == 0 ? 0 : (dy > 0 ? 1 : -1);
        monster.gridX = next.x;
        monster.gridY = next.y;
        result.monsterMoved   = true;
        result.newMonsterCell = next;
        Debug.Log($"[MonsterAI] Moved to ({next.x},{next.y}) facing ({monster.facingX},{monster.facingY})");
    }
    return result;
}

private static string DetermineRandomBodyPart()
{
    // Weighted body part selection for monster attacks
    var parts = new (string name, int weight)[]
    {
        ("Head",     1), ("Torso",    3),
        ("LeftArm",  2), ("RightArm", 2),
        ("LeftLeg",  2), ("RightLeg", 2),
    };
    int total = 0;
    foreach (var p in parts) total += p.weight;
    int roll = Random.Range(0, total);
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
        zone.fleshCurrent = Mathf.Max(0, zone.fleshCurrent - damage);
        hunter.bodyZones[i] = zone;
        return;
    }
}
```

---

## Part 9: Mood Card Removal (Placeholder Evaluator)

Add this method to MonsterAI:

```csharp
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

    // "N turns" countdown — managed via stanceTag as a counter string
    if (cond.Contains("turns") &&
        int.TryParse(System.Text.RegularExpressions.Regex.Match(cond, @"\d+").Value, out int turns))
    {
        // Track via a temporary field in currentStanceTag — format: "MOOD_TURNS:N:cardName"
        string key = $"MOOD_TURNS_{card.cardName}";
        string val = state.monster.currentStanceTag ?? "";
        if (!val.Contains(key))
        {
            state.monster.currentStanceTag += $"|{key}:{turns}";
            return false;
        }
        // Decrement
        // Full implementation in Stage 9 when Grit UI is added
        return false;
    }

    // "hunter inflicts a wound" — checked externally when ResolveWound fires
    // "hunter spends N grit" — checked at Grit window by CombatManager
    // Both of these are wired in Stage 9; return false here as placeholder
    return false;
}
```

---

## Part 10: Grit Window Event

Add to `MonsterAI.cs` (alongside `OnMonsterDefeated`):

```csharp
public enum GritWindowPhase
{
    AfterDraw,
    AfterTargetIdentification,
    AfterMovement,
    BeforeDamageApplied,
    AfterDamageApplied,
    EndOfMonsterTurn,
}

public event System.Action<GritWindowPhase, BehaviorCardSO> OnGritWindow;
```

Add `OnGritWindow` to `IMonsterAI` as well.

---

## Part 11: Update BehaviorCardResult

Open `Assets/_Game/Scripts/Core.Data/BehaviorCardResult.cs`. Add new fields:

```csharp
public bool   monsterDefeated   = false;   // Defeat condition was met this execution
public string pendingDamageHunterId = null; // Set before Grit window, applied after
public string pendingDamageZone     = null;
```

---

## Part 12: CombatManager — Wound Resolution

Add wound resolution to `CombatManager`. This is called when `TryPlayCard` results in a hit:

```csharp
/// <summary>
/// Called when a hunter successfully hits the monster (to-hit roll passed).
/// Draws a wound location, runs the force roll, and removes a behavior card on wound/critical.
/// Returns the WoundOutcome for UI and event dispatch.
/// </summary>
public WoundOutcome ResolveWound(string hunterId)
{
    var hunter = GetHunter(hunterId);
    if (hunter == null || _monsterAI == null)
    {
        Debug.LogWarning("[Combat] ResolveWound: hunter or AI null");
        return WoundOutcome.Failure;
    }

    var woundDeck = (_monsterAI as MonsterAI)?._woundDeckPublic;
    if (woundDeck == null)
    {
        Debug.LogWarning("[Combat] ResolveWound: WoundDeck not accessible");
        return WoundOutcome.Failure;
    }

    // ── Draw wound location ───────────────────────────────────────
    var location = woundDeck.Draw();
    if (location == null)
    {
        Debug.LogWarning("[Combat] ResolveWound: wound deck empty");
        return WoundOutcome.Failure;
    }

    Debug.Log($"[Wound] Drew: {location.locationName} (target: {location.woundTarget}, " +
              $"trap: {location.isTrap}, impervious: {location.isImpervious})");

    // ── Trap ──────────────────────────────────────────────────────
    if (location.isTrap)
    {
        Debug.Log($"[Wound] TRAP: {location.trapEffect}");
        OnWoundResolved?.Invoke(hunterId, WoundOutcome.Trap, location.locationName);
        woundDeck.SendToDiscard(location);
        woundDeck.ReshuffleDiscardIntoDeck();   // Trap cycles back immediately
        return WoundOutcome.Trap;
    }

    // ── Force Roll ────────────────────────────────────────────────
    int roll = Random.Range(1, 11);   // d10
    // Terrain bonus from GridManager applied here in Stage 8-S
    int strength = GetHunterStat(hunterId, "strength");
    bool woundPassed = (roll + strength) > location.woundTarget;

    Debug.Log($"[Wound] Force roll: d10={roll} + STR={strength} = {roll + strength} vs target {location.woundTarget} " +
              $"→ {(woundPassed ? "WOUND CHECK PASSES" : "FAILURE")}");

    if (!woundPassed)
    {
        if (!string.IsNullOrEmpty(location.failureEffect))
            Debug.Log($"[Wound] Failure effect: {location.failureEffect}");
        OnWoundResolved?.Invoke(hunterId, WoundOutcome.Failure, location.locationName);
        woundDeck.SendToDiscard(location);
        return WoundOutcome.Failure;
    }

    // ── Critical Sub-Check (only when wound passed) ───────────────
    int luck = GetHunterStat(hunterId, "luck");
    int critThreshold = 10 - luck;   // Luck 2 → crit on d10 ≥ 8
    bool isCritical = roll >= critThreshold;

    Debug.Log($"[Wound] Critical check: d10 natural={roll} vs threshold {critThreshold} " +
              $"→ {(isCritical ? "CRITICAL" : "standard wound")}");

    // ── Apply Wound ───────────────────────────────────────────────
    WoundOutcome outcome = isCritical ? WoundOutcome.Critical : WoundOutcome.Wound;

    // Log effects
    if (!string.IsNullOrEmpty(location.woundEffect))
        Debug.Log($"[Wound] Wound effect: {location.woundEffect}");
    if (isCritical && !string.IsNullOrEmpty(location.criticalEffect))
        Debug.Log($"[Wound] Critical effect: {location.criticalEffect}");

    // Set critical wound tag
    if (isCritical && !string.IsNullOrEmpty(location.criticalWoundTag))
    {
        (_monsterAI as MonsterAI)?.AddCriticalWoundTag(location.criticalWoundTag);
        var monState = CurrentState.monster;
        var tags = new List<string>(monState.criticalWoundTags ?? new string[0]);
        if (!tags.Contains(location.criticalWoundTag)) tags.Add(location.criticalWoundTag);
        monState.criticalWoundTags = tags.ToArray();
        Debug.Log($"[Wound] Critical tag set: {location.criticalWoundTag}");
    }

    // Grant resources (placeholder — wire to ResourceManager in Stage 9-E)
    if (location.woundResources != null && location.woundResources.Length > 0)
        Debug.Log($"[Wound] Resources: {location.woundResources.Length} entries (wire to ResourceManager in 9-E)");

    // ── Impervious: effects fire but no behavior card removed ─────
    if (location.isImpervious)
    {
        Debug.Log($"[Wound] Location is IMPERVIOUS — no behavior card removed");
        OnWoundResolved?.Invoke(hunterId, outcome, location.locationName);
        woundDeck.SendToDiscard(location);
        return outcome;
    }

    // ── Remove behavior card (default: top of deck) ───────────────
    var removedCard = (_monsterAI as MonsterAI)?._behaviorDeckPublic?.RemoveTopCard();
    if (removedCard != null)
        Debug.Log($"[Wound] '{removedCard.cardName}' removed from monster health pool");

    OnWoundResolved?.Invoke(hunterId, outcome, location.locationName);
    woundDeck.SendToDiscard(location);

    // ── Defeat check ──────────────────────────────────────────────
    if ((_monsterAI as MonsterAI)?._behaviorDeckPublic?.IsDefeated == true)
    {
        Debug.Log("[Combat] *** MONSTER DEFEATED — last behavior card removed by wound ***");
        OnMonsterDefeated?.Invoke();
    }

    return outcome;
}

private int GetHunterStat(string hunterId, string stat)
{
    // Resolve stat from CharacterSO registry via GameStateManager — placeholder
    // Full implementation when CharacterSO registry is wired in Stage 9
    return stat switch
    {
        "strength" => 3,   // Default strength for testing
        "luck"     => 1,   // Default luck
        _          => 0,
    };
}

public event System.Action<string, WoundOutcome, string> OnWoundResolved;
```

**Add public accessors to MonsterAI** so CombatManager can reach the decks:

```csharp
// Public accessors for CombatManager wound resolution
public BehaviorDeck _behaviorDeckPublic => _behaviorDeck;
public WoundDeck    _woundDeckPublic    => _woundDeck;

public void AddCriticalWoundTag(string tag) => _criticalWoundTags.Add(tag);
```

---

## Part 13: Update CombatManager.RunMonsterPhase

Replace the old `RunMonsterPhase` to use the new result fields:

```csharp
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
        OnDamageDealt?.Invoke(hit.hunterId, hit.damage, "flesh");
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
```

---

## Part 14: Author Gaunt Standard Content as SO Assets

Create these assets in the Unity Editor. For each `BehaviorCardSO`, use the menu **MnM/Cards/BehaviorCard**. For `WoundLocationSO`, use **MnM/Cards/WoundLocation**.

### Folder Structure
```
Assets/_Game/Data/Monsters/Gaunt/
  BehaviorCards/
    Base/
      Gaunt_CreepingAdvance.asset
      Gaunt_GauntSlash.asset
      Gaunt_BoneRattle.asset
      Gaunt_Brace.asset
      Gaunt_ScrabbleSurge.asset
      Gaunt_ScentLock.asset
    Advanced/
      Gaunt_SpearThrust.asset
      Gaunt_BoneLance.asset
  WoundLocations/
    Standard/
      Gaunt_GauntJaw.asset
      Gaunt_GauntClaw.asset
      Gaunt_SpikedTail.asset
      Gaunt_BonyShoulder.asset
      Gaunt_SpineTrap.asset
      Gaunt_RibCage.asset
    Hardened/
      (copies of Standard + 2 additional)
  MonsterSO/
    Monster_GauntStandard.asset
    Monster_GauntHardened.asset
```

### Base Card Pool (6 cards)

| Asset | cardName | cardType | hasTarget | hasMove | hasDamage | targetRule | forcedBodyPart | Notes |
|---|---|---|---|---|---|---|---|---|
| Gaunt_CreepingAdvance | Creeping Advance | Removable | false | true | false | — | — | |
| Gaunt_GauntSlash | Gaunt Slash | Removable | true | false | true | nearest | — | criticalWoundCondition: "GauntJaw_Critical" |
| Gaunt_BoneRattle | Bone Rattle | Mood | false | false | false | — | — | removalCondition: "Hunter inflicts a wound" |
| Gaunt_Brace | Brace | Removable | false | false | false | — | — | No sub-phases (reaction card) |
| Gaunt_ScrabbleSurge | Scrabble Surge | Removable | true | true | true | nearest | — | Move then damage |
| Gaunt_ScentLock | Scent Lock | Removable | true | false | false | — | — | Target ident only — shifts aggro |

### Advanced Card Pool (2 cards)

| Asset | cardName | cardType | hasTarget | hasMove | hasDamage | targetRule | forcedBodyPart | Notes |
|---|---|---|---|---|---|---|---|---|
| Gaunt_SpearThrust | Spear Thrust | SingleTrigger | true | false | true | nearest | Torso | Fires once, then permanent removal |
| Gaunt_BoneLance | Bone Lance | Removable | true | true | true | mostInjured | — | |

### Gaunt Standard Deck Composition (index 0)
- baseCardCount: 4
- advancedCardCount: 1
- overwhelmingCardCount: 0
- Total health pool: 5

### Gaunt Hardened Deck Composition (index 1)
- baseCardCount: 5
- advancedCardCount: 2
- overwhelmingCardCount: 0
- Total health pool: 7

### Standard Wound Location Deck (6 cards)

| Asset | locationName | partTag | woundTarget | isTrap | isImpervious | criticalWoundTag |
|---|---|---|---|---|---|---|
| Gaunt_GauntJaw | Gaunt Jaw | Head | 6 | false | false | GauntJaw_Critical |
| Gaunt_GauntClaw | Gaunt Claw | Arms | 5 | false | false | — |
| Gaunt_SpikedTail | Spiked Tail | Tail | 7 | false | false | — |
| Gaunt_BonyShoulder | Bony Shoulder | Torso | 5 | true | — | — |
| Gaunt_SpineTrap | Spine Trap | Back | 0 | true | — | — |
| Gaunt_RibCage | Rib Cage | Torso | 4 | false | false | — |

**Spine Trap** trapEffect: `"The Gaunt's spine barb catches the hunter — take 1 flesh damage before the attack resolves"`

**Bony Shoulder** isImpervious: true, woundEffect: `"Cracked but not broken — the shoulder resists"`

**Gaunt Slash** alternate behavior when `GauntJaw_Critical`:
- alternateTriggerCondition: `"Draws back, jaw hanging"`
- alternateEffectDescription: `"The Gaunt recoils from its wounded jaw — cries out, no damage this turn"`

---

## Verification Test: Aldric vs Gaunt Standard — Round 1

**Setup:** Gaunt Standard deck = 4 base cards drawn from 6-card pool + 1 advanced card

**Hunter Phase — Aldric attacks:**
- [ ] To-hit roll fires (d10 logged)
- [ ] On hit: `[Wound] Drew: [location name]` logged
- [ ] Force roll logged: `d10=[N] + STR=3 = [total] vs target [X]`
- [ ] FAILURE: `failureEffect` logged; no behavior card removed; health pool unchanged
- [ ] WOUND: behavior card removed; `[BehaviorDeck] '[cardName]' permanently removed. Health pool: deck=[N]...`
- [ ] CRITICAL: same as wound + critical tag set if location has one
- [ ] TRAP: `[Wound] TRAP: [effect]` logged; discard reshuffled immediately; no card removed
- [ ] IMPERVIOUS + WOUND: wound effect logged; resources noted; no card removed

**Monster Phase:**
- [ ] `[MonsterAI] ExecuteCard: [cardName] | Target:X Move:X Damage:X` logged
- [ ] `[MonsterAI] GRIT WINDOW: [phase]` logged before each sub-phase
- [ ] Card with `hasMovement` → monster position changes; GridManager updated
- [ ] Card with `hasDamage` → `[MonsterAI] Applied 1 flesh to [hunter] [zone]`
- [ ] `OnDamageDealt` fires; UI bar updates
- [ ] Removable card → `[BehaviorDeck] '[cardName]' → discard. Health: [N]`
- [ ] Mood card → `[BehaviorDeck] '[cardName]' → mood in play. Health: [N]`
- [ ] SingleTrigger → `[BehaviorDeck] '[cardName]' → permanently removed`
- [ ] `[MonsterAI] *** MONSTER DEFEATED ***` fires when HealthPool reaches 0
- [ ] Victory modal appears on defeat

**Critical wound alternate behavior:**
- [ ] Land a critical on Gaunt Jaw (`GauntJaw_Critical` tag set)
- [ ] Next time `Gaunt Slash` is drawn: `'Gaunt Slash' using ALTERNATE behavior` logged; no damage applied

---

## Definition of Done — Stage 8-N

- [ ] `BehaviorDeck.cs` and `WoundDeck.cs` compile; all methods present
- [ ] `MonsterAI.InitializeDeck` uses `deckCompositions` and pool arrays — no `BehaviorGroup` references remain
- [ ] `MonsterAI.ExecuteCard` uses sub-phase booleans (`hasTargetIdentification`, `hasMovement`, `hasDamage`) — no `movementPattern` or `attackTargetType` references remain
- [ ] Grit window event fires 6 times per monster turn (all phases)
- [ ] `CombatManager.ResolveWound` implements full wound flow: trap → failure → wound → critical; impervious handled; defeat checked
- [ ] `BehaviorCardResult` has `monsterDefeated` and `pendingDamageZone` fields
- [ ] `CombatState.MonsterCombatState` has `behaviorDeckCount`, `behaviorDiscardCount`, `moodCardsInPlayCount`, `criticalWoundTags`
- [ ] Gaunt Standard: 6 base cards, 2 advanced cards, Standard deck composition (4+1=5 health), 6 wound locations authored as SO assets
- [ ] Full verification pass (Aldric vs Gaunt Standard) passes all checklist items
- [ ] Zero compile errors
