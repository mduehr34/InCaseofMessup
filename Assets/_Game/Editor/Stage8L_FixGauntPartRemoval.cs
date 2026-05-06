using UnityEngine;
using UnityEditor;
using MnM.Core.Data;

public class Stage8L_FixGauntPartRemoval
{
    public static void Execute()
    {
        var gaunt = AssetDatabase.LoadAssetAtPath<MonsterSO>(
            "Assets/_Game/Data/Monsters/Monster_Gaunt.asset");
        if (gaunt == null) { Debug.LogError("[8L Fix] Monster_Gaunt not found"); return; }

        // Map each opening card removal to a part that makes thematic sense:
        //   Right Flank break  → removes Gaunt_CreepingAdvance  (already correct)
        //   Hind Legs  break   → removes Gaunt_LungeStrike       (was Gaunt_Lunge — wrong name)
        //   Tail       break   → removes Gaunt_DeadStillness     (was Gaunt_FlankSense — wrong name)
        // The other parts (Throat, Left Flank, Torso, Head) reference future ability cards
        // not yet in the deck — leave them as-is so they're no-ops until those cards exist.

        var parts = gaunt.standardParts;
        bool dirty = false;

        for (int i = 0; i < parts.Length; i++)
        {
            var p = parts[i];
            switch (p.partName)
            {
                case "Hind Legs":
                    p.breakRemovesCardNames = new[] { "Gaunt_LungeStrike" };
                    parts[i] = p;
                    dirty = true;
                    Debug.Log("[8L Fix] Hind Legs break → Gaunt_LungeStrike");
                    break;

                case "Tail":
                    p.breakRemovesCardNames = new[] { "Gaunt_DeadStillness" };
                    parts[i] = p;
                    dirty = true;
                    Debug.Log("[8L Fix] Tail break → Gaunt_DeadStillness");
                    break;
            }
        }

        if (dirty)
        {
            gaunt.standardParts = parts;
            EditorUtility.SetDirty(gaunt);
            AssetDatabase.SaveAssets();
            Debug.Log("[8L Fix] Done — breaking Right Flank, Hind Legs, and Tail will now " +
                      "remove all 3 opening cards and unlock Sweeping Flail.");
        }
        else
        {
            Debug.Log("[8L Fix] No changes needed.");
        }
    }
}
