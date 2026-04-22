using UnityEditor;
using UnityEngine;
using System.IO;

public class VerifyAudioSetup
{
    public static void Execute()
    {
        string outPath = "Assets/Editor/audio_verify.txt";
        var sb = new System.Text.StringBuilder();

        string[] clips = {
            "Assets/_Game/Audio/Music/music_settlement_early.wav",
            "Assets/_Game/Audio/Music/music_settlement_late.wav",
            "Assets/_Game/Audio/Music/music_hunt_travel.wav",
            "Assets/_Game/Audio/Music/music_combat_standard.wav",
            "Assets/_Game/Audio/Music/music_combat_overlord.wav",
            "Assets/_Game/Audio/SFX/sfx_shell_hit.wav",
            "Assets/_Game/Audio/SFX/sfx_flesh_hit.wav",
            "Assets/_Game/Audio/SFX/sfx_miss.wav",
            "Assets/_Game/Audio/SFX/sfx_card_play.wav",
            "Assets/_Game/Audio/SFX/sfx_part_break.wav",
            "Assets/_Game/Audio/SFX/sfx_hunter_collapse.wav",
            "Assets/_Game/Audio/SFX/sfx_monster_defeated.wav",
            "Assets/_Game/Audio/SFX/sfx_death_sting.wav",
        };

        int ok = 0, missing = 0;
        foreach (var path in clips)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip != null)
            {
                sb.AppendLine($"OK   {Path.GetFileName(path)}  ({clip.length:F2}s)");
                ok++;
            }
            else
            {
                sb.AppendLine($"MISS {path}");
                missing++;
            }
        }

        // Check prefab
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/AudioManager.prefab");
        sb.AppendLine($"\nPrefab: {(prefab != null ? "EXISTS" : "MISSING")}");

        if (prefab != null)
        {
            var am = prefab.GetComponent<MnM.Core.Systems.AudioManager>();
            sb.AppendLine($"AudioManager component: {(am != null ? "YES" : "NO")}");
            var sources = prefab.GetComponentsInChildren<AudioSource>();
            sb.AppendLine($"AudioSources: {sources.Length}");
        }

        sb.AppendLine($"\nResult: {ok}/13 clips loaded, {missing} missing");
        File.WriteAllText(outPath, sb.ToString());
        AssetDatabase.Refresh();
        Debug.Log("[AudioVerify] Written to " + outPath);
    }
}
