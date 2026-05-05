<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude Code to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 8-T | Monster Wounds & Defeat Logic Rework
Status: Stage 8-K complete. Hunter movement, card play, behavior
card removal on flesh wounds, and part targeting all functional.
Task: Rework monster health/wounds to make behavior cards the
monster's life total. Flesh wounds remove behavior cards.
Shell breaks expose parts. Monster is defeated only when all
removable behavior cards are gone — NOT when parts break.

Read these files before doing anything:
- CLAUDE.md
- _Docs/Stage_08/STAGE_08_T.md
- Assets/_Game/Scripts/Core.Systems/CombatManager.cs
- Assets/_Game/Scripts/Core.Logic/PartResolver.cs
- Assets/_Game/Scripts/Core.Logic/CardResolver.cs
- Assets/_Game/Scripts/Core.Systems/MonsterAI.cs
- Assets/_Game/Scripts/Core.Data/CombatState.cs
- Assets/_Game/Scripts/Core.Data/DataStructs.cs

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 8-T: Monster Wounds & Defeat Logic Rework

**Resuming from:** Stage 8-K complete — card play, flesh wounds removing behavior cards, targeting all working
**Done when:** Monster parts have correctly-bounded HP; flesh wounds reliably remove one behavior card per hit; the defeat condition is solely "zero removable cards remaining"; the UI reflects this correctly
**Commit:** `"8T: Monster wounds rework — behavior cards as life total, correct defeat condition"`
**Next session:** STAGE_08_U.md (or next in sequence)

---

## The Problem

The current system has two conflated notions of "monster health":

1. **Part HP** — each part has Shell + Flesh. Shell breaks at 0. Flesh is reduced by wounds.
2. **Behavior card count** — `RemainingRemovableCount` is the intended defeat condition.

These are disconnected. When all parts are broken, `FindMonsterPartAtCell` was returning `-1` (blocking attacks), even though the monster still had behavior cards and was still active. This has been partially fixed (broken parts are now targetable for flesh damage), but the underlying model needs clarification.

### The Correct Design

- **Shell = armor layer.** Each attack against an unbroken part deals 1 Shell damage. When Shell reaches 0, the part is **broken** (exposed). Broken parts can still be targeted.
- **Flesh = wound threshold.** Once a part's shell is 0 (broken or critical hit bypasses), attacks roll Force. A wound deals 1 Flesh damage and **removes one behavior card** from the monster's deck.
- **Behavior cards = life total.** The monster is defeated when `RemainingRemovableCount == 0`.
- **Flesh HP per part** should be meaningful — it limits how many wounds a single part can absorb. But total wounds across all parts = total cards removed = defeat.
- **Part death (flesh = 0)** doesn't kill the monster — it just means that part can no longer absorb wounds. Attacks on a fully-wounded part still hit the monster but deal no flesh damage (the wound capacity is exhausted).

---

## Current Flesh Wound → Card Removal Logic

In `CombatManager.TryPlayCard`, after `CardResolver.Resolve`:

```
// Per-part wound data (woundRemovesCardNames on MonsterBodyPart in MonsterSO)
foreach (var removedName in result.removedCardNames)
    _monsterAI?.RemoveCard(removedName);

// Fallback — if MonsterSO has no woundRemovesCardNames configured,
// pick a random removable card and remove it on any flesh hit
if (result.damageType == DamageType.Flesh && result.damageDealt > 0
    && result.removedCardNames.Count == 0
    && _monsterAI is MonsterAI concreteAI
    && concreteAI.HasRemovableCards())
{
    var fallback = concreteAI.GetRandomRemovableCardName();
    if (fallback != null) { ... RemoveCard(fallback); }
}
```

The fallback is working but is a stopgap. The ideal path is: MonsterSO parts have `woundRemovesCardNames` populated so specific wounds remove specific cards. The fallback handles the mock scenario where those arrays are empty.

---

## Part 1: Clarify Part HP Bounds

### Current Issue
The mock `CombatStateFactory` gives every part `shellMax=2, fleshMax=3`. After 2 shell hits the part breaks. After 3 flesh wounds the flesh reaches 0. But the monster has 15 removable behavior cards — far more than 7 parts × 3 flesh = 21 potential wounds. The numbers don't add up coherently.

### Resolution Options (choose one)
**Option A — Pure fallback (simplest):** Keep current fleshMax per part. Any flesh wound = 1 card removed (fallback already does this). Parts eventually reach fleshCurrent=0 and stop absorbing wounds, but the monster lives until cards are gone. Wounds beyond fleshMax on a given part are lost.

**Option B — Unlimited wounds per part:** Remove the `Mathf.Max(0, ...)` floor in `PartResolver.ApplyFleshDamage`. Allow flesh to go negative — each successive wound still removes a card. The part acts as a window into the monster's total card pool, not a bounded HP bar.

**Option C — Per-part card budget:** Each MonsterBodyPart has exactly N woundRemovesCardNames. Once those N cards are removed via that part, the part is "exhausted" and further wounds still land but remove from the global pool (fallback). This requires populating the MonsterSO inspector data.

**Recommendation:** Start with Option A for the mock. Wire Option C properly when MonsterSO data is filled in.

---

## Part 2: Targeting After All Parts Broken

The quick fix applied in Stage 8-K session:

```csharp
// Prefer unbroken parts; fall back to broken parts (still have flesh)
var preferred = eligible.Where(i => !m.parts[i].isBroken).ToList();
var pool      = preferred.Count > 0 ? preferred : eligible;
```

**Verify this works:** When all parts are broken, `preferred` is empty, so `pool = eligible` (all revealed parts). Attacks still resolve, deal flesh damage (since shell=0 → `shellDepleted=true` → `goesToFlesh=true`), and trigger the card removal fallback.

**Additional check:** `PartResolver.ApplyFleshDamage` currently guards `if (part.fleshCurrent >= prev) return result` — this prevents card removal when flesh is already at 0. For Option A, this means a part at fleshCurrent=0 no longer removes cards on hit. May need to remove the guard or route through the global fallback separately.

---

## Part 3: UI — Part HP Bars Should Show Broken State

Currently the part bars show Shell/Flesh as numeric bars. A broken part (shell=0) should visually differ from an unbroken one. Consider:

- **Shell bar hidden or grayed** when `isBroken == true`
- **Part name label turns red/amber** when broken
- **"BROKEN" badge** on the part row (CSS class `part-bar--broken`)

This is cosmetic but important for readability.

---

## Part 4: Win Condition Verification

The defeat path is already wired:
1. `MonsterAI.RemoveCard` → `if (!HasRemovableCards()) OnMonsterDefeated?.Invoke()`
2. `CombatManager.HandleMonsterDefeated` → `OnCombatEnded?.Invoke(new CombatResult { isVictory = true })`
3. UI shows victory modal

**Verify:** After the last removable card is removed, `OnCombatEnded` fires, the hunt result is set, and the victory screen appears. The monster should stop acting on its phase (MonsterPhase should early-exit if `!_monsterAI.HasRemovableCards()`).

Add early-exit guard to `RunMonsterPhase`:
```csharp
if (!_monsterAI.HasRemovableCards())
{
    Debug.Log("[MonsterPhase] Monster has no removable cards — skipping (defeat should have fired)");
    return;
}
```

---

## Part 5: Mock Data Alignment

For the Aldric vs Gaunt Standard mock (or any 4-hunter party mock):
- Monster starts with N removable cards (currently ~15 in the asset)
- 4 hunters × ~2 cards each × multiple rounds should be able to defeat it
- Shell durability should require roughly 2-3 hits before breaking per part
- Flesh wounds: 1 per successful wound roll = 1 card removed

Consider adjusting the mock Gaunt to have `shellMax=2` (already set) and ensuring the behavior card count is in the 8–12 range for a 30-45 minute playtest.

---

## Verification Checklist

- [ ] Hunter can still attack monster when all parts are broken (no "invalid target")
- [ ] Attacks on a broken part deal flesh damage (shell=0 → goesToFlesh=true in CardResolver)
- [ ] Each flesh wound removes exactly one behavior card (console: `[Combat] Monster behavior card discarded`)
- [ ] `Removable: N` in the UI decreases by 1 per successful wound
- [ ] Monster continues acting on its phase while cards remain
- [ ] Monster phase skips (or ends combat) when removable count reaches 0
- [ ] Victory modal appears when last card removed
- [ ] No "invalid target" warnings during normal attack flow
- [ ] Part HP bars visually indicate broken state
- [ ] Broken parts show no shell bar (or grayed)
