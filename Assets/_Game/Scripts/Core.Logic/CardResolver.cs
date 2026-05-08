using System.Collections.Generic;
using UnityEngine;
using MnM.Core.Data;

namespace MnM.Core.Logic
{
    /// <summary>
    /// Pure-calculation layer for the full action card resolution pipeline.
    /// Returns a CardResolutionResult — callers in Core.Systems are responsible
    /// for acting on removedCardNames (calling ai.RemoveCard for each entry)
    /// and for triggering Apex via IMonsterAI.TriggerApex().
    /// No dependency on Core.Systems — keeps Core.Logic acyclic.
    /// </summary>
    public static class CardResolver
    {
        // ── Main Resolution Entry Point ──────────────────────────
        public static CardResolutionResult Resolve(
            ActionCardSO card,
            HunterCombatState attacker,
            MonsterCombatState monster,
            ref MonsterPartState targetPart,
            MonsterSO monsterData,
            bool firstPartBreakOccurredThisCombat)
        {
            var result = new CardResolutionResult
            {
                cardName         = card.cardName,
                removedCardNames = new List<string>(),
            };

            Debug.Log($"[Card] Resolving: \"{card.cardName}\" by {attacker.hunterName} " +
                      $"targeting {targetPart.partName} | AP:{attacker.apRemaining} " +
                      $"Cost:{card.apCost} Refund:{card.apRefund} Loud:{card.isLoud}");

            // ── Step 1: Loud Flag ────────────────────────────────
            if (card.isLoud)
            {
                result.wasLoud = true;
                Debug.Log("[Card] LOUD card played — MonsterAI reactive triggers may fire");
                // MonsterAI checks this flag via EvaluateTrigger in Stage 3-D
            }

            // ── Step 2: Reaction Cards ───────────────────────────
            if (card.category == CardCategory.Reaction)
            {
                Debug.Log($"[Card] REACTION: {card.cardName} — {card.effectDescription}");
                result.reactionApplied   = true;
                attacker.apRemaining    -= (card.apCost - card.apRefund);
                return result;
            }

            // ── Step 3: Precision Check ──────────────────────────
            int effectiveAccuracy = attacker.accuracy;
            int effectiveMovement = attacker.movement;
            StatusEffectResolver.ApplyStatusPenalties(
                attacker, ref effectiveAccuracy, ref effectiveMovement);

            bool hasWeakness   = HasElementMatch(card, monsterData.weaknesses);
            bool hasResistance = HasElementMatch(card, monsterData.resistances);

            var precision = DiceResolver.ResolvePrecision(
                effectiveAccuracy,
                GetMonsterEvasion(monster, monsterData),
                attacker.luck,
                hasWeakness,
                hasResistance);

            StatusEffectResolver.TickAfterAction(ref attacker.activeStatusEffects, attacker);

            if (!precision.isHit)
            {
                result.wasMiss          = true;
                Debug.Log($"[Card] MISS — {card.cardName}");
                result.apRefundGranted  = card.apRefund;
                attacker.apRemaining   -= (card.apCost - card.apRefund);
                return result;
            }

            result.isCritical = precision.isCritical;

            // ── Step 4: Wound Resolution (Stage 8-M stub) ────────
            // Monster shell/flesh body parts removed. Wound resolution now uses
            // WoundLocationSO deck draws — full pipeline implemented in Stage 8-N.
            // Hit registered; damage dealt via wound deck in CombatManager.ResolveWound.
            result.damageDealt = precision.isCritical ? 2 : 1; // placeholder count for logging
            Debug.Log($"[Card] HIT on monster — wound deck resolution in Stage 8-N " +
                      $"(isCritical:{precision.isCritical})");

            // ── Step 5: AP Management ────────────────────────────
            result.apRefundGranted  = card.apRefund;
            attacker.apRemaining   -= (card.apCost - card.apRefund);

            Debug.Log($"[Card] Resolution complete. Hit:{result.damageDealt > 0} Critical:{result.isCritical} " +
                      $"AP remaining:{attacker.apRemaining}");

            return result;
        }

        // ── Helpers ──────────────────────────────────────────────
        // Element matching wired to weapon SO in Stage 7 — returns false for now
        private static bool HasElementMatch(ActionCardSO card, ElementTag[] monsterElements) => false;

        private static int GetMonsterEvasion(MonsterCombatState monster, MonsterSO monsterData)
        {
            int i = DiffIndex(monster.difficulty);
            if (monsterData.statBlocks == null || i >= monsterData.statBlocks.Length) return 2;
            return monsterData.statBlocks[i].evasion;
        }

        private static int GetMonsterToughness(MonsterCombatState monster, MonsterSO monsterData)
        {
            int i = DiffIndex(monster.difficulty);
            if (monsterData.statBlocks == null || i >= monsterData.statBlocks.Length) return 1;
            return monsterData.statBlocks[i].toughness;
        }

        private static int DiffIndex(string difficulty) => difficulty switch
        {
            "Hardened" => 1,
            "Apex"     => 2,
            _          => 0,
        };
    }

    // ── Result ───────────────────────────────────────────────────
    public struct CardResolutionResult
    {
        public string       cardName;
        public int          damageDealt;    // Placeholder count; real wound resolved via WoundDeck in 8-N
        public int          apRefundGranted;
        public bool         reactionApplied;
        public bool         wasLoud;
        public bool         wasMiss;
        public bool         isCritical;
        public bool         apexShouldTrigger;
        public List<string> removedCardNames;
    }
}
