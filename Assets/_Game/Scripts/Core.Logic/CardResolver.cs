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

            // ── Step 4: Shell or Flesh? ──────────────────────────
            bool shellDepleted = targetPart.shellCurrent == 0;
            bool goesToFlesh   = shellDepleted || precision.isCritical;

            if (goesToFlesh)
            {
                var force = DiceResolver.ResolveForce(
                    attacker.strength,
                    GetMonsterToughness(monster, monsterData),
                    targetPart.isExposed,
                    shellDepleted);

                if (force.isWound)
                {
                    int fleshDamage = CalculateFleshDamage(attacker, precision.isCritical);
                    var partResult  = PartResolver.ApplyDamage(
                        ref targetPart, fleshDamage, DamageType.Flesh, monsterData);

                    result.damageDealt = fleshDamage;
                    result.damageType  = DamageType.Flesh;
                    result.removedCardNames.AddRange(partResult.removedCardNames);

                    Debug.Log($"[Card] FLESH WOUND — {fleshDamage} Flesh to {targetPart.partName}");
                }
                else
                {
                    Debug.Log("[Card] Force Check failed — no flesh damage");
                }
            }
            else
            {
                int shellDamage = CalculateShellDamage();
                var partResult  = PartResolver.ApplyDamage(
                    ref targetPart, shellDamage, DamageType.Shell, monsterData);

                result.damageDealt = shellDamage;
                result.damageType  = DamageType.Shell;
                result.removedCardNames.AddRange(partResult.removedCardNames);

                if (partResult.partBreakOccurred && !firstPartBreakOccurredThisCombat)
                {
                    result.apexShouldTrigger = true;
                    Debug.Log("[Card] First part break — Apex trigger flagged");
                }

                Debug.Log($"[Card] SHELL HIT — {shellDamage} Shell to {targetPart.partName}");
            }

            // ── Step 5: AP Management ────────────────────────────
            result.apRefundGranted  = card.apRefund;
            attacker.apRemaining   -= (card.apCost - card.apRefund);

            Debug.Log($"[Card] Resolution complete. Damage:{result.damageDealt} {result.damageType} " +
                      $"AP remaining:{attacker.apRemaining} " +
                      $"Cards removed:[{string.Join(", ", result.removedCardNames)}]");

            return result;
        }

        // ── Damage Calculation ───────────────────────────────────
        // Shell is always 1 base — weapon-type modifiers wired in Stage 7
        private static int CalculateShellDamage() => 1;

        // Flesh is 1 base; crits add +1
        private static int CalculateFleshDamage(HunterCombatState attacker, bool isCrit)
        {
            int base_ = 1;
            if (isCrit)
            {
                base_ += 1;
                Debug.Log("[Card] Critical hit — +1 Flesh damage");
            }
            return base_;
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
        public int          damageDealt;
        public DamageType   damageType;
        public int          apRefundGranted;
        public bool         reactionApplied;
        public bool         wasLoud;
        public bool         wasMiss;
        public bool         isCritical;
        public bool         apexShouldTrigger;
        public List<string> removedCardNames;
    }
}
