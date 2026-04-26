<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 9-H | Bog Caller Full Monster Design
Status: Stage 9-G complete. Ivory Stampede pack system working.
Task: Create the full Bog Caller monster — lore, 5 breakable parts,
16 behavior cards, and the MonsterSO asset. The Bog Caller is a
mid-tier predator that uses a poisonous mist ability and ambush
tactics. It punishes hunters who cluster together.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_09/STAGE_09_H.md
- Assets/_Game/Scripts/Core.Data/MonsterSO.cs
- Assets/_Game/Scripts/Core.Data/BehaviorCardSO.cs

Then confirm:
- MonsterSO structure is the same as Thornback (not PackMonsterSO)
- Poison is represented as a StatusEffect string in the card's
  specialEffect field — CombatManager applies it
- Mist Zone is a temporary grid modifier stored as a bool[] on
  CombatState (same size as the grid)
- What you will NOT build (Bog Caller sprite — art generation
  session is separate)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 9-H: Bog Caller Full Monster Design

**Resuming from:** Stage 9-G complete — Ivory Stampede pack system working
**Done when:** Bog Caller MonsterSO exists with 16 behavior cards; mist zone mechanic is tracked in CombatState; poison status effect resolves correctly
**Commit:** `"9H: Bog Caller monster — 16 behavior cards, poison mist, MonsterSO"`
**Next session:** STAGE_09_I.md

---

## The Bog Caller — Monster Design

**Name:** The Bog Caller
**Type:** Beast
**Tier:** Standard (hunted Years 4–8)
**Difficulty:** Standard / Veteran

**Lore:** It does not chase. It calls. The Bog Caller moves slowly through the deep mud season, emitting a subsonic resonance that draws prey toward it. By the time settlers identified the pattern, three hunting parties had already walked into range unaware. The mist it exhales is not breath — the settlers who studied the bodies said the fluid in the creature's throat sacs was the same as the ichor they'd been harvesting for months. They stopped harvesting for a while after that.

**Combat Identity:**
- Does not aggressively pursue — instead draws hunters into mist zones
- Mist zones apply Poison to hunters who end their turn inside
- Rewards spread-out positioning; punishes clustering (AoE mist)
- Vulnerable to ranged attacks from outside mist range
- Uses Lure mechanic to pull aggro targets toward it

**Mist Zone Rule:**
When the Bog Caller creates a Mist Zone, mark a 3-cell radius around the monster with the `mistZone` flag in `CombatState`. Any hunter who ends their turn on a mist zone cell gains **Poison** (lose 1 Flesh at the start of each of their next 2 turns). Mist dissipates after 2 rounds.

---

## Add Mist Zone to CombatState

Add to `CombatState` (create this class if it doesn't exist, or add to the appropriate state class):

```csharp
// CombatState.cs — add these fields:
public bool[] mistZone;          // Flat array, same size as grid (width * height)
public int    mistRoundsRemaining;  // Counts down each round

// Helper
public bool IsMistAt(int x, int y, int gridWidth)
{
    int idx = y * gridWidth + x;
    return mistZone != null && idx < mistZone.Length && mistZone[idx];
}
```

In `CombatManager`, after each hunter's turn ends, check mist:

```csharp
private void CheckMistPoisoning(HunterState hunter, Vector2Int pos)
{
    if (_combatState.IsMistAt(pos.x, pos.y, _gridWidth))
    {
        Debug.Log($"[Combat] {hunter.hunterName} ends turn in mist — Poison applied.");
        _statusEffectSystem.ApplyEffect(hunter.hunterId, "Poison", duration: 2);
    }
}
```

---

## Parts

| Part | Shell HP | Flesh HP | Break Effect |
|---|---|---|---|
| Gullet Sac | 3 | 5 | Breaking shell removes Lure ability; breaking flesh disables Mist Create |
| Chest | 5 | 10 | Core target — no special break effect |
| Forelimb (Left) | 3 | 6 | Breaking slows Bog Caller (−1 move per card) |
| Forelimb (Right) | 3 | 6 | Breaking disables Push Back movement |
| Hide Crest (back) | 6 | 4 | Breaking shell triggers Mist Burst (immediate mist zone) |

---

## 16 Behavior Cards

Create in `Assets/_Game/Data/Monsters/BogCaller/BehaviorCards/`.

### Movement Cards (4)

**BC-B01 — Slow Advance**
```
movementEffect: Move 1 space toward the aggro target.
attackEffect: —
specialEffect: —
```

**BC-B02 — Bog Step**
```
movementEffect: Move 1 space. Can enter Mist Zones without penalty.
attackEffect: —
specialEffect: Any hunter the Bog Caller moves through (if occupying same cell) gains Slowed.
```

**BC-B03 — Lure Pull**
```
movementEffect: No movement.
attackEffect: —
specialEffect: The aggro target must move 1 space toward the Bog Caller
  (Lure effect). If Gullet Sac shell is broken, this card has no effect.
```

**BC-B04 — Retreat Mist**
```
movementEffect: Move 2 spaces away from the closest hunter.
attackEffect: —
specialEffect: Create a Mist Zone in the 2 cells vacated. Any hunter
  entering those cells this round gains Poison.
```

---

### Attack Cards (5)

**BC-B05 — Bile Spray**
```
movementEffect: No movement.
attackEffect: 2 Flesh damage to all hunters within 2 spaces.
specialEffect: Each hunter hit must make a Toughness check (roll ≥ 4 on d10);
  failure applies Poison (2 rounds). If Gullet Sac flesh is broken,
  this card has no effect.
```

**BC-B06 — Claw Rake**
```
movementEffect: No movement.
attackEffect: 3 Flesh damage to aggro target.
specialEffect: —
```

**BC-B07 — Slam**
```
movementEffect: Move 1 toward aggro.
attackEffect: 2 Flesh damage to aggro target. Knockback 1 space.
specialEffect: —
```

**BC-B08 — Ensnare**
```
movementEffect: No movement.
attackEffect: 1 Flesh damage to all adjacent hunters.
specialEffect: All adjacent hunters gain Pinned (cannot move next round).
```

**BC-B09 — Rupture**
```
movementEffect: No movement.
attackEffect: 5 Flesh damage to aggro target.
triggerCondition: Aggro target is adjacent and Poisoned.
specialEffect: If trigger not met: treat as Claw Rake instead.
  isShuffle: true
```

---

### Special Cards (5)

**BC-B10 — Mist Create**
```
movementEffect: No movement.
attackEffect: —
specialEffect: Create a Mist Zone in all cells within 3 spaces of the Bog Caller.
  Zone lasts 2 rounds. Any hunter ending their turn in the zone gains Poison (2 rounds).
  If Gullet Sac flesh is broken, this card has no effect.
```

**BC-B11 — Mist Thicken**
```
triggerCondition: A Mist Zone is currently active.
movementEffect: No movement.
attackEffect: All hunters in the Mist Zone take 1 Flesh damage.
specialEffect: Mist Zone duration is extended by 1 round.
  If no Mist Zone is active: treat as Slow Advance.
```

**BC-B12 — Blind Fog**
```
movementEffect: No movement.
attackEffect: —
specialEffect: All hunters gain −2 Accuracy until end of their next turn
  (the fog obscures vision). Does not require a Mist Zone.
```

**BC-B13 — Cluster Punishment**
```
triggerCondition: 2+ hunters are within 2 spaces of each other.
movementEffect: No movement.
attackEffect: 3 Flesh damage to all hunters within 2 spaces of another hunter.
specialEffect: isShuffle: true. If trigger not met: no effect.
```

**BC-B14 — Hunger Call**
```
movementEffect: Move 2 toward the hunter with the most Poison stacks.
attackEffect: If adjacent after movement: 3 Flesh damage.
specialEffect: The Bog Caller gains +1 to all attack damage for 1 round.
```

---

### Passive / Skip Cards (2)

**BC-B15 — Patience**
```
movementEffect: No movement.
attackEffect: —
specialEffect: Aggro token moves to the hunter with the most Flesh HP remaining.
  The Bog Caller will call that hunter in.
```

**BC-B16 — Subsonic Pulse**
```
movementEffect: No movement.
attackEffect: —
specialEffect: All hunters lose 1 Grit until the start of their next turn.
  Any hunter in a Mist Zone also loses 1 Evasion.
  isShuffle: true
```

---

## Bog Caller MonsterSO Asset

Create `Assets/_Game/Data/Monsters/BogCaller/BogCaller_Standard.asset`:

```
monsterName: The Bog Caller
monsterType: Beast
difficulty: Standard
huntYearMin: 4
huntYearMax: 8

Parts:
  Gullet Sac: shellHP=3, fleshHP=5
  Chest: shellHP=5, fleshHP=10
  Forelimb (Left): shellHP=3, fleshHP=6
  Forelimb (Right): shellHP=3, fleshHP=6
  Hide Crest: shellHP=6, fleshHP=4

behaviorDeck: [BC-B01 through BC-B16]
startingAggroTarget: Hunter with lowest Grit
```

---

## Verification Test

- [ ] Bog Caller MonsterSO exists with correct parts
- [ ] 16 behavior card assets in the correct folder, all with non-empty fields
- [ ] BC-B10 (Mist Create) fires → `CombatState.mistZone` cells flagged within 3 spaces
- [ ] Hunter ends turn in mist zone → Poison status applied, logged in Console
- [ ] Poison ticks 1 Flesh damage at start of hunter's next 2 turns
- [ ] After 2 rounds: mist zone clears, no more automatic Poison on entry
- [ ] BC-B03 (Lure Pull) fires → aggro target moves 1 space toward Bog Caller
- [ ] BC-B09 (Rupture) trigger check: only 5 damage if target is adjacent AND Poisoned
- [ ] BC-B13 (Cluster Punishment) does not fire if no two hunters are within 2 spaces
- [ ] Gullet Sac flesh broken → Mist Create and Bile Spray have no effect (log that)
- [ ] No Console errors when Mist Zone is active and then clears

---

## Next Session

**File:** `_Docs/Stage_09/STAGE_09_I.md`
**Covers:** The Shriek — a fast aerial-capable mid-tier monster that uses high evasion, dive-bomb attacks, and fear effects. Full design with 16 behavior cards and MonsterSO.
