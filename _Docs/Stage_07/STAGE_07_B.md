<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 7-B | Import Pipeline Verification — First Two Sprites
Status: Stage 7-A complete. Art Importer window opens and
copies PNGs to the project correctly.
Task: Import the first two sprites (Aldric idle + Gaunt idle)
using the Art Importer window. Confirm folder structure,
naming convention, and import settings are all correct.
This verifies the pipeline before batch-importing everything.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_07/STAGE_07_B.md

Then confirm:
- That the naming convention below will be followed for all
  subsequent sprites
- That Point (No Filter) import settings are applied
- That the two test sprites are visible in Unity without
  blurring at game resolution
- What you will NOT import this session (everything else —
  that is Sessions 7-C through 7-E)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 7-B: Import Pipeline Verification — First Two Sprites

**Resuming from:** Stage 7-A complete — Art Importer window works  
**Done when:** `char_aethel_idle_s.png` and `monster_gaunt.png` are imported, import settings verified, visible in Unity Sprite Editor without blurring  
**Commit:** `"7B: Import pipeline verified — Aldric idle and Gaunt idle sprites imported"`  
**Next session:** STAGE_07_C.md  

---

## Why This Session Exists

Catching import setting mistakes on two sprites is far cheaper than catching them after all 30+ sprites are imported. One session confirming the pipeline pays off across all subsequent import sessions.

---

## GDD Appendix B — Style Reference

All sprites must match this visual standard. Use when reviewing sprites before importing.

```
Style:    Dark pixel art, 16-bit era detail level, high contrast, desaturated palette
Palette:  Ash grey (#8A8A8A), bone white (#D4CCBA), dried blood brown (#4A2020),
          Marrow gold (#B8860B), shadow black (#0A0A0C), cold blue-green ambient
Lighting: Torchlight and fire primary. Moonlight for outdoor. NEVER warm sunlight.
Linework: Bold pixel outlines on characters and monsters. Thinner on environment.
```

---

## Canonical Sprite Naming Convention

All sprites follow this naming scheme. It must be consistent — SO assets and code reference these exact filenames.

**Direction tokens** (used by characters and future monster facing):
- `_s` = south / facing camera (default, always generated first)
- `_n` = north / facing away
- `_e` = east / facing right
- `_w` = west / facing left

**Characters (idle per direction):** `char_[buildname]_idle_[dir].png`
- `char_aethel_idle_s.png`, `char_aethel_idle_n.png`, etc.
- Build name is the character archetype — NOT the player-given hunter name.
  The player names their hunter "Aldric"; the file stays `char_aethel_idle_s.png`.

**Character animation frames:** `[firstname]_[state]_[dir]_[frame].png`
- `aldric_walk_s_01.png`, `aldric_attack_e_02.png`, etc.

**Monsters (per direction):** `monster_[name]_[dir].png`
- `monster_gaunt_s.png`, `monster_gaunt_n.png`, `monster_gaunt_e.png`, `monster_gaunt_w.png`
- Full 4-way for all monsters — each direction is unique art (no horizontal flip).
- Stage 7-B/D import south (`_s`) only. Remaining directions imported when animation work begins.

**UI elements:** `ui_[descriptor].png`
- `ui_stone_panel_bg.png`, `ui_card_frame.png`, etc.

**Settlement buildings:** `building_[name].png`
- `building_boneworks.png`, `building_herbalist.png`, etc.

---

## Import Step 1: Aldric Idle

**Your file:** `char_aethel_idle_s.png` (32×64 px, transparent background, facing camera)

1. Open Window → MnM → Art Generator
2. Browse to your `char_aethel_idle_s.png`
3. Set **Subfolder:** `Characters`, **File Name:** `char_aethel_idle_s`
4. Click **SAVE TO PROJECT**
5. Find `Assets/_Game/Art/Generated/Characters/char_aethel_idle_s.png` in the Project window

---

## Import Step 2: Gaunt Idle

**Your file:** `monster_gaunt_s.png` (64×64 px, transparent background, facing camera)

1. Browse to your `monster_gaunt_s.png`
2. Set **Subfolder:** `Monsters`, **File Name:** `monster_gaunt_s`
3. Click **SAVE TO PROJECT**

---

## Apply Import Settings (Both Sprites)

Select each sprite in the Project window and set these in the Inspector:

```
Texture Type:     Sprite (2D and UI)
Pixels Per Unit:  16   (32px character = 2 Unity units; 64px monster = 4 units)
Filter Mode:      Point (No Filter)   ← Critical for pixel art — NEVER Bilinear
Compression:      None
Max Size:         64
```

Click **Apply** after setting each sprite.

> ⚑ Filter Mode **must** be Point (No Filter). Any other setting blurs pixel art at game resolution. Check this on every sprite you import.

---

## Verification Test

- [ ] `Assets/_Game/Art/Generated/Characters/char_aethel_idle_s.png` exists
- [ ] `Assets/_Game/Art/Generated/Monsters/monster_gaunt_s.png` exists
- [ ] Both sprites have Filter Mode: Point (No Filter) in Inspector
- [ ] Both sprites have Pixels Per Unit: 16
- [ ] Character sprite is 32×64 — visible in Sprite Editor without blurring
- [ ] Monster sprite is 64×64 — visible in Sprite Editor without blurring
- [ ] Sprites display correctly at game scale (not oversized or undersized)

---

## Next Session

**File:** `_Docs/Stage_07/STAGE_07_C.md`  
**Covers:** Import all 8 character base sprites and apply correct import settings
