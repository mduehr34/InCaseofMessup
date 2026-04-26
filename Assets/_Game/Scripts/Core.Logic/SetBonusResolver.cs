using System.Collections.Generic;
using UnityEngine;
using MnM.Core.Data;

namespace MnM.Core.Logic
{
    public static class SetBonusResolver
    {
        // Returns total stat mods from all active set bonuses in this loadout.
        // activeEffectTags receives non-stat effect tags for use by CombatManager.
        public static StatModifiers ResolveSetBonuses(ItemSO[] equippedItems, out string[] activeEffectTags)
        {
            var totals     = new StatModifiers();
            var effectTags = new List<string>();

            if (equippedItems == null || equippedItems.Length == 0)
            {
                activeEffectTags = new string[0];
                return totals;
            }

            // Group by setNameTag
            var groups = new Dictionary<string, List<ItemSO>>();
            foreach (var item in equippedItems)
            {
                if (item == null || string.IsNullOrEmpty(item.setNameTag)) continue;
                if (!groups.ContainsKey(item.setNameTag))
                    groups[item.setNameTag] = new List<ItemSO>();
                groups[item.setNameTag].Add(item);
            }

            foreach (var kvp in groups)
            {
                string setTag      = kvp.Key;
                var    setItems    = kvp.Value;
                int    pieceCount  = setItems.Count;

                // Find the anchor piece — the one with setBonuses populated
                ItemSO anchor = null;
                foreach (var item in setItems)
                {
                    if (item.setBonuses != null && item.setBonuses.Length > 0)
                    {
                        anchor = item;
                        break;
                    }
                }

                if (anchor == null) continue;

                foreach (var entry in anchor.setBonuses)
                {
                    if (pieceCount < entry.requiredPieceCount) continue;

                    totals.accuracy  += entry.bonusAccuracy;
                    totals.strength  += entry.bonusStrength;
                    totals.toughness += entry.bonusToughness;
                    totals.evasion   += entry.bonusEvasion;
                    totals.luck      += entry.bonusLuck;
                    totals.movement  += entry.bonusMovement;

                    if (!string.IsNullOrEmpty(entry.effectTag))
                        effectTags.Add(entry.effectTag);

                    Debug.Log($"[SetBonus] {setTag} {entry.requiredPieceCount}-piece active " +
                              $"({pieceCount} pieces equipped)" +
                              (string.IsNullOrEmpty(entry.effectTag) ? "" : $" — tag: {entry.effectTag}"));
                }
            }

            activeEffectTags = effectTags.ToArray();
            return totals;
        }
    }
}
