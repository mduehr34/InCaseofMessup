<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 7-N | All Monster SO Assets — Complete Data
Status: Stage 7-M complete. All 20 Innovations and 5 Guiding
Principals verified. Cascade tree confirmed in-game.
Task: Complete all monster SO assets with full data:
stat blocks (3 difficulties), body parts, behavior card
references, facing tables, loot tables. Gaunt already has
mock data — complete and rename it. Build 4 more monsters.
The Suture gets a skeleton only — behavior deck is NOT defined
in GDD v4.0 and must NOT be invented.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_07/STAGE_07_N.md
- Assets/_Game/Data/Monsters/Mock_GauntStandard.asset
- Assets/_Game/Scripts/Core.Data/MonsterSO.cs
- Assets/_Game/Data/Cards/Behavior/Gaunt/ (all Gaunt cards)

⚑ STOP AND ASK before creating The Suture's behavior deck.
  The GDD does not fully define The Suture's cards.
  Do not guess or invent them.

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 7-N: All Monster SO Assets — Complete Data

**Resuming from:** Stage 7-M complete  
**Done when:** 5 monster SOs fully populated (Gaunt, Thornback, Pack, Serpent, Pale Stag); Suture skeleton created with stat blocks only; behavior deck fields empty and flagged  
**Commit:** `"7N: All monster SO assets complete — Suture behavior deck pending design"`  
**Next session:** STAGE_07_O.md  

---

## The Gaunt — Complete Existing Mock Asset

Rename `Mock_GauntStandard.asset` → `Monster_Gaunt.asset`. Populate all remaining fields.

**Standard body parts (Shell 2 / Flesh 3 each):** Head, Throat, Torso, Left Flank, Right Flank, Hind Legs, Tail

**breakRemovesCardNames (must exactly match BehaviorCardSO asset names):**

| Part | On Shell Break Removes |
|---|---|
| Throat | Gaunt_TheHowl, Gaunt_ScentLock |
| Left Flank | Gaunt_ThroatLock |
| Right Flank | Gaunt_CreepingAdvance |
| Hind Legs | Gaunt_Lunge |
| Tail | Gaunt_FlankSense |

**woundRemovesCardNames:**

| Part | Wound 1 | Wound 2 |
|---|---|---|
| Head | Gaunt_TremorRead | Gaunt_PackMemory |
| Torso | Gaunt_Frenzy | — |

**Facing tables (weights must sum to 100 per arc):**

| Arc | Primary | Secondary | Tertiary |
|---|---|---|---|
| Front | Torso 50 | Head 30 | Arms 20 |
| Flank | Arms 50 | Torso 30 | Legs 20 |
| Rear | Legs 50 | Waist 30 | Back 20 |

**Loot table (Standard):** Bone 2–4 (w:40), Hide 2–3 (w:30), Sinew 1–2 (w:20), GauntFang 2–3 (w:60), GauntPelt 1–2 (w:40), GauntEye 0–1 (w:20)

**weaknesses:** None. **resistances:** None. **trapZoneParts:** empty (Gaunt has no trap zones).

---

## Thornback Patriarch

**Save:** `Assets/_Game/Data/Monsters/Monster_Thornback.asset`

**Identity:** Material Tier 2. Years 3+. Enormous boar. `animalBasis`: Marrow-enhanced wild boar, armored with calcified bone spikes from dorsal ridge. `combatEmotion`: Aggression — charges and tramples, does not retreat.

**Stat Blocks:**

| Stat | Standard | Hardened | Apex |
|---|---|---|---|
| Movement | 7 | 9 | 11 |
| Accuracy | 2 | 3 | 4 |
| Strength | 3 | 4 | 5 |
| Toughness | 2 | 3 | 4 |
| Evasion | 1 | 2 | 3 |
| Grid Footprint | 3×2 | 3×2 | 4×3 |
| Removable Cards | 10 | 13 | 16 |

**Standard body parts (Shell 3 / Flesh 3):** Skull, Snout (trap zone), Left Shoulder, Right Shoulder, Dorsal Ridge (trap zone), Haunches, Tail Bone

**trapZoneParts:** ["Snout", "Dorsal Ridge"]

**Create Thornback behavior cards** in `Assets/_Game/Data/Cards/Behavior/Thornback/`. Naming: `Thornback_[CardName]`. Design as a charge-focused, high-mobility monster that punishes hunters staying in front arc.

**Loot table (Standard):** Bone 3–5, Hide 3–4, ThornbackPlate 2–3 (Common), ThornbackTusk 1–2 (Uncommon), ThornbackCrystal 0–1 (Rare)

---

## The Pack — PackMonsterSO

**Save:** `Assets/_Game/Data/Monsters/Monster_ThePack.asset`

> ⚑ Must use `PackMonsterSO`, not `MonsterSO`. This is the ONLY monster that uses PackMonsterSO.

`wolfCount = 3`. One shared behavior deck. Each wolf has its own health pool tracked at runtime.

**Individual wolf stats (Standard):** Movement 8, Accuracy 2, Strength 2, Toughness 1, Evasion 3, footprint 1×1  
**Shared deck (Standard):** 9 removable, 1 permanent

**Pack body parts per wolf (Standard Shell 2 / Flesh 2):** Head, Neck, Body, Legs (4 parts × 3 wolves = 12 tracked independently, but removed from SHARED deck)

**Create Pack behavior cards** in `Assets/_Game/Data/Cards/Behavior/Pack/`. All 3 wolves execute the same drawn card simultaneously each Monster Phase.

**Loot table (Standard):** Bone 2–4, Hide 2–3, PackFang 2–3 (Common), PackPelt 1–2 (Uncommon), PackEye 0–1 (Rare)

---

## The Gilded Serpent

**Save:** `Assets/_Game/Data/Monsters/Monster_GildedSerpent.asset`

**Identity:** Material Tier 3. Years 8+. `animalBasis`: Vast serpent, Marrow-saturated, gold luminescent scales. `combatEmotion`: Patient menace — coils, controls terrain, strikes with precision.

**Stat Blocks:**

| Stat | Standard | Hardened | Apex |
|---|---|---|---|
| Movement | 5 | 7 | 9 |
| Accuracy | 3 | 4 | 5 |
| Strength | 3 | 4 | 5 |
| Toughness | 3 | 4 | 5 |
| Evasion | 2 | 3 | 4 |
| Grid Footprint | 4×1 | 4×1 | 5×1 |
| Removable Cards | 10 | 13 | 16 |

**Standard body parts (Shell 3 / Flesh 4):** Head, Upper Coil (trap zone), Mid Coil, Lower Coil (trap zone), Tail Tip  
**weaknesses:** Ice. **resistances:** Fire. **trapZoneParts:** ["Upper Coil", "Lower Coil"]

**Create Serpent behavior cards** in `Assets/_Game/Data/Cards/Behavior/Serpent/`. Design around terrain control, venom application, and punishing hunters who cluster together.

**Loot table (Standard):** SerpentScale 2–3 (Common), SerpentFang 1–2 (Uncommon), SerpentHeart 0–1 (Rare)

---

## The Pale Stag

**Save:** `Assets/_Game/Data/Monsters/Monster_PaleStag.asset`

**Identity:** Material Tier 3. Year 12+ (gate-locked behind EVT-21). `animalBasis`: Enormous albino stag, antlers glow faintly with Marrow energy. `combatEmotion`: Alien calm — the Stag does not fear. It simply acts.

**Stat Blocks:**

| Stat | Standard | Hardened | Apex |
|---|---|---|---|
| Movement | 9 | 11 | 13 |
| Accuracy | 3 | 4 | 5 |
| Strength | 3 | 4 | 5 |
| Toughness | 2 | 3 | 4 |
| Evasion | 4 | 5 | 6 |
| Grid Footprint | 2×3 | 2×3 | 3×4 |
| Removable Cards | 10 | 13 | 16 |

**Standard body parts (Shell 2 / Flesh 4):** Crown (antlers), Head, Neck, Chest, Left Flank, Right Flank, Haunches  
**weaknesses:** Venom. **resistances:** None.

**Create Stag behavior cards** in `Assets/_Game/Data/Cards/Behavior/Stag/`. Design around high mobility, blinding Marrow effects, and reactions to Loud cards.

**Loot table (Standard):** StagAntler 2–3 (Common), StagPelt 1–2 (Uncommon), CrownShard 0–1 (Rare)

---

## The Suture — Skeleton Only

**Save:** `Assets/_Game/Data/Monsters/Monster_Suture.asset`

Fill stat blocks only:

| Stat | Standard | Hardened | Apex |
|---|---|---|---|
| Movement | 8 | 10 | 12 |
| Accuracy | 4 | 5 | 6 |
| Strength | 5 | 6 | 7 |
| Toughness | 4 | 5 | 6 |
| Evasion | 3 | 4 | 5 |
| Grid Footprint | 4×4 | 4×4 | 5×5 |

**Leave empty:** standardParts, hardenedParts, apexParts, openingCards, escalationCards, apexCards, permanentCards, lootTable

> ⚑ Do NOT add The Suture to any CampaignSO monsterRoster until its behavior deck is designed. The stat blocks are provided for planning purposes only.

---

## Verification Test

- [ ] Monster_Gaunt.asset has all body parts, break/wound removals, facing tables, loot table
- [ ] breakRemovesCardNames strings exactly match BehaviorCardSO asset names
- [ ] Monster_ThePack.asset uses PackMonsterSO (confirmed in Inspector Type field)
- [ ] EVT-21 correctly gates Pale Stag — not in hunt roster until event fires
- [ ] Monster_Suture.asset exists with stat blocks but empty behavior fields
- [ ] Suture NOT in Standard CampaignSO monsterRoster

---

## Next Session

**File:** `_Docs/Stage_07/STAGE_07_O.md`  
**Covers:** All Gaunt Boneworks craft set — 5 armor pieces, 4 weapons, 2 accessories, 1 consumable
