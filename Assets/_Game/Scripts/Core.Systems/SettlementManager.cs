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

        // ── STEP 1: Apply Hunt Results ───────────────────────────────────────
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

        // ── STEP 2: Chronicle Event Draw ─────────────────────────────────────
        public EventSO DrawChronicleEvent()
        {
            var eligible = GetEligibleEvents();

            if (eligible.Count == 0)
            {
                Debug.Log("[Settlement] No eligible Chronicle Events this year");
                return null;
            }

            // Mandatory events take priority over random draw
            var mandatory = eligible.FirstOrDefault(e => e.isMandatory);
            if (mandatory != null)
            {
                Debug.Log($"[Settlement] Mandatory event: {mandatory.eventId} — {mandatory.eventName}");
                return mandatory;
            }

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
            // Check chronicle log for the required monster name (simple string match)
            return _campaign.chronicleLog.Any(entry => entry.Contains(evt.monsterTag));
        }

        public void ResolveEvent(EventSO evt, int choiceIndex)
        {
            // Mark resolved — prevents drawing again
            var resolved = new List<string>(_campaign.resolvedEventIds) { evt.eventId };
            _campaign.resolvedEventIds = resolved.ToArray();

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

            if (!string.IsNullOrEmpty(choice.codexEntryId))
                UnlockCodexEntry(choice.codexEntryId);
            if (!string.IsNullOrEmpty(choice.artifactUnlockId))
                UnlockArtifact(choice.artifactUnlockId);

            if (!string.IsNullOrEmpty(choice.guidingPrincipalTrigger))
                TriggerGuidingPrincipal(choice.guidingPrincipalTrigger);

            // Per .cursorrules: do NOT interpret mechanicalEffect strings — log for manual application
            if (!string.IsNullOrEmpty(choice.mechanicalEffect))
                Debug.Log($"[Settlement] Event mechanical effect (apply manually): {choice.mechanicalEffect}");

            Debug.Log($"[Settlement] Event resolved: {evt.eventId}");
        }

        // ── STEP 3: Guiding Principals ───────────────────────────────────────
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

            string choiceLabel = ((char)('A' + choiceIndex)).ToString();
            AddToChronicle($"Year {_campaign.currentYear}: " +
                           $"Guiding Principal {principalId} — Choice {choiceLabel} chosen.");
            Debug.Log($"[GP] Resolved: {principalId}, Choice:{choiceLabel}");
        }

        // ── Resource Management ──────────────────────────────────────────────
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
            Debug.Log($"[Resources] +{amount} {resourceName}. Total: {GetResourceAmount(resourceName)}");
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
            Debug.Log($"[Resources] -{amount} {resourceName}. Remaining: {GetResourceAmount(resourceName)}");
            return true;
        }

        public int GetResourceAmount(string resourceName)
        {
            var entry = System.Array.Find(
                _campaign.resources, r => r.resourceName == resourceName);
            return entry.resourceName == resourceName ? entry.amount : 0;
        }

        // ── Codex / Artifact Unlocks ─────────────────────────────────────────
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

        // ── Chronicle Log ─────────────────────────────────────────────────────
        public void AddToChronicle(string entry)
        {
            var log = new List<string>(_campaign.chronicleLog) { entry };
            _campaign.chronicleLog = log.ToArray();
            Debug.Log($"[Chronicle] {entry}");
        }

        // ── Character Lookup ──────────────────────────────────────────────────
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
