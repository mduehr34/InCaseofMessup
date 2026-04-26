<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 1-C | Remaining ScriptableObject Classes
Status: Stage 1-B complete. InjuryCardSO, ItemSO, WeaponSO,
MonsterSO and DataStructs all compile with no errors.
Task: Create the remaining 8 SO classes: PackMonsterSO,
EventSO, InnovationSO, GuidingPrincipalSO, CampaignSO,
CharacterSO, CrafterSO, ArtifactSO.
Do not create any mock data assets yet — that is Session 1-D.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_01/STAGE_01_C.md
- Assets/_Game/Scripts/Core.Data/Enums.cs
- Assets/_Game/Scripts/Core.Data/DataStructs.cs
- Assets/_Game/Scripts/Core.Data/MonsterSO.cs

Then confirm:
- What files you will create
- What you will NOT touch this session

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 1-C: Remaining ScriptableObject Classes

**Resuming from:** Stage 1-B complete  
**Done when:** All 8 remaining SO classes compile and appear in the Create Asset menu  
**Commit:** `"1C: PackMonsterSO, EventSO, InnovationSO, GuidingPrincipalSO, CampaignSO, CharacterSO, CrafterSO, ArtifactSO"`  
**Next session:** STAGE_01_D.md  

---

## Step 1: PackMonsterSO

**Path:** `Assets/_Game/Scripts/Core.Data/PackMonsterSO.cs`

> ⚑ This is the ONLY monster that uses this class. All other monsters use MonsterSO directly.

```csharp
using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/PackMonster", fileName = "New PackMonster")]
    public class PackMonsterSO : MonsterSO
    {
        [Header("Herd-Specific")]
        public int unitCount;               // Always 3 for The Ivory Stampede
        // Shared deck defined in base MonsterSO openingCards/escalationCards/apexCards
        // Each elephant's health tracked at runtime — not stored here
        // Aggro rule on kill: KILLING_BLOW_HOLDER (constant, no data field needed)
    }
}
```

---

## Step 2: EventSO Supporting Struct + EventSO

Add `EventChoice` struct to `DataStructs.cs` first:

```csharp
// Add inside namespace MnM.Core.Data in DataStructs.cs
[System.Serializable]
public struct EventChoice
{
    public string choiceLabel;              // "A:" or "B:"
    [TextArea] public string outcomeText;
    [TextArea] public string mechanicalEffect;  // Human-readable; resolved by Core.Logic
    public string artifactUnlockId;
    public string codexEntryId;
    public string guidingPrincipalTrigger;
}
```

**Path:** `Assets/_Game/Scripts/Core.Data/EventSO.cs`

```csharp
using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/Event", fileName = "New Event")]
    public class EventSO : ScriptableObject
    {
        public string eventId;              // e.g. "EVT-01"
        public string eventName;
        public int yearRangeMin;
        public int yearRangeMax;
        public bool isMandatory;
        public string campaignTag;
        public string monsterTag;           // Empty if not monster-specific
        public string seasonTag;
        public string difficultyTag;
        [TextArea] public string narrativeText;
        // Max 2 choices. 0 choices = mandatory outcome, no player decision.
        public EventChoice[] choices;
    }
}
```

---

## Step 3: InnovationSO

**Path:** `Assets/_Game/Scripts/Core.Data/InnovationSO.cs`

```csharp
using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/Innovation", fileName = "New Innovation")]
    public class InnovationSO : ScriptableObject
    {
        public string innovationId;         // e.g. "INN-01"
        public string innovationName;
        [TextArea] public string effect;
        // Cards added to the Innovation Deck pool when this is adopted
        public InnovationSO[] addsToDeck;
        public string gritSkillUnlocked;    // Empty if none
        public GuidingPrincipalTag guidingPrincipalTag;
    }
}
```

---

## Step 4: GuidingPrincipalSO

**Path:** `Assets/_Game/Scripts/Core.Data/GuidingPrincipalSO.cs`

```csharp
using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/GuidingPrincipal", fileName = "New GuidingPrincipal")]
    public class GuidingPrincipalSO : ScriptableObject
    {
        public string principalId;          // e.g. "GP-01"
        public string principalName;
        [TextArea] public string triggerCondition;
        public EventChoice choiceA;
        public EventChoice choiceB;
    }
}
```

---

## Step 5: CampaignSO

**Path:** `Assets/_Game/Scripts/Core.Data/CampaignSO.cs`

```csharp
using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/Campaign", fileName = "New Campaign")]
    public class CampaignSO : ScriptableObject
    {
        [Header("Identity")]
        public string campaignName;
        public DifficultyLevel difficulty;
        public int campaignLengthYears;     // Default 30

        [Header("Starting Conditions")]
        public int startingCharacterCount;  // 6 / 8 / 10 per difficulty
        public int baseMovement;
        public int startingGrit;
        public bool ironmanMode;

        [Header("Content Pools")]
        public MonsterSO[] monsterRoster;
        public EventSO[] eventPool;
        public InnovationSO[] startingInnovations;  // Base deck — seeds 12 cards
        public CrafterSO[] crafterPool;
        public GuidingPrincipalSO[] guidingPrincipals;

        [Header("Thresholds")]
        public int retirementHuntCount;
        public int birthConditionAge;

        [Header("Overlord")]
        public MonsterSO overlordMonster;           // Year 30 final boss
        public int[] overlordApproachYears;         // Years where Overlord warning events fire
    }
}
```

---

## Step 6: WeaponProficiency Struct + CharacterSO

Add `WeaponProficiency` struct to `DataStructs.cs`:

```csharp
// Add inside namespace MnM.Core.Data in DataStructs.cs
[System.Serializable]
public struct WeaponProficiency
{
    public WeaponType weaponType;
    public int tier;                        // 1–5
    public int successfulActivations;
}
```

**Path:** `Assets/_Game/Scripts/Core.Data/CharacterSO.cs`

```csharp
using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/Character", fileName = "New Character")]
    public class CharacterSO : ScriptableObject
    {
        [Header("Identity")]
        public string characterName;
        public CharacterBuild bodyBuild;
        public CharacterSex sex;

        [Header("Stats — modified by events/innovations only, never by leveling")]
        public int accuracy;
        public int evasion;
        public int strength;
        public int toughness;
        public int luck;
        public int movement;

        [Header("Deck")]
        public ActionCardSO[] currentDeck;
        public InjuryCardSO[] injuryCards;
        public ActionCardSO[] fightingArts;
        public ActionCardSO[] disorders;

        [Header("Proficiency")]
        public WeaponProficiency[] weaponProficiencies;

        [Header("State")]
        public int huntCount;
        public bool isRetired;

        [Header("Gear")]
        public ItemSO[] equippedItems;
        public WeaponSO equippedWeapon;
    }
}
```

---

## Step 7: CrafterSO

**Path:** `Assets/_Game/Scripts/Core.Data/CrafterSO.cs`

```csharp
using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/Crafter", fileName = "New Crafter")]
    public class CrafterSO : ScriptableObject
    {
        public string crafterName;
        public string monsterTag;               // Monster materials that unlock this
        public int materialTier;
        public ItemSO[] recipeList;
        public ResourceSO[] unlockCost;
        public int[] unlockCostAmounts;         // Parallel array with unlockCost
        // Settlement scene placement
        public Vector2 settlementScenePosition;
        public string spriteAssetPath;
    }
}
```

---

## Step 8: ArtifactSO

**Path:** `Assets/_Game/Scripts/Core.Data/ArtifactSO.cs`

```csharp
using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/Artifact", fileName = "New Artifact")]
    public class ArtifactSO : ScriptableObject
    {
        public string artifactId;
        public string artifactName;
        public CodexCategory codexCategory;
        [TextArea] public string loreText;
        public string unlockCondition;
        // yearFound set at runtime — not stored in SO
    }
}
```

---

## Verification Test

In the Unity Editor:

1. Confirm zero compiler errors in the Console
2. Right-click → Create → MnM → confirm the full menu now contains:
   - `MnM/Resource`
   - `MnM/Cards/ActionCard`
   - `MnM/Cards/BehaviorCard`
   - `MnM/Cards/InjuryCard`
   - `MnM/Item`
   - `MnM/Weapon`
   - `MnM/Monster`
   - `MnM/PackMonster`
   - `MnM/Event`
   - `MnM/Innovation`
   - `MnM/GuidingPrincipal`
   - `MnM/Campaign`
   - `MnM/Character`
   - `MnM/Crafter`
   - `MnM/Artifact`
3. Create one `CampaignSO` and confirm all sections appear in the Inspector
4. Confirm `DataStructs.cs` now contains: `LinkPoint`, `MonsterStatBlock`, `MonsterBodyPart`, `FacingTable`, `LootEntry`, `StanceDefinition`, `EventChoice`, `WeaponProficiency`

If all 4 checks pass — this session is done.

---

## Next Session

**File:** `_Docs/Stage_01/STAGE_01_D.md`  
**Covers:** Creating mock data assets in the Editor to verify the full data layer before any logic is written
