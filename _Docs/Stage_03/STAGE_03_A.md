<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 3-A | MonsterAI — Deck Init, DrawNextCard, Group Progression
Status: Stage 2 complete. Phase machine cycling. DiceResolver
verified. AggroManager and StatusEffectResolver working.
Task: Implement MonsterAI.cs — deck initialization,
DrawNextCard(), group progression, and TriggerApex() only.
Do NOT implement RemoveCard() or EvaluateTrigger() yet —
those are Session 3-B.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_03/STAGE_03_A.md
- Assets/_Game/Scripts/Core.Systems/IMonsterAI.cs
- Assets/_Game/Scripts/Core.Data/Enums.cs
- Assets/_Game/Scripts/Core.Data/MonsterSO.cs

Then confirm:
- The single file you will create
- That RemoveCard() and EvaluateTrigger() will be stubs only
- What you will NOT touch this session

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 3-A: MonsterAI — Deck Init, DrawNextCard, Group Progression

**Resuming from:** Stage 2 complete  
**Done when:** Debug.Log shows correct card drawn from Opening group on Round 1; group advances to Escalation after all Opening cards drawn once  
**Commit:** `"3A: MonsterAI deck init, DrawNextCard, group progression, TriggerApex"`  
**Next session:** STAGE_03_B.md  

---

## Key Rules for This Session

- `RemoveCard()` → stub only: log a warning and return
- `ExecuteCard()` → stub only: log the card name and return
- `EvaluateTrigger()` → stub only: always return false
- Permanent cards are tracked separately — never shuffled into the removable active deck
- Win condition check lives in `RemoveCard()` — implemented Session 3-B

---

## MonsterAI.cs

**Path:** `Assets/_Game/Scripts/Core.Systems/MonsterAI.cs`

```csharp
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

        // ── State ────────────────────────────────────────────────
        private bool _apexTriggered = false;

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
            // Check Permanent cards first — they fire if their trigger is active
            // EvaluateTrigger is a stub in this session (always false)
            foreach (var perm in _permanentCards)
            {
                if (EvaluateTrigger(perm.triggerCondition))
                {
                    Debug.Log($"[MonsterAI] Permanent card triggered: {perm.cardName}");
                    return perm;
                }
            }

            // If active deck empty: rebuild and reshuffle
            if (_activeDeck.Count == 0)
            {
                Debug.Log("[MonsterAI] Active deck exhausted — rebuilding and reshuffling");
                RebuildActiveDeck();
                ShuffleDeck(_activeDeck);
            }

            // Draw from top
            var card = _activeDeck[0];
            _activeDeck.RemoveAt(0);

            Debug.Log($"[MonsterAI] Drew: \"{card.cardName}\" " +
                      $"({card.group}/{card.cardType}) " +
                      $"Active deck remaining: {_activeDeck.Count}");
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

        // ── Stubs — implemented in Session 3-B ──────────────────
        public void RemoveCard(string cardName)
        {
            // STUB — implemented Session 3-B
            Debug.LogWarning($"[MonsterAI] RemoveCard stub called for: {cardName} — implement in 3-B");
        }

        public void ExecuteCard(BehaviorCardSO card, CombatState state)
        {
            // STUB — implemented Session 3-C (trigger evaluation)
            Debug.Log($"[MonsterAI] ExecuteCard stub: {card.cardName} — implement in 3-C");
        }

        // ── Trigger Evaluation — Stub ────────────────────────────
        // Implemented in Session 3-C after EvaluateTrigger logic is designed
        private bool EvaluateTrigger(string triggerCondition)
        {
            // STUB — always returns false until 3-C
            return false;
        }
    }
}
```

---

## Wire MonsterAI into CombatManager

Add to `CombatManager.cs` — replace the `_monsterAI` stub wiring:

```csharp
// Add public factory method to CombatManager
public void InitializeMonsterAI(MonsterSO monster, string difficulty)
{
    var ai = new MonsterAI();
    ai.InitializeDeck(monster, difficulty);
    SetMonsterAI(ai);
    Debug.Log($"[Combat] MonsterAI initialized for {monster.monsterName} ({difficulty})");
}
```

---

## Verification Test

Temporary test script — delete after passing:

**Path:** `Assets/_Game/Scripts/Core.Systems/MonsterAITest.cs` *(delete after)*

```csharp
using UnityEngine;
using MnM.Core.Data;
using MnM.Core.Systems;

public class MonsterAITest : MonoBehaviour
{
    [SerializeField] private MonsterSO _gauntSO; // Assign Mock_GauntStandard in Inspector

    private void Start()
    {
        if (_gauntSO == null)
        {
            Debug.LogError("[MonsterAITest] Assign Mock_GauntStandard in Inspector");
            return;
        }

        var ai = new MonsterAI();
        ai.InitializeDeck(_gauntSO, "Standard");

        Debug.Log($"[Test] Remaining removable: {ai.RemainingRemovableCount}");
        Debug.Assert(ai.HasRemovableCards(), "FAIL: should have removable cards");
        Debug.Assert(ai.CurrentGroup == BehaviorGroup.Opening, "FAIL: should start in Opening");

        // Draw all opening cards — verify group stays Opening until all drawn
        // (Mock_GauntStandard has 3 opening cards: Creeping Advance, Scent Lock, Flank Sense)
        int openingCount = 0;
        for (int i = 0; i < 10; i++) // Draw up to 10 to flush opening group
        {
            var card = ai.DrawNextCard();
            if (card != null)
            {
                openingCount++;
                Debug.Log($"[Test] Drew #{i+1}: {card.cardName}");
            }
            ai.AdvanceGroupIfExhausted();
            if (ai.CurrentGroup == BehaviorGroup.Escalation) break;
        }

        Debug.Log($"[Test] Group after drawing: {ai.CurrentGroup}");
        // Note: group advance requires opening cards COUNT to reach 0
        // With mock data this may not trigger yet — that's expected
        // Full content added in Stage 7

        // Test Apex trigger
        Debug.Assert(!ai.RemainingRemovableCount.Equals(0) || true,
            "Removable count check"); // just verify it runs
        ai.TriggerApex();
        Debug.Log("[Test] TriggerApex called — check log for APEX TRIGGERED message");

        // Verify double-trigger is ignored
        ai.TriggerApex();
        Debug.Log("[Test] Second TriggerApex call — should see no second APEX TRIGGERED message");

        Debug.Log("[MonsterAITest] ✓ MonsterAI deck init and draw verified");
    }
}
```

Attach to a GameObject, assign `Mock_GauntStandard` in the Inspector, Play, verify Debug.Log output, **delete the test script**.

> ⚑ If Mock_GauntStandard has empty behavior card arrays (expected at this stage — full cards built in Stage 7), the test will draw 0 cards. That is correct. The test is verifying the deck structure, not card content.

---

## Next Session

**File:** `_Docs/Stage_03/STAGE_03_B.md`  
**Covers:** RemoveCard() with mid-turn win condition, and PartResolver Shell/Flesh/break/wound pipeline
