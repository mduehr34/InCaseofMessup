<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 1-B | InjuryCardSO, ItemSO, WeaponSO, MonsterSO
Status: Stage 1-A complete. asmdef files exist, Enums.cs,
ResourceSO, ActionCardSO, BehaviorCardSO all compile and are
visible in the Create Asset menu.
Task: Create InjuryCardSO, ItemSO (with LinkPoint struct),
WeaponSO, and MonsterSO (with all supporting structs).
Do not create PackMonsterSO or any remaining SOs yet.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_01/STAGE_01_B.md
- Assets/_Game/Scripts/Core.Data/Enums.cs

Then confirm:
- What files you will create
- What you will NOT touch this session

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 1-B: InjuryCardSO, ItemSO, WeaponSO, MonsterSO

**Resuming from:** Stage 1-A complete  
**Done when:** All 4 SO classes compile, appear in Create Asset menu, and all fields are visible in the Inspector  
**Commit:** `"1B: InjuryCardSO, ItemSO, WeaponSO, MonsterSO with supporting structs"`  
**Next session:** STAGE_01_C.md  

---

## Step 1: InjuryCardSO

**Path:** `Assets/_Game/Scripts/Core.Data/InjuryCardSO.cs`

```csharp
using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/Cards/InjuryCard", fileName = "New InjuryCard")]
    public class InjuryCardSO : ScriptableObject
    {
        public string injuryName;
        public BodyPartTag bodyPartTag;
        public InjurySeverity severity;
        [TextArea] public string effect;
        public string removalCondition;     // e.g. "Settlement healing action"
    }
}
```

---

## Step 2: Supporting Structs File

**Path:** `Assets/_Game/Scripts/Core.Data/DataStructs.cs`

Create this file first — ItemSO and MonsterSO both depend on it.

```csharp
using UnityEngine;

namespace MnM.Core.Data
{
    [System.Serializable]
    public struct LinkPoint
    {
        public string affinityTag;
        public Vector2Int direction;    // Which edge of the item the link point is on
    }

    [System.Serializable]
    public struct MonsterStatBlock
    {
        public int movement;
        public int accuracy;
        public int strength;
        public int toughness;
        public int evasion;
        public int behaviorDeckSizeRemovable;
    }

    [System.Serializable]
    public struct MonsterBodyPart
    {
        public string partName;
        public BodyPartTag partTag;
        public int shellDurability;
        public int fleshDurability;
        // Names must exactly match BehaviorCardSO asset names
        public string[] breakRemovesCardNames;
        public string[] woundRemovesCardNames;
        public bool isTrapZone;
    }

    [System.Serializable]
    public struct FacingTable
    {
        public BodyPartTag primaryZone;
        public BodyPartTag secondaryZone;
        public BodyPartTag tertiaryZone;
        public int primaryZoneWeight;       // Must sum to 100 with secondary + tertiary
        public int secondaryZoneWeight;
        public int tertiaryZoneWeight;
    }

    [System.Serializable]
    public struct LootEntry
    {
        public ResourceSO resource;
        public int minAmount;
        public int maxAmount;
        public int weight;                  // Relative probability weight
    }

    [System.Serializable]
    public struct StanceDefinition
    {
        public string stanceName;
        public string stanceTag;
        [TextArea] public string effect;
    }
}
```

---

## Step 3: ItemSO

**Path:** `Assets/_Game/Scripts/Core.Data/ItemSO.cs`

```csharp
using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/Item", fileName = "New Item")]
    public class ItemSO : ScriptableObject
    {
        public string itemName;
        public int materialTier;                // 1–4
        public Vector2Int gridDimensions;       // e.g. (2,2) for a 2×2 gear item
        public bool isConsumable;
        public string setNameTag;               // Empty if not part of a set

        [Header("Stat Modifiers")]
        public int accuracyMod;
        public int strengthMod;
        public int toughnessMod;
        public int evasionMod;
        public int luckMod;
        public int movementMod;

        [Header("Links & Affinity")]
        public string[] affinityTags;
        public LinkPoint[] linkPoints;

        [Header("Effect & Crafting")]
        [TextArea] public string specialEffect;
        public ResourceSO[] craftingCost;
        public int[] craftingCostAmounts;       // Parallel array with craftingCost
    }
}
```

---

## Step 4: WeaponSO

**Path:** `Assets/_Game/Scripts/Core.Data/WeaponSO.cs`

```csharp
using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/Weapon", fileName = "New Weapon")]
    public class WeaponSO : ScriptableObject
    {
        public string weaponName;
        public WeaponType weaponType;
        public ElementTag elementTag;
        public int accuracyMod;
        public int strengthMod;
        public int attacksPerTurn;          // 1–3
        public int range;                   // 0 = adjacent; 2 = 2 tiles
        public bool isAlwaysLoud;
        public ActionCardSO signatureCard;

        [Header("Proficiency Deck Unlocks")]
        // Index 0 = Tier 1 cards, index 1 = Tier 2, etc.
        public ActionCardSO[] tier1Cards;
        public ActionCardSO[] tier2Cards;
        public ActionCardSO[] tier3Cards;
        public ActionCardSO[] tier4Cards;
        public ActionCardSO[] tier5Cards;

        [Header("Identity")]
        [TextArea] public string uniqueCapability;
        [TextArea] public string genuineCost;
    }
}
```

---

## Step 5: MonsterSO

**Path:** `Assets/_Game/Scripts/Core.Data/MonsterSO.cs`

```csharp
using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/Monster", fileName = "New Monster")]
    public class MonsterSO : ScriptableObject
    {
        [Header("Identity")]
        public string monsterName;
        public int materialTier;
        [TextArea] public string animalBasis;
        [TextArea] public string combatEmotion;
        [TextArea] public string coreSkillTaught;

        [Header("Grid Footprint per Difficulty")]
        public Vector2Int gridFootprintStandard;
        public Vector2Int gridFootprintHardened;
        public Vector2Int gridFootprintApex;

        [Header("Stat Blocks — index 0=Standard, 1=Hardened, 2=Apex")]
        public MonsterStatBlock[] statBlocks;

        [Header("Body Parts — index 0=Standard, 1=Hardened, 2=Apex")]
        public MonsterBodyPart[] standardParts;
        public MonsterBodyPart[] hardenedParts;
        public MonsterBodyPart[] apexParts;

        [Header("Behavior Deck")]
        public BehaviorCardSO[] openingCards;
        public BehaviorCardSO[] escalationCards;
        public BehaviorCardSO[] apexCards;
        public BehaviorCardSO[] permanentCards;

        [Header("Elemental Profile")]
        public ElementTag[] weaknesses;
        public ElementTag[] resistances;

        [Header("Facing Tables")]
        public FacingTable frontFacing;
        public FacingTable flankFacing;
        public FacingTable rearFacing;

        [Header("Trap Zones")]
        public string[] trapZoneParts;      // Part names that are trap zones

        [Header("Loot")]
        public LootEntry[] lootTable;

        [Header("Stances")]
        public StanceDefinition[] stances;
    }
}
```

---

## Verification Test

In the Unity Editor:

1. Confirm no compiler errors in the Console
2. Right-click → Create → MnM → confirm these now exist:
   - `MnM/Cards/InjuryCard`
   - `MnM/Item`
   - `MnM/Weapon`
   - `MnM/Monster`
3. Create one `MonsterSO` asset. Confirm all sections appear in the Inspector:
   - Identity, Grid Footprint, Stat Blocks, Body Parts, Behavior Deck, Elemental Profile, Facing Tables, Trap Zones, Loot, Stances
4. Confirm `DataStructs.cs` has no namespace conflicts with `Enums.cs`

If all 4 checks pass — this session is done.

---

## Next Session

**File:** `_Docs/Stage_01/STAGE_01_C.md`  
**Covers:** PackMonsterSO, EventSO, InnovationSO, GuidingPrincipalSO, CampaignSO, CharacterSO, CrafterSO, ArtifactSO
