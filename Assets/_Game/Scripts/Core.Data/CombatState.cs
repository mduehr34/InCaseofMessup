using System;

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
        public string lastAttackerId;       // hunterId of the last hunter who attacked the monster
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
        public int accuracy;
        public int strength;
        public int luck;
        public int movement;
        public int currentGrit;
        public int maxGrit;
        public int apRemaining;             // Resets to 2 each Hunter Phase
        public bool hasActedThisPhase;
        public bool hasMovedThisPhase;
        public bool isCollapsed;
        // Body zones: Head, Torso, LeftArm, RightArm, LeftLeg, RightLeg
        public BodyZoneState[] bodyZones;
        // Deck state
        public string[] handCardNames;
        public string[] deckCardNames;
        public string[] discardCardNames;
        // Active status effects as string tags e.g. ["Shaken", "Pinned"]
        public string[] activeStatusEffects;
        // Gear effect tags from active set bonuses e.g. ["GAUNT_3PC_LOUD_SUPPRESS"]
        public string[] activeGearEffectTags;
        // Item names whose once-per-hunt ability has already fired this hunt
        public string[] spentHuntAbilities;
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

        // ── Behavior Deck Runtime State ───────────────────────────────────
        // Counts only — actual card references live in MonsterAI.BehaviorDeck.
        // CombatState tracks counts so UI and save/load can read health without touching MonsterAI.
        public int behaviorDeckCount;
        public int behaviorDiscardCount;
        public int moodCardsInPlayCount;
        public int permanentlyRemovedCount;

        // ── Wound Deck Runtime State ──────────────────────────────────────
        public int woundDeckCount;
        public int woundDiscardCount;

        // ── Critical Wound Tags ───────────────────────────────────────────
        // Set when a critical wound lands on a location with a criticalWoundTag.
        // Read by MonsterAI when resolving ExecuteCard with criticalWoundCondition.
        public string[] criticalWoundTags;
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
