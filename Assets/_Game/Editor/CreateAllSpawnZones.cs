using UnityEditor;
using UnityEngine;
using MnM.Core.Data;
using MnM.Core.UI;

public class CreateAllSpawnZones
{
    public static void Execute()
    {
        const string folder = "Assets/_Game/Data/Combat";

        // Ensure folder exists
        if (!AssetDatabase.IsValidFolder(folder))
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Game/Data"))
                AssetDatabase.CreateFolder("Assets/_Game", "Data");
            AssetDatabase.CreateFolder("Assets/_Game/Data", "Combat");
        }

        // Define all four zones on a 22x16 grid
        var defs = new[]
        {
            // name,                     rectX, rectY, rectW, rectH
            ("SpawnZone_GauntStandard",      2,     5,     4,     6),   // Left flank
            ("SpawnZone_GauntStandard_Right",16,     5,     4,     6),  // Right flank
            ("SpawnZone_GauntStandard_Top",   8,     1,     6,     3),  // Top
            ("SpawnZone_GauntStandard_Bottom",8,    12,     6,     3),  // Bottom
        };

        var zones = new SpawnZoneSO[defs.Length];
        for (int i = 0; i < defs.Length; i++)
        {
            var (name, rx, ry, rw, rh) = defs[i];
            string path = $"{folder}/{name}.asset";

            // Reuse existing asset or create new
            var zone = AssetDatabase.LoadAssetAtPath<SpawnZoneSO>(path);
            if (zone == null)
            {
                zone = ScriptableObject.CreateInstance<SpawnZoneSO>();
                AssetDatabase.CreateAsset(zone, path);
            }

            zone.shape      = SpawnZoneShape.Rect;
            zone.rectX      = rx;
            zone.rectY      = ry;
            zone.rectWidth  = rw;
            zone.rectHeight = rh;

            EditorUtility.SetDirty(zone);
            zones[i] = zone;
            Debug.Log($"[8-O] Zone '{name}': Rect({rx},{ry}) {rw}x{rh}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Assign all four to CombatUI._spawnZones
        var go = GameObject.Find("CombatUI");
        if (go == null) { Debug.LogError("[8-O] CombatUI not found"); return; }

        var ctrl = go.GetComponent<CombatScreenController>();
        if (ctrl == null) { Debug.LogError("[8-O] CombatScreenController not found"); return; }

        var so = new SerializedObject(ctrl);
        var prop = so.FindProperty("_spawnZones");
        prop.arraySize = zones.Length;
        for (int i = 0; i < zones.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = zones[i];
        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(go);
        Debug.Log($"[8-O] CombatUI._spawnZones assigned — {zones.Length} zones total");
    }
}
