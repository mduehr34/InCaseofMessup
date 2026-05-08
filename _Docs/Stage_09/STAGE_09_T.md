<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude Code to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 9-T | Bleed & Poison — Status Counter System
Status: Stage 9-S complete. Full Stage 9 content verified.
Task: Implement the Bleed and Poison status counter systems
for hunters. Bleed is a stacking counter that causes collapse
at 5 stacks. Poison ticks random body zone damage each monster
phase. Both can be inflicted by monster attacks and by
environmental triggers (limb hitting 0 flesh triggers Bleed).
Both require cure actions to clear.

Read these files before doing anything:
- CLAUDE.md
- _Docs/Stage_09/STAGE_09_T.md
- Assets/_Game/Scripts/Core.Data/CombatState.cs
- Assets/_Game/Scripts/Core.Data/DataStructs.cs
- Assets/_Game/Scripts/Core.Systems/CombatManager.cs
- Assets/_Game/Scripts/Core.Systems/MonsterAI.cs
- Assets/_Game/Scripts/Core.UI/CombatScreenController.cs

Then confirm:
- HunterCombatState has `activeStatusEffects string[]` (tag-based)
- There is NO StatusCounter struct yet — you will add it
- BodyZoneState tracks fleshCurrent per zone (Head, Torso, LeftArm, etc.)
- CombatManager.CheckHunterCollapse() already checks Head/Torso flesh
- What you will NOT build: cure item UI (that's gear/action card integration)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 9-T: Bleed & Poison — Status Counter System

**Resuming from:** Stage 9-S complete — Stage 9 content fully verified
**Done when:** Bleed counters stack to 5 and collapse the hunter; Poison ticks random body zone damage each monster phase; limb hitting 0 flesh automatically adds 1 Bleed; monster attacks can inflict both; cure actions can clear counters; all counter states visible in the UI
**Commit:** `"9T: Bleed and Poison status counters — stacking, tick damage, limb-wound trigger, cure"`
**Next session:** STAGE_09_U.md (or next in Stage 9 sequence)

---

## Design Intent

Bleed and Poison are **attrition counters**, not instant effects. They are distinct from the string-tag status effects (Pinned, Shaken, Slowed) in that they accumulate over time and have a threshold that collapses a hunter if ignored. This creates a "ticking clock" dynamic separate from direct body zone damage.

- **Bleed** — each counter represents ongoing blood loss. At 5 stacks the hunter collapses regardless of body zone HP. Sources: monster attacks that inflict bleed, and limb body zones (Arms, Legs) reaching 0 flesh.
- **Poison** — each counter ticks 1 flesh damage to a random body zone at the end of each monster phase. Does not itself collapse the hunter, but accelerates body zone damage toward the collapse conditions (Head or Torso flesh = 0). Sources: monster attacks only.

Both counters have a `current` and `max` value. Max starts at 5 for Bleed (configurable per balance pass) and 5 for Poison. Reaching max triggers their respective effects.

---

## Part 1: DataStructs.cs — Add StatusCounter

Add to `Assets/_Game/Scripts/Core.Data/DataStructs.cs`:

```csharp
[Serializable]
public struct StatusCounter
{
    public string type;     // "Bleed" or "Poison"
    public int current;     // Current stack count
    public int max;         // Collapse threshold (Bleed) or max tick damage (Poison)
                            // Default: 5 for both
}
```

---

## Part 2: CombatState.cs — Add Counters to HunterCombatState

Open `Assets/_Game/Scripts/Core.Data/CombatState.cs`.

Add to `HunterCombatState`:

```csharp
// Status counters — tracked separately from string-tag effects
// Bleed: stacks to max → immediate collapse
// Poison: ticks 1 flesh damage per monster phase
public StatusCounter[] statusCounters;
```

These are distinct from `activeStatusEffects` (the string tag array). Tag effects are binary (present/absent); counters are accumulating.

---

## Part 3: StatusCounterResolver.cs — New File

Create `Assets/_Game/Scripts/Core.Logic/StatusCounterResolver.cs`:

```csharp
using System.Collections.Generic;
using UnityEngine;
using MnM.Core.Data;

namespace MnM.Core.Logic
{
    public static class StatusCounterResolver
    {
        public const int BLEED_COLLAPSE_THRESHOLD = 5;
        public const int POISON_MAX_STACKS        = 5;

        // ── Add Counter ──────────────────────────────────────────
        // Returns true if the counter hit its max threshold this add
        public static bool AddCounter(HunterCombatState hunter, string type, int amount = 1)
        {
            var counters = new List<StatusCounter>(hunter.statusCounters ?? new StatusCounter[0]);

            int idx = counters.FindIndex(c => c.type == type);
            if (idx < 0)
            {
                int max = type == "Bleed" ? BLEED_COLLAPSE_THRESHOLD : POISON_MAX_STACKS;
                counters.Add(new StatusCounter { type = type, current = 0, max = max });
                idx = counters.Count - 1;
            }

            var counter    = counters[idx];
            counter.current = Mathf.Min(counter.current + amount, counter.max);
            counters[idx]  = counter;
            hunter.statusCounters = counters.ToArray();

            Debug.Log($"[Counter] {hunter.hunterName} {type}: {counter.current}/{counter.max}");
            return counter.current >= counter.max;
        }

        // ── Remove Counter (cure) ─────────────────────────────────
        // Removes up to `amount` stacks. Returns remaining stack count.
        public static int RemoveCounter(HunterCombatState hunter, string type, int amount = int.MaxValue)
        {
            var counters = new List<StatusCounter>(hunter.statusCounters ?? new StatusCounter[0]);

            int idx = counters.FindIndex(c => c.type == type);
            if (idx < 0) return 0;

            var counter     = counters[idx];
            counter.current = Mathf.Max(0, counter.current - amount);

            if (counter.current == 0)
                counters.RemoveAt(idx);
            else
                counters[idx] = counter;

            hunter.statusCounters = counters.ToArray();
            int remaining = counter.current;
            Debug.Log($"[Counter] {hunter.hunterName} {type} cleared by {amount}. Remaining: {remaining}");
            return remaining;
        }

        // ── Get Counter ───────────────────────────────────────────
        public static int GetCount(HunterCombatState hunter, string type)
        {
            if (hunter.statusCounters == null) return 0;
            foreach (var c in hunter.statusCounters)
                if (c.type == type) return c.current;
            return 0;
        }

        // ── Poison Tick ───────────────────────────────────────────
        // Called at end of each monster phase for each hunter with Poison stacks.
        // Deals 1 flesh to a random body zone per Poison stack.
        // Returns the total flesh dealt this tick.
        public static int TickPoison(HunterCombatState hunter)
        {
            int stacks = GetCount(hunter, "Poison");
            if (stacks <= 0 || hunter.bodyZones == null || hunter.bodyZones.Length == 0)
                return 0;

            // Pick a random body zone for each stack
            int totalDamage = 0;
            for (int i = 0; i < stacks; i++)
            {
                int zoneIdx = Random.Range(0, hunter.bodyZones.Length);
                var zone    = hunter.bodyZones[zoneIdx];
                zone.fleshCurrent = Mathf.Max(0, zone.fleshCurrent - 1);
                hunter.bodyZones[zoneIdx] = zone;
                totalDamage++;
                Debug.Log($"[Counter] Poison tick: {hunter.hunterName} {zone.zone} " +
                          $"{zone.fleshCurrent}/{zone.fleshMax}");
            }
            return totalDamage;
        }
    }
}
```

---

## Part 4: CombatManager — Wire Bleed and Poison

### 4A: Limb → Bleed trigger

In `CombatManager.CheckHunterCollapse()`, after the existing Head/Torso collapse check, add a limb check:

```csharp
// Limb at 0 flesh → add 1 Bleed counter
var limbZones = new[] { "LeftArm", "RightArm", "LeftLeg", "RightLeg" };
foreach (var zone in hunter.bodyZones)
{
    if (System.Array.IndexOf(limbZones, zone.zone) < 0) continue;
    if (zone.fleshCurrent > 0) continue;

    // Only add Bleed once per zone — check via a status tag so it doesn't stack each call
    string triggeredTag = $"BleedTriggered_{zone.zone}";
    if (System.Array.Exists(hunter.activeStatusEffects ?? new string[0],
        t => t == triggeredTag)) continue;

    bool collapsed = StatusCounterResolver.AddCounter(hunter, "Bleed", 1);
    Debug.Log($"[Combat] {hunter.hunterName} {zone.zone} at 0 flesh — Bleed counter added " +
              $"({StatusCounterResolver.GetCount(hunter, "Bleed")}/{StatusCounterResolver.BLEED_COLLAPSE_THRESHOLD})");

    // Mark this limb as having triggered Bleed so repeated calls don't stack
    var tags = new List<string>(hunter.activeStatusEffects ?? new string[0]);
    tags.Add(triggeredTag);
    hunter.activeStatusEffects = tags.ToArray();

    if (collapsed)
    {
        Debug.Log($"[Combat] *** {hunter.hunterName} COLLAPSED from Bleed ***");
        hunter.isCollapsed = true;
        OnEntityCollapsed?.Invoke(hunter.hunterId);
        (_gridManager as IGridManager)?.RemoveOccupant(hunter.hunterId);
        CheckHuntLoss();
        return;
    }
}
```

### 4B: Poison tick at end of Monster Phase

In `CombatManager.RunMonsterPhase()`, after the behavior card resolves, add:

```csharp
// Tick Poison for all hunters
foreach (var hunter in CurrentState.hunters)
{
    if (hunter.isCollapsed) continue;
    int poisonDamage = StatusCounterResolver.TickPoison(hunter);
    if (poisonDamage > 0)
    {
        Debug.Log($"[Combat] Poison dealt {poisonDamage} flesh to {hunter.hunterName}");
        CheckHunterCollapse(hunter);
    }
}
```

### 4C: Public API for monster attacks to inflict counters

Add to `CombatManager`:

```csharp
/// <summary>
/// Called by MonsterAI or BehaviorCard resolution when a card inflicts Bleed or Poison.
/// Returns true if the counter hit its collapse/max threshold.
/// </summary>
public bool ApplyStatusCounter(string hunterId, string counterType, int stacks = 1)
{
    var hunter = GetHunter(hunterId);
    if (hunter == null)
    {
        Debug.LogWarning($"[Combat] ApplyStatusCounter: hunter {hunterId} not found");
        return false;
    }

    bool thresholdReached = StatusCounterResolver.AddCounter(hunter, counterType, stacks);

    if (thresholdReached && counterType == "Bleed" && !hunter.isCollapsed)
    {
        Debug.Log($"[Combat] *** {hunter.hunterName} COLLAPSED from Bleed (max stacks) ***");
        hunter.isCollapsed = true;
        OnEntityCollapsed?.Invoke(hunter.hunterId);
        (_gridManager as IGridManager)?.RemoveOccupant(hunter.hunterId);
        CheckHuntLoss();
    }

    return thresholdReached;
}

/// <summary>
/// Called by cure items, actions, or settlement healing to remove counter stacks.
/// </summary>
public int CureStatusCounter(string hunterId, string counterType, int stacks = int.MaxValue)
{
    var hunter = GetHunter(hunterId);
    if (hunter == null) return 0;
    return StatusCounterResolver.RemoveCounter(hunter, counterType, stacks);
}
```

---

## Part 5: BehaviorCardSO — Bleed/Poison Infliction Fields

Open `Assets/_Game/Scripts/Core.Data/BehaviorCardSO.cs`.

Add to the existing damage sub-phase fields:

```csharp
[Header("Status Counter Infliction (on successful hit)")]
public int bleedStacks;    // Bleed counters added to the target hunter on hit (0 = none)
public int poisonStacks;   // Poison counters added to the target hunter on hit (0 = none)
```

In `MonsterAI.ApplyAttack()`, after applying body zone damage to each hit target, add:

```csharp
if (card.bleedStacks > 0)
    _combatManager?.ApplyStatusCounter(target.hunterId, "Bleed", card.bleedStacks);
if (card.poisonStacks > 0)
    _combatManager?.ApplyStatusCounter(target.hunterId, "Poison", card.poisonStacks);
```

> **Note:** `MonsterAI` needs a reference back to `CombatManager` for this call. Add `private CombatManager _combatManager;` and inject it via `InjectCombatManager(CombatManager cm)`. Wire in `CombatManager.InitializeMonsterAI()`.

---

## Part 6: UI — Display Counters

In `CombatScreenController.RefreshHunterPanel()`, add counter display below the body zone bars:

```csharp
// Show Bleed and Poison counters
int bleed  = StatusCounterResolver.GetCount(hunter, "Bleed");
int poison = StatusCounterResolver.GetCount(hunter, "Poison");

// Find or create counter labels in the panel
var bleedLabel  = hunterPanelEl.Q<Label>("bleed-counter");
var poisonLabel = hunterPanelEl.Q<Label>("poison-counter");

if (bleedLabel  != null) bleedLabel.text  = bleed  > 0 ? $"BLEED {bleed}/5"  : "";
if (poisonLabel != null) poisonLabel.text = poison > 0 ? $"POISON {poison}/5" : "";

bleedLabel?.EnableInClassList("counter--active",  bleed  > 0);
poisonLabel?.EnableInClassList("counter--active", poison > 0);
```

Add to the hunter panel UXML:
```xml
<ui:Label name="bleed-counter"  class="status-counter bleed-counter"  text="" />
<ui:Label name="poison-counter" class="status-counter poison-counter" text="" />
```

Add to `combat-screen.uss`:
```css
.status-counter {
    font-size: 11px;
    color: rgb(140, 120, 100);
    display: none;
}
.counter--active {
    display: flex;
}
.bleed-counter.counter--active {
    color: rgb(200, 60, 60);
}
.poison-counter.counter--active {
    color: rgb(80, 180, 80);
}
```

---

## Cure Mechanics

Cures are not fully wired until gear/action cards are authored in Stage 9-O onward. The API is in place (`CureStatusCounter`). Document the intended cure items here for content authors:

| Counter | Cure item / action | Stacks removed |
|---|---|---|
| Bleed | Bandage (consumable) | All stacks |
| Bleed | Cauterize (action card) | All stacks; applies 1 flesh damage to self |
| Poison | Antidote (consumable) | All stacks |
| Poison | Purge (action card) | 2 stacks |

These items are ScriptableObjects authored in Stage 9-O. When played, the action card calls `CombatManager.CureStatusCounter(hunterId, type, amount)`.

---

## Limb Wound → Disorder (Future — Stage 9-U)

When a limb reaches 0 flesh, two things happen in this session:
1. 1 Bleed counter is added (implemented above)
2. A disorder flag is set (implemented in Stage 9-U)

The `BleedTriggered_{zone}` tag added in Part 4A serves double duty — it marks that the limb event fired, so Stage 9-U's disorder trigger can check for the same tag without doubling up. Stage 9-U will read this tag and draw a disorder card from the campaign's disorder deck.

The design intent is that a limb at 0 is a serious event — not instantly fatal, but creating both an attrition clock (Bleed) and a permanent campaign scar (Disorder).

---

## Mock Data for Verification

Add `bleedStacks = 1` to the Gaunt's **Spear Thrust** mock card (`SingleTrigger`, targets Torso). This tests the full path: monster attack → bleed inflicted → counter visible in UI → counter ticks toward threshold.

Add `poisonStacks = 1` to a second Gaunt card for testing Poison tick behavior.

---

## Debug Verification: Aldric vs Gaunt Standard, Round 1 (Bleed/Poison pass)

**Bleed path:**
```
1. Gaunt Spear Thrust resolves — hits Aldric Torso
2. ApplyStatusCounter("Aldric", "Bleed", 1) called
Debug.Log: "[Counter] Aldric Bleed: 1/5"
3. UI updates: bleed-counter label shows "BLEED 1/5" in red
4. No collapse (1 < 5)
```

**Limb trigger path:**
```
1. Aldric LeftArm takes damage → fleshCurrent = 0
2. CheckHunterCollapse fires
3. LeftArm at 0 → AddCounter "Bleed" +1
Debug.Log: "[Combat] Aldric LeftArm at 0 flesh — Bleed counter added (1/5)"
4. BleedTriggered_LeftArm tag added to activeStatusEffects
5. Subsequent CheckHunterCollapse calls do not add another Bleed for this limb
```

**Collapse path:**
```
1. After 5 separate Bleed inflictions across multiple rounds:
Debug.Log: "[Counter] Aldric Bleed: 5/5"
Debug.Log: "[Combat] *** Aldric COLLAPSED from Bleed (max stacks) ***"
2. hunter.isCollapsed = true; OnEntityCollapsed fires
3. CheckHuntLoss checks all hunters
```

**Poison tick path:**
```
1. End of Monster Phase
2. Aldric has Poison: 2 stacks
3. TickPoison runs: 2 random zone hits, 1 flesh each
Debug.Log: "[Counter] Poison tick: Aldric RightArm 3/4"
Debug.Log: "[Counter] Poison tick: Aldric Torso 5/6"
4. CheckHunterCollapse runs — verify no false collapse
```

---

## Definition of Done — Stage 9-T

- [ ] `StatusCounter` struct in `DataStructs.cs`: `type`, `current`, `max` fields
- [ ] `HunterCombatState.statusCounters` field added in `CombatState.cs`
- [ ] `StatusCounterResolver.cs` created: `AddCounter`, `RemoveCounter`, `GetCount`, `TickPoison` all implemented
- [ ] Limb at 0 flesh triggers Bleed (+1 counter) via `CheckHunterCollapse` — fires only once per limb per combat via `BleedTriggered_*` tag
- [ ] Bleed at 5 stacks collapses the hunter immediately
- [ ] Poison ticks 1 flesh per stack to a random zone at end of each monster phase
- [ ] `BehaviorCardSO` has `bleedStacks` and `poisonStacks` fields; `MonsterAI.ApplyAttack` applies them
- [ ] `CombatManager.ApplyStatusCounter` and `CureStatusCounter` are public and callable by gear/action systems
- [ ] Bleed and Poison counters display in the hunter panel UI
- [ ] Mock Gaunt card(s) inflict Bleed and Poison for testing
- [ ] Debug verification passes: bleed infliction, limb trigger, bleed collapse, poison tick all logged correctly
- [ ] No compile errors

---

## Next Session

**File:** `_Docs/Stage_09/STAGE_09_U.md`
**Covers:** Limb wound → Disorder trigger — when a limb hits 0 flesh, draw from the campaign disorder deck and apply a persistent negative effect to the hunter that carries into settlement
