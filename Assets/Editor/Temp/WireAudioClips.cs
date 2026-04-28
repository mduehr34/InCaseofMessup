using UnityEngine;
using UnityEditor;
using MnM.Core.Systems;

public class WireAudioClips
{
    public static void Execute()
    {
        var go = GameObject.Find("AudioManager");
        if (go == null) { Debug.LogError("[WireAudio] AudioManager GameObject not found"); return; }

        var am = go.GetComponent<AudioManager>();
        if (am == null) { Debug.LogError("[WireAudio] AudioManager component not found"); return; }

        var mainMenu = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/Music/music_main_menu.wav");
        var uiClick  = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/SFX/sfx_ui_click.wav");

        if (mainMenu == null) Debug.LogError("[WireAudio] music_main_menu.wav not found at expected path");
        if (uiClick  == null) Debug.LogError("[WireAudio] sfx_ui_click.wav not found at expected path");

        var so = new SerializedObject(am);
        so.FindProperty("_mainMenuMusic").objectReferenceValue = mainMenu;
        so.FindProperty("_sfxUiClick").objectReferenceValue   = uiClick;
        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(go);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(go.scene);

        Debug.Log($"[WireAudio] Done — mainMenuMusic={mainMenu?.name ?? "NULL"}, sfxUiClick={uiClick?.name ?? "NULL"}");
    }
}
