using UnityEngine;
using UnityEditor;
using MnM.Core.Data;
using System.IO;

public class CreateGreatswordBowCards
{
    public static void Execute()
    {
        CreateGreatswordCards();
        CreateBowCards();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Stage 7-J: All 36 Greatsword and Bow ActionCardSO assets created.");
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

    static ActionCardSO Make(string folder, string assetName, string cardName,
        WeaponType weapon, CardCategory category, int apCost, int apRefund,
        int tier, bool isReaction, bool isLoud, string effect)
    {
        string fullPath = $"{folder}/{assetName}.asset";
        ActionCardSO existing = AssetDatabase.LoadAssetAtPath<ActionCardSO>(fullPath);
        if (existing != null)
        {
            Debug.Log($"  Skipping (already exists): {assetName}");
            return existing;
        }
        var card = ScriptableObject.CreateInstance<ActionCardSO>();
        card.cardName = cardName;
        card.weaponType = weapon;
        card.category = category;
        card.apCost = apCost;
        card.apRefund = apRefund;
        card.proficiencyTierRequired = tier;
        card.isReaction = isReaction;
        card.isLoud = isLoud;
        card.effectDescription = effect;
        AssetDatabase.CreateAsset(card, fullPath);
        Debug.Log($"  Created: {assetName}");
        return card;
    }

    static void CreateGreatswordCards()
    {
        const string dir = "Assets/_Game/Data/Cards/Action/Greatsword";
        EnsureDir("Assets/_Game/Data/Cards/Action");
        EnsureDir(dir);
        var w = WeaponType.Greatsword;

        // Tier 1
        Make(dir, "Greatsword_T1_SweepingCut", "Sweeping Cut", w, CardCategory.BasicAttack, 1, 0, 1, false, false,
            "Attack. Hits two parts simultaneously (facing-determined). Both resolve Force Check on same roll.");
        Make(dir, "Greatsword_T1_MeasuredSwing", "Measured Swing", w, CardCategory.BasicAttack, 1, 1, 1, false, false,
            "Attack one part only at -1 Strength. Costs 0 net AP.");
        Make(dir, "Greatsword_T1_BrutalArc", "Brutal Arc", w, CardCategory.BasicAttack, 1, 0, 1, false, false,
            "Attack. Both struck parts take +1 Strength damage on arc hit.");
        Make(dir, "Greatsword_T1_WideArc", "Wide Arc", w, CardCategory.Signature, 1, 0, 1, false, false,
            "Attack primary target AND one adjacent part simultaneously. Both resolve Force Check.");

        // Tier 2
        Make(dir, "Greatsword_T2_DrivingOpener", "Driving Opener", w, CardCategory.Opener, 1, 0, 2, false, false,
            "Attack primary part. Push monster 1 square. Starts combo.");
        Make(dir, "Greatsword_T2_ClearTheField", "Clear the Field", w, CardCategory.Opener, 1, 1, 2, false, false,
            "No damage. Move 2 squares in any direction free. Starts combo. Costs 0 net AP.");
        Make(dir, "Greatsword_T2_InterruptingSlash", "Interrupting Slash", w, CardCategory.Reaction, 1, 0, 2, true, false,
            "When monster targets an adjacent hunter: make a free standard sweep attack (2 parts).");
        Make(dir, "Greatsword_T2_SweepingStrike", "Sweeping Strike", w, CardCategory.BasicAttack, 1, 0, 2, false, false,
            "Attack. Hits 3 parts in arc on crit (normal = 2).");

        // Tier 3
        Make(dir, "Greatsword_T3_MomentumLinker", "Momentum Linker", w, CardCategory.Linker, 1, 0, 3, false, false,
            "Attack 2 parts. If both parts hit, apply Slowed. Continues combo.");
        Make(dir, "Greatsword_T3_GroundControl", "Ground Control", w, CardCategory.Linker, 1, 1, 3, false, false,
            "No attack. Designate 3 squares as movement-denied. Continues combo. Costs 0 net AP.");
        Make(dir, "Greatsword_T3_ArcOfDespair", "Arc of Despair", w, CardCategory.BasicAttack, 1, 0, 3, false, false,
            "Attack. Both struck parts take +1 Flesh damage on wound.");
        Make(dir, "Greatsword_T3_PressureStrike", "Pressure Strike", w, CardCategory.BasicAttack, 1, 0, 3, false, false,
            "Attack. On Shell break on either part: apply Exposed to the other part.");

        // Tier 4
        Make(dir, "Greatsword_T4_ExecutionArc", "Execution Arc", w, CardCategory.Linker, 1, 0, 4, false, false,
            "Attack 2 parts. If either part Exposed: auto-pass Force Check on that part. Continues combo.");
        Make(dir, "Greatsword_T4_Riposte", "Riposte", w, CardCategory.Reaction, 1, 0, 4, true, false,
            "When you take damage: immediately make a free Sweeping Cut.");
        Make(dir, "Greatsword_T4_DoubleDown", "Double Down", w, CardCategory.BasicAttack, 1, 0, 4, false, false,
            "Attack same part twice. Second hit gains +1 Strength if first hit succeeded.");

        // Tier 5
        Make(dir, "Greatsword_T5_TidalStrike", "Tidal Strike", w, CardCategory.Finisher, 1, 0, 5, false, false,
            "Attack. Hits ALL parts in front arc. Each resolves Force Check separately. Ends combo.");
        Make(dir, "Greatsword_T5_LastSweep", "Last Sweep", w, CardCategory.Finisher, 1, 1, 5, false, false,
            "No attack. All hunters adjacent to monster gain 3 Grit. Ends combo. Costs 0 net AP.");
        Make(dir, "Greatsword_T5_TheEndingBlow", "The Ending Blow", w, CardCategory.BasicAttack, 1, 0, 5, false, false,
            "Attack 2 parts. On wound on either: that part becomes permanently Exposed.");
    }

    static void CreateBowCards()
    {
        const string dir = "Assets/_Game/Data/Cards/Action/Bow";
        EnsureDir(dir);
        var w = WeaponType.Bow;

        // Tier 1
        Make(dir, "Bow_T1_LoosedArrow", "Loosed Arrow", w, CardCategory.BasicAttack, 1, 0, 1, false, false,
            "Standard ranged attack from 3-6 tiles. Cannot attack adjacent targets.");
        Make(dir, "Bow_T1_NockAndDraw", "Nock and Draw", w, CardCategory.BasicAttack, 1, 1, 1, false, false,
            "Ranged attack at -1 Accuracy. On hit: apply Shaken. Costs 0 net AP.");
        Make(dir, "Bow_T1_ControlledShot", "Controlled Shot", w, CardCategory.BasicAttack, 1, 0, 1, false, false,
            "Ranged attack. Choose target part specifically (no facing table — player picks).");
        Make(dir, "Bow_T1_MarkedTarget", "Marked Target", w, CardCategory.Signature, 1, 0, 1, false, false,
            "No damage. Mark a part — all attacks against it gain +1 Accuracy this round.");

        // Tier 2
        Make(dir, "Bow_T2_PinningShot", "Pinning Shot", w, CardCategory.Opener, 1, 0, 2, false, false,
            "Ranged attack. On hit: apply Pinned. Starts combo.");
        Make(dir, "Bow_T2_RapidFire", "Rapid Fire", w, CardCategory.Opener, 1, 1, 2, false, false,
            "Make 2 ranged attacks at -1 Accuracy each. Starts combo. Costs 0 net AP.");
        Make(dir, "Bow_T2_DisengageShot", "Disengage Shot", w, CardCategory.Reaction, 1, 0, 2, true, false,
            "When monster moves adjacent: move 3 squares away free AND make a ranged attack.");
        Make(dir, "Bow_T2_ExposedFlank", "Exposed Flank", w, CardCategory.BasicAttack, 1, 0, 2, false, false,
            "Ranged attack. If attacking from Rear arc: gains +2 Accuracy bonus.");

        // Tier 3
        Make(dir, "Bow_T3_SuppressingFire", "Suppressing Fire", w, CardCategory.Linker, 1, 0, 3, false, false,
            "Ranged attack. On hit: monster cannot target you next Monster Phase. Continues combo.");
        Make(dir, "Bow_T3_TacticalRetreat", "Tactical Retreat", w, CardCategory.Linker, 1, 1, 3, false, false,
            "Move 4 squares away free. No attack. Continues combo. Costs 0 net AP.");
        Make(dir, "Bow_T3_HeadShot", "Head Shot", w, CardCategory.BasicAttack, 1, 0, 3, false, false,
            "Ranged attack. If target is Head part: crit threshold -1 this attack.");
        Make(dir, "Bow_T3_VitalShot", "Vital Shot", w, CardCategory.BasicAttack, 1, 0, 3, false, false,
            "Ranged attack. On wound: the wounded part loses 1 additional Flesh.");

        // Tier 4
        Make(dir, "Bow_T4_ArrowBarrage", "Arrow Barrage", w, CardCategory.Linker, 1, 0, 4, false, false,
            "Make 3 ranged attacks at separate parts at -1 Accuracy each. Continues combo.");
        Make(dir, "Bow_T4_CoverFire", "Cover Fire", w, CardCategory.Reaction, 1, 0, 4, true, false,
            "When any hunter targeted: make a free ranged attack against monster (may distract).");
        Make(dir, "Bow_T4_ExecutionShot", "Execution Shot", w, CardCategory.BasicAttack, 1, 0, 4, false, false,
            "Ranged attack. If target Exposed AND part Shell=0: auto-crit.");

        // Tier 5
        Make(dir, "Bow_T5_FinalVolley", "Final Volley", w, CardCategory.Finisher, 1, 0, 5, false, false,
            "Make ranged attacks against ALL parts. Each resolves separately. Ends combo.");
        Make(dir, "Bow_T5_SignalFlare", "Signal Flare", w, CardCategory.Finisher, 1, 1, 5, false, false,
            "No damage. All hunters gain 1 Grit AND next round all Reaction cards cost 0 AP. Ends combo. Costs 0 net AP.");
        Make(dir, "Bow_T5_DeadEye", "Dead Eye", w, CardCategory.BasicAttack, 1, 0, 5, false, false,
            "Ranged attack. Ignore all accuracy penalties this attack (Shaken, arc, etc.). On crit: apply Exposed.");
    }
}
