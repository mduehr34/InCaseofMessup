using UnityEditor;
using UnityEngine;
using MnM.Core.Data;
using MnM.Core.UI;

public class AssignSpawnZones
{
    public static void Execute()
    {
        var zone = AssetDatabase.LoadAssetAtPath<SpawnZoneSO>(
            "Assets/_Game/Data/Combat/SpawnZone_GauntStandard.asset");

        if (zone == null)
        {
            Debug.LogError("[8-O] SpawnZone_GauntStandard.asset not found");
            return;
        }

        var go = GameObject.Find("CombatUI");
        if (go == null)
        {
            Debug.LogError("[8-O] CombatUI not found in scene");
            return;
        }

        var ctrl = go.GetComponent<CombatScreenController>();
        if (ctrl == null)
        {
            Debug.LogError("[8-O] CombatScreenController not found on CombatUI");
            return;
        }

        var so = new SerializedObject(ctrl);
        var prop = so.FindProperty("_spawnZones");
        prop.arraySize = 1;
        prop.GetArrayElementAtIndex(0).objectReferenceValue = zone;
        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(go);
        Debug.Log("[8-O] _spawnZones[0] assigned to SpawnZone_GauntStandard");
    }
}
