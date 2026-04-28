using UnityEngine;
using UnityEditor;
using MnM.Core.Systems;

public class WireAudioPrefab
{
    public static void Execute()
    {
        const string prefabPath = "Assets/_Game/Prefabs/AudioManager.prefab";

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null) { Debug.LogError("[WireAudioPrefab] Prefab not found"); return; }

        var am = prefab.GetComponent<AudioManager>();
        if (am == null) { Debug.LogError("[WireAudioPrefab] AudioManager component not found on prefab"); return; }

        var mainMenu = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/Music/music_main_menu.wav");
        var uiClick  = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/SFX/sfx_ui_click.wav");

        if (mainMenu == null) { Debug.LogError("[WireAudioPrefab] music_main_menu.wav not found"); return; }
        if (uiClick  == null) { Debug.LogError("[WireAudioPrefab] sfx_ui_click.wav not found"); return; }

        var so = new SerializedObject(am);
        so.FindProperty("_mainMenuMusic").objectReferenceValue = mainMenu;
        so.FindProperty("_sfxUiClick").objectReferenceValue   = uiClick;
        so.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SavePrefabAsset(prefab);

        Debug.Log($"[WireAudioPrefab] Done — mainMenuMusic={mainMenu.name}, sfxUiClick={uiClick.name}");
    }
}
