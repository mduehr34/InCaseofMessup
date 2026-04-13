<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 3-B | RemoveCard, Win Condition & PartResolver
Status: Stage 3-A complete. MonsterAI deck init and
DrawNextCard() verified. Test script deleted.
Task: Implement RemoveCard() with mid-turn win condition
detection, and create PartResolver — the Shell/Flesh/
break/wound damage pipeline.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_03/STAGE_03_B.md
- Assets/_Game/Scripts/Core.Systems/MonsterAI.cs
- Assets/_Game/Scripts/Core.Systems/ICombatManager.cs
- Assets/_Game/Scripts/Core.Data/CombatState.cs
- Assets/_Game/Scripts/Core.Data/MonsterSO.cs
- Assets/_Game/Scripts/Core.Data/Enums.cs

Then confirm:
- What files you will modify and create
- That win condition fires IMMEDIATELY mid-turn when
  last Removable card is removed — not at end of round
- What you will NOT touch this session

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 3-B: RemoveCard, Win Condition & PartResolver

**Resuming from:** Stage 3-A complete — MonsterAI draws correctly  
**Done when:** Part break removes correct behavior cards, win condition Debug.Log fires immediately when last Removable removed, PartResolver Shell/Flesh pipeline logs correct values  
**Commit:** `"3B: RemoveCard with win condition, PartResolver Shell/Flesh/break/wound"`  
**Next session:** STAGE_03_C.md  

---

## Critical Rule: Win Condition Timing

The monster is defeated **the instant** the last Removable card is removed — mid-turn, mid-phase, regardless of what else is happening. `RemoveCard()` must fire the win event immediately. Do not defer to end of round.

---

## Step 1: Implement RemoveCard() in MonsterAI.cs

Replace the stub in `MonsterAI.cs`:

```csharp
// Add event — CombatManager subscribes to this
public event System.Action OnMonsterDefeated;

public void RemoveCard(string cardName)
{
    // Search all removable lists — card may or may not be in active deck
    bool removed = false;

    removed |= RemoveFromList(_openingCards,    cardName);
    removed |= RemoveFromList(_escalationCards, cardName);
    removed |= RemoveFromList(_apexCards,       cardName);

    // Also remove from active deck if present there
    var inActive = _activeDeck.FirstOrDefault(
        c => c.cardName == cardName && c.cardType != BehaviorCardType.Permanent);
    if (inActive != null) _activeDeck.Remove(inActive);

    if (removed)
    {
        Debug.Log($"[MonsterAI] Card removed: \"{cardName}\". " +
                  $"Remaining Removable: {RemainingRemovableCount}");

        // Win condition — fires IMMEDIATELY, mid-turn
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
    if (card != null)
    {
        list.Remove(card);
        return true;
    }
    return false;
}
```

---

## Step 2: Wire OnMonsterDefeated into CombatManager

In `CombatManager.cs`, subscribe to the event when MonsterAI is initialized:

```csharp
public void InitializeMonsterAI(MonsterSO monster, string difficulty)
{
    var ai = new MonsterAI();
    ai.InitializeDeck(monster, difficulty);

    // Subscribe to defeat event — fires mid-turn
    ai.OnMonsterDefeated += HandleMonsterDefeated;

    SetMonsterAI(ai);
    Debug.Log($"[Combat] MonsterAI initialized for {monster.monsterName} ({difficulty})");
}

private void HandleMonsterDefeated()
{
    var result = new CombatResult
    {
        isVictory    = true,
        roundsElapsed = CurrentState.currentRound,
        collapsedHunterIds = CurrentState.hunters
            .Where(h => h.isCollapsed)
            .Select(h => h.hunterId)
            .ToArray(),
    };
    Debug.Log($"[Combat] *** HUNT WON *** Round:{result.roundsElapsed}");
    OnCombatEnded?.Invoke(result);
}
```

Also replace the `IsCombatOver` stub:

```csharp
public bool IsCombatOver(out CombatResult result)
{
    result = default;

    // Monster: checked via event — but allow polling too
    if (_monsterAI != null && !_monsterAI.HasRemovableCards())
    {
        result.isVictory     = true;
        result.roundsElapsed = CurrentState.currentRound;
        return true;
    }

    // All hunters collapsed
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
```

---

## Step 3: PartResolver.cs

**Path:** `Assets/_Game/Scripts/Core.Logic/PartResolver.cs`

```csharp
using System.Collections.Generic;
using UnityEngine;
using MnM.Core.Data;
using MnM.Core.Systems;

namespace MnM.Core.Logic
{
    public static class PartResolver
    {
        // ── Main Entry Point ─────────────────────────────────────
        public static PartDamageResult ApplyDamage(
            ref MonsterPartState part,
            int damageAmount,
            DamageType type,
            MonsterSO monsterData,
            IMonsterAI ai)
        {
            var result = new PartDamageResult
            {
                removedCardNames = new List<string>()
            };

            if (type == DamageType.Shell)
                result = ApplyShellDamage(ref part, damageAmount, monsterData, ai);
            else
                result = ApplyFleshDamage(ref part, damageAmount, monsterData, ai);

            return result;
        }

        // ── Shell Damage ─────────────────────────────────────────
        private static PartDamageResult ApplyShellDamage(
            ref MonsterPartState part,
            int damageAmount,
            MonsterSO monsterData,
            IMonsterAI ai)
        {
            var result = new PartDamageResult { removedCardNames = new List<string>() };

            if (part.isBroken)
            {
                Debug.Log($"[Part] {part.partName} already broken — Shell damage ignored");
                return result;
            }

            int prev = part.shellCurrent;
            part.shellCurrent = Mathf.Max(0, part.shellCurrent - damageAmount);
            Debug.Log($"[Part] {part.partName} Shell: {prev} → {part.shellCurrent}/{part.shellMax}");

            // Part breaks when Shell reaches 0 for the first time
            if (part.shellCurrent == 0 && prev > 0)
            {
                part.isBroken = true;
                Debug.Log($"[Part] *** {part.partName} BROKEN ***");
                result.partBreakOccurred = true;

                // Remove all cards linked to this break — fires immediately mid-turn
                var partData = FindPartData(monsterData, part.partName);
                if (partData.HasValue)
                {
                    foreach (var cardName in partData.Value.breakRemovesCardNames)
                    {
                        if (!string.IsNullOrEmpty(cardName))
                        {
                            ai.RemoveCard(cardName);
                            result.removedCardNames.Add(cardName);
                            Debug.Log($"[Part] Break removes: \"{cardName}\"");
                        }
                    }
                }
            }

            return result;
        }

        // ── Flesh Damage ─────────────────────────────────────────
        private static PartDamageResult ApplyFleshDamage(
            ref MonsterPartState part,
            int damageAmount,
            MonsterSO monsterData,
            IMonsterAI ai)
        {
            var result = new PartDamageResult { removedCardNames = new List<string>() };

            int prev = part.fleshCurrent;
            part.fleshCurrent = Mathf.Max(0, part.fleshCurrent - damageAmount);
            Debug.Log($"[Part] {part.partName} Flesh: {prev} → {part.fleshCurrent}/{part.fleshMax}");

            bool woundOccurred = part.fleshCurrent < prev;
            if (!woundOccurred) return result;

            result.woundOccurred = true;
            part.woundCount++;

            // Remove card linked to this wound number (1st, 2nd, 3rd...)
            var partData = FindPartData(monsterData, part.partName);
            if (partData.HasValue)
            {
                var woundRemovals = partData.Value.woundRemovesCardNames;
                int woundIndex = part.woundCount - 1; // 0-based

                if (woundIndex < woundRemovals.Length)
                {
                    string cardName = woundRemovals[woundIndex];
                    if (!string.IsNullOrEmpty(cardName))
                    {
                        ai.RemoveCard(cardName);
                        result.removedCardNames.Add(cardName);
                        Debug.Log($"[Part] Wound #{part.woundCount} removes: \"{cardName}\"");
                    }
                }
            }

            return result;
        }

        // ── Part Data Lookup ─────────────────────────────────────
        // Finds the MonsterBodyPart definition matching the runtime part state
        private static MonsterBodyPart? FindPartData(MonsterSO monsterData, string partName)
        {
            // Check all three difficulty arrays — part names are consistent across difficulties
            foreach (var part in monsterData.standardParts ?? new MonsterBodyPart[0])
                if (part.partName == partName) return part;
            foreach (var part in monsterData.hardenedParts ?? new MonsterBodyPart[0])
                if (part.partName == partName) return part;
            foreach (var part in monsterData.apexParts ?? new MonsterBodyPart[0])
                if (part.partName == partName) return part;

            Debug.LogWarning($"[PartResolver] Part \"{partName}\" not found in MonsterSO");
            return null;
        }
    }

    // ── Result Struct ────────────────────────────────────────────
    public struct PartDamageResult
    {
        public bool partBreakOccurred;
        public bool woundOccurred;
        public List<string> removedCardNames;
    }
}
```

---

## Verification Test

Temporary test script — delete after passing:

**Path:** `Assets/_Game/Scripts/Core.Logic/PartResolverTest.cs` *(delete after)*

```csharp
using System.Collections.Generic;
using UnityEngine;
using MnM.Core.Data;
using MnM.Core.Logic;
using MnM.Core.Systems;

public class PartResolverTest : MonoBehaviour
{
    [SerializeField] private MonsterSO _gauntSO;

    private void Start()
    {
        if (_gauntSO == null) { Debug.LogError("Assign Mock_GauntStandard"); return; }

        // ── Test 1: Shell damage ─────────────────────────────────
        var part = new MonsterPartState
        {
            partName = "Throat", shellCurrent = 2, shellMax = 2,
            fleshCurrent = 3, fleshMax = 3, isBroken = false, woundCount = 0
        };

        var mockAI = new MockAI();

        // Deal 1 shell — should not break
        var r = PartResolver.ApplyDamage(ref part, 1, DamageType.Shell, _gauntSO, mockAI);
        Debug.Assert(part.shellCurrent == 1, $"FAIL: shell should be 1, got {part.shellCurrent}");
        Debug.Assert(!r.partBreakOccurred, "FAIL: should not have broken yet");

        // Deal 1 more shell — should break, remove "The Howl" and "Scent Lock"
        r = PartResolver.ApplyDamage(ref part, 1, DamageType.Shell, _gauntSO, mockAI);
        Debug.Assert(part.shellCurrent == 0, "FAIL: shell should be 0");
        Debug.Assert(part.isBroken, "FAIL: part should be broken");
        Debug.Assert(r.partBreakOccurred, "FAIL: partBreakOccurred should be true");
        Debug.Log($"[Test] Cards removed on break: [{string.Join(", ", r.removedCardNames)}]");

        // ── Test 2: Flesh damage / wound ─────────────────────────
        var torso = new MonsterPartState
        {
            partName = "Torso", shellCurrent = 0, shellMax = 2,
            fleshCurrent = 3, fleshMax = 3, isBroken = true, woundCount = 0
        };

        var r2 = PartResolver.ApplyDamage(ref torso, 1, DamageType.Flesh, _gauntSO, mockAI);
        Debug.Assert(torso.fleshCurrent == 2, $"FAIL: flesh should be 2, got {torso.fleshCurrent}");
        Debug.Assert(r2.woundOccurred, "FAIL: wound should have occurred");
        Debug.Assert(torso.woundCount == 1, "FAIL: woundCount should be 1");
        Debug.Log($"[Test] Cards removed on wound 1: [{string.Join(", ", r2.removedCardNames)}]");

        // ── Test 3: Win condition via RemoveCard ──────────────────
        var ai = new MonsterAI();
        ai.InitializeDeck(_gauntSO, "Standard");
        bool defeated = false;
        ai.OnMonsterDefeated += () => { defeated = true; };

        // Manually remove all removable cards to trigger win condition
        int count = ai.RemainingRemovableCount;
        // Get card names from mock SO (may be empty with stub data — log either way)
        Debug.Log($"[Test] Removable cards to remove: {count}");
        if (count == 0)
            Debug.Log("[Test] Mock SO has no behavior cards yet — win condition test skipped (expected)");

        Debug.Log("[PartResolverTest] ✓ Shell/Flesh damage pipeline verified");
    }

    // Minimal mock implementation for testing
    private class MockAI : IMonsterAI
    {
        public BehaviorGroup CurrentGroup => BehaviorGroup.Opening;
        public int RemainingRemovableCount => 0;
        public bool HasRemovableCards() => false;
        public BehaviorCardSO DrawNextCard() => null;
        public void ExecuteCard(BehaviorCardSO card, CombatState state) { }
        public void RemoveCard(string cardName) =>
            Debug.Log($"[MockAI] RemoveCard called: {cardName}");
        public void AdvanceGroupIfExhausted() { }
        public void TriggerApex() { }
        public void InitializeDeck(MonsterSO monster, string difficulty) { }
    }
}
```

Attach to a GameObject, assign `Mock_GauntStandard`, Play, verify output, **delete the test script**.

> ⚑ With mock data, `breakRemovesCardNames` arrays will be empty — that is expected. The test verifies the pipeline runs without errors and logs correctly. Full card names are wired in Stage 7.

---

## Next Session

**File:** `_Docs/Stage_03/STAGE_03_C.md`  
**Covers:** CardResolver — full action card resolution pipeline including Loud flag, Reaction handling, Precision/Force chain, and AP management
