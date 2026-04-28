using UnityEditor;
using UnityEngine;
using MnM.Core.UI;

public class WireMenuSprites
{
    public static void Execute()
    {
        var uiDocGO = GameObject.Find("UIDocument");
        if (uiDocGO == null) { Debug.LogError("[WireSprites] UIDocument not found."); return; }

        var ctrl = uiDocGO.GetComponent<MainMenuController>();
        if (ctrl == null) { Debug.LogError("[WireSprites] MainMenuController not found."); return; }

        var bg   = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Game/Art/Generated/UI/ui_main_menu_bg.png");
        var logo = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Game/Art/Generated/UI/ui_title_logo.png");

        if (bg   == null) { Debug.LogError("[WireSprites] ui_main_menu_bg.png not found."); return; }
        if (logo == null) { Debug.LogError("[WireSprites] ui_title_logo.png not found.");   return; }

        var so = new SerializedObject(ctrl);
        so.FindProperty("_bgSprite").objectReferenceValue   = bg;
        so.FindProperty("_logoSprite").objectReferenceValue = logo;
        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(uiDocGO);
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[WireSprites] _bgSprite + _logoSprite assigned. Scene saved.");
    }
}
