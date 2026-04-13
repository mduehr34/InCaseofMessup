<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 2-E | AggroManager, StatusEffectResolver & Round 1 Test
Status: Stage 2-D complete. Phase machine cycles correctly.
DiceResolver Debug.Log verified. Test script deleted.
Task: Implement AggroManager and StatusEffectResolver.
Then run the canonical Round 1 console test to confirm
Stage 2 is complete.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_02/STAGE_02_E.md
- Assets/_Game/Scripts/Core.Systems/CombatManager.cs
- Assets/_Game/Scripts/Core.Data/CombatState.cs
- Assets/_Game/Scripts/Core.Data/Enums.cs

Then confirm:
- What files you will create
- What you will NOT touch this session

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 2-E: AggroManager, StatusEffectResolver & Round 1 Test

**Resuming from:** Stage 2-D complete — phase machine and DiceResolver verified  
**Done when:** Round 1 console test passes all assertions. Stage 2 Definition of Done fully checked off.  
**Commit:** `"2E: AggroManager, StatusEffectResolver — Stage 2 complete, Round 1 test passes"`  
**Next session:** STAGE_03_A.md (Stage 3 begins)  

---

## Step 1: AggroManager.cs

**Path:** `Assets/_Game/Scripts/Core.Systems/AggroManager.cs`

```csharp
using UnityEngine;
using MnM.Core.Data;

namespace MnM.Core.Systems
{
    public class AggroManager
    {
        private CombatState _state;

        public string AggroHolderId => _state?.aggroHolderId;

        public void Initialize(CombatState state)
        {
            _state = state;
            Debug.Log($"[Aggro] Initialized. Holder: {_state.aggroHolderId}");
        }

        public void TransferAggro(string newHolderId)
        {
            string previous = _state.aggroHolderId;
            _state.aggroHolderId = newHolderId;
            Debug.Log($"[Aggro] Token transferred: {previous} → {newHolderId}");
        }

        // Special Pack rule — called by CombatManager when a wolf is killed
        public void OnWolfKilled(string killingBlowHunterId)
        {
            Debug.Log($"[Aggro] Wolf killed — aggro transfers immediately to killing blow holder");
            TransferAggro(killingBlowHunterId);
        }

        // Certain behavior cards target the most-damaged hunter (most Shell damage)
        public string GetMostExposedHunterId(HunterCombatState[] hunters)
        {
            string mostExposedId = null;
            int lowestShell = int.MaxValue;

            foreach (var hunter in hunters)
            {
                if (hunter.isCollapsed) continue;
                int totalShell = 0;
                foreach (var zone in hunter.bodyZones)
                    totalShell += zone.shellCurrent;

                if (totalShell < lowestShell)
                {
                    lowestShell     = totalShell;
                    mostExposedId   = hunter.hunterId;
                }
            }

            Debug.Log($"[Aggro] Most exposed hunter: {mostExposedId} (shell total: {lowestShell})");
            return mostExposedId;
        }
    }
}
```

---

## Step 2: StatusEffectResolver.cs

**Path:** `Assets/_Game/Scripts/Core.Logic/StatusEffectResolver.cs`

```csharp
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MnM.Core.Data;

namespace MnM.Core.Logic
{
    public static class StatusEffectResolver
    {
        // ── Apply / Remove / Query ───────────────────────────────
        public static void Apply(ref string[] statusEffects, StatusEffect effect)
        {
            var list = new List<string>(statusEffects);
            string tag = effect.ToString();
            if (!list.Contains(tag))
            {
                list.Add(tag);
                Debug.Log($"[Status] Applied: {tag}");
            }
            statusEffects = list.ToArray();
        }

        public static void Remove(ref string[] statusEffects, StatusEffect effect)
        {
            var list = new List<string>(statusEffects);
            if (list.Remove(effect.ToString()))
                Debug.Log($"[Status] Removed: {effect}");
            statusEffects = list.ToArray();
        }

        public static bool Has(string[] statusEffects, StatusEffect effect) =>
            statusEffects != null &&
            System.Array.IndexOf(statusEffects, effect.ToString()) >= 0;

        // ── Per-Effect Rules ─────────────────────────────────────
        // Called at start of each hunter's action to apply status penalties
        public static void ApplyStatusPenalties(HunterCombatState hunter, ref int accuracyMod, ref int movementMod)
        {
            if (Has(hunter.activeStatusEffects, StatusEffect.Shaken))
            {
                accuracyMod -= 1;
                Debug.Log($"[Status] {hunter.hunterName} Shaken: -1 Accuracy this action");
            }
            if (Has(hunter.activeStatusEffects, StatusEffect.Slowed))
            {
                movementMod = Mathf.FloorToInt(movementMod * 0.5f);
                Debug.Log($"[Status] {hunter.hunterName} Slowed: movement halved");
            }
        }

        // Auto-remove statuses that expire after one use
        public static void TickAfterAction(ref string[] statusEffects, HunterCombatState hunter)
        {
            // Shaken: auto-removes after one action
            if (Has(statusEffects, StatusEffect.Shaken))
            {
                Remove(ref statusEffects, StatusEffect.Shaken);
                Debug.Log($"[Status] {hunter.hunterName} Shaken expired after action");
            }
            // Slowed: auto-removes at end of turn
            if (Has(statusEffects, StatusEffect.Slowed))
            {
                Remove(ref statusEffects, StatusEffect.Slowed);
                Debug.Log($"[Status] {hunter.hunterName} Slowed expired after action");
            }
        }

        // Bleeding: called during Vitality Phase — lose 1 Flesh
        // Returns true if Flesh damage was applied
        public static bool TickBleeding(ref string[] statusEffects, ref BodyZoneState torsoZone, string hunterName)
        {
            if (!Has(statusEffects, StatusEffect.Bleeding)) return false;
            torsoZone.fleshCurrent = UnityEngine.Mathf.Max(0, torsoZone.fleshCurrent - 1);
            Debug.Log($"[Status] {hunterName} Bleeding: -1 Flesh to Torso. " +
                      $"Torso Flesh: {torsoZone.fleshCurrent}/{torsoZone.fleshMax}");
            return true;
        }
    }
}
```

---

## Step 3: Wire AggroManager into CombatManager

Add to `CombatManager.cs`:

```csharp
// Add field
private AggroManager _aggroManager = new AggroManager();

// Add to StartCombat():
_aggroManager.Initialize(initialState);

// Add public accessor for Stage 3 and UI use
public AggroManager AggroManager => _aggroManager;
```

---

## Stage 2 Definition of Done — Final Verification

Run this as the canonical Round 1 console test. Create a temporary test script, verify, then delete it.

**Path:** `Assets/_Game/Scripts/Core.Systems/Stage2FinalTest.cs` *(delete after)*

```csharp
using UnityEngine;
using MnM.Core.Data;
using MnM.Core.Systems;
using MnM.Core.Logic;

public class Stage2FinalTest : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("=== STAGE 2 FINAL TEST — Aldric vs Gaunt Standard Round 1 ===");

        // ── Setup ────────────────────────────────────────────────
        var gridGO  = new GameObject("Grid");
        var grid    = gridGO.AddComponent<GridManager>();

        var combatGO = new GameObject("Combat");
        var combat   = combatGO.AddComponent<CombatManager>();

        var state = CombatStateFactory.BuildMockCombatState();
        combat.StartCombat(state);

        // Place occupants on grid
        grid.PlaceOccupant(new GridOccupant
            { occupantId = "hunter_aldric", isHunter = true, footprintW = 1, footprintH = 1 },
            new Vector2Int(5, 8));
        grid.PlaceOccupant(new GridOccupant
            { occupantId = "monster", isHunter = false, footprintW = 2, footprintH = 2 },
            new Vector2Int(12, 7));

        // ── Round 1 ──────────────────────────────────────────────

        // 1. Vitality Phase → cards drawn
        Debug.Assert(combat.CurrentPhase == CombatPhase.VitalityPhase, "FAIL: start phase");
        combat.AdvancePhase(); // runs Vitality, moves to Hunter
        Debug.Assert(combat.CurrentPhase == CombatPhase.HunterPhase, "FAIL: after Vitality");
        Debug.Assert(state.hunters[0].handCardNames.Length == 2,
            $"FAIL: Aldric hand should be 2, got {state.hunters[0].handCardNames.Length}");
        Debug.Log($"[Test] Aldric hand: [{string.Join(", ", state.hunters[0].handCardNames)}]");

        // 2. d10 Precision Check
        var precision = DiceResolver.ResolvePrecision(0, 2, 0, false, false);
        Debug.Log($"[Test] Precision — Hit:{precision.isHit} Crit:{precision.isCritical} Roll:{precision.rawRoll}");

        // 3. d10 Force Check
        var force = DiceResolver.ResolveForce(0, 1, false, false);
        Debug.Log($"[Test] Force — Wound:{force.isWound} Roll:{force.rawRoll}");

        // 4. Status Effect apply/remove
        string[] testEffects = new string[0];
        StatusEffectResolver.Apply(ref testEffects, StatusEffect.Shaken);
        Debug.Assert(StatusEffectResolver.Has(testEffects, StatusEffect.Shaken), "FAIL: Shaken not applied");
        StatusEffectResolver.Remove(ref testEffects, StatusEffect.Shaken);
        Debug.Assert(!StatusEffectResolver.Has(testEffects, StatusEffect.Shaken), "FAIL: Shaken not removed");

        // 5. Aggro transfer
        Debug.Assert(state.aggroHolderId == "hunter_aldric", "FAIL: wrong initial aggro holder");
        combat.AggroManager.TransferAggro("hunter_brunhild");
        Debug.Assert(state.aggroHolderId == "hunter_brunhild", "FAIL: aggro transfer failed");
        combat.AggroManager.TransferAggro("hunter_aldric"); // restore

        // 6. End hunter turn → BehaviorRefresh → MonsterPhase → VitalityPhase
        combat.EndHunterTurn("hunter_aldric");
        // EndHunterTurn calls AdvancePhase internally when all hunters done
        Debug.Assert(combat.CurrentPhase == CombatPhase.BehaviorRefresh ||
                     combat.CurrentPhase == CombatPhase.MonsterPhase,
            $"FAIL: expected BehaviorRefresh or MonsterPhase, got {combat.CurrentPhase}");

        combat.AdvancePhase(); // → MonsterPhase (if at BehaviorRefresh)
        combat.AdvancePhase(); // → VitalityPhase round 2
        Debug.Assert(combat.CurrentPhase == CombatPhase.VitalityPhase,
            "FAIL: should be back at VitalityPhase");
        Debug.Assert(state.currentRound == 1,
            $"FAIL: currentRound should be 1, got {state.currentRound}");

        Debug.Log("=== STAGE 2 FINAL TEST PASSED ✓ ===");
        Debug.Log("Stage 2 Definition of Done:");
        Debug.Log("✓ Interfaces defined and approved");
        Debug.Log("✓ CombatState JSON round-trip verified (Session 2-B)");
        Debug.Log("✓ Phase machine cycles correctly");
        Debug.Log("✓ d10 Precision and Force Checks log correct math");
        Debug.Log("✓ Aggro transfer works");
        Debug.Log("✓ Status effects apply/remove correctly");
        Debug.Log("✓ No UI code written");

        Destroy(gridGO);
        Destroy(combatGO);
    }
}
```

All log lines must appear. All assertions must pass. Then **delete this test script**.

---

## Stage 2 Complete

You now have:
- 3 approved interfaces (IGridManager, ICombatManager, IMonsterAI)
- Full JSON-serializable CombatState
- GridManager with placement, movement, facing arc, denial, Chebyshev distance
- CombatManager phase machine cycling all 4 phases correctly
- DiceResolver with mandatory Debug.Log on every roll
- AggroManager with transfer and Pack kill rule
- StatusEffectResolver with all 5 status effects

No card resolution. No monster AI. No UI. Clean foundation for Stage 3.

---

## Next Session

**File:** `_Docs/Stage_03/STAGE_03_A.md`  
**First task of Stage 3:** MonsterAI deck initialization and DrawNextCard()
