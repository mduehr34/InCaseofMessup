using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/Monster", fileName = "New Monster")]
    public class MonsterSO : ScriptableObject
    {
        [Header("Identity")]
        public string monsterName;
        public int materialTier;
        public int availableFromYear;   // First year this monster appears in hunt selection (0 = always)
        [TextArea] public string animalBasis;
        [TextArea] public string combatEmotion;
        [TextArea] public string coreSkillTaught;

        [Header("Grid Footprint per Difficulty")]
        public Vector2Int gridFootprintStandard;
        public Vector2Int gridFootprintHardened;
        public Vector2Int gridFootprintApex;

        [Header("Stat Blocks — index 0=Standard, 1=Hardened, 2=Apex")]
        public MonsterStatBlock[] statBlocks;

        [Header("Behavior Card Pools")]
        // Cards are randomly drawn from these pools at combat start to build the fight's deck.
        // Pools are authored larger than any single deck — each fight draws a different subset,
        // making repeat encounters feel varied even against the same monster.
        public BehaviorCardSO[] baseCardPool;           // Core cards; available at all difficulties
        public BehaviorCardSO[] advancedCardPool;       // More complex / dangerous cards
        public BehaviorCardSO[] overwhelmingCardPool;   // Apex-tier — peak threat cards

        [Header("Behavior Deck Composition — index 0=Standard, 1=Hardened, 2=Apex")]
        // How many cards to draw from each pool per difficulty.
        // Example: Standard = 12 base + 3 advanced + 0 overwhelming (15 health total)
        //          Hardened  = 14 base + 4 advanced + 2 overwhelming (20 health total)
        // Random draw uses Fisher-Yates on each pool, take first N — see MonsterAI.InitializeDeck
        public BehaviorDeckComposition[] deckCompositions;

        [Header("Wound Location Deck — per Difficulty")]
        // Can be customized per difficulty. Harder difficulties may add
        // higher woundTarget locations or additional traps.
        public WoundLocationSO[] standardWoundDeck;
        public WoundLocationSO[] hardenedWoundDeck;
        public WoundLocationSO[] apexWoundDeck;

        [Header("Elemental Profile")]
        public ElementTag[] weaknesses;
        public ElementTag[] resistances;

        [Header("Facing — Accuracy Modifiers Only")]
        // Wound location draws are NOT filtered by facing (pure random from full wound deck).
        // Facing only modifies the hunter's to-hit roll.
        public FacingAccuracyBonus[] facingBonuses;

        [Header("Loot")]
        public LootEntry[] lootTable;

        [Header("Stances")]
        public StanceDefinition[] stances;

        [Header("Combat Setup")]
        public SpawnZoneSO[] hunterSpawnZones;   // All valid deployment zones — hunters may place in any of them
    }
}
