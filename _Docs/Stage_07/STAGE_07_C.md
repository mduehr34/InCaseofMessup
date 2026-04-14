<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 7-C | Art Batch — All 8 Character Base Sprites
Status: Stage 7-B complete. Canonical prompt template locked.
Aldric and Gaunt approved sprites saved.
Task: Generate all 8 character base sprites (4 male builds,
4 female builds) using the LOCKED prompt template from
Stage 7-B. Each sprite = idle frame only at this stage.
Animation frames are a separate batch.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_07/STAGE_07_C.md
- Assets/_Game/Art/Generated/Characters/aldric_approved.png
  (use as visual reference for consistency)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 7-C: Art Batch — All 8 Character Base Sprites

**Resuming from:** Stage 7-B complete — locked prompt template approved  
**Done when:** All 8 approved character idle sprites saved with correct import settings  
**Commit:** `"7C: All 8 character base sprites generated and saved"`  
**Next session:** STAGE_07_D.md  

---

## Characters to Generate

Use the LOCKED PROMPT from Stage 7-B for each. Vary only the build description.

| File Name | Build | Sex | Build Description |
|---|---|---|---|
| `char_aethel_idle.png` | Aethel | Male | lean, narrow shoulders, wiry frame |
| `char_beorn_idle.png` | Beorn | Male | stocky, broad chest, thick arms, short neck |
| `char_cyne_idle.png` | Cyne | Male | average build, balanced proportions |
| `char_duna_idle.png` | Duna | Male | muscular, wide shoulders, powerful legs |
| `char_eira_idle.png` | Eira | Female | lean, narrow hips, long limbs |
| `char_freya_idle.png` | Freya | Female | athletic, defined muscle, balanced frame |
| `char_gerd_idle.png` | Gerd | Female | average female build, sturdy |
| `char_hild_idle.png` | Hild | Female | muscular female, broad shoulders, powerful |

**Save path:** `Assets/_Game/Art/Generated/Characters/`

---

## Sprite Import Settings

After saving each sprite, set these import settings in the Unity Inspector:

```
Texture Type:     Sprite (2D and UI)
Pixels Per Unit:  16   (32px sprite = 2 Unity units tall)
Filter Mode:      Point (No Filter)   ← Critical for pixel art
Compression:      None
Max Size:         64
```

> ⚑ Filter Mode **must** be Point (No Filter). Any other setting blurs pixel art at game resolution.

---

## Verification Test

- [ ] All 8 sprite files exist in `Assets/_Game/Art/Generated/Characters/`
- [ ] All import settings set to Point (No Filter)
- [ ] Sprites visible in Unity Sprite Editor without blurring
- [ ] Male builds look visually distinct from each other
- [ ] Female builds look visually distinct from each other

---

## Next Session

**File:** `_Docs/Stage_07/STAGE_07_D.md`  
**Covers:** All monster sprites — Gaunt, Thornback, Pack Wolf, Gilded Serpent, Pale Stag, Suture
