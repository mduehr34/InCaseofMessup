<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 4-A | CampaignState, RuntimeCharacterState & SaveManager
Status: Stage 3 complete. Full combat loop verified in console.
All Stage 3 test scripts deleted.
Task: Create CampaignState and all sub-state classes
(JSON-serializable, no Unity types), SaveManager, and verify
a clean save/load round-trip to disk before any settlement
logic is written.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_04/STAGE_04_A.md
- Assets/_Game/Scripts/Core.Data/Enums.cs
- Assets/_Game/Scripts/Core.Data/DataStructs.cs

Then confirm:
- What files you will create
- That no state class contains Transform, GameObject,
  Vector3, MonoBehaviour, or Component
- That SaveManager writes to Application.persistentDataPath
- What you will NOT touch this session

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 4-A: CampaignState, RuntimeCharacterState & SaveManager

**Resuming from:** Stage 3 complete  
**Done when:** CampaignState JSON round-trip passes all assertions; SaveManager writes and reads from disk correctly  
**Commit:** `"4A: CampaignState, RuntimeCharacterState, SaveManager — save/load round-trip verified"`  
**Next session:** STAGE_04_B.md  

---

## Hard Rules for This Session

- No Unity engine types in any state class — `int`, `float`, `string`, `bool`, arrays only
- `DifficultyLevel` enum stored as `string` in state (cleaner JSON) — use `difficulty.ToString()` when writing
- `SaveManager` is a static class — no MonoBehaviour
- Save file location: `Application.persistentDataPath/campaign_save.json`

---

## Step 1: CampaignState.cs

**Path:** `Assets/_Game/Scripts/Core.Data/CampaignState.cs`

```csharp
using System;

namespace MnM.Core.Data
{
    [Serializable]
    public class CampaignState : IJsonSerializable
    {
        public string campaignId;
        public string campaignSoName;       // SO asset name — used to reload CampaignSO at runtime
        public int    currentYear;          // 1–30
        public string difficulty;           // "Easy", "Medium", "Hard" — string for clean JSON

        // Characters
        public RuntimeCharacterState[] characters;
        public RuntimeCharacterState[] retiredCharacters;

        // Resources — flat inventory, one entry per resource type
        public ResourceEntry[] resources;

        // Settlement
        public string[] builtCrafterNames;      // CrafterSO asset names of built Crafters
        public string[] availableRecipeNames;   // ItemSO names currently craftable

        // Campaign progression — all IDs stored as strings
        public string[] adoptedInnovationIds;
        public string[] availableInnovationIds; // Current Innovation Deck pool
        public string[] resolvedEventIds;       // EVT-XX IDs already seen
        public string[] unlockedArtifactIds;
        public string[] unlockedCodexEntryIds;
        public string[] activeGuidingPrincipalIds;
        public string[] resolvedGuidingPrincipalIds;

        // Chronicle — human-readable log of all events and decisions
        public string[] chronicleLog;

        // Set after hunt resolves; consumed at start of settlement phase
        public HuntResult pendingHuntResult;
    }

    [Serializable]
    public class RuntimeCharacterState : IJsonSerializable
    {
        public string characterId;      // Guid string — stable across saves
        public string characterName;
        public string bodyBuild;        // "Aethel", "Beorn", etc.
        public string sex;              // "Male" or "Female"

        // Stats — modified ONLY by events/innovations, never by leveling
        public int accuracy;
        public int evasion;
        public int strength;
        public int toughness;
        public int luck;
        public int movement;

        // Deck — card names only; SOs resolved at runtime via registry
        public string[] deckCardNames;
        public string[] injuryCardNames;
        public string[] fightingArtNames;
        public string[] disorderNames;

        // Weapon proficiency — parallel arrays (cleaner JSON than struct arrays)
        public string[] proficiencyWeaponTypes;
        public int[]    proficiencyTiers;
        public int[]    proficiencyActivations;

        // History
        public int  huntCount;
        public bool isRetired;

        // Gear — item and weapon names; SOs resolved at runtime
        public string[] equippedItemNames;
        public string   equippedWeaponName;
    }

    [Serializable]
    public struct ResourceEntry : IJsonSerializable
    {
        public string resourceName;
        public int    amount;
    }

    [Serializable]
    public struct HuntResult : IJsonSerializable
    {
        public bool   isVictory;
        public string monsterName;
        public string monsterDifficulty;
        public int    roundsFought;
        public string[] collapsedHunterIds;
        public string[] survivingHunterIds;
        public ResourceEntry[] lootGained;
        // Parallel with survivingHunterIds — empty string = no injury
        public string[] injuryCardNamesApplied;
    }
}
```

---

## Step 2: SaveManager.cs

**Path:** `Assets/_Game/Scripts/Core.Systems/SaveManager.cs`

```csharp
using System.IO;
using UnityEngine;
using MnM.Core.Data;

namespace MnM.Core.Systems
{
    public static class SaveManager
    {
        private static readonly string SavePath =
            Path.Combine(Application.persistentDataPath, "campaign_save.json");

        // ── Save ─────────────────────────────────────────────────
        public static void Save(CampaignState state)
        {
            string json = JsonUtility.ToJson(state, prettyPrint: true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"[Save] Campaign saved. Year:{state.currentYear} Path:{SavePath}");
        }

        // ── Load ─────────────────────────────────────────────────
        public static CampaignState Load()
        {
            if (!File.Exists(SavePath))
            {
                Debug.Log("[Save] No save file found.");
                return null;
            }

            string json  = File.ReadAllText(SavePath);
            var state    = JsonUtility.FromJson<CampaignState>(json);
            Debug.Log($"[Save] Campaign loaded. Year:{state.currentYear} " +
                      $"Characters:{state.characters?.Length ?? 0}");
            return state;
        }

        // ── Utility ──────────────────────────────────────────────
        public static bool HasSave()    => File.Exists(SavePath);

        public static void DeleteSave()
        {
            if (File.Exists(SavePath)) File.Delete(SavePath);
            Debug.Log("[Save] Save file deleted.");
        }

        public static string GetSavePath() => SavePath;
    }
}
```

---

## Step 3: CampaignStateFactory.cs

A factory for building valid test states — mirrors `CombatStateFactory` from Stage 2.

**Path:** `Assets/_Game/Scripts/Core.Data/CampaignStateFactory.cs`

```csharp
using System;

namespace MnM.Core.Data
{
    public static class CampaignStateFactory
    {
        // Builds a minimal Year 1 campaign state for testing
        // Uses mock data: 2 characters, no resources, no crafters
        public static CampaignState BuildMockYear1State()
        {
            return new CampaignState
            {
                campaignId          = Guid.NewGuid().ToString(),
                campaignSoName      = "Mock_TutorialCampaign",
                currentYear         = 1,
                difficulty          = "Medium",

                characters = new[]
                {
                    BuildMockCharacter("char_aldric",   "Aldric",   "Male",   "Aethel"),
                    BuildMockCharacter("char_brunhild", "Brunhild", "Female", "Eira"),
                },
                retiredCharacters   = new RuntimeCharacterState[0],

                resources           = new ResourceEntry[0],
                builtCrafterNames   = new string[0],
                availableRecipeNames = new string[0],

                adoptedInnovationIds        = new string[0],
                availableInnovationIds      = new[] { "INN-01", "INN-02", "INN-03" }, // Mock seed
                resolvedEventIds            = new string[0],
                unlockedArtifactIds         = new string[0],
                unlockedCodexEntryIds       = new string[0],
                activeGuidingPrincipalIds   = new string[0],
                resolvedGuidingPrincipalIds = new string[0],

                chronicleLog        = new[] { "Year 1: The settlement begins." },
                pendingHuntResult   = default,
            };
        }

        public static RuntimeCharacterState BuildMockCharacter(
            string id, string name, string sex, string build)
        {
            return new RuntimeCharacterState
            {
                characterId             = id,
                characterName           = name,
                bodyBuild               = build,
                sex                     = sex,
                accuracy                = 0,
                evasion                 = 0,
                strength                = 0,
                toughness               = 0,
                luck                    = 0,
                movement                = 3,
                deckCardNames           = new[] { "Brace", "Shove" },
                injuryCardNames         = new string[0],
                fightingArtNames        = new string[0],
                disorderNames           = new string[0],
                proficiencyWeaponTypes  = new[] { "FistWeapon" },
                proficiencyTiers        = new[] { 1 },
                proficiencyActivations  = new[] { 0 },
                huntCount               = 0,
                isRetired               = false,
                equippedItemNames       = new string[0],
                equippedWeaponName      = "",
            };
        }

        // Builds a mock HuntResult for a Gaunt Standard victory
        public static HuntResult BuildMockGauntVictory(string[] hunterIds)
        {
            return new HuntResult
            {
                isVictory           = true,
                monsterName         = "The Gaunt",
                monsterDifficulty   = "Standard",
                roundsFought        = 8,
                collapsedHunterIds  = new string[0],
                survivingHunterIds  = hunterIds,
                lootGained = new[]
                {
                    new ResourceEntry { resourceName = "Gaunt Fang", amount = 2 },
                    new ResourceEntry { resourceName = "Bone",       amount = 2 },
                    new ResourceEntry { resourceName = "Sinew",      amount = 1 },
                },
                injuryCardNamesApplied = new string[hunterIds.Length],
            };
        }
    }
}
```

---

## Verification Test

Temporary test script — delete after passing:

**Path:** `Assets/_Game/Scripts/Core.Systems/SaveManagerTest.cs` *(delete after)*

```csharp
using UnityEngine;
using MnM.Core.Data;
using MnM.Core.Systems;

public class SaveManagerTest : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("=== SAVE MANAGER TEST ===");
        Debug.Log($"[Test] Save path: {SaveManager.GetSavePath()}");

        // Clean slate
        SaveManager.DeleteSave();
        Debug.Assert(!SaveManager.HasSave(), "FAIL: save should not exist after delete");

        // Build mock state
        var state = CampaignStateFactory.BuildMockYear1State();
        Debug.Assert(state.currentYear == 1, "FAIL: year should be 1");
        Debug.Assert(state.characters.Length == 2, "FAIL: should have 2 characters");
        Debug.Assert(state.availableInnovationIds.Length == 3,
            "FAIL: should have 3 innovation IDs in pool");

        // Save
        SaveManager.Save(state);
        Debug.Assert(SaveManager.HasSave(), "FAIL: save file should exist");

        // Modify state in memory
        state.currentYear = 2;
        state.chronicleLog = new[] { "Year 1: ...", "Year 2: Begins." };

        // Load — should restore Year 1, not the modified Year 2
        var loaded = SaveManager.Load();
        Debug.Assert(loaded != null, "FAIL: loaded state should not be null");
        Debug.Assert(loaded.currentYear == 1,
            $"FAIL: loaded year should be 1, got {loaded.currentYear}");
        Debug.Assert(loaded.characters.Length == 2,
            $"FAIL: loaded characters count wrong, got {loaded.characters.Length}");
        Debug.Assert(loaded.characters[0].characterName == "Aldric",
            "FAIL: first character name wrong after load");
        Debug.Assert(loaded.characters[1].characterName == "Brunhild",
            "FAIL: second character name wrong after load");
        Debug.Assert(loaded.availableInnovationIds.Length == 3,
            "FAIL: innovation pool count wrong after load");
        Debug.Assert(loaded.chronicleLog.Length == 1,
            $"FAIL: chronicle log should have 1 entry after load, got {loaded.chronicleLog.Length}");

        // Verify HuntResult struct survives round-trip (default empty)
        Debug.Assert(!loaded.pendingHuntResult.isVictory,
            "FAIL: empty HuntResult should not be a victory");

        // Clean up
        SaveManager.DeleteSave();
        Debug.Assert(!SaveManager.HasSave(), "FAIL: save should be gone after delete");

        Debug.Log("[SaveManagerTest] ✓ All save/load assertions passed");
        Debug.Log("=== SAVE MANAGER TEST COMPLETE ===");
    }
}
```

Attach to a GameObject, Play, verify all assertions pass, **delete the test script**.

---

## Next Session

**File:** `_Docs/Stage_04/STAGE_04_B.md`  
**Covers:** CampaignInitializer — creating a new campaign from a CampaignSO, generating starting characters with correct name pool and gender split
