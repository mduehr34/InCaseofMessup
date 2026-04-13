<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 4-C | SettlementManager — Hunt Results, Resources & Events
Status: Stage 4-B complete. CampaignInitializer verified.
Test script deleted.
Task: Create SettlementManager with ApplyHuntResults(),
resource add/remove, Chronicle Event draw/resolve, and
Guiding Principal trigger/resolve. No crafting or Innovation
logic yet — those are Session 4-D.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_04/STAGE_04_C.md
- Assets/_Game/Scripts/Core.Data/CampaignState.cs
- Assets/_Game/Scripts/Core.Data/CampaignStateFactory.cs
- Assets/_Game/Scripts/Core.Data/EventSO.cs
- Assets/_Game/Scripts/Core.Data/GuidingPrincipalSO.cs
- Assets/_Game/Scripts/Core.Data/Enums.cs

Then confirm:
- The single file you will create
- That loot is only added on victory, not on loss
- That injuries apply on both victory AND loss
- That hunt count increments for ALL participants
  (both surviving and collapsed hunters)
- What you will NOT touch this session

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 4-C: SettlementManager — Hunt Results, Resources & Events

**Resuming from:** Stage 4-B complete — CampaignInitializer verified  
**Done when:** ApplyHuntResults() correctly gates loot on victory, applies injuries regardless, increments hunt counts; Chronicle Event draw filters by year range and resolved IDs; Guiding Principal triggers and resolves correctly  
**Commit:** `"4C: SettlementManager — hunt results, resources, chronicle events, guiding principals"`  
**Next session:** STAGE_04_D.md  

---

## SettlementManager.cs

**Path:** `Assets/_Game/Scripts/Core.Systems/SettlementManager.cs`

```csharp
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MnM.Core.Data;

namespace MnM.Core.Systems
{
    public class SettlementManager
    {
        private CampaignState _campaign;
        private CampaignSO    _campaignData;

        public void Initialize(CampaignState campaign, CampaignSO campaignData)
        {
            _campaign     = campaign;
            _campaignData = campaignData;
            Debug.Log($"[Settlement] Initialized. Year:{campaign.currentYear} " +
                      $"Characters:{campaign.characters.Length}");
        }

        // ── STEP 1: Apply Hunt Results ───────────────────────────
        public void ApplyHuntResults(HuntResult result)
        {
            Debug.Log($"[Settlement] Applying hunt result. Victory:{result.isVictory} " +
                      $"Monster:{result.monsterName} Rounds:{result.roundsFought}");

            // Loot — ONLY on victory
            if (result.isVictory && result.lootGained != null)
            {
                foreach (var loot in result.lootGained)
                {
                    AddResource(loot.resourceName, loot.amount);
                }
            }
            else if (!result.isVictory)
            {
                Debug.Log("[Settlement] Hunt lost — no loot gained");
            }

            // Injuries — apply to surviving hunters regardless of win/loss
            if (result.survivingHunterIds != null && result.injuryCardNamesApplied != null)
            {
                for (int i = 0; i < result.survivingHunterIds.Length; i++)
                {
                    var hunter = GetCharacter(result.survivingHunterIds[i]);
                    if (hunter == null) continue;

                    if (i < result.injuryCardNamesApplied.Length &&
                        !string.IsNullOrEmpty(result.injuryCardNamesApplied[i]))
                    {
                        string injuryName = result.injuryCardNamesApplied[i];
                        var list = new List<string>(hunter.injuryCardNames) { injuryName };
                        hunter.injuryCardNames = list.ToArray();
                        Debug.Log($"[Settlement] {hunter.characterName} gains injury: {injuryName}");
                    }
                }
            }

            // Hunt count — ALL participants (surviving + collapsed)
            var allParticipants = new List<string>();
            if (result.survivingHunterIds != null)
                allParticipants.AddRange(result.survivingHunterIds);
            if (result.collapsedHunterIds != null)
                allParticipants.AddRange(result.collapsedHunterIds);

            foreach (var id in allParticipants)
            {
                var hunter = GetCharacter(id);
                if (hunter != null)
                {
                    hunter.huntCount++;
                    Debug.Log($"[Settlement] {hunter.characterName} hunt count: {hunter.huntCount}");
                }
            }

            // Clear pending
            _campaign.pendingHuntResult = default;
        }

        // ── STEP 2: Chronicle Event Draw ─────────────────────────
        public EventSO DrawChronicleEvent()
        {
            var eligible = GetEligibleEvents();

            if (eligible.Count == 0)
            {
                Debug.Log("[Settlement] No eligible Chronicle Events this year");
                return null;
            }

            // Check for mandatory events first — they take priority
            var mandatory = eligible.FirstOrDefault(e => e.isMandatory);
            if (mandatory != null)
            {
                Debug.Log($"[Settlement] Mandatory event: {mandatory.eventId} — {mandatory.eventName}");
                return mandatory;
            }

            // Otherwise draw random from eligible pool
            var drawn = eligible[Random.Range(0, eligible.Count)];
            Debug.Log($"[Settlement] Event drawn: {drawn.eventId} — {drawn.eventName}");
            return drawn;
        }

        private List<EventSO> GetEligibleEvents()
        {
            if (_campaignData.eventPool == null) return new List<EventSO>();

            return _campaignData.eventPool
                .Where(e => e != null
                    && !_campaign.resolvedEventIds.Contains(e.eventId)
                    && _campaign.currentYear >= e.yearRangeMin
                    && _campaign.currentYear <= e.yearRangeMax
                    && MatchesCampaignTag(e)
                    && MatchesMonsterTag(e))
                .ToList();
        }

        private bool MatchesCampaignTag(EventSO evt) =>
            string.IsNullOrEmpty(evt.campaignTag) ||
            evt.campaignTag == _campaignData.campaignName;

        private bool MatchesMonsterTag(EventSO evt)
        {
            if (string.IsNullOrEmpty(evt.monsterTag)) return true;
            // Event requires a specific monster to have been hunted
            // Check chronicle log for that monster name (simple string match)
            return _campaign.chronicleLog.Any(
                entry => entry.Contains(evt.monsterTag));
        }

        public void ResolveEvent(EventSO evt, int choiceIndex)
        {
            // Mark resolved — prevents drawing again
            var resolved = new List<string>(_campaign.resolvedEventIds) { evt.eventId };
            _campaign.resolvedEventIds = resolved.ToArray();

            // Get the chosen branch (choiceIndex -1 = mandatory, no choice)
            EventChoice choice = default;
            bool hasChoice = choiceIndex >= 0 &&
                             evt.choices != null &&
                             choiceIndex < evt.choices.Length;
            if (hasChoice) choice = evt.choices[choiceIndex];

            // Chronicle log entry
            string logEntry = $"Year {_campaign.currentYear}: {evt.eventName}";
            if (hasChoice && !string.IsNullOrEmpty(choice.choiceLabel))
                logEntry += $" — {choice.choiceLabel} chosen";
            AddToChronicle(logEntry);

            // Unlock codex / artifact
            if (!string.IsNullOrEmpty(choice.codexEntryId))
                UnlockCodexEntry(choice.codexEntryId);
            if (!string.IsNullOrEmpty(choice.artifactUnlockId))
                UnlockArtifact(choice.artifactUnlockId);

            // Trigger Guiding Principal if flagged
            if (!string.IsNullOrEmpty(choice.guidingPrincipalTrigger))
                TriggerGuidingPrincipal(choice.guidingPrincipalTrigger);

            // Mechanical effects — logged for manual implementation
            // ⚑ Per .cursorrules: stop and ask before implementing
            //   mechanicalEffect strings that aren't simple stat/resource changes
            if (!string.IsNullOrEmpty(choice.mechanicalEffect))
                Debug.Log($"[Settlement] Event mechanical effect (apply manually): " +
                          $"{choice.mechanicalEffect}");

            Debug.Log($"[Settlement] Event resolved: {evt.eventId}");
        }

        // ── STEP 3: Guiding Principals ───────────────────────────
        public void TriggerGuidingPrincipal(string principalId)
        {
            if (_campaign.resolvedGuidingPrincipalIds.Contains(principalId))
            {
                Debug.Log($"[GP] {principalId} already resolved — skip");
                return;
            }
            if (_campaign.activeGuidingPrincipalIds.Contains(principalId))
            {
                Debug.Log($"[GP] {principalId} already active — skip");
                return;
            }

            var active = new List<string>(_campaign.activeGuidingPrincipalIds) { principalId };
            _campaign.activeGuidingPrincipalIds = active.ToArray();
            Debug.Log($"[GP] *** GUIDING PRINCIPAL TRIGGERED: {principalId} — Awaiting player choice ***");
        }

        public void ResolveGuidingPrincipal(string principalId, int choiceIndex)
        {
            var active   = new List<string>(_campaign.activeGuidingPrincipalIds);
            var resolved = new List<string>(_campaign.resolvedGuidingPrincipalIds);

            active.Remove(principalId);
            resolved.Add(principalId);

            _campaign.activeGuidingPrincipalIds   = active.ToArray();
            _campaign.resolvedGuidingPrincipalIds = resolved.ToArray();

            string choiceLabel = (char)('A' + choiceIndex) + "";
            AddToChronicle($"Year {_campaign.currentYear}: " +
                           $"Guiding Principal {principalId} — Choice {choiceLabel} chosen.");
            Debug.Log($"[GP] Resolved: {principalId}, Choice:{choiceLabel}");
        }

        // ── Resource Management ──────────────────────────────────
        public void AddResource(string resourceName, int amount)
        {
            var list = new List<ResourceEntry>(_campaign.resources);
            int idx  = list.FindIndex(r => r.resourceName == resourceName);

            if (idx >= 0)
            {
                var entry = list[idx];
                entry.amount += amount;
                list[idx] = entry;
            }
            else
            {
                list.Add(new ResourceEntry { resourceName = resourceName, amount = amount });
            }

            _campaign.resources = list.ToArray();
            Debug.Log($"[Resources] +{amount} {resourceName}. " +
                      $"Total: {GetResourceAmount(resourceName)}");
        }

        public bool RemoveResource(string resourceName, int amount)
        {
            int have = GetResourceAmount(resourceName);
            if (have < amount)
            {
                Debug.LogWarning($"[Resources] Cannot remove {amount} {resourceName} — only have {have}");
                return false;
            }

            var list = new List<ResourceEntry>(_campaign.resources);
            int idx  = list.FindIndex(r => r.resourceName == resourceName);
            if (idx < 0) return false;

            var entry = list[idx];
            entry.amount -= amount;
            if (entry.amount <= 0) list.RemoveAt(idx);
            else                   list[idx] = entry;

            _campaign.resources = list.ToArray();
            Debug.Log($"[Resources] -{amount} {resourceName}. " +
                      $"Remaining: {GetResourceAmount(resourceName)}");
            return true;
        }

        public int GetResourceAmount(string resourceName)
        {
            var entry = System.Array.Find(
                _campaign.resources, r => r.resourceName == resourceName);
            return entry.resourceName == resourceName ? entry.amount : 0;
        }

        // ── Codex / Artifact Unlocks ─────────────────────────────
        public void UnlockCodexEntry(string entryId)
        {
            if (_campaign.unlockedCodexEntryIds.Contains(entryId)) return;
            var list = new List<string>(_campaign.unlockedCodexEntryIds) { entryId };
            _campaign.unlockedCodexEntryIds = list.ToArray();
            Debug.Log($"[Codex] Unlocked: {entryId}");
        }

        public void UnlockArtifact(string artifactId)
        {
            if (_campaign.unlockedArtifactIds.Contains(artifactId)) return;
            var list = new List<string>(_campaign.unlockedArtifactIds) { artifactId };
            _campaign.unlockedArtifactIds = list.ToArray();
            Debug.Log($"[Codex] Artifact unlocked: {artifactId}");
        }

        // ── Chronicle Log ────────────────────────────────────────
        public void AddToChronicle(string entry)
        {
            var log = new List<string>(_campaign.chronicleLog) { entry };
            _campaign.chronicleLog = log.ToArray();
            Debug.Log($"[Chronicle] {entry}");
        }

        // ── Character Lookup ─────────────────────────────────────
        public RuntimeCharacterState GetCharacter(string characterId)
        {
            var ch = System.Array.Find(
                _campaign.characters, c => c.characterId == characterId);
            if (ch == null)
                Debug.LogWarning($"[Settlement] Character not found: {characterId}");
            return ch;
        }
    }
}
```

---

## Verification Test

Temporary test script — delete after passing:

**Path:** `Assets/_Game/Scripts/Core.Systems/SettlementManagerTest.cs` *(delete after)*

```csharp
using UnityEngine;
using MnM.Core.Data;
using MnM.Core.Systems;

public class SettlementManagerTest : MonoBehaviour
{
    [SerializeField] private CampaignSO _tutorialSO;

    private void Start()
    {
        if (_tutorialSO == null) { Debug.LogError("Assign Mock_TutorialCampaign"); return; }

        Debug.Log("=== SETTLEMENT MANAGER TEST ===");

        var state      = CampaignStateFactory.BuildMockYear1State();
        var settlement = new SettlementManager();
        settlement.Initialize(state, _tutorialSO);

        // ── Test 1: Victory loot ─────────────────────────────────
        var victoryResult = CampaignStateFactory.BuildMockGauntVictory(
            new[] { "char_aldric", "char_brunhild" });
        settlement.ApplyHuntResults(victoryResult);

        Debug.Assert(settlement.GetResourceAmount("Gaunt Fang") == 2,
            "FAIL: should have 2 Gaunt Fang");
        Debug.Assert(settlement.GetResourceAmount("Bone") == 2,
            "FAIL: should have 2 Bone");
        Debug.Assert(settlement.GetResourceAmount("Sinew") == 1,
            "FAIL: should have 1 Sinew");
        Debug.Assert(state.characters[0].huntCount == 1,
            "FAIL: Aldric hunt count should be 1");
        Debug.Assert(state.characters[1].huntCount == 1,
            "FAIL: Brunhild hunt count should be 1");
        Debug.Log("✓ Victory loot and hunt count correct");

        // ── Test 2: Loss — no loot ───────────────────────────────
        var lossResult = new HuntResult
        {
            isVictory          = false,
            monsterName        = "The Gaunt",
            monsterDifficulty  = "Standard",
            roundsFought       = 5,
            survivingHunterIds = new[] { "char_aldric" },
            collapsedHunterIds = new[] { "char_brunhild" },
            lootGained = new[]
            {
                new ResourceEntry { resourceName = "Gaunt Fang", amount = 5 }
            },
            injuryCardNamesApplied = new[] { "Spear Wound" },
        };
        settlement.ApplyHuntResults(lossResult);

        // No loot added on loss
        Debug.Assert(settlement.GetResourceAmount("Gaunt Fang") == 2,
            "FAIL: Gaunt Fang should still be 2 — no loot on loss");
        // Injury applied
        Debug.Assert(state.characters[0].injuryCardNames.Length == 1,
            "FAIL: Aldric should have 1 injury");
        Debug.Assert(state.characters[0].injuryCardNames[0] == "Spear Wound",
            "FAIL: injury name wrong");
        // Hunt count increments for all (surviving + collapsed)
        Debug.Assert(state.characters[0].huntCount == 2,
            "FAIL: Aldric count should be 2");
        Debug.Assert(state.characters[1].huntCount == 2,
            "FAIL: Brunhild count should be 2 (was collapsed)");
        Debug.Log("✓ Loss: no loot, injury applied, hunt count incremented");

        // ── Test 3: Resource add/remove ──────────────────────────
        settlement.AddResource("Bone", 3);
        Debug.Assert(settlement.GetResourceAmount("Bone") == 5,
            $"FAIL: should have 5 Bone, got {settlement.GetResourceAmount("Bone")}");
        bool removed = settlement.RemoveResource("Bone", 2);
        Debug.Assert(removed, "FAIL: remove should succeed");
        Debug.Assert(settlement.GetResourceAmount("Bone") == 3,
            "FAIL: should have 3 Bone after remove");
        bool failedRemove = settlement.RemoveResource("Bone", 99);
        Debug.Assert(!failedRemove, "FAIL: over-remove should return false");
        Debug.Log("✓ Resource add/remove correct");

        // ── Test 4: Chronicle log ────────────────────────────────
        Debug.Assert(state.chronicleLog.Length >= 1, "FAIL: chronicle should have entries");
        Debug.Log($"✓ Chronicle entries: {state.chronicleLog.Length}");

        // ── Test 5: Guiding Principal trigger/resolve ────────────
        settlement.TriggerGuidingPrincipal("GP-01");
        Debug.Assert(state.activeGuidingPrincipalIds.Length == 1,
            "FAIL: GP-01 should be active");
        settlement.TriggerGuidingPrincipal("GP-01"); // Should be ignored
        Debug.Assert(state.activeGuidingPrincipalIds.Length == 1,
            "FAIL: duplicate trigger should be ignored");
        settlement.ResolveGuidingPrincipal("GP-01", 0);
        Debug.Assert(state.activeGuidingPrincipalIds.Length == 0,
            "FAIL: GP-01 should no longer be active");
        Debug.Assert(state.resolvedGuidingPrincipalIds.Length == 1,
            "FAIL: GP-01 should be in resolved list");
        settlement.TriggerGuidingPrincipal("GP-01"); // Already resolved — should skip
        Debug.Assert(state.activeGuidingPrincipalIds.Length == 0,
            "FAIL: resolved GP should not re-trigger");
        Debug.Log("✓ Guiding Principal trigger/resolve correct");

        Debug.Log("[SettlementManagerTest] ✓ All assertions passed");
        Debug.Log("=== SETTLEMENT MANAGER TEST COMPLETE ===");
    }
}
```

Attach to a GameObject, assign `Mock_TutorialCampaign`, Play, verify all assertions, **delete the test script**.

---

## Next Session

**File:** `_Docs/Stage_04/STAGE_04_D.md`  
**Covers:** Innovation deck draw/adopt with cascade unlocks, Crafter unlock, crafting pipeline, and GearLinkResolver
