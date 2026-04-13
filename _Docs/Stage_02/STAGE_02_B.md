<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 2-B | Combat State Classes
Status: Stage 2-A complete. IGridManager, ICombatManager,
IMonsterAI interfaces approved and compiling.
Task: Create all JSON-serializable combat state classes.
No MonoBehaviours. No Unity engine types in any state class.
No gameplay logic — data containers only.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_02/STAGE_02_B.md
- Assets/_Game/Scripts/Core.Data/Enums.cs
- Assets/_Game/Scripts/Core.Data/DataStructs.cs

Then confirm:
- What files you will create
- That no state class will contain Transform, GameObject,
  Vector3, MonoBehaviour, or Component
- What you will NOT touch this session

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 2-B: Combat State Classes

**Resuming from:** Stage 2-A complete — interfaces approved  
**Done when:** All state classes compile, `JsonUtility.ToJson()` round-trip test passes with no data loss  
**Commit:** `"2B: CombatState and all sub-state classes — JSON round-trip verified"`  
**Next session:** STAGE_02_C.md  

---

## Hard Rules for This Session

- Every class in this file implements `IJsonSerializable`
- No Unity engine types: no `Transform`, `GameObject`, `Vector3`, `MonoBehaviour`, `Component`
- `Vector2Int` is acceptable — it serializes cleanly
- Grid positions stored as separate `int` fields (`gridX`, `gridY`) not `Vector2Int` — cleaner JSON
- Use `string` for phase and difficulty — not enums — so JSON is human-readable

---

## Step 1: CombatState.cs

**Path:** `Assets/_Game/Scripts/Core.Data/CombatState.cs`

```csharp
using System;
using MnM.Core.Data;

namespace MnM.Core.Data
{
    [Serializable]
    public class CombatState : IJsonSerializable
    {
        public string campaignId;
        public int campaignYear;
        public int currentRound;
        public string currentPhase;         // "VitalityPhase", "HunterPhase", etc.
        public string aggroHolderId;        // hunterId of current Aggro Token holder
        public HunterCombatState[] hunters;
        public MonsterCombatState monster;
        public GridState grid;
        public string[] log;                // Round-by-round event log entries
    }

    [Serializable]
    public class HunterCombatState : IJsonSerializable
    {
        public string hunterId;
        public string hunterName;
        // Grid position — separate ints for clean JSON
        public int gridX;
        public int gridY;
        public int facingX;                 // Unit vector: -1, 0, or 1
        public int facingY;
        // Combat stats
        public int currentGrit;
        public int maxGrit;
        public int apRemaining;             // Resets to 2 each Hunter Phase
        public bool hasActedThisPhase;
        public bool isCollapsed;
        // Body zones: Head, Torso, LeftArm, RightArm, LeftLeg, RightLeg
        public BodyZoneState[] bodyZones;
        // Deck state
        public string[] handCardNames;
        public string[] deckCardNames;
        public string[] discardCardNames;
        // Active status effects as string tags e.g. ["Shaken", "Pinned"]
        public string[] activeStatusEffects;
    }

    [Serializable]
    public struct BodyZoneState : IJsonSerializable
    {
        public string zone;                 // "Head", "Torso", "LeftArm", etc.
        public int shellCurrent;
        public int shellMax;
        public int fleshCurrent;
        public int fleshMax;
    }

    [Serializable]
    public class MonsterCombatState : IJsonSerializable
    {
        public string monsterName;
        public string difficulty;           // "Standard", "Hardened", "Apex"
        public int gridX;
        public int gridY;
        public int facingX;
        public int facingY;
        public int footprintW;
        public int footprintH;
        public MonsterPartState[] parts;
        public string[] activeDeckCardNames;
        public string[] removedCardNames;
        public string currentStanceTag;
        public string[] activeStatusEffects;
    }

    [Serializable]
    public struct MonsterPartState : IJsonSerializable
    {
        public string partName;
        public int shellCurrent;
        public int shellMax;
        public int fleshCurrent;
        public int fleshMax;
        public bool isBroken;
        public bool isRevealed;             // Trap zones start false
        public bool isExposed;
        public int woundCount;              // Tracks which wound removal to apply next
    }

    [Serializable]
    public class GridState : IJsonSerializable
    {
        public int width;                   // Always 22
        public int height;                  // Always 16
        public DeniedCell[] deniedCells;
        public string[] marrowSinkCells;    // Encoded as "x,y" e.g. "5,3"
    }

    [Serializable]
    public struct DeniedCell : IJsonSerializable
    {
        public int x;
        public int y;
        public int roundsRemaining;
    }

    [Serializable]
    public struct CombatResult
    {
        public bool isVictory;
        public string[] collapsedHunterIds;
        public string[] removedBehaviorCardNames;   // For loot calculation
        public int roundsElapsed;
    }
}
```

---

## Step 2: CombatStateFactory.cs

A helper that builds a valid starting `CombatState` from mock data. Used in verification and later by `CampaignInitializer`.

**Path:** `Assets/_Game/Scripts/Core.Data/CombatStateFactory.cs`

```csharp
using System.Collections.Generic;

namespace MnM.Core.Data
{
    public static class CombatStateFactory
    {
        // Builds a minimal valid CombatState for testing
        // Uses the canonical mock scenario: Aldric vs Gaunt Standard
        public static CombatState BuildMockCombatState()
        {
            var aldric = new HunterCombatState
            {
                hunterId        = "hunter_aldric",
                hunterName      = "Aldric",
                gridX           = 5,
                gridY           = 8,
                facingX         = 1,    // Facing East
                facingY         = 0,
                currentGrit     = 3,
                maxGrit         = 3,
                apRemaining     = 2,
                hasActedThisPhase = false,
                isCollapsed     = false,
                bodyZones       = BuildHunterBodyZones(),
                handCardNames   = new string[0],
                deckCardNames   = new[] { "Brace", "Shove" },
                discardCardNames = new string[0],
                activeStatusEffects = new string[0],
            };

            var gaunt = new MonsterCombatState
            {
                monsterName     = "The Gaunt",
                difficulty      = "Standard",
                gridX           = 12,
                gridY           = 7,
                facingX         = -1,   // Facing West
                facingY         = 0,
                footprintW      = 2,
                footprintH      = 2,
                parts           = BuildGauntStandardParts(),
                activeDeckCardNames = new[] { "Creeping Advance", "Scent Lock", "Flank Sense" },
                removedCardNames = new string[0],
                currentStanceTag = "",
                activeStatusEffects = new string[0],
            };

            return new CombatState
            {
                campaignId      = "mock_campaign",
                campaignYear    = 1,
                currentRound    = 0,
                currentPhase    = "VitalityPhase",
                aggroHolderId   = "hunter_aldric",
                hunters         = new[] { aldric },
                monster         = gaunt,
                grid            = BuildEmptyGrid(),
                log             = new string[0],
            };
        }

        private static BodyZoneState[] BuildHunterBodyZones()
        {
            return new[]
            {
                new BodyZoneState { zone = "Head",     shellCurrent = 2, shellMax = 2, fleshCurrent = 3, fleshMax = 3 },
                new BodyZoneState { zone = "Torso",    shellCurrent = 2, shellMax = 2, fleshCurrent = 3, fleshMax = 3 },
                new BodyZoneState { zone = "LeftArm",  shellCurrent = 1, shellMax = 1, fleshCurrent = 2, fleshMax = 2 },
                new BodyZoneState { zone = "RightArm", shellCurrent = 1, shellMax = 1, fleshCurrent = 2, fleshMax = 2 },
                new BodyZoneState { zone = "LeftLeg",  shellCurrent = 1, shellMax = 1, fleshCurrent = 2, fleshMax = 2 },
                new BodyZoneState { zone = "RightLeg", shellCurrent = 1, shellMax = 1, fleshCurrent = 2, fleshMax = 2 },
            };
        }

        private static MonsterPartState[] BuildGauntStandardParts()
        {
            // Shell 2 / Flesh 3 per part — Standard difficulty
            string[] partNames = { "Head", "Throat", "Torso", "Left Flank", "Right Flank", "Hind Legs", "Tail" };
            var parts = new List<MonsterPartState>();
            foreach (var name in partNames)
            {
                parts.Add(new MonsterPartState
                {
                    partName     = name,
                    shellCurrent = 2, shellMax = 2,
                    fleshCurrent = 3, fleshMax = 3,
                    isBroken     = false,
                    isRevealed   = true,    // No trap zones on Standard Gaunt
                    isExposed    = false,
                    woundCount   = 0,
                });
            }
            return parts.ToArray();
        }

        private static GridState BuildEmptyGrid()
        {
            return new GridState
            {
                width         = 22,
                height        = 16,
                deniedCells   = new DeniedCell[0],
                marrowSinkCells = new string[0],
            };
        }
    }
}
```

---

## Verification Test

Create a temporary test MonoBehaviour to run this — delete it after verification:

**Path:** `Assets/_Game/Scripts/Core.Data/CombatStateTest.cs` *(delete after test passes)*

```csharp
using UnityEngine;
using MnM.Core.Data;

public class CombatStateTest : MonoBehaviour
{
    private void Start()
    {
        var state = CombatStateFactory.BuildMockCombatState();

        // JSON round-trip test
        string json = JsonUtility.ToJson(state, prettyPrint: true);
        Debug.Log("[CombatStateTest] JSON output:\n" + json);

        var restored = JsonUtility.FromJson<CombatState>(json);

        // Verify key fields survived round-trip
        Debug.Assert(restored.hunters[0].hunterName == "Aldric",
            "FAIL: hunterName did not survive JSON round-trip");
        Debug.Assert(restored.monster.parts.Length == 7,
            "FAIL: monster parts count wrong after JSON round-trip");
        Debug.Assert(restored.grid.width == 22,
            "FAIL: grid width wrong after JSON round-trip");
        Debug.Assert(restored.currentPhase == "VitalityPhase",
            "FAIL: currentPhase wrong after JSON round-trip");

        Debug.Log("[CombatStateTest] ✓ All JSON round-trip assertions passed");
    }
}
```

Attach `CombatStateTest` to any GameObject in an empty scene, press Play, confirm all assertions pass in Console, then **delete the test script**.

---

## Next Session

**File:** `_Docs/Stage_02/STAGE_02_C.md`  
**Covers:** GridManager implementation — the concrete class that implements IGridManager
