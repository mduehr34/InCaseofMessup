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
**Done when:** 5 monster SOs fully populated (Gaunt, Thornback, Ivory Stampede, Gilded Serpent, The Spite); Suture skeleton created with stat blocks only; behavior deck fields empty and flagged  
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

## Thornback

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

## The Ivory Stampede — PackMonsterSO

**Save:** `Assets/_Game/Data/Monsters/Monster_TheIvoryStampede.asset`

> ⚑ Must use `PackMonsterSO`, not `MonsterSO`. This is the ONLY monster that uses PackMonsterSO.

`unitCount = 3`. One shared behavior deck. Each elephant has its own health pool tracked at runtime.

**Individual elephant stats (Standard):** Movement 8, Accuracy 2, Strength 2, Toughness 1, Evasion 3, footprint 1×1  
**Shared deck (Standard):** 9 removable, 1 permanent

**Herd body parts per elephant (Standard Shell 2 / Flesh 2):** Head, Neck, Body, Legs (4 parts × 3 elephants = 12 tracked independently, but removed from SHARED deck)

**Create Stampede behavior cards** in `Assets/_Game/Data/Cards/Behavior/IvoryStampede/`. All 3 elephants execute the same drawn card simultaneously each Monster Phase.

**Loot table (Standard):** Bone 2–4, Hide 2–3, IvoryTusk 2–3 (Common), HerdHide 1–2 (Uncommon), HerdEye 0–1 (Rare)

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

## The The Spite

**Save:** `Assets/_Game/Data/Monsters/Monster_TheSpite.asset`

**Identity:** Material Tier 3. Year 12+ (gate-locked behind EVT-21). `animalBasis`: Massive Marrow-enhanced honey badger (in-world name: The Spite), Marrow saturation concentrated in the hide and jaw. `combatEmotion`: Relentless — does not retreat, does not hesitate, continues through wounds that would collapse anything else.

> ⚑ Combat identity to be fully defined in development stage. Two candidate mechanics under consideration:
> - **Option A:** Shell regeneration — Shell values recover partially at the start of each Monster Phase, forcing hunters to manage break order carefully
> - **Option B:** Wound resistance — wounds require a higher Force Check threshold to apply; hunters must commit harder to each strike

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

**Standard body parts (Shell 2 / Flesh 4):** Head, Jaw, Neck, Torso, Left Flank, Right Flank, Hindquarters  
**weaknesses:** TBD at development stage. **resistances:** TBD at development stage.

**Create The Spite behavior cards** in `Assets/_Game/Data/Cards/Behavior/Spite/`. Design to be defined at development stage alongside combat identity selection.

**Loot table (Standard):** IronClaw 2–3 (Common), IronhidePelt 1–2 (Uncommon), GallShard 0–1 (Rare)

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

## The Penitent — Overlord Skeleton (OVR-02)

**Save:** `Assets/_Game/Data/Monsters/Overlord_Penitent.asset`

**Identity:** Overlord. Year 15. `animalBasis`: Massive Marrow-corrupted primate, twisted into a permanent hunched posture by Marrow saturation. `combatEmotion`: Does not attack out of hunger or territory — it senses harvested Marrow on the hunters and is drawn to whoever carries the most. Deliberate, ancient, wrong.

**Targeting Rule:** At the start of each Monster Phase, The Penitent targets the hunter carrying the `MarrowBeacon` status if present. Otherwise targets the hunter with the highest total Shell across all equipped armor. This is resolved by a new `AggroManager.GetPenitentTarget()` method.

> ⚑ `ExecuteCard()` and `EvaluateTrigger()` are currently stubs — targeting logic slots in when those are implemented (scheduled for Stage 3-C).

**Loot table:** Bone 3–5, Hide 3–4, PenitentGland 1 (always, Rare — required for Marrow Lure recipe)

**Drops:** First Tier 3 materials

---

### Marrow Lure — Design Spec (Item, Tier 3)

> ⚑ Full ItemSO definition belongs in the crafting stage. This section is a design anchor — do not build the SO here.

**Unlock:** Craftable after first Penitent kill. Requires `PenitentGland`.  
**Recipe (proposed):** PenitentGland × 1, Bone × 4, Hide × 3, any Tier 3 unique × 1  
**Effect:** Hunter equipped with this item receives `MarrowBeacon` status at hunt start. All Marrow-sensitive monsters (The Penitent, The Suture, any future monsters flagged `marrowSensitive = true`) lock this hunter as primary target.  
**Design note:** Intentional risk/reward — equipping hunter acts as a permanent taunt. Pairs with the Draw the Hunt card to manage where the beacon sits mid-combat.

---

### Draw the Hunt — Design Spec (Hunter Action Card)

> ⚑ Full card SO definition belongs in the hunter card design stage. This section is a design anchor only.

**Source:** Unlocked alongside the Marrow Lure — granted when the recipe is first crafted.  
**Type:** Hunter action card (played from hand during Hunter Phase).  
**Effect:** Transfer `MarrowBeacon` status from the current carrier to another hunter in range for 1 round. At the start of the next round the status returns to the equipped hunter.  
**Cost:** 1 AP  
**Design note:** Converts the Lure from a passive taunt into a tactical tool. Lets the party bait a specific attack onto a hunter who is better positioned to absorb it, then return aggro to the tank.

---

## Verification Test

- [ ] Monster_Gaunt.asset has all body parts, break/wound removals, facing tables, loot table
- [ ] breakRemovesCardNames strings exactly match BehaviorCardSO asset names
- [ ] Monster_TheIvoryStampede.asset uses PackMonsterSO (confirmed in Inspector Type field)
- [ ] EVT-21 correctly gates The Spite — not in hunt roster until event fires
- [ ] Monster_Suture.asset exists with stat blocks but empty behavior fields
- [ ] Suture NOT in Standard CampaignSO monsterRoster

---

## Next Session

**File:** `_Docs/Stage_07/STAGE_07_O.md`  
**Covers:** All Gaunt Boneworks craft set — 5 armor pieces, 4 weapons, 2 accessories, 1 consumable
