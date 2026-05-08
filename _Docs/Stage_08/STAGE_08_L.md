<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 8-L | Monster Action Execution Engine
Status: Stage 8-K complete. Hunter movement wired.
Task: Implement the two stubs in MonsterAI that make the monster
inert: ExecuteCard() (just logs "implement in 3-C") and
EvaluateTrigger() (always returns false). After this stage the
monster moves toward its aggro target, deals damage to hunter
body zones, and respects trigger conditions (Always, adjacent,
N+ spaces away, behind, part broken, below 50% HP).

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_08/STAGE_08_L.md
- Assets/_Game/Scripts/Core.Systems/MonsterAI.cs
- Assets/_Game/Scripts/Core.Systems/CombatManager.cs
- Assets/_Game/Scripts/Core.Systems/IMonsterAI.cs
- Assets/_Game/Scripts/Core.Data/BehaviorCardSO.cs
- Assets/_Game/Scripts/Core.Data/CombatState.cs
- Assets/_Game/Scripts/Core.Data/Enums.cs

Then confirm:
- MonsterAI.ExecuteCard() currently logs "implement in 3-C" and returns void
- MonsterAI.EvaluateTrigger() currently always returns false
- BehaviorCardSO has only text fields — no numeric movementDistance or attackDamage
- IMonsterAI.ExecuteCard returns void — must change to return BehaviorCardResult
- GridManager is NOT injected into MonsterAI — must add that wiring
- What you will NOT build: token lerp animations (Stage 10-M), multi-step
  pathfinding around obstacles (monster steps through, Knockback if occupied)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 8-L: Monster Action Execution Engine

**Resuming from:** Stage 8-K complete — hunter movement wired
**Done when:** Monster draws a behavior card and visibly acts — moves toward aggro target, deals flesh damage to a hunter body zone, and respects trigger conditions; all verified on Aldric vs Gaunt Standard
**Commit:** `"8L: Monster execution engine — ExecuteCard, EvaluateTrigger, facing, body zone damage"`
**Next session:** STAGE_08_M.md — Monster Health Rework

---

## What's Missing

`MonsterAI.ExecuteCard()` is a one-line stub: `Debug.Log("implement in 3-C")`. `EvaluateTrigger()` always returns `false`. When the monster draws a card each round, nothing happens. This stage adds structured execution fields to `BehaviorCardSO`, implements both methods, injects `IGridManager` into `MonsterAI`, and updates `CombatManager.RunMonsterPhase()` to process the result and fire the right events.

---

## Part 1: New Enums

Open `Assets/_Game/Scripts/Core.Data/Enums.cs`. Add these enums (confirm they don't already exist):

```csharp
public enum MovementPattern
{
    None,
    Approach,   // Step-by-step toward aggro target
    Charge,     // Full distance in a straight line, push through hunters
    Pivot,      // Face toward lowest-Flesh hunter, no position change
}

public enum AttackTargetType
{
    None,
    AggroTarget,    // Single hit on the aggro holder
    AllAdjacent,    // All hunters within 1 cell
    AllBehind,      // All hunters in rear arc (behind monster facing)
    AllInFront,     // All hunters in front arc
    AllInRange,     // All hunters within attackRange cells
}
```

---

## Part 2: Structured Execution Fields on BehaviorCardSO

Open `Assets/_Game/Scripts/Core.Data/BehaviorCardSO.cs`.

Add a new `[Header("Execution")]` block. Keep all existing fields untouched — these are additions only:

```csharp
[Header("Execution — Movement")]
public MovementPattern movementPattern = MovementPattern.None;
public int             movementDistance = 0;   // Cells to move (0 = no movement)

[Header("Execution — Attack")]
public AttackTargetType attackTargetType = AttackTargetType.None;
public int              attackDamage     = 0;  // Base flesh damage per target (0 = no attack)
public int              attackRange      = 1;  // Max cells to reach target (1 = adjacent/melee)

[Header("Execution — Special")]
public string specialTag = "";
// Simple effect tags — resolved in MonsterAI.ApplySpecial():
//   "PINNED"          — apply Pinned status to all adjacent hunters
//   "REGEN:N"         — restore N flesh to the most-damaged part
//   "STANCE:tagname"  — set MonsterCombatState.currentStanceTag
//   "STUN_SELF"       — skip next card draw (no action next monster phase)
//   "AGGRO:LOWEST"    — move aggro token to hunter with lowest flesh total

[Header("Execution — Deck")]
public bool isShuffle = false;  // Reshuffle active deck after this card resolves
```

---

## Part 3: BehaviorCardResult Struct

Create `Assets/_Game/Scripts/Core.Data/BehaviorCardResult.cs`:

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace MnM.Core.Data
{
    public class BehaviorCardResult
    {
        // Movement
        public bool       monsterMoved   = false;
        public Vector2Int newMonsterCell = Vector2Int.zero;

        // Attack outcomes — one entry per hunter hit
        public List<HitRecord> hits = new();

        // Special
        public bool   specialFired = false;
        public string specialTag   = "";

        public struct HitRecord
        {
            public string hunterId;
            public string zone;      // Body zone name hit
            public int    damage;
        }
    }
}
```

---

## Part 4: Update IMonsterAI

Open `Assets/_Game/Scripts/Core.Systems/IMonsterAI.cs`.

Change `ExecuteCard` return type from `void` to `BehaviorCardResult`, and add the grid injection method:

```csharp
using MnM.Core.Data;
using UnityEngine;

namespace MnM.Core.Systems
{
    public interface IMonsterAI
    {
        BehaviorGroup CurrentGroup          { get; }
        int           RemainingRemovableCount { get; }

        event System.Action OnMonsterDefeated;

        void             InitializeDeck(MonsterSO monster, string difficulty);
        BehaviorCardSO   DrawNextCard();
        BehaviorCardResult ExecuteCard(BehaviorCardSO card, CombatState state); // changed
        void             RemoveCard(string cardName);
        void             TriggerApex();
        void             AdvanceGroupIfExhausted();
        bool             HasRemovableCards();
        BehaviorCardSO[] GetActiveBehaviorCards();
        void             InjectGrid(IGridManager grid);  // new
    }
}
```

---

## Part 5: MonsterAI — Grid Injection and Helpers

Open `Assets/_Game/Scripts/Core.Systems/MonsterAI.cs`.

Add the grid field and inject method near the top of the class (alongside the deck lists):

```csharp
// ── Grid Reference ───────────────────────────────────────────
private IGridManager _grid;
public void InjectGrid(IGridManager grid) => _grid = grid;
```

Add these private helpers at the bottom of the class (before the closing brace):

```csharp
// ── Targeting Helpers ────────────────────────────────────────

private static HunterCombatState FindHunter(CombatState state, string id)
{
    if (state?.hunters == null || id == null) return null;
    return System.Array.Find(state.hunters, h => h.hunterId == id && !h.isCollapsed);
}

private static HunterCombatState FindHunterWithLowestFlesh(CombatState state)
{
    HunterCombatState best = null;
    int lowestFlesh = int.MaxValue;
    foreach (var h in state.hunters)
    {
        if (h.isCollapsed) continue;
        int total = 0;
        foreach (var z in h.bodyZones) total += z.fleshCurrent;
        if (total < lowestFlesh) { lowestFlesh = total; best = h; }
    }
    return best;
}

private bool AnyHunterWithinRange(CombatState state, Vector2Int origin, int range)
{
    if (_grid == null) return false;
    foreach (var h in state.hunters)
    {
        if (h.isCollapsed) continue;
        if (_grid.GetDistance(origin, new Vector2Int(h.gridX, h.gridY)) <= range) return true;
    }
    return false;
}

private List<HunterCombatState> GetHuntersWithinRange(CombatState state, Vector2Int origin, int range)
{
    var result = new List<HunterCombatState>();
    if (_grid == null) return result;
    foreach (var h in state.hunters)
    {
        if (h.isCollapsed) continue;
        if (_grid.GetDistance(origin, new Vector2Int(h.gridX, h.gridY)) <= range) result.Add(h);
    }
    return result;
}

private List<HunterCombatState> GetHuntersInArc(CombatState state, MonsterCombatState monster, FacingArc arc)
{
    var result = new List<HunterCombatState>();
    if (_grid == null) return result;
    var monCell     = new Vector2Int(monster.gridX, monster.gridY);
    var monFacing   = new Vector2Int(monster.facingX, monster.facingY);
    foreach (var h in state.hunters)
    {
        if (h.isCollapsed) continue;
        var hunterCell = new Vector2Int(h.gridX, h.gridY);
        if (_grid.GetArcFromAttackerToTarget(hunterCell, monCell, monFacing) == arc)
            result.Add(h);
    }
    return result;
}

// Step one cell toward target, prefer cardinal movement
private static Vector2Int StepToward(Vector2Int from, Vector2Int to)
{
    int dx = to.x - from.x;
    int dy = to.y - from.y;
    int stepX = dx == 0 ? 0 : (dx > 0 ? 1 : -1);
    int stepY = dy == 0 ? 0 : (dy > 0 ? 1 : -1);
    // Prefer axis with greater distance to stay on-track
    if (Mathf.Abs(dx) >= Mathf.Abs(dy))
        return new Vector2Int(from.x + stepX, from.y);
    return new Vector2Int(from.x, from.y + stepY);
}

// Apply damage to a random hunter body zone, return zone name hit
private static string DamageRandomZone(HunterCombatState hunter, int damage)
{
    if (hunter.bodyZones == null || hunter.bodyZones.Length == 0) return "Unknown";

    // Weighted: torso and limbs more likely than head
    var weights = new (string zone, int weight)[]
    {
        ("Head",     1),
        ("Torso",    3),
        ("LeftArm",  2),
        ("RightArm", 2),
        ("LeftLeg",  2),
        ("RightLeg", 2),
    };

    int total = 0;
    foreach (var w in weights) total += w.weight;
    int roll = Random.Range(0, total);

    string chosen = "Torso";
    int running = 0;
    foreach (var w in weights)
    {
        running += w.weight;
        if (roll < running) { chosen = w.zone; break; }
    }

    for (int i = 0; i < hunter.bodyZones.Length; i++)
    {
        if (hunter.bodyZones[i].zone != chosen) continue;
        var zone = hunter.bodyZones[i];
        zone.fleshCurrent = Mathf.Max(0, zone.fleshCurrent - damage);
        hunter.bodyZones[i] = zone;
        Debug.Log($"[MonsterAI] {hunter.hunterName} takes {damage} flesh to {chosen} " +
                  $"({zone.fleshCurrent}/{zone.fleshMax})");
        return chosen;
    }
    return chosen;
}
```

---

## Part 6: Implement EvaluateTrigger

Replace the stub `EvaluateTrigger` method entirely:

```csharp
private bool EvaluateTrigger(BehaviorCardSO card, CombatState state)
{
    string cond = (card.triggerCondition ?? "Always").Trim().ToLower();

    if (cond == "" || cond == "always")
    {
        Debug.Log($"[MonsterAI] EvaluateTrigger: Always — met");
        return true;
    }

    var monster    = state.monster;
    var aggroHunter = FindHunter(state, state.aggroHolderId);
    var monCell    = new Vector2Int(monster.gridX, monster.gridY);
    int distToAggro = aggroHunter != null && _grid != null
        ? _grid.GetDistance(monCell, new Vector2Int(aggroHunter.gridX, aggroHunter.gridY))
        : 99;

    // "any hunter is adjacent" / "no hunter is adjacent"
    bool anyAdjacent = AnyHunterWithinRange(state, monCell, 1);
    if (cond.Contains("no hunter") && cond.Contains("adjacent"))
    {
        bool met = !anyAdjacent;
        Debug.Log($"[MonsterAI] EvaluateTrigger: no hunter adjacent — {met}");
        return met;
    }
    if (cond.Contains("any hunter") && cond.Contains("adjacent"))
    {
        Debug.Log($"[MonsterAI] EvaluateTrigger: any hunter adjacent — {anyAdjacent}");
        return anyAdjacent;
    }

    // "aggro target is adjacent"
    if (cond.Contains("aggro target") && cond.Contains("adjacent"))
    {
        bool met = distToAggro <= 1;
        Debug.Log($"[MonsterAI] EvaluateTrigger: aggro adjacent (dist={distToAggro}) — {met}");
        return met;
    }

    // "aggro target is N+ spaces away"
    if (cond.Contains("spaces away") || cond.Contains("space away"))
    {
        var m = System.Text.RegularExpressions.Regex.Match(cond, @"(\d+)\+?\s+space");
        if (m.Success && int.TryParse(m.Groups[1].Value, out int minDist))
        {
            bool met = distToAggro >= minDist;
            Debug.Log($"[MonsterAI] EvaluateTrigger: aggro {minDist}+ spaces away (dist={distToAggro}) — {met}");
            return met;
        }
    }

    // "any hunter is behind" (rear arc)
    if (cond.Contains("behind"))
    {
        var behind = GetHuntersInArc(state, monster, FacingArc.Rear);
        bool met = behind.Count > 0;
        Debug.Log($"[MonsterAI] EvaluateTrigger: any hunter behind — {met}");
        return met;
    }

    // "spine crest shell is broken" / "[partName] shell is broken" / "[partName] is broken"
    if (cond.Contains("broken"))
    {
        foreach (var part in monster.parts)
        {
            string pname = part.partName.ToLower();
            if (!cond.Contains(pname)) continue;
            bool shellCheck = cond.Contains("shell") ? part.shellCurrent <= 0 : part.isBroken;
            Debug.Log($"[MonsterAI] EvaluateTrigger: {part.partName} broken check — {shellCheck}");
            return shellCheck;
        }
    }

    // "below 50%" / "below half"
    if (cond.Contains("below 50%") || cond.Contains("below half"))
    {
        int totalFlesh = 0, maxFlesh = 0;
        foreach (var p in monster.parts) { totalFlesh += p.fleshCurrent; maxFlesh += p.fleshMax; }
        bool met = maxFlesh > 0 && totalFlesh < maxFlesh / 2;
        Debug.Log($"[MonsterAI] EvaluateTrigger: below half HP ({totalFlesh}/{maxFlesh}) — {met}");
        return met;
    }

    // "any part flesh is broken" (any part at 0 flesh)
    if (cond.Contains("flesh") && cond.Contains("broken"))
    {
        foreach (var p in monster.parts)
        {
            if (p.fleshCurrent <= 0)
            {
                Debug.Log($"[MonsterAI] EvaluateTrigger: part flesh broken ({p.partName}) — true");
                return true;
            }
        }
        Debug.Log("[MonsterAI] EvaluateTrigger: no flesh part broken — false");
        return false;
    }

    // Unhandled — treat as Always and warn
    Debug.Log($"[MonsterAI] EvaluateTrigger: unhandled '{card.triggerCondition}' — treating as Always");
    return true;
}
```

---

## Part 7: Implement ExecuteCard

Replace the stub `ExecuteCard` with the full implementation. Change its signature to return `BehaviorCardResult`:

```csharp
public BehaviorCardResult ExecuteCard(BehaviorCardSO card, CombatState state)
{
    var result = new BehaviorCardResult();
    Debug.Log($"[MonsterAI] ExecuteCard: {card.cardName}");

    if (!EvaluateTrigger(card, state))
    {
        Debug.Log($"[MonsterAI] Trigger not met — {card.cardName} skipped");
        return result;
    }

    // ── Movement ──────────────────────────────────────────────
    if (card.movementPattern != MovementPattern.None && card.movementDistance > 0)
        result = ApplyMovement(card, state, result);

    // ── Attack ────────────────────────────────────────────────
    if (card.attackTargetType != AttackTargetType.None && card.attackDamage > 0)
        result = ApplyAttack(card, state, result);

    // ── Special ───────────────────────────────────────────────
    if (!string.IsNullOrEmpty(card.specialTag))
        result = ApplySpecial(card, state, result);

    // ── Reshuffle ─────────────────────────────────────────────
    if (card.isShuffle)
    {
        ShuffleDeck(_activeDeck);
        Debug.Log("[MonsterAI] Deck reshuffled after card");
    }

    return result;
}

private BehaviorCardResult ApplyMovement(BehaviorCardSO card, CombatState state,
                                          BehaviorCardResult result)
{
    if (_grid == null)
    {
        Debug.LogWarning("[MonsterAI] IGridManager not injected — movement skipped");
        return result;
    }

    var monster     = state.monster;
    var aggroHunter = FindHunter(state, state.aggroHolderId);

    if (aggroHunter == null)
    {
        Debug.LogWarning("[MonsterAI] No aggro target — movement skipped");
        return result;
    }

    if (card.movementPattern == MovementPattern.Pivot)
    {
        // Pivot: face lowest-flesh hunter, no position change
        var target = FindHunterWithLowestFlesh(state);
        if (target != null)
        {
            int dx = target.gridX - monster.gridX;
            int dy = target.gridY - monster.gridY;
            monster.facingX = dx == 0 ? 0 : (dx > 0 ? 1 : -1);
            monster.facingY = dy == 0 ? 0 : (dy > 0 ? 1 : -1);
            Debug.Log($"[MonsterAI] Pivot — monster now facing ({monster.facingX},{monster.facingY}) " +
                      $"toward {target.hunterName}");
        }
        return result;
    }

    // Approach / Charge — step toward aggro target
    var targetCell  = new Vector2Int(aggroHunter.gridX, aggroHunter.gridY);
    var currentCell = new Vector2Int(monster.gridX, monster.gridY);
    var bestCell    = currentCell;

    for (int step = 0; step < card.movementDistance; step++)
    {
        var next = StepToward(bestCell, targetCell);
        if (next == bestCell) break; // Already adjacent or blocked by target

        bool occupied = _grid.IsOccupied(next);
        if (occupied && card.movementPattern != MovementPattern.Charge) break;
        if (!_grid.IsInBounds(next)) break;

        bestCell = next;
    }

    if (bestCell != currentCell)
    {
        // Update facing toward movement direction
        int dx = bestCell.x - currentCell.x;
        int dy = bestCell.y - currentCell.y;
        if (dx != 0 || dy != 0)
        {
            monster.facingX = dx == 0 ? 0 : (dx > 0 ? 1 : -1);
            monster.facingY = dy == 0 ? 0 : (dy > 0 ? 1 : -1);
        }

        monster.gridX = bestCell.x;
        monster.gridY = bestCell.y;
        result.monsterMoved   = true;
        result.newMonsterCell = bestCell;

        Debug.Log($"[MonsterAI] Monster moved to ({bestCell.x},{bestCell.y}) " +
                  $"facing ({monster.facingX},{monster.facingY})");
    }

    return result;
}

private BehaviorCardResult ApplyAttack(BehaviorCardSO card, CombatState state,
                                        BehaviorCardResult result)
{
    var monster  = state.monster;
    var monCell  = new Vector2Int(monster.gridX, monster.gridY);
    var targets  = new List<HunterCombatState>();

    switch (card.attackTargetType)
    {
        case AttackTargetType.AggroTarget:
            var aggro = FindHunter(state, state.aggroHolderId);
            if (aggro != null) targets.Add(aggro);
            break;

        case AttackTargetType.AllAdjacent:
            targets = GetHuntersWithinRange(state, monCell, 1);
            break;

        case AttackTargetType.AllBehind:
            targets = GetHuntersInArc(state, monster, FacingArc.Rear);
            break;

        case AttackTargetType.AllInFront:
            targets = GetHuntersInArc(state, monster, FacingArc.Front);
            break;

        case AttackTargetType.AllInRange:
            targets = GetHuntersWithinRange(state, monCell, card.attackRange);
            break;
    }

    foreach (var target in targets)
    {
        if (target.isCollapsed) continue;
        string zone = DamageRandomZone(target, card.attackDamage);
        result.hits.Add(new BehaviorCardResult.HitRecord
        {
            hunterId = target.hunterId,
            zone     = zone,
            damage   = card.attackDamage,
        });
    }

    if (targets.Count == 0)
        Debug.Log($"[MonsterAI] Attack ({card.attackTargetType}) — no valid targets in range");

    return result;
}

private BehaviorCardResult ApplySpecial(BehaviorCardSO card, CombatState state,
                                         BehaviorCardResult result)
{
    result.specialFired = true;
    result.specialTag   = card.specialTag;
    var monster = state.monster;
    var monCell = new Vector2Int(monster.gridX, monster.gridY);

    // STANCE:tagname — set the monster's current stance
    if (card.specialTag.StartsWith("STANCE:"))
    {
        string tag = card.specialTag.Substring(7);
        monster.currentStanceTag = tag;
        Debug.Log($"[MonsterAI] Special: stance set to '{tag}'");
        return result;
    }

    // REGEN:N — restore N flesh to most-damaged part
    if (card.specialTag.StartsWith("REGEN:") &&
        int.TryParse(card.specialTag.Substring(6), out int regenAmount))
    {
        int lowestFlesh = int.MaxValue;
        int targetIdx   = -1;
        for (int i = 0; i < monster.parts.Length; i++)
        {
            if (monster.parts[i].fleshCurrent < lowestFlesh && !monster.parts[i].isBroken)
            {
                lowestFlesh = monster.parts[i].fleshCurrent;
                targetIdx   = i;
            }
        }
        if (targetIdx >= 0)
        {
            var p = monster.parts[targetIdx];
            p.fleshCurrent = Mathf.Min(p.fleshMax, p.fleshCurrent + regenAmount);
            monster.parts[targetIdx] = p;
            Debug.Log($"[MonsterAI] Special: REGEN {regenAmount} on {p.partName} " +
                      $"→ {p.fleshCurrent}/{p.fleshMax}");
        }
        return result;
    }

    // PINNED — apply Pinned to all adjacent hunters
    if (card.specialTag == "PINNED")
    {
        var adjacent = GetHuntersWithinRange(state, monCell, 1);
        foreach (var h in adjacent)
        {
            var effects = new List<string>(h.activeStatusEffects ?? new string[0]);
            if (!effects.Contains("Pinned")) effects.Add("Pinned");
            h.activeStatusEffects = effects.ToArray();
            Debug.Log($"[MonsterAI] Special: {h.hunterName} gains Pinned");
        }
        return result;
    }

    // AGGRO:LOWEST — move aggro to hunter with lowest total flesh
    if (card.specialTag == "AGGRO:LOWEST")
    {
        var lowest = FindHunterWithLowestFlesh(state);
        if (lowest != null)
        {
            state.aggroHolderId = lowest.hunterId;
            Debug.Log($"[MonsterAI] Special: aggro moved to {lowest.hunterName} (lowest flesh)");
        }
        return result;
    }

    // STUN_SELF — mark monster to skip next card (CombatManager reads this via specialTag)
    if (card.specialTag == "STUN_SELF")
    {
        monster.currentStanceTag = "STUNNED";
        Debug.Log("[MonsterAI] Special: STUN_SELF — monster enters STUNNED stance");
        return result;
    }

    Debug.LogWarning($"[MonsterAI] Special tag '{card.specialTag}' unhandled");
    return result;
}
```

---

## Part 8: Update CombatManager — Inject Grid and Process Result

Open `Assets/_Game/Scripts/Core.Systems/CombatManager.cs`.

**In `InitializeMonsterAI()`**, after `SetMonsterAI(ai)`, inject the grid:

```csharp
ai.InjectGrid(_gridManager as IGridManager);
```

**Replace `RunMonsterPhase()`** entirely:

```csharp
private void RunMonsterPhase()
{
    if (_monsterAI == null)
    {
        Debug.LogWarning("[MonsterPhase] IMonsterAI not assigned — skipping");
        return;
    }

    // Skip if monster is stunned
    if (CurrentState.monster.currentStanceTag == "STUNNED")
    {
        CurrentState.monster.currentStanceTag = "";
        Debug.Log("[MonsterPhase] Monster was STUNNED — skipping action, clearing stun");
        return;
    }

    var card = _monsterAI.DrawNextCard();
    Debug.Log($"[MonsterPhase] Executing: {card.cardName}");
    OnBehaviorCardActivated?.Invoke(card.cardName);

    var result = _monsterAI.ExecuteCard(card, CurrentState);

    // Process movement — update GridManager occupancy
    if (result.monsterMoved && _gridManager != null)
    {
        string monsterId = CurrentState.monster.monsterName;
        (_gridManager as IGridManager).MoveOccupant(monsterId, result.newMonsterCell);
        Debug.Log($"[MonsterPhase] GridManager updated — monster at " +
                  $"({result.newMonsterCell.x},{result.newMonsterCell.y})");
    }

    // Process hits — fire events and check collapse
    foreach (var hit in result.hits)
    {
        OnDamageDealt?.Invoke(hit.hunterId, hit.damage, DamageType.Flesh);
        var hunter = GetHunter(hit.hunterId);
        if (hunter != null) CheckHunterCollapse(hunter);
    }

    // Special tag side-effects visible to UI
    if (result.specialFired)
        Debug.Log($"[MonsterPhase] Special resolved: {result.specialTag}");
}
```

---

## Part 9: Update the Gaunt Standard MonsterSO

Open the Gaunt Standard MonsterSO asset (`Assets/_Game/Data/Monsters/Gaunt/`). For each behavior card in the deck, fill in the new execution fields. This is the mock combat scenario — these values must be set for verification to work.

Set at minimum on 3–4 cards:

| Card | movementPattern | movementDistance | attackTargetType | attackDamage |
|---|---|---|---|---|
| Any "Creep/Advance" card | Approach | 1 | None | 0 |
| Any "Lunge/Strike" card | None | 0 | AggroTarget | 2 |
| Any "Sweep/Flail" card | None | 0 | AllAdjacent | 1 |
| Any "Stillness/Pause" card | None | 0 | None | 0 |

Leave `specialTag` empty for now — wired in Stage 9F+ when full decks are built.

---

## Verification Test

- [ ] Zero compile errors after all changes
- [ ] Play combat vs Gaunt Standard — Monster Phase auto-advances (1.5s delay)
- [ ] Console shows `[MonsterAI] ExecuteCard: [card name]` each Monster Phase
- [ ] `[MonsterAI] EvaluateTrigger: Always — met` for basic cards
- [ ] Monster with movementPattern=Approach moves closer to Aldric — gridX/Y changes in Inspector
- [ ] `[MonsterPhase] GridManager updated — monster at (X,Y)` logged after move
- [ ] Monster with attackTargetType=AggroTarget deals damage — body zone fleshCurrent drops in Inspector
- [ ] `[Combat] OnDamageDealt` event fires — UI shows damage (RefreshAll called)
- [ ] Hunter at 0 flesh on Head or Torso → collapses — `*** COLLAPSED ***` in Console
- [ ] `AGGRO:LOWEST` special — aggroHolderId changes to hunter with least flesh
- [ ] `STANCE:X` special — monster.currentStanceTag set in Inspector
- [ ] `STUN_SELF` — monster currentStanceTag shows "STUNNED"; next Monster Phase skipped and cleared
- [ ] EvaluateTrigger: draw a card with `triggerCondition = "Aggro target is adjacent"` — returns true when Aldric is 1 cell away, false when 5 cells away
- [ ] No NullReferenceException when _grid is injected correctly

---

## Next Session

**File:** `_Docs/Stage_08/STAGE_08_M.md`
**Covers:** Monster Health Rework — the shell/flesh HP system and body part arrays are replaced with a behavior-deck-as-life model and a wound location deck; this is the data model overhaul that supersedes the execution fields added in this session
