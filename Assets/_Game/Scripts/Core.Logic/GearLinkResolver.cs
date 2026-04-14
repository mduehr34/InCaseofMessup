using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MnM.Core.Data;

namespace MnM.Core.Logic
{
    public static class GearLinkResolver
    {
        // Returns all active link bonuses for a given loadout
        // Called at equip time — no runtime cost during combat
        public static LinkBonus[] ResolveLinks(ItemSO[] equippedItems)
        {
            if (equippedItems == null || equippedItems.Length == 0)
                return new LinkBonus[0];

            var bonuses = new List<LinkBonus>();

            for (int i = 0; i < equippedItems.Length; i++)
            {
                var itemA = equippedItems[i];
                if (itemA == null || itemA.linkPoints == null) continue;

                foreach (var linkPoint in itemA.linkPoints)
                {
                    if (string.IsNullOrEmpty(linkPoint.affinityTag)) continue;

                    // Find another equipped item that shares this affinity tag
                    for (int j = 0; j < equippedItems.Length; j++)
                    {
                        if (i == j) continue;
                        var itemB = equippedItems[j];
                        if (itemB == null || itemB.affinityTags == null) continue;

                        if (System.Array.IndexOf(itemB.affinityTags, linkPoint.affinityTag) >= 0)
                        {
                            // Avoid duplicate bonuses (A↔B and B↔A)
                            bool alreadyLogged = bonuses.Any(b =>
                                (b.itemAName == itemA.itemName && b.itemBName == itemB.itemName) ||
                                (b.itemAName == itemB.itemName && b.itemBName == itemA.itemName));

                            if (!alreadyLogged)
                            {
                                var bonus = new LinkBonus
                                {
                                    itemAName         = itemA.itemName,
                                    itemBName         = itemB.itemName,
                                    affinityTag       = linkPoint.affinityTag,
                                    effectDescription = $"{itemA.itemName} ↔ {itemB.itemName}: " +
                                                        $"{linkPoint.affinityTag} link active",
                                };
                                bonuses.Add(bonus);
                                Debug.Log($"[GearLink] Link active: {bonus.effectDescription}");
                            }
                        }
                    }
                }
            }

            if (bonuses.Count == 0)
                Debug.Log("[GearLink] No active links in current loadout");

            return bonuses.ToArray();
        }

        // Returns stat modifier totals from all equipped items
        public static StatModifiers SumEquippedStats(ItemSO[] equippedItems)
        {
            var totals = new StatModifiers();
            if (equippedItems == null) return totals;

            foreach (var item in equippedItems)
            {
                if (item == null) continue;
                totals.accuracy  += item.accuracyMod;
                totals.strength  += item.strengthMod;
                totals.toughness += item.toughnessMod;
                totals.evasion   += item.evasionMod;
                totals.luck      += item.luckMod;
                totals.movement  += item.movementMod;
            }

            Debug.Log($"[GearLink] Stat totals from gear — " +
                      $"Acc:{totals.accuracy} Str:{totals.strength} " +
                      $"Tgh:{totals.toughness} Eva:{totals.evasion} " +
                      $"Lck:{totals.luck} Mov:{totals.movement}");
            return totals;
        }
    }

    public struct LinkBonus
    {
        public string itemAName;
        public string itemBName;
        public string affinityTag;
        public string effectDescription;
    }

    public struct StatModifiers
    {
        public int accuracy;
        public int strength;
        public int toughness;
        public int evasion;
        public int luck;
        public int movement;
    }
}
