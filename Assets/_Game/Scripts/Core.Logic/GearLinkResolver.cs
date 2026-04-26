using System.Collections.Generic;
using UnityEngine;
using MnM.Core.Data;

namespace MnM.Core.Logic
{
    // Item + its top-left anchor cell on the gear grid (Y-down screen-space coords)
    public struct GearGridSlot
    {
        public ItemSO item;
        public Vector2Int cell;
    }

    public static class GearLinkResolver
    {
        // Returns all active link bonuses for a positioned loadout.
        // Direction vectors on LinkPoint use Y-down screen-space (0,+1 = below, 0,-1 = above).
        public static LinkBonus[] ResolveLinks(GearGridSlot[] loadout)
        {
            if (loadout == null || loadout.Length == 0)
                return new LinkBonus[0];

            var bonuses = new List<LinkBonus>();
            // Dedup: "itemName_statField" — prevents dual-link items from counting a stat twice
            var appliedStatKeys = new HashSet<string>();
            // Dedup: directional pair already processed
            var processedPairs = new HashSet<string>();

            for (int i = 0; i < loadout.Length; i++)
            {
                var slotA = loadout[i];
                if (slotA.item == null || slotA.item.linkPoints == null) continue;
                if (slotA.item.isConsumable) continue;

                foreach (var linkPt in slotA.item.linkPoints)
                {
                    if (string.IsNullOrEmpty(linkPt.affinityTag)) continue;

                    var neighborCell = slotA.cell + linkPt.direction;

                    // Find item B whose footprint contains neighborCell
                    for (int j = 0; j < loadout.Length; j++)
                    {
                        if (i == j) continue;
                        var slotB = loadout[j];
                        if (slotB.item == null || slotB.item.isConsumable) continue;

                        // Check if neighborCell falls within B's footprint
                        var dims = slotB.item.gridDimensions;
                        bool inFootprint =
                            neighborCell.x >= slotB.cell.x &&
                            neighborCell.x <= slotB.cell.x + dims.x - 1 &&
                            neighborCell.y >= slotB.cell.y &&
                            neighborCell.y <= slotB.cell.y + dims.y - 1;

                        if (!inFootprint) continue;
                        if (slotB.item.affinityTags == null) continue;
                        if (System.Array.IndexOf(slotB.item.affinityTags, linkPt.affinityTag) < 0) continue;

                        // Directional pair key prevents processing A→B and then B→A as two links
                        string pairKey = $"{slotA.item.itemName}|{slotB.item.itemName}|{linkPt.direction}";
                        if (processedPairs.Contains(pairKey)) continue;
                        processedPairs.Add(pairKey);

                        // Accumulate stat delta, deduplicating per-item stat bonuses
                        var delta = new StatModifiers();
                        AccumulateStat(slotA.item.itemName, "acc",  linkPt.bonusAccuracy,  ref delta.accuracy,  appliedStatKeys);
                        AccumulateStat(slotA.item.itemName, "str",  linkPt.bonusStrength,  ref delta.strength,  appliedStatKeys);
                        AccumulateStat(slotA.item.itemName, "tgh",  linkPt.bonusToughness, ref delta.toughness, appliedStatKeys);
                        AccumulateStat(slotA.item.itemName, "eva",  linkPt.bonusEvasion,   ref delta.evasion,   appliedStatKeys);
                        AccumulateStat(slotA.item.itemName, "lck",  linkPt.bonusLuck,      ref delta.luck,      appliedStatKeys);
                        AccumulateStat(slotA.item.itemName, "mov",  linkPt.bonusMovement,  ref delta.movement,  appliedStatKeys);

                        string description = $"{slotA.item.itemName} \u2194 {slotB.item.itemName}: " +
                                             $"{linkPt.affinityTag} link active";
                        bonuses.Add(new LinkBonus
                        {
                            itemAName         = slotA.item.itemName,
                            itemBName         = slotB.item.itemName,
                            affinityTag       = linkPt.affinityTag,
                            effectDescription = description,
                            delta             = delta,
                        });
                        Debug.Log($"[GearLink] Link active: {description}");
                    }
                }
            }

            if (bonuses.Count == 0)
                Debug.Log("[GearLink] No active links in current loadout");

            return bonuses.ToArray();
        }

        // Sums base item stats plus all active link deltas.
        public static StatModifiers SumEquippedStats(GearGridSlot[] loadout)
        {
            var totals = new StatModifiers();
            if (loadout == null) return totals;

            foreach (var slot in loadout)
            {
                if (slot.item == null) continue;
                totals.accuracy  += slot.item.accuracyMod;
                totals.strength  += slot.item.strengthMod;
                totals.toughness += slot.item.toughnessMod;
                totals.evasion   += slot.item.evasionMod;
                totals.luck      += slot.item.luckMod;
                totals.movement  += slot.item.movementMod;
            }

            foreach (var link in ResolveLinks(loadout))
            {
                totals.accuracy  += link.delta.accuracy;
                totals.strength  += link.delta.strength;
                totals.toughness += link.delta.toughness;
                totals.evasion   += link.delta.evasion;
                totals.luck      += link.delta.luck;
                totals.movement  += link.delta.movement;
            }

            Debug.Log($"[GearLink] Stat totals (base+links) — " +
                      $"Acc:{totals.accuracy} Str:{totals.strength} " +
                      $"Tgh:{totals.toughness} Eva:{totals.evasion} " +
                      $"Lck:{totals.luck} Mov:{totals.movement}");
            return totals;
        }

        private static void AccumulateStat(string itemName, string statKey, int value,
                                           ref int target, HashSet<string> applied)
        {
            if (value == 0) return;
            string key = $"{itemName}_{statKey}";
            if (applied.Contains(key)) return;
            applied.Add(key);
            target += value;
        }
    }

    public struct LinkBonus
    {
        public string itemAName;
        public string itemBName;
        public string affinityTag;
        public string effectDescription;
        public StatModifiers delta;
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
