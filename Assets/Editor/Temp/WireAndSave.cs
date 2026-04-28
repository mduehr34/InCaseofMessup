using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using MnM.Core.Systems;

public class WireAndSave
{
    public static void Execute()
    {
        var go = GameObject.Find("AudioManager");
        if (go == null) { Debug.LogError("[WireAndSave] AudioManager not found"); return; }

        var am = go.GetComponent<AudioManager>();
        if (am == null) { Debug.LogError("[WireAndSave] AudioManager component not found"); return; }

        var mainMenu = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/Music/music_main_menu.wav");
        var uiClick  = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/SFX/sfx_ui_click.wav");

        if (mainMenu == null) { Debug.LogError("[WireAndSave] music_main_menu.wav not found"); return; }
        if (uiClick  == null) { Debug.LogError("[WireAndSave] sfx_ui_click.wav not found"); return; }

        var so = new SerializedObject(am);
        so.FindProperty("_mainMenuMusic").objectReferenceValue = mainMenu;
        so.FindProperty("_sfxUiClick").objectReferenceValue   = uiClick;
        so.ApplyModifiedPropertiesWithoutUndo();

        var scene = go.scene;
        EditorSceneManager.MarkSceneDirty(scene);
        bool saved = EditorSceneManager.SaveScene(scene, scene.path);

        Debug.Log($"[WireAndSave] mainMenuMusic={mainMenu.name}, sfxUiClick={uiClick.name}, scene saved={saved} to {scene.path}");
    }
}
