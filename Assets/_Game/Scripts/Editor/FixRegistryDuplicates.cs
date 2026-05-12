using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using MnM.Core.Data;

namespace MnM.Editor
{
    public static class FixRegistryDuplicates
    {
        [MenuItem("MnM/Fix Registry Duplicates")]
        public static void Execute()
        {
            // ── WeaponRegistry ────────────────────────────────────────────
            var weaponRegistry = Resources.Load<WeaponRegistrySO>("WeaponRegistry");
            if (weaponRegistry != null && weaponRegistry.weapons != null)
            {
                var seen    = new HashSet<WeaponType>();
                var cleaned = new List<WeaponSO>();
                int removedWeapons = 0;
                foreach (var w in weaponRegistry.weapons)
                {
                    if (w == null) continue;
                    if (seen.Contains(w.weaponType))
                    {
                        Debug.Log($"[Fix] WeaponRegistry: removed duplicate '{w.weaponType}'");
                        removedWeapons++;
                        continue;
                    }
                    seen.Add(w.weaponType);
                    cleaned.Add(w);
                }
                if (removedWeapons > 0)
                {
                    weaponRegistry.weapons = cleaned.ToArray();
                    EditorUtility.SetDirty(weaponRegistry);
                    Debug.Log($"[Fix] WeaponRegistry cleaned — removed {removedWeapons} duplicates, {cleaned.Count} entries remain");
                }
                else
                {
                    Debug.Log($"[Fix] WeaponRegistry — no duplicates found ({cleaned.Count} entries)");
                }
            }
            else
            {
                Debug.LogWarning("[Fix] WeaponRegistry not found at Resources/WeaponRegistry");
            }

            // ── ActionCardRegistry ────────────────────────────────────────
            var cardRegistry = Resources.Load<ActionCardRegistrySO>("ActionCardRegistry");
            if (cardRegistry != null && cardRegistry.cards != null)
            {
                var seen    = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
                var cleaned = new List<ActionCardSO>();
                int removedCards = 0;
                foreach (var c in cardRegistry.cards)
                {
                    if (c == null) continue;
                    if (seen.Contains(c.cardName))
                    {
                        Debug.Log($"[Fix] CardRegistry: removed duplicate '{c.cardName}'");
                        removedCards++;
                        continue;
                    }
                    seen.Add(c.cardName);
                    cleaned.Add(c);
                }
                if (removedCards > 0)
                {
                    cardRegistry.cards = cleaned.ToArray();
                    EditorUtility.SetDirty(cardRegistry);
                    Debug.Log($"[Fix] CardRegistry cleaned — removed {removedCards} duplicates, {cleaned.Count} entries remain");
                }
                else
                {
                    Debug.Log($"[Fix] CardRegistry — no duplicates found ({cleaned.Count} entries)");
                }
            }
            else
            {
                Debug.LogWarning("[Fix] ActionCardRegistry not found at Resources/ActionCardRegistry");
            }

            // ── Report Gaunt deckCompositions ─────────────────────────────
            var gauntGuids = AssetDatabase.FindAssets("Monster_Gaunt t:MonsterSO");
            foreach (var guid in gauntGuids)
            {
                var path    = AssetDatabase.GUIDToAssetPath(guid);
                var monster = AssetDatabase.LoadAssetAtPath<MonsterSO>(path);
                if (monster == null) continue;
                bool hasComps = monster.deckCompositions != null && monster.deckCompositions.Length > 0;
                bool hasBase  = monster.baseCardPool     != null && monster.baseCardPool.Length    > 0;
                Debug.Log($"[Fix] {monster.name} — deckCompositions:{(hasComps ? monster.deckCompositions.Length : 0)} " +
                          $"baseCardPool:{(hasBase ? monster.baseCardPool.Length : 0)} " +
                          $"standardWoundDeck:{(monster.standardWoundDeck?.Length ?? 0)}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Fix] Done — registry duplicates cleaned");
        }
    }
}
