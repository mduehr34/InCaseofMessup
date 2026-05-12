using UnityEngine;
using UnityEditor;
using MnM.Core.Data;

public class CreateGauntWoundDeck
{
    public static void Execute()
    {
        string folder = "Assets/_Game/Data/Cards/WoundLocations/Gaunt";
        if (!AssetDatabase.IsValidFolder("Assets/_Game/Data/Cards/WoundLocations"))
            AssetDatabase.CreateFolder("Assets/_Game/Data/Cards", "WoundLocations");
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/_Game/Data/Cards/WoundLocations", "Gaunt");

        // ── Create wound location SOs ──────────────────────────────────────────
        var head = Make(folder, "Gaunt_WL_Head",
            locationName: "Head",
            partTag:       BodyPartTag.Head,
            woundTarget:   7,
            isTrap:        false,
            isImpervious:  false,
            failureEffect: "The blow glances off the Gaunt's skull — no effect.",
            woundEffect:   "Skull strike — Gaunt loses its next free step.",
            criticalEffect: "Eye burst — Gaunt is Disoriented; -2 to all Accuracy checks until round end.",
            critWoundTag:  "GauntHead_Critical",
            woundRes:      new[] { new ResourceEntry { resourceName = "Bone", amount = 1 } },
            critRes:       new[] { new ResourceEntry { resourceName = "Bone", amount = 2 } });

        var throat = Make(folder, "Gaunt_WL_Throat",
            locationName: "Throat",
            partTag:       BodyPartTag.Torso,
            woundTarget:   6,
            isTrap:        false,
            isImpervious:  false,
            failureEffect: "Claws deflect the strike — the Gaunt snarls but is unharmed.",
            woundEffect:   "Throat tear — Gaunt loses 1 AP on its next turn.",
            criticalEffect: "Windpipe crush — Gaunt is Stunned for 1 round.",
            critWoundTag:  "GauntThroat_Critical",
            woundRes:      new[] { new ResourceEntry { resourceName = "Hide", amount = 1 } },
            critRes:       new[] { new ResourceEntry { resourceName = "Hide", amount = 1 },
                                   new ResourceEntry { resourceName = "Bone", amount = 1 } });

        var torso = Make(folder, "Gaunt_WL_Torso",
            locationName: "Torso",
            partTag:       BodyPartTag.Torso,
            woundTarget:   5,
            isTrap:        false,
            isImpervious:  false,
            failureEffect: "The Gaunt absorbs the blow with its dense musculature.",
            woundEffect:   "Body wound — behavior card removed from deck.",
            criticalEffect: "Organ rupture — behavior card removed; Gaunt loses 1 AP next turn.",
            critWoundTag:  "GauntTorso_Critical",
            woundRes:      new[] { new ResourceEntry { resourceName = "Organ", amount = 1 } },
            critRes:       new[] { new ResourceEntry { resourceName = "Organ", amount = 2 } });

        var leftFlank = Make(folder, "Gaunt_WL_LeftFlank",
            locationName: "Left Flank",
            partTag:       BodyPartTag.Torso,
            woundTarget:   5,
            isTrap:        false,
            isImpervious:  false,
            failureEffect: "The Gaunt twists away — the strike finds only hide.",
            woundEffect:   "Flank gash — behavior card removed; light bleeding.",
            criticalEffect: "Deep flank wound — behavior card removed; Gaunt movement -1 until end of round.",
            critWoundTag:  "GauntFlank_Critical",
            woundRes:      new[] { new ResourceEntry { resourceName = "Hide", amount = 2 } },
            critRes:       new[] { new ResourceEntry { resourceName = "Hide", amount = 2 },
                                   new ResourceEntry { resourceName = "Organ", amount = 1 } });

        var rightFlank = Make(folder, "Gaunt_WL_RightFlank",
            locationName: "Right Flank",
            partTag:       BodyPartTag.Torso,
            woundTarget:   5,
            isTrap:        false,
            isImpervious:  false,
            failureEffect: "Hide deflects — the Gaunt shrugs the hit.",
            woundEffect:   "Flank gash — behavior card removed; light bleeding.",
            criticalEffect: "Deep flank wound — behavior card removed; Gaunt movement -1 until end of round.",
            critWoundTag:  "GauntFlank_Critical",
            woundRes:      new[] { new ResourceEntry { resourceName = "Hide", amount = 2 } },
            critRes:       new[] { new ResourceEntry { resourceName = "Hide", amount = 2 },
                                   new ResourceEntry { resourceName = "Organ", amount = 1 } });

        var hindLegs = Make(folder, "Gaunt_WL_HindLegs",
            locationName: "Hind Legs",
            partTag:       BodyPartTag.Legs,
            woundTarget:   4,
            isTrap:        false,
            isImpervious:  false,
            failureEffect: "The Gaunt kicks free before the strike lands.",
            woundEffect:   "Hamstring — Gaunt movement reduced by 2 for 1 round.",
            criticalEffect: "Leg break — Gaunt movement halved; cannot charge this round.",
            critWoundTag:  "GauntLegs_Critical",
            woundRes:      new[] { new ResourceEntry { resourceName = "Bone", amount = 1 } },
            critRes:       new[] { new ResourceEntry { resourceName = "Bone", amount = 2 } });

        var tail = Make(folder, "Gaunt_WL_Tail",
            locationName: "Tail",
            partTag:       BodyPartTag.Legs,
            woundTarget:   4,
            isTrap:        false,
            isImpervious:  false,
            failureEffect: "The tail whips away — no contact made.",
            woundEffect:   "Tail wound — behavior card removed.",
            criticalEffect: "Tail severed — behavior card removed; Gaunt loses balance reaction.",
            critWoundTag:  "GauntTail_Critical",
            woundRes:      new[] { new ResourceEntry { resourceName = "Bone", amount = 1 } },
            critRes:       new[] { new ResourceEntry { resourceName = "Bone", amount = 1 },
                                   new ResourceEntry { resourceName = "Hide", amount = 1 } });

        AssetDatabase.SaveAssets();
        Debug.Log("[WoundDeck] Created 7 Gaunt wound location assets");

        // ── Wire standard wound deck on Monster_Gaunt (12 cards) ──────────────
        // Torso x3, Flanks x2 each, Hind Legs x2, Head x1, Throat x1, Tail x1
        var gaunt = AssetDatabase.LoadAssetAtPath<MonsterSO>("Assets/_Game/Data/Monsters/Monster_Gaunt.asset");
        if (gaunt == null) { Debug.LogError("[WoundDeck] Monster_Gaunt.asset not found"); return; }

        var deck = new WoundLocationSO[]
        {
            torso, torso, torso,
            leftFlank, leftFlank,
            rightFlank, rightFlank,
            hindLegs, hindLegs,
            head,
            throat,
            tail,
        };

        var so = new SerializedObject(gaunt);
        var prop = so.FindProperty("standardWoundDeck");
        prop.arraySize = deck.Length;
        for (int i = 0; i < deck.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = deck[i];
        so.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();

        Debug.Log($"[WoundDeck] Monster_Gaunt standardWoundDeck set — {deck.Length} cards " +
                  "(Torso x3, Flanks x2, HindLegs x2, Head x1, Throat x1, Tail x1)");
    }

    private static WoundLocationSO Make(
        string folder, string assetName,
        string locationName, BodyPartTag partTag,
        int woundTarget, bool isTrap, bool isImpervious,
        string failureEffect, string woundEffect, string criticalEffect,
        string critWoundTag,
        ResourceEntry[] woundRes, ResourceEntry[] critRes)
    {
        string path = $"{folder}/{assetName}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<WoundLocationSO>(path);
        if (existing != null) return existing;

        var so = ScriptableObject.CreateInstance<WoundLocationSO>();
        so.locationName    = locationName;
        so.partTag         = partTag;
        so.woundTarget     = woundTarget;
        so.isTrap          = isTrap;
        so.isImpervious    = isImpervious;
        so.failureEffect   = failureEffect;
        so.woundEffect     = woundEffect;
        so.criticalEffect  = criticalEffect;
        so.criticalWoundTag = critWoundTag;
        so.woundResources  = woundRes;
        so.criticalResources = critRes;

        AssetDatabase.CreateAsset(so, path);
        return so;
    }
}
