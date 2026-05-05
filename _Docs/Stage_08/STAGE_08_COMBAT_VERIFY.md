<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 8 Combat Verify | Full Combat Loop Integration Gate
Status: Stage 8-M complete. Hunt travel scene wired.
Task: This is a gate session — no new features. Run the full
Aldric vs Gaunt Standard combat from Settlement through Hunt
Travel into a complete combat round-loop and back to Settlement.
Every mechanical system introduced in 8K, 8L, and 8M must pass
before Stage 9 content begins. Fix any bugs found. Do not add
features. Tag a verify-combat commit when done.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_08/STAGE_08_COMBAT_VERIFY.md
- Assets/_Game/Scripts/Core.Systems/CombatManager.cs
- Assets/_Game/Scripts/Core.Systems/MonsterAI.cs
- Assets/_Game/Scripts/Core.UI/CombatScreenController.cs

Then confirm:
- All scripts compile with zero errors
- CombatTestBootstrapper exists for jumping directly to combat
- The mock scenario (Aldric vs Gaunt Standard) is loadable
- What you will NOT build here: any new feature or visual change

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 8 — Combat Loop Integration Gate

**Resuming from:** Stage 8-M complete — hunt travel scene wired
**Done when:** Every item in the full verification checklist below is checked; all mechanical systems from 8K, 8L, 8M pass; zero compile errors
**Commit:** `"8-verify: combat loop integration gate — all systems green"`
**Next session:** STAGE_08_Q.md

---

## Purpose

This is a bug-fix and validation session only. Do not add new features, animations, or content. The canonical mock scenario is **Aldric vs The Gaunt Standard, Round 1**. Test every mechanical system end-to-end and fix any bugs found before stage 9 content begins layering on top of this foundation.

---

## Bug Fix Protocol

When you find a bug:

1. Identify the file and approximate line
2. Fix it minimally — don't refactor
3. Re-run only the affected checklist item to confirm the fix
4. Log the fix with a comment: `// VERIFY-FIX: [description]`
5. Continue to the next checklist item

---

## Full Verification Checklist

### Compile and Scene Setup
- [ ] Zero compile errors in Unity Console
- [ ] All scenes in Build Settings (Bootstrap → MainMenu → CampaignSelect → CharacterCreation → Settlement → HuntTravel → CombatScene → GameOver → VictoryEpilogue)
- [ ] CombatTestBootstrapper or CombatScene can be played directly with mock data

### Hunt Flow (Stage 8-M)
- [ ] Settlement: SEND HUNTING PARTY → HuntTravel scene loads with fade
- [ ] HuntTravel: wilderness background visible; hunt target label shows "THE GAUNT (STANDARD)"
- [ ] HuntTravel: 0–3 travel events display as cards; each resolves on click
- [ ] HuntTravel: CONTINUE TO HUNT → CombatScene loads with fade
- [ ] Returning from combat → Settlement loads correctly

### Combat Initialisation
- [ ] CombatScene opens → Phase banner shows "VITALITY PHASE"
- [ ] All 4 hunter panels populate with names, body zones, AP, Grit
- [ ] Monster panel shows Gaunt Standard parts with correct shell/flesh maxes
- [ ] Behavior deck panel shows Gaunt's cards
- [ ] Grid renders 22×16 cells; hunter tokens placed at starting positions

### Hunter Movement (Stage 8-K)
- [ ] Hunter Phase starts → green movement range highlights appear around active hunter
- [ ] Click a green cell → hunter moves; range redraws from new position
- [ ] Movement respects movement stat (default movement, confirm range size)
- [ ] Click occupied cell → no move, no error
- [ ] Card selected → movement highlights clear; monster cells show gold
- [ ] Card deselected → movement highlights return
- [ ] End Turn → current hunter's highlights clear; next hunter's range appears

### Hunter Facing
- [ ] After move, Console logs `[Combat] [name] moved to (X,Y) facing (dx,dy)`
- [ ] Arc calculation fires: `[Grid] Arc check: attacker... → Front/Flank/Rear`

### Card Play & Damage
- [ ] Select a card → click a monster cell → TryPlayCard fires
- [ ] Console shows `[Card] Resolving: "[card]"` with accuracy, AP cost
- [ ] Hit result: `[Card] SHELL HIT` or `[Card] FLESH WOUND` logged
- [ ] Miss result: `[Card] MISS` logged
- [ ] Monster part bars update after shell or flesh damage
- [ ] AP display decrements after each card play
- [ ] Card removed from hand after play; discard count increments

### Monster Card Removal
- [ ] When a monster part's shell reaches 0 → `[Card] First part break — Apex trigger flagged`
- [ ] If behavior card linked to that part: `[MonsterAI] Card removed: "[name]"`
- [ ] `Removable:` count in behavior deck panel decrements
- [ ] If last removable removed → `[MonsterAI] *** LAST REMOVABLE CARD REMOVED — MONSTER DEFEATED ***`
- [ ] Victory modal appears

### Monster Execution (Stage 8-L)
- [ ] Monster Phase auto-advances 1.5s after all hunters act
- [ ] Console shows `[MonsterAI] ExecuteCard: [card name]`
- [ ] `EvaluateTrigger: Always — met` for basic cards
- [ ] Monster with Approach card: gridX/Y changes; `[MonsterPhase] GridManager updated`
- [ ] Monster with attack card: hunter body zone fleshCurrent drops; `OnDamageDealt` fires
- [ ] Body zone bars in hunter panel update after monster attack
- [ ] STUNNED stance: monster skips one Monster Phase, then clears

### Collapse & Combat End
- [ ] Hunter Head or Torso flesh → 0: `*** [name] COLLAPSED ***`; hunter panel shows collapsed state
- [ ] All hunters collapsed → hunt-lost modal
- [ ] Monster defeated → hunt-won modal
- [ ] Both modals: RETURN TO SETTLEMENT button → Settlement loads; hunt result recorded

### Round Counter
- [ ] Round label increments each full round (Vitality → Hunter → BehaviorRefresh → Monster)
- [ ] Round 2+ draws new cards into hunter hands; discard reshuffles into deck when empty

---

## Common Fixes Reference

| Symptom | Likely Cause | Fix |
|---|---|---|
| Monster stays still | `movementDistance = 0` on all Gaunt cards | Set movementDistance on at least one card |
| Monster deals no damage | `attackDamage = 0` or `attackTargetType = None` on all cards | Set at least one attack card |
| Movement range never shows | `_gridManager` not assigned in Inspector on CombatScreenController | Wire the GridManager reference |
| `EvaluateTrigger` returns false for "Always" | `triggerCondition` field is null not "" | Set to "Always" or "" on cards |
| GridManager occupancy wrong after monster move | `MonsterAI.InjectGrid()` not called | Check `InitializeMonsterAI()` calls `ai.InjectGrid()` |
| HuntTravel loads but no events | `eventPool` empty or no events have `isTravel = true` | Set `isTravel = true` on at least 2 events |

---

## Next Session

**File:** `_Docs/Stage_08/STAGE_08_Q.md`
**Covers:** Save/Load UI, Game Over screen, Victory Epilogue — completes the campaign state persistence loop and the end-state screens
