<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 8-D | Audio Production — All Music Tracks & SFX
Status: Stage 8-C complete. Character creation working.
Task: Generate all 6 music tracks and 9 SFX clips using the
CoPlay MCP generate_music and generate_sfx tools. Add a
MainMenu music context to AudioManager. Wire all clips into
the AudioManager inspector fields. Verify audio plays
correctly in each scene context.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_08/STAGE_08_D.md
- Assets/_Game/Scripts/Core.Systems/AudioManager.cs

Then confirm:
- generate_music is the CoPlay tool for music tracks
- generate_sfx is the CoPlay tool for sound effects
- All audio files save to Assets/_Game/Audio/
- AudioManager gains a new MainMenu context and _mainMenuMusic field
- What you will NOT do (music mixing, mastering — out of scope)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 8-D: Audio Production — All Music Tracks & SFX

**Resuming from:** Stage 8-C complete — character creation working
**Done when:** All 6 music tracks and 9 SFX clips generated and saved; AudioManager updated with MainMenu context; all Inspector clip fields assigned; music plays in correct scene
**Commit:** `"8D: All audio generated — music tracks and SFX wired into AudioManager"`
**Next session:** STAGE_08_E.md

---

## What You Are Building

Right now AudioManager has code for music and SFX but every clip field is empty — the game is silent. This session fills all of them.

**New developer note:** Use the CoPlay MCP tools directly in Claude Code:
- `generate_music` → creates a music track from a text description
- `generate_sfx` → creates a sound effect from a text description

After generating each file, move it into the correct folder in the Unity Project window so Unity can find it.

---

## Folder Structure

Create this folder in Unity (right-click in Project → Create → Folder):
```
Assets/_Game/Audio/
├── Music/
└── SFX/
```

---

## Part 1: Music Tracks (6 total)

Use CoPlay `generate_music` for each. Save all to `Assets/_Game/Audio/Music/`.

---

### Track 1 — Main Menu
**Filename:** `music_main_menu.wav`
```
Prompt: Slow, atmospheric dark ambient music. Sparse percussion.
Deep drone bass. Occasional distant bell or bone chime.
Haunting but not aggressive. Sparse melody — long notes,
wide intervals. Feels like waiting at the edge of something.
Duration: 90 seconds, loopable. No lyrics.
Genre: Dark ambient / medieval atmospheric.
```

---

### Track 2 — Settlement Early (Years 1–12)
**Filename:** `music_settlement_early.wav`
```
Prompt: Quiet, melancholic folk ambient. Slow plucked strings,
occasional wooden flute. Minor key. Feels like a small fire
in the dark — warmth fighting against vast cold. Hopeful
undercurrent beneath the sadness. Loopable, 90 seconds.
No percussion, no lyrics. Sparse texture.
```

---

### Track 3 — Settlement Late (Years 13–30)
**Filename:** `music_settlement_late.wav`
```
Prompt: Heavier dark ambient with low rhythmic percussion.
Deep strings, ominous sustained pads. Same melodic fragments
as the early settlement theme but slower, grander, more foreboding.
Feels like something is coming. Loopable, 90 seconds. No lyrics.
```

---

### Track 4 — Hunt / Travel
**Filename:** `music_hunt_travel.wav`
```
Prompt: Tense, forward-moving dark orchestral. Steady slow
percussion, low strings building tension. Sparse but purposeful.
Feels like moving through dangerous wilderness — alert,
watchful. Not combat-fast but not relaxed. Loopable, 90 seconds.
No lyrics. Minor key.
```

---

### Track 5 — Combat (Standard Monster)
**Filename:** `music_combat_standard.wav`
```
Prompt: Driving dark combat music. Aggressive percussion,
urgent strings, dissonant brass stabs. Fast tempo.
Danger, urgency, violent rhythm. Not epic/heroic — this is
survival, not glory. Loopable, 60 seconds. No lyrics.
```

---

### Track 6 — Combat (Overlord)
**Filename:** `music_combat_overlord.wav`
```
Prompt: Massive, crushing dark orchestral combat music.
Massive drums, deep brass, dissonant choir. Feels ancient
and enormous. This thing is bigger than you. Desperate urgency.
More intense than standard combat. Loopable, 60 seconds.
No lyrics. Should feel like fighting something that cannot
be reasoned with.
```

---

## Part 2: SFX Clips (9 total)

Use CoPlay `generate_sfx` for each. Save all to `Assets/_Game/Audio/SFX/`.

| Filename | Description |
|---|---|
| `sfx_attack_shell.wav` | A hard crack — stone or bone plate taking a solid hit. Short, sharp, percussive. |
| `sfx_attack_flesh.wav` | A wet thud — impact on exposed muscle or hide. Heavier, deeper than shell hit. |
| `sfx_attack_miss.wav` | A whoosh — weapon cutting air. Light, fast, no impact. |
| `sfx_card_play.wav` | A short papery shuffle-click — a card being slapped down onto a surface. Satisfying. |
| `sfx_part_break.wav` | A loud crack and crumble — bone or shell shattering. More dramatic than shell hit. Reverb tail. |
| `sfx_hunter_collapse.wav` | A heavy thud and exhale — a person falling to the ground. Weight and impact. No dramatic music cue here. |
| `sfx_monster_defeated.wav` | A massive crash and rumble as something large falls. Deep impact, dust and debris sound. |
| `sfx_death_sting.wav` | A single mournful string note, then silence. 3 seconds total. Sparse, grief-stricken. No drama. |
| `sfx_ui_click.wav` | A quiet stone-on-stone click — like pressing a carved button. Short, satisfying, subtle. |

---

## Part 3: Update AudioManager.cs

Open `Assets/_Game/Scripts/Core.Systems/AudioManager.cs`.

**Add a new AudioContext value** to `Enums.cs`:
```csharp
// In the AudioContext enum, add:
MainMenu,
```

**Add field and case to AudioManager:**
```csharp
// In the [Header("Music")] block, add:
[SerializeField] private AudioClip _mainMenuMusic;

// In the AudioContext switch in SetMusicContext(), add:
AudioContext.MainMenu => _mainMenuMusic,
```

**Add a new SFX field:**
```csharp
// In the [Header("SFX")] block, add:
[SerializeField] private AudioClip _sfxUiClick;

// In the PlaySFX() switch, add:
"UI_Click" => _sfxUiClick,
```

**Call from MainMenuController.OnEnable():**
```csharp
// Add at the end of OnEnable() in MainMenuController:
if (AudioManager.Instance != null)
    AudioManager.Instance.SetMusicContext(AudioContext.MainMenu);
```

---

## Part 4: Wire All Clips in Inspector

1. In the Hierarchy, find the `AudioManager` GameObject (should be in Settlement or a persistent scene)
2. In the Inspector, find `AudioManager` component
3. Assign each clip to its matching field:

| Inspector Field | Assign This File |
|---|---|
| Main Menu Music | music_main_menu.wav |
| Settlement Early | music_settlement_early.wav |
| Settlement Late | music_settlement_late.wav |
| Hunt Travel | music_hunt_travel.wav |
| Combat Standard | music_combat_standard.wav |
| Combat Overlord | music_combat_overlord.wav |
| Sfx Shell Hit | sfx_attack_shell.wav |
| Sfx Flesh Hit | sfx_attack_flesh.wav |
| Sfx Miss | sfx_attack_miss.wav |
| Sfx Card Play | sfx_card_play.wav |
| Sfx Part Break | sfx_part_break.wav |
| Sfx Hunter Collapse | sfx_hunter_collapse.wav |
| Sfx Monster Defeated | sfx_monster_defeated.wav |
| Sfx Death Sting | sfx_death_sting.wav |
| Sfx Ui Click | sfx_ui_click.wav |

**To assign:** Click the small circle (⊙) next to each field → find and select the audio file in the popup window.

---

## Part 5: Audio Import Settings

Select all audio files in the Project window (hold Ctrl to multi-select):
- **Audio Clip** settings in Inspector:
  - **Load Type:** Streaming (for music), Decompress On Load (for SFX)
  - **Compression Format:** Vorbis
  - **Quality:** 70 (music), 100 (SFX)
- Click **Apply**

---

## Verification Test

- [ ] All 6 music files exist in `Assets/_Game/Audio/Music/`
- [ ] All 9 SFX files exist in `Assets/_Game/Audio/SFX/`
- [ ] AudioManager compiles without errors after Enums.cs change
- [ ] Load MainMenu scene — main menu music starts playing
- [ ] Load Settlement — settlement early music crossfades in
- [ ] Load CombatScene — combat standard music crossfades in
- [ ] Hunter collapse in combat → death sting plays, 2s silence, music resumes
- [ ] PlaySFX("UI_Click") fires without error
- [ ] No audio sources fire in editor without pressing Play

---

## Next Session

**File:** `_Docs/Stage_08/STAGE_08_E.md`
**Covers:** Settings screen (volume sliders, fullscreen toggle) and in-game pause menu for both combat and settlement scenes
