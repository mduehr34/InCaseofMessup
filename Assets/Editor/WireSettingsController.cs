using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using MnM.Core.UI;

public class WireSettingsController
{
    public static void Execute()
    {
        var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>("Assets/_Game/Audio/MasterMixer.mixer");
        if (mixer == null) { Debug.LogError("[WireSettings] MasterMixer not found"); return; }

        var go = GameObject.Find("SettingsUI");
        if (go == null) { Debug.LogError("[WireSettings] SettingsUI not found in scene"); return; }

        var ctrl = go.GetComponent<SettingsController>();
        if (ctrl == null) { Debug.LogError("[WireSettings] SettingsController not found on SettingsUI"); return; }

        var so = new SerializedObject(ctrl);
        so.FindProperty("_masterMixer").objectReferenceValue = mixer;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(go);
        Debug.Log("[WireSettings] AudioMixer wired on SettingsController");
    }
}
