using System.IO;
using UnityEngine;
using MnM.Core.Data;

namespace MnM.Core.Systems
{
    public static class SaveManager
    {
        private static readonly string SavePath =
            Path.Combine(Application.persistentDataPath, "campaign_save.json");

        // ── Save ─────────────────────────────────────────────────
        public static void Save(CampaignState state)
        {
            string json = JsonUtility.ToJson(state, prettyPrint: true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"[Save] Campaign saved. Year:{state.currentYear} Path:{SavePath}");
        }

        // ── Load ─────────────────────────────────────────────────
        public static CampaignState Load()
        {
            if (!File.Exists(SavePath))
            {
                Debug.Log("[Save] No save file found.");
                return null;
            }

            string json = File.ReadAllText(SavePath);
            var state   = JsonUtility.FromJson<CampaignState>(json);
            Debug.Log($"[Save] Campaign loaded. Year:{state.currentYear} " +
                      $"Characters:{state.characters?.Length ?? 0}");
            return state;
        }

        // ── Utility ──────────────────────────────────────────────
        public static bool HasSave()    => File.Exists(SavePath);

        public static void DeleteSave()
        {
            if (File.Exists(SavePath)) File.Delete(SavePath);
            Debug.Log("[Save] Save file deleted.");
        }

        public static string GetSavePath() => SavePath;
    }
}
