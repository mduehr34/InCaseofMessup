using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AddCampaignSelectToBuildSettings
{
    public static void Execute()
    {
        const string scenePath = "Assets/_Game/Scenes/CampaignSelect.unity";

        var scenes = EditorBuildSettings.scenes.ToList();

        bool alreadyPresent = scenes.Any(s => s.path == scenePath);
        if (alreadyPresent)
        {
            Debug.Log("[BuildSettings] CampaignSelect already in scene list.");
            return;
        }

        // Insert after the last _Game scene (index 5, after Codex)
        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();

        Debug.Log($"[BuildSettings] Added '{scenePath}' — total scenes: {scenes.Count}");
        foreach (var s in EditorBuildSettings.scenes)
            Debug.Log($"  [{(s.enabled ? "x" : " ")}] {s.path}");
    }
}
