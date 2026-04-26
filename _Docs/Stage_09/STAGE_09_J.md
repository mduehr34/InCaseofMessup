<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 9-J | The Rotmother Full Monster Design
Status: Stage 9-I complete. The Shriek monster done.
Task: Create the full Rotmother monster — a slow, massive late-game
creature that spawns Rot Spawn minions each round and uses a
Corruption mechanic to permanently damage hunters who let too many
Spawn survive. Design lore, 5 parts, 16 behavior cards, and the
MonsterSO. Implement the Rot Spawn minion spawning system.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_09/STAGE_09_J.md
- Assets/_Game/Scripts/Core.Data/MonsterSO.cs
- Assets/_Game/Scripts/Core.Data/BehaviorCardSO.cs
- Assets/_Game/Scripts/Core.Systems/CombatManager.cs

Then confirm:
- Rot Spawn are simple minions with 1 HP — represented as
  tokens on the grid with no behavior deck of their own
- Corruption is a campaign-level consequence: if 3+ Spawn
  survive a hunt, one random hunter gains a Disorder
- Spawning is tracked in CombatState.activeSpawnCount
- What you will NOT build (minion sprites — separate art session)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 9-J: The Rotmother Full Monster Design

**Resuming from:** Stage 9-I complete — The Shriek monster done
**Done when:** Rotmother MonsterSO exists with 16 cards; Rot Spawn minion system works; Corruption consequence fires post-hunt when 3+ Spawn survive
**Commit:** `"9J: The Rotmother monster — spawn minions, corruption mechanic, 16 behavior cards"`
**Next session:** STAGE_09_K.md

---

## The Rotmother — Monster Design

**Name:** The Rotmother
**Type:** Beast (Massive)
**Tier:** Standard (hunted Years 8–14)
**Difficulty:** Nightmare recommended

**Lore:** The rot does not spread from the Rotmother. The rot is what the Rotmother eats. It moves through blighted ground, its mass sustained by slow decomposition of everything within reach. The settlers noticed the pattern three seasons before they understood it: wherever the Rotmother had been, nothing grew the following year. They have been burning the ground after every hunt since then. They are not sure it helps.

**The Spawn:** Small, fast, partially-formed creatures that emerge from the Rotmother's side-mass. They have no intelligence — they move toward the nearest living thing and deal 1 damage if they reach it. They are not dangerous individually. The problem is that each one that survives the hunt leaves a trace of corruption in the hunters who were near it.

**Combat Identity:**
- Slow but massive (occupies 2×2 cells — centred on grid)
- Spawns 1 Rot Spawn per round (or 2 per round after mid-HP)
- Spawn move toward the nearest hunter and deal 1 Flesh damage if adjacent
- Hunters must split attention between the Rotmother and the Spawn
- Corruption: if 3+ Spawn survive the hunt (i.e., Rotmother killed before all Spawn killed), one hunter gains a random Disorder
- High HP pool — a war of attrition

**Rot Spawn Rules:**
- 1 HP each (one hit from any attack kills them)
- Spawn tokens are placed on cells adjacent to the Rotmother
- Spawn move 1 space toward the nearest hunter each monster phase
- Spawn deal 1 automatic Flesh damage if adjacent to a hunter at end of monster phase
- Players can attack Spawn (treat as a "part" with 0 Shell, 1 Flesh)
- Track active Spawn in `CombatState.activeSpawnCount`
- Track Spawn that survived to hunt end in `CombatState.survivingSpawnCount`

---

## CombatState — Add Spawn Tracking

```csharp
// Add to CombatState:
public int activeSpawnCount;       // Currently alive on the grid
public int survivingSpawnCount;    // Spawn alive when Rotmother dies (for corruption)
```

---

## Rot Spawn Minion System

In `CombatManager`, add:

```csharp
public void SpawnRotSpawn(int count)
{
    for (int i = 0; i < count; i++)
    {
        _combatState.activeSpawnCount++;
        // Find a cell adjacent to the Rotmother and place a spawn token
        var spawnPos = FindAdjacentEmptyCell(_monsterGridPos);
        if (spawnPos.HasValue)
        {
            PlaceSpawnToken(spawnPos.Value);
            Debug.Log($"[Combat] Rot Spawn spawned at {spawnPos.Value}. " +
                      $"Active: {_combatState.activeSpawnCount}");
        }
    }
}

public void MoveSpawnTokens()
{
    // Each spawn moves toward nearest hunter
    foreach (var spawn in _activeSpawnTokens)
    {
        HunterState nearest = GetClosestHunter(spawn.gridPos);
        if (nearest == null) continue;
        var targetPos = GetHunterPosition(nearest.hunterId);
        spawn.gridPos = StepToward(spawn.gridPos, targetPos);

        if (IsAdjacent(spawn.gridPos, targetPos))
        {
            Debug.Log($"[Combat] Rot Spawn adjacent to {nearest.hunterName} — 1 Flesh damage.");
            ApplyDirectDamage(nearest.hunterId, 1);
        }
    }
}

public void OnSpawnKilled(SpawnToken spawn)
{
    _activeSpawnTokens.Remove(spawn);
    _combatState.activeSpawnCount--;
    Debug.Log($"[Combat] Rot Spawn killed. Remaining: {_combatState.activeSpawnCount}");
}

public void OnRotmotherDefeated()
{
    _combatState.survivingSpawnCount = _combatState.activeSpawnCount;
    Debug.Log($"[Combat] Rotmother defeated. {_combatState.survivingSpawnCount} Spawn survived.");

    if (_combatState.survivingSpawnCount >= 3)
    {
        // Apply corruption to a random living hunter
        var living = GetLivingHunters();
        if (living.Count > 0)
        {
            var target = living[Random.Range(0, living.Count)];
            string[] disorders = { "DIS-01", "DIS-03", "DIS-05", "DIS-07" };
            string disorder    = disorders[Random.Range(0, disorders.Length)];
            GameStateManager.Instance.GrantDisorder(target.hunterId, disorder);
            Debug.Log($"[Combat] Corruption: {target.hunterName} gains {disorder}.");
        }
    }

    OnMonsterDefeated();
}
```

---

## Parts

| Part | Shell HP | Flesh HP | Break Effect |
|---|---|---|---|
| Maw | 3 | 7 | Breaking disables Rot Bite attack |
| Body Mass (Front) | 6 | 12 | Primary target |
| Body Mass (Rear) | 6 | 10 | — |
| Spawn Sac (Left) | 4 | 5 | Breaking reduces spawning: only 1 spawn max per round |
| Spawn Sac (Right) | 4 | 5 | Breaking reduces spawning: only 1 spawn max per round (stacks) |

Both Spawn Sacs broken = no more spawning; only the Rotmother itself remains.

---

## 16 Behavior Cards

Create in `Assets/_Game/Data/Monsters/Rotmother/BehaviorCards/`.

### Movement Cards (3)

**ROM-B01 — Ponderous Advance**
```
movementEffect: Move 1 space toward the aggro target.
attackEffect: —
specialEffect: All hunters adjacent to the Rotmother's new position
  gain Slowed (the ground shifts around it).
```

**ROM-B02 — Rot Surge**
```
movementEffect: Move 2 spaces toward aggro target.
attackEffect: 2 Flesh damage to all hunters in the path of movement.
specialEffect: —
```

**ROM-B03 — Settle**
```
movementEffect: No movement.
attackEffect: —
specialEffect: The Rotmother roots itself this round. All Rot Spawn
  gain +1 move (move 2 instead of 1). Spawn adjacent to hunters deal
  2 Flesh damage instead of 1.
```

---

### Attack Cards (5)

**ROM-B04 — Rot Bite**
```
triggerCondition: Maw is intact
movementEffect: No movement.
attackEffect: 4 Flesh damage to aggro target.
specialEffect: Target gains Poison (2 rounds).
  If Maw is broken: no effect (treat as Spawn Phase).
```

**ROM-B05 — Crush**
```
movementEffect: No movement.
attackEffect: 3 Flesh damage to all hunters adjacent to the Rotmother.
specialEffect: Knockback 1 space for each hit hunter.
```

**ROM-B06 — Rot Slam**
```
movementEffect: Move 1 toward aggro.
attackEffect: 5 Flesh damage to aggro target.
specialEffect: isShuffle: true
```

**ROM-B07 — Sweeping Mass**
```
movementEffect: Rotate 90 degrees in place (facing changes).
attackEffect: 2 Flesh damage to all hunters in the swept arc.
specialEffect: Spawn in the swept arc are also killed
  (the Rotmother doesn't care about them).
```

**ROM-B08 — Engulf**
```
triggerCondition: Aggro target is adjacent.
movementEffect: No movement.
attackEffect: 3 Flesh damage to aggro target. Target gains Pinned.
specialEffect: The target takes +1 Flesh damage from the next Spawn
  that reaches them (they're held in place).
```

---

### Spawn / Special Cards (6)

**ROM-B09 — Spawn Phase**
```
movementEffect: No movement.
attackEffect: —
specialEffect: Spawn 2 Rot Spawn tokens adjacent to the Rotmother.
  If both Spawn Sacs are broken: spawn 0. If one Sac broken: spawn 1.
```

**ROM-B10 — Mass Spawn**
```
triggerCondition: Rotmother's total Flesh is below 50%
movementEffect: No movement.
attackEffect: —
specialEffect: Spawn 3 Rot Spawn tokens. If Spawn Sacs are broken,
  reduce count by 1 per broken sac. isShuffle: true
```

**ROM-B11 — Corruption Pulse**
```
movementEffect: No movement.
attackEffect: —
specialEffect: Each hunter within 3 spaces loses 1 Grit permanently
  (this is the one permanent-effect card — log it and apply to HunterState).
  This effect fires only if at least 1 Spawn is alive on the grid.
```

**ROM-B12 — Accelerate Rot**
```
movementEffect: No movement.
attackEffect: All hunters who have Poison take 1 additional Flesh damage.
specialEffect: Each Poisoned hunter's Poison duration is extended by 1 round.
```

**ROM-B13 — Spawn and Strike**
```
movementEffect: No movement.
attackEffect: 2 Flesh damage to aggro target.
specialEffect: Spawn 1 Rot Spawn. Then all existing Spawn move 1 toward
  their nearest target.
```

**ROM-B14 — Devour Spawn**
```
movementEffect: No movement.
attackEffect: —
specialEffect: Remove all active Rot Spawn tokens (the Rotmother absorbs them).
  For each Spawn removed: Rotmother heals 2 Flesh HP (to its lowest HP part).
  Update activeSpawnCount accordingly.
```

---

### Passive / Skip Cards (2)

**ROM-B15 — Patience**
```
movementEffect: No movement.
attackEffect: —
specialEffect: Aggro shifts to the hunter with the most accumulated damage
  dealt to the Rotmother this hunt (most threatening hunter).
```

**ROM-B16 — Mass Stir**
```
movementEffect: No movement.
attackEffect: —
specialEffect: All Rot Spawn on the field move 1 extra space this round
  (in addition to their normal movement). Any Spawn that reach adjacency
  deal 2 Flesh damage instead of 1. isShuffle: true
```

---

## Rotmother MonsterSO Asset

Create `Assets/_Game/Data/Monsters/Rotmother/Rotmother_Nightmare.asset`:

```
monsterName: The Rotmother
monsterType: Massive Beast
difficulty: Nightmare
huntYearMin: 8
huntYearMax: 14

Parts:
  Maw: shellHP=3, fleshHP=7
  Body Mass (Front): shellHP=6, fleshHP=12
  Body Mass (Rear): shellHP=6, fleshHP=10
  Spawn Sac (Left): shellHP=4, fleshHP=5
  Spawn Sac (Right): shellHP=4, fleshHP=5

behaviorDeck: [ROM-B01 through ROM-B16]
startingAggroTarget: Hunter with lowest Speed
```

---

## Verification Test

- [ ] Rotmother MonsterSO asset exists with all 5 parts and 16 cards
- [ ] Each monster phase: `SpawnRotSpawn()` called from ROM-B09 → spawn tokens appear on grid
- [ ] Spawn tokens move 1 space per round toward nearest hunter
- [ ] Spawn adjacent to hunter → 1 Flesh damage applied, logged
- [ ] Hunter attacks spawn → spawn removed in 1 hit
- [ ] `CombatState.activeSpawnCount` tracks correctly (goes up on spawn, down on kill)
- [ ] ROM-B14 (Devour Spawn) fires → all spawn tokens removed; Rotmother heals
- [ ] Both Spawn Sacs broken → ROM-B09 spawns 0 tokens
- [ ] Rotmother defeated with 4 Spawn alive → random living hunter gains a random Disorder
- [ ] Rotmother defeated with 2 Spawn alive → no Disorder (below 3 threshold)
- [ ] ROM-B11 (Corruption Pulse) fires → Grit −1 permanently applied to nearby hunters
- [ ] No null ref if no Spawn are alive when Mass Stir fires

---

## Next Session

**File:** `_Docs/Stage_09/STAGE_09_K.md`
**Covers:** The Gilded Serpent — a large mid-tier serpent monster with constrict, venom, and a golden-scale armour mechanic. Full design with 16 behavior cards and MonsterSO.
