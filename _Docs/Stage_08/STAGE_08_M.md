<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude Code to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 8-M | Monster Health Rework — Behavior Deck as Life & Wound Locations
Status: Stage 8-L complete. Monster execution engine (first pass) done.
Playtesting revealed the shell/flesh HP system created frustrating combat
pacing. This session replaces the data model entirely.
Task: Rework the monster health system. Shells and flesh wounds are
removed. Behavior cards are the monster's only health. A separate wound
location deck is introduced. No escalation logic — all behavior cards
for a difficulty are shuffled into a single starting deck.

Read these files before doing anything:
- CLAUDE.md
- _Docs/Stage_08/STAGE_08_M.md
- Assets/_Game/Scripts/Core.Data/Enums.cs
- Assets/_Game/Scripts/Core.Data/DataStructs.cs
- Assets/_Game/Scripts/Core.Data/MonsterSO.cs
- Assets/_Game/Scripts/Core.Data/BehaviorCardSO.cs
- Assets/_Game/Editor/MockDataCreator.cs

Then confirm:
- Which fields you are REMOVING from existing files
- Which new files you are CREATING
- What you will NOT touch this session

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 8-M: Monster Health Rework — Behavior Deck as Life & Wound Locations

**Resuming from:** Stage 8-L complete — monster execution engine (first pass); playtesting revealed shell/flesh HP pacing was frustrating; this session reworks the data model  
**Done when:** All data model files compile clean; MockDataCreator updated to new format; Gaunt SO inspectable in Editor with new deck structure  
**Commit:** `"8M: Monster health rework — behavior deck as life, wound location deck, mood cards"`  
**Next session:** STAGE_08_N.md — Wire up the runtime implementation: BehaviorDeck/WoundDeck wrappers, MonsterAI rebuild, wound resolution, Gaunt SO assets

---

## Design Philosophy

The shell/flesh/body-part HP system is replaced with a Kingdom Death: Monster-inspired approach:

- **Behavior cards are the monster's only health.** When all removable behavior cards are gone the monster is defeated.
- **Wound locations are a separate shuffled deck** drawn on a successful hit — they tell the hunter what Force roll is needed to wound and what happens on failure, wound, and critical.
- **No escalation logic.** All behavior cards for a given difficulty are shuffled into a single starting deck. Harder difficulties have more cards (more health) and more powerful cards.
- **Facing affects to-hit only.** Wound location draws are pure random from the full wound deck.

---

## What This Stage Touches

| File | Action |
|---|---|
| `Enums.cs` | Remove `BehaviorGroup`, remove `DamageType`, update `BehaviorCardType`, add `WoundOutcome` |
| `DataStructs.cs` | Remove `MonsterBodyPart`, update `MonsterStatBlock`, add `FacingAccuracyBonus` |
| `WoundLocationSO.cs` | **New file** |
| `BehaviorCardSO.cs` | Remove `group`, add sub-phase fields + critical wound alternate behavior |
| `MonsterSO.cs` | Replace 4 behavior arrays + 3 body part arrays with 3 difficulty decks + 3 wound decks; replace facing tables |
| `MockDataCreator.cs` | Update Gaunt mock data to new format |

**Not touched this session:** Any Core.Systems, Core.Logic, Core.UI, or scene files. Runtime combat logic is Stage 8-N onward.

---

## Step 1: Enums.cs — Remove Obsolete, Update BehaviorCardType, Add WoundOutcome

**Remove entirely:**
- `BehaviorGroup { Opening, Escalation, Apex }` — escalation no longer exists
- `DamageType { Shell, Flesh }` — shell/flesh HP no longer exists

**Replace:**
```csharp
// OLD
public enum BehaviorCardType { Removable, Permanent, SingleTrigger }

// NEW
public enum BehaviorCardType { Removable, Mood, SingleTrigger }
```

`Mood` replaces `Permanent`. Unlike the old Permanent type, Mood cards that are removed from play return to the behavior discard pile and can be reshuffled — they are not gone forever.

**Add:**
```csharp
public enum WoundOutcome { Wound, Critical, Failure, Trap }
```

Used at runtime by the wound resolution system to branch outcomes.

---

## Step 2: DataStructs.cs — Remove MonsterBodyPart, Update MonsterStatBlock, Add FacingAccuracyBonus

**Remove `MonsterBodyPart` entirely.** Wound location SOs replace per-part HP.

**Update `MonsterStatBlock`** — remove the now-redundant deck size field; deck size is derived at runtime from the card array length:

```csharp
[System.Serializable]
public struct MonsterStatBlock
{
    public int movement;
    public int accuracy;
    public int strength;
    public int toughness;
    public int evasion;
    // REMOVED: behaviorDeckSizeRemovable — deck size is now BehaviorCardSO[].Length
}
```

**Add `BehaviorDeckComposition`** — defines how many cards to randomly draw from each pool when building a monster's deck at combat start:

```csharp
[System.Serializable]
public struct BehaviorDeckComposition
{
    public int baseCardCount;           // Cards drawn from the monster's base pool
    public int advancedCardCount;       // Cards drawn from the advanced pool
    public int overwhelmingCardCount;   // Cards drawn from the overwhelming pool
    // Total drawn = health pool for this difficulty.
    // Pools are larger than counts — each fight uses a different random subset.
}
```

**Add `FacingAccuracyBonus`** — facing now only affects the to-hit roll, not wound location draws:

```csharp
[System.Serializable]
public struct FacingAccuracyBonus
{
    public FacingArc arc;
    public int accuracyModifier;    // e.g. Rear = +2 (easier to hit from behind)
}
```

---

## Step 3: WoundLocationSO.cs (New File)

**Path:** `Assets/_Game/Scripts/Core.Data/WoundLocationSO.cs`

```csharp
using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/Cards/WoundLocation", fileName = "New WoundLocation")]
    public class WoundLocationSO : ScriptableObject
    {
        [Header("Identity")]
        public string locationName;         // e.g. "Monster Mouth", "Gaunt Claw"
        public BodyPartTag partTag;         // Narrative / future custom sprite — not used for draw filtering

        [Header("Wound Threshold")]
        public int woundTarget;             // d10 + Hunter.Strength > this = wound
                                            // Critical: d10 result alone >= (10 - Hunter.Luck)

        [Header("Trap")]
        public bool isTrap;                 // True = monster responds, no wound, no behavior card removed
        [TextArea] public string trapEffect;

        [Header("Impervious")]
        public bool isImpervious;           // True = force roll cannot remove a behavior card here
                                            // Wound/critical effects still fire; resources still granted
                                            // Strategic value: criticals set wound tags that alter behavior cards
                                            // Does NOT interact with isTrap — a location is one or the other

        [Header("Outcomes")]
        [TextArea] public string failureEffect;   // Fires when force roll fails (and not a trap)
        [TextArea] public string woundEffect;     // Additional effect beyond removing one behavior card
        [TextArea] public string criticalEffect;  // Fires on critical in addition to woundEffect

        [Header("Critical Wound Tracking")]
        public string criticalWoundTag;     // Runtime flag set when a critical lands here
                                            // e.g. "GauntMouth_Critical"
                                            // Behavior cards read this tag to alter their resolution

        [Header("Resources")]
        public ResourceEntry[] woundResources;      // Resources gained on a wound
        public ResourceEntry[] criticalResources;   // Additional resources on a critical (stacks with woundResources)
    }
}
```

### Trap card rules
When a trap wound location is drawn:
1. `trapEffect` fires (monster attacks or responds)
2. Wound location card → WoundDiscard
3. WoundDiscard reshuffles into WoundDeck immediately (trap cards cycle back)
4. No behavior card is removed from the monster

### Impervious location rules
When a non-trap wound location with `isImpervious == true` is drawn:
1. Force roll runs normally — the hunter must still beat the woundTarget
2. On wound or critical: `woundEffect` fires, `woundResources` granted; NO behavior card removed
3. On critical: additionally sets `criticalWoundTag`, fires `criticalEffect`, grants `criticalResources`; still NO behavior card removed
4. On failure: `failureEffect` fires as normal
5. Wound location → WoundDiscard as normal
The primary gameplay value of impervious criticals is setting `criticalWoundTag` flags that alter how specific behavior cards resolve — a bone-plated shoulder can't be wounded through, but a critical fracture forces the monster's posture to change.

### Wound deck reshuffle rule
When WoundDeck is empty before a draw, shuffle the entire WoundDiscard back into WoundDeck. Trap cards cycle back in as normal.

---

## Step 4: BehaviorCardSO.cs — Rework

**Path:** `Assets/_Game/Scripts/Core.Data/BehaviorCardSO.cs`

Full replacement:

```csharp
using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/Cards/BehaviorCard", fileName = "New BehaviorCard")]
    public class BehaviorCardSO : ScriptableObject
    {
        [Header("Identity")]
        public string cardName;
        public BehaviorCardType cardType;   // Removable, Mood, or SingleTrigger
        // REMOVED: BehaviorGroup group

        [Header("Trigger & Effect")]
        [TextArea] public string triggerCondition;
        [TextArea] public string effectDescription;

        [Header("Monster Turn Sub-Phases")]
        public bool hasTargetIdentification;
        public string targetRule;           // e.g. "nearest", "aggro", "mostInjured", "last_attacker"
        public bool hasMovement;
        public bool hasDamage;
        public string forcedHunterBodyPart; // Leave empty for random roll; override e.g. "Head", "Torso"

        [Header("Mood Card — Removal Condition")]
        [TextArea] public string removalCondition;
        // Examples:
        //   "Hunter spends 1 Grit"
        //   "Hunter inflicts a wound"
        //   "3 turns"
        // Evaluated by Core.Logic each turn. When met: card → BehaviorDiscard (re-enters health pool)

        [Header("Critical Wound — Alternate Behavior")]
        public string criticalWoundCondition;           // Tag from WoundLocationSO.criticalWoundTag
                                                        // e.g. "GauntMouth_Critical"
        [TextArea] public string alternateTriggerCondition;
        [TextArea] public string alternateEffectDescription;
        // If criticalWoundCondition is set and that tag is active at runtime,
        // the alternate fields replace triggerCondition and effectDescription for this draw.

        [Header("Tags")]
        public string stanceTag;
        public string groupTag;
    }
}
```

**Key changes from old version:**
- `BehaviorGroup group` removed
- `removalCondition` repurposed: now specifically describes how a Mood card leaves the "in play" zone
- New sub-phase booleans (`hasTargetIdentification`, `hasMovement`, `hasDamage`) drive the structured monster turn flow
- `targetRule` and `forcedHunterBodyPart` give the card authoring control over targeting
- New `criticalWoundCondition` + `alternate*` fields allow wound state to alter how a card resolves

---

## Step 5: MonsterSO.cs — Rework Deck Structure and Facing

Full replacement:

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

        [Header("Behavior Card Pools")]
        // Cards are randomly drawn from these pools at combat start to build the fight's deck.
        // Pools are authored larger than any single deck — each fight draws a different subset,
        // making repeat encounters feel varied even against the same monster.
        public BehaviorCardSO[] baseCardPool;           // Core cards; available at all difficulties
        public BehaviorCardSO[] advancedCardPool;       // More complex / dangerous cards
        public BehaviorCardSO[] overwhelmingCardPool;   // Apex-tier — peak threat cards

        [Header("Behavior Deck Composition — index 0=Standard, 1=Hardened, 2=Apex")]
        // How many cards to draw from each pool per difficulty.
        // Example: Standard = 12 base + 3 advanced + 0 overwhelming (15 health total)
        //          Hardened  = 14 base + 4 advanced + 2 overwhelming (20 health total)
        // Random draw uses Fisher-Yates on each pool, take first N — see MonsterAI.InitializeDeck
        public BehaviorDeckComposition[] deckCompositions;

        [Header("Wound Location Deck — per Difficulty")]
        // Can be customized per difficulty. Harder difficulties may add
        // higher woundTarget locations or additional traps.
        public WoundLocationSO[] standardWoundDeck;
        public WoundLocationSO[] hardenedWoundDeck;
        public WoundLocationSO[] apexWoundDeck;

        [Header("Elemental Profile")]
        public ElementTag[] weaknesses;
        public ElementTag[] resistances;

        [Header("Facing — Accuracy Modifiers Only")]
        // Wound location draws are NOT filtered by facing (pure random from full wound deck).
        // Facing only modifies the hunter's to-hit roll.
        public FacingAccuracyBonus[] facingBonuses;

        [Header("Loot")]
        public LootEntry[] lootTable;

        [Header("Stances")]
        public StanceDefinition[] stances;
    }
}
```

**Removed fields:**
- `openingCards`, `escalationCards`, `apexCards`, `permanentCards`
- `standardDeck`, `hardenedDeck`, `apexDeck` (replaced by pool arrays + deckCompositions)
- `standardParts`, `hardenedParts`, `apexParts` (MonsterBodyPart arrays)
- `frontFacing`, `flankFacing`, `rearFacing` (FacingTable — weighted zone draw system)
- `trapZoneParts`

---

## Step 6: Runtime State Design (Reference for Stage 8-N)

This section describes what `CombatState` must track. Do not implement here — implementation is Stage 8-N.

### Deck construction at combat start
`MonsterAI.InitializeDeck(MonsterSO, difficulty)` builds the starting `behaviorDeck` by:
1. Read `deckCompositions[difficultyIndex]` to get counts per pool
2. Shuffle each pool independently (Fisher-Yates)
3. Take the first N cards from each shuffled pool
4. Combine all drawn cards into one list, shuffle the combined list
5. That combined list becomes the starting `behaviorDeck`

This means two Standard-difficulty Gaunt fights can have different cards in the deck (different draws from the base pool), while the total health pool size stays predictable.

### Per-monster runtime state
```
behaviorDeck         : List<BehaviorCardSO>     // shuffled draw pile
behaviorDiscard      : List<BehaviorCardSO>     // standard discard; reshuffled when deck empty
moodCardsInPlay      : List<BehaviorCardSO>     // active Mood cards (ongoing effects)
permanentlyRemoved   : List<BehaviorCardSO>     // wounded away or SingleTrigger fired — never return
woundDeck            : List<WoundLocationSO>    // shuffled draw pile
woundDiscard         : List<WoundLocationSO>    // reshuffled when empty (trap cards cycle back)
criticalWoundTags    : HashSet<string>          // tags flagged by landed criticals
```

### Health pool (computed, read-only)
```
monsterHealth = behaviorDeck.Count + behaviorDiscard.Count + moodCardsInPlay.Count
```
Mood cards in play are counted but cannot be removed by wounds while active. SingleTrigger cards that have fired and permanentlyRemoved cards do not contribute.

### Defeat condition
```
IF behaviorDeck.Count + behaviorDiscard.Count == 0
    → Monster defeated immediately
```
Mood cards currently in play do not block defeat. This prevents the player from being locked in a state where the only remaining "health" is a Mood card that cannot currently be removed. When a Mood card is removed from play (its `removalCondition` is met), it goes to `behaviorDiscard` — if the deck was empty, it would now count again and the defeat condition would not yet be met.

### Per-hunter runtime state (dependency for Grit windows)
```
currentGrit : int   // initialized from CampaignSO.startingGrit; tracked per hunter
```
`CharacterSO` does not need a Grit field — Grit is a runtime combat resource, not a persistent character stat.

> **Behavior deck contract — position-aware ordered list**
>
> The runtime `behaviorDeck` is a `List<BehaviorCardSO>` where **index 0 = top of deck** (next to draw). The shuffle algorithm (Fisher-Yates) produces a fully ordered list — every card's position is known and addressable at all times.
>
> **Default wound removal:** top card of `behaviorDeck` (index 0 → `permanentlyRemoved`). If deck is empty, shuffle `behaviorDiscard` first, then remove top.
>
> **Hunter abilities and Grit spends may use these deck operations — all supported natively by `List<T>`:**
>
> | Operation | Description | Example use |
> |---|---|---|
> | `Draw()` | Remove and return index 0 | Normal monster turn |
> | `PeekTop()` | Read index 0 without removing | Hunter ability: "look at next behavior card" |
> | `PeekTop(n)` | Read indices 0..n-1 without removing | Hunter ability: "look at top 3 cards" |
> | `MoveTopToBottom()` | Remove index 0, append to end | Hunter ability: "push top card to bottom of deck" |
> | `ReorderTop(n, newOrder)` | Replace indices 0..n-1 with caller-supplied permutation | Hunter ability: "rearrange top 3 as you see fit" |
> | `RemoveSpecific(card)` | Remove a specific card by reference | Grit spend: "choose which behavior card is removed on this wound" |
>
> Stage 8-N should implement these as methods on a `BehaviorDeck` wrapper class (not raw list manipulation at the call site) so all deck interaction goes through a single auditable interface.

---

## Step 7: Wound Resolution Flow

Triggered when a hunter's attack roll succeeds (hits the monster):

```
1. DRAW top card of WoundDeck
   (if WoundDeck empty → shuffle WoundDiscard → draw)

   ┌── isTrap == true ──────────────────────────────────────────┐
   │  trapEffect fires                                           │
   │  WoundLocation → WoundDiscard                              │
   │  WoundDiscard reshuffles into WoundDeck immediately        │
   │  No behavior card removed                                  │
   │  → DONE                                                    │
   └────────────────────────────────────────────────────────────┘

2. FORCE ROLL (not a trap)
   Hunter rolls d10

   Wound check (resolved first):
     d10 + Hunter.Strength > woundLocation.woundTarget → WOUND
     d10 + Hunter.Strength <= woundLocation.woundTarget → FAILURE
     (A high d10 that still fails the wound check is not a critical — it just fails)

   Critical sub-check (only when wound check passes):
     d10 natural result >= (10 - Hunter.Luck) → CRITICAL WOUND
     Otherwise → standard WOUND

3a. FAILURE
    failureEffect fires
    WoundLocation → WoundDiscard
    No behavior card removed

3b. WOUND (non-critical, non-impervious)
    woundEffect fires (if any)
    woundResources granted to hunter
    Top card of BehaviorDeck → permanentlyRemoved
      (if BehaviorDeck empty: shuffle BehaviorDiscard first, then remove top)
    WoundLocation → WoundDiscard
    → Run DEFEAT CHECK

3c. CRITICAL WOUND (non-impervious)
    criticalWoundTag added to criticalWoundTags runtime set
    criticalEffect fires (in addition to woundEffect)
    woundResources + criticalResources granted to hunter
    Top card of BehaviorDeck → permanentlyRemoved
    WoundLocation → WoundDiscard
    → Run DEFEAT CHECK

3d. WOUND — IMPERVIOUS LOCATION (isImpervious == true, wound check passed)
    woundEffect fires (if any)
    woundResources granted to hunter
    NO behavior card removed
    WoundLocation → WoundDiscard
    → No defeat check needed

3e. CRITICAL — IMPERVIOUS LOCATION (isImpervious == true, wound + critical both passed)
    criticalWoundTag added to criticalWoundTags runtime set
    criticalEffect fires (in addition to woundEffect)
    woundResources + criticalResources granted to hunter
    NO behavior card removed
    WoundLocation → WoundDiscard
    → No defeat check needed
    Note: The wound tag set here is the strategic payoff — it alters future behavior card resolutions
```

---

## Step 8: Monster Turn Flow — Behavior Card Sub-Phases with Grit Windows

Each monster turn: draw one behavior card from `behaviorDeck`. If `behaviorDeck` is empty, shuffle `behaviorDiscard` into `behaviorDeck` before drawing. `permanentlyRemoved` cards never re-enter.

**Before resolving:** Check `criticalWoundCondition`. If non-empty and that tag is in `criticalWoundTags`, substitute `alternateTriggerCondition` and `alternateEffectDescription` for this resolution.

```
1.  DRAW behavior card
        ↓
2.  [GRIT WINDOW] — Hunters may spend Grit or use reactions
        ↓
3.  [IF hasTargetIdentification]
        Monster identifies target per targetRule
        ("nearest", "aggro", "mostInjured", "last_attacker")
        ↓
4.  [GRIT WINDOW]
        ↓
5.  [IF hasMovement]
        Monster moves toward/around target
        ↓
6.  [GRIT WINDOW]
        ↓
7.  [IF hasDamage]
        Monster rolls for damage
        Determine hunter wound location:
          IF forcedHunterBodyPart is set → use that part
          ELSE → random from: Head, Torso, Arms, Waist, Legs
        ↓
8.  [GRIT WINDOW]
        ↓
9.  [IF hasDamage]
        Damage applied to hunter at determined body part
        ↓
10. [GRIT WINDOW]
        ↓
11. Resolve card type:
    ├── Removable     → BehaviorDiscard
    ├── Mood          → moodCardsInPlay  (ongoing effect begins)
    └── SingleTrigger → permanentlyRemoved
                        → Run DEFEAT CHECK immediately
        ↓
12. Check all active Mood cards for removal conditions
    IF a Mood card's removalCondition is met:
      → card leaves moodCardsInPlay → BehaviorDiscard
      (re-enters health pool; can be reshuffled and potentially drawn again)
        ↓
13. Run DEFEAT CHECK
        ↓
14. Turn passes
```

### Mood card removal condition patterns

The `removalCondition` string is resolved by `Core.Logic`. Three supported patterns at launch:

| Pattern | Example string | Trigger |
|---|---|---|
| Grit spend | `"Hunter spends 1 Grit"` | Hunter uses a Grit window to pay the cost |
| Wound trigger | `"Hunter inflicts a wound"` | Any successful wound this combat removes the card |
| Turn countdown | `"3 turns"` | Decrement counter each monster turn; remove at 0 |

---

## Step 9: MockDataCreator.cs — Update for New Format

Remove all references to `MonsterBodyPart`, `openingCards`, `escalationCards`, `apexCards`, `permanentCards`, `standardDeck`, `hardenedDeck`, `apexDeck`, `frontFacing`, `flankFacing`, `rearFacing`, and `trapZoneParts`.

Populate the Gaunt with pool arrays and a composition for debug verification. Actual authored content is Stage 8-N.

### Gaunt Behavior Card Pools (mock cards)

**Base Card Pool (4 cards — drawn from for all difficulties):**

| Card name | Type | hasTarget | hasMove | hasDamage | targetRule | forcedBodyPart | Notes |
|---|---|---|---|---|---|---|---|
| Creeping Advance | Removable | false | true | false | — | — | |
| Gaunt Slash | Removable | true | false | true | nearest | — | criticalWoundCondition: "GauntJaw_Critical" |
| Bone Rattle | Mood | false | false | false | — | — | removalCondition: "Hunter inflicts a wound" |
| Brace | Removable | false | false | false | — | — | Reaction; no sub-phases |

**Advanced Card Pool (1 card — drawn from for Hardened+):**

| Card name | Type | hasTarget | hasMove | hasDamage | targetRule | forcedBodyPart | Notes |
|---|---|---|---|---|---|---|---|
| Spear Thrust | SingleTrigger | true | false | true | nearest | Torso | |

**Overwhelming Card Pool:** Empty for mock (authored in Stage 8-N)

### Gaunt Deck Compositions (mock values)

| Difficulty | baseCardCount | advancedCardCount | overwhelmingCardCount | Total health |
|---|---|---|---|---|
| Standard | 3 | 0 | 0 | 3 |
| Hardened | 4 | 1 | 0 | 5 |
| Apex | 4 | 1 | 0 | 5 (update in 8-U) |

The mock Standard deck draws 3 random cards from the 4-card base pool — each Standard fight uses a different trio. This exercises the pool construction logic without requiring full authored content.

Gaunt Slash alternate behavior when GauntJaw_Critical is flagged:  
`alternateTriggerCondition`: "Draws back, jaw hanging"  
`alternateEffectDescription`: "The Gaunt recoils from its wounded jaw — cries out, no attack this turn"

### Gaunt Standard Wound Location Deck (5 mock cards)

| Location name | partTag | woundTarget | isTrap | trapEffect | criticalWoundTag | Notes |
|---|---|---|---|---|---|---|
| Gaunt Jaw | Head | 6 | false | — | GauntJaw_Critical | |
| Gaunt Claw | Arms | 5 | false | — | — | |
| Spiked Tail | Tail | 7 | false | — | — | |
| Bony Shoulder | Torso | 5 | false | — | — | |
| Spine Trap | Back | 0 | true | "Gaunt strikes back for 1 damage before the hunter can react" | — | isTrap=true; woundTarget irrelevant |

### Gaunt facingBonuses (replaces facing tables)

| arc | accuracyModifier |
|---|---|
| Front | 0 |
| Flank | +1 |
| Rear | +2 |

---

## Debug Verification: Aldric vs The Gaunt Standard, Round 1

Use this as the first manual pass after all files compile.

**Setup:**
- Gaunt Standard deck: 3 cards drawn randomly from 4-card base pool (e.g. Creeping Advance, Gaunt Slash, Brace)
- Gaunt Standard wound deck: 5 cards (4 wound locations, 1 trap)
- Aldric: Strength 3, Luck 2 → Critical threshold: d10 ≥ 8

**Hunter Phase — Aldric attacks:**
```
1. To-hit roll succeeds (assume pass for mock)
2. Draw wound location: "Gaunt Jaw" (woundTarget: 6)
3. Force roll: d10 + 3 > 6 → need d10 > 3
   - Roll 5: 5 + 3 = 8 > 6 → WOUND CHECK PASSES
   - Critical sub-check: d10 natural result (5) ≥ 8? No → standard wound
4. Top card of BehaviorDeck → permanentlyRemoved (e.g. "Brace")
5. Gaunt Jaw → WoundDiscard

Debug.Log: "[Wound] Gaunt Jaw — WOUND. Brace permanently removed. Health: deck=1 discard=0 moodInPlay=0"
```

Note: Deck started at 3 cards drawn from pool. After 1 wound removal, deck=1 + discard=0 + moodInPlay=0 = 2 health remaining.

**Monster Phase:**
```
1. Draw: "Creeping Advance" (Removable)
Debug.Log: "[Monster] Drew Creeping Advance | hasTarget:false hasMove:true hasDamage:false"
2. GRIT WINDOW
3. No target identification
4. GRIT WINDOW
5. Monster moves
6. GRIT WINDOW
7. No damage
8. GRIT WINDOW
9. No damage applied
10. GRIT WINDOW
11. Removable → BehaviorDiscard
12. No Mood cards to check
13. Defeat check: deck=1 + discard=1 = 2 > 0 → not defeated
Debug.Log: "[Monster] Creeping Advance discarded. Health: deck=1 discard=1 moodInPlay=0"
```

**Round 1 PASS if:**
- [ ] Deck construction logged: N cards drawn from each pool (e.g. "3 base, 0 advanced drawn from Gaunt pools")
- [ ] Wound location draw logged with woundTarget and outcome
- [ ] Force roll wound check logged first (d10 + Strength > target), then critical sub-check
- [ ] Behavior card removed from deck on wound (permanentlyRemoved count = 1)
- [ ] Monster turn sub-phases logged in order with all Grit windows
- [ ] Health pool count correct after both events
- [ ] No compile errors

---

## Definition of Done — Stage 8-M

- [ ] `Enums.cs` compiles: `BehaviorGroup` removed, `DamageType` removed, `BehaviorCardType` is `{Removable, Mood, SingleTrigger}`, `WoundOutcome` added
- [ ] `DataStructs.cs` compiles: `MonsterBodyPart` removed, `MonsterStatBlock` has no `behaviorDeckSizeRemovable`, `BehaviorDeckComposition` added, `FacingAccuracyBonus` added
- [ ] `WoundLocationSO.cs` created and compiles; `isImpervious`, `isTrap`, all outcome fields visible in Inspector
- [ ] `BehaviorCardSO.cs` updated: `group` field gone, sub-phase booleans and critical wound fields visible in Inspector
- [ ] `MonsterSO.cs` updated: body part arrays gone, escalation arrays gone, fixed deck arrays gone; `baseCardPool`, `advancedCardPool`, `overwhelmingCardPool`, `deckCompositions`, wound decks, and `facingBonuses` all present
- [ ] `MockDataCreator.cs` updated: no references to removed fields; Gaunt pools (4 base, 1 advanced) and compositions (Standard: 3/0/0, Hardened: 4/1/0) populated; wound deck (5 locations) populated
- [ ] Deck construction in `MonsterAI.InitializeDeck` randomly draws from pools per composition counts
- [ ] Wound resolution: wound check runs before critical sub-check; critical only fires when wound check passes
- [ ] Impervious locations: wound/critical effects and resources fire, but no behavior card is removed
- [ ] No compile errors across all assemblies
- [ ] Gaunt SO inspectable in Unity Editor — Inspector shows pool arrays, deckCompositions, wound deck arrays, and facing bonuses

---

## What This Stage Does Not Cover

The following are intentional out-of-scope items for later stages:

- **CombatState.cs** — runtime tracking of deck/discard/moodInPlay/permanentlyRemoved/criticalWoundTags/currentGrit (Stage 8-N)
- **`BehaviorDeck` wrapper class** — position-aware deck operations (Draw, PeekTop, MoveTopToBottom, ReorderTop, RemoveSpecific) that all combat systems go through instead of raw list manipulation (Stage 8-N)
- **Wound resolution logic** — the actual combat code that draws wound locations, runs force rolls, and removes behavior cards (Stage 8-N)
- **Behavior card draw and resolve logic** — the monster turn state machine with Grit windows (Stage 8-N)
- **Mood card removal condition evaluation** — Core.Logic parsing of removalCondition strings (Stage 8-N)
- **Defeat condition check** — runtime combat manager defeat detection (Stage 8-N)
- **Authored Gaunt content** — full Standard and Hardened wound location and behavior decks as real SO assets (Stage 8-N)
- **Grit UI** — per-hunter Grit display in combat HUD (Stage 9+)

---

## Next Session

**File:** `_Docs/Stage_08/STAGE_08_N.md`
**Covers:** New Combat Runtime — implement the runtime side of the new health model: `BehaviorDeck` and `WoundDeck` wrapper classes, rebuilt `MonsterAI.InitializeDeck` (pool-based), rebuilt `MonsterAI.ExecuteCard` (sub-phase flow with Grit windows), `CombatManager.ResolveWound`, defeat condition, and authoring the full Gaunt Standard SO assets
