using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using MnM.Core.Systems;

public class SetupAudioManager
{
    private const string PrefabPath = "Assets/_Game/Prefabs/AudioManager.prefab";

    public static void Execute()
    {
        // ── Load all clips ────────────────────────────────────────
        var settlementEarly    = Load("Assets/_Game/Audio/Music/music_settlement_early.wav");
        var settlementLate     = Load("Assets/_Game/Audio/Music/music_settlement_late.wav");
        var huntTravel         = Load("Assets/_Game/Audio/Music/music_hunt_travel.wav");
        var combatStandard     = Load("Assets/_Game/Audio/Music/music_combat_standard.wav");
        var combatOverlord     = Load("Assets/_Game/Audio/Music/music_combat_overlord.wav");
        var sfxShellHit        = Load("Assets/_Game/Audio/SFX/sfx_shell_hit.wav");
        var sfxFleshHit        = Load("Assets/_Game/Audio/SFX/sfx_flesh_hit.wav");
        var sfxMiss            = Load("Assets/_Game/Audio/SFX/sfx_miss.wav");
        var sfxCardPlay        = Load("Assets/_Game/Audio/SFX/sfx_card_play.wav");
        var sfxPartBreak       = Load("Assets/_Game/Audio/SFX/sfx_part_break.wav");
        var sfxHunterCollapse  = Load("Assets/_Game/Audio/SFX/sfx_hunter_collapse.wav");
        var sfxMonsterDefeated = Load("Assets/_Game/Audio/SFX/sfx_monster_defeated.wav");
        var sfxDeathSting      = Load("Assets/_Game/Audio/SFX/sfx_death_sting.wav");

        // ── Ensure Prefabs folder ─────────────────────────────────
        if (!System.IO.Directory.Exists("Assets/_Game/Prefabs"))
        {
            System.IO.Directory.CreateDirectory("Assets/_Game/Prefabs");
            AssetDatabase.Refresh();
        }

        // Delete existing prefab if present
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null)
            AssetDatabase.DeleteAsset(PrefabPath);

        // ── Build GameObject hierarchy ────────────────────────────
        var root = new GameObject("AudioManager");
        root.AddComponent<AudioManager>();

        var musicSrc   = AddSource(root, "MusicSource",   loop: true,  playOnAwake: true,  volume: 0.7f);
        var sfxSrc     = AddSource(root, "SFXSource",     loop: false, playOnAwake: false, volume: 1.0f);
        var ambientSrc = AddSource(root, "AmbientSource", loop: true,  playOnAwake: false, volume: 0.4f);

        // ── Assign fields via SerializedObject ────────────────────
        var am = root.GetComponent<AudioManager>();
        var so = new SerializedObject(am);

        // Sources (no AudioMixer yet — add manually once mixer is created via Assets > Create > Audio Mixer)
        Assign(so, "_musicSource",         musicSrc);
        Assign(so, "_sfxSource",           sfxSrc);
        Assign(so, "_ambientSource",       ambientSrc);

        // Music
        Assign(so, "_settlementEarly",     settlementEarly);
        Assign(so, "_settlementLate",      settlementLate);
        Assign(so, "_huntTravel",          huntTravel);
        Assign(so, "_combatStandard",      combatStandard);
        Assign(so, "_combatOverlord",      combatOverlord);

        // SFX
        Assign(so, "_sfxShellHit",         sfxShellHit);
        Assign(so, "_sfxFleshHit",         sfxFleshHit);
        Assign(so, "_sfxMiss",             sfxMiss);
        Assign(so, "_sfxCardPlay",         sfxCardPlay);
        Assign(so, "_sfxPartBreak",        sfxPartBreak);
        Assign(so, "_sfxHunterCollapse",   sfxHunterCollapse);
        Assign(so, "_sfxMonsterDefeated",  sfxMonsterDefeated);
        Assign(so, "_sfxDeathSting",       sfxDeathSting);

        so.ApplyModifiedProperties();

        // ── Save prefab ───────────────────────────────────────────
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (prefab != null)
            Debug.Log("[AudioSetup] AudioManager prefab saved → " + PrefabPath +
                      "\nNote: Assign _masterMixer manually once you create an AudioMixer at Assets/_Game/Audio/MasterMixer.mixer");
        else
            Debug.LogError("[AudioSetup] Prefab save failed.");
    }

    private static AudioSource AddSource(GameObject parent, string name, bool loop, bool playOnAwake, float volume)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent.transform);
        var src           = go.AddComponent<AudioSource>();
        src.loop          = loop;
        src.playOnAwake   = playOnAwake;
        src.volume        = volume;
        return src;
    }

    private static AudioClip Load(string path)
    {
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        if (clip == null) Debug.LogWarning("[AudioSetup] Missing: " + path);
        else              Debug.Log("[AudioSetup] OK: " + System.IO.Path.GetFileName(path));
        return clip;
    }

    private static void Assign(SerializedObject so, string field, Object value)
    {
        var prop = so.FindProperty(field);
        if (prop != null) prop.objectReferenceValue = value;
        else              Debug.LogWarning("[AudioSetup] Field not found: " + field);
    }
}
