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
        public RuntimeCharacterState[] deceasedCharacters;

        // Resources — flat inventory, one entry per resource type
        public ResourceEntry[] resources;

        // Settlement
        public string[] builtCrafterNames;       // CrafterSO asset names of built Crafters
        public string[] availableRecipeNames;    // ItemSO names currently craftable

        // Campaign progression — all IDs stored as strings
        public string[] adoptedInnovationIds;
        public string[] availableInnovationIds;  // Current Innovation Deck pool
        public string[] resolvedEventIds;        // EVT-XX IDs already seen
        public string[] pendingEventIds;         // Queued to fire next settlement regardless of year/tag filter
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
