using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using MnM.Core.Systems;

public class WireHuntTravelAudio
{
    public static void Execute()
    {
        // Try both file names — user may have used either convention
        string[] candidates = new[]
        {
            "Assets/_Game/Audio/Music/mus_hunt_travel.wav",
            "Assets/_Game/Audio/Music/music_hunt_travel.wav",
        };

        AudioClip clip = null;
        string usedPath = null;
        foreach (var path in candidates)
        {
            clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip != null) { usedPath = path; break; }
        }

        if (clip == null)
        {
            Debug.LogError("[WireHuntTravelAudio] AudioClip not found at any candidate path");
            return;
        }
        Debug.Log($"[WireHuntTravelAudio] Loaded clip: {usedPath}");

        // Find AudioManager in the Bootstrap scene
        var bootstrapScene = EditorSceneManager.OpenScene(
            "Assets/_Game/Scenes/Bootstrap.unity",
            OpenSceneMode.Additive);

        AudioManager audioMgr = null;
        foreach (var go in bootstrapScene.GetRootGameObjects())
        {
            audioMgr = go.GetComponentInChildren<AudioManager>(true);
            if (audioMgr != null) break;
        }

        if (audioMgr == null)
        {
            Debug.LogError("[WireHuntTravelAudio] AudioManager not found in Bootstrap scene");
            EditorSceneManager.CloseScene(bootstrapScene, true);
            return;
        }

        // Use SerializedObject so the change is properly serialized
        var so  = new SerializedObject(audioMgr);
        var prop = so.FindProperty("_huntTravel");
        if (prop == null)
        {
            Debug.LogError("[WireHuntTravelAudio] '_huntTravel' field not found on AudioManager");
            EditorSceneManager.CloseScene(bootstrapScene, true);
            return;
        }

        prop.objectReferenceValue = clip;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(audioMgr);
        EditorSceneManager.SaveScene(bootstrapScene);

        Debug.Log($"[WireHuntTravelAudio] Wired '{clip.name}' → AudioManager._huntTravel in Bootstrap");
        EditorSceneManager.CloseScene(bootstrapScene, true);
    }
}
