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
                campaignId           = Guid.NewGuid().ToString(),
                campaignSoName       = "Mock_TutorialCampaign",
                currentYear          = 1,
                difficulty           = "Medium",

                characters = new[]
                {
                    BuildMockCharacter("char_aldric",   "Aldric",   "Male",   "Aethel"),
                    BuildMockCharacter("char_brunhild", "Brunhild", "Female", "Eira"),
                },
                retiredCharacters     = new RuntimeCharacterState[0],
                deceasedCharacters    = new RuntimeCharacterState[0],

                resources            = new ResourceEntry[0],
                builtCrafterNames    = new string[0],
                availableRecipeNames = new string[0],

                adoptedInnovationIds        = new string[0],
                availableInnovationIds      = new[] { "INN-01", "INN-02", "INN-03" },
                resolvedEventIds            = new string[0],
                pendingEventIds             = new string[0],
                unlockedArtifactIds         = new string[0],
                unlockedCodexEntryIds       = new string[0],
                activeGuidingPrincipalIds   = new string[0],
                resolvedGuidingPrincipalIds = new string[0],

                chronicleLog      = new[] { "Year 1: The settlement begins." },
                pendingHuntResult = default,
            };
        }

        public static RuntimeCharacterState BuildMockCharacter(
            string id, string name, string sex, string build)
        {
            return new RuntimeCharacterState
            {
                characterId            = id,
                characterName          = name,
                bodyBuild              = build,
                sex                    = sex,
                accuracy               = 0,
                evasion                = 0,
                strength               = 0,
                toughness              = 0,
                luck                   = 0,
                movement               = 3,
                deckCardNames          = new[] { "Brace", "Shove" },
                injuryCardNames        = new string[0],
                fightingArtNames       = new string[0],
                disorderNames          = new string[0],
                proficiencyWeaponTypes = new[] { "FistWeapon" },
                proficiencyTiers       = new[] { 1 },
                proficiencyActivations = new[] { 0 },
                huntCount              = 0,
                isRetired              = false,
                equippedItemNames      = new string[0],
                equippedWeaponName     = "",
            };
        }

        // Builds a mock HuntResult for a Gaunt Standard victory
        public static HuntResult BuildMockGauntVictory(string[] hunterIds)
        {
            return new HuntResult
            {
                isVictory         = true,
                monsterName       = "The Gaunt",
                monsterDifficulty = "Standard",
                roundsFought      = 8,
                collapsedHunterIds = new string[0],
                survivingHunterIds = hunterIds,
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
