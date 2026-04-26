<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 9-Q | Craft Sets Part 2 — Ichor, Auric, Rot & Ivory
Status: Stage 9-P complete. Carapace, Membrane, Mire sets done.
Task: Build all craftable GearSO assets for four remaining craft
sets: Ichor Works, Auric Scales, Rot Garden, and Ivory Hall.
Each set needs 6 items. After this session, the full gear economy
is complete — all 7 sets with 42 total items.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_09/STAGE_09_Q.md
- Assets/_Game/Scripts/Core.Data/GearSO.cs
- Assets/_Game/Data/Gear/ (existing sets from 9-P for reference)

Then confirm:
- Ichor Works: unlocked after killing The Penitent
- Auric Works: unlocked after killing the Gilded Serpent
- Rot Garden: unlocked after killing the Rotmother
- Ivory Hall: unlocked after the Ivory Stampede is killed
- What you will NOT build (gear overlay sprite generation — that
  is a separate art session; reference the sprite filenames but
  leave overlaySprite field unassigned until art is generated)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 9-Q: Craft Sets Part 2 — Ichor, Auric, Rot & Ivory

**Resuming from:** Stage 9-P complete — Carapace, Membrane, and Mire sets done
**Done when:** 24 additional GearSO assets created (6 per new set); all 7 craft sets complete; unlock conditions wired to monster victories
**Commit:** `"9Q: Craft sets part 2 — Ichor Works, Auric Scales, Rot Garden, Ivory Hall"`
**Next session:** STAGE_09_R.md

---

## Unlock Wiring

Add to `CombatManager.OnOverlordDefeated()` and `OnMonsterDefeated()`:

```csharp
// In OnMonsterDefeated (standard monsters):
if (_activeMonster.monsterName == "The Ivory Stampede")
    GameStateManager.Instance.UnlockCraftSet("Ivory");
if (_activeMonster.monsterName == "The Gilded Serpent")
    GameStateManager.Instance.UnlockCraftSet("Auric");
if (_activeMonster.monsterName == "The Rotmother")
    GameStateManager.Instance.UnlockCraftSet("Rot");
```

Overlord unlocks are already handled in `OnOverlordDefeated()` via `rewardCraftSetId`:
- Siltborn → "Mire" (already done in 9-M)
- Penitent → "Ichor" (already done in 9-N)

---

## Set 4: Ichor Works

**Unlocked after killing The Penitent.**
**Flavour:** Armour and weapons built from crystallised ichor — a process that took the settlement's crafters three years to figure out. The result is rigid but surprisingly light.
**Resources used:** Ichor, Bone, Sinew

Create in `Assets/_Game/Data/Gear/Ichor/`.

### ICH-01 — Ichor Helm
```
gearId: GEAR-ICH-01
gearName: Ichor Helm
gearSlot: Head
craftSet: Ichor
recipe: Ichor×4, Bone×2
bonusGrit: +2
bonusToughness: +1
description: "A helm of set ichor resin over a bone frame. Amber-dark.
  Muffles some sounds. Hunters say it helps them focus."
specialEffect: Immune to Fear effects.
```

### ICH-02 — Ichor Cuirass
```
gearId: GEAR-ICH-02
gearName: Ichor Cuirass
gearSlot: Chest
craftSet: Ichor
recipe: Ichor×6, Sinew×3
bonusToughness: +2
bonusGrit: +1
description: "A resin cuirass that hardens on impact. Takes the first hit of any
  hunt harder than the wearer does."
specialEffect: Once per hunt: the first attack that would deal 3+ damage to this
  hunter deals 1 less.
```

### ICH-03 — Ichor Bracers
```
gearId: GEAR-ICH-03
gearName: Ichor Bracers
gearSlot: Arms
craftSet: Ichor
recipe: Ichor×3, Sinew×2
bonusGrit: +1
bonusAccuracy: +1
description: "Resin-coated bracers with sinew tension straps.
  They make the arm feel like part of the weapon."
specialEffect: —
```

### ICH-04 — Ichor Lance
```
gearId: GEAR-ICH-04
gearName: Ichor Lance
gearSlot: Weapon
craftSet: Ichor
recipe: Bone×3, Ichor×3, Sinew×2
bonusAccuracy: +2
bonusGrit: +1
description: "A long thrusting weapon with an ichor-resin tip that hardening
  on contact. Reaches parts that are hard to get to otherwise."
specialEffect: Can attack monster parts that are 2 cells away (not just adjacent).
```

### ICH-05 — Resin Ward (OffHand)
```
gearId: GEAR-ICH-05
gearName: Resin Ward
gearSlot: OffHand
craftSet: Ichor
recipe: Ichor×4, Bone×1
bonusToughness: +2
description: "A thick disc of hardened ichor resin worn on the off-hand.
  Not a shield — more of an absorber."
specialEffect: Reduce all Poison damage by 1 per tick (minimum 0).
```

### ICH-06 — Ichor Vial
```
gearId: GEAR-ICH-06
gearName: Ichor Vial
gearSlot: Accessory
craftSet: Ichor
recipe: Ichor×3
bonusGrit: +2
description: "A small vial of raw ichor worn at the collar. When things go
  very badly, hunters report breaking it and continuing. The mechanism
  is not understood."
specialEffect: Once per campaign: survive a lethal hit with 1 Flesh remaining
  instead of dying. Vial is consumed.
```

---

## Set 5: Auric Scales

**Unlocked after killing the Gilded Serpent.**
**Flavour:** Made from the Gilded Serpent's gold-tinted scale plates. Extremely high accuracy bonuses from the fine-tooled surface. Hunters who wear it are easy to spot — the scales catch light.
**Resources used:** Ivory (repurposed scale material stored as Ivory), Bone, Sinew

Create in `Assets/_Game/Data/Gear/Auric/`.

### AUR-01 — Scale Visor
```
gearId: GEAR-AUR-01
gearName: Scale Visor
gearSlot: Head
craftSet: Auric
recipe: Ivory×3, Sinew×1
bonusAccuracy: +2
bonusLuck: +1
description: "A gold-tinted scale visor with fine-ground lenses over the eyes.
  Everything looks slightly different through it. More precise."
specialEffect: —
```

### AUR-02 — Scale Coat
```
gearId: GEAR-AUR-02
gearName: Scale Coat
gearSlot: Chest
craftSet: Auric
recipe: Ivory×5, Bone×2
bonusAccuracy: +2
bonusEvasion: +1
description: "A coat of overlapping scale plates. Heavy but flexible.
  The gold catches light in ways that confuse. Hunters use this."
specialEffect: Monsters targeting this hunter must reroll their first
  attack check each round (the glare disorients them).
```

### AUR-03 — Scale Gloves
```
gearId: GEAR-AUR-03
gearName: Scale Gloves
gearSlot: Arms
craftSet: Auric
recipe: Ivory×2, Sinew×2
bonusAccuracy: +2
description: "Fine scale-plated gloves. The fingertips are precise.
  Hunters who wear them report feeling everything."
specialEffect: —
```

### AUR-04 — Scale Blade
```
gearId: GEAR-AUR-04
gearName: Scale Blade
gearSlot: Weapon
craftSet: Auric
recipe: Ivory×4, Bone×2, Sinew×1
bonusAccuracy: +3
bonusLuck: +1
description: "A blade edged with ground serpent scales. Holds an edge
  indefinitely. The settlers are still learning how to make more."
specialEffect: Reroll any attack that rolls a 1 (natural fumble).
```

### AUR-05 — Scale Buckler (OffHand)
```
gearId: GEAR-AUR-05
gearName: Scale Buckler
gearSlot: OffHand
craftSet: Auric
recipe: Ivory×3, Bone×1
bonusEvasion: +2
bonusLuck: +1
description: "A small scale-edged buckler. The reflected light from
  its surface distracts attackers at the last moment."
specialEffect: Once per hunt: cause a monster attack to miss automatically.
```

### AUR-06 — Scale Charm
```
gearId: GEAR-AUR-06
gearName: Scale Charm
gearSlot: Accessory
craftSet: Auric
recipe: Ivory×2
bonusLuck: +2
bonusAccuracy: +1
description: "A single serpent scale worn as a pendant. 
  Hunters who carry them tend to find themselves in the right place at the right time.
  Settlers who don't carry them note this enviously."
specialEffect: —
```

---

## Set 6: Rot Garden

**Unlocked after killing The Rotmother.**
**Flavour:** Built from the Rotmother's organic material — the parts that didn't decompose immediately. Strange, slightly warm, and with a smell that doesn't go away.
**Resources used:** RotGland, Membrane, Hide

Create in `Assets/_Game/Data/Gear/Rot/`.

### ROT-01 — Rot Mask
```
gearId: GEAR-ROT-01
gearName: Rot Mask
gearSlot: Head
craftSet: Rot
recipe: RotGland×3, Hide×1
bonusLuck: +2
bonusGrit: +1
description: "A sealed mask of treated rot-matter. Nothing gets through
  the filter. Nothing. The smell is permanent."
specialEffect: Immune to Venom. Immune to Rot Spawn corruption at hunt end.
```

### ROT-02 — Rot-Weave Coat
```
gearId: GEAR-ROT-02
gearName: Rot-Weave Coat
gearSlot: Chest
craftSet: Rot
recipe: RotGland×4, Membrane×3
bonusLuck: +2
bonusToughness: +1
description: "A coat woven from rot-treated membrane fibre.
  Extremely resistant to biological damage. Smells of marshland and old things."
specialEffect: At the start of each round: 20% chance (roll 8+ on d10) to remove
  one Poison or Bleed tick automatically.
```

### ROT-03 — Rot Wraps
```
gearId: GEAR-ROT-03
gearName: Rot Wraps
gearSlot: Arms
craftSet: Rot
recipe: RotGland×2, Membrane×2
bonusLuck: +1
bonusGrit: +1
description: "Forearm wraps of rot-treated membrane.
  They seem to stabilise shaking. Useful after a hard hit."
specialEffect: —
```

### ROT-04 — Blight Spear
```
gearId: GEAR-ROT-04
gearName: Blight Spear
gearSlot: Weapon
craftSet: Rot
recipe: Bone×3, RotGland×3, Membrane×1
bonusAccuracy: +1
bonusLuck: +2
description: "A spear whose tip has been treated in multiple rot extracts.
  Hits that land leave the target visibly different. Hunters try not to look."
specialEffect: On a hit: target part loses 1 Shell HP (the rot eats into armour).
```

### ROT-05 — Rot Shield (OffHand)
```
gearId: GEAR-ROT-05
gearName: Rot Shield
gearSlot: OffHand
craftSet: Rot
recipe: RotGland×3, Hide×3
bonusToughness: +1
bonusLuck: +2
description: "A shield whose surface is treated rot-matter.
  Attacking it is unpleasant for the attacker."
specialEffect: When blocking (successful Evasion): attacker takes 1 Flesh damage
  (contact with the rot surface).
```

### ROT-06 — Rot Seed
```
gearId: GEAR-ROT-06
gearName: Rot Seed
gearSlot: Accessory
craftSet: Rot
recipe: RotGland×2
bonusLuck: +3
description: "A small mass of compressed rot-matter worn at the belt.
  Living things avoid it instinctively. Hunters find this useful."
specialEffect: Once per hunt: monsters must spend their movement approaching
  this hunter, regardless of aggro targeting (forced approach).
```

---

## Set 7: Ivory Hall

**Unlocked after killing The Ivory Stampede.**
**Flavour:** Ivory is harder than bone, smoother than carapace, and older-looking than anything the settlers have made before. They feel like heirlooms the moment they are finished.
**Resources used:** Ivory, Bone, Hide

Create in `Assets/_Game/Data/Gear/Ivory/`.

### IVY-01 — Ivory Crown
```
gearId: GEAR-IVY-01
gearName: Ivory Crown
gearSlot: Head
craftSet: Ivory
recipe: Ivory×4, Hide×1
bonusSpeed: +2
bonusLuck: +1
description: "A smooth ivory helm with a low profile.
  Weighs almost nothing. Hunters who wear it move differently."
specialEffect: —
```

### IVY-02 — Ivory Plate
```
gearId: GEAR-IVY-02
gearName: Ivory Plate
gearSlot: Chest
craftSet: Ivory
recipe: Ivory×6, Bone×2
bonusSpeed: +1
bonusToughness: +1
bonusEvasion: +1
description: "Ivory chest plate, carved from a single tusk section.
  The weight distribution is wrong in a way that somehow works."
specialEffect: —
```

### IVY-03 — Ivory Bracers
```
gearId: GEAR-IVY-03
gearName: Ivory Bracers
gearSlot: Arms
craftSet: Ivory
recipe: Ivory×3, Sinew×1
bonusSpeed: +1
bonusAccuracy: +1
description: "Ivory forearm guards that reduce drag during a swing.
  Hunters say the motion feels natural, like the weapon is part of the arm."
specialEffect: —
```

### IVY-04 — Tusk Blade
```
gearId: GEAR-IVY-04
gearName: Tusk Blade
gearSlot: Weapon
craftSet: Ivory
recipe: Ivory×4, Bone×2
bonusAccuracy: +2
bonusSpeed: +2
description: "A weapon carved from a full Stampede tusk. Long, curved, fast.
  It remembers being a weapon — just a different kind."
specialEffect: On a hit: push target back 1 space (built-in Knockback from tusk weight).
```

### IVY-05 — Tusk Shield (OffHand)
```
gearId: GEAR-IVY-05
gearName: Tusk Shield
gearSlot: OffHand
craftSet: Ivory
recipe: Ivory×3, Hide×2
bonusEvasion: +2
bonusSpeed: +1
description: "A curved tusk section worn on the forearm.
  Shaped for deflection rather than blocking. The curved surface redirects force."
specialEffect: When successfully Evading: push the attacker back 1 space.
```

### IVY-06 — Ivory Bead
```
gearId: GEAR-IVY-06
gearName: Ivory Bead
gearSlot: Accessory
craftSet: Ivory
recipe: Ivory×2
bonusSpeed: +2
description: "A smooth ivory bead on a cord. Hunters wear them at the
  wrist or ankle. Movement feels freer. It is probably psychological.
  It works anyway."
specialEffect: This hunter may always act first in a round, before other
  hunters and before the monster (Speed tie-breaking rule).
```

---

## Final Gear Economy Overview

After Stages 9-P and 9-Q, the full gear economy is:

| Set | Items | Unlock Condition |
|---|---|---|
| Carapace Forge | 6 | Always available |
| Membrane Loft | 6 | Membrane resource collected |
| Mire Apothecary | 6 | Siltborn killed |
| Ichor Works | 6 | Penitent killed |
| Auric Scales | 6 | Gilded Serpent killed |
| Rot Garden | 6 | Rotmother killed |
| Ivory Hall | 6 | Ivory Stampede killed |
| **Total** | **42** | |

---

## Verification Test

- [ ] All 24 new GearSO assets created in correct folders (6 per set)
- [ ] Each has gearId, gearName, gearSlot, craftSet, recipe, at least one stat bonus
- [ ] ICH-06 (Ichor Vial) special: prevents one lethal hit per campaign
- [ ] AUR-04 (Scale Blade) special: reroll natural 1 attack rolls
- [ ] AUR-05 (Scale Buckler) special: auto-miss once per hunt — fires correctly
- [ ] ROT-04 (Blight Spear) special: −1 Shell HP per hit to target part
- [ ] ROT-05 (Rot Shield) special: 1 damage to attacker on Evasion success
- [ ] IVY-04 (Tusk Blade) special: Knockback 1 on every hit
- [ ] IVY-06 (Ivory Bead) special: this hunter always acts first (speed override)
- [ ] Killing Gilded Serpent → Auric Scales unlocked → items appear in Crafting tab
- [ ] Killing Rotmother → Rot Garden unlocked
- [ ] Killing Ivory Stampede → Ivory Hall unlocked
- [ ] Killing Penitent → Ichor Works unlocked
- [ ] 42 total GearSO assets exist across all 7 sets in `Assets/_Game/Data/Gear/`
- [ ] Crafting any item from new sets deducts correct resources

---

## Next Session

**File:** `_Docs/Stage_09/STAGE_09_R.md`
**Covers:** Stage 9 Final Integration & Full Campaign Definition of Done — running the complete 30-year campaign smoke test, verifying all monsters, gear, codex entries, and progression systems work end-to-end; committing the final v1.0 milestone tag
