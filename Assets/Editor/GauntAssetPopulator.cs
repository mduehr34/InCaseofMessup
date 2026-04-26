using UnityEngine;
using UnityEditor;
using MnM.Core.Data;

// One-shot editor utility — run via CoPlay, delete when done.
public class GauntAssetPopulator
{
    public static void Execute()
    {
        PopulateArmor();
        PopulateSetBonuses();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[GauntAssetPopulator] Done. All Gaunt armor link/set-bonus data updated.");
    }

    // ── Armor Link Points ─────────────────────────────────────────
    // Direction vectors use Y-down screen-space:
    //   (0,-1) = above (toward lower row index)
    //   (0,+1) = below (toward higher row index)
    //   (+1,0) = right
    private static void PopulateArmor()
    {
        SetSkullCap();
        SetHideVest();
        SetSinewWrap();
        SetBoneBracers();
        SetHideBoots();
    }

    private static void SetSkullCap()
    {
        var item = Load("Item_GauntSkullCap");
        if (item == null) return;

        item.linkPoints = new[]
        {
            new LinkPoint
            {
                affinityTag    = "GAUNT",
                direction      = new Vector2Int(0, 1),  // looks below
                bonusAccuracy  = 1,
            }
        };

        EditorUtility.SetDirty(item);
        Debug.Log("[GauntAssetPopulator] SkullCap: direction(0,+1) bonusAccuracy:+1");
    }

    private static void SetHideVest()
    {
        var item = Load("Item_GauntHideVest");
        if (item == null) return;

        // Two link points: one toward SkullCap above, one toward SinewWrap/Boots below.
        // Each carries bonusToughness:+1; dedup in resolver caps the actual grant at +1 total.
        item.linkPoints = new[]
        {
            new LinkPoint
            {
                affinityTag    = "GAUNT",
                direction      = new Vector2Int(0, -1), // looks above
                bonusToughness = 1,
            },
            new LinkPoint
            {
                affinityTag    = "GAUNT",
                direction      = new Vector2Int(0, 1),  // looks below
                bonusToughness = 1,
            },
        };

        EditorUtility.SetDirty(item);
        Debug.Log("[GauntAssetPopulator] HideVest: direction(0,-1)+(0,+1) bonusToughness:+1 each");
    }

    private static void SetSinewWrap()
    {
        var item = Load("Item_GauntSinewWrap");
        if (item == null) return;

        item.linkPoints = new[]
        {
            new LinkPoint
            {
                affinityTag   = "GAUNT",
                direction     = new Vector2Int(0, -1), // looks above
                bonusMovement = 1,
            },
            new LinkPoint
            {
                affinityTag   = "GAUNT",
                direction     = new Vector2Int(0, 1),  // looks below
                bonusMovement = 1,
            },
        };

        EditorUtility.SetDirty(item);
        Debug.Log("[GauntAssetPopulator] SinewWrap: direction(0,-1)+(0,+1) bonusMovement:+1 each");
    }

    private static void SetBoneBracers()
    {
        var item = Load("Item_GauntBoneBracers");
        if (item == null) return;

        item.linkPoints = new[]
        {
            new LinkPoint
            {
                affinityTag    = "GAUNT",
                direction      = new Vector2Int(1, 0),  // looks right
                bonusStrength  = 1,
            }
        };

        EditorUtility.SetDirty(item);
        Debug.Log("[GauntAssetPopulator] BoneBracers: direction(+1,0) bonusStrength:+1");
    }

    private static void SetHideBoots()
    {
        var item = Load("Item_GauntHideBoots");
        if (item == null) return;

        item.linkPoints = new[]
        {
            new LinkPoint
            {
                affinityTag  = "GAUNT",
                direction     = new Vector2Int(0, -1), // looks above
                bonusEvasion  = 1,
            }
        };

        EditorUtility.SetDirty(item);
        Debug.Log("[GauntAssetPopulator] HideBoots: direction(0,-1) bonusEvasion:+1");
    }

    // ── Set Bonuses (anchor: HideVest) ────────────────────────────
    private static void PopulateSetBonuses()
    {
        var vest = Load("Item_GauntHideVest");
        if (vest == null) return;

        vest.setBonuses = new[]
        {
            new SetBonusEntry
            {
                requiredPieceCount = 2,
                bonusEvasion       = 1,
                effectTag          = "",
                effectDescription  = "+1 Evasion flat while 2+ GAUNT pieces equipped",
            },
            new SetBonusEntry
            {
                requiredPieceCount = 3,
                effectTag          = "GAUNT_3PC_LOUD_SUPPRESS",
                effectDescription  = "Behavior cards triggered by Loud card plays have movement effect -2 squares",
            },
            new SetBonusEntry
            {
                requiredPieceCount = 5,
                effectTag          = "GAUNT_5PC_DEATH_CHEAT",
                effectDescription  = "Once per hunt: when this hunter would collapse, survive with 1 Flesh on struck part",
            },
        };

        EditorUtility.SetDirty(vest);
        Debug.Log("[GauntAssetPopulator] HideVest setBonuses: 2-pc/3-pc/5-pc populated");
    }

    private static ItemSO Load(string assetName)
    {
        var guids = AssetDatabase.FindAssets($"t:ItemSO {assetName}");
        if (guids.Length == 0)
        {
            Debug.LogError($"[GauntAssetPopulator] Asset not found: {assetName}");
            return null;
        }
        return AssetDatabase.LoadAssetAtPath<ItemSO>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }
}
