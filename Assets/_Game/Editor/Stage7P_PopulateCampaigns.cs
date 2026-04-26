using UnityEditor;
using UnityEngine;
using MnM.Core.Data;
using System.IO;

public class Stage7P_PopulateCampaigns
{
    public static void Execute()
    {
        CreateOverlordStubs();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        PopulateTutorialCampaign();
        PopulateStandardCampaign();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[Stage7P] Campaign SO population complete.");
    }

    static void CreateOverlordStubs()
    {
        CreateOverlordStub(
            "Assets/_Game/Data/Monsters/Overlord_Siltborn.asset",
            "The Siltborn",
            "The river-drowned colossal — Year 5 overlord.");

        CreateOverlordStub(
            "Assets/_Game/Data/Monsters/Overlord_PaleStag.asset",
            "Pale Stag Ascendant",
            "The pale stag reborn — Year 25 overlord.");
    }

    static void CreateOverlordStub(string path, string monsterName, string note)
    {
        if (File.Exists(Path.Combine(Application.dataPath, "..", path)))
        {
            Debug.Log($"[Stage7P] {monsterName} already exists — skipping.");
            return;
        }

        var so = ScriptableObject.CreateInstance<MonsterSO>();
        so.monsterName = monsterName;
        AssetDatabase.CreateAsset(so, path);
        Debug.Log($"[Stage7P] Created stub: {path}");
    }

    static void PopulateTutorialCampaign()
    {
        const string tutPath = "Assets/_Game/Data/Campaigns/Campaign_Tutorial.asset";
        CampaignSO tut = AssetDatabase.LoadAssetAtPath<CampaignSO>(tutPath);
        if (tut == null)
        {
            tut = ScriptableObject.CreateInstance<CampaignSO>();
            AssetDatabase.CreateAsset(tut, tutPath);
            Debug.Log("[Stage7P] Created Campaign_Tutorial.asset");
        }

        tut.campaignName           = "Tutorial Campaign";
        tut.difficulty             = DifficultyLevel.Medium;
        tut.campaignLengthYears    = 3;
        tut.startingCharacterCount = 8;
        tut.baseMovement           = 3;
        tut.startingGrit           = 3;
        tut.ironmanMode            = false;
        tut.retirementHuntCount    = 10;
        tut.birthConditionAge      = 0;

        tut.monsterRoster = new[]
        {
            Load<MonsterSO>("Assets/_Game/Data/Monsters/Monster_Gaunt.asset"),
        };

        tut.eventPool = new[]
        {
            Load<EventSO>("Assets/_Game/Data/Events/Event_EVT01.asset"),
            Load<EventSO>("Assets/_Game/Data/Events/Event_EVT02.asset"),
            Load<EventSO>("Assets/_Game/Data/Events/Event_EVT03.asset"),
            Load<EventSO>("Assets/_Game/Data/Events/Event_EVT05.asset"),
            Load<EventSO>("Assets/_Game/Data/Events/Event_EVT06.asset"),
        };

        tut.startingInnovations = new[]
        {
            Load<InnovationSO>("Assets/_Game/Data/Innovations/INN01_DesperateSprint.asset"),
            Load<InnovationSO>("Assets/_Game/Data/Innovations/INN02_MeasuredHand.asset"),
            Load<InnovationSO>("Assets/_Game/Data/Innovations/INN03_ShouldertoShoulder.asset"),
        };

        tut.crafterPool = new[]
        {
            Load<CrafterSO>("Assets/_Game/Data/Crafters/Crafter_Boneworks.asset"),
        };

        tut.guidingPrincipals = new[]
        {
            Load<GuidingPrincipalSO>("Assets/_Game/Data/GuidingPrincipals/GP01_LifeorStrength.asset"),
        };

        tut.overlordSchedule = new OverlordScheduleEntry[0];

        EditorUtility.SetDirty(tut);
        Debug.Log("[Stage7P] Tutorial Campaign populated.");
    }

    static void PopulateStandardCampaign()
    {
        const string stdPath = "Assets/_Game/Data/Campaigns/Campaign_Standard.asset";
        CampaignSO std = AssetDatabase.LoadAssetAtPath<CampaignSO>(stdPath);
        if (std == null)
        {
            Debug.LogError("[Stage7P] Campaign_Standard.asset not found!");
            return;
        }

        std.campaignName           = "The Standard Campaign";
        std.difficulty             = DifficultyLevel.Medium;
        std.campaignLengthYears    = 30;
        std.startingCharacterCount = 8;
        std.baseMovement           = 3;
        std.startingGrit           = 3;
        std.ironmanMode            = false;
        std.retirementHuntCount    = 10;
        std.birthConditionAge      = 0;

        // The Suture NOT included — behavior deck not yet designed
        std.monsterRoster = new[]
        {
            Load<MonsterSO>("Assets/_Game/Data/Monsters/Monster_Gaunt.asset"),
            Load<MonsterSO>("Assets/_Game/Data/Monsters/Monster_Thornback.asset"),
            Load<MonsterSO>("Assets/_Game/Data/Monsters/Monster_TheIvoryStampede.asset"),
            Load<MonsterSO>("Assets/_Game/Data/Monsters/Monster_GildedSerpent.asset"),
            Load<MonsterSO>("Assets/_Game/Data/Monsters/Monster_TheSpite.asset"),
        };

        std.eventPool = new[]
        {
            Load<EventSO>("Assets/_Game/Data/Events/Event_EVT01.asset"),
            Load<EventSO>("Assets/_Game/Data/Events/Event_EVT02.asset"),
            Load<EventSO>("Assets/_Game/Data/Events/Event_EVT03.asset"),
            Load<EventSO>("Assets/_Game/Data/Events/Event_EVT04.asset"),
            Load<EventSO>("Assets/_Game/Data/Events/Event_EVT05.asset"),
            Load<EventSO>("Assets/_Game/Data/Events/Event_EVT06.asset"),
            Load<EventSO>("Assets/_Game/Data/Events/Event_EVT07.asset"),
            Load<EventSO>("Assets/_Game/Data/Events/Event_EVT08.asset"),
            Load<EventSO>("Assets/_Game/Data/Events/Event_EVT09.asset"),
            Load<EventSO>("Assets/_Game/Data/Events/Event_EVT10.asset"),
            Load<EventSO>("Assets/_Game/Data/Events/Event_EVT11.asset"),
            Load<EventSO>("Assets/_Game/Data/Events/Event_EVT12.asset"),
            Load<EventSO>("Assets/_Game/Data/Events/Event_EVT13.asset"),
            Load<EventSO>("Assets/_Game/Data/Events/Event_EVT14.asset"),
            Load<EventSO>("Assets/_Game/Data/Events/Event_EVT15.asset"),
            Load<EventSO>("Assets/_Game/Data/Events/Event_EVT16.asset"),
            Load<EventSO>("Assets/_Game/Data/Events/Event_EVT17.asset"),
            Load<EventSO>("Assets/_Game/Data/Events/Event_EVT18.asset"),
            Load<EventSO>("Assets/_Game/Data/Events/Event_EVT19.asset"),
            Load<EventSO>("Assets/_Game/Data/Events/Event_EVT20.asset"),
            Load<EventSO>("Assets/_Game/Data/Events/Event_EVT21.asset"),
            Load<EventSO>("Assets/_Game/Data/Events/Event_EVT22.asset"),
            Load<EventSO>("Assets/_Game/Data/Events/Event_EVT23.asset"),
            Load<EventSO>("Assets/_Game/Data/Events/Event_EVT24.asset"),
            Load<EventSO>("Assets/_Game/Data/Events/Event_EVT25.asset"),
            Load<EventSO>("Assets/_Game/Data/Events/Event_EVT26.asset"),
            Load<EventSO>("Assets/_Game/Data/Events/Event_EVT27.asset"),
            Load<EventSO>("Assets/_Game/Data/Events/Event_EVT28.asset"),
            Load<EventSO>("Assets/_Game/Data/Events/Event_EVT29.asset"),
            Load<EventSO>("Assets/_Game/Data/Events/Event_EVT30.asset"),
        };

        std.startingInnovations = new[]
        {
            Load<InnovationSO>("Assets/_Game/Data/Innovations/INN01_DesperateSprint.asset"),
            Load<InnovationSO>("Assets/_Game/Data/Innovations/INN02_MeasuredHand.asset"),
            Load<InnovationSO>("Assets/_Game/Data/Innovations/INN03_ShouldertoShoulder.asset"),
            Load<InnovationSO>("Assets/_Game/Data/Innovations/INN04_IronWill.asset"),
            Load<InnovationSO>("Assets/_Game/Data/Innovations/INN05_BoneReading.asset"),
            Load<InnovationSO>("Assets/_Game/Data/Innovations/INN06_KeenEye.asset"),
        };

        std.crafterPool = new[]
        {
            Load<CrafterSO>("Assets/_Game/Data/Crafters/Crafter_Boneworks.asset"),
        };

        std.guidingPrincipals = new[]
        {
            Load<GuidingPrincipalSO>("Assets/_Game/Data/GuidingPrincipals/GP01_LifeorStrength.asset"),
            Load<GuidingPrincipalSO>("Assets/_Game/Data/GuidingPrincipals/GP02_BloodPrice.asset"),
            Load<GuidingPrincipalSO>("Assets/_Game/Data/GuidingPrincipals/GP03_MarrowKnowledge.asset"),
            Load<GuidingPrincipalSO>("Assets/_Game/Data/GuidingPrincipals/GP04_LegacyorForgetting.asset"),
            Load<GuidingPrincipalSO>("Assets/_Game/Data/GuidingPrincipals/GP05_TheSuture.asset"),
        };

        // OVR-04 uses Monster_Suture as the overlord reference (it arrives as final boss, not a huntable roster monster)
        std.overlordSchedule = new[]
        {
            new OverlordScheduleEntry
            {
                overlordMonster = Load<MonsterSO>("Assets/_Game/Data/Monsters/Overlord_Siltborn.asset"),
                arrivalYear     = 5,
                approachYears   = new[] { 3, 4 },
            },
            new OverlordScheduleEntry
            {
                overlordMonster = Load<MonsterSO>("Assets/_Game/Data/Monsters/Overlord_Penitent.asset"),
                arrivalYear     = 15,
                approachYears   = new[] { 12, 14 },
            },
            new OverlordScheduleEntry
            {
                overlordMonster = Load<MonsterSO>("Assets/_Game/Data/Monsters/Overlord_PaleStag.asset"),
                arrivalYear     = 25,
                approachYears   = new[] { 22, 24 },
            },
            new OverlordScheduleEntry
            {
                overlordMonster = Load<MonsterSO>("Assets/_Game/Data/Monsters/Monster_Suture.asset"),
                arrivalYear     = 30,
                approachYears   = new[] { 25, 27, 29 },
            },
        };

        EditorUtility.SetDirty(std);
        Debug.Log("[Stage7P] Standard Campaign populated.");
    }

    static T Load<T>(string path) where T : UnityEngine.Object
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
            Debug.LogWarning($"[Stage7P] Missing asset: {path}");
        return asset;
    }
}
