<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 2-D | CombatManager Phase Machine & DiceResolver
Status: Stage 2-C complete. GridManager passes all tests.
Test script deleted.
Task: Implement CombatManager (phase state machine only —
no card resolution yet) and DiceResolver with mandatory
Debug.Log output for all d10 math.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_02/STAGE_02_D.md
- Assets/_Game/Scripts/Core.Systems/ICombatManager.cs
- Assets/_Game/Scripts/Core.Systems/IGridManager.cs
- Assets/_Game/Scripts/Core.Systems/IMonsterAI.cs
- Assets/_Game/Scripts/Core.Data/CombatState.cs
- Assets/_Game/Scripts/Core.Data/Enums.cs

Then confirm:
- What files you will create
- That DiceResolver will have Debug.Log on every roll
- What you will NOT implement yet (card resolution,
  MonsterAI, collapse detection — those are Stage 3)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 2-D: CombatManager Phase Machine & DiceResolver

**Resuming from:** Stage 2-C complete — GridManager verified  
**Done when:** Phase machine cycles all 4 phases in correct order with Debug.Log at each step; DiceResolver logs correct d10 math  
**Commit:** `"2D: CombatManager phase machine, DiceResolver with Debug.Log verified"`  
**Next session:** STAGE_02_E.md  

---

## What This Session Does NOT Include

- Card resolution logic (Stage 3)
- MonsterAI implementation (Stage 3)
- Collapse detection (Stage 3)
- `TryPlayCard` and `TryMoveHunter` full implementations — stub them to return `false` and log a warning

---

## Step 1: DiceResolver.cs

**Path:** `Assets/_Game/Scripts/Core.Logic/DiceResolver.cs`

> ⚑ Every roll must produce a Debug.Log. This is a hard rule from `claude.md`.

```csharp
using UnityEngine;
using MnM.Core.Data;

namespace MnM.Core.Logic
{
    public static class DiceResolver
    {
        // ── Precision Check ──────────────────────────────────────
        // d10 + attackerAccuracy vs targetEvasion
        public static PrecisionResult ResolvePrecision(
            int attackerAccuracy,
            int targetEvasion,
            int luckModifier,
            bool hasElementWeakness,
            bool hasElementResistance)
        {
            int roll    = Random.Range(1, 11);   // d10: 1–10 inclusive
            int critThreshold = 10 - luckModifier; // Luck 1 = crit on 9+

            int effectiveRoll = roll;

            // Bonus die on weakness: roll again, take higher
            if (hasElementWeakness)
            {
                int bonusRoll = Random.Range(1, 11);
                int prev = effectiveRoll;
                effectiveRoll = Mathf.Max(effectiveRoll, bonusRoll);
                Debug.Log($"[d10] Element weakness bonus die: {bonusRoll} (kept {effectiveRoll}, discarded {prev})");
            }

            // Penalty die on resistance: roll again, take lower
            if (hasElementResistance)
            {
                int penaltyRoll = Random.Range(1, 11);
                int prev = effectiveRoll;
                effectiveRoll = Mathf.Min(effectiveRoll, penaltyRoll);
                Debug.Log($"[d10] Element resistance penalty die: {penaltyRoll} (kept {effectiveRoll}, discarded {prev})");
            }

            int  total  = effectiveRoll + attackerAccuracy;
            bool isHit  = total >= targetEvasion;
            bool isCrit = effectiveRoll >= critThreshold;

            Debug.Log($"[d10 Precision] Roll:{roll} effective:{effectiveRoll} +Acc:{attackerAccuracy} " +
                      $"= {total} vs Evasion:{targetEvasion} | " +
                      $"CritThreshold:{critThreshold} | " +
                      $"Result:{(isCrit ? "CRIT" : isHit ? "HIT" : "MISS")}");

            return new PrecisionResult
            {
                isHit      = isHit,
                isCritical = isCrit,
                rawRoll    = roll,
                total      = total,
            };
        }

        // ── Force Check ──────────────────────────────────────────
        // d10 + attackerStrength vs targetToughness
        public static ForceResult ResolveForce(
            int attackerStrength,
            int targetToughness,
            bool targetExposed,
            bool targetShellIsZero)
        {
            // Exposed part: Force Check auto-passes — no roll needed
            if (targetExposed)
            {
                Debug.Log($"[d10 Force] AUTO-PASS — part is Exposed. Result: WOUND (Flesh)");
                return new ForceResult { isWound = true, rawRoll = 0, total = 0, wasAutoPass = true };
            }

            int  roll  = Random.Range(1, 11);
            int  total = roll + attackerStrength;
            bool isWound = total > targetToughness;

            Debug.Log($"[d10 Force] Roll:{roll} +Str:{attackerStrength} = {total} " +
                      $"vs Toughness:{targetToughness} | " +
                      $"Result:{(isWound ? "WOUND (Flesh)" : "SHELL HIT")}");

            return new ForceResult { isWound = isWound, rawRoll = roll, total = total, wasAutoPass = false };
        }
    }

    public struct PrecisionResult
    {
        public bool isHit;
        public bool isCritical;
        public int  rawRoll;
        public int  total;
    }

    public struct ForceResult
    {
        public bool isWound;
        public int  rawRoll;
        public int  total;
        public bool wasAutoPass;
    }
}
```

---

## Step 2: CombatManager.cs

**Path:** `Assets/_Game/Scripts/Core.Systems/CombatManager.cs`

This session: phase machine + Vitality card draw only. `TryPlayCard`, `TryMoveHunter`, and `IsCombatOver` are stubs — implemented fully in Stage 3.

```csharp
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
        public CombatState  CurrentState { get; private set; }
        public CombatPhase  CurrentPhase { get; private set; }

        // ── Events ───────────────────────────────────────────────
        public event System.Action<CombatPhase>          OnPhaseChanged;
        public event System.Action<string, int, DamageType> OnDamageDealt;
        public event System.Action<string>               OnEntityCollapsed;
        public event System.Action<CombatResult>         OnCombatEnded;

        // ── Lifecycle ────────────────────────────────────────────
        public void StartCombat(CombatState initialState)
        {
            CurrentState = initialState;
            CurrentPhase = CombatPhase.VitalityPhase;
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
                        Debug.Log($"[Combat] HunterPhase — waiting for remaining hunters");
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
            Debug.LogWarning($"[Combat] TryPlayCard stub — implement in Stage 3");
            return false;
        }

        public bool TryMoveHunter(string hunterId, Vector2Int destination)
        {
            Debug.LogWarning($"[Combat] TryMoveHunter stub — implement in Stage 3");
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
```

---

## Verification Test

Temporary test script — delete after passing:

**Path:** `Assets/_Game/Scripts/Core.Systems/CombatManagerTest.cs` *(delete after)*

```csharp
using UnityEngine;
using MnM.Core.Data;
using MnM.Core.Systems;

public class CombatManagerTest : MonoBehaviour
{
    private void Start()
    {
        // Setup
        var go      = new GameObject("CombatManager");
        var combat  = go.AddComponent<CombatManager>();
        var state   = CombatStateFactory.BuildMockCombatState();
        combat.StartCombat(state);

        // Test phase cycle — one full round
        Debug.Assert(combat.CurrentPhase == CombatPhase.VitalityPhase,
            "FAIL: should start in VitalityPhase");

        combat.AdvancePhase(); // → HunterPhase
        Debug.Assert(combat.CurrentPhase == CombatPhase.HunterPhase,
            "FAIL: should be HunterPhase");
        Debug.Assert(combat.CurrentState.hunters[0].handCardNames.Length == 2,
            "FAIL: Aldric should have 2 cards in hand after Vitality");

        // End hunter turn → triggers BehaviorRefresh (only 1 hunter in mock)
        combat.EndHunterTurn("hunter_aldric");
        Debug.Assert(combat.CurrentPhase == CombatPhase.BehaviorRefresh,
            "FAIL: should be BehaviorRefresh after all hunters acted");

        combat.AdvancePhase(); // → MonsterPhase
        Debug.Assert(combat.CurrentPhase == CombatPhase.MonsterPhase,
            "FAIL: should be MonsterPhase");

        combat.AdvancePhase(); // → VitalityPhase round 2
        Debug.Assert(combat.CurrentPhase == CombatPhase.VitalityPhase,
            "FAIL: should loop back to VitalityPhase");
        Debug.Assert(combat.CurrentState.currentRound == 1,
            "FAIL: currentRound should be 1 after first round completes");

        // d10 test
        var precision = MnM.Core.Logic.DiceResolver.ResolvePrecision(
            attackerAccuracy: 0, targetEvasion: 2, luckModifier: 0,
            hasElementWeakness: false, hasElementResistance: false);
        Debug.Log($"[CombatManagerTest] Precision result — Hit:{precision.isHit} Crit:{precision.isCritical}");

        var force = MnM.Core.Logic.DiceResolver.ResolveForce(
            attackerStrength: 0, targetToughness: 1,
            targetExposed: false, targetShellIsZero: false);
        Debug.Log($"[CombatManagerTest] Force result — Wound:{force.isWound}");

        Debug.Log("[CombatManagerTest] ✓ All phase cycle assertions passed");
        Destroy(go);
    }
}
```

Attach to a GameObject, Play, verify all assertions pass and d10 logs appear, **delete the test script**.

---

## Next Session

**File:** `_Docs/Stage_02/STAGE_02_E.md`  
**Covers:** AggroManager, StatusEffectResolver, and the full Round 1 console test (Aldric vs Gaunt Standard)
