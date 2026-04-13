using UnityEditor;
using UnityEngine;
using MnM.Core.Data;

namespace MnM.Editor
{
    public static class MockDataCreator
    {
        [MenuItem("MnM/Create Mock Data Assets")]
        public static void CreateAllMockAssets()
        {
            var brace         = CreateMockBrace();
            var creepAdv      = CreateMockCreepingAdvance();
            var spearWound    = CreateMockSpearWound();
            var gauntFang     = CreateMockGauntFang();
            var gauntStandard = CreateMockGauntStandard();
            var aldric        = CreateMockAldric(brace);
            var campaign      = CreateMockTutorialCampaign(gauntStandard);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[MockDataCreator] 7 mock assets created. Stage 1-D complete.");
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = campaign;
        }

        // ------------------------------------------------------------------ //

        private static ResourceSO CreateMockGauntFang()
        {
            var so = ScriptableObject.CreateInstance<ResourceSO>();
            so.resourceName   = "Gaunt Fang";
            so.type           = ResourceType.UniqueCommon;
            so.tier           = 1;
            so.conversionRate = 1f;
            AssetDatabase.CreateAsset(so, "Assets/_Game/Data/Resources/Mock_GauntFang.asset");
            return so;
        }

        private static ActionCardSO CreateMockBrace()
        {
            var so = ScriptableObject.CreateInstance<ActionCardSO>();
            so.cardName                 = "Brace";
            so.weaponType               = WeaponType.FistWeapon;
            so.category                 = CardCategory.Reaction;
            so.apCost                   = 0;
            so.apRefund                 = 0;
            so.isLoud                   = false;
            so.isReaction               = true;
            so.proficiencyTierRequired  = 1;
            so.effectDescription        = "When you take damage, reduce that damage by 2 Shell or 1 Flesh. Declare before damage.";
            AssetDatabase.CreateAsset(so, "Assets/_Game/Data/Cards/Action/Mock_Brace.asset");
            return so;
        }

        private static BehaviorCardSO CreateMockCreepingAdvance()
        {
            var so = ScriptableObject.CreateInstance<BehaviorCardSO>();
            so.cardName          = "Creeping Advance";
            so.cardType          = BehaviorCardType.Removable;
            so.group             = BehaviorGroup.Opening;
            so.triggerCondition  = "End of round";
            so.effectDescription = "Move 3 squares toward Aggro holder";
            so.removalCondition  = "Right Flank Shell break";
            AssetDatabase.CreateAsset(so, "Assets/_Game/Data/Cards/Behavior/Mock_CreepingAdvance.asset");
            return so;
        }

        private static InjuryCardSO CreateMockSpearWound()
        {
            var so = ScriptableObject.CreateInstance<InjuryCardSO>();
            so.injuryName        = "Spear Wound";
            so.bodyPartTag       = BodyPartTag.Torso;
            so.severity          = InjurySeverity.Minor;
            so.effect            = "-1 Strength for the next 2 hunts";
            so.removalCondition  = "Settlement healing action";
            AssetDatabase.CreateAsset(so, "Assets/_Game/Data/Cards/Injury/Mock_SpearWound.asset");
            return so;
        }

        private static MonsterSO CreateMockGauntStandard()
        {
            var so = ScriptableObject.CreateInstance<MonsterSO>();

            // Identity
            so.monsterName     = "The Gaunt";
            so.materialTier    = 1;
            so.animalBasis     = "Marrow-starved wolf, enormous, blind \u2014 hunts by sound and vibration";
            so.combatEmotion   = "Tension \u2014 the monster reacts to noise and movement, not sight";
            so.coreSkillTaught = "Positioning and facing";

            // Grid footprints
            so.gridFootprintStandard = new Vector2Int(2, 2);
            so.gridFootprintHardened = new Vector2Int(2, 2);
            so.gridFootprintApex     = new Vector2Int(3, 3);

            // Stat blocks: index 0=Standard, 1=Hardened, 2=Apex
            so.statBlocks = new MonsterStatBlock[]
            {
                new MonsterStatBlock { movement = 6,  accuracy = 1, strength = 2, toughness = 1, evasion = 2, behaviorDeckSizeRemovable = 9  },
                new MonsterStatBlock { movement = 8,  accuracy = 2, strength = 3, toughness = 2, evasion = 3, behaviorDeckSizeRemovable = 12 },
                new MonsterStatBlock { movement = 10, accuracy = 3, strength = 4, toughness = 3, evasion = 4, behaviorDeckSizeRemovable = 15 },
            };

            // Facing tables
            so.frontFacing = new FacingTable
            {
                primaryZone        = BodyPartTag.Torso,
                secondaryZone      = BodyPartTag.Head,
                tertiaryZone       = BodyPartTag.Arms,
                primaryZoneWeight   = 50,
                secondaryZoneWeight = 30,
                tertiaryZoneWeight  = 20,
            };
            so.flankFacing = new FacingTable
            {
                primaryZone        = BodyPartTag.Arms,
                secondaryZone      = BodyPartTag.Torso,
                tertiaryZone       = BodyPartTag.Legs,
                primaryZoneWeight   = 50,
                secondaryZoneWeight = 30,
                tertiaryZoneWeight  = 20,
            };
            so.rearFacing = new FacingTable
            {
                primaryZone        = BodyPartTag.Legs,
                secondaryZone      = BodyPartTag.Waist,
                tertiaryZone       = BodyPartTag.Back,
                primaryZoneWeight   = 50,
                secondaryZoneWeight = 30,
                tertiaryZoneWeight  = 20,
            };

            // All card arrays, body parts, loot, stances left empty — populated in Stage 7

            AssetDatabase.CreateAsset(so, "Assets/_Game/Data/Monsters/Mock_GauntStandard.asset");
            return so;
        }

        private static CharacterSO CreateMockAldric(ActionCardSO brace)
        {
            var so = ScriptableObject.CreateInstance<CharacterSO>();
            so.characterName = "Aldric";
            so.bodyBuild     = CharacterBuild.Aethel;
            so.sex           = CharacterSex.Male;
            so.accuracy      = 0;
            so.evasion       = 0;
            so.strength      = 0;
            so.toughness     = 0;
            so.luck          = 0;
            so.movement      = 3;
            so.huntCount     = 0;
            so.isRetired     = false;
            so.currentDeck   = new ActionCardSO[] { brace };
            AssetDatabase.CreateAsset(so, "Assets/_Game/Data/Characters/Mock_Aldric.asset");
            return so;
        }

        private static CampaignSO CreateMockTutorialCampaign(MonsterSO gauntStandard)
        {
            var so = ScriptableObject.CreateInstance<CampaignSO>();
            so.campaignName          = "Tutorial Campaign";
            so.difficulty            = DifficultyLevel.Medium;
            so.campaignLengthYears   = 3;
            so.startingCharacterCount = 8;
            so.baseMovement          = 3;
            so.startingGrit          = 3;
            so.ironmanMode           = false;
            so.retirementHuntCount   = 10;
            so.monsterRoster         = new MonsterSO[] { gauntStandard };
            AssetDatabase.CreateAsset(so, "Assets/_Game/Data/Campaigns/Mock_TutorialCampaign.asset");
            return so;
        }
    }
}
