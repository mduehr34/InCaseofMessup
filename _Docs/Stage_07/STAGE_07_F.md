<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 7-F | Aldric Animation Frames & AudioManager
Status: Stage 7-E complete. UI and settlement art imported.
Task: Import the Aldric animation sprite sheet (or individual
frames) and slice them into Unity sprite frames. Then implement
AudioManager.cs with full music context switching and SFX.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_07/STAGE_07_F.md
- Assets/_Game/Art/Generated/Characters/char_aethel_idle.png
- Assets/_Game/Scripts/Core.Data/Enums.cs

Then confirm:
- Whether the art is delivered as a sprite sheet or individual
  frames (this determines the import method below)
- AudioManager uses Unity AudioMixer
- Death sting: brief mournful sound, then 2s silence before
  music resumes
- What you will NOT touch this session

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 7-F: Aldric Animation Frames & AudioManager

**Resuming from:** Stage 7-E complete  
**Done when:** All 11 Aldric animation frames sliced and named correctly in Unity; AudioManager compiles and logs context switches correctly  
**Commit:** `"7F: Aldric animation frames imported and sliced, AudioManager with death sting and context switching"`  
**Next session:** STAGE_07_G.md  

---

## Part 1: Animation Frames

Per GDD Appendix B.2, Aldric (Aethel build) needs:
- Idle: 2 frames
- Walk: 4 frames
- Attack: 3 frames
- Collapse: 2 frames

**Total: 11 frames.**

**Save path:** `Assets/_Game/Art/Generated/Characters/Aldric/`

---

### Option A — Sprite Sheet Import

If the art is delivered as a single horizontal sprite sheet with all 11 frames:

1. Import the sheet PNG using the Art Importer → subfolder `Characters`, name `aldric_sheet`
2. In the Inspector set:
   ```
   Texture Type:     Sprite (2D and UI)
   Sprite Mode:      Multiple
   Pixels Per Unit:  16
   Filter Mode:      Point (No Filter)
   Compression:      None
   ```
3. Click **Sprite Editor** → **Slice** → **Grid By Cell Size** → Width: 32, Height: 64
4. Rename each sliced cell in the Sprite Editor to match the convention below
5. Click **Apply**

---

### Option B — Individual Frame Import

If the art is delivered as separate PNG files per frame:

1. Import each frame individually with the Art Importer → subfolder `Characters`
2. Use this exact naming convention:

```
aldric_idle_01.png     aldric_idle_02.png
aldric_walk_01.png     aldric_walk_02.png
aldric_walk_03.png     aldric_walk_04.png
aldric_attack_01.png   aldric_attack_02.png   aldric_attack_03.png
aldric_collapse_01.png aldric_collapse_02.png
```

3. Apply import settings to each frame:
   ```
   Texture Type:     Sprite (2D and UI)
   Pixels Per Unit:  16
   Filter Mode:      Point (No Filter)
   Compression:      None
   Max Size:         64
   ```

---

### Frame Content Reference

| Frame | Pose Description |
|---|---|
| Idle 1 | Resting stance |
| Idle 2 | Subtle weight shift, slightly different arm position |
| Walk 1–4 | Full walk cycle, legs progressing through stride |
| Attack 1 | Windup — weapon raised or fist drawn back |
| Attack 2 | Mid-swing — weapon/fist extended forward |
| Attack 3 | Follow-through — slight recoil |
| Collapse 1 | Stumbling backward, knees buckling |
| Collapse 2 | On the ground, motionless |

---

## Part 2: AudioManager.cs

**Path:** `Assets/_Game/Scripts/Core.Systems/AudioManager.cs`

```csharp
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

        // ── Music Clips — assign in Inspector ───────────────────
        [Header("Music")]
        [SerializeField] private AudioClip _settlementEarly;
        [SerializeField] private AudioClip _settlementLate;
        [SerializeField] private AudioClip _huntTravel;
        [SerializeField] private AudioClip _combatStandard;
        [SerializeField] private AudioClip _combatOverlord;

        // ── SFX Clips — assign in Inspector ─────────────────────
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
                AudioContext.SettlementEarly  => _settlementEarly,
                AudioContext.SettlementLate   => _settlementLate,
                AudioContext.HuntTravel       => _huntTravel,
                AudioContext.CombatStandard   => _combatStandard,
                AudioContext.CombatOverlord   => _combatOverlord,
                _                             => _settlementEarly,
            };

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(CrossfadeMusic(clip));

            Debug.Log($"[Audio] Music context → {context}");
        }

        // Determine context from campaign year
        public void SetContextForYear(int year)
        {
            var ctx = year <= 12 ? AudioContext.SettlementEarly : AudioContext.SettlementLate;
            SetMusicContext(ctx);
        }

        private IEnumerator CrossfadeMusic(AudioClip newClip)
        {
            if (newClip == null) yield break;

            // Fade out current
            float startVolume = _musicSource.volume;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 1.5f;
                _musicSource.volume = Mathf.Lerp(startVolume, 0f, t);
                yield return null;
            }

            // Swap clip
            _musicSource.clip = newClip;
            _musicSource.loop = true;
            _musicSource.Play();

            // Fade in
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
                "Attack_Hit_Shell"   => _sfxShellHit,
                "Attack_Hit_Flesh"   => _sfxFleshHit,
                "Attack_Miss"        => _sfxMiss,
                "Card_Play"          => _sfxCardPlay,
                "PartBreak"          => _sfxPartBreak,
                "HunterCollapse"     => _sfxHunterCollapse,
                "MonsterDefeated"    => _sfxMonsterDefeated,
                _                    => null,
            };

            if (clip != null)
                _sfxSource.PlayOneShot(clip);
            else
                Debug.LogWarning($"[Audio] SFX not found: {sfxName}");
        }

        // ── Death Sting ───────────────────────────────────────────
        // Brief mournful sting → 2s silence → music resumes
        public void PlayDeathSting()
        {
            StartCoroutine(DeathStingSequence());
        }

        private IEnumerator DeathStingSequence()
        {
            if (_sfxDeathSting == null) yield break;

            float prevVolume = _musicSource.volume;

            // Cut music
            _musicSource.volume = 0f;
            _sfxSource.PlayOneShot(_sfxDeathSting);

            Debug.Log("[Audio] Death sting — 2s silence");
            yield return new WaitForSeconds(2f);

            // Fade music back in
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime;
                _musicSource.volume = Mathf.Lerp(0f, prevVolume, t);
                yield return null;
            }
            _musicSource.volume = prevVolume;
        }

        // ── Wire to Combat Events ─────────────────────────────────
        // Call from CombatScreenController when events fire
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
```

**Wire AudioManager into CombatScreenController:**

```csharp
// Add to OnEnable() in CombatScreenController after WireEvents():
if (AudioManager.Instance != null)
    AudioManager.Instance.SetMusicContext(AudioContext.CombatStandard);

// Add to OnDamageDealt():
AudioManager.Instance?.OnDamageDealt(type);

// Add to OnEntityCollapsed():
AudioManager.Instance?.OnEntityCollapsed(entityId);

// Add to OnCombatEnded() in ShowResultModal():
if (result.isVictory) AudioManager.Instance?.OnMonsterDefeated();
```

**Wire into SettlementScreenController:**

```csharp
// Add to OnEnable():
AudioManager.Instance?.SetContextForYear(
    GameStateManager.Instance.CampaignState.currentYear);
```

---

## Verification Test

**Art:**
- [ ] 11 Aldric frames exist in `Assets/_Game/Art/Generated/Characters/Aldric/`
- [ ] All frames use correct naming convention (aldric_[state]_[NN].png)
- [ ] All frames have Point (No Filter) import setting
- [ ] Walk cycle looks like coherent motion when viewed sequentially in Sprite Editor
- [ ] Collapse frame 2 is clearly defeated / on ground

**Audio:**
- [ ] AudioManager compiles with no errors
- [ ] SetMusicContext logs correctly
- [ ] Death sting coroutine: music cuts → 2s pause → fades back
- [ ] PlaySFX("PartBreak") plays without error if clip assigned

---

## Next Session

**File:** `_Docs/Stage_07/STAGE_07_G.md`  
**Covers:** All Gaunt behavior cards as SO assets — the canonical content template
