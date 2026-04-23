using UnityEngine;
using UnityEditor;
using MnM.Core.Data;
using System.IO;

public class WireWeaponSOs
{
    const string WeaponDir = "Assets/_Game/Data/Weapons";

    public static void Execute()
    {
        EnsureDir(WeaponDir);

        WireFistWeapon();
        WireSpear();
        WireAxe();
        WireHammerMaul();
        WireDagger();
        WireSwordAndShield();
        WireGreatsword();
        WireBow();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Stage 7-J: All 8 WeaponSO assets wired successfully.");
    }

    static void EnsureDir(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string folder = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }

    static WeaponSO GetOrCreateWeapon(string assetName, WeaponType type,
        string displayName, int range, bool isAlwaysLoud, int attacksPerTurn = 1)
    {
        string path = $"{WeaponDir}/{assetName}.asset";
        var so = AssetDatabase.LoadAssetAtPath<WeaponSO>(path);
        if (so == null)
        {
            so = ScriptableObject.CreateInstance<WeaponSO>();
            AssetDatabase.CreateAsset(so, path);
            Debug.Log($"  Created WeaponSO: {assetName}");
        }
        else
        {
            Debug.Log($"  Updating WeaponSO: {assetName}");
        }
        so.weaponName = displayName;
        so.weaponType = type;
        so.range = range;
        so.isAlwaysLoud = isAlwaysLoud;
        so.attacksPerTurn = attacksPerTurn;
        EditorUtility.SetDirty(so);
        return so;
    }

    static ActionCardSO Card(string path)
    {
        var c = AssetDatabase.LoadAssetAtPath<ActionCardSO>(path);
        if (c == null) Debug.LogWarning($"  MISSING card: {path}");
        return c;
    }

    // ── FistWeapon ──────────────────────────────────────────────────
    static void WireFistWeapon()
    {
        const string d = "Assets/_Game/Data/Cards/Action/FistWeapon";
        var w = GetOrCreateWeapon("FistWeapon", WeaponType.FistWeapon, "Fist Weapon", 0, false);
        w.signatureCard   = Card($"{d}/Fist_T1_StrikeAndMove.asset");
        w.tier1Cards      = Cards(d, "Fist_T1_Brace", "Fist_T1_Shove", "Fist_T1_QuickJab");
        w.tier2Cards      = Cards(d, "Fist_T2_GrappleOpener", "Fist_T2_HammerFist", "Fist_T2_Deflect", "Fist_T2_BodyBlow");
        w.tier3Cards      = Cards(d, "Fist_T3_FollowThrough", "Fist_T3_StaggeringBlow", "Fist_T3_ExposedStrike", "Fist_T3_Counterstrike");
        w.tier4Cards      = Cards(d, "Fist_T4_CrushingGrip", "Fist_T4_ThrowingArm", "Fist_T4_PrecisionBlow");
        w.tier5Cards      = Cards(d, "Fist_T5_FinalStrike", "Fist_T5_SurvivorInstinct", "Fist_T5_BreakingPoint");
        w.uniqueCapability = "Shield absorbs 1 Shell hit per round free.";
        EditorUtility.SetDirty(w);
    }

    // ── Spear ────────────────────────────────────────────────────────
    static void WireSpear()
    {
        const string d = "Assets/_Game/Data/Cards/Action/Spear";
        var w = GetOrCreateWeapon("Spear", WeaponType.Spear, "Spear", 2, false);
        w.signatureCard   = Card($"{d}/Spear_T1_BracePosition.asset");
        w.tier1Cards      = Cards(d, "Spear_T1_LongThrust", "Spear_T1_Jab", "Spear_T1_SetSpear");
        w.tier2Cards      = Cards(d, "Spear_T2_ReachOut", "Spear_T2_ZoneControl", "Spear_T2_Interceptor", "Spear_T2_PinningThrust");
        w.tier3Cards      = Cards(d, "Spear_T3_SuppressingStrike", "Spear_T3_Withdraw", "Spear_T3_Overextend", "Spear_T3_DenyGround");
        w.tier4Cards      = Cards(d, "Spear_T4_Impale", "Spear_T4_CoverZone", "Spear_T4_SweepingThrust");
        w.tier5Cards      = Cards(d, "Spear_T5_LineBreaker", "Spear_T5_DeadZone", "Spear_T5_Skewer");
        w.uniqueCapability = "Range 2. Cannot attack adjacent targets.";
        EditorUtility.SetDirty(w);
    }

    // ── Axe ──────────────────────────────────────────────────────────
    static void WireAxe()
    {
        const string d = "Assets/_Game/Data/Cards/Action/Axe";
        var w = GetOrCreateWeapon("Axe", WeaponType.Axe, "Axe", 0, false);
        w.signatureCard   = Card($"{d}/Axe_T1_ChopAndBeat.asset");
        w.tier1Cards      = Cards(d, "Axe_T1_ShieldSplitter", "Axe_T1_Cleave", "Axe_T1_HeavySwing");
        w.tier2Cards      = Cards(d, "Axe_T2_ArmbreakOpener", "Axe_T2_WeighIn", "Axe_T2_ReactiveChop", "Axe_T2_CrackTheShell");
        w.tier3Cards      = Cards(d, "Axe_T3_Splinter", "Axe_T3_GrindDown", "Axe_T3_ShatteringBlow", "Axe_T3_RelentlessChop");
        w.tier4Cards      = Cards(d, "Axe_T4_ExposingSplit", "Axe_T4_Reverb", "Axe_T4_ArmourBane");
        w.tier5Cards      = Cards(d, "Axe_T5_Demolish", "Axe_T5_LastRites", "Axe_T5_OverkillBlow");
        w.uniqueCapability = "Shell hits count as 2 Shell damage.";
        EditorUtility.SetDirty(w);
    }

    // ── HammerMaul ───────────────────────────────────────────────────
    static void WireHammerMaul()
    {
        const string d = "Assets/_Game/Data/Cards/Action/HammerMaul";
        var w = GetOrCreateWeapon("HammerMaul", WeaponType.HammerMaul, "Hammer/Maul", 0, true);
        w.signatureCard   = null; // No Signature category card — all T1s in deck
        w.tier1Cards      = Cards(d, "Hammer_T1_ThunderingBlow", "Hammer_T1_WindUp", "Hammer_T1_Shove", "Hammer_T1_Brace");
        w.tier2Cards      = Cards(d, "Hammer_T2_GroundSlam", "Hammer_T2_Momentum", "Hammer_T2_SmashThrough", "Hammer_T2_Stagger");
        w.tier3Cards      = Cards(d, "Hammer_T3_SeeingRed", "Hammer_T3_BuildingForce", "Hammer_T3_Aftershock", "Hammer_T3_EarthShaker");
        w.tier4Cards      = Cards(d, "Hammer_T4_PileDriver", "Hammer_T4_BraceForImpact", "Hammer_T4_BoneShatter");
        w.tier5Cards      = Cards(d, "Hammer_T5_Annihilate", "Hammer_T5_RallyingCrash", "Hammer_T5_Devastation");
        w.uniqueCapability = "Every attack is Loud. On Shell hit: deal 1 Flesh bypass. -1 Movement while equipped.";
        EditorUtility.SetDirty(w);
    }

    // ── Dagger ───────────────────────────────────────────────────────
    static void WireDagger()
    {
        const string d = "Assets/_Game/Data/Cards/Action/Dagger";
        var w = GetOrCreateWeapon("Dagger", WeaponType.Dagger, "Dagger", 0, false);
        w.signatureCard   = Card($"{d}/Dagger_T1_ShadowStep.asset");
        // QuickSlash is intentionally duplicated in the starting deck
        var quickSlash = Card($"{d}/Dagger_T1_QuickSlash.asset");
        w.tier1Cards      = new ActionCardSO[] { quickSlash, quickSlash, Card($"{d}/Dagger_T1_GlancingCut.asset") };
        w.tier2Cards      = Cards(d, "Dagger_T2_FirstBlood", "Dagger_T2_Feint", "Dagger_T2_Vanish", "Dagger_T2_FindTheGap");
        w.tier3Cards      = Cards(d, "Dagger_T3_TwistTheBlade", "Dagger_T3_BleedShadow", "Dagger_T3_DoubleStrike", "Dagger_T3_SlipBehind");
        w.tier4Cards      = Cards(d, "Dagger_T4_Rupture", "Dagger_T4_GhostStep", "Dagger_T4_Precision");
        w.tier5Cards      = Cards(d, "Dagger_T5_KillingEdge", "Dagger_T5_DeathMark", "Dagger_T5_ShadowFlurry");
        w.uniqueCapability = "Full Accuracy bonus from Rear arc. Crit threshold permanently -1. -2 Accuracy from Front arc.";
        EditorUtility.SetDirty(w);
    }

    // ── Sword & Shield ───────────────────────────────────────────────
    static void WireSwordAndShield()
    {
        const string d = "Assets/_Game/Data/Cards/Action/SwordAndShield";
        var w = GetOrCreateWeapon("SwordAndShield", WeaponType.SwordAndShield, "Sword & Shield", 0, false);
        w.signatureCard   = Card($"{d}/Sword_T1_ShieldBlock.asset");
        w.tier1Cards      = Cards(d, "Sword_T1_GuardedStrike", "Sword_T1_ShieldBash", "Sword_T1_Parry");
        w.tier2Cards      = Cards(d, "Sword_T2_RallyingDefense", "Sword_T2_ControlledStrike", "Sword_T2_CounterThrust", "Sword_T2_PressForward");
        w.tier3Cards      = Cards(d, "Sword_T3_FormationStrike", "Sword_T3_HoldTheLine", "Sword_T3_ShieldWall", "Sword_T3_CoverAlly");
        w.tier4Cards      = Cards(d, "Sword_T4_BreachingStrike", "Sword_T4_IronWill", "Sword_T4_Advance");
        w.tier5Cards      = Cards(d, "Sword_T5_JusticeStrike", "Sword_T5_LastStand", "Sword_T5_ExecuteOrder");
        w.uniqueCapability = "Only 1 attack per turn. Shield absorbs 1 Shell hit per round free. No Strength modifier.";
        EditorUtility.SetDirty(w);
    }

    // ── Greatsword ───────────────────────────────────────────────────
    static void WireGreatsword()
    {
        const string d = "Assets/_Game/Data/Cards/Action/Greatsword";
        var w = GetOrCreateWeapon("Greatsword", WeaponType.Greatsword, "Greatsword", 0, false);
        w.signatureCard   = Card($"{d}/Greatsword_T1_WideArc.asset");
        w.tier1Cards      = Cards(d, "Greatsword_T1_SweepingCut", "Greatsword_T1_MeasuredSwing", "Greatsword_T1_BrutalArc");
        w.tier2Cards      = Cards(d, "Greatsword_T2_DrivingOpener", "Greatsword_T2_ClearTheField", "Greatsword_T2_InterruptingSlash", "Greatsword_T2_SweepingStrike");
        w.tier3Cards      = Cards(d, "Greatsword_T3_MomentumLinker", "Greatsword_T3_GroundControl", "Greatsword_T3_ArcOfDespair", "Greatsword_T3_PressureStrike");
        w.tier4Cards      = Cards(d, "Greatsword_T4_ExecutionArc", "Greatsword_T4_Riposte", "Greatsword_T4_DoubleDown");
        w.tier5Cards      = Cards(d, "Greatsword_T5_TidalStrike", "Greatsword_T5_LastSweep", "Greatsword_T5_TheEndingBlow");
        w.uniqueCapability = "Hits TWO parts simultaneously (facing-determined). Both resolve Force Check on same roll.";
        EditorUtility.SetDirty(w);
    }

    // ── Bow ──────────────────────────────────────────────────────────
    static void WireBow()
    {
        const string d = "Assets/_Game/Data/Cards/Action/Bow";
        var w = GetOrCreateWeapon("Bow", WeaponType.Bow, "Bow", 3, false);
        w.signatureCard   = Card($"{d}/Bow_T1_MarkedTarget.asset");
        w.tier1Cards      = Cards(d, "Bow_T1_LoosedArrow", "Bow_T1_NockAndDraw", "Bow_T1_ControlledShot");
        w.tier2Cards      = Cards(d, "Bow_T2_PinningShot", "Bow_T2_RapidFire", "Bow_T2_DisengageShot", "Bow_T2_ExposedFlank");
        w.tier3Cards      = Cards(d, "Bow_T3_SuppressingFire", "Bow_T3_TacticalRetreat", "Bow_T3_HeadShot", "Bow_T3_VitalShot");
        w.tier4Cards      = Cards(d, "Bow_T4_ArrowBarrage", "Bow_T4_CoverFire", "Bow_T4_ExecutionShot");
        w.tier5Cards      = Cards(d, "Bow_T5_FinalVolley", "Bow_T5_SignalFlare", "Bow_T5_DeadEye");
        w.uniqueCapability = "Range 3-6 tiles. Cannot attack adjacent. Proximity behavior cards never trigger from this hunter.";
        EditorUtility.SetDirty(w);
    }

    // ── Helpers ──────────────────────────────────────────────────────
    static ActionCardSO[] Cards(string dir, params string[] names)
    {
        var result = new ActionCardSO[names.Length];
        for (int i = 0; i < names.Length; i++)
            result[i] = Card($"{dir}/{names[i]}.asset");
        return result;
    }
}
