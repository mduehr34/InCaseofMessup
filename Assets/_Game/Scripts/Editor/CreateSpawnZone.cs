using UnityEngine;
using UnityEditor;
using MnM.Core.Data;

public class CreateSpawnZone
{
    public static void Execute()
    {
        // Create a rect spawn zone covering the left 6 columns, all 16 rows
        // Gives both hunters plenty of room during deployment
        var zone = ScriptableObject.CreateInstance<SpawnZoneSO>();
        zone.shape     = SpawnZoneShape.Rect;
        zone.rectX     = 0;
        zone.rectY     = 0;
        zone.rectWidth  = 6;
        zone.rectHeight = 16;

        System.IO.Directory.CreateDirectory("Assets/_Game/Data/SpawnZones");
        AssetDatabase.CreateAsset(zone, "Assets/_Game/Data/SpawnZones/SpawnZone_MockLeft.asset");
        AssetDatabase.SaveAssets();

        Debug.Log("[Setup] SpawnZone_MockLeft created: x0-5, all rows 0-15");

        // Wire it to CombatScreenController in the scene
        var go = GameObject.Find("CombatUI");
        if (go == null) { Debug.LogError("[Setup] CombatUI not found in scene"); return; }

        var ctrl = go.GetComponent<MnM.Core.UI.CombatScreenController>();
        if (ctrl == null) { Debug.LogError("[Setup] CombatScreenController not found"); return; }

        var serialized = new SerializedObject(ctrl);
        var spawnZonesProp = serialized.FindProperty("_spawnZones");
        spawnZonesProp.arraySize = 1;
        spawnZonesProp.GetArrayElementAtIndex(0).objectReferenceValue = zone;
        serialized.ApplyModifiedProperties();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[Setup] SpawnZone_MockLeft wired to CombatScreenController._spawnZones[0]");
    }
}
