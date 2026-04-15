using UnityEditor;
using UnityEngine;
using MnM.Core.Data;

/// <summary>
/// One-shot editor script — creates mock EventSO, GuidingPrincipalSO, CrafterSO, and
/// InnovationSO assets for Stage 6-D verification, then wires them into
/// Mock_TutorialCampaign.asset.
/// </summary>
public class CreateMockSettlementData
{
    [MenuItem("MnM/Dev/Create Mock Settlement Data (6-D)")]
    public static void Execute()
    {
        // ── Ensure folders exist ──────────────────────────────────────
        EnsureFolder("Assets/_Game/Data", "Events");
        EnsureFolder("Assets/_Game/Data", "GuidingPrincipals");
        EnsureFolder("Assets/_Game/Data", "Crafters");
        EnsureFolder("Assets/_Game/Data", "Innovations");

        // ── Events ────────────────────────────────────────────────────
        var evt01 = CreateOrLoad<EventSO>("Assets/_Game/Data/Events/Mock_EVT01_FirstWinter.asset");
        evt01.eventId       = "EVT-01";
        evt01.eventName     = "The First Winter";
        evt01.yearRangeMin  = 1;
        evt01.yearRangeMax  = 2;
        evt01.isMandatory   = true;
        evt01.narrativeText = "The first snow falls earlier than expected. " +
                              "The settlement huddles together against the cold. " +
                              "A child points to the horizon — something moves in the white.";
        evt01.choices       = new EventChoice[0];
        EditorUtility.SetDirty(evt01);

        var evt02 = CreateOrLoad<EventSO>("Assets/_Game/Data/Events/Mock_EVT02_Wanderer.asset");
        evt02.eventId       = "EVT-02";
        evt02.eventName     = "The Wanderer";
        evt02.yearRangeMin  = 1;
        evt02.yearRangeMax  = 5;
        evt02.isMandatory   = false;
        evt02.narrativeText = "A scarred stranger arrives at the settlement gates. " +
                              "They carry a satchel of tools and an unreadable expression. " +
                              "They say only: \"I know things worth knowing.\"";
        evt02.choices = new EventChoice[]
        {
            new EventChoice
            {
                choiceLabel            = "A",
                outcomeText            = "Welcome the wanderer. They share their knowledge freely.",
                mechanicalEffect       = "+2 to next Innovations draw (apply manually)",
                guidingPrincipalTrigger = "",
                codexEntryId           = "",
                artifactUnlockId       = "",
            },
            new EventChoice
            {
                choiceLabel            = "B",
                outcomeText            = "Ask what they truly want. Their answer changes everything.",
                mechanicalEffect       = "Triggers Guiding Principal: Blood or Bone (apply manually)",
                guidingPrincipalTrigger = "GP-01",
                codexEntryId           = "",
                artifactUnlockId       = "",
            },
        };
        EditorUtility.SetDirty(evt02);

        // ── Guiding Principal ─────────────────────────────────────────
        var gp01 = CreateOrLoad<GuidingPrincipalSO>(
            "Assets/_Game/Data/GuidingPrincipals/Mock_GP01_BloodOrBone.asset");
        gp01.principalId      = "GP-01";
        gp01.principalName    = "Blood or Bone";
        gp01.triggerCondition = "The wanderer demands a price for their secrets. " +
                                "You must choose what the settlement values most.";
        gp01.choiceA = new EventChoice
        {
            choiceLabel      = "A",
            outcomeText      = "Blood. The settlement will always take the hunt, no matter the cost.",
            mechanicalEffect = "Hunters can never willingly retreat from a hunt (apply manually)",
        };
        gp01.choiceB = new EventChoice
        {
            choiceLabel      = "B",
            outcomeText      = "Bone. The settlement values its people above glory.",
            mechanicalEffect = "Hunters may retreat but lose 1 resource on return (apply manually)",
        };
        EditorUtility.SetDirty(gp01);

        // ── Crafter ───────────────────────────────────────────────────
        var boneShaper = CreateOrLoad<CrafterSO>(
            "Assets/_Game/Data/Crafters/Mock_Crafter_BoneShaper.asset");
        boneShaper.crafterName  = "Bone Shaper";
        boneShaper.monsterTag   = "Gaunt";
        boneShaper.materialTier = 1;
        boneShaper.recipeList   = new ItemSO[0];
        boneShaper.unlockCost   = new ResourceSO[0];
        boneShaper.unlockCostAmounts = new int[0];
        EditorUtility.SetDirty(boneShaper);

        var hideWorker = CreateOrLoad<CrafterSO>(
            "Assets/_Game/Data/Crafters/Mock_Crafter_HideWorker.asset");
        hideWorker.crafterName  = "Hide Worker";
        hideWorker.monsterTag   = "Gaunt";
        hideWorker.materialTier = 1;
        hideWorker.recipeList   = new ItemSO[0];

        // Costs 2 Gaunt Fang to unlock — load the existing mock resource
        var gauntFang = AssetDatabase.LoadAssetAtPath<ResourceSO>(
            "Assets/_Game/Data/Resources/Mock_GauntFang.asset");
        if (gauntFang != null)
        {
            hideWorker.unlockCost        = new ResourceSO[] { gauntFang };
            hideWorker.unlockCostAmounts = new int[] { 2 };
        }
        else
        {
            hideWorker.unlockCost        = new ResourceSO[0];
            hideWorker.unlockCostAmounts = new int[0];
            Debug.LogWarning("[MockData] Mock_GauntFang.asset not found — HideWorker cost set to free");
        }
        EditorUtility.SetDirty(hideWorker);

        // ── Innovations ───────────────────────────────────────────────
        var inn01 = CreateOrLoad<InnovationSO>(
            "Assets/_Game/Data/Innovations/Mock_INN01_Bloodwine.asset");
        inn01.innovationId   = "INN-01";
        inn01.innovationName = "Bloodwine";
        inn01.effect         = "Hunters may spend 1 Bone during Settlement to restore 1 Flesh wound.";
        inn01.addsToDeck     = new InnovationSO[0];
        EditorUtility.SetDirty(inn01);

        var inn02 = CreateOrLoad<InnovationSO>(
            "Assets/_Game/Data/Innovations/Mock_INN02_BoneArmor.asset");
        inn02.innovationId   = "INN-02";
        inn02.innovationName = "Bone Armor";
        inn02.effect         = "Once per hunt, a hunter may ignore the first Shell Hit against them.";
        inn02.addsToDeck     = new InnovationSO[0];
        EditorUtility.SetDirty(inn02);

        var inn03 = CreateOrLoad<InnovationSO>(
            "Assets/_Game/Data/Innovations/Mock_INN03_Ammonia.asset");
        inn03.innovationId   = "INN-03";
        inn03.innovationName = "Ammonia";
        inn03.effect         = "Hunters gain +1 Accuracy on the first round of any hunt.";
        inn03.addsToDeck     = new InnovationSO[0];
        EditorUtility.SetDirty(inn03);

        // ── Wire into Tutorial Campaign SO ────────────────────────────
        var campaign = AssetDatabase.LoadAssetAtPath<CampaignSO>(
            "Assets/_Game/Data/Campaigns/Mock_TutorialCampaign.asset");

        if (campaign == null)
        {
            Debug.LogError("[MockData] Could not load Mock_TutorialCampaign.asset — aborting wiring");
            AssetDatabase.SaveAssets();
            return;
        }

        campaign.eventPool          = new EventSO[] { evt01, evt02 };
        campaign.guidingPrincipals  = new GuidingPrincipalSO[] { gp01 };
        campaign.crafterPool        = new CrafterSO[] { boneShaper, hideWorker };
        campaign.startingInnovations = new InnovationSO[] { inn01, inn02, inn03 };

        EditorUtility.SetDirty(campaign);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[MockData] Stage 6-D mock data created and wired into Mock_TutorialCampaign. " +
                  "Start a NEW campaign to seed the innovation pool correctly.");
    }

    // ── Helpers ───────────────────────────────────────────────────────

    private static void EnsureFolder(string parent, string folderName)
    {
        string path = $"{parent}/{folderName}";
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, folderName);
            Debug.Log($"[MockData] Created folder: {path}");
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
