using UnityEditor;
using UnityEngine;
using MnM.Core.Data;

/// <summary>
/// Stage 7-H — batch-creates all 36 ActionCardSO assets for Fist Weapons and Spear
/// (Tier 1–5, 18 cards each) and updates/creates the WeaponSO for each weapon type.
/// Menu: MnM/Dev/Create Weapon Cards — Fist & Spear (7-H)
/// </summary>
public class CreateWeaponCards_7H
{
    const string FistPath  = "Assets/_Game/Data/Cards/Action/FistWeapon";
    const string SpearPath = "Assets/_Game/Data/Cards/Action/Spear";
    const string WeaponPath = "Assets/_Game/Data/Weapons";

    [MenuItem("MnM/Dev/Create Weapon Cards — Fist & Spear (7-H)")]
    public static void Execute()
    {
        // ── Ensure folders exist ──────────────────────────────────────────────
        EnsureFolder("Assets/_Game/Data/Cards/Action", "FistWeapon");
        EnsureFolder("Assets/_Game/Data/Cards/Action", "Spear");
        EnsureFolder("Assets/_Game/Data", "Weapons");

        // ── Fist Weapon cards ─────────────────────────────────────────────────
        // Tier 1 — 4 cards (starting cards — all hunters begin here)
        var fist_t1_brace = Card(FistPath, "Fist_T1_Brace",
            "Brace", WeaponType.FistWeapon, CardCategory.Reaction, apCost: 0, apRefund: 0,
            isReaction: true, tier: 1,
            effect: "When you take damage, reduce that damage by 2 Shell or 1 Flesh. Declare before damage resolves.");

        var fist_t1_shove = Card(FistPath, "Fist_T1_Shove",
            "Shove", WeaponType.FistWeapon, CardCategory.BasicAttack, apCost: 1, apRefund: 0,
            isReaction: false, tier: 1,
            effect: "No weapon damage. Push monster 1 square back. Apply Shaken.");

        var fist_t1_quickJab = Card(FistPath, "Fist_T1_QuickJab",
            "Quick Jab", WeaponType.FistWeapon, CardCategory.BasicAttack, apCost: 1, apRefund: 1,
            isReaction: false, tier: 1,
            effect: "Standard attack at -1 Strength. On hit: apply Shaken. Costs 0 net AP.");

        var fist_t1_strikeAndMove = Card(FistPath, "Fist_T1_StrikeAndMove",
            "Strike and Move", WeaponType.FistWeapon, CardCategory.Signature, apCost: 1, apRefund: 0,
            isReaction: false, tier: 1,
            effect: "Make a standard attack AND move up to 2 squares in any order. Both happen in one action.");

        // Tier 2 — 4 cards
        var fist_t2_grappleOpener = Card(FistPath, "Fist_T2_GrappleOpener",
            "Grapple Opener", WeaponType.FistWeapon, CardCategory.Opener, apCost: 1, apRefund: 1,
            isReaction: false, tier: 2,
            effect: "No damage. Apply Pinned to target. Starts combo. Costs 0 net AP.");

        var fist_t2_hammerFist = Card(FistPath, "Fist_T2_HammerFist",
            "Hammer Fist", WeaponType.FistWeapon, CardCategory.Opener, apCost: 1, apRefund: 0,
            isReaction: false, tier: 2,
            effect: "Attack at +1 Strength. Starts combo.");

        var fist_t2_deflect = Card(FistPath, "Fist_T2_Deflect",
            "Deflect", WeaponType.FistWeapon, CardCategory.Reaction, apCost: 0, apRefund: 0,
            isReaction: true, tier: 2,
            effect: "When targeted by a melee attack, reduce all damage by 1 Shell AND 1 Flesh this turn.");

        var fist_t2_bodyBlow = Card(FistPath, "Fist_T2_BodyBlow",
            "Body Blow", WeaponType.FistWeapon, CardCategory.BasicAttack, apCost: 1, apRefund: 0,
            isReaction: false, tier: 2,
            effect: "Attack. On wound: apply Slowed.");

        // Tier 3 — 4 cards
        var fist_t3_followThrough = Card(FistPath, "Fist_T3_FollowThrough",
            "Follow Through", WeaponType.FistWeapon, CardCategory.Linker, apCost: 1, apRefund: 0,
            isReaction: false, tier: 3,
            effect: "Attack at +1 Strength. On hit: move 1 square free. Continues combo.");

        var fist_t3_staggeringBlow = Card(FistPath, "Fist_T3_StaggeringBlow",
            "Staggering Blow", WeaponType.FistWeapon, CardCategory.Linker, apCost: 1, apRefund: 1,
            isReaction: false, tier: 3,
            effect: "No damage. Apply Shaken AND Slowed. Continues combo. Costs 0 net AP.");

        var fist_t3_exposedStrike = Card(FistPath, "Fist_T3_ExposedStrike",
            "Exposed Strike", WeaponType.FistWeapon, CardCategory.BasicAttack, apCost: 1, apRefund: 0,
            isReaction: false, tier: 3,
            effect: "Attack. If target part Shell is 0, apply Exposed.");

        var fist_t3_counterstrike = Card(FistPath, "Fist_T3_Counterstrike",
            "Counterstrike", WeaponType.FistWeapon, CardCategory.Reaction, apCost: 0, apRefund: 0,
            isReaction: true, tier: 3,
            effect: "When monster attacks you and misses, immediately make a free standard attack.");

        // Tier 4 — 3 cards
        var fist_t4_crushingGrip = Card(FistPath, "Fist_T4_CrushingGrip",
            "Crushing Grip", WeaponType.FistWeapon, CardCategory.Linker, apCost: 1, apRefund: 0,
            isReaction: false, tier: 4,
            effect: "Attack. If target is Pinned, auto-pass Force Check AND apply Exposed. Continues combo.");

        var fist_t4_throwingArm = Card(FistPath, "Fist_T4_ThrowingArm",
            "Throwing Arm", WeaponType.FistWeapon, CardCategory.Reaction, apCost: 0, apRefund: 0,
            isReaction: true, tier: 4,
            effect: "When monster moves adjacent, push it 2 squares back as a free reaction.");

        var fist_t4_precisionBlow = Card(FistPath, "Fist_T4_PrecisionBlow",
            "Precision Blow", WeaponType.FistWeapon, CardCategory.BasicAttack, apCost: 1, apRefund: 0,
            isReaction: false, tier: 4,
            effect: "Attack. Crit threshold reduced by 1 this attack only.");

        // Tier 5 — 3 cards
        var fist_t5_finalStrike = Card(FistPath, "Fist_T5_FinalStrike",
            "Final Strike", WeaponType.FistWeapon, CardCategory.Finisher, apCost: 1, apRefund: 0,
            isReaction: false, tier: 5,
            effect: "Attack at +2 Strength. On wound: apply Exposed permanently for rest of hunt. Ends combo.");

        var fist_t5_survivorInstinct = Card(FistPath, "Fist_T5_SurvivorInstinct",
            "Survivor Instinct", WeaponType.FistWeapon, CardCategory.Finisher, apCost: 1, apRefund: 1,
            isReaction: false, tier: 5,
            effect: "Gain 2 Grit. All adjacent hunters gain 1 Grit. Ends combo. Costs 0 net AP.");

        var fist_t5_breakingPoint = Card(FistPath, "Fist_T5_BreakingPoint",
            "Breaking Point", WeaponType.FistWeapon, CardCategory.BasicAttack, apCost: 1, apRefund: 0,
            isReaction: false, tier: 5,
            effect: "Attack. On Shell break: deal 2 additional Flesh damage bypassing Shell.");

        // ── Spear cards ───────────────────────────────────────────────────────
        // Tier 1 — 4 cards
        var spear_t1_longThrust = Card(SpearPath, "Spear_T1_LongThrust",
            "Long Thrust", WeaponType.Spear, CardCategory.BasicAttack, apCost: 1, apRefund: 0,
            isReaction: false, tier: 1,
            effect: "Standard attack from 2 tiles. Cannot attack adjacent targets (Spear passive).");

        var spear_t1_bracePosition = Card(SpearPath, "Spear_T1_BracePosition",
            "Brace Position", WeaponType.Spear, CardCategory.Signature, apCost: 1, apRefund: 0,
            isReaction: false, tier: 1,
            effect: "Designate 2 adjacent squares as movement-denied for monster this round. You cannot move this turn.");

        var spear_t1_jab = Card(SpearPath, "Spear_T1_Jab",
            "Jab", WeaponType.Spear, CardCategory.BasicAttack, apCost: 1, apRefund: 1,
            isReaction: false, tier: 1,
            effect: "Attack at -1 Strength from 2 tiles. On hit: apply Shaken. Costs 0 net AP.");

        var spear_t1_setSpear = Card(SpearPath, "Spear_T1_SetSpear",
            "Set Spear", WeaponType.Spear, CardCategory.BasicAttack, apCost: 1, apRefund: 0,
            isReaction: false, tier: 1,
            effect: "Attack. If monster moved toward you this round, gains +1 Strength.");

        // Tier 2 — 4 cards
        var spear_t2_reachOut = Card(SpearPath, "Spear_T2_ReachOut",
            "Reach Out", WeaponType.Spear, CardCategory.Opener, apCost: 1, apRefund: 1,
            isReaction: false, tier: 2,
            effect: "Attack from 3 tiles (Tier 2 passive). Starts combo. Costs 0 net AP.");

        var spear_t2_zoneControl = Card(SpearPath, "Spear_T2_ZoneControl",
            "Zone Control", WeaponType.Spear, CardCategory.Opener, apCost: 1, apRefund: 1,
            isReaction: false, tier: 2,
            effect: "Designate 1 square as denied. No attack. Starts combo. Costs 0 net AP.");

        var spear_t2_interceptor = Card(SpearPath, "Spear_T2_Interceptor",
            "Interceptor", WeaponType.Spear, CardCategory.Reaction, apCost: 0, apRefund: 0,
            isReaction: true, tier: 2,
            effect: "When monster moves toward any hunter, make a standard attack from 2 tiles before it arrives.");

        var spear_t2_pinningThrust = Card(SpearPath, "Spear_T2_PinningThrust",
            "Pinning Thrust", WeaponType.Spear, CardCategory.BasicAttack, apCost: 1, apRefund: 0,
            isReaction: false, tier: 2,
            effect: "Attack from 2 tiles. On wound: apply Pinned.");

        // Tier 3 — 4 cards
        var spear_t3_suppressingStrike = Card(SpearPath, "Spear_T3_SuppressingStrike",
            "Suppressing Strike", WeaponType.Spear, CardCategory.Linker, apCost: 1, apRefund: 0,
            isReaction: false, tier: 3,
            effect: "Attack from range. On hit: designate 1 square adjacent to struck part as denied. Continues combo.");

        var spear_t3_withdraw = Card(SpearPath, "Spear_T3_Withdraw",
            "Withdraw", WeaponType.Spear, CardCategory.Linker, apCost: 1, apRefund: 1,
            isReaction: false, tier: 3,
            effect: "Move 2 squares away from monster. No attack. Continues combo. Costs 0 net AP.");

        var spear_t3_overextend = Card(SpearPath, "Spear_T3_Overextend",
            "Overextend", WeaponType.Spear, CardCategory.BasicAttack, apCost: 1, apRefund: 0,
            isReaction: false, tier: 3,
            effect: "Attack from 3 tiles at +1 Strength. You cannot move next turn.");

        var spear_t3_denyGround = Card(SpearPath, "Spear_T3_DenyGround",
            "Deny Ground", WeaponType.Spear, CardCategory.BasicAttack, apCost: 1, apRefund: 0,
            isReaction: false, tier: 3,
            effect: "No attack. Designate 2 squares as movement-denied AND apply Exposed to one named part.");

        // Tier 4 — 3 cards
        var spear_t4_impale = Card(SpearPath, "Spear_T4_Impale",
            "Impale", WeaponType.Spear, CardCategory.Linker, apCost: 1, apRefund: 0,
            isReaction: false, tier: 4,
            effect: "Attack. If target Exposed, auto-pass Force Check. On wound: apply Bleeding. Continues combo.");

        var spear_t4_coverZone = Card(SpearPath, "Spear_T4_CoverZone",
            "Cover Zone", WeaponType.Spear, CardCategory.Reaction, apCost: 0, apRefund: 0,
            isReaction: true, tier: 4,
            effect: "When monster enters a movement-denied square (forced by behavior card), make a free attack.");

        var spear_t4_sweepingThrust = Card(SpearPath, "Spear_T4_SweepingThrust",
            "Sweeping Thrust", WeaponType.Spear, CardCategory.BasicAttack, apCost: 1, apRefund: 0,
            isReaction: false, tier: 4,
            effect: "Attack. Hits primary target AND one adjacent part in a straight line. Resolve Force Check for each.");

        // Tier 5 — 3 cards
        var spear_t5_lineBreaker = Card(SpearPath, "Spear_T5_LineBreaker",
            "Line Breaker", WeaponType.Spear, CardCategory.Finisher, apCost: 1, apRefund: 0,
            isReaction: false, tier: 5,
            effect: "Attack. Hits every part in a straight line from your position. Each resolves Force Check separately. Ends combo.");

        var spear_t5_deadZone = Card(SpearPath, "Spear_T5_DeadZone",
            "Dead Zone", WeaponType.Spear, CardCategory.Finisher, apCost: 1, apRefund: 1,
            isReaction: false, tier: 5,
            effect: "No attack. Designate a 3x1 line as movement-denied for the entire hunt. Ends combo. Costs 0 net AP.");

        var spear_t5_skewer = Card(SpearPath, "Spear_T5_Skewer",
            "Skewer", WeaponType.Spear, CardCategory.BasicAttack, apCost: 1, apRefund: 0,
            isReaction: false, tier: 5,
            effect: "Attack. On wound: monster cannot voluntarily move toward you for 2 rounds.");

        // ── WeaponSO: FistWeapon ──────────────────────────────────────────────
        var fistWeaponSO = CreateOrLoad<WeaponSO>($"{WeaponPath}/FistWeapon.asset");
        fistWeaponSO.weaponName     = "Fist Weapon";
        fistWeaponSO.weaponType     = WeaponType.FistWeapon;
        fistWeaponSO.elementTag     = ElementTag.None;
        fistWeaponSO.accuracyMod    = 0;
        fistWeaponSO.strengthMod    = 0;
        fistWeaponSO.attacksPerTurn = 1;
        fistWeaponSO.range          = 0;
        fistWeaponSO.isAlwaysLoud   = false;
        fistWeaponSO.signatureCard  = fist_t1_strikeAndMove;
        fistWeaponSO.tier1Cards     = new[] { fist_t1_brace, fist_t1_shove, fist_t1_quickJab };
        fistWeaponSO.tier2Cards     = new[] { fist_t2_grappleOpener, fist_t2_hammerFist, fist_t2_deflect, fist_t2_bodyBlow };
        fistWeaponSO.tier3Cards     = new[] { fist_t3_followThrough, fist_t3_staggeringBlow, fist_t3_exposedStrike, fist_t3_counterstrike };
        fistWeaponSO.tier4Cards     = new[] { fist_t4_crushingGrip, fist_t4_throwingArm, fist_t4_precisionBlow };
        fistWeaponSO.tier5Cards     = new[] { fist_t5_finalStrike, fist_t5_survivorInstinct, fist_t5_breakingPoint };
        fistWeaponSO.uniqueCapability = "Combo system: Opener → Linker → Finisher chains unlock bonus effects. No range restriction.";
        fistWeaponSO.genuineCost      = "Adjacent range only. No Shell damage on basic attacks. Relies on status stacking for pressure.";
        EditorUtility.SetDirty(fistWeaponSO);

        // ── WeaponSO: Spear ───────────────────────────────────────────────────
        var spearSO = CreateOrLoad<WeaponSO>($"{WeaponPath}/Spear.asset");
        spearSO.weaponName     = "Spear";
        spearSO.weaponType     = WeaponType.Spear;
        spearSO.elementTag     = ElementTag.None;
        spearSO.accuracyMod    = 0;
        spearSO.strengthMod    = 0;
        spearSO.attacksPerTurn = 1;
        spearSO.range          = 2;
        spearSO.isAlwaysLoud   = false;
        spearSO.signatureCard  = spear_t1_bracePosition;
        spearSO.tier1Cards     = new[] { spear_t1_longThrust, spear_t1_bracePosition, spear_t1_jab, spear_t1_setSpear };
        spearSO.tier2Cards     = new[] { spear_t2_reachOut, spear_t2_zoneControl, spear_t2_interceptor, spear_t2_pinningThrust };
        spearSO.tier3Cards     = new[] { spear_t3_suppressingStrike, spear_t3_withdraw, spear_t3_overextend, spear_t3_denyGround };
        spearSO.tier4Cards     = new[] { spear_t4_impale, spear_t4_coverZone, spear_t4_sweepingThrust };
        spearSO.tier5Cards     = new[] { spear_t5_lineBreaker, spear_t5_deadZone, spear_t5_skewer };
        spearSO.uniqueCapability = "Zone control: movement-denied squares punish monster positioning. Interceptor provides reactive threat.";
        spearSO.genuineCost      = "Cannot attack adjacent targets. Overextend and Brace Position self-impose movement restrictions.";
        EditorUtility.SetDirty(spearSO);

        // ── Save all ──────────────────────────────────────────────────────────
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[7-H] Done. Created 18 Fist Weapon + 18 Spear ActionCardSOs and 2 WeaponSOs. " +
                  "Verify: FistWeapon.asset and Spear.asset in Assets/_Game/Data/Weapons/");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static ActionCardSO Card(
        string folder, string assetName,
        string cardName, WeaponType weaponType, CardCategory category,
        int apCost, int apRefund, bool isReaction, int tier, string effect)
    {
        var path = $"{folder}/{assetName}.asset";
        var card = CreateOrLoad<ActionCardSO>(path);
        card.cardName               = cardName;
        card.weaponType             = weaponType;
        card.category               = category;
        card.apCost                 = apCost;
        card.apRefund               = apRefund;
        card.isLoud                 = false;
        card.isReaction             = isReaction;
        card.proficiencyTierRequired = tier;
        card.effectDescription      = effect;
        card.flavorText             = "";
        EditorUtility.SetDirty(card);
        return card;
    }

    private static void EnsureFolder(string parent, string folderName)
    {
        string path = $"{parent}/{folderName}";
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, folderName);
            Debug.Log($"[7-H] Created folder: {path}");
        }
    }

    private static T CreateOrLoad<T>(string path) where T : ScriptableObject
    {
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null) return existing;

        var asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }
}
