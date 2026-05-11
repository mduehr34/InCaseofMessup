using UnityEngine;
using UnityEditor;
using MnM.Core.Data;
using System.IO;

public class CreateGauntSOAssets_8N
{
    public static void Execute()
    {
        // ── Folder Structure ─────────────────────────────────────────
        string[] folders = new[]
        {
            "Assets/_Game/Data/Monsters/Gaunt",
            "Assets/_Game/Data/Monsters/Gaunt/BehaviorCards",
            "Assets/_Game/Data/Monsters/Gaunt/BehaviorCards/Base",
            "Assets/_Game/Data/Monsters/Gaunt/BehaviorCards/Advanced",
            "Assets/_Game/Data/Monsters/Gaunt/WoundLocations",
            "Assets/_Game/Data/Monsters/Gaunt/WoundLocations/Standard",
            "Assets/_Game/Data/Monsters/Gaunt/WoundLocations/Hardened",
            "Assets/_Game/Data/Monsters/Gaunt/MonsterSO",
        };
        foreach (var folder in folders)
            if (!AssetDatabase.IsValidFolder(folder))
            {
                var parts = folder.Split('/');
                var parent = string.Join("/", parts, 0, parts.Length - 1);
                AssetDatabase.CreateFolder(parent, parts[parts.Length - 1]);
                Debug.Log($"[8N] Created folder: {folder}");
            }

        // ── Helper ───────────────────────────────────────────────────
        T CreateOrLoad<T>(string path) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;
            var so = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(so, path);
            return so;
        }

        // ═══════════════════════════════════════════════════════════════
        // BASE CARD POOL — 6 cards
        // ═══════════════════════════════════════════════════════════════

        // 1. Creeping Advance
        var creepingAdvance = CreateOrLoad<BehaviorCardSO>(
            "Assets/_Game/Data/Monsters/Gaunt/BehaviorCards/Base/Gaunt_CreepingAdvance.asset");
        creepingAdvance.cardName              = "Creeping Advance";
        creepingAdvance.cardType              = BehaviorCardType.Removable;
        creepingAdvance.triggerCondition      = "The Gaunt slinks forward, drawn by the scent of prey.";
        creepingAdvance.effectDescription     = "The Gaunt moves one space toward the nearest hunter.";
        creepingAdvance.hasTargetIdentification = false;
        creepingAdvance.hasMovement           = true;
        creepingAdvance.hasDamage             = false;
        creepingAdvance.targetRule            = "";
        creepingAdvance.forcedHunterBodyPart  = "";
        EditorUtility.SetDirty(creepingAdvance);

        // 2. Gaunt Slash
        var gauntSlash = CreateOrLoad<BehaviorCardSO>(
            "Assets/_Game/Data/Monsters/Gaunt/BehaviorCards/Base/Gaunt_GauntSlash.asset");
        gauntSlash.cardName                 = "Gaunt Slash";
        gauntSlash.cardType                 = BehaviorCardType.Removable;
        gauntSlash.triggerCondition         = "The Gaunt lunges, raking its claws across the hunter.";
        gauntSlash.effectDescription        = "The Gaunt slashes the nearest hunter.";
        gauntSlash.hasTargetIdentification  = true;
        gauntSlash.hasMovement              = false;
        gauntSlash.hasDamage                = true;
        gauntSlash.targetRule               = "nearest";
        gauntSlash.forcedHunterBodyPart     = "";
        gauntSlash.criticalWoundCondition   = "GauntJaw_Critical";
        gauntSlash.alternateTriggerCondition  = "Draws back, jaw hanging";
        gauntSlash.alternateEffectDescription = "The Gaunt recoils from its wounded jaw — cries out, no damage this turn";
        EditorUtility.SetDirty(gauntSlash);

        // 3. Bone Rattle
        var boneRattle = CreateOrLoad<BehaviorCardSO>(
            "Assets/_Game/Data/Monsters/Gaunt/BehaviorCards/Base/Gaunt_BoneRattle.asset");
        boneRattle.cardName                = "Bone Rattle";
        boneRattle.cardType                = BehaviorCardType.Mood;
        boneRattle.triggerCondition        = "The Gaunt rattles its hollow bones — an eerie, rhythmic clatter.";
        boneRattle.effectDescription       = "Hunters suffer -1 Accuracy while this card is in play.";
        boneRattle.hasTargetIdentification = false;
        boneRattle.hasMovement             = false;
        boneRattle.hasDamage               = false;
        boneRattle.removalCondition        = "Hunter inflicts a wound";
        EditorUtility.SetDirty(boneRattle);

        // 4. Brace
        var brace = CreateOrLoad<BehaviorCardSO>(
            "Assets/_Game/Data/Monsters/Gaunt/BehaviorCards/Base/Gaunt_Brace.asset");
        brace.cardName                = "Brace";
        brace.cardType                = BehaviorCardType.Removable;
        brace.triggerCondition        = "The Gaunt draws its limbs inward, absorbing the next strike.";
        brace.effectDescription       = "Reaction — the Gaunt negates the next wound it would suffer this round.";
        brace.hasTargetIdentification = false;
        brace.hasMovement             = false;
        brace.hasDamage               = false;
        EditorUtility.SetDirty(brace);

        // 5. Scrabble Surge
        var scrabbleSurge = CreateOrLoad<BehaviorCardSO>(
            "Assets/_Game/Data/Monsters/Gaunt/BehaviorCards/Base/Gaunt_ScrabbleSurge.asset");
        scrabbleSurge.cardName                = "Scrabble Surge";
        scrabbleSurge.cardType                = BehaviorCardType.Removable;
        scrabbleSurge.triggerCondition        = "The Gaunt skitters forward in a burst of frantic motion.";
        scrabbleSurge.effectDescription       = "The Gaunt moves then strikes the nearest hunter.";
        scrabbleSurge.hasTargetIdentification = true;
        scrabbleSurge.hasMovement             = true;
        scrabbleSurge.hasDamage               = true;
        scrabbleSurge.targetRule              = "nearest";
        scrabbleSurge.forcedHunterBodyPart    = "";
        EditorUtility.SetDirty(scrabbleSurge);

        // 6. Scent Lock
        var scentLock = CreateOrLoad<BehaviorCardSO>(
            "Assets/_Game/Data/Monsters/Gaunt/BehaviorCards/Base/Gaunt_ScentLock.asset");
        scentLock.cardName                = "Scent Lock";
        scentLock.cardType                = BehaviorCardType.Removable;
        scentLock.triggerCondition        = "The Gaunt inhales deeply, locking on to a hunter's scent.";
        scentLock.effectDescription       = "The Gaunt fixes on a target — Aggro token transfers to the nearest hunter.";
        scentLock.hasTargetIdentification = true;
        scentLock.hasMovement             = false;
        scentLock.hasDamage               = false;
        scentLock.targetRule              = "";
        EditorUtility.SetDirty(scentLock);

        // ═══════════════════════════════════════════════════════════════
        // ADVANCED CARD POOL — 2 cards
        // ═══════════════════════════════════════════════════════════════

        // 7. Spear Thrust
        var spearThrust = CreateOrLoad<BehaviorCardSO>(
            "Assets/_Game/Data/Monsters/Gaunt/BehaviorCards/Advanced/Gaunt_SpearThrust.asset");
        spearThrust.cardName                = "Spear Thrust";
        spearThrust.cardType                = BehaviorCardType.SingleTrigger;
        spearThrust.triggerCondition        = "The Gaunt rears back and drives a bone-shard lance into a hunter's torso.";
        spearThrust.effectDescription       = "Deals 1 damage to the Torso of the nearest hunter. Fires once, then is removed.";
        spearThrust.hasTargetIdentification = true;
        spearThrust.hasMovement             = false;
        spearThrust.hasDamage               = true;
        spearThrust.targetRule              = "nearest";
        spearThrust.forcedHunterBodyPart    = "Torso";
        EditorUtility.SetDirty(spearThrust);

        // 8. Bone Lance
        var boneLance = CreateOrLoad<BehaviorCardSO>(
            "Assets/_Game/Data/Monsters/Gaunt/BehaviorCards/Advanced/Gaunt_BoneLance.asset");
        boneLance.cardName                = "Bone Lance";
        boneLance.cardType                = BehaviorCardType.Removable;
        boneLance.triggerCondition        = "The Gaunt charges the most weakened hunter, lance lowered.";
        boneLance.effectDescription       = "The Gaunt moves toward and strikes the most injured hunter.";
        boneLance.hasTargetIdentification = true;
        boneLance.hasMovement             = true;
        boneLance.hasDamage               = true;
        boneLance.targetRule              = "mostInjured";
        boneLance.forcedHunterBodyPart    = "";
        EditorUtility.SetDirty(boneLance);

        // ═══════════════════════════════════════════════════════════════
        // STANDARD WOUND LOCATION DECK — 6 cards
        // ═══════════════════════════════════════════════════════════════

        // 1. Gaunt Jaw
        var gauntJaw = CreateOrLoad<WoundLocationSO>(
            "Assets/_Game/Data/Monsters/Gaunt/WoundLocations/Standard/Gaunt_GauntJaw.asset");
        gauntJaw.locationName   = "Gaunt Jaw";
        gauntJaw.partTag        = BodyPartTag.Head;
        gauntJaw.woundTarget    = 6;
        gauntJaw.isTrap         = false;
        gauntJaw.isImpervious   = false;
        gauntJaw.woundEffect    = "The Gaunt's jaw is cracked — it recoils, fighting through the pain.";
        gauntJaw.criticalEffect = "The Gaunt's jaw shatters. Its biting attacks are permanently altered.";
        gauntJaw.criticalWoundTag = "GauntJaw_Critical";
        EditorUtility.SetDirty(gauntJaw);

        // 2. Gaunt Claw
        var gauntClaw = CreateOrLoad<WoundLocationSO>(
            "Assets/_Game/Data/Monsters/Gaunt/WoundLocations/Standard/Gaunt_GauntClaw.asset");
        gauntClaw.locationName  = "Gaunt Claw";
        gauntClaw.partTag       = BodyPartTag.Arms;
        gauntClaw.woundTarget   = 5;
        gauntClaw.isTrap        = false;
        gauntClaw.isImpervious  = false;
        gauntClaw.woundEffect   = "The claw is scored deeply — the Gaunt favors that limb.";
        gauntClaw.failureEffect = "The strike glances off the cartilaginous claw — no purchase.";
        EditorUtility.SetDirty(gauntClaw);

        // 3. Spiked Tail
        var spikedTail = CreateOrLoad<WoundLocationSO>(
            "Assets/_Game/Data/Monsters/Gaunt/WoundLocations/Standard/Gaunt_SpikedTail.asset");
        spikedTail.locationName  = "Spiked Tail";
        spikedTail.partTag       = BodyPartTag.Tail;
        spikedTail.woundTarget   = 7;
        spikedTail.isTrap        = false;
        spikedTail.isImpervious  = false;
        spikedTail.woundEffect   = "The tail is gouged — the Gaunt stumbles on its next step.";
        spikedTail.failureEffect = "The tail's spines deflect the blow. The hunter's weapon vibrates.";
        EditorUtility.SetDirty(spikedTail);

        // 4. Bony Shoulder — Impervious
        var bonyShoulder = CreateOrLoad<WoundLocationSO>(
            "Assets/_Game/Data/Monsters/Gaunt/WoundLocations/Standard/Gaunt_BonyShoulder.asset");
        bonyShoulder.locationName  = "Bony Shoulder";
        bonyShoulder.partTag       = BodyPartTag.Torso;
        bonyShoulder.woundTarget   = 5;
        bonyShoulder.isTrap        = false;
        bonyShoulder.isImpervious  = true;
        bonyShoulder.woundEffect   = "Cracked but not broken — the shoulder resists.";
        bonyShoulder.criticalEffect = "The bone plate fractures under the blow — a fissure spreads.";
        EditorUtility.SetDirty(bonyShoulder);

        // 5. Spine Trap — Trap
        var spineTrap = CreateOrLoad<WoundLocationSO>(
            "Assets/_Game/Data/Monsters/Gaunt/WoundLocations/Standard/Gaunt_SpineTrap.asset");
        spineTrap.locationName = "Spine Trap";
        spineTrap.partTag      = BodyPartTag.Back;
        spineTrap.woundTarget  = 0;  // irrelevant for traps
        spineTrap.isTrap       = true;
        spineTrap.trapEffect   = "The Gaunt's spine barb catches the hunter — take 1 flesh damage before the attack resolves";
        EditorUtility.SetDirty(spineTrap);

        // 6. Rib Cage
        var ribCage = CreateOrLoad<WoundLocationSO>(
            "Assets/_Game/Data/Monsters/Gaunt/WoundLocations/Standard/Gaunt_RibCage.asset");
        ribCage.locationName  = "Rib Cage";
        ribCage.partTag       = BodyPartTag.Torso;
        ribCage.woundTarget   = 4;
        ribCage.isTrap        = false;
        ribCage.isImpervious  = false;
        ribCage.woundEffect   = "A rib splinters inward — the Gaunt shudders.";
        ribCage.failureEffect = "The ribcage absorbs the strike with a hollow thud.";
        EditorUtility.SetDirty(ribCage);

        AssetDatabase.SaveAssets();

        // ═══════════════════════════════════════════════════════════════
        // MONSTER SO — Gaunt Standard
        // ═══════════════════════════════════════════════════════════════

        var standardPath = "Assets/_Game/Data/Monsters/Gaunt/MonsterSO/Monster_GauntStandard.asset";
        var gauntSO = AssetDatabase.LoadAssetAtPath<MonsterSO>(standardPath);
        if (gauntSO == null)
        {
            gauntSO = ScriptableObject.CreateInstance<MonsterSO>();
            AssetDatabase.CreateAsset(gauntSO, standardPath);
        }

        gauntSO.monsterName              = "The Gaunt";
        gauntSO.materialTier             = 1;
        gauntSO.animalBasis              = "Carrion predator — hollow-boned, skull-faced, ambush hunter";
        gauntSO.combatEmotion            = "Methodical patience giving way to frenzied hunger";
        gauntSO.coreSkillTaught          = "Reading the monster's deck — anticipating its next move";
        gauntSO.gridFootprintStandard    = new Vector2Int(2, 2);
        gauntSO.gridFootprintHardened    = new Vector2Int(2, 2);
        gauntSO.gridFootprintApex        = new Vector2Int(3, 2);

        // Stat blocks (placeholder — scaled in Stage 9)
        gauntSO.statBlocks = new MonsterStatBlock[]
        {
            new MonsterStatBlock { movement = 4, accuracy = 5, strength = 2, toughness = 5, evasion = 3 },
            new MonsterStatBlock { movement = 5, accuracy = 6, strength = 3, toughness = 6, evasion = 4 },
            new MonsterStatBlock { movement = 6, accuracy = 7, strength = 4, toughness = 7, evasion = 5 },
        };

        // Behavior card pools
        gauntSO.baseCardPool = new BehaviorCardSO[]
        {
            creepingAdvance, gauntSlash, boneRattle, brace, scrabbleSurge, scentLock
        };
        gauntSO.advancedCardPool = new BehaviorCardSO[]
        {
            spearThrust, boneLance
        };
        gauntSO.overwhelmingCardPool = new BehaviorCardSO[0];

        // Deck compositions — index 0=Standard, 1=Hardened, 2=Apex
        gauntSO.deckCompositions = new BehaviorDeckComposition[]
        {
            new BehaviorDeckComposition { baseCardCount = 4, advancedCardCount = 1, overwhelmingCardCount = 0 },
            new BehaviorDeckComposition { baseCardCount = 5, advancedCardCount = 2, overwhelmingCardCount = 0 },
            new BehaviorDeckComposition { baseCardCount = 6, advancedCardCount = 2, overwhelmingCardCount = 0 },
        };

        // Wound decks
        var standardWoundDeck = new WoundLocationSO[]
        {
            gauntJaw, gauntClaw, spikedTail, bonyShoulder, spineTrap, ribCage
        };
        gauntSO.standardWoundDeck = standardWoundDeck;
        gauntSO.hardenedWoundDeck = standardWoundDeck;  // Extended in Stage 9
        gauntSO.apexWoundDeck     = standardWoundDeck;  // Extended in Stage 9

        // Facing accuracy bonuses
        gauntSO.facingBonuses = new FacingAccuracyBonus[]
        {
            new FacingAccuracyBonus { arc = FacingArc.Front, accuracyModifier = 0  },
            new FacingAccuracyBonus { arc = FacingArc.Flank, accuracyModifier = 1  },
            new FacingAccuracyBonus { arc = FacingArc.Rear,  accuracyModifier = 2  },
        };

        EditorUtility.SetDirty(gauntSO);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[8N] ✓ Gaunt Standard SO assets created. Summary:");
        Debug.Log($"  Base pool: {gauntSO.baseCardPool.Length} cards");
        Debug.Log($"  Advanced pool: {gauntSO.advancedCardPool.Length} cards");
        Debug.Log($"  Deck compositions: {gauntSO.deckCompositions.Length} (Standard={gauntSO.deckCompositions[0].baseCardCount}+{gauntSO.deckCompositions[0].advancedCardCount} Hardened={gauntSO.deckCompositions[1].baseCardCount}+{gauntSO.deckCompositions[1].advancedCardCount})");
        Debug.Log($"  Standard wound deck: {gauntSO.standardWoundDeck.Length} locations");
        Debug.Log($"  Asset path: {standardPath}");
    }
}
