<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 9-K | The Gilded Serpent Full Monster Design
Status: Stage 9-J complete. Rotmother monster done.
Task: Create the full Gilded Serpent monster — a large serpentine
creature with golden scale plating that reflects certain attacks
and reduces damage until the scales are stripped. Design lore,
5 breakable parts, 16 behavior cards, and the MonsterSO. Implement
the scale-reflection mechanic.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_09/STAGE_09_K.md
- Assets/_Game/Scripts/Core.Data/MonsterSO.cs
- Assets/_Game/Scripts/Core.Data/BehaviorCardSO.cs
- Assets/_Game/Scripts/Core.Systems/CombatManager.cs

Then confirm:
- Scale Reflection: when Shell HP on Neck or Mid-Body is above 50%,
  attacks for 1 damage or less are reflected back at the attacker
  (they take 1 Flesh damage instead)
- Constrict is a special status on a hunter: they take 1 Flesh
  damage at start of each turn and cannot use movement cards
- Venom is an upgraded version of Poison: 2 Flesh damage per tick
  instead of 1, for 3 rounds
- What you will NOT build (serpent sprite — separate art session)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 9-K: The Gilded Serpent Full Monster Design

**Resuming from:** Stage 9-J complete — The Rotmother monster done
**Done when:** Gilded Serpent MonsterSO with 16 cards; Scale Reflection mechanic works; Constrict and Venom status effects resolve correctly
**Commit:** `"9K: Gilded Serpent monster — scale reflection, constrict, venom, 16 cards"`
**Next session:** STAGE_09_L.md

---

## The Gilded Serpent — Monster Design

**Name:** The Gilded Serpent
**Type:** Serpent
**Tier:** Standard (hunted Years 6–12)
**Difficulty:** Standard / Veteran

**Lore:** The scales are not decoration. The settlers who first encountered the Gilded Serpent sent back a scout with a hand missing — "it reflected the bolt back," she said, and no one believed her until the second expedition. The gold plates on its outer coils are fused bone and crystallized marrow, layered over decades. It is, in a sense, wearing armour it grew itself. The scales dull with age. Younger specimens are nearly impenetrable. The ancient ones are more dangerous for entirely different reasons.

**Scale Reflection Mechanic:**
When the Neck Shell or Mid-Body Shell is above 50% of its maximum HP, any attack that would deal 1 or less Flesh damage to that part is **reflected**: the attacker takes 1 Flesh damage instead. The attack deals no damage to the Serpent.

Add to `CombatManager.ResolveHunterAttack()`:

```csharp
private bool CheckScaleReflection(MonsterPartState part, int damage, string hunterId)
{
    if (part.partName != "Neck" && part.partName != "Mid-Body") return false;
    if (part.currentShellHP <= part.maxShellHP / 2) return false;  // Scales damaged enough
    if (damage > 1) return false;   // Only reflects weak hits

    Debug.Log($"[Combat] Scale Reflection! {damage} damage reflected back to {hunterId}.");
    ApplyDirectDamage(hunterId, 1);
    return true;   // Return true = attack was reflected, deal no damage to monster
}
```

---

## Constrict Status Effect

Add to `StatusEffectSystem`:

```csharp
// Constrict: hunter cannot use movement cards; takes 1 Flesh damage at turn start
// Represented as status "Constrict" with duration tracking
// Applied by GS-B07 (Wrap) card
```

In `CombatManager`, at start of a constricted hunter's turn:
```csharp
if (HasStatus(hunter, "Constrict"))
{
    ApplyDirectDamage(hunter.hunterId, 1);
    Debug.Log($"[Combat] {hunter.hunterName} constricted — 1 Flesh damage, movement blocked.");
    // Block movement card play (flag on hunter runtime state)
}
```

## Venom Status Effect

Venom = Poison with 2× damage per tick:
```csharp
// When Venom is applied: store as "Venom:3" (3 rounds, 2 damage/tick)
// Tick resolution: 2 Flesh damage per round for duration
```

---

## Parts

| Part | Shell HP | Flesh HP | Break Effect |
|---|---|---|---|
| Head | 2 | 6 | Breaking disables Venom Bite; breaks scales over entire body (Reflection disabled) |
| Neck | 5 | 7 | Scale Reflection applies when shell >50% |
| Mid-Body | 6 | 10 | Scale Reflection applies when shell >50%; core target |
| Tail | 3 | 6 | Breaking disables Tail Whip attack |
| Coil | 4 | 5 | Breaking disables Constrict (Wrap) card |

---

## 16 Behavior Cards

Create in `Assets/_Game/Data/Monsters/GildedSerpent/BehaviorCards/`.

### Movement Cards (4)

**GS-B01 — Slither**
```
movementEffect: Move 2 spaces toward aggro target.
attackEffect: —
specialEffect: —
```

**GS-B02 — Coil Advance**
```
movementEffect: Move 1 space toward aggro target.
attackEffect: —
specialEffect: If the Serpent occupies the same column or row as the
  aggro target, move 2 spaces instead.
```

**GS-B03 — Recoil**
```
movementEffect: Move 2 spaces away from the hunter with the highest
  Accuracy. The Serpent avoids ranged threats.
attackEffect: —
specialEffect: —
```

**GS-B04 — Encircle**
```
movementEffect: Move to a cell adjacent to the aggro target
  from the opposite side of their current position.
attackEffect: —
specialEffect: The Serpent has flanked the target. Next attack
  against them ignores Evasion.
```

---

### Attack Cards (5)

**GS-B05 — Venom Bite**
```
triggerCondition: Head is intact
movementEffect: No movement.
attackEffect: 3 Flesh damage to aggro target.
specialEffect: Target gains Venom (2 Flesh damage/round, 3 rounds).
  If Head is broken: treat as Strike instead.
```

**GS-B06 — Strike**
```
movementEffect: No movement.
attackEffect: 4 Flesh damage to aggro target.
specialEffect: —
```

**GS-B07 — Wrap**
```
triggerCondition: Coil is intact
movementEffect: No movement.
attackEffect: 2 Flesh damage to aggro target.
specialEffect: Target gains Constrict (cannot use movement cards;
  takes 1 Flesh damage at start of each of their turns for 2 rounds).
  If Coil is broken: no Constrict applied.
```

**GS-B08 — Tail Whip**
```
triggerCondition: Tail is intact
movementEffect: No movement.
attackEffect: 2 Flesh damage to all hunters behind the Serpent.
specialEffect: Each hit hunter is pushed 2 spaces away (Knockback 2).
  If Tail broken: no effect.
```

**GS-B09 — Crushing Coil**
```
triggerCondition: A hunter is Constricted.
movementEffect: No movement.
attackEffect: 4 Flesh damage to the Constricted hunter.
  This attack cannot be Evaded.
specialEffect: isShuffle: true.
  If no hunter is Constricted: treat as Strike.
```

---

### Special Cards (5)

**GS-B10 — Scale Flex**
```
movementEffect: No movement.
attackEffect: —
specialEffect: The Gilded Serpent's Neck and Mid-Body Shell HP each
  regenerate 1 HP (up to their maximum). This reinforces the reflection
  armour. Log the regeneration.
```

**GS-B11 — Intimidate**
```
movementEffect: No movement.
attackEffect: —
specialEffect: All hunters within 3 spaces gain Shaken and lose −1
  Grit for 1 round. Hunters who are already Constricted also gain Pinned.
```

**GS-B12 — Lunge**
```
movementEffect: Move 3 spaces directly toward aggro target.
attackEffect: 3 Flesh damage to aggro target at end of movement.
specialEffect: If the Serpent moved through any hunter's cell: those
  hunters take 1 Flesh damage (constriction graze).
```

**GS-B13 — Squeeze**
```
triggerCondition: A hunter is Constricted.
movementEffect: No movement.
attackEffect: —
specialEffect: The Constricted hunter's Constrict duration is extended
  by 1 round. That hunter also takes 2 Flesh damage immediately.
```

**GS-B14 — Reflection Stance**
```
movementEffect: No movement.
attackEffect: —
specialEffect: Until the next card draw: all damage dealt to the Serpent's
  Neck or Mid-Body is halved (rounded down). The scales have locked into
  full defensive position.
```

---

### Passive / Skip Cards (2)

**GS-B15 — Survey**
```
movementEffect: No movement.
attackEffect: —
specialEffect: Aggro shifts to the hunter who has dealt the most damage
  to the Serpent this hunt. Retribution targeting.
```

**GS-B16 — Patient Coil**
```
movementEffect: No movement.
attackEffect: —
specialEffect: The Serpent waits. If any hunter has Venom, they take
  1 additional Flesh damage this round (Venom worsens while the Serpent
  observes). isShuffle: true
```

---

## Gilded Serpent MonsterSO Asset

Create `Assets/_Game/Data/Monsters/GildedSerpent/GildedSerpent_Standard.asset`:

```
monsterName: The Gilded Serpent
monsterType: Serpent
difficulty: Standard
huntYearMin: 6
huntYearMax: 12

Parts:
  Head: shellHP=2, fleshHP=6
  Neck: shellHP=5, fleshHP=7
  Mid-Body: shellHP=6, fleshHP=10
  Tail: shellHP=3, fleshHP=6
  Coil: shellHP=4, fleshHP=5

behaviorDeck: [GS-B01 through GS-B16]
startingAggroTarget: Hunter with highest Grit
```

---

## Verification Test

- [ ] Gilded Serpent MonsterSO asset exists with 5 parts and 16 cards
- [ ] Hunter attacks Neck (shell >50%): deal 1 damage → reflected back, hunter takes 1 Flesh
- [ ] Hunter attacks Neck (shell ≤50%): attack lands normally, no reflection
- [ ] GS-B07 (Wrap) fires → hunter gains Constrict status
- [ ] Constricted hunter: takes 1 Flesh at turn start; cannot play movement cards
- [ ] GS-B05 (Venom Bite) fires → hunter gains Venom: 2 Flesh/round for 3 rounds
- [ ] Venom correctly ticks 2 damage (not 1) at each turn start
- [ ] Head broken → GS-B05 has no effect
- [ ] Coil broken → GS-B07 applies no Constrict (but still deals 2 damage)
- [ ] GS-B10 (Scale Flex) → Neck and Mid-Body shell each +1 (capped at max)
- [ ] GS-B09 (Crushing Coil) → fires only if a hunter is Constricted
- [ ] Scale Reflection disabled when Head is broken
- [ ] No console errors when Constrict expires naturally after 2 rounds

---

## Next Session

**File:** `_Docs/Stage_09/STAGE_09_L.md`
**Covers:** The Spite (The Ironhide) — identity decision and full monster design. The Spite is referenced in the codex as "The Ironhide" — confirm the final naming decision and build the complete behavior card set for this mid-to-late tier monster.
