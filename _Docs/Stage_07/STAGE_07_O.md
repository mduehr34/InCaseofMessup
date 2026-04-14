<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 7-O | Gaunt Craft Set — All Item SO Assets
Status: Stage 7-N complete. All monster SOs done (Suture
skeleton only — behavior deck pending design decision).
Task: Create all Gaunt Boneworks items as ItemSO and WeaponSO
assets. 5 armor pieces (Gaunt Hunter's Set), 4 weapons,
2 accessories, 1 consumable. Create Boneworks CrafterSO
referencing all items.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_07/STAGE_07_O.md
- Assets/_Game/Scripts/Core.Data/ItemSO.cs
- Assets/_Game/Scripts/Core.Data/WeaponSO.cs
- Assets/_Game/Scripts/Core.Data/CrafterSO.cs
- Assets/_Game/Scripts/Core.Data/DataStructs.cs

Then confirm:
- All armor pieces share setNameTag = "Gaunt Hunter's Set"
- All armor pieces share affinityTags = ["GAUNT"]
- BoneSplint has isConsumable = true
- craftingCost arrays and craftingCostAmounts are
  parallel arrays (same length, same indices match)
- What you will NOT create this session (other monster sets)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 7-O: Gaunt Craft Set — All Item SO Assets

**Resuming from:** Stage 7-N complete  
**Done when:** All 12 Gaunt items exist as assets; Boneworks CrafterSO populated; crafting Gaunt Skull Cap deducts correct resources and adds item to hunter loadout in a test playthrough  
**Commit:** `"7O: Gaunt Boneworks craft set — all item assets, CrafterSO populated"`  
**Next session:** STAGE_07_P.md  

---

## Save Paths

- `Assets/_Game/Data/Items/Gaunt/`
- `Assets/_Game/Data/Weapons/Gaunt/`
- `Assets/_Game/Data/Crafters/`

---

## Gaunt Hunter's Set — 5 Armor Pieces

All share: `setNameTag = "Gaunt Hunter's Set"`, `materialTier = 1`, `affinityTags = ["GAUNT"]`

| Asset | itemName | gridDimensions | Stat Mods | specialEffect | craftingCost | amounts |
|---|---|---|---|---|---|---|
| `Item_GauntSkullCap` | Gaunt Skull Cap | (1,1) | evasionMod: +1 | Link bonus: +1 Accuracy when linked to any GAUNT piece below | [GauntPelt, Bone] | [1, 1] |
| `Item_GauntHideVest` | Gaunt Hide Vest | (2,2) | — | Link bonus: +1 Toughness when linked above AND below simultaneously | [GauntPelt, GauntFang, Hide] | [2, 1, 1] |
| `Item_GauntSinewWrap` | Gaunt Sinew Wrap | (2,1) | evasionMod: +1 | Link bonus: +1 Movement when linked above AND below simultaneously | [Sinew, GauntPelt] | [2, 1] |
| `Item_GauntBoneBracers` | Gaunt Bone Bracers | (1,2) | — | Link bonus: +1 Strength when linked to any GAUNT piece on right side | [Bone, Sinew] | [2, 1] |
| `Item_GauntHideBoots` | Gaunt Hide Boots | (1,2) | movementMod: +1 | Link bonus: +1 Evasion when linked to any GAUNT piece above | [GauntPelt, Bone, Sinew] | [1, 1, 1] |

**Set bonuses** — record in `Item_GauntHideVest.specialEffect` as the anchor piece:

```
2 piece: +1 Evasion (flat bonus while 2+ GAUNT pieces equipped)
3 piece: Behavior cards triggered by Loud card plays have movement effect -2 squares
5 piece (full set): Once per hunt — when this hunter would collapse, they instead survive with 1 Flesh remaining on the struck part. "The Gaunt survived on nothing. So can you."
```

---

## Gaunt Boneworks — 4 Weapons (WeaponSO assets)

| Asset | weaponName | weaponType | accMod | strMod | specialEffect | craftingCost | amounts |
|---|---|---|---|---|---|---|---|
| `Weapon_GauntFangDaggers` | Gaunt Fang Daggers | Dagger | +2 | -1 | Rear arc attacks gain additional +1 Accuracy (total +3 from Rear) | [GauntFang, Sinew, Bone] | [2, 1, 1] |
| `Weapon_GauntJawAxe` | Gaunt Jaw Axe | Axe | -1 | 0 | Shell hits count as 2 Shell damage. +1 Shell damage vs Tier 1 monsters only. | [GauntFang, Bone, GauntPelt] | [3, 2, 1] |
| `Weapon_GauntBoneSpear` | Gaunt Bone Spear | Spear | 0 | 0 | Zone denial card available from Tier 1 — designate 1 square as denied for monster this round. | [Bone, GauntFang, Sinew] | [2, 1, 1] |
| `Weapon_GauntSinewBow` | Gaunt Sinew Bow | Bow | +1 | -1 | Proximity behavior cards never trigger from this hunter. Cannot attack if monster adjacent. | [Sinew, GauntPelt, Bone] | [2, 1, 1] |

---

## Accessories & Consumable — 3 Items

| Asset | itemName | gridDimensions | isConsumable | statMods | specialEffect | craftingCost | amounts |
|---|---|---|---|---|---|---|---|
| `Item_GauntFangNecklace` | Gaunt Fang Necklace | (1,1) | false | accuracyMod: +1 | None | [GauntFang, Sinew] | [1, 1] |
| `Item_GauntEyePendant` | Gaunt Eye Pendant | (1,1) | false | luckMod: +1 | Once per hunt: when this hunter draws a Scar Card, may discard without triggering its effect. Card remains in deck. Requires Rare Unique (Hardened/Apex only). | [GauntEye, Sinew] | [1, 1] |
| `Item_BoneSplint` | Bone Splint | consumable slot | true | — | Restore 2 Shell Durability to any one body zone. Usable on self or adjacent hunter. Single use. | [Bone, Sinew] | [2, 1] |

---

## Boneworks CrafterSO

**Asset:** `Assets/_Game/Data/Crafters/Crafter_Boneworks.asset`

```
crafterName:              Boneworks
monsterTag:               The Gaunt
materialTier:             1
recipeList:               [all 12 items and weapons above]
unlockCost:               empty array   (Year 1 auto-unlock in Tutorial)
unlockCostAmounts:        empty array
settlementScenePosition:  (120, 400)   ← adjust in session to fit layout
spriteAssetPath:          Art/Generated/Settlement/building_boneworks
```

---

## Verification Test

1. Start new campaign, hunt Gaunt Standard, win with loot (GauntFang ×2, Bone ×2, Sinew ×1)
2. Settlement → Crafters tab → Boneworks shows as available to unlock (or auto-unlocked in Tutorial)
3. Unlock Boneworks (no cost Tutorial, or deduct resources Standard)
4. Craft Gaunt Skull Cap for Aldric — verify 1 GauntPelt and 1 Bone deducted
5. Open Gear Grid for Aldric → Gaunt Skull Cap appears in items
6. Place in cell (0,0) → stats summary shows evasionMod: +1
7. Place Gaunt Hide Vest in cell (0,1) → link resolves → +1 Toughness bonus appears

---

## Next Session

**File:** `_Docs/Stage_07/STAGE_07_P.md`  
**Covers:** Tutorial and Standard Campaign SOs fully populated + complete 3-year Tutorial playthrough
