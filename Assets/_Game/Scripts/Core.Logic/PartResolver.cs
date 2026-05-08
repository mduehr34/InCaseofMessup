// Stage 8-M: PartResolver stubbed — the shell/flesh monster body part system is removed.
// Monster wound resolution is replaced by WoundLocationSO + behavior card removal.
// Full wound resolution pipeline implemented in Stage 8-N (CombatManager.ResolveWound).
using System.Collections.Generic;
using UnityEngine;
using MnM.Core.Data;

namespace MnM.Core.Logic
{
    public static class PartResolver
    {
        public static PartDamageResult ApplyDamage(
            ref MonsterPartState part,
            int damageAmount,
            MonsterSO monsterData)
        {
            // Monster body parts (shell/flesh HP) removed in Stage 8-M.
            // Wound resolution now draws from WoundLocationSO deck — see Stage 8-N.
            Debug.LogWarning("[PartResolver] ApplyDamage called on stub — not implemented. See Stage 8-N.");
            return new PartDamageResult { removedCardNames = new List<string>() };
        }
    }

    public struct PartDamageResult
    {
        public bool partBreakOccurred;
        public bool woundOccurred;
        public List<string> removedCardNames;
    }
}
