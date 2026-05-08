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
            // Behavior card pools
            var creepAdv    = CreateMockCreepingAdvance();
            var gauntSlash  = CreateMockGauntSlash();
            var boneRattle  = CreateMockBoneRattle();
            var braceCard   = CreateMockBraceCard();
            var spearThrust = CreateMockSpearThrust();

            // Ensure WoundLocation folder exists
            if (!AssetDatabase.IsValidFolder("Assets/_Game/Data/Cards/WoundLocation"))
                AssetDatabase.CreateFolder("Assets/_Game/Data/Cards", "WoundLocation");

            // Wound locations
            var gauntJaw      = CreateMockGauntJaw();
            var gauntClaw     = CreateMockGauntClaw();
            var spikedTail    = CreateMockSpikedTail();
            var bonyShoulder  = CreateMockBonyShoulder();
            var spineTrap     = CreateMockSpineTrap();

            var gauntStandard = CreateMockGauntStandard(
                new BehaviorCardSO[] { creepAdv, gauntSlash, boneRattle, braceCard },
                new BehaviorCardSO[] { spearThrust },
                new WoundLocationSO[] { gauntJaw, gauntClaw, spikedTail, bonyShoulder, spineTrap });

            // Hunter action card (Brace — separate from monster Brace behavior card)
            var braceAction = CreateMockBraceActionCard();
            var gauntFang   = CreateMockGauntFang();
            var aldric      = CreateMockAldric(braceAction);
            var campaign    = CreateMockTutorialCampaign(gauntStandard);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[MockDataCreator] Mock assets created (Stage 8-M format). " +
                      "Gaunt: 4 base cards, 1 advanced card, 5 wound locations.");
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = campaign;
        }

        // ── Behavior Cards — Base Pool ──────────────────────────── //

        private static BehaviorCardSO CreateMockCreepingAdvance()
        {
            var so = ScriptableObject.CreateInstance<BehaviorCardSO>();
            so.cardName          = "Creeping Advance";
            so.cardType          = BehaviorCardType.Removable;
            so.triggerCondition  = "Always";
            so.effectDescription = "Monster moves 3 squares toward nearest hunter";
            so.hasTargetIdentification = false;
            so.hasMovement       = true;
            so.hasDamage         = false;
            AssetDatabase.CreateAsset(so, "Assets/_Game/Data/Cards/Behavior/Mock_CreepingAdvance.asset");
            return so;
        }

        private static BehaviorCardSO CreateMockGauntSlash()
        {
            var so = ScriptableObject.CreateInstance<BehaviorCardSO>();
            so.cardName          = "Gaunt Slash";
            so.cardType          = BehaviorCardType.Removable;
            so.triggerCondition  = "Always";
            so.effectDescription = "The Gaunt rakes a claw across the nearest hunter";
            so.hasTargetIdentification = true;
            so.targetRule        = "nearest";
            so.hasMovement       = false;
            so.hasDamage         = true;
            so.forcedHunterBodyPart = "";
            // Alternate behavior when GauntJaw_Critical is active
            so.criticalWoundCondition    = "GauntJaw_Critical";
            so.alternateTriggerCondition = "Draws back, jaw hanging";
            so.alternateEffectDescription = "The Gaunt recoils from its wounded jaw — cries out, no attack this turn";
            AssetDatabase.CreateAsset(so, "Assets/_Game/Data/Cards/Behavior/Mock_GauntSlash.asset");
            return so;
        }

        private static BehaviorCardSO CreateMockBoneRattle()
        {
            var so = ScriptableObject.CreateInstance<BehaviorCardSO>();
            so.cardName          = "Bone Rattle";
            so.cardType          = BehaviorCardType.Mood;
            so.triggerCondition  = "Always";
            so.effectDescription = "The Gaunt's bones clatter — hunters are unnerved. -1 to all accuracy rolls while active.";
            so.hasTargetIdentification = false;
            so.hasMovement       = false;
            so.hasDamage         = false;
            so.removalCondition  = "Hunter inflicts a wound";
            AssetDatabase.CreateAsset(so, "Assets/_Game/Data/Cards/Behavior/Mock_BoneRattle.asset");
            return so;
        }

        private static BehaviorCardSO CreateMockBraceCard()
        {
            var so = ScriptableObject.CreateInstance<BehaviorCardSO>();
            so.cardName          = "Brace";
            so.cardType          = BehaviorCardType.Removable;
            so.triggerCondition  = "Always";
            so.effectDescription = "The Gaunt hunkers down — reaction only, no attack this turn";
            so.hasTargetIdentification = false;
            so.hasMovement       = false;
            so.hasDamage         = false;
            AssetDatabase.CreateAsset(so, "Assets/_Game/Data/Cards/Behavior/Mock_BraceMonster.asset");
            return so;
        }

        // ── Behavior Cards — Advanced Pool ──────────────────────── //

        private static BehaviorCardSO CreateMockSpearThrust()
        {
            var so = ScriptableObject.CreateInstance<BehaviorCardSO>();
            so.cardName          = "Spear Thrust";
            so.cardType          = BehaviorCardType.SingleTrigger;
            so.triggerCondition  = "Always";
            so.effectDescription = "A devastating lunge — the Gaunt drives its spine-crest forward";
            so.hasTargetIdentification = true;
            so.targetRule        = "nearest";
            so.hasMovement       = false;
            so.hasDamage         = true;
            so.forcedHunterBodyPart = "Torso";
            AssetDatabase.CreateAsset(so, "Assets/_Game/Data/Cards/Behavior/Mock_SpearThrust.asset");
            return so;
        }

        // ── Wound Locations ─────────────────────────────────────── //

        private static WoundLocationSO CreateMockGauntJaw()
        {
            var so = ScriptableObject.CreateInstance<WoundLocationSO>();
            so.locationName      = "Gaunt Jaw";
            so.partTag           = BodyPartTag.Head;
            so.woundTarget       = 6;
            so.isTrap            = false;
            so.isImpervious      = false;
            so.failureEffect     = "The jaw snaps shut — the hunter recoils but escapes";
            so.woundEffect       = "The Gaunt's jaw cracks";
            so.criticalEffect    = "The jaw splits — GauntJaw_Critical flag set";
            so.criticalWoundTag  = "GauntJaw_Critical";
            AssetDatabase.CreateAsset(so, "Assets/_Game/Data/Cards/WoundLocation/Mock_WL_GauntJaw.asset");
            return so;
        }

        private static WoundLocationSO CreateMockGauntClaw()
        {
            var so = ScriptableObject.CreateInstance<WoundLocationSO>();
            so.locationName      = "Gaunt Claw";
            so.partTag           = BodyPartTag.Arms;
            so.woundTarget       = 5;
            so.isTrap            = false;
            so.isImpervious      = false;
            so.failureEffect     = "The claw deflects the blow";
            so.woundEffect       = "A claw shatters";
            so.criticalEffect    = "Two claws shear off — grievous wound";
            AssetDatabase.CreateAsset(so, "Assets/_Game/Data/Cards/WoundLocation/Mock_WL_GauntClaw.asset");
            return so;
        }

        private static WoundLocationSO CreateMockSpikedTail()
        {
            var so = ScriptableObject.CreateInstance<WoundLocationSO>();
            so.locationName      = "Spiked Tail";
            so.partTag           = BodyPartTag.Tail;
            so.woundTarget       = 7;
            so.isTrap            = false;
            so.isImpervious      = false;
            so.failureEffect     = "The tail whips away — too armored to penetrate";
            so.woundEffect       = "A spine snaps from the tail";
            so.criticalEffect    = "The tail is severed — hunter gains a spine trophy";
            AssetDatabase.CreateAsset(so, "Assets/_Game/Data/Cards/WoundLocation/Mock_WL_SpikedTail.asset");
            return so;
        }

        private static WoundLocationSO CreateMockBonyShoulder()
        {
            var so = ScriptableObject.CreateInstance<WoundLocationSO>();
            so.locationName      = "Bony Shoulder";
            so.partTag           = BodyPartTag.Torso;
            so.woundTarget       = 5;
            so.isTrap            = false;
            so.isImpervious      = false;
            so.failureEffect     = "Strike glances off the bony plate";
            so.woundEffect       = "A chunk of shoulder bone breaks free";
            so.criticalEffect    = "The shoulder plate shatters";
            AssetDatabase.CreateAsset(so, "Assets/_Game/Data/Cards/WoundLocation/Mock_WL_BonyShoulder.asset");
            return so;
        }

        private static WoundLocationSO CreateMockSpineTrap()
        {
            var so = ScriptableObject.CreateInstance<WoundLocationSO>();
            so.locationName      = "Spine Trap";
            so.partTag           = BodyPartTag.Back;
            so.woundTarget       = 0;    // Irrelevant for traps
            so.isTrap            = true;
            so.trapEffect        = "Gaunt strikes back for 1 damage before the hunter can react";
            AssetDatabase.CreateAsset(so, "Assets/_Game/Data/Cards/WoundLocation/Mock_WL_SpineTrap.asset");
            return so;
        }

        // ── Monster SO ──────────────────────────────────────────── //

        private static MonsterSO CreateMockGauntStandard(
            BehaviorCardSO[] basePool,
            BehaviorCardSO[] advancedPool,
            WoundLocationSO[] woundDeck)
        {
            var so = ScriptableObject.CreateInstance<MonsterSO>();

            so.monsterName     = "The Gaunt";
            so.materialTier    = 1;
            so.animalBasis     = "Marrow-starved wolf, enormous, blind — hunts by sound and vibration";
            so.combatEmotion   = "Tension — the monster reacts to noise and movement, not sight";
            so.coreSkillTaught = "Positioning and facing";

            so.gridFootprintStandard = new Vector2Int(2, 2);
            so.gridFootprintHardened = new Vector2Int(2, 2);
            so.gridFootprintApex     = new Vector2Int(3, 3);

            // Stat blocks: index 0=Standard, 1=Hardened, 2=Apex
            so.statBlocks = new MonsterStatBlock[]
            {
                new MonsterStatBlock { movement = 6,  accuracy = 1, strength = 2, toughness = 1, evasion = 2 },
                new MonsterStatBlock { movement = 8,  accuracy = 2, strength = 3, toughness = 2, evasion = 3 },
                new MonsterStatBlock { movement = 10, accuracy = 3, strength = 4, toughness = 3, evasion = 4 },
            };

            // Card pools
            so.baseCardPool        = basePool;
            so.advancedCardPool    = advancedPool;
            so.overwhelmingCardPool = new BehaviorCardSO[0];

            // Deck compositions: Standard=3/0/0 (3 health), Hardened=4/1/0 (5 health), Apex=4/1/0 (stub — update in 8-U)
            so.deckCompositions = new BehaviorDeckComposition[]
            {
                new BehaviorDeckComposition { baseCardCount = 3, advancedCardCount = 0, overwhelmingCardCount = 0 },
                new BehaviorDeckComposition { baseCardCount = 4, advancedCardCount = 1, overwhelmingCardCount = 0 },
                new BehaviorDeckComposition { baseCardCount = 4, advancedCardCount = 1, overwhelmingCardCount = 0 },
            };

            // Wound decks — Standard uses the 5-card mock deck
            so.standardWoundDeck = woundDeck;
            so.hardenedWoundDeck = woundDeck;
            so.apexWoundDeck     = woundDeck;

            // Facing — accuracy modifiers only (wound location draws are pure random)
            so.facingBonuses = new FacingAccuracyBonus[]
            {
                new FacingAccuracyBonus { arc = FacingArc.Front, accuracyModifier = 0 },
                new FacingAccuracyBonus { arc = FacingArc.Flank, accuracyModifier = 1 },
                new FacingAccuracyBonus { arc = FacingArc.Rear,  accuracyModifier = 2 },
            };

            AssetDatabase.CreateAsset(so, "Assets/_Game/Data/Monsters/Mock_GauntStandard.asset");
            return so;
        }

        // ── Hunter / Campaign ────────────────────────────────────── //

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

        private static ActionCardSO CreateMockBraceActionCard()
        {
            var so = ScriptableObject.CreateInstance<ActionCardSO>();
            so.cardName                = "Brace";
            so.weaponType              = WeaponType.FistWeapon;
            so.category                = CardCategory.Reaction;
            so.apCost                  = 0;
            so.apRefund                = 0;
            so.isLoud                  = false;
            so.isReaction              = true;
            so.proficiencyTierRequired = 1;
            so.effectDescription       = "When you take damage, reduce that damage by 2 Shell or 1 Flesh. Declare before damage.";
            AssetDatabase.CreateAsset(so, "Assets/_Game/Data/Cards/Action/Mock_Brace.asset");
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
            so.strength      = 3;   // Stage 8-M mock: Strength 3, Luck 2 per debug scenario
            so.toughness     = 0;
            so.luck          = 2;
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
            so.campaignName           = "Tutorial Campaign";
            so.difficulty             = DifficultyLevel.Medium;
            so.campaignLengthYears    = 3;
            so.startingCharacterCount = 8;
            so.baseMovement           = 3;
            so.startingGrit           = 3;
            so.ironmanMode            = false;
            so.retirementHuntCount    = 10;
            so.monsterRoster          = new MonsterSO[] { gauntStandard };
            AssetDatabase.CreateAsset(so, "Assets/_Game/Data/Campaigns/Mock_TutorialCampaign.asset");
            return so;
        }
    }
}
