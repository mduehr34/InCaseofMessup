using UnityEditor;
using UnityEngine;
using MnM.Core.Data;

public class CreateGauntStandardSpawnZone
{
    public static void Execute()
    {
        const string folder = "Assets/_Game/Data/Combat";
        const string assetPath = folder + "/SpawnZone_GauntStandard.asset";

        // Ensure folder exists
        if (!AssetDatabase.IsValidFolder(folder))
        {
            string parent = "Assets/_Game/Data";
            if (!AssetDatabase.IsValidFolder(parent))
                AssetDatabase.CreateFolder("Assets/_Game", "Data");
            AssetDatabase.CreateFolder(parent, "Combat");
        }

        // Remove stale asset if present
        AssetDatabase.DeleteAsset(assetPath);

        var zone = ScriptableObject.CreateInstance<SpawnZoneSO>();
        zone.shape      = SpawnZoneShape.Rect;
        zone.rectX      = 2;
        zone.rectY      = 5;
        zone.rectWidth  = 4;
        zone.rectHeight = 6;

        AssetDatabase.CreateAsset(zone, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[8-O] SpawnZone_GauntStandard created at {assetPath} — " +
                  $"Rect({zone.rectX},{zone.rectY}) {zone.rectWidth}x{zone.rectHeight}");
    }
}
