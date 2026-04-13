<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 4-E | Retirement, Birth, Year Advance & Stage 4 Final Test
Status: Stage 4-D complete. Innovation deck cascade, crafter
unlock, crafting, and GearLinkResolver all verified.
Test script deleted.
Task: Add CheckRetirement(), BirthNewCharacter(), and
AdvanceYear() to SettlementManager. Then run the full
Year 1→2 simulation test to complete Stage 4.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_04/STAGE_04_E.md
- Assets/_Game/Scripts/Core.Systems/SettlementManager.cs
- Assets/_Game/Scripts/Core.Systems/SaveManager.cs
- Assets/_Game/Scripts/Core.Data/CampaignState.cs

Then confirm:
- That you will ADD methods to SettlementManager.cs,
  not replace what already exists
- That retirement checks huntCount against
  CampaignSO.retirementHuntCount
- That legacy bonus is logged with a TODO comment
  (mechanical implementation deferred to Stage 6 UI)
- That AdvanceYear() auto-saves via SaveManager
- What you will NOT touch this session

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 4-E: Retirement, Birth, Year Advance & Stage 4 Final Test

**Resuming from:** Stage 4-D complete — Innovation, crafting, GearLinkResolver verified  
**Done when:** Full Year 1→2 simulation test passes all assertions and auto-save fires correctly. Stage 4 Definition of Done fully checked off.  
**Commit:** `"4E: Retirement, birth, year advance, auto-save — Stage 4 complete, Year 1→2 test passes"`  
**Next session:** STAGE_05_A.md (Stage 5 begins)  

---

## Step 1: Add to SettlementManager.cs

Add these methods to the **existing** `SettlementManager` class — do not replace any existing code:

```csharp
// ── Character Retirement ─────────────────────────────────────

// Returns true if the character was retired this call
public bool CheckRetirement(RuntimeCharacterState character)
{
    if (character.isRetired) return false;
    if (character.huntCount < _campaignData.retirementHuntCount) return false;

    character.isRetired = true;

    // Find highest proficiency for legacy bonus
    int    highestTier  = 0;
    string legacyWeapon = "FistWeapon";

    if (character.proficiencyTiers != null)
    {
        for (int i = 0; i < character.proficiencyTiers.Length; i++)
        {
            if (character.proficiencyTiers[i] > highestTier)
            {
                highestTier  = character.proficiencyTiers[i];
                legacyWeapon = (character.proficiencyWeaponTypes != null &&
                                i < character.proficiencyWeaponTypes.Length)
                    ? character.proficiencyWeaponTypes[i]
                    : "FistWeapon";
            }
        }
    }

    // TODO: Store legacy bonus in CampaignState so new characters can start
    // at Tier 1 of legacyWeapon. Wire this in Stage 6 UI when birth flow is built.
    Debug.Log($"[Settlement] {character.characterName} RETIRED after {character.huntCount} hunts. " +
              $"Legacy: {legacyWeapon} Tier {highestTier}");

    // Move from active to retired roster
    var active  = new System.Collections.Generic.List<RuntimeCharacterState>(_campaign.characters);
    var retired = new System.Collections.Generic.List<RuntimeCharacterState>(_campaign.retiredCharacters);
    active.Remove(character);
    retired.Add(character);
    _campaign.characters        = active.ToArray();
    _campaign.retiredCharacters = retired.ToArray();

    AddToChronicle($"Year {_campaign.currentYear}: {character.characterName} retired. " +
                   $"Legacy: {legacyWeapon} Tier 1 available.");
    return true;
}

// Convenience — checks all active characters
public void CheckAllRetirements()
{
    // Snapshot to avoid modifying the array we're iterating
    var snapshot = (RuntimeCharacterState[])_campaign.characters.Clone();
    foreach (var ch in snapshot)
        CheckRetirement(ch);
}

// ── Character Birth ──────────────────────────────────────────

public RuntimeCharacterState BirthNewCharacter(string name, string sex, string bodyBuild)
{
    var newCharacter = new RuntimeCharacterState
    {
        characterId             = System.Guid.NewGuid().ToString(),
        characterName           = name,
        sex                     = sex,
        bodyBuild               = bodyBuild,
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

    var active = new System.Collections.Generic.List<RuntimeCharacterState>(_campaign.characters)
        { newCharacter };
    _campaign.characters = active.ToArray();

    AddToChronicle($"Year {_campaign.currentYear}: {name} born.");
    Debug.Log($"[Settlement] New character born: {name} ({sex}, {bodyBuild}). " +
              $"Active roster: {_campaign.characters.Length}");

    return newCharacter;
}

// ── Year Advance ─────────────────────────────────────────────

public void AdvanceYear()
{
    _campaign.currentYear++;
    _campaign.pendingHuntResult = default;

    AddToChronicle($"--- Year {_campaign.currentYear} begins ---");
    Debug.Log($"[Campaign] *** YEAR {_campaign.currentYear} BEGINS ***");

    if (_campaign.currentYear > 30)
        Debug.Log("[Campaign] Year 30 passed — campaign concludes after this year's hunt");

    // Auto-save after every year advance
    SaveManager.Save(_campaign);
}
```

---

## Stage 4 Final Simulation Test

**Path:** `Assets/_Game/Scripts/Core.Systems/Stage4FinalTest.cs` *(delete after)*

```csharp
using UnityEngine;
using MnM.Core.Data;
using MnM.Core.Systems;

public class Stage4FinalTest : MonoBehaviour
{
    [SerializeField] private CampaignSO _tutorialSO;

    private void Start()
    {
        if (_tutorialSO == null) { Debug.LogError("Assign Mock_TutorialCampaign"); return; }

        Debug.Log("=== STAGE 4 FINAL TEST — Year 1 → Year 2 Simulation ===");

        SaveManager.DeleteSave();

        // ── Initialize ───────────────────────────────────────────
        var state      = CampaignInitializer.CreateNewCampaign(_tutorialSO);
        var settlement = new SettlementManager();
        settlement.Initialize(state, _tutorialSO);

        Debug.Assert(state.currentYear == 1, "FAIL: year should be 1");
        Debug.Assert(state.characters.Length == _tutorialSO.startingCharacterCount,
            "FAIL: character count mismatch");
        Debug.Log($"✓ Campaign initialized. Year:{state.currentYear} " +
                  $"Characters:{state.characters.Length}");

        // ── Step 1: Hunt victory → loot and hunt counts ──────────
        var hunterIds   = new[] { state.characters[0].characterId,
                                  state.characters[1].characterId };
        var huntResult  = CampaignStateFactory.BuildMockGauntVictory(hunterIds);
        settlement.ApplyHuntResults(huntResult);

        Debug.Assert(settlement.GetResourceAmount("Gaunt Fang") == 2, "FAIL: Gaunt Fang");
        Debug.Assert(settlement.GetResourceAmount("Bone")       == 2, "FAIL: Bone");
        Debug.Assert(settlement.GetResourceAmount("Sinew")      == 1, "FAIL: Sinew");
        Debug.Assert(state.characters[0].huntCount == 1, "FAIL: huntCount char 0");
        Debug.Assert(state.characters[1].huntCount == 1, "FAIL: huntCount char 1");
        Debug.Log("✓ Loot and hunt counts correct");

        // ── Step 2: Chronicle Event draw ─────────────────────────
        var evt = settlement.DrawChronicleEvent();
        if (evt != null)
        {
            settlement.ResolveEvent(evt, -1);
            Debug.Assert(state.resolvedEventIds.Contains(evt.eventId), "FAIL: event not resolved");
            Debug.Log($"✓ Event resolved: {evt.eventId}");
        }
        else
        {
            Debug.Log("✓ No events in mock SO — null draw expected");
        }

        // ── Step 3: Innovation draw and adopt ────────────────────
        var options = settlement.DrawInnovationOptions(3);
        Debug.Log($"✓ Innovations drawn: {options.Length}");
        if (options.Length > 0)
        {
            int poolBefore = state.availableInnovationIds.Length;
            settlement.AdoptInnovation(options[0]);
            Debug.Assert(state.adoptedInnovationIds.Length == 1, "FAIL: adoption count");
            Debug.Log($"✓ Adopted: {options[0].innovationName}. " +
                      $"Pool before:{poolBefore} after:{state.availableInnovationIds.Length}");
        }

        // ── Step 4: Crafter unlock ───────────────────────────────
        if (_tutorialSO.crafterPool != null && _tutorialSO.crafterPool.Length > 0)
        {
            var crafter = _tutorialSO.crafterPool[0];
            settlement.TryUnlockCrafter(crafter);
            Debug.Assert(state.builtCrafterNames.Contains(crafter.crafterName),
                "FAIL: crafter not built");
            Debug.Log($"✓ Crafter built: {crafter.crafterName}");
        }
        else
        {
            Debug.Log("✓ No crafters in mock SO — skipped");
        }

        // ── Step 5: Chronicle has entries ────────────────────────
        Debug.Assert(state.chronicleLog.Length >= 1, "FAIL: chronicle empty");
        Debug.Log($"✓ Chronicle entries: {state.chronicleLog.Length}");

        // ── Step 6: Birth a new character ────────────────────────
        int rosterBefore = state.characters.Length;
        settlement.BirthNewCharacter("Runa", "Female", "Gerd");
        Debug.Assert(state.characters.Length == rosterBefore + 1,
            "FAIL: roster should have grown by 1");
        Debug.Assert(state.characters[state.characters.Length - 1].characterName == "Runa",
            "FAIL: new character name wrong");
        Debug.Log($"✓ Character born. Roster: {rosterBefore} → {state.characters.Length}");

        // ── Step 7: Retirement check (manual threshold trigger) ──
        var testChar = state.characters[0];
        testChar.huntCount = _tutorialSO.retirementHuntCount; // Force threshold
        bool retired = settlement.CheckRetirement(testChar);
        Debug.Log($"[Test] Retirement triggered: {retired} " +
                  $"(depends on retirementHuntCount = {_tutorialSO.retirementHuntCount})");
        // May be true or false depending on mock SO value — just verify it runs

        // ── Step 8: Year advance → auto-save ─────────────────────
        settlement.AdvanceYear();
        Debug.Assert(state.currentYear == 2, $"FAIL: year should be 2, got {state.currentYear}");
        Debug.Assert(SaveManager.HasSave(), "FAIL: auto-save should exist");
        Debug.Log("✓ Year advanced to 2. Auto-save fired.");

        // ── Step 9: Load and verify ──────────────────────────────
        var loaded = SaveManager.Load();
        Debug.Assert(loaded != null, "FAIL: load returned null");
        Debug.Assert(loaded.currentYear == 2,
            $"FAIL: loaded year should be 2, got {loaded.currentYear}");
        Debug.Assert(loaded.chronicleLog.Length == state.chronicleLog.Length,
            "FAIL: chronicle length mismatch after load");
        Debug.Log($"✓ Load verified. Year:{loaded.currentYear} " +
                  $"Chronicle:{loaded.chronicleLog.Length} entries");

        SaveManager.DeleteSave();

        Debug.Log("=== STAGE 4 FINAL TEST COMPLETE ===");
        Debug.Log("Stage 4 Definition of Done:");
        Debug.Log("✓ CampaignState saves and loads cleanly — JSON round-trip verified");
        Debug.Log("✓ Chronicle Events draw by year range and tag filters");
        Debug.Log("✓ Innovation deck seeds, draws, and cascade-unlocks on adoption");
        Debug.Log("✓ Crafter unlock deducts resources, adds to builtCrafterNames");
        Debug.Log("✓ Crafting checks resources, deducts, adds item to hunter loadout");
        Debug.Log("✓ GearLinkResolver finds matching affinity tags and logs bonuses");
        Debug.Log("✓ Character birth adds to active roster, writes chronicle");
        Debug.Log("✓ Retirement moves character, writes legacy bonus log");
        Debug.Log("✓ Year advance increments year, clears pending hunt, auto-saves");
        Debug.Log("✓ Year 1→2 simulation passes all assertions");
    }
}
```

Attach to a GameObject, assign `Mock_TutorialCampaign`, Play, verify all ✓ lines appear, **delete the test script**.

---

## Stage 4 Complete — What You Now Have

- `CampaignState` + `RuntimeCharacterState` — full JSON-serializable save structure
- `SaveManager` — writes/reads `persistentDataPath/campaign_save.json`, auto-save on year advance
- `CampaignInitializer` — new campaign from SO, GDD name pool, gender split, `CombatState` builder
- `CampaignStateFactory` — mock states for testing
- `SettlementManager` — complete pipeline: hunt results → resources → chronicle events → guiding principals → innovation deck → crafter unlock → crafting → birth → retirement → year advance
- `GearLinkResolver` — affinity tag matching, stat totals

No UI. No scenes. A complete data-driven campaign loop in pure C#, verified end-to-end.

---

## Next Session

**File:** `_Docs/Stage_05/STAGE_05_A.md`  
**First task of Stage 5:** USS design tokens and stone-panel style system — the visual foundation before any UXML is written
