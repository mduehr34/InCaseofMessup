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

        [Header("Body Parts — index 0=Standard, 1=Hardened, 2=Apex")]
        public MonsterBodyPart[] standardParts;
        public MonsterBodyPart[] hardenedParts;
        public MonsterBodyPart[] apexParts;

        [Header("Behavior Deck")]
        public BehaviorCardSO[] openingCards;
        public BehaviorCardSO[] escalationCards;
        public BehaviorCardSO[] apexCards;
        public BehaviorCardSO[] permanentCards;

        [Header("Elemental Profile")]
        public ElementTag[] weaknesses;
        public ElementTag[] resistances;

        [Header("Facing Tables")]
        public FacingTable frontFacing;
        public FacingTable flankFacing;
        public FacingTable rearFacing;

        [Header("Trap Zones")]
        public string[] trapZoneParts;      // Part names that are trap zones

        [Header("Loot")]
        public LootEntry[] lootTable;

        [Header("Stances")]
        public StanceDefinition[] stances;
    }
}
