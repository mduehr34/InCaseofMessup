<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 10-B | Injury, Disorder & Fighting Art Mechanical Enforcement
Status: Stage 10-A complete. All Gaunt stubs resolved.
Task: The lifecycle card SOs created in Stage 9-A have data but
no mechanical teeth — InjurySO stat penalties are not applied,
DisorderSO triggers never fire, and FightingArtSO effects are not
playable. This session wires all three into combat and the
settlement hunter panel so they affect gameplay.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_10/STAGE_10_B.md
- Assets/_Game/Scripts/Core.Data/InjurySO.cs
- Assets/_Game/Scripts/Core.Data/ScarSO.cs
- Assets/_Game/Scripts/Core.Data/DisorderSO.cs
- Assets/_Game/Scripts/Core.Data/FightingArtSO.cs
- Assets/_Game/Scripts/Core.Data/HunterState.cs
- Assets/_Game/Scripts/Core.Systems/CombatManager.cs
- Assets/_Game/Scripts/Core.UI/SettlementScreenController.cs

Then confirm:
- All four SO types have mechanicalEffect strings already populated (Stage 9-A)
- HunterState already stores injuryIds, scarIds, disorderIds, fightingArtIds
- CombatManager initialises HunterCombatState from HunterState at hunt start
- You will parse mechanicalEffect strings once (at hunt start) into StatModifiers
- What you will NOT do (UI art, animations, audio)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 10-B: Injury, Disorder & Fighting Art Mechanical Enforcement

**Resuming from:** Stage 10-A complete — all Gaunt stubs resolved
**Done when:** Injury stat penalties apply at hunt start; disorder triggers fire at correct conditions; fighting arts appear in the action hand and resolve correctly
**Commit:** `"10B: Injury penalties, disorder triggers, and fighting art resolution wired into combat"`
**Next session:** STAGE_10_C.md

---

## What Each Type Does (Enforcement Summary)

| Type | When applied | Where resolved |
|------|-------------|----------------|
| **Injury** | Permanent stat penalty (e.g. -1 Accuracy) | Applied once at hunt start when building HunterCombatState |
| **Scar** | Conditional bonus (e.g. +1 TOU below half Flesh) | Checked each round; applied as a temporary modifier while condition holds |
| **Disorder** | Fires on specific trigger conditions | Checked each round; applies a round-scoped penalty when triggered |
| **Fighting Art** | Active technique — player chooses to activate | Appears as a special card in the hunter's hand; resolved via CombatManager |

---

## Part 1 — LifecycleCardResolver (new static class)

**Path:** `Assets/_Game/Scripts/Core.Logic/LifecycleCardResolver.cs`

This class owns all parsing and application of the four lifecycle card types. Everything else calls it.

```csharp
using System.Collections.Generic;
using UnityEngine;
using MnM.Core.Data;

namespace MnM.Core.Logic
{
    public static class LifecycleCardResolver
    {
        // ─────────────────────────────────────────────────────────────
        // INJURIES — parse mechanicalEffect into stat deltas
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns a StatModifiers representing all permanent stat penalties
        /// from this hunter's injuries. Called once per hunt at initialisation.
        /// </summary>
        public static StatModifiers ResolveInjuries(HunterState hunter, InjurySO[] allInjuries)
        {
            var total = new StatModifiers();
            if (hunter.injuryIds == null) return total;

            foreach (var id in hunter.injuryIds)
            {
                var so = FindById(allInjuries, (i) => i.injuryId == id);
                if (so == null) { Debug.LogWarning($"[Injury] SO not found for id {id}"); continue; }
                AddEffectString(ref total, so.mechanicalEffect);
            }
            return total;
        }

        // ─────────────────────────────────────────────────────────────
        // SCARS — conditional bonuses, re-evaluated each round
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns stat bonuses from scars whose conditions are currently met.
        /// Call this every round to compute the round's effective scar modifiers.
        /// </summary>
        public static StatModifiers ResolveActiveScars(HunterCombatState combat,
                                                        HunterState persistent,
                                                        ScarSO[] allScars)
        {
            var total = new StatModifiers();
            if (persistent.scarIds == null) return total;

            foreach (var id in persistent.scarIds)
            {
                var so = FindById(allScars, (s) => s.scarId == id);
                if (so == null) continue;
                if (IsScarConditionMet(so, combat))
                    AddEffectString(ref total, so.mechanicalEffect);
            }
            return total;
        }

        private static bool IsScarConditionMet(ScarSO scar, HunterCombatState combat)
        {
            var e = scar.mechanicalEffect.ToLower();
            if (e.Contains("below half flesh"))
                return combat.currentFlesh < combat.maxFlesh / 2;
            if (e.Contains("at 1 flesh"))
                return combat.currentFlesh <= 1;
            if (e.Contains("not attacked") || e.Contains("has not attacked"))
                return !combat.hasAttackedThisRound;
            if (e.Contains("first round"))
                return combat.currentRound == 1;
            // Default: unconditional bonus (e.g. flat stat bonuses)
            return true;
        }

        // ─────────────────────────────────────────────────────────────
        // DISORDERS — trigger evaluation, called each round
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Evaluates all disorder triggers for a hunter and returns any
        /// active penalties for this round. Context provides trigger data.
        /// </summary>
        public static StatModifiers ResolveDisordersThisRound(HunterCombatState combat,
                                                               HunterState persistent,
                                                               DisorderSO[] allDisorders,
                                                               DisorderContext ctx)
        {
            var total = new StatModifiers();
            if (persistent.disorderIds == null) return total;

            foreach (var id in persistent.disorderIds)
            {
                var so = FindById(allDisorders, (d) => d.disorderId == id);
                if (so == null) continue;
                if (IsDisorderTriggered(so, combat, ctx))
                {
                    Debug.Log($"[Disorder] {so.disorderName} triggered for {combat.hunterName}");
                    AddEffectString(ref total, so.mechanicalEffect);
                }
            }
            return total;
        }

        private static bool IsDisorderTriggered(DisorderSO disorder,
                                                 HunterCombatState combat,
                                                 DisorderContext ctx)
        {
            var trigger = disorder.triggerCondition.ToLower();

            if (trigger.Contains("start of each hunt"))
                return ctx.isFirstRound;
            if (trigger.Contains("killing blow"))
                return ctx.hunterJustLandedKill;
            if (trigger.Contains("another hunter") && trigger.Contains("takes damage"))
                return ctx.anotherHunterTookDamageThisRound;
            if (trigger.Contains("open grid zone") || trigger.Contains("no adjacent obstacles"))
                return ctx.hunterIsInOpenZone;
            if (trigger.Contains("below half flesh"))
                return combat.currentFlesh < combat.maxFlesh / 2;
            if (trigger.Contains("only conscious hunter"))
                return ctx.hunterIsLastConscious;
            if (trigger.Contains("overlord"))
                return ctx.monsterIsOverlord;
            // Fallback: always check triggers with "at the start of each hunt" semantics
            return false;
        }

        // ─────────────────────────────────────────────────────────────
        // FIGHTING ARTS — build the art hand at hunt start
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns all FightingArtSO objects this hunter can currently use.
        /// Filtered by unlockYear requirement.
        /// </summary>
        public static FightingArtSO[] GetUsableFightingArts(HunterState hunter,
                                                              FightingArtSO[] allArts)
        {
            if (hunter.fightingArtIds == null) return new FightingArtSO[0];
            var result = new List<FightingArtSO>();
            foreach (var id in hunter.fightingArtIds)
            {
                var so = FindById(allArts, (f) => f.artId == id);
                if (so == null) continue;
                if (hunter.yearsActive >= so.unlockYear)
                    result.Add(so);
            }
            return result.ToArray();
        }

        // ─────────────────────────────────────────────────────────────
        // PRIVATE HELPERS
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Parses a mechanicalEffect string like "-1 Accuracy; -1 Luck"
        /// and accumulates the numeric deltas into modifiers.
        /// Supports: Accuracy, Evasion, Toughness, Speed, Grit, Luck, Movement.
        /// </summary>
        private static void AddEffectString(ref StatModifiers mods, string effect)
        {
            if (string.IsNullOrEmpty(effect)) return;
            var parts = effect.Split(';');
            foreach (var part in parts)
            {
                var s = part.Trim().ToLower();
                // Match patterns like "-1 accuracy", "+2 toughness", "cannot use two-handed" (ignored for now)
                if (!TryParseStatDelta(s, out int delta, out string stat)) continue;
                switch (stat)
                {
                    case "accuracy":   mods.accuracy   += delta; break;
                    case "evasion":    mods.evasion     += delta; break;
                    case "toughness":  mods.toughness   += delta; break;
                    case "speed":      mods.speed       += delta; break;
                    case "grit":       mods.grit        += delta; break;
                    case "luck":       mods.luck        += delta; break;
                    case "movement":   mods.movement    += delta; break;
                }
            }
        }

        private static bool TryParseStatDelta(string s, out int delta, out string statName)
        {
            delta    = 0;
            statName = "";
            // Match: optional sign, digit(s), space, stat name
            var m = System.Text.RegularExpressions.Regex.Match(
                s, @"([+\-]?\d+)\s+(accuracy|evasion|toughness|speed|grit|luck|movement)");
            if (!m.Success) return false;
            delta    = int.Parse(m.Groups[1].Value);
            statName = m.Groups[2].Value;
            return true;
        }

        private static T FindById<T>(T[] pool, System.Func<T, bool> predicate) where T : class
        {
            if (pool == null) return null;
            foreach (var item in pool)
                if (predicate(item)) return item;
            return null;
        }
    }

    /// <summary>
    /// Provides round-level context needed to evaluate disorder trigger conditions.
    /// Populated by CombatManager at the start of each round.
    /// </summary>
    public class DisorderContext
    {
        public bool isFirstRound;
        public bool hunterJustLandedKill;
        public bool anotherHunterTookDamageThisRound;
        public bool hunterIsInOpenZone;
        public bool hunterIsLastConscious;
        public bool monsterIsOverlord;
    }
}
```

---

## Part 2 — Integrate Injuries into HunterCombatState Initialisation

In `CombatManager`, find where `HunterCombatState` objects are built from `HunterState` at hunt start. Add injury resolution:

```csharp
// In CombatManager.InitialiseHuntersForHunt()
// After setting base stats from hunterState:

var injuryPenalties = LifecycleCardResolver.ResolveInjuries(hunterState, _allInjuries);
combatState.accuracy   = Mathf.Max(0, hunterState.accuracy   + injuryPenalties.accuracy);
combatState.evasion    = Mathf.Max(0, hunterState.evasion    + injuryPenalties.evasion);
combatState.toughness  = Mathf.Max(0, hunterState.toughness  + injuryPenalties.toughness);
combatState.speed      = Mathf.Max(0, hunterState.speed      + injuryPenalties.speed);
combatState.grit       = Mathf.Max(0, hunterState.grit       + injuryPenalties.grit);
combatState.luck       = Mathf.Max(0, hunterState.luck       + injuryPenalties.luck);

if (injuryPenalties.accuracy != 0 || injuryPenalties.toughness != 0)
    Debug.Log($"[Injury] {hunterState.hunterName}: injury penalties applied " +
              $"ACC{injuryPenalties.accuracy:+0;-0} TOU{injuryPenalties.toughness:+0;-0}");
```

Add `_allInjuries`, `_allScars`, `_allDisorders`, `_allFightingArts` as `[SerializeField]` fields on `CombatManager` and wire them in the Inspector to the SO arrays.

---

## Part 3 — Scar Resolution in the Round Loop

In `CombatManager.BeginHunterTurn(HunterCombatState hunter)`:

```csharp
// Resolve active scar bonuses for this turn
var persistentState = GameStateManager.Instance.CampaignState.GetHunter(hunter.hunterId);
var scarBonuses = LifecycleCardResolver.ResolveActiveScars(hunter, persistentState, _allScars);
hunter.roundAccuracyBonus  += scarBonuses.accuracy;
hunter.roundEvasionBonus   += scarBonuses.evasion;
hunter.roundToughnessBonus += scarBonuses.toughness;
// (These round bonus fields should already exist from the gear system — if not, add them)
```

These bonuses are cleared at end of round — do not persist between turns.

---

## Part 4 — Disorder Resolution in the Round Loop

In `CombatManager.BeginHunterTurn(HunterCombatState hunter)`, after scar resolution:

```csharp
// Build context for this hunter's disorders
var ctx = new DisorderContext
{
    isFirstRound                      = _currentRound == 1,
    hunterJustLandedKill              = hunter.landedKillThisRound,
    anotherHunterTookDamageThisRound  = _anyHunterTookDamageThisRound,
    hunterIsInOpenZone                = IsHunterInOpenZone(hunter),
    hunterIsLastConscious             = CountConsciousHunters() == 1,
    monsterIsOverlord                 = _activeMonster.isOverlord
};

var disorderPenalties = LifecycleCardResolver.ResolveDisordersThisRound(
    hunter, persistentState, _allDisorders, ctx);

hunter.roundAccuracyBonus  += disorderPenalties.accuracy;
hunter.roundEvasionBonus   += disorderPenalties.evasion;
hunter.roundToughnessBonus += disorderPenalties.toughness;
// Disorder penalties are negative deltas — the same round bonus fields handle them
```

Add helper to `CombatManager`:
```csharp
private bool IsHunterInOpenZone(HunterCombatState h)
{
    // Open zone = no allied or monster token in any of the 4 orthogonal neighbours
    int[] dx = { 0,0,1,-1 };
    int[] dy = { 1,-1,0,0 };
    for (int i = 0; i < 4; i++)
    {
        int nx = h.gridX + dx[i];
        int ny = h.gridY + dy[i];
        if (IsOccupied(nx, ny)) return false;
    }
    return true;
}
```

---

## Part 5 — Fighting Arts in the Action Hand

Fighting arts appear as special action cards in the hunter's hand. Add them at hand-draw time.

In `CombatManager.DrawHand(HunterCombatState hunter, HunterState persistent)`:

```csharp
// After drawing regular action cards:
var arts = LifecycleCardResolver.GetUsableFightingArts(persistent, _allFightingArts);
foreach (var art in arts)
{
    // Convert FightingArtSO to a transient ActionCardSO-like display object
    var artCard = FightingArtCardAdapter.ToDisplayCard(art);
    hunter.handCards.Add(artCard);
}
```

**`FightingArtCardAdapter`** — new file at `Assets/_Game/Scripts/Core.Logic/FightingArtCardAdapter.cs`:

```csharp
using MnM.Core.Data;

namespace MnM.Core.Logic
{
    public static class FightingArtCardAdapter
    {
        /// <summary>
        /// Wraps a FightingArtSO as a transient ActionCardSO so it can be displayed
        /// in the action hand without creating persistent assets.
        /// </summary>
        public static ActionCardSO ToDisplayCard(FightingArtSO art)
        {
            var card = ScriptableObject.CreateInstance<ActionCardSO>();
            card.cardId          = art.artId;
            card.cardName        = art.artName;
            card.cardCategory    = "FightingArt";
            card.effectText      = art.mechanicalEffect;
            card.isFightingArt   = true;
            card.fightingArtRef  = art;
            return card;
        }
    }
}
```

Add to `ActionCardSO`:
```csharp
[Header("Fighting Art (transient)")]
public bool          isFightingArt  = false;
public FightingArtSO fightingArtRef = null;
```

**Resolving the fighting art effect in `CombatManager.PlayCard()`:**

```csharp
if (card.isFightingArt && card.fightingArtRef != null)
{
    ResolveFightingArt(card.fightingArtRef, activeHunter);
    return;
}
```

```csharp
private void ResolveFightingArt(FightingArtSO art, HunterCombatState hunter)
{
    var e = art.mechanicalEffect.ToLower();
    Debug.Log($"[FightingArt] {hunter.hunterName} uses {art.artName}: {art.mechanicalEffect}");

    // FA-01 Trample: after successful hit, push monster back 1 space
    if (art.artId == "FA-01")
    {
        hunter.pendingFightingArtEffect = "TRAMPLE_PUSH_1";
        return;
    }
    // FA-02 Quiet Step: +2 Accuracy on first attack if monster hasn't moved
    if (art.artId == "FA-02")
    {
        if (!_activeMonster.hasMovedThisHunt)
            hunter.roundAccuracyBonus += 2;
        return;
    }
    // FA-03 Brace Position: reduce incoming by 1 when adjacent to another hunter
    if (art.artId == "FA-03")
    {
        if (IsAdjacentToAnyHunter(hunter))
            hunter.activeFightingArtDamageReduction = 1;
        return;
    }
    // FA-04 Wound Reading: +1 Accuracy targeting an already-injured part
    if (art.artId == "FA-04")
    {
        hunter.pendingFightingArtEffect = "WOUND_READING_ACC+1";
        return;
    }
    // FA-05 Reckless Charge: +3 Accuracy, +1 damage, gain Shaken after
    if (art.artId == "FA-05")
    {
        hunter.roundAccuracyBonus += 3;
        hunter.roundDamageBonus   += 1;
        hunter.pendingFightingArtEffect = "RECKLESS_CHARGE_SHAKEN";
        return;
    }
    // FA-06 Ghost Leap: move through occupied space without triggering reaction
    if (art.artId == "FA-06")
    {
        hunter.ghostLeapActiveThisRound = true;
        return;
    }
    // FA-07 Iron Stance: if stationary 2 rounds, ignore next knockback
    if (art.artId == "FA-07")
    {
        if (hunter.stationaryRounds >= 2)
            hunter.immuneToNextKnockback = true;
        return;
    }
    // FA-08 Venom Analysis: +2 TOU vs poison; predict ability
    if (art.artId == "FA-08")
    {
        hunter.roundToughnessBonus += 2;
        // Ability prediction is informational — show next card name in HUD
        if (_activeDeck.Count > 0)
            _combatHUD.ShowNextCardPreview(_activeDeck[0].cardName);
        return;
    }
    // FA-09 Frenzy Blow: deal 3 hits to one part simultaneously
    if (art.artId == "FA-09")
    {
        hunter.pendingFightingArtEffect = "FRENZY_BLOW_3HITS";
        return;
    }
    // FA-10 Abyssal Read: name a monster ability; if correct, +2 Evasion
    if (art.artId == "FA-10")
    {
        // Show "name an ability" modal — simplified: +2 Evasion granted as a goodwill bonus
        // (full ability-naming UI is out of scope; treat as unconditional +2 EVA once per hunt)
        if (!hunter.spentHuntAbilities.Contains("FA-10"))
        {
            hunter.roundEvasionBonus += 2;
            hunter.spentHuntAbilities = AppendToArray(hunter.spentHuntAbilities, "FA-10");
        }
        return;
    }

    Debug.LogWarning($"[FightingArt] No handler for {art.artId} — effect text: {art.mechanicalEffect}");
}
```

Add these fields to `HunterCombatState` if not already present:
```csharp
public string  pendingFightingArtEffect      = "";
public int     activeFightingArtDamageReduction = 0;
public bool    ghostLeapActiveThisRound      = false;
public bool    immuneToNextKnockback         = false;
public int     stationaryRounds              = 0;
public bool    hasAttackedThisRound          = false;
public bool    landedKillThisRound           = false;
public int     roundDamageBonus              = 0;
```

---

## Part 6 — StatModifiers Struct

If `StatModifiers` doesn't already have a `movement` field, add it:

```csharp
[System.Serializable]
public struct StatModifiers
{
    public int accuracy;
    public int evasion;
    public int toughness;
    public int speed;
    public int grit;
    public int luck;
    public int movement;
    public int maxFleshHP;
}
```

---

## Verification Tests

- [ ] Hunter with `INJ-01` (Broken Arm: -1 Accuracy) enters combat → CombatState shows Accuracy -1 vs base; log confirms `[Injury] ... ACC-1`
- [ ] Hunter with `INJ-04` (Gouged Eye: -1 Accuracy, -1 Luck) → two separate penalties both applied
- [ ] Hunter with `SCAR-01` (Bite Mark: +1 TOU below half Flesh) at 3/8 Flesh → TOU +1 shown; at 6/8 Flesh → TOU bonus absent
- [ ] Hunter with `DIS-01` (Nightmare: Shaken at Round 1 start) → Round 1 start adds Shaken status; Round 2 Shaken not re-added
- [ ] Hunter with `DIS-08` (Megalophobia) fighting an overlord → -1 Grit, -1 TOU on Round 1; cleared Round 2
- [ ] Hunter with `FA-01` (Trample) → hand contains a "TRAMPLE" card visible in UI
- [ ] Play FA-01 → `pendingFightingArtEffect = "TRAMPLE_PUSH_1"`; on next successful hit, monster pushed 1 space
- [ ] Play FA-05 (Reckless Charge) → accuracy +3 and damage +1 this round; Shaken applied after attack
- [ ] Hunter with no lifecycle cards → no errors, no phantom bonuses
- [ ] Save mid-hunt → load → lifecycle effects still correct (not double-applied)

---

## Next Session

**File:** `_Docs/Stage_10/STAGE_10_C.md`
**Covers:** Monster sprite generation — generating sprites for all 8 standard monsters and 4 overlords using CoPlay's image generation tools, importing them into Unity, assigning them to MonsterSO assets, and verifying they display correctly on combat tokens
