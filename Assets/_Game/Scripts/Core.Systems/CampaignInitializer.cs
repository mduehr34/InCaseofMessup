using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MnM.Core.Data;

namespace MnM.Core.Systems
{
    public static class CampaignInitializer
    {
        // ── Name Pools (GDD canonical) ───────────────────────────
        private static readonly string[] MaleNames =
        {
            "Aldric", "Aethel", "Beorn", "Brunulf", "Cyne", "Duna",
            "Eadric", "Faelan", "Gerulf", "Godwin", "Hrothgar", "Ingvar",
            "Jorund", "Knut", "Leofric", "Modulf", "Nidhogg", "Oswin",
            "Ragnar", "Sigbert", "Thorald", "Ulf", "Wulfric", "Yrsa"
        };

        private static readonly string[] FemaleNames =
        {
            "Aldis", "Brunhild", "Dagrun", "Edith", "Eira", "Elfrun",
            "Freya", "Gerd", "Gunhild", "Hild", "Hildur", "Ingrid",
            "Isrun", "Kadrun", "Liesel", "Moira", "Norna", "Osrun",
            "Runa", "Sigrun", "Svala", "Thora", "Ulfhild", "Ylva"
        };

        private static readonly string[] MaleBuilds =
            { "Aethel", "Beorn", "Cyne", "Duna" };

        private static readonly string[] FemaleBuilds =
            { "Eira", "Freya", "Gerd", "Hild" };

        // ── Main Entry Point ─────────────────────────────────────
        public static CampaignState CreateNewCampaign(CampaignSO campaignData)
        {
            if (campaignData == null)
                throw new ArgumentNullException(nameof(campaignData));

            var state = new CampaignState
            {
                campaignId          = Guid.NewGuid().ToString(),
                campaignSoName      = campaignData.name,
                currentYear         = 1,
                difficulty          = campaignData.difficulty.ToString(),

                retiredCharacters           = new RuntimeCharacterState[0],
                resources                   = new ResourceEntry[0],
                builtCrafterNames           = new string[0],
                availableRecipeNames        = new string[0],
                adoptedInnovationIds        = new string[0],
                resolvedEventIds            = new string[0],
                unlockedArtifactIds         = new string[0],
                unlockedCodexEntryIds       = new string[0],
                activeGuidingPrincipalIds   = new string[0],
                resolvedGuidingPrincipalIds = new string[0],
                chronicleLog                = new[] { "Year 1: The settlement begins." },
                pendingHuntResult           = default,
            };

            // Seed Innovation Deck from CampaignSO starting set
            state.availableInnovationIds = campaignData.startingInnovations != null
                ? campaignData.startingInnovations
                    .Where(i => i != null)
                    .Select(i => i.innovationId)
                    .ToArray()
                : new string[0];

            // Generate starting characters with correct gender split
            state.characters = GenerateStartingCharacters(campaignData);

            Debug.Log($"[Campaign] New campaign created. " +
                      $"Difficulty:{campaignData.difficulty} " +
                      $"Characters:{state.characters.Length} " +
                      $"Innovation pool size:{state.availableInnovationIds.Length}");

            return state;
        }

        // ── Character Generation ─────────────────────────────────
        private static RuntimeCharacterState[] GenerateStartingCharacters(CampaignSO campaignData)
        {
            int total  = campaignData.startingCharacterCount;
            int male   = total / 2;
            int female = total - male; // Handles odd totals gracefully

            var malePool   = new List<string>(MaleNames);
            var femalePool = new List<string>(FemaleNames);
            ShuffleList(malePool);
            ShuffleList(femalePool);

            var characters = new List<RuntimeCharacterState>();

            for (int i = 0; i < male; i++)
            {
                string name  = malePool[i % malePool.Count];
                string build = MaleBuilds[UnityEngine.Random.Range(0, MaleBuilds.Length)];
                characters.Add(CreateStartingCharacter(name, "Male", build));
                Debug.Log($"[Campaign] Generated character: {name} (Male, {build})");
            }

            for (int i = 0; i < female; i++)
            {
                string name  = femalePool[i % femalePool.Count];
                string build = FemaleBuilds[UnityEngine.Random.Range(0, FemaleBuilds.Length)];
                characters.Add(CreateStartingCharacter(name, "Female", build));
                Debug.Log($"[Campaign] Generated character: {name} (Female, {build})");
            }

            return characters.ToArray();
        }

        private static RuntimeCharacterState CreateStartingCharacter(
            string name, string sex, string build)
        {
            return new RuntimeCharacterState
            {
                characterId             = Guid.NewGuid().ToString(),
                characterName           = name,
                bodyBuild               = build,
                sex                     = sex,
                // All stats start at 0 — modified only by events/innovations
                accuracy                = 0,
                evasion                 = 0,
                strength                = 0,
                toughness               = 0,
                luck                    = 0,
                movement                = 3, // Base movement — overridden by CampaignSO later
                // Year 1: bare fists only — Brace and Shove are the starting cards
                deckCardNames           = new[] { "Brace", "Shove" },
                injuryCardNames         = new string[0],
                fightingArtNames        = new string[0],
                disorderNames           = new string[0],
                // Bare fist proficiency starts at Tier 1
                proficiencyWeaponTypes  = new[] { "FistWeapon" },
                proficiencyTiers        = new[] { 1 },
                proficiencyActivations  = new[] { 0 },
                huntCount               = 0,
                isRetired               = false,
                equippedItemNames       = new string[0],
                equippedWeaponName      = "",
            };
        }

        // ── Helpers ──────────────────────────────────────────────
        private static void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        // ── CombatState Builder ──────────────────────────────────
        // Converts a CampaignState + chosen hunters into a CombatState
        // Called by GameStateManager before loading the combat scene (Stage 6)
        public static CombatState BuildCombatState(
            CampaignState campaign,
            MonsterSO monster,
            string difficulty,
            RuntimeCharacterState[] selectedHunters)
        {
            var hunters = new HunterCombatState[selectedHunters.Length];
            for (int i = 0; i < selectedHunters.Length; i++)
            {
                var ch = selectedHunters[i];
                hunters[i] = new HunterCombatState
                {
                    hunterId            = ch.characterId,
                    hunterName          = ch.characterName,
                    gridX               = 3 + i * 2, // Default starting positions
                    gridY               = 8,
                    facingX             = 1,
                    facingY             = 0,
                    currentGrit         = 3, // Seeded from CampaignSO in Stage 6
                    maxGrit             = 3,
                    apRemaining         = 2,
                    hasActedThisPhase   = false,
                    isCollapsed         = false,
                    bodyZones           = BuildHunterBodyZones(),
                    handCardNames       = new string[0],
                    deckCardNames       = ch.deckCardNames,
                    discardCardNames    = new string[0],
                    activeStatusEffects = new string[0],
                    // Stats from RuntimeCharacterState
                    accuracy            = ch.accuracy,
                    strength            = ch.strength,
                    luck                = ch.luck,
                    movement            = ch.movement,
                };
            }

            // Monster state from SO
            int diffIndex = difficulty switch { "Hardened" => 1, "Apex" => 2, _ => 0 };
            var stats     = monster.statBlocks != null && diffIndex < monster.statBlocks.Length
                ? monster.statBlocks[diffIndex]
                : default;

            var footprint = difficulty switch
            {
                "Hardened" => monster.gridFootprintHardened,
                "Apex"     => monster.gridFootprintApex,
                _          => monster.gridFootprintStandard,
            };

            var monsterState = new MonsterCombatState
            {
                monsterName         = monster.monsterName,
                difficulty          = difficulty,
                gridX               = 14,
                gridY               = 7,
                facingX             = -1,
                facingY             = 0,
                footprintW          = footprint.x,
                footprintH          = footprint.y,
                parts               = BuildMonsterParts(monster, difficulty),
                activeDeckCardNames = GetDeckCardNames(monster),
                removedCardNames    = new string[0],
                currentStanceTag    = "",
                activeStatusEffects = new string[0],
            };

            return new CombatState
            {
                campaignId    = campaign.campaignId,
                campaignYear  = campaign.currentYear,
                currentRound  = 0,
                currentPhase  = "VitalityPhase",
                aggroHolderId = hunters.Length > 0 ? hunters[0].hunterId : "",
                hunters       = hunters,
                monster       = monsterState,
                grid          = new GridState
                {
                    width           = 22,
                    height          = 16,
                    deniedCells     = new DeniedCell[0],
                    marrowSinkCells = new string[0],
                },
                log = new string[0],
            };
        }

        private static BodyZoneState[] BuildHunterBodyZones() => new[]
        {
            new BodyZoneState { zone="Head",     shellCurrent=2, shellMax=2, fleshCurrent=3, fleshMax=3 },
            new BodyZoneState { zone="Torso",    shellCurrent=2, shellMax=2, fleshCurrent=3, fleshMax=3 },
            new BodyZoneState { zone="LeftArm",  shellCurrent=1, shellMax=1, fleshCurrent=2, fleshMax=2 },
            new BodyZoneState { zone="RightArm", shellCurrent=1, shellMax=1, fleshCurrent=2, fleshMax=2 },
            new BodyZoneState { zone="LeftLeg",  shellCurrent=1, shellMax=1, fleshCurrent=2, fleshMax=2 },
            new BodyZoneState { zone="RightLeg", shellCurrent=1, shellMax=1, fleshCurrent=2, fleshMax=2 },
        };

        private static MonsterPartState[] BuildMonsterParts(MonsterSO monster, string difficulty)
        {
            var parts = difficulty switch
            {
                "Hardened" => monster.hardenedParts,
                "Apex"     => monster.apexParts,
                _          => monster.standardParts,
            };
            if (parts == null || parts.Length == 0) return new MonsterPartState[0];

            return System.Array.ConvertAll(parts, p => new MonsterPartState
            {
                partName     = p.partName,
                shellCurrent = p.shellDurability,
                shellMax     = p.shellDurability,
                fleshCurrent = p.fleshDurability,
                fleshMax     = p.fleshDurability,
                isBroken     = false,
                isRevealed   = !p.isTrapZone, // Trap zones start hidden
                isExposed    = false,
                woundCount   = 0,
            });
        }

        private static string[] GetDeckCardNames(MonsterSO monster)
        {
            var names = new List<string>();
            if (monster.openingCards    != null)
                foreach (var c in monster.openingCards)    if (c != null) names.Add(c.cardName);
            if (monster.escalationCards != null)
                foreach (var c in monster.escalationCards) if (c != null) names.Add(c.cardName);
            if (monster.apexCards       != null)
                foreach (var c in monster.apexCards)       if (c != null) names.Add(c.cardName);
            return names.ToArray();
        }
    }
}
