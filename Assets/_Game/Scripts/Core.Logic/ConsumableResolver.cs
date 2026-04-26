using UnityEngine;
using MnM.Core.Data;

namespace MnM.Core.Logic
{
    public static class ConsumableResolver
    {
        // Returns true if hunter at (ax,ay) can target hunter at (bx,by) with a consumable.
        // Adjacency: Chebyshev distance ≤ 1 (includes diagonals).
        public static bool IsValidConsumableTarget(int ax, int ay, int bx, int by)
        {
            return Mathf.Abs(ax - bx) <= 1 && Mathf.Abs(ay - by) <= 1;
        }

        // Applies BoneSplint effect: +2 Shell to the named zone, capped at shellMax.
        // Caller is responsible for removing "Bone Splint" from RuntimeCharacterState.equippedItemNames.
        public static void ApplyBoneSplint(HunterCombatState target, string zoneName)
        {
            for (int i = 0; i < target.bodyZones.Length; i++)
            {
                if (target.bodyZones[i].zone != zoneName) continue;

                var zone = target.bodyZones[i];
                zone.shellCurrent = Mathf.Min(zone.shellCurrent + 2, zone.shellMax);
                target.bodyZones[i] = zone;
                Debug.Log($"[Consumable] BoneSplint: {target.hunterName} {zoneName} " +
                          $"shell restored to {zone.shellCurrent}");
                return;
            }
            Debug.LogWarning($"[Consumable] BoneSplint: zone '{zoneName}' not found on {target.hunterName}");
        }
    }
}
