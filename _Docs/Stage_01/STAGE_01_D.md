<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 1-D | Mock Data Assets & Stage 1 Final Verification
Status: Stage 1-C complete. All 15 SO classes compile with
zero errors. Full Create Asset menu confirmed.
Task: Create the minimum mock data assets needed to verify
the data layer before any logic is written. No gameplay
systems yet — data only.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_01/STAGE_01_D.md

Then confirm:
- What assets you will create and where
- What you will NOT touch this session

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 1-D: Mock Data Assets & Stage 1 Final Verification

**Resuming from:** Stage 1-C complete — all 15 SO classes compile  
**Done when:** All mock assets created, inspectable, and cross-references resolve correctly in the Inspector  
**Commit:** `"1D: Mock data assets — Stage 1 complete"`  
**Next session:** STAGE_02_A.md (Stage 2 begins)

---

## Why Mock Data First

Before writing a single line of gameplay logic, we need to confirm the data layer is solid. These assets will be used in every subsequent stage as the test bed. If a field is wrong or a reference breaks, it's much cheaper to fix it now than after Stage 3 is built on top of it.

---

## Assets to Create

Create all assets in the paths listed. Use the field values exactly as specified — these are the canonical test values referenced in later stage verification tests.

---

### Mock Asset 1: Gaunt Fang (ResourceSO)

**Path:** `Assets/_Game/Data/Resources/Mock_GauntFang.asset`

| Field | Value |
|---|---|
| resourceName | Gaunt Fang |
| type | UniqueCommon |
| tier | 1 |
| conversionRate | 1 |

---

### Mock Asset 2: Brace (ActionCardSO)

**Path:** `Assets/_Game/Data/Cards/Action/Mock_Brace.asset`

| Field | Value |
|---|---|
| cardName | Brace |
| weaponType | FistWeapon |
| category | Reaction |
| apCost | 0 |
| apRefund | 0 |
| isLoud | false |
| isReaction | true |
| proficiencyTierRequired | 1 |
| effectDescription | When you take damage, reduce that damage by 2 Shell or 1 Flesh. Declare before damage. |

---

### Mock Asset 3: Creeping Advance (BehaviorCardSO)

**Path:** `Assets/_Game/Data/Cards/Behavior/Mock_CreepingAdvance.asset`

| Field | Value |
|---|---|
| cardName | Creeping Advance |
| cardType | Removable |
| group | Opening |
| triggerCondition | End of round |
| effectDescription | Move 3 squares toward Aggro holder |
| removalCondition | Right Flank Shell break |

---

### Mock Asset 4: Gaunt Bone Spear (InjuryCardSO)

> This is a stand-in injury for testing. Real injury cards created in Stage 7.

**Path:** `Assets/_Game/Data/Cards/Injury/Mock_SpearWound.asset`

| Field | Value |
|---|---|
| injuryName | Spear Wound |
| bodyPartTag | Torso |
| severity | Minor |
| effect | -1 Strength for the next 2 hunts |
| removalCondition | Settlement healing action |

---

### Mock Asset 5: The Gaunt Standard (MonsterSO)

**Path:** `Assets/_Game/Data/Monsters/Mock_GauntStandard.asset`

Fill in the following fields — leave all array fields empty for now (they'll reference BehaviorCardSO assets not yet created):

| Field | Value |
|---|---|
| monsterName | The Gaunt |
| materialTier | 1 |
| animalBasis | Marrow-starved wolf, enormous, blind — hunts by sound and vibration |
| combatEmotion | Tension — the monster reacts to noise and movement, not sight |
| coreSkillTaught | Positioning and facing |
| gridFootprintStandard | (2, 2) |
| gridFootprintHardened | (2, 2) |
| gridFootprintApex | (3, 3) |

**Stat Blocks array (size 3):**

| Index | movement | accuracy | strength | toughness | evasion | behaviorDeckSizeRemovable |
|---|---|---|---|---|---|---|
| 0 (Standard) | 6 | 1 | 2 | 1 | 2 | 9 |
| 1 (Hardened) | 8 | 2 | 3 | 2 | 3 | 12 |
| 2 (Apex) | 10 | 3 | 4 | 3 | 4 | 15 |

**Facing Tables:**

Front: primaryZone=Torso, secondaryZone=Head, tertiaryZone=Arms, weights=50/30/20  
Flank: primaryZone=Arms, secondaryZone=Torso, tertiaryZone=Legs, weights=50/30/20  
Rear: primaryZone=Legs, secondaryZone=Waist, tertiaryZone=Back, weights=50/30/20  

Leave openingCards, escalationCards, apexCards, permanentCards, lootTable, and all body parts empty — these are populated in Stage 7 when real content is built.

---

### Mock Asset 6: Aldric (CharacterSO)

**Path:** `Assets/_Game/Data/Characters/Mock_Aldric.asset`

| Field | Value |
|---|---|
| characterName | Aldric |
| bodyBuild | Aethel |
| sex | Male |
| accuracy | 0 |
| evasion | 0 |
| strength | 0 |
| toughness | 0 |
| luck | 0 |
| movement | 3 |
| huntCount | 0 |
| isRetired | false |

currentDeck: add Mock_Brace as the one entry  
All other arrays: leave empty

---

### Mock Asset 7: Tutorial Campaign (CampaignSO)

**Path:** `Assets/_Game/Data/Campaigns/Mock_TutorialCampaign.asset`

| Field | Value |
|---|---|
| campaignName | Tutorial Campaign |
| difficulty | Medium |
| campaignLengthYears | 3 |
| startingCharacterCount | 8 |
| baseMovement | 3 |
| startingGrit | 3 |
| ironmanMode | false |
| retirementHuntCount | 10 |

monsterRoster: add Mock_GauntStandard  
All other arrays: leave empty

---

## Verification Test

Work through this checklist in the Unity Editor:

**Compile check:**
- [ ] Zero errors in Console

**Asset references:**
- [ ] Open Mock_Aldric → currentDeck slot 0 shows Mock_Brace (not missing)
- [ ] Open Mock_TutorialCampaign → monsterRoster slot 0 shows Mock_GauntStandard (not missing)
- [ ] Open Mock_GauntStandard → statBlocks array shows 3 entries with correct values

**Create Asset menu:**
- [ ] Right-click → Create → MnM shows all 15 options with no duplicates

**No logic written:**
- [ ] Confirm Scripts/Core.Systems/, Scripts/Core.Logic/, Scripts/Core.UI/ are all empty (no .cs files)

If all boxes checked — **Stage 1 is complete.**

---

## Stage 1 Complete — What You Now Have

- 4 assembly definitions with no circular dependencies
- 15 ScriptableObject classes covering every data type in the GDD
- A complete Enums.cs covering all game states
- Supporting structs for complex data structures
- 7 mock assets that will be used as the test bed for all future stages
- Zero gameplay logic — the data layer is clean and ready

---

## Next Session

**File:** `_Docs/Stage_02/STAGE_02_A.md`  
**First task of Stage 2:** Define the IGridManager, ICombatManager, and IMonsterAI interfaces — no implementations until interfaces are approved
