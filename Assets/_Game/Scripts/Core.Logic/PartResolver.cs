using System.Collections.Generic;
using UnityEngine;
using MnM.Core.Data;

namespace MnM.Core.Logic
{
    /// <summary>
    /// Pure-calculation layer for the Shell/Flesh damage pipeline.
    /// Returns a PartDamageResult — callers in Core.Systems are responsible
    /// for acting on removedCardNames (calling ai.RemoveCard for each entry).
    /// No dependency on Core.Systems — keeps Core.Logic acyclic.
    /// </summary>
    public static class PartResolver
    {
        // ── Main Entry Point ─────────────────────────────────────
        public static PartDamageResult ApplyDamage(
            ref MonsterPartState part,
            int damageAmount,
            DamageType type,
            MonsterSO monsterData)
        {
            return type == DamageType.Shell
                ? ApplyShellDamage(ref part, damageAmount, monsterData)
                : ApplyFleshDamage(ref part, damageAmount, monsterData);
        }

        // ── Shell Damage ─────────────────────────────────────────
        private static PartDamageResult ApplyShellDamage(
            ref MonsterPartState part,
            int damageAmount,
            MonsterSO monsterData)
        {
            var result = new PartDamageResult { removedCardNames = new List<string>() };

            if (part.isBroken)
            {
                Debug.Log($"[Part] {part.partName} already broken — Shell damage ignored");
                return result;
            }

            int prev = part.shellCurrent;
            part.shellCurrent = Mathf.Max(0, part.shellCurrent - damageAmount);
            Debug.Log($"[Part] {part.partName} Shell: {prev} → {part.shellCurrent}/{part.shellMax}");

            // Part breaks when Shell reaches 0 for the first time
            if (part.shellCurrent == 0 && prev > 0)
            {
                part.isBroken = true;
                Debug.Log($"[Part] *** {part.partName} BROKEN ***");
                result.partBreakOccurred = true;

                var partData = FindPartData(monsterData, part.partName);
                if (partData.HasValue)
                {
                    foreach (var cardName in partData.Value.breakRemovesCardNames ?? new string[0])
                    {
                        if (!string.IsNullOrEmpty(cardName))
                        {
                            result.removedCardNames.Add(cardName);
                            Debug.Log($"[Part] Break queues removal: \"{cardName}\"");
                        }
                    }
                }
            }

            return result;
        }

        // ── Flesh Damage ─────────────────────────────────────────
        private static PartDamageResult ApplyFleshDamage(
            ref MonsterPartState part,
            int damageAmount,
            MonsterSO monsterData)
        {
            var result = new PartDamageResult { removedCardNames = new List<string>() };

            int prev = part.fleshCurrent;
            part.fleshCurrent = Mathf.Max(0, part.fleshCurrent - damageAmount);
            Debug.Log($"[Part] {part.partName} Flesh: {prev} → {part.fleshCurrent}/{part.fleshMax}");

            if (part.fleshCurrent >= prev) return result; // no damage landed

            result.woundOccurred = true;
            part.woundCount++;

            var partData = FindPartData(monsterData, part.partName);
            if (partData.HasValue)
            {
                var woundRemovals = partData.Value.woundRemovesCardNames ?? new string[0];
                int woundIndex = part.woundCount - 1; // 0-based

                if (woundIndex < woundRemovals.Length)
                {
                    string cardName = woundRemovals[woundIndex];
                    if (!string.IsNullOrEmpty(cardName))
                    {
                        result.removedCardNames.Add(cardName);
                        Debug.Log($"[Part] Wound #{part.woundCount} queues removal: \"{cardName}\"");
                    }
                }
            }

            return result;
        }

        // ── Part Data Lookup ─────────────────────────────────────
        // Searches all three difficulty arrays — part names are consistent across difficulties
        private static MonsterBodyPart? FindPartData(MonsterSO monsterData, string partName)
        {
            foreach (var p in monsterData.standardParts ?? new MonsterBodyPart[0])
                if (p.partName == partName) return p;
            foreach (var p in monsterData.hardenedParts ?? new MonsterBodyPart[0])
                if (p.partName == partName) return p;
            foreach (var p in monsterData.apexParts ?? new MonsterBodyPart[0])
                if (p.partName == partName) return p;

            Debug.LogWarning($"[PartResolver] Part \"{partName}\" not found in MonsterSO");
            return null;
        }
    }

    // ── Result ───────────────────────────────────────────────────
    public struct PartDamageResult
    {
        public bool partBreakOccurred;
        public bool woundOccurred;
        /// <summary>
        /// Card names to remove. Caller must call IMonsterAI.RemoveCard() for each.
        /// </summary>
        public List<string> removedCardNames;
    }
}
