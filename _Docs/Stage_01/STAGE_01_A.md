<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 1-A | Project Scaffold, Enums & First SO Classes
Status: Fresh Unity 6 project created. No scripts written yet.
Task: Create the 4 assembly definition files, Enums.cs, and the
first 3 ScriptableObject classes: ResourceSO, ActionCardSO,
BehaviorCardSO. Do not create any other SO classes yet.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_01/STAGE_01_A.md

Then confirm:
- What files you will create
- What folders you will create
- What you will NOT touch this session

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 1-A: Project Scaffold, Enums & First SO Classes

**Resuming from:** Fresh Unity 6 project  
**Done when:** 4 asmdef files exist, Enums.cs compiles, ResourceSO / ActionCardSO / BehaviorCardSO all visible in Unity's Create Asset menu  
**Commit:** `"1A: asmdef scaffold, Enums.cs, ResourceSO, ActionCardSO, BehaviorCardSO"`  
**Next session:** STAGE_01_B.md  

---

## Folder Structure to Create

```
Assets/
└── _Game/
    ├── Data/
    │   ├── Monsters/
    │   ├── Cards/
    │   │   ├── Action/
    │   │   ├── Behavior/
    │   │   └── Injury/
    │   ├── Items/
    │   ├── Weapons/
    │   ├── Events/
    │   ├── Innovations/
    │   ├── Campaigns/
    │   ├── Characters/
    │   └── Resources/
    └── Scripts/
        ├── Core.Data/
        ├── Core.Systems/
        ├── Core.UI/
        └── Core.Logic/
```

---

## Step 1: Assembly Definitions

Create one `.asmdef` file in each Scripts subfolder.

| File | Folder | References |
|---|---|---|
| `MnM.Core.Data.asmdef` | `Scripts/Core.Data/` | *(none)* |
| `MnM.Core.Systems.asmdef` | `Scripts/Core.Systems/` | `MnM.Core.Data` |
| `MnM.Core.Logic.asmdef` | `Scripts/Core.Logic/` | `MnM.Core.Data`, `MnM.Core.Systems` |
| `MnM.Core.UI.asmdef` | `Scripts/Core.UI/` | `MnM.Core.Data`, `MnM.Core.Systems` |

No circular dependencies. No exceptions.

---

## Step 2: Enums.cs

**Path:** `Assets/_Game/Scripts/Core.Data/Enums.cs`  
**Assembly:** `MnM.Core.Data`

```csharp
namespace MnM.Core.Data
{
    public enum WeaponType
    {
        FistWeapon, Dagger, SwordAndShield, Axe,
        HammerMaul, Spear, Greatsword, Bow
    }

    public enum ElementTag { None, Fire, Ice, Venom, Shock }

    public enum BodyPartTag
    {
        Head, Throat, Torso, LeftFlank, RightFlank,
        HindLegs, Tail, Arms, Legs, Waist, Back
    }

    public enum ResourceType
    {
        Bone, Hide, Organ,
        UniqueCommon, UniqueUncommon, UniqueRare
    }

    public enum BehaviorCardType { Removable, Permanent, SingleTrigger }
    public enum BehaviorGroup { Opening, Escalation, Apex }

    public enum CardCategory
    {
        Opener, Linker, Finisher,
        BasicAttack, Reaction, Signature
    }

    public enum DifficultyLevel { Easy, Medium, Hard }
    public enum InjurySeverity { Minor, Major, Critical }
    public enum CodexCategory { Monsters, Artifacts, SettlementRecords }
    public enum CharacterSex { Male, Female }

    public enum CharacterBuild
    {
        // Male builds
        Aethel, Beorn, Cyne, Duna,
        // Female builds
        Eira, Freya, Gerd, Hild
    }

    public enum GuidingPrincipalTag
    {
        LifeOrStrength, BloodPrice, MarrowKnowledge,
        LegacyOrForgetting, TheSuture
    }

    public enum StatusEffect { Shaken, Slowed, Pinned, Exposed, Bleeding }
    public enum CombatPhase { VitalityPhase, HunterPhase, BehaviorRefresh, MonsterPhase }
    public enum DamageType { Shell, Flesh }
    public enum FacingArc { Front, Flank, Rear }
    public enum AudioContext { SettlementEarly, SettlementLate, HuntTravel, CombatStandard, CombatOverlord }
}
```

---

## Step 3: ResourceSO

**Path:** `Assets/_Game/Scripts/Core.Data/ResourceSO.cs`

```csharp
using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/Resource", fileName = "New Resource")]
    public class ResourceSO : ScriptableObject
    {
        public string resourceName;
        public ResourceType type;
        public int tier;                // 1–4
        public float conversionRate;    // e.g. 2 UniqueCommon = 1 UniqueUncommon
    }
}
```

---

## Step 4: ActionCardSO

**Path:** `Assets/_Game/Scripts/Core.Data/ActionCardSO.cs`

```csharp
using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/Cards/ActionCard", fileName = "New ActionCard")]
    public class ActionCardSO : ScriptableObject
    {
        public string cardName;
        public WeaponType weaponType;
        public CardCategory category;
        public int apCost;
        public int apRefund;
        public bool isLoud;
        public bool isReaction;
        public int proficiencyTierRequired;     // 1–5
        [TextArea] public string flavorText;
        [TextArea] public string effectDescription;
        // Effect resolution handled by MnM.Core.Logic — no logic in this class
    }
}
```

---

## Step 5: BehaviorCardSO

**Path:** `Assets/_Game/Scripts/Core.Data/BehaviorCardSO.cs`

```csharp
using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/Cards/BehaviorCard", fileName = "New BehaviorCard")]
    public class BehaviorCardSO : ScriptableObject
    {
        public string cardName;
        public BehaviorCardType cardType;
        public BehaviorGroup group;
        [TextArea] public string triggerCondition;
        [TextArea] public string effectDescription;
        public string removalCondition;     // e.g. "Throat Shell break"
        public string stanceTag;
        public string groupTag;
        // Logic resolved by MnM.Core.Systems — no logic in this class
    }
}
```

---

## Verification Test

In the Unity Editor:

1. Confirm no compiler errors in the Console
2. Right-click in the Project window → Create → MnM → confirm these exist:
   - `MnM/Resource`
   - `MnM/Cards/ActionCard`
   - `MnM/Cards/BehaviorCard`
3. Create one of each asset and confirm all fields appear in the Inspector
4. Confirm the 4 asmdef files appear in the Project window under their respective folders

If all 4 checks pass — this session is done.

---

## Next Session

**File:** `_Docs/Stage_01/STAGE_01_B.md`  
**Covers:** InjuryCardSO, ItemSO (with supporting structs), WeaponSO, MonsterSO (with supporting structs)
