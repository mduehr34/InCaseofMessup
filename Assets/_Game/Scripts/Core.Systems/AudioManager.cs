using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using MnM.Core.Data;

namespace MnM.Core.Systems
{
    public class AudioManager : MonoBehaviour
    {
        // ── Singleton ────────────────────────────────────────────
        public static AudioManager Instance { get; private set; }

        // ── Inspector ────────────────────────────────────────────
        [SerializeField] private AudioMixer  _masterMixer;
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioSource _ambientSource;

        // ── Music Clips ──────────────────────────────────────────
        [Header("Music")]
        [SerializeField] private AudioClip _settlementEarly;
        [SerializeField] private AudioClip _settlementLate;
        [SerializeField] private AudioClip _huntTravel;
        [SerializeField] private AudioClip _combatStandard;
        [SerializeField] private AudioClip _combatOverlord;

        // ── SFX Clips ────────────────────────────────────────────
        [Header("SFX")]
        [SerializeField] private AudioClip _sfxShellHit;
        [SerializeField] private AudioClip _sfxFleshHit;
        [SerializeField] private AudioClip _sfxMiss;
        [SerializeField] private AudioClip _sfxCardPlay;
        [SerializeField] private AudioClip _sfxPartBreak;
        [SerializeField] private AudioClip _sfxHunterCollapse;
        [SerializeField] private AudioClip _sfxMonsterDefeated;
        [SerializeField] private AudioClip _sfxDeathSting;

        private AudioContext _currentContext = AudioContext.SettlementEarly;
        private Coroutine    _fadeCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ── Music Context ─────────────────────────────────────────
        public void SetMusicContext(AudioContext context)
        {
            if (context == _currentContext) return;
            _currentContext = context;

            AudioClip clip = context switch
            {
                AudioContext.SettlementEarly => _settlementEarly,
                AudioContext.SettlementLate  => _settlementLate,
                AudioContext.HuntTravel      => _huntTravel,
                AudioContext.CombatStandard  => _combatStandard,
                AudioContext.CombatOverlord  => _combatOverlord,
                _                            => _settlementEarly,
            };

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(CrossfadeMusic(clip));

            Debug.Log($"[Audio] Music context → {context}");
        }

        public void SetContextForYear(int year)
        {
            var ctx = year <= 12 ? AudioContext.SettlementEarly : AudioContext.SettlementLate;
            SetMusicContext(ctx);
        }

        private IEnumerator CrossfadeMusic(AudioClip newClip)
        {
            if (newClip == null) yield break;

            float startVolume = _musicSource.volume;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 1.5f;
                _musicSource.volume = Mathf.Lerp(startVolume, 0f, t);
                yield return null;
            }

            _musicSource.clip = newClip;
            _musicSource.loop = true;
            _musicSource.Play();

            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 1.5f;
                _musicSource.volume = Mathf.Lerp(0f, startVolume, t);
                yield return null;
            }
            _musicSource.volume = startVolume;
        }

        // ── SFX ──────────────────────────────────────────────────
        public void PlaySFX(string sfxName)
        {
            AudioClip clip = sfxName switch
            {
                "Attack_Hit_Shell" => _sfxShellHit,
                "Attack_Hit_Flesh" => _sfxFleshHit,
                "Attack_Miss"      => _sfxMiss,
                "Card_Play"        => _sfxCardPlay,
                "PartBreak"        => _sfxPartBreak,
                "HunterCollapse"   => _sfxHunterCollapse,
                "MonsterDefeated"  => _sfxMonsterDefeated,
                _                  => null,
            };

            if (clip != null)
                _sfxSource.PlayOneShot(clip);
            else
                Debug.LogWarning($"[Audio] SFX not found: {sfxName}");
        }

        // ── Death Sting ───────────────────────────────────────────
        public void PlayDeathSting()
        {
            StartCoroutine(DeathStingSequence());
        }

        private IEnumerator DeathStingSequence()
        {
            if (_sfxDeathSting == null) yield break;

            float prevVolume = _musicSource.volume;

            _musicSource.volume = 0f;
            _sfxSource.PlayOneShot(_sfxDeathSting);

            Debug.Log("[Audio] Death sting — 2s silence");
            yield return new WaitForSeconds(2f);

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime;
                _musicSource.volume = Mathf.Lerp(0f, prevVolume, t);
                yield return null;
            }
            _musicSource.volume = prevVolume;
        }

        // ── Combat Event Hooks ────────────────────────────────────
        public void OnDamageDealt(DamageType type)
        {
            PlaySFX(type == DamageType.Shell ? "Attack_Hit_Shell" : "Attack_Hit_Flesh");
        }

        public void OnEntityCollapsed(string entityId)
        {
            PlaySFX("HunterCollapse");
            PlayDeathSting();
        }

        public void OnMonsterDefeated()
        {
            PlaySFX("MonsterDefeated");
        }

        public void OnCardPlayed()
        {
            PlaySFX("Card_Play");
        }

        public void OnPartBroken()
        {
            PlaySFX("PartBreak");
        }
    }
}
