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

        public struct HitRecord
        {
            public string hunterId;
            public string zone;      // Body zone name hit
            public int    damage;
        }
    }
}
