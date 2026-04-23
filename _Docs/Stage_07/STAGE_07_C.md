<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 7-C | Art Batch — Import All 8 Character Base Sprites
Status: Stage 7-B complete. Import pipeline verified.
Aldric idle and Gaunt idle imported with correct settings.
Task: Import all 8 character idle sprites using the Art
Importer window. Apply Point filtering and correct import
settings to each. Verify all are visible in Unity without
blurring.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_07/STAGE_07_C.md

Then confirm:
- Naming convention matches the table below exactly
- Point (No Filter) applied to all 8 sprites
- Pixels Per Unit is 16 on all sprites
- What you will NOT import this session (monsters, UI — 7-D/E)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 7-C: Art Batch — Import All 8 Character Base Sprites

**Resuming from:** Stage 7-B complete — import pipeline verified  
**Done when:** All 8 approved character idle sprites imported with correct settings  
**Commit:** `"7C: All 8 character base sprites imported with Point filtering verified"`  
**Next session:** STAGE_07_D.md  

---

## Sprites to Import

Place your finished PNG files in `Assets/_Game/Art/Generated/Characters/` using the Art Importer window (Window → MnM → Art Importer), then apply import settings.

All filenames include the south (`_s`) direction token. This is the default facing (toward camera). North/east/west variants are out of scope until animation work begins.

| File Name | Build | Sex | Canvas Size |
|---|---|---|---|
| `char_aethel_idle_s.png` | Aethel | Male | 32×64 |
| `char_beorn_idle_s.png` | Beorn | Male | 32×64 |
| `char_cyne_idle_s.png` | Cyne | Male | 32×64 |
| `char_duna_idle_s.png` | Duna | Male | 32×64 |
| `char_eira_idle_s.png` | Eira | Female | 32×64 |
| `char_freya_idle_s.png` | Freya | Female | 32×64 |
| `char_gerd_idle_s.png` | Gerd | Female | 32×64 |
| `char_hild_idle_s.png` | Hild | Female | 32×64 |

**Save path:** `Assets/_Game/Art/Generated/Characters/`

> ⚑ `char_aethel_idle_s.png` was already imported in Stage 7-B. Verify its settings match before continuing.

---

## Build Visual Reference

Each build should be visually distinct when laid out side by side in the Project window:

| Build | Key Silhouette Trait |
|---|---|
| Aethel (M) | Lean, narrow shoulders, wiry frame |
| Beorn (M) | Stocky, broad chest, short neck |
| Cyne (M) | Average build, balanced proportions |
| Duna (M) | Muscular, wide shoulders, powerful legs |
| Eira (F) | Lean, narrow hips, long limbs |
| Freya (F) | Athletic, defined muscle, balanced frame |
| Gerd (F) | Average female build, sturdy |
| Hild (F) | Muscular female, broad shoulders, powerful |

---

## Import Settings (Apply to All 8)

Select each sprite in the Project window and set in the Inspector:

```
Texture Type:     Sprite (2D and UI)
Pixels Per Unit:  16
Filter Mode:      Point (No Filter)   ← Never Bilinear or Trilinear
Compression:      None
Max Size:         64
```

Click **Apply** after each sprite.

---

## Verification Test

- [ ] All 8 sprite files (`char_*_idle_s.png`) exist in `Assets/_Game/Art/Generated/Characters/`
- [ ] All 8 sprites have Filter Mode: Point (No Filter)
- [ ] All 8 sprites have Pixels Per Unit: 16
- [ ] All 8 sprites visible in Sprite Editor without blurring
- [ ] Male builds are visually distinct from each other
- [ ] Female builds are visually distinct from each other

---

## Next Session

**File:** `_Docs/Stage_07/STAGE_07_D.md`  
**Covers:** Import all monster sprites — Gaunt, Bog Caller, Thornback, Ivory Stampede (single), Shriek, Rotmother, Gilded Serpent, The Spite + 4 overlords
