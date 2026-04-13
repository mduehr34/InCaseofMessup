<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 3-C | CardResolver — Full Action Card Resolution
Status: Stage 3-B complete. RemoveCard() fires win condition
immediately. PartResolver Shell/Flesh pipeline verified.
Test script deleted.
Task: Create CardResolver — the full pipeline for resolving
a hunter playing an action card: Loud flag, Reaction handling,
Precision Check, Force Check, damage application, AP management.
Also implement TryPlayCard() in CombatManager.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_03/STAGE_03_C.md
- Assets/_Game/Scripts/Core.Logic/PartResolver.cs
- Assets/_Game/Scripts/Core.Logic/DiceResolver.cs
- Assets/_Game/Scripts/Core.Systems/CombatManager.cs
- Assets/_Game/Scripts/Core.Data/ActionCardSO.cs
- Assets/_Game/Scripts/Core.Data/CombatState.cs
- Assets/_Game/Scripts/Core.Data/MonsterSO.cs
- Assets/_Game/Scripts/Core.Data/Enums.cs

Then confirm:
- What files you will create and modify
- That Reaction cards skip Precision/Force entirely
- That AP refund is applied even on a miss for weak cards
- What you will NOT touch this session

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 3-C: CardResolver — Full Action Card Resolution

**Resuming from:** Stage 3-B complete — PartResolver and RemoveCard() verified  
**Done when:** A hunter playing a card runs the full Loud → Reaction → Precision → Force → Damage → AP pipeline with correct Debug.Log at each step  
**Commit:** `"3C: CardResolver full pipeline, TryPlayCard() implemented in CombatManager"`  
**Next session:** STAGE_03_D.md  

---

## Resolution Pipeline — Order Is Fixed

```
1. Loud check → log warning, set flag for MonsterAI trigger evaluation
2. Reaction check → if Reaction card, apply effect directly, skip 3–5
3. Precision Check (d10 + Accuracy vs Evasion)
   → Miss: apply AP refund if weak card, return early
   → Hit: continue
   → Crit: skip Force Check, go straight to Flesh
4. Shell or Flesh?
   → Shell still has durability AND not a crit: Force Check for Shell hit
   → Shell = 0 OR crit: Force Check for Flesh wound
5. Apply damage via PartResolver
6. Deduct AP (cost − refund), check if Apex trigger needed
```

---

## Step 1: CardResolver.cs

**Path:** `Assets/_Game/Scripts/Core.Logic/CardResolver.cs`

```csharp
using System.Collections.Generic;
using UnityEngine;
using MnM.Core.Data;
using MnM.Core.Systems;

namespace MnM.Core.Logic
{
    public static class CardResolver
    {
        // ── Main Resolution Entry Point ──────────────────────────
        public static CardResolutionResult Resolve(
            ActionCardSO card,
            HunterCombatState attacker,
            MonsterCombatState monster,
            ref MonsterPartState targetPart,
            MonsterSO monsterData,
            IMonsterAI ai,
            AggroManager aggro,
            bool firstPartBreakOccurredThisCombat)
        {
            var result = new CardResolutionResult
            {
                cardName         = card.cardName,
                removedCardNames = new List<string>(),
            };

            Debug.Log($"[Card] Resolving: \"{card.cardName}\" by {attacker.hunterName} " +
                      $"targeting {targetPart.partName} | AP:{attacker.apRemaining} " +
                      $"Cost:{card.apCost} Refund:{card.apRefund} Loud:{card.isLoud}");

            // ── Step 1: Loud Flag ────────────────────────────────
            if (card.isLoud)
            {
                result.wasLoud = true;
                Debug.Log($"[Card] LOUD card played — MonsterAI reactive triggers may fire");
                // MonsterAI checks this flag via EvaluateTrigger in Session 3-D
            }

            // ── Step 2: Reaction Cards ───────────────────────────
            if (card.category == CardCategory.Reaction)
            {
                Debug.Log($"[Card] REACTION: {card.cardName} — {card.effectDescription}");
                result.reactionApplied = true;
                // Reactions have no AP cost in most cases — refund handles it
                attacker.apRemaining -= (card.apCost - card.apRefund);
                return result;
            }

            // ── Step 3: Precision Check ──────────────────────────
            // Get attacker's effective accuracy (status effects applied in StatusEffectResolver)
            int effectiveAccuracy = attacker.accuracy;
            int effectiveMovement = attacker.movement; // Not used here but available
            StatusEffectResolver.ApplyStatusPenalties(
                attacker, ref effectiveAccuracy, ref effectiveMovement);

            // Element check
            bool hasWeakness   = HasElementMatch(card, monsterData.weaknesses);
            bool hasResistance = HasElementMatch(card, monsterData.resistances);

            var precision = DiceResolver.ResolvePrecision(
                effectiveAccuracy, GetMonsterEvasion(monster, monsterData),
                attacker.luck, hasWeakness, hasResistance);

            // Tick status effects after action
            StatusEffectResolver.TickAfterAction(ref attacker.activeStatusEffects, attacker);

            if (!precision.isHit)
            {
                Debug.Log($"[Card] MISS — {card.cardName}");
                // AP refund applies on miss for weak cards
                attacker.apRemaining -= (card.apCost - card.apRefund);
                result.apRefundGranted = card.apRefund;
                return result;
            }

            // ── Step 4: Shell or Flesh? ──────────────────────────
            bool shellDepleted = targetPart.shellCurrent == 0;
            bool goesToFlesh   = shellDepleted || precision.isCritical;

            PartDamageResult partResult;

            if (goesToFlesh)
            {
                // Force Check for Flesh wound
                var force = DiceResolver.ResolveForce(
                    attacker.strength,
                    GetMonsterToughness(monster, monsterData),
                    targetPart.isExposed,
                    shellDepleted);

                if (force.isWound)
                {
                    int fleshDamage = CalculateFleshDamage(card, attacker, precision.isCritical);
                    partResult = PartResolver.ApplyDamage(
                        ref targetPart, fleshDamage, DamageType.Flesh, monsterData, ai);
                    result.damageDealt = fleshDamage;
                    result.damageType  = DamageType.Flesh;
                    result.removedCardNames.AddRange(partResult.removedCardNames);

                    Debug.Log($"[Card] FLESH WOUND — {fleshDamage} Flesh to {targetPart.partName}");
                }
                else
                {
                    Debug.Log($"[Card] Force Check failed — no flesh damage");
                }
            }
            else
            {
                // Shell hit
                int shellDamage = CalculateShellDamage(card, attacker);
                partResult = PartResolver.ApplyDamage(
                    ref targetPart, shellDamage, DamageType.Shell, monsterData, ai);
                result.damageDealt = shellDamage;
                result.damageType  = DamageType.Shell;
                result.removedCardNames.AddRange(partResult.removedCardNames);

                // Check Apex trigger on first part break this combat
                if (partResult.partBreakOccurred && !firstPartBreakOccurredThisCombat)
                {
                    result.apexShouldTrigger = true;
                    Debug.Log($"[Card] First part break — Apex trigger flagged");
                }

                Debug.Log($"[Card] SHELL HIT — {shellDamage} Shell to {targetPart.partName}");
            }

            // ── Step 5: AP Management ────────────────────────────
            result.apRefundGranted   = card.apRefund;
            attacker.apRemaining    -= (card.apCost - card.apRefund);

            Debug.Log($"[Card] Resolution complete. Damage:{result.damageDealt} {result.damageType} " +
                      $"AP remaining:{attacker.apRemaining} " +
                      $"Cards removed:[{string.Join(", ", result.removedCardNames)}]");

            return result;
        }

        // ── Damage Calculation ───────────────────────────────────
        // Base shell damage = 1 (weapon type modifiers applied via WeaponSO in Stage 7)
        private static int CalculateShellDamage(ActionCardSO card, HunterCombatState attacker)
        {
            int base_ = 1;
            // Strength modifier affects Flesh only — Shell is always 1 base
            // Weapon-specific passives (e.g. Axe = Shell hits count as 2) handled in Stage 7
            return base_;
        }

        // Base flesh damage = 1; crits and card effects may modify this
        private static int CalculateFleshDamage(
            ActionCardSO card, HunterCombatState attacker, bool isCrit)
        {
            int base_ = 1;
            if (isCrit)
            {
                base_ += 1; // Crits deal +1 Flesh base
                Debug.Log("[Card] Critical hit — +1 Flesh damage");
            }
            return base_;
        }

        // ── Helpers ──────────────────────────────────────────────
        private static bool HasElementMatch(ActionCardSO card, ElementTag[] monsterElements)
        {
            if (monsterElements == null) return false;
            // Action cards don't store element directly — weapon element used
            // For now return false; weapon element matching wired in Stage 7
            return false;
        }

        private static int GetMonsterEvasion(MonsterCombatState monster, MonsterSO monsterData)
        {
            int diffIndex = monster.difficulty switch
            {
                "Hardened" => 1,
                "Apex"     => 2,
                _          => 0,
            };
            if (monsterData.statBlocks == null || diffIndex >= monsterData.statBlocks.Length)
                return 2; // Default fallback
            return monsterData.statBlocks[diffIndex].evasion;
        }

        private static int GetMonsterToughness(MonsterCombatState monster, MonsterSO monsterData)
        {
            int diffIndex = monster.difficulty switch
            {
                "Hardened" => 1,
                "Apex"     => 2,
                _          => 0,
            };
            if (monsterData.statBlocks == null || diffIndex >= monsterData.statBlocks.Length)
                return 1; // Default fallback
            return monsterData.statBlocks[diffIndex].toughness;
        }
    }

    // ── Result ───────────────────────────────────────────────────
    public struct CardResolutionResult
    {
        public string cardName;
        public int damageDealt;
        public DamageType damageType;
        public int apRefundGranted;
        public bool reactionApplied;
        public bool wasLoud;
        public bool apexShouldTrigger;
        public List<string> removedCardNames;
    }
}
```

---

## Step 2: Implement TryPlayCard() in CombatManager

Replace the stub in `CombatManager.cs`:

```csharp
// Add field
private bool _firstPartBreakOccurred = false;

public bool TryPlayCard(string hunterId, string cardName, Vector2Int targetCell)
{
    var hunter = GetHunter(hunterId);
    if (hunter == null)
    {
        Debug.LogWarning($"[Combat] TryPlayCard: hunter {hunterId} not found");
        return false;
    }

    // Check card is in hand
    if (!System.Array.Contains(hunter.handCardNames, cardName))
    {
        Debug.LogWarning($"[Combat] TryPlayCard: \"{cardName}\" not in {hunter.hunterName}'s hand");
        return false;
    }

    // Load card SO — via Resources (Stage 5 will use a proper registry)
    var card = Resources.Load<ActionCardSO>($"Data/Cards/Action/{cardName}");
    if (card == null)
    {
        Debug.LogWarning($"[Combat] TryPlayCard: ActionCardSO not found for \"{cardName}\"");
        return false;
    }

    // AP check
    int netCost = card.apCost - card.apRefund;
    if (hunter.apRemaining < netCost && card.category != CardCategory.Reaction)
    {
        Debug.LogWarning($"[Combat] TryPlayCard: insufficient AP. " +
                         $"Have:{hunter.apRemaining} Need:{netCost}");
        return false;
    }

    // Find target part at targetCell
    var targetPartIndex = FindMonsterPartAtCell(targetCell);
    if (targetPartIndex < 0 && card.category != CardCategory.Reaction)
    {
        Debug.LogWarning($"[Combat] TryPlayCard: no monster part at ({targetCell.x},{targetCell.y})");
        return false;
    }

    // Resolve
    if (targetPartIndex >= 0)
    {
        var targetPart = CurrentState.monster.parts[targetPartIndex];
        var result = CardResolver.Resolve(
            card, hunter, CurrentState.monster,
            ref targetPart, GetMonsterSO(),
            _monsterAI, _aggroManager,
            _firstPartBreakOccurred);

        // Write back modified part
        CurrentState.monster.parts[targetPartIndex] = targetPart;

        // Handle Apex trigger
        if (result.apexShouldTrigger && !_firstPartBreakOccurred)
        {
            _firstPartBreakOccurred = true;
            _monsterAI?.TriggerApex();
        }

        // Fire damage event for UI
        if (result.damageDealt > 0)
            OnDamageDealt?.Invoke(CurrentState.monster.monsterName,
                result.damageDealt, result.damageType);
    }

    // Remove card from hand, add to discard
    RemoveCardFromHand(hunter, cardName);

    return true;
}

private int FindMonsterPartAtCell(Vector2Int cell)
{
    // For now: return index 0 if target cell is within monster footprint
    // Stage 5 UI will map cells to specific parts properly
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
    Resources.Load<MonsterSO>($"Data/Monsters/{CurrentState.monster.monsterName.Replace(" ", "")}");
```

---

## Verification Test

Temporary test script — delete after passing:

**Path:** `Assets/_Game/Scripts/Core.Logic/CardResolverTest.cs` *(delete after)*

```csharp
using System.Collections.Generic;
using UnityEngine;
using MnM.Core.Data;
using MnM.Core.Logic;
using MnM.Core.Systems;

public class CardResolverTest : MonoBehaviour
{
    [SerializeField] private MonsterSO _gauntSO;

    private void Start()
    {
        if (_gauntSO == null) { Debug.LogError("Assign Mock_GauntStandard"); return; }

        // Build mock attacker
        var aldric = new HunterCombatState
        {
            hunterId = "hunter_aldric", hunterName = "Aldric",
            accuracy = 0, strength = 0, luck = 0, movement = 3,
            apRemaining = 2, currentGrit = 3,
            activeStatusEffects = new string[0],
            bodyZones = new[]
            {
                new BodyZoneState { zone = "Head",  shellCurrent=2, shellMax=2, fleshCurrent=3, fleshMax=3 },
                new BodyZoneState { zone = "Torso", shellCurrent=2, shellMax=2, fleshCurrent=3, fleshMax=3 },
            }
        };

        // Build mock monster state
        var monster = new MonsterCombatState
        {
            monsterName = "The Gaunt", difficulty = "Standard",
            activeStatusEffects = new string[0]
        };

        // Build target part (Throat, Shell=2)
        var part = new MonsterPartState
        {
            partName = "Throat", shellCurrent = 2, shellMax = 2,
            fleshCurrent = 3, fleshMax = 3, isBroken = false, woundCount = 0
        };

        var mockAI    = new MockAI();
        var aggro     = new AggroManager();

        // Build mock card (BasicAttack, not Loud)
        var mockCard  = ScriptableObject.CreateInstance<ActionCardSO>();
        mockCard.cardName   = "Test Strike";
        mockCard.category   = CardCategory.BasicAttack;
        mockCard.apCost     = 1;
        mockCard.apRefund   = 0;
        mockCard.isLoud     = false;
        mockCard.isReaction = false;

        // Test 1: Normal attack — Precision Check should fire and log
        Debug.Log("=== Test 1: Normal Attack ===");
        var result = CardResolver.Resolve(
            mockCard, aldric, monster, ref part, _gauntSO, mockAI, aggro, false);
        Debug.Log($"[Test] Result — Hit or Miss logged above. AP remaining: {aldric.apRemaining}");
        Debug.Assert(aldric.apRemaining == 1, $"FAIL: AP should be 1, got {aldric.apRemaining}");

        // Test 2: Reaction card — should skip Precision/Force
        Debug.Log("=== Test 2: Reaction Card ===");
        aldric.apRemaining = 2;
        var reactionCard = ScriptableObject.CreateInstance<ActionCardSO>();
        reactionCard.cardName   = "Brace";
        reactionCard.category   = CardCategory.Reaction;
        reactionCard.apCost     = 0;
        reactionCard.apRefund   = 0;
        reactionCard.isReaction = true;

        var r2 = CardResolver.Resolve(
            reactionCard, aldric, monster, ref part, _gauntSO, mockAI, aggro, false);
        Debug.Assert(r2.reactionApplied, "FAIL: reactionApplied should be true");
        Debug.Log($"[Test] Reaction applied: {r2.reactionApplied}");

        // Test 3: Loud card — should log LOUD warning
        Debug.Log("=== Test 3: Loud Card ===");
        aldric.apRemaining = 2;
        var loudCard = ScriptableObject.CreateInstance<ActionCardSO>();
        loudCard.cardName   = "Thundering Blow";
        loudCard.category   = CardCategory.BasicAttack;
        loudCard.apCost     = 1;
        loudCard.apRefund   = 0;
        loudCard.isLoud     = true;

        var r3 = CardResolver.Resolve(
            loudCard, aldric, monster, ref part, _gauntSO, mockAI, aggro, false);
        Debug.Assert(r3.wasLoud, "FAIL: wasLoud should be true");

        Debug.Log("[CardResolverTest] ✓ Card resolution pipeline verified");
        Destroy(this);
    }

    private class MockAI : IMonsterAI
    {
        public event System.Action OnMonsterDefeated;
        public BehaviorGroup CurrentGroup => BehaviorGroup.Opening;
        public int RemainingRemovableCount => 3;
        public bool HasRemovableCards() => true;
        public BehaviorCardSO DrawNextCard() => null;
        public void ExecuteCard(BehaviorCardSO card, CombatState state) { }
        public void RemoveCard(string cardName) =>
            Debug.Log($"[MockAI] RemoveCard: {cardName}");
        public void AdvanceGroupIfExhausted() { }
        public void TriggerApex() { }
        public void InitializeDeck(MonsterSO monster, string difficulty) { }
    }
}
```

Attach to a GameObject, assign `Mock_GauntStandard`, Play, verify all 3 tests log correctly, **delete the test script**.

---

## Next Session

**File:** `_Docs/Stage_03/STAGE_03_D.md`  
**Covers:** ComboTracker, hunter collapse detection, TryMoveHunter, and the full Stage 3 console fight test
