using UnityEditor;
using UnityEngine;
using MnM.Core.Data;

public class CreateInnovationsAndGPs_7M
{
    [MenuItem("MnM/Dev/Create Innovations & GPs (7M)")]
    public static void Execute()
    {
        EnsureFolder("Assets/_Game/Data", "Innovations");
        EnsureFolder("Assets/_Game/Data", "GuidingPrincipals");
        EnsureFolder("Assets/_Game/Data", "Campaigns");

        // ── Pass 1: Create all 20 InnovationSOs without cascade refs ──
        var inn01 = MakeInnovation("INN-01", "Desperate Sprint",
            "Spend 1 Grit: move 3 additional squares per turn.", "Surge");
        var inn02 = MakeInnovation("INN-02", "Measured Hand",
            "Spend 1 Grit: re-roll one die on any check.", "Steady");
        var inn03 = MakeInnovation("INN-03", "Shoulder to Shoulder",
            "Spend 2 Grit: adjacent hunter draws 1 card out of turn.", "Rally");
        var inn04 = MakeInnovation("INN-04", "Iron Will",
            "Spend 2 Grit: ignore one drawn Injury Card effect.", "Endure");
        var inn05 = MakeInnovation("INN-05", "Bone Reading",
            "After each successful hunt, examine 2 loot table results and choose 1 to keep.", "");
        var inn06 = MakeInnovation("INN-06", "Keen Eye",
            "Trap Zones are revealed at combat start — no longer hidden.", "");
        var inn07 = MakeInnovation("INN-07", "Pursuit Tactics",
            "Hunters moving with Surge do not trigger facing behaviors that round.", "");
        var inn08 = MakeInnovation("INN-08", "Calculated Risk",
            "When spending Grit on Steady, re-roll both dice instead of one.", "");
        var inn09 = MakeInnovation("INN-09", "Unified Front",
            "Rally costs 1 Grit instead of 2 when recipient has Aggro token.", "");
        var inn10 = MakeInnovation("INN-10", "Scarred but Standing",
            "Endure keeps the ignored Injury Card out of deck permanently.", "");
        var inn11 = MakeInnovation("INN-11", "Survival Instinct",
            "Hunters gain 1 Grit whenever they survive a hit that would have caused Flesh damage.", "");
        var inn12 = MakeInnovation("INN-12", "The Patient Hunter",
            "Once per hunt, a hunter may hold 1 card from hand to next turn instead of discarding.", "");
        var inn13 = MakeInnovation("INN-13", "Blood Pact",
            "When any hunter collapses, all adjacent hunters gain 1 Grit.", "");
        var inn14 = MakeInnovation("INN-14", "Scar Tissue",
            "Injury Cards no longer trigger immediately — hunter may choose to discard them once per draw.", "");
        var inn15 = MakeInnovation("INN-15", "Marrow Sense",
            "Hunters detect Marrow-Sink tiles before entering them — revealed at combat start.", "");
        var inn16 = MakeInnovation("INN-16", "Weak Points",
            "All hunters gain +1 Accuracy on first attack against any newly revealed or broken monster part.", "");
        var inn17 = MakeInnovation("INN-17", "Pack Mentality",
            "When 2+ hunters adjacent to same part, all attacks against it gain +1 Strength.", "");
        var inn18 = MakeInnovation("INN-18", "Controlled Breathing",
            "Once per hunter per hunt, that hunter may add +1 to any single die roll for free.", "");
        var inn19 = MakeInnovation("INN-19", "Battle Rhythm",
            "When a hunter plays 2 cards in succession from same category, draw 1 extra card.", "");
        var inn20 = MakeInnovation("INN-20", "The Final Push",
            "Once per combat: when a hunter collapses, all surviving hunters immediately gain 2 Grit and 2 AP.", "");

        // ── Pass 2: Wire cascade addsToDeck references ──
        inn01.addsToDeck = new[] { inn07, inn11 };
        inn02.addsToDeck = new[] { inn08, inn12 };
        inn03.addsToDeck = new[] { inn09, inn13 };
        inn04.addsToDeck = new[] { inn10, inn14 };
        inn05.addsToDeck = new[] { inn15 };
        inn06.addsToDeck = new[] { inn16 };
        inn07.addsToDeck = new[] { inn17 };
        inn08.addsToDeck = new[] { inn18 };
        inn09.addsToDeck = new[] { inn19 };
        inn10.addsToDeck = new[] { inn20 };

        MarkAllDirty(inn01, inn02, inn03, inn04, inn05, inn06, inn07, inn08, inn09, inn10,
                     inn11, inn12, inn13, inn14, inn15, inn16, inn17, inn18, inn19, inn20);

        // ── Guiding Principals ────────────────────────────────────────
        var gp01 = MakeGP("GP-01", "Life or Strength",
            "Year 1, automatic.",
            "Life", "Settlement grows cautiously: +2 starting characters next campaign.",
                     "+2 starting characters (apply in next campaign setup)",
            "Strength", "Hunters are forged harder: all hunters start with +1 Strength.",
                        "+1 Strength to all hunters at campaign start (apply manually)");

        var gp02 = MakeGP("GP-02", "Blood Price",
            "After first permanent character death.",
            "Honor", "Build a memorial: unlock Artifact 'The First Stone,' chronicle entry.",
                     "UnlockArtifact:TheFirstStone;AddChronicleEntry",
            "Drive", "Channel grief: all remaining hunters gain +1 Accuracy permanently.",
                     "+1 Accuracy to all surviving hunters (apply manually)");
        gp02.choiceA = SetArtifact(gp02.choiceA, "TheFirstStone");

        var gp03 = MakeGP("GP-03", "Marrow Knowledge",
            "EVT-13 Study outcome.",
            "Study Deeper", "Gain 3 Innovations immediately from pool.",
                            "DrawInnovations:3",
            "Seal the Knowledge", "Destroy 2 Innovations from pool, gain immunity to Marrow-Sink tile effects.",
                                  "RemoveInnovations:2;GrantImmunity:MarrowSink");

        var gp04 = MakeGP("GP-04", "Legacy or Forgetting",
            "Any character retirement.",
            "Legacy", "Retired hunter's name carved into settlement: +1 Grit starting value for all new characters.",
                      "+1 Grit starting value for all new characters (apply manually)",
            "Forgetting", "Settlement moves forward: unlock 2 random Innovations immediately.",
                          "DrawInnovations:2");

        var gp05 = MakeGP("GP-05", "The Suture",
            "Year 26+, approaching end.",
            "Stand Firm", "All hunters start Year 30 with maximum Grit.",
                          "MaxGrit:AllHunters:Year30",
            "Prepare", "Unlock all remaining locked Innovations in the pool immediately.",
                       "UnlockAllInnovations");

        EditorUtility.SetDirty(gp01);
        EditorUtility.SetDirty(gp02);
        EditorUtility.SetDirty(gp03);
        EditorUtility.SetDirty(gp04);
        EditorUtility.SetDirty(gp05);

        // ── Update Tutorial Campaign ──────────────────────────────────
        var tutorialPath = "Assets/_Game/Data/Campaigns/Mock_TutorialCampaign.asset";
        var tutorial = AssetDatabase.LoadAssetAtPath<CampaignSO>(tutorialPath);
        if (tutorial != null)
        {
            tutorial.startingInnovations = new[] { inn01, inn02, inn03 };
            tutorial.guidingPrincipals   = new[] { gp01 };
            EditorUtility.SetDirty(tutorial);
        }
        else
        {
            Debug.LogWarning("7M: Mock_TutorialCampaign.asset not found — skipping tutorial update.");
        }

        // ── Create Standard Campaign ──────────────────────────────────
        const string standardPath = "Assets/_Game/Data/Campaigns/Campaign_Standard.asset";
        var standard = CreateOrLoad<CampaignSO>(standardPath);
        standard.campaignName          = "Standard Campaign";
        standard.difficulty            = DifficultyLevel.Medium;
        standard.campaignLengthYears   = 30;
        standard.startingCharacterCount = 8;
        standard.baseMovement          = 3;
        standard.startingGrit          = 3;
        standard.ironmanMode           = false;
        standard.retirementHuntCount   = 10;
        standard.startingInnovations   = new[] { inn01, inn02, inn03, inn04, inn05, inn06 };
        standard.guidingPrincipals     = new[] { gp01, gp02, gp03, gp04, gp05 };
        EditorUtility.SetDirty(standard);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("7M: Created 20 InnovationSO assets, 5 GuidingPrincipalSO assets, " +
                  "wired cascade tree, updated Tutorial campaign, created Standard campaign.");
    }

    // ── Helpers ───────────────────────────────────────────────────────

    static InnovationSO MakeInnovation(string id, string name, string effect, string gritSkill)
    {
        string sanitized = name.Replace(" ", "").Replace("'", "").Replace(",", "");
        string idClean   = id.Replace("-", "");
        string path      = $"Assets/_Game/Data/Innovations/{idClean}_{sanitized}.asset";
        var so           = CreateOrLoad<InnovationSO>(path);
        so.innovationId      = id;
        so.innovationName    = name;
        so.effect            = effect;
        so.gritSkillUnlocked = gritSkill;
        so.addsToDeck        = new InnovationSO[0];
        return so;
    }

    static GuidingPrincipalSO MakeGP(
        string id, string name, string trigger,
        string aLabel, string aOutcome, string aMechEffect,
        string bLabel, string bOutcome, string bMechEffect)
    {
        string sanitized = name.Replace(" ", "").Replace("'", "");
        string idClean   = id.Replace("-", "");
        string path      = $"Assets/_Game/Data/GuidingPrincipals/{idClean}_{sanitized}.asset";
        var so           = CreateOrLoad<GuidingPrincipalSO>(path);
        so.principalId        = id;
        so.principalName      = name;
        so.triggerCondition   = trigger;
        so.choiceA = new EventChoice
        {
            choiceLabel       = aLabel,
            outcomeText       = aOutcome,
            mechanicalEffect  = aMechEffect,
        };
        so.choiceB = new EventChoice
        {
            choiceLabel       = bLabel,
            outcomeText       = bOutcome,
            mechanicalEffect  = bMechEffect,
        };
        return so;
    }

    static EventChoice SetArtifact(EventChoice c, string artifactId)
    {
        c.artifactUnlockId = artifactId;
        return c;
    }

    static T CreateOrLoad<T>(string path) where T : ScriptableObject
    {
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null) return existing;
        var so = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(so, path);
        return so;
    }

    static void EnsureFolder(string parent, string child)
    {
        string full = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(full))
            AssetDatabase.CreateFolder(parent, child);
    }

    static void MarkAllDirty(params InnovationSO[] assets)
    {
        foreach (var a in assets) EditorUtility.SetDirty(a);
    }
}
