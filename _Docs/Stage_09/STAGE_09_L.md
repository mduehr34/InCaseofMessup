<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 9-L | The Ironhide Full Monster Design
Status: Stage 9-K complete. Gilded Serpent done.
Task: The creature referenced in the codex as "The Spite" is
officially named "The Ironhide." Confirm this in the CodexEntrySO
asset Codex_TheSpite (rename the entryTitle to "The Ironhide").
Then build the complete Ironhide monster: a heavily armoured,
territorial ambush predator. Full lore, 5 parts, 16 behavior cards,
MonsterSO. The Ironhide excels at punishing aggressive hunters.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_09/STAGE_09_L.md
- Assets/_Game/Data/Codex/Codex_TheSpite.asset (update entryTitle)
- Assets/_Game/Scripts/Core.Data/MonsterSO.cs
- Assets/_Game/Scripts/Core.Data/BehaviorCardSO.cs

Then confirm:
- Codex_TheSpite.asset entryTitle is updated to "The Ironhide"
  (entryId stays "CodexEntry_TheSpite" for backward compatibility)
- The Ironhide's combat identity: rewards patience, punishes
  overcommitting attacks
- Counterattack mechanic: if a hunter attacks the Ironhide and
  misses, the Ironhide immediately deals 1 Flesh damage to them
- What you will NOT build (Ironhide sprite — separate art session)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 9-L: The Ironhide Full Monster Design

**Resuming from:** Stage 9-K complete — Gilded Serpent done
**Done when:** Codex_TheSpite.asset updated with correct title; Ironhide MonsterSO with 16 cards; counterattack mechanic fires on hunter misses
**Commit:** `"9L: The Ironhide monster — counterattack mechanic, 16 behavior cards, codex fix"`
**Next session:** STAGE_09_M.md

---

## Step 0: Update the Codex Entry

Open `Assets/_Game/Data/Codex/Codex_TheSpite.asset` in Unity Inspector and change:
- **entryTitle:** `The Spite` → `The Ironhide`
- **bodyText:** Update to match new lore below

**New bodyText:**
```
"We keep finding evidence of it but never the creature itself.
The bait untouched. Prints the size of a hunter's torso.
Something has been watching us claim this land.
It does not appear concerned.
We are calling it the Ironhide now. The old name felt too personal."
```

Leave `entryId` as `CodexEntry_TheSpite` — changing it would break any save files that reference it.

---

## The Ironhide — Monster Design

**Name:** The Ironhide
**Type:** Armoured Beast
**Tier:** Standard (hunted Years 7–15)
**Difficulty:** Standard / Veteran

**Lore:** The Ironhide does not pursue. It observes. It has been studying the settlement's hunting patterns for longer than the settlement has existed — the settlers are certain of this because the cave drawings they found (Year 9 codex entry) show a creature matching its description alongside figures that predate the first settlers by a century. It is territorial, not aggressive. Hunters who enter its territory experience this as the same thing.

**Combat Identity:**
- Very high Shell HP across all parts — attrition-focused
- Counterattack on hunter miss: if a hunter attacks the Ironhide and misses, it immediately deals 1 Flesh damage to that hunter (reactive)
- Punishes reckless aggression; rewards methodical play
- Slow movement — positioning is its weakness
- Once hunters break through shell, flesh is moderate

**Counterattack Mechanic:**
Add to `CombatManager.ResolveHunterAttack()`:

```csharp
if (!hit && _activeMonster != null && _activeMonster.monsterName == "The Ironhide")
{
    Debug.Log($"[Combat] Ironhide Counterattack — {hunter.hunterName} missed and takes 1 Flesh.");
    ApplyDirectDamage(hunter.hunterId, 1);
    // Add Shaken if hunter has no remaining Grit
    if (GameStateManager.Instance.GetEffectiveStats(hunter).grit <= 0)
        _statusSystem.ApplyEffect(hunter.hunterId, "Shaken", 1);
}
```

This counterattack fires immediately, before the next card draw.

---

## Parts

| Part | Shell HP | Flesh HP | Break Effect |
|---|---|---|---|
| Iron Skull | 8 | 6 | Breaking removes Counterattack from all head attacks |
| Iron Chest | 10 | 8 | Primary target — highest shell in the game |
| Iron Flank (Left) | 6 | 6 | Breaking reduces Ironhide's Counterattack damage to 0 (Left side open) |
| Iron Flank (Right) | 6 | 6 | Breaking reduces Counterattack damage to 0 (Right side open) |
| Iron Haunch | 5 | 7 | Breaking slows the Ironhide to 1 move per card (cannot use 2-move cards) |

Note: Both flanks broken = Counterattack disabled entirely.

---

## 16 Behavior Cards

Create in `Assets/_Game/Data/Monsters/Ironhide/BehaviorCards/`.

### Movement Cards (3)

**IH-B01 — Territorial Step**
```
movementEffect: Move 1 space toward aggro target.
attackEffect: —
specialEffect: —
```

**IH-B02 — Charge**
```
movementEffect: Move 2 spaces toward aggro target.
attackEffect: 2 Flesh damage to aggro target if adjacent after movement.
specialEffect: —
```

**IH-B03 — Reposition**
```
movementEffect: Move 1 space to maximize adjacency to the most hunters.
attackEffect: —
specialEffect: The Ironhide re-centres itself in the fight.
```

---

### Attack Cards (5)

**IH-B04 — Iron Slam**
```
movementEffect: No movement.
attackEffect: 4 Flesh damage to aggro target.
specialEffect: —
```

**IH-B05 — Iron Sweep**
```
movementEffect: No movement.
attackEffect: 2 Flesh damage to all adjacent hunters.
specialEffect: Each hunter hit is pushed 1 space away (Knockback 1).
```

**IH-B06 — Focused Strike**
```
movementEffect: Move 1 toward aggro.
attackEffect: 5 Flesh damage to aggro target. Cannot be reduced by armour.
specialEffect: isShuffle: true
```

**IH-B07 — Skull Crash**
```
triggerCondition: Iron Skull is intact
movementEffect: No movement.
attackEffect: 3 Flesh damage to aggro target.
specialEffect: Target gains Shaken. If target is already Shaken: +2 damage.
  If Skull broken: treat as Iron Slam.
```

**IH-B08 — Pin Down**
```
movementEffect: No movement.
attackEffect: 2 Flesh damage to aggro target.
specialEffect: Aggro target gains Pinned for 1 round.
```

---

### Special Cards (6)

**IH-B09 — Iron Wall**
```
movementEffect: No movement.
attackEffect: —
specialEffect: Until the next card draw: all attacks on the Ironhide's
  Chest deal −1 damage (minimum 0). It has raised its most armoured side.
```

**IH-B10 — Territorial Growl**
```
movementEffect: No movement.
attackEffect: —
specialEffect: All hunters gain −1 Accuracy for 1 round.
  Hunters who are adjacent to the Ironhide also gain Shaken.
```

**IH-B11 — Observation**
```
movementEffect: No movement.
attackEffect: —
specialEffect: Aggro shifts to the hunter who has made the most attack
  attempts this round (most aggressive hunter). The Ironhide will focus
  the hunter pressing it hardest.
```

**IH-B12 — Iron Resolve**
```
movementEffect: No movement.
attackEffect: —
specialEffect: The Ironhide's Iron Chest regenerates 2 Shell HP (capped at max).
  This represents its passive hardening.
```

**IH-B13 — Reactive Stance**
```
movementEffect: No movement.
attackEffect: —
specialEffect: Until the next card draw: Counterattack damage increases
  to 2 (instead of 1) on any hunter miss. The Ironhide is actively
  punishing aggressive attacks.
```

**IH-B14 — Crush**
```
triggerCondition: Aggro target is adjacent and has Pinned.
movementEffect: No movement.
attackEffect: 6 Flesh damage to aggro target.
  This attack cannot be Evaded.
specialEffect: If trigger not met: treat as Iron Slam.
  isShuffle: true
```

---

### Passive / Skip Cards (2)

**IH-B15 — Wait**
```
movementEffect: No movement.
attackEffect: —
specialEffect: The Ironhide does not move or attack. It is waiting for
  the hunters to overextend. Aggro remains on current target.
```

**IH-B16 — Survey Threat**
```
movementEffect: No movement.
attackEffect: —
specialEffect: Aggro shifts to the hunter with the highest combined
  Accuracy + Speed (the most dangerous-seeming target).
  isShuffle: true
```

---

## Ironhide MonsterSO Asset

Create `Assets/_Game/Data/Monsters/Ironhide/Ironhide_Standard.asset`:

```
monsterName: The Ironhide
monsterType: Armoured Beast
difficulty: Standard
huntYearMin: 7
huntYearMax: 15

Parts:
  Iron Skull: shellHP=8, fleshHP=6
  Iron Chest: shellHP=10, fleshHP=8
  Iron Flank (Left): shellHP=6, fleshHP=6
  Iron Flank (Right): shellHP=6, fleshHP=6
  Iron Haunch: shellHP=5, fleshHP=7

behaviorDeck: [IH-B01 through IH-B16]
startingAggroTarget: Hunter with highest combined Accuracy+Speed
```

---

## Verification Test

- [ ] `Codex_TheSpite.asset` entryTitle shows "The Ironhide" in Inspector
- [ ] Ironhide MonsterSO with 5 parts and 16 cards
- [ ] Hunter misses an attack against Ironhide → `[Combat] Ironhide Counterattack` logged
- [ ] Counterattack deals 1 Flesh to the missing hunter
- [ ] Both Iron Flanks broken → no more Counterattack (0 damage on miss)
- [ ] IH-B13 (Reactive Stance) active → missed attack deals 2 Flesh instead of 1
- [ ] IH-B12 (Iron Resolve) → Iron Chest shell HP restored by 2, capped
- [ ] IH-B09 (Iron Wall) → all Chest attacks deal −1 damage for 1 round
- [ ] IH-B14 (Crush) fires only when aggro target is adjacent AND Pinned
- [ ] Iron Haunch broken → Ironhide can only move 1 space (IH-B02 becomes IH-B01)
- [ ] Ironhide combat with all flanks and skull intact → feels appropriately punishing

---

## Next Session

**File:** `_Docs/Stage_09/STAGE_09_M.md`
**Covers:** Overlord — The Siltborn. Full overlord monster design: 6-part boss with multi-phase behavior deck, unique defeat condition (kill all three nodes simultaneously), and campaign-level reward (unlocks the Siltborn codex entry and a craft set).
