using UnityEngine;
using UnityEditor;
using MnM.Core.Data;

public class CreateWeaponRegistry
{
    public static void Execute()
    {
        // Ensure destination folder exists
        const string resourcesPath = "Assets/_Game/Data/Resources";
        if (!AssetDatabase.IsValidFolder(resourcesPath))
        {
            AssetDatabase.CreateFolder("Assets/_Game/Data", "Resources");
            Debug.Log("[WeaponRegistry] Created Resources folder");
        }

        const string assetPath = resourcesPath + "/WeaponRegistry.asset";

        // Load or create the registry SO
        var registry = AssetDatabase.LoadAssetAtPath<WeaponRegistrySO>(assetPath);
        if (registry == null)
        {
            registry = ScriptableObject.CreateInstance<WeaponRegistrySO>();
            AssetDatabase.CreateAsset(registry, assetPath);
            Debug.Log("[WeaponRegistry] Created WeaponRegistry.asset");
        }
        else
        {
            Debug.Log("[WeaponRegistry] WeaponRegistry.asset already exists — updating weapons list");
        }

        // Find all WeaponSO assets in the project
        var guids  = AssetDatabase.FindAssets("t:WeaponSO");
        var weapons = new WeaponSO[guids.Length];
        for (int i = 0; i < guids.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[i]);
            weapons[i] = AssetDatabase.LoadAssetAtPath<WeaponSO>(path);
            Debug.Log($"[WeaponRegistry] Found weapon: {weapons[i]?.weaponName} ({weapons[i]?.weaponType}) at {path}");
        }

        registry.weapons = weapons;
        EditorUtility.SetDirty(registry);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[WeaponRegistry] Done — registered {weapons.Length} weapon(s).");
    }
}
