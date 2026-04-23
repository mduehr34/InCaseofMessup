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
            // Pending events (e.g. EVT-14 queued by death) fire first, bypassing year/tag filters
            if (_campaign.pendingEventIds != null && _campaign.pendingEventIds.Length > 0)
            {
                string pendingId = _campaign.pendingEventIds[0];
                var pendingEvt   = System.Array.Find(_campaignData.eventPool, e => e.eventId == pendingId);
                var remaining    = new List<string>(_campaign.pendingEventIds);
                remaining.RemoveAt(0);
                _campaign.pendingEventIds = remaining.ToArray();

                if (pendingEvt != null)
                {
                    Debug.Log($"[Settlement] Pending event fires: {pendingEvt.eventId} — {pendingEvt.eventName}");
                    return pendingEvt;
                }

                Debug.LogWarning($"[Settlement] Pending event '{pendingId}' not found in event pool — skipped");
            }

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

            // EVT-21: gate-unlock The Spite for hunt selection
            if (evt.eventId == "EVT-21")
            {
                var unlocked = new List<string>(_campaign.unlockedCodexEntryIds) { "TheSpite_Unlocked" };
                _campaign.unlockedCodexEntryIds = unlocked.ToArray();
                Debug.Log("[Settlement] EVT-21 resolved — The Spite added to hunt roster");
            }

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

        // ── Innovation Deck ───────────────────────────────────────────────────
        public InnovationSO[] DrawInnovationOptions(int drawCount = 3)
        {
            if (_campaignData.startingInnovations == null)
                return new InnovationSO[0];

            // Pool = all available IDs not yet adopted
            var pool = _campaign.availableInnovationIds
                .Where(id => !_campaign.adoptedInnovationIds.Contains(id))
                .Select(id => GetInnovationById(id))
                .Where(inn => inn != null)
                .ToList();

            if (pool.Count == 0)
            {
                Debug.Log("[Innovation] No innovations available to draw");
                return new InnovationSO[0];
            }

            // Shuffle and take up to drawCount
            ShuffleList(pool);
            var drawn = pool.Take(drawCount).ToArray();
            Debug.Log($"[Innovation] Drew {drawn.Length} options: " +
                      $"[{string.Join(", ", drawn.Select(i => i.innovationName))}]");
            return drawn;
        }

        public void AdoptInnovation(InnovationSO innovation)
        {
            if (_campaign.adoptedInnovationIds.Contains(innovation.innovationId))
            {
                Debug.LogWarning($"[Innovation] Already adopted: {innovation.innovationId}");
                return;
            }

            // Mark as adopted
            var adopted = new List<string>(_campaign.adoptedInnovationIds)
                { innovation.innovationId };
            _campaign.adoptedInnovationIds = adopted.ToArray();

            // Cascade — add newly unlocked cards to available pool (no duplicates)
            if (innovation.addsToDeck != null)
            {
                var available = new List<string>(_campaign.availableInnovationIds);
                foreach (var unlocked in innovation.addsToDeck)
                {
                    if (unlocked != null && !available.Contains(unlocked.innovationId))
                    {
                        available.Add(unlocked.innovationId);
                        Debug.Log($"[Innovation] Cascade unlock: {unlocked.innovationId} " +
                                  $"({unlocked.innovationName}) added to pool");
                    }
                }
                _campaign.availableInnovationIds = available.ToArray();
            }

            AddToChronicle($"Year {_campaign.currentYear}: Innovation adopted — {innovation.innovationName}.");
            Debug.Log($"[Innovation] Adopted: {innovation.innovationName}. " +
                      $"Pool now: {_campaign.availableInnovationIds.Length} " +
                      $"Adopted: {_campaign.adoptedInnovationIds.Length}");
        }

        private InnovationSO GetInnovationById(string id)
        {
            if (_campaignData.startingInnovations == null) return null;
            // Search starting set; cascaded SOs must be referenced from starting SOs
            foreach (var inn in _campaignData.startingInnovations)
            {
                if (inn == null) continue;
                if (inn.innovationId == id) return inn;
                // Check cascade references
                if (inn.addsToDeck != null)
                    foreach (var child in inn.addsToDeck)
                        if (child != null && child.innovationId == id) return child;
            }
            Debug.LogWarning($"[Innovation] InnovationSO not found for id: {id}");
            return null;
        }

        // ── Crafter Unlock ────────────────────────────────────────────────────
        public bool TryUnlockCrafter(CrafterSO crafter)
        {
            if (_campaign.builtCrafterNames.Contains(crafter.crafterName))
            {
                Debug.Log($"[Crafter] Already built: {crafter.crafterName}");
                return false;
            }

            // Check unlock cost
            if (crafter.unlockCost != null)
            {
                for (int i = 0; i < crafter.unlockCost.Length; i++)
                {
                    int needed = (crafter.unlockCostAmounts != null && i < crafter.unlockCostAmounts.Length)
                        ? crafter.unlockCostAmounts[i] : 0;
                    int have   = GetResourceAmount(crafter.unlockCost[i].resourceName);
                    if (have < needed)
                    {
                        Debug.LogWarning($"[Crafter] Cannot unlock {crafter.crafterName} — " +
                                         $"need {needed} {crafter.unlockCost[i].resourceName}, have {have}");
                        return false;
                    }
                }

                // Deduct
                for (int i = 0; i < crafter.unlockCost.Length; i++)
                {
                    int needed = (crafter.unlockCostAmounts != null && i < crafter.unlockCostAmounts.Length)
                        ? crafter.unlockCostAmounts[i] : 0;
                    RemoveResource(crafter.unlockCost[i].resourceName, needed);
                }
            }

            var built = new List<string>(_campaign.builtCrafterNames) { crafter.crafterName };
            _campaign.builtCrafterNames = built.ToArray();

            AddToChronicle($"Year {_campaign.currentYear}: {crafter.crafterName} built.");
            Debug.Log($"[Crafter] Unlocked: {crafter.crafterName}");
            return true;
        }

        // ── Crafting ──────────────────────────────────────────────────────────
        public bool TryCraftItem(ItemSO item, string forCharacterId)
        {
            // Crafter must be built
            if (!IsCrafterBuiltForItem(item))
            {
                Debug.LogWarning($"[Crafting] No built Crafter for: {item.itemName}");
                return false;
            }

            // Check resources
            if (item.craftingCost != null)
            {
                for (int i = 0; i < item.craftingCost.Length; i++)
                {
                    int needed = (item.craftingCostAmounts != null && i < item.craftingCostAmounts.Length)
                        ? item.craftingCostAmounts[i] : 0;
                    int have   = GetResourceAmount(item.craftingCost[i].resourceName);
                    if (have < needed)
                    {
                        Debug.LogWarning($"[Crafting] Insufficient {item.craftingCost[i].resourceName}: " +
                                         $"need {needed}, have {have}");
                        return false;
                    }
                }

                // Deduct
                for (int i = 0; i < item.craftingCost.Length; i++)
                {
                    int needed = (item.craftingCostAmounts != null && i < item.craftingCostAmounts.Length)
                        ? item.craftingCostAmounts[i] : 0;
                    RemoveResource(item.craftingCost[i].resourceName, needed);
                }
            }

            // Add to character's loadout
            var character = GetCharacter(forCharacterId);
            if (character == null) return false;

            var items = new List<string>(character.equippedItemNames) { item.itemName };
            character.equippedItemNames = items.ToArray();

            Debug.Log($"[Crafting] Crafted {item.itemName} for {character.characterName}");
            return true;
        }

        private bool IsCrafterBuiltForItem(ItemSO item)
        {
            // An item can be crafted if ANY built crafter lists it in its recipeList
            if (_campaignData.crafterPool == null) return false;
            return _campaignData.crafterPool.Any(crafter =>
                crafter != null &&
                _campaign.builtCrafterNames.Contains(crafter.crafterName) &&
                crafter.recipeList != null &&
                crafter.recipeList.Any(recipe => recipe != null && recipe.itemName == item.itemName));
        }

        // ── Character Retirement ─────────────────────────────────────────────

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

        // ── Permanent Character Death ─────────────────────────────────────────
        public void OnPermanentCharacterDeath(string characterId)
        {
            var character = GetCharacter(characterId);
            if (character == null)
            {
                Debug.LogWarning($"[Settlement] OnPermanentCharacterDeath: character '{characterId}' not found");
                return;
            }

            // Move from active roster to deceased
            var active   = new List<RuntimeCharacterState>(_campaign.characters);
            var deceased = new List<RuntimeCharacterState>(_campaign.deceasedCharacters);
            active.Remove(character);
            deceased.Add(character);
            _campaign.characters         = active.ToArray();
            _campaign.deceasedCharacters = deceased.ToArray();

            AddToChronicle($"Year {_campaign.currentYear}: {character.characterName} has died.");
            Debug.Log($"[Settlement] *** PERMANENT DEATH: {character.characterName} ***");

            // Queue EVT-14 (The Grief) — fires next settlement phase regardless of year/tag
            var evt14 = System.Array.Find(_campaignData.eventPool, e => e.eventId == "EVT-14");
            if (evt14 == null)
            {
                Debug.LogWarning("[Settlement] EVT-14 not found in event pool — cannot queue The Grief");
                return;
            }

            if (_campaign.resolvedEventIds.Contains("EVT-14"))
            {
                Debug.Log("[Settlement] EVT-14 already resolved — The Grief will not fire again");
                return;
            }

            if (_campaign.pendingEventIds.Contains("EVT-14"))
            {
                Debug.Log("[Settlement] EVT-14 already pending");
                return;
            }

            var pending = new List<string>(_campaign.pendingEventIds) { "EVT-14" };
            _campaign.pendingEventIds = pending.ToArray();
            Debug.Log("[Settlement] EVT-14 (The Grief) queued — will fire next settlement phase");
        }

        // ── Character Birth ───────────────────────────────────────────────────

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

        // ── Year Advance ──────────────────────────────────────────────────────

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

        // ── Shuffle Helper ────────────────────────────────────────────────────
        private static void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
