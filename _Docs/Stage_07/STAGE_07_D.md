<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 7-D | Art Batch — Import All Monster Sprites
Status: Stage 7-C complete. All 8 character sprites imported
with Point filtering verified.
Task: Import idle sprites for all 5 huntable monsters plus
the Pack Wolf and the Suture. Apply correct import settings.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_07/STAGE_07_D.md

Then confirm:
- Naming convention matches the table below exactly
- Point (No Filter) applied to all sprites
- Suture sprite is noticeably larger than all others
- Pack Wolf sprite is noticeably smaller than the Gaunt

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 7-D: Art Batch — Import All Monster Sprites

**Resuming from:** Stage 7-C complete — all 8 character sprites imported  
**Done when:** All 7 monster sprite files imported and imported with Point filtering  
**Commit:** `"7D: All monster sprites imported with Point filtering verified"`  
**Next session:** STAGE_07_E.md  

---

## Sprites to Import

Use the Art Generator (Window → MnM → Art Generator), subfolder: **Monsters**.

All monsters use full 4-way facing (`_s` `_n` `_e` `_w`). **This session imports south (`_s`) only** — the default facing used on the combat grid. Remaining directions are imported when animation work begins.

| File | Monster | Canvas Size | Key Visual Check |
|---|---|---|---|
| `monster_gaunt_s.png` | The Gaunt | 64×64 | Blind wolf — no eyes, oversized jaw, Marrow gold veins |
| `monster_thornback_s.png` | Thornback Patriarch | 64×64 | Massive boar, bone-white spikes on spine |
| `monster_pack_wolf_s.png` | Pack Wolf (single) | 48×48 | Smaller than Gaunt, Marrow-tainted fur patches |
| `monster_gilded_serpent_s.png` | Gilded Serpent | 80×48 | Coiled strike pose, gold iridescent scales |
| `monster_pale_stag_s.png` | Pale Stag | 64×80 | Enormous albino stag, branching antlers with golden glow |
| `monster_suture_s.png` | The Suture | 96×96 | Many-limbed biological horror, Marrow gold veins everywhere |

**Save path:** `Assets/_Game/Art/Generated/Monsters/`

> ⚑ `monster_gaunt_s.png` was already imported in Stage 7-B. Verify its settings match before continuing.

---

## Import Settings (Apply to All)

```
Texture Type:     Sprite (2D and UI)
Pixels Per Unit:  16
Filter Mode:      Point (No Filter)
Compression:      None
```

Click **Apply** after each sprite.

---

## Verification Test

- [ ] All 6 south-facing sprite files (`monster_*_s.png`) exist in `Assets/_Game/Art/Generated/Monsters/`
- [ ] All imported with Point (No Filter)
- [ ] Suture sprite is visually larger and more imposing than all other monsters
- [ ] Pack Wolf sprite is visually smaller than the Gaunt
- [ ] Pale Stag has visible golden glow on antler tips

---

## Next Session

**File:** `_Docs/Stage_07/STAGE_07_E.md`  
**Covers:** Import UI art and all 5 settlement structure sprites, then wire structures into SettlementScreenController
