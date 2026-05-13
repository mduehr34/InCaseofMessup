using UnityEngine;
using UnityEditor;
using MnM.Core.Data;

public class CreateTravelEvents
{
    public static void Execute()
    {
        string folder = "Assets/_Game/Data/Events";
        if (!AssetDatabase.IsValidFolder(folder))
        {
            // Create parent folders as needed
            if (!AssetDatabase.IsValidFolder("Assets/_Game/Data"))
                AssetDatabase.CreateFolder("Assets/_Game", "Data");
            AssetDatabase.CreateFolder("Assets/_Game/Data", "Events");
        }

        CreateEvent(folder, "Event_TRV01", new EventConfig
        {
            eventId       = "TRV-01",
            eventName     = "Tracks",
            narrativeText = "Fresh tracks in the mud. Large. Recent. Something big passed through here not long ago, and the trail is still warm.",
            yearMin       = 1, yearMax = 30, isTravel = true,
            choices = new[]
            {
                new ChoiceConfig { label = "A", outcome = "Follow the tracks. (+1 Accuracy for the first combat round — you know exactly where it is.)", effect = "accuracy_bonus_round1:1" },
                new ChoiceConfig { label = "B", outcome = "Avoid the trail. Circle wide and approach from upwind. (No effect — but no risk either.)", effect = "" }
            }
        });

        CreateEvent(folder, "Event_TRV02", new EventConfig
        {
            eventId       = "TRV-02",
            eventName     = "Old Cairn",
            narrativeText = "Someone built this. Stacked stones, deliberate. A warning, maybe. Or a grave. The bones worked into the base are human-sized.",
            yearMin       = 1, yearMax = 30, isTravel = true,
            choices = new[]
            {
                new ChoiceConfig { label = "A", outcome = "Search the cairn. (Gain 1 Bone.)", effect = "resource:Bone:1" },
                new ChoiceConfig { label = "B", outcome = "Leave it undisturbed. Whatever it wards against, you don't want to know. (No effect.)", effect = "" }
            }
        });

        CreateEvent(folder, "Event_TRV03", new EventConfig
        {
            eventId       = "TRV-03",
            eventName     = "The Fog",
            narrativeText = "The fog came in fast. One moment you could see each other; the next, only shapes in grey. You split up briefly. Something moved between you.",
            yearMin       = 1, yearMax = 30, isTravel = true,
            choices = new[]
            {
                new ChoiceConfig { label = "A", outcome = "Call out — stay loud until you regroup. (No effect. You arrive together.)", effect = "" },
                new ChoiceConfig { label = "B", outcome = "Stay silent. Whatever it was might still be close. (All hunters begin combat with the Shaken status.)", effect = "status:Shaken:all" }
            }
        });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CreateTravelEvents] TRV-01, TRV-02, TRV-03 created in " + folder);
    }

    private struct EventConfig
    {
        public string eventId, eventName, narrativeText;
        public int yearMin, yearMax;
        public bool isTravel;
        public ChoiceConfig[] choices;
    }

    private struct ChoiceConfig
    {
        public string label, outcome, effect;
    }

    private static void CreateEvent(string folder, string assetName, EventConfig cfg)
    {
        string path = $"{folder}/{assetName}.asset";
        if (AssetDatabase.LoadAssetAtPath<EventSO>(path) != null)
        {
            Debug.Log($"[CreateTravelEvents] {assetName} already exists — skipping");
            return;
        }

        var so = ScriptableObject.CreateInstance<EventSO>();
        so.eventId       = cfg.eventId;
        so.eventName     = cfg.eventName;
        so.narrativeText = cfg.narrativeText;
        so.yearRangeMin  = cfg.yearMin;
        so.yearRangeMax  = cfg.yearMax;
        so.isTravel      = cfg.isTravel;
        so.campaignTag   = "travel";    // belt-and-suspenders tag for future filters
        so.isMandatory   = false;

        if (cfg.choices != null && cfg.choices.Length > 0)
        {
            so.choices = new EventChoice[cfg.choices.Length];
            for (int i = 0; i < cfg.choices.Length; i++)
            {
                so.choices[i] = new EventChoice
                {
                    choiceLabel       = cfg.choices[i].label,
                    outcomeText       = cfg.choices[i].outcome,
                    mechanicalEffect  = cfg.choices[i].effect
                };
            }
        }

        AssetDatabase.CreateAsset(so, path);
        Debug.Log($"[CreateTravelEvents] Created {path}");
    }
}
