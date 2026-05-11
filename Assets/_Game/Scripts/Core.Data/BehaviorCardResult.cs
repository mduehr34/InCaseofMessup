using System.Collections.Generic;
using UnityEngine;

namespace MnM.Core.Data
{
    public class BehaviorCardResult
    {
        // Movement
        public bool       monsterMoved   = false;
        public Vector2Int newMonsterCell = Vector2Int.zero;

        // Attack outcomes — one entry per hunter hit
        public List<HitRecord> hits = new();

        // Special
        public bool   specialFired = false;
        public string specialTag   = "";

        // Defeat / pending damage (Stage 8-N)
        public bool   monsterDefeated       = false;
        public string pendingDamageHunterId = null;  // Set before Grit window, applied after
        public string pendingDamageZone     = null;

        public struct HitRecord
        {
            public string hunterId;
            public string zone;      // Body zone name hit
            public int    damage;
        }
    }
}
