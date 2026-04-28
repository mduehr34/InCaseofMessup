using UnityEditor;
using UnityEngine;

public class VerifyBuildScenes
{
    public static void Execute()
    {
        var scenes = EditorBuildSettings.scenes;
        Debug.Log($"[BuildScenes] Total: {scenes.Length}");
        for (int i = 0; i < scenes.Length; i++)
            Debug.Log($"[BuildScenes] [{i}] {scenes[i].path} enabled={scenes[i].enabled}");
    }
}
