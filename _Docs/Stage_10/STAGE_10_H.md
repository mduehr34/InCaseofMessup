<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 10-H | Audio Completion — Monster SFX, UI Sounds, Ambient Tracks
Status: Stage 10-G complete. Combat visual polish done.
Task: Fill the audio gaps identified after Stage 9. Stage 8
generated 6 music tracks and 9 SFX. What's missing:
  - Monster audio: roar/screech on encounter start, movement
    footstep (looping while moving), death sound
  - UI audio: hover, error/invalid, notification/event chime
  - Settlement ambient: quiet background ambience for the hub
  - All new audio must be wired into AudioManager

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_10/STAGE_10_H.md
- Assets/_Game/Scripts/Core.Systems/AudioManager.cs
- Assets/_Game/Data/Monsters/             ← MonsterSO assets need audioRoar field

Then confirm:
- AudioManager.PlaySFX(string clipName) already works
- MonsterSO does NOT yet have audio fields — add them this session
- CoPlay MCP tool for SFX: mcp__coplay-mcp__generate_sfx
- CoPlay MCP tool for music/ambient: mcp__coplay-mcp__generate_music
- Output folder: Assets/_Game/Audio/Generated/
- What you will NOT do (settings UI, accessibility — those are Stage 10-I)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 10-H: Audio Completion — Monster SFX, UI Sounds, Ambient Tracks

**Resuming from:** Stage 10-G complete — combat visual polish done
**Done when:** All monsters have roar/move/death SFX; UI has hover/error/notification sounds; settlement has ambient audio; all wired into AudioManager
**Commit:** `"10H: Audio completion — monster SFX, UI sounds, settlement ambience wired"`
**Next session:** STAGE_10_I.md

---

## Part 1 — Add Audio Fields to MonsterSO

Open `MonsterSO.cs` and add:

```csharp
[Header("Audio")]
public AudioClip roarClip;       // Plays when monster is first encountered (encounter start)
public AudioClip footstepClip;   // Short loop; played while monster is moving
public AudioClip deathClip;      // Plays when monster is defeated
public AudioClip hitClip;        // Plays when monster takes a hit (optional; reuse generic if null)
```

If `AudioClip` isn't imported, add `using UnityEngine;` — it's a standard UnityEngine type.

---

## Part 2 — Monster SFX Generation

Generate audio using `mcp__coplay-mcp__generate_sfx` for each monster.

**Output path for all SFX:** `Assets/_Game/Audio/Generated/`

After generating each clip, assign it to the correct MonsterSO field. Use the editor script at the end of this section to bulk-assign.

---

### Standard Monsters — Roar SFX

Generate a roar/encounter sound for each standard monster:

| Monster | Filename | Prompt |
|---------|----------|--------|
| The Gaunt | `SFX_Gaunt_Roar.wav` | "A dry hollow shriek from an emaciated humanoid creature, distant and unsettling, short 1–2 second burst" |
| Thornback | `SFX_Thornback_Roar.wav` | "A deep guttural animal roar, quadruped predator, heavy and low, 1–2 seconds" |
| Ivory Stampede | `SFX_IvoryStampede_Roar.wav` | "A thunderous bull bellow mixed with the sound of hooves on stone, 1–2 seconds" |
| Bog Caller | `SFX_BogCaller_Roar.wav` | "A wet resonant croaking bellow, amphibian creature, 1–2 seconds" |
| The Shriek | `SFX_Shriek_Roar.wav` | "A piercing aerial screech, high-pitched and sharp, 0.5–1 second" |
| Rotmother | `SFX_Rotmother_Roar.wav` | "A deep churning insectoid vibration mixed with a wet organic sound, 1–2 seconds" |
| Gilded Serpent | `SFX_GildedSerpent_Roar.wav` | "A long resonant hiss that builds to a low threatening rumble, 1.5–2 seconds" |
| Ironhide | `SFX_Ironhide_Roar.wav` | "A metallic grinding roar, like iron scraping stone, 1–2 seconds" |

### Overlord Monsters — Roar SFX

| Monster | Filename | Prompt |
|---------|----------|--------|
| The Siltborn | `SFX_Siltborn_Roar.wav` | "A massive subsonic rumble, like stone grinding and earth moving, 2–3 seconds" |
| The Penitent | `SFX_Penitent_Roar.wav` | "A rattling chain sound mixed with a pained hollow moan, 2 seconds" |
| The Suture | `SFX_Suture_Roar.wav` | "Multiple overlapping human moans stitched together into one discordant sound, 2 seconds" |
| Pale Stag Ascendant | `SFX_PaleStag_Roar.wav` | "A majestic and terrifying stag bellow that echoes as if heard through many places at once, 2–3 seconds" |

### Monster Footstep Loops (4 generic types)

Rather than per-monster footsteps, generate 4 types covering the main locomotion categories:

| Type | Filename | Prompt |
|------|----------|--------|
| Heavy quadruped | `SFX_Footstep_Heavy.wav` | "A single heavy four-legged animal footfall on stone, deep thud, 0.3 seconds" |
| Light aerial | `SFX_Footstep_Light.wav` | "A soft quick claw-tap on stone, small creature, 0.2 seconds" |
| Massive bipedal | `SFX_Footstep_Massive.wav` | "A bone-shaking heavy footfall, giant creature, deep impact resonance, 0.4 seconds" |
| Slithering | `SFX_Footstep_Slither.wav` | "A dry scraping slither sound, serpent on stone, 0.3 seconds" |

Assign footstep type to each MonsterSO in the `footstepClip` field:
- Gaunt → `SFX_Footstep_Massive.wav` (bipedal, heavy)
- Thornback → `SFX_Footstep_Heavy.wav`
- Ivory Stampede → `SFX_Footstep_Heavy.wav`
- Bog Caller → `SFX_Footstep_Heavy.wav`
- Shriek → `SFX_Footstep_Light.wav`
- Rotmother → `SFX_Footstep_Heavy.wav`
- Gilded Serpent → `SFX_Footstep_Slither.wav`
- Ironhide → `SFX_Footstep_Massive.wav`
- Siltborn → `SFX_Footstep_Massive.wav`
- Penitent → `SFX_Footstep_Massive.wav`
- Suture → `SFX_Footstep_Heavy.wav`
- Pale Stag → `SFX_Footstep_Heavy.wav` (Phase 1 physical) / silence (Phase 2 Ascendant)

### Monster Death SFX (4 generic types)

| Type | Filename | Prompt |
|------|----------|--------|
| Standard creature death | `SFX_Monster_Death_Standard.wav` | "A large animal dying — a final groaning exhale and body collapse, 1.5–2 seconds" |
| Aerial creature death | `SFX_Monster_Death_Aerial.wav` | "A piercing screech cutting off abruptly, followed by a soft thud, 1.5 seconds" |
| Overlord death | `SFX_Monster_Death_Overlord.wav` | "A massive creature dying — a long resonant death groan with distant echo, 3–4 seconds" |
| Ascendant dissolution | `SFX_Monster_Death_Ascendant.wav` | "A reverberating shattering sound, like crystal breaking in slow motion, 3 seconds" |

Assign:
- Shriek → Aerial death
- Pale Stag Phase 1 → Standard death
- Pale Stag Phase 2 (Ascendant) → Ascendant dissolution
- All overlords → Overlord death
- All other standard → Standard death

---

## Part 3 — UI Sound Additions

The existing 9 SFX cover: UI click, craft success, gear equip, innovation adopt, card draw/play/discard, hit shell, hit flesh.

Generate these additional UI sounds:

| Filename | Prompt | Use |
|----------|--------|-----|
| `SFX_UI_Hover.wav` | "A very soft dry tick sound, like a fingernail lightly tapping wood, 0.05 seconds" | Button hover |
| `SFX_UI_Error.wav` | "A low flat thunk, negative feedback sound, not harsh, 0.2 seconds" | Invalid action |
| `SFX_UI_Notification.wav` | "A soft two-tone chime, ascending, warm, 0.4 seconds" | Event available, codex unlock |
| `SFX_UI_ChronicleEntry.wav` | "A quill on parchment — a brief scratching sound, 0.3 seconds" | Chronicle entry written |
| `SFX_UI_YearAdvance.wav` | "A deep resonant bell toll, single strike, 1 second" | Year advance banner |
| `SFX_Combat_MonsterCardDraw.wav` | "A low dry card-flip with a subtle menacing tone, 0.2 seconds" | Behavior card drawn |
| `SFX_Combat_PartBreak.wav` | "A sharp crack like bone snapping, 0.3 seconds" | Monster part breaks |
| `SFX_Combat_HunterDeath.wav` | "A heavy dull thud with a short gasp, 0.5 seconds" | Hunter collapses |
| `SFX_Combat_StatusApply.wav` | "A brief wet or hissing sound depending on status type, 0.2 seconds" | Status effect applied |

---

## Part 4 — Settlement Ambient Audio

Generate two ambient audio loops for the settlement scenes:

| Filename | Prompt | Use |
|----------|--------|-----|
| `AMB_Settlement_Early.wav` | "Quiet nighttime settlement ambience — distant wind, occasional wood creak, a single crackling fire, no music, looping 30-second loop" | Settlement Years 1–10 |
| `AMB_Settlement_Late.wav` | "Established settlement ambience — distant murmur of activity, fire crackling, occasional tool sound, warmer atmosphere, looping 30-second loop" | Settlement Years 11–30 |

---

## Part 5 — Wire Everything into AudioManager

### 5a — AudioManager New Clip Fields

Open `AudioManager.cs` and add the new audio arrays:

```csharp
[Header("Ambient Tracks")]
[SerializeField] private AudioClip _ambSettlementEarly;
[SerializeField] private AudioClip _ambSettlementLate;

[Header("New UI SFX")]
[SerializeField] private AudioClip _sfxHover;
[SerializeField] private AudioClip _sfxError;
[SerializeField] private AudioClip _sfxNotification;
[SerializeField] private AudioClip _sfxChronicleEntry;
[SerializeField] private AudioClip _sfxYearAdvance;
[SerializeField] private AudioClip _sfxMonsterCardDraw;
[SerializeField] private AudioClip _sfxPartBreak;
[SerializeField] private AudioClip _sfxHunterDeath;
[SerializeField] private AudioClip _sfxStatusApply;
```

### 5b — Ambient Layer AudioSource

Add a dedicated ambient `AudioSource` to the AudioManager GameObject (separate from the music source so ambient and music can play simultaneously):

```csharp
[Header("Audio Sources")]
[SerializeField] private AudioSource _musicSource;
[SerializeField] private AudioSource _sfxSource;
[SerializeField] private AudioSource _ambientSource;   // ← Add this

public void PlayAmbient(AudioClip clip, float fadeIn = 1f)
{
    if (_ambientSource.clip == clip) return;
    StartCoroutine(FadeAmbient(clip, fadeIn));
}

private IEnumerator FadeAmbient(AudioClip clip, float duration)
{
    // Fade out current ambient
    float start = _ambientSource.volume;
    float t     = 0f;
    while (t < 1f && _ambientSource.isPlaying)
    {
        t += Time.deltaTime / duration;
        _ambientSource.volume = Mathf.Lerp(start, 0f, t);
        yield return null;
    }

    _ambientSource.Stop();
    _ambientSource.clip   = clip;
    _ambientSource.loop   = true;
    _ambientSource.volume = 0f;
    _ambientSource.Play();

    t = 0f;
    while (t < 1f)
    {
        t += Time.deltaTime / duration;
        _ambientSource.volume = Mathf.Lerp(0f, _ambientVolume, t);
        yield return null;
    }
}

private float _ambientVolume = 0.25f; // Quieter than music
```

### 5c — PlaySFX Extension

Add named methods for the new SFX so call sites don't use magic strings:

```csharp
public void PlayHover()          => PlaySFX(_sfxHover);
public void PlayError()          => PlaySFX(_sfxError);
public void PlayNotification()   => PlaySFX(_sfxNotification);
public void PlayChronicleEntry() => PlaySFX(_sfxChronicleEntry);
public void PlayYearAdvance()    => PlaySFX(_sfxYearAdvance);
public void PlayMonsterCardDraw()=> PlaySFX(_sfxMonsterCardDraw);
public void PlayPartBreak()      => PlaySFX(_sfxPartBreak);
public void PlayHunterDeath()    => PlaySFX(_sfxHunterDeath);
public void PlayStatusApply()    => PlaySFX(_sfxStatusApply);

public void PlayMonsterRoar(MonsterSO monster)
{
    if (monster?.roarClip != null) PlaySFX(monster.roarClip);
}
public void PlayMonsterFootstep(MonsterSO monster)
{
    if (monster?.footstepClip != null) PlaySFX(monster.footstepClip);
}
public void PlayMonsterDeath(MonsterSO monster)
{
    if (monster?.deathClip != null) PlaySFX(monster.deathClip);
}
```

### 5d — Call Sites

Wire the new SFX at the correct locations:

| Event | Call |
|-------|------|
| Any button OnPointerEnter | `AudioManager.Instance.PlayHover()` |
| Invalid card target / out of range | `AudioManager.Instance.PlayError()` |
| Settlement event available / codex unlocked | `AudioManager.Instance.PlayNotification()` |
| `AddChronicleEntry()` | `AudioManager.Instance.PlayChronicleEntry()` |
| Year advance banner appears | `AudioManager.Instance.PlayYearAdvance()` |
| Behavior card drawn from deck | `AudioManager.Instance.PlayMonsterCardDraw()` |
| Monster part breaks | `AudioManager.Instance.PlayPartBreak()` |
| Hunter collapses | `AudioManager.Instance.PlayHunterDeath()` |
| Status effect applied | `AudioManager.Instance.PlayStatusApply()` |
| Combat scene loads | `AudioManager.Instance.PlayMonsterRoar(activeMonster)` |
| Monster movement begins | `AudioManager.Instance.PlayMonsterFootstep(activeMonster)` |
| Monster dies | `AudioManager.Instance.PlayMonsterDeath(activeMonster)` |
| Settlement scene loads (Year ≤10) | `AudioManager.Instance.PlayAmbient(_ambSettlementEarly)` |
| Settlement scene loads (Year >10) | `AudioManager.Instance.PlayAmbient(_ambSettlementLate)` |

### 5e — Button Hover Wiring (UIToolkit)

UIToolkit doesn't expose OnPointerEnter per-button easily. Add a utility in `SettlementScreenController` and `CombatScreenController`:

```csharp
private void RegisterHoverSound(VisualElement element)
{
    element.RegisterCallback<MouseEnterEvent>(_ =>
        AudioManager.Instance.PlayHover());
}

// Call for every button in the scene:
root.Query<Button>().ForEach(btn => RegisterHoverSound(btn));
```

Call this at the end of each controller's `Initialise()` or `OnEnable()`.

---

## Part 6 — Monster Audio Editor Script (Bulk Assignment)

**Path:** `Assets/_Game/Editor/AssignMonsterAudio.cs`

```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using MnM.Core.Data;

namespace MnM.Editor
{
    public static class AssignMonsterAudio
    {
        private static string AudioPath(string filename)
            => $"Assets/_Game/Audio/Generated/{filename}";

        [MenuItem("MnM/Assign Monster Audio")]
        public static void AssignAll()
        {
            AssignMonster("Monster_TheGaunt",       "SFX_Gaunt_Roar",        "SFX_Footstep_Massive", "SFX_Monster_Death_Standard");
            AssignMonster("Thornback_Standard",     "SFX_Thornback_Roar",    "SFX_Footstep_Heavy",   "SFX_Monster_Death_Standard");
            AssignMonster("IvoryStampede_Standard", "SFX_IvoryStampede_Roar","SFX_Footstep_Heavy",   "SFX_Monster_Death_Standard");
            AssignMonster("BogCaller_Standard",     "SFX_BogCaller_Roar",    "SFX_Footstep_Heavy",   "SFX_Monster_Death_Standard");
            AssignMonster("Shriek_Standard",        "SFX_Shriek_Roar",       "SFX_Footstep_Light",   "SFX_Monster_Death_Aerial");
            AssignMonster("Rotmother_Nightmare",    "SFX_Rotmother_Roar",    "SFX_Footstep_Heavy",   "SFX_Monster_Death_Standard");
            AssignMonster("GildedSerpent_Standard", "SFX_GildedSerpent_Roar","SFX_Footstep_Slither", "SFX_Monster_Death_Standard");
            AssignMonster("Ironhide_Standard",      "SFX_Ironhide_Roar",     "SFX_Footstep_Massive", "SFX_Monster_Death_Standard");
            AssignMonster("Siltborn_Overlord",      "SFX_Siltborn_Roar",     "SFX_Footstep_Massive", "SFX_Monster_Death_Overlord");
            AssignMonster("Penitent_Overlord",      "SFX_Penitent_Roar",     "SFX_Footstep_Massive", "SFX_Monster_Death_Overlord");
            AssignMonster("Monster_Suture",         "SFX_Suture_Roar",       "SFX_Footstep_Heavy",   "SFX_Monster_Death_Overlord");
            AssignMonster("PaleStag_Overlord",      "SFX_PaleStag_Roar",     "SFX_Footstep_Heavy",   "SFX_Monster_Death_Overlord");
            AssetDatabase.SaveAssets();
            Debug.Log("[AssignMonsterAudio] Done.");
        }

        private static void AssignMonster(string soName, string roar, string step, string death)
        {
            var so = AssetDatabase.LoadAssetAtPath<MonsterSO>(
                $"Assets/_Game/Data/Monsters/{soName}.asset");
            if (so == null) { Debug.LogWarning($"MonsterSO not found: {soName}"); return; }

            so.roarClip      = LoadClip(roar);
            so.footstepClip  = LoadClip(step);
            so.deathClip     = LoadClip(death);
            EditorUtility.SetDirty(so);
        }

        private static AudioClip LoadClip(string name)
            => AssetDatabase.LoadAssetAtPath<AudioClip>(AudioPath(name + ".wav"));
    }
}
#endif
```

Run via **MnM → Assign Monster Audio**.

---

## Verification Checklist

- [ ] MonsterSO compiles with `roarClip`, `footstepClip`, `deathClip` fields
- [ ] All 12 monsters have roar clips assigned in Inspector
- [ ] All 12 monsters have footstep clips assigned in Inspector
- [ ] All 12 monsters have death clips assigned in Inspector
- [ ] `AudioManager.Instance.PlayHover()` does not throw null-ref
- [ ] Button hover in Main Menu plays a soft tick sound
- [ ] Invalid action (e.g. clicking out-of-range target) plays error thunk
- [ ] Codex entry unlocks → notification chime plays
- [ ] Year advance banner → bell toll plays
- [ ] Monster part breaks → crack sound plays
- [ ] Hunter collapse → dull thud plays
- [ ] Combat scene loads with Thornback → Thornback roar plays within 0.5s of load
- [ ] Settlement scene loads (Year 1) → early ambient loop plays quietly under music
- [ ] Settlement scene loads (Year 15) → late ambient loop plays
- [ ] Ambient and music play simultaneously without clipping

---

## Next Session

**File:** `_Docs/Stage_10/STAGE_10_I.md`
**Covers:** UI polish and accessibility — tab transition animations in the settlement, button hover visual states, a fully functional settings menu (volume sliders, fullscreen toggle, input remapping stub), and basic accessibility options (font size, high-contrast mode toggle)
