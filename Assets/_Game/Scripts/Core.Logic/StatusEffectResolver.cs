using System.Collections.Generic;
using UnityEngine;
using MnM.Core.Data;

namespace MnM.Core.Logic
{
    public static class StatusEffectResolver
    {
        // ── Apply / Remove / Query ───────────────────────────────
        public static void Apply(ref string[] statusEffects, StatusEffect effect)
        {
            var list = new List<string>(statusEffects);
            string tag = effect.ToString();
            if (!list.Contains(tag))
            {
                list.Add(tag);
                Debug.Log($"[Status] Applied: {tag}");
            }
            statusEffects = list.ToArray();
        }

        public static void Remove(ref string[] statusEffects, StatusEffect effect)
        {
            var list = new List<string>(statusEffects);
            if (list.Remove(effect.ToString()))
                Debug.Log($"[Status] Removed: {effect}");
            statusEffects = list.ToArray();
        }

        public static bool Has(string[] statusEffects, StatusEffect effect) =>
            statusEffects != null &&
            System.Array.IndexOf(statusEffects, effect.ToString()) >= 0;

        // ── Per-Effect Rules ─────────────────────────────────────
        // Called at start of each hunter's action to apply status penalties
        public static void ApplyStatusPenalties(HunterCombatState hunter, ref int accuracyMod, ref int movementMod)
        {
            if (Has(hunter.activeStatusEffects, StatusEffect.Shaken))
            {
                accuracyMod -= 1;
                Debug.Log($"[Status] {hunter.hunterName} Shaken: -1 Accuracy this action");
            }
            if (Has(hunter.activeStatusEffects, StatusEffect.Slowed))
            {
                movementMod = Mathf.FloorToInt(movementMod * 0.5f);
                Debug.Log($"[Status] {hunter.hunterName} Slowed: movement halved");
            }
        }

        // Auto-remove statuses that expire after one use
        public static void TickAfterAction(ref string[] statusEffects, HunterCombatState hunter)
        {
            // Shaken: auto-removes after one action
            if (Has(statusEffects, StatusEffect.Shaken))
            {
                Remove(ref statusEffects, StatusEffect.Shaken);
                Debug.Log($"[Status] {hunter.hunterName} Shaken expired after action");
            }
            // Slowed: auto-removes at end of turn
            if (Has(statusEffects, StatusEffect.Slowed))
            {
                Remove(ref statusEffects, StatusEffect.Slowed);
                Debug.Log($"[Status] {hunter.hunterName} Slowed expired after action");
            }
        }

        // Bleeding: called during Vitality Phase — lose 1 Flesh
        // Returns true if Flesh damage was applied
        public static bool TickBleeding(ref string[] statusEffects, ref BodyZoneState torsoZone, string hunterName)
        {
            if (!Has(statusEffects, StatusEffect.Bleeding)) return false;
            torsoZone.fleshCurrent = Mathf.Max(0, torsoZone.fleshCurrent - 1);
            Debug.Log($"[Status] {hunterName} Bleeding: -1 Flesh to Torso. " +
                      $"Torso Flesh: {torsoZone.fleshCurrent}/{torsoZone.fleshMax}");
            return true;
        }
    }
}
