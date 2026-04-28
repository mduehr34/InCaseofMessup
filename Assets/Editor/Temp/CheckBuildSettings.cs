using UnityEditor;
using UnityEngine;

public class CheckBuildSettings
{
    public static void Execute()
    {
        var scenes = EditorBuildSettings.scenes;
        Debug.Log($"[BuildSettings] Total scenes: {scenes.Length}");
        for (int i = 0; i < scenes.Length; i++)
        {
            Debug.Log($"[BuildSettings] [{i}] {scenes[i].path} enabled={scenes[i].enabled}");
        }
    }
}
