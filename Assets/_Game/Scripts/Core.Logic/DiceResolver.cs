using UnityEngine;
using MnM.Core.Data;

namespace MnM.Core.Logic
{
    public static class DiceResolver
    {
        // ── Precision Check ──────────────────────────────────────
        // d10 + attackerAccuracy vs targetEvasion
        public static PrecisionResult ResolvePrecision(
            int attackerAccuracy,
            int targetEvasion,
            int luckModifier,
            bool hasElementWeakness,
            bool hasElementResistance)
        {
            int roll          = Random.Range(1, 11);   // d10: 1–10 inclusive
            int critThreshold = 10 - luckModifier;     // Luck 1 = crit on 9+, Luck 2 = crit on 8+

            int effectiveRoll = roll;

            // Bonus die on weakness: roll again, take higher
            if (hasElementWeakness)
            {
                int bonusRoll = Random.Range(1, 11);
                int prev      = effectiveRoll;
                effectiveRoll = Mathf.Max(effectiveRoll, bonusRoll);
                Debug.Log($"[d10] Element weakness bonus die: {bonusRoll} (kept {effectiveRoll}, discarded {prev})");
            }

            // Penalty die on resistance: roll again, take lower
            if (hasElementResistance)
            {
                int penaltyRoll = Random.Range(1, 11);
                int prev        = effectiveRoll;
                effectiveRoll   = Mathf.Min(effectiveRoll, penaltyRoll);
                Debug.Log($"[d10] Element resistance penalty die: {penaltyRoll} (kept {effectiveRoll}, discarded {prev})");
            }

            int  total  = effectiveRoll + attackerAccuracy;
            bool isHit  = total >= targetEvasion;
            bool isCrit = effectiveRoll >= critThreshold;

            Debug.Log($"[d10 Precision] Roll:{roll} effective:{effectiveRoll} +Acc:{attackerAccuracy} " +
                      $"= {total} vs Evasion:{targetEvasion} | " +
                      $"CritThreshold:{critThreshold} | " +
                      $"Result:{(isCrit ? "CRIT" : isHit ? "HIT" : "MISS")}");

            return new PrecisionResult
            {
                isHit      = isHit,
                isCritical = isCrit,
                rawRoll    = roll,
                total      = total,
            };
        }

        // ── Force Check ──────────────────────────────────────────
        // d10 + attackerStrength vs targetToughness
        public static ForceResult ResolveForce(
            int attackerStrength,
            int targetToughness,
            bool targetExposed,
            bool targetShellIsZero)
        {
            // Exposed part: Force Check auto-passes — no roll needed
            if (targetExposed)
            {
                Debug.Log("[d10 Force] AUTO-PASS — part is Exposed. Result: WOUND (Flesh)");
                return new ForceResult { isWound = true, rawRoll = 0, total = 0, wasAutoPass = true };
            }

            int  roll    = Random.Range(1, 11);
            int  total   = roll + attackerStrength;
            bool isWound = total > targetToughness;

            Debug.Log($"[d10 Force] Roll:{roll} +Str:{attackerStrength} = {total} " +
                      $"vs Toughness:{targetToughness} | " +
                      $"Result:{(isWound ? "WOUND (Flesh)" : "SHELL HIT")}");

            return new ForceResult { isWound = isWound, rawRoll = roll, total = total, wasAutoPass = false };
        }
    }

    public struct PrecisionResult
    {
        public bool isHit;
        public bool isCritical;
        public int  rawRoll;
        public int  total;
    }

    public struct ForceResult
    {
        public bool isWound;
        public int  rawRoll;
        public int  total;
        public bool wasAutoPass;
    }
}
