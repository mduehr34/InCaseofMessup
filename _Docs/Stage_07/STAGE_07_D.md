<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 7-D | Art Batch — Import All Monster Sprites
Status: Stage 7-C complete. All 8 character sprites imported
with Point filtering verified.
Task: Import idle sprites for all 8 standard monsters plus
all 4 overlords. Apply correct import settings.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_07/STAGE_07_D.md

Then confirm:
- Naming convention matches the table below exactly
- Point (No Filter) applied to all sprites
- All standard monsters are 64×64
- All overlords are 96×96

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

### Standard Monsters — all 64×64

**Save path:** `Assets/_Game/Art/Generated/Monsters/`

| File | Monster | Canvas Size | Key Visual Check |
|---|---|---|---|
| `monster_gaunt_s.png` | The Gaunt | 64×64 | Blind wolf — no eyes, oversized jaw, Marrow gold veins |
| `monster_bog_caller_s.png` | Bog Caller | 64×64 | Snapping turtle, shell fused with Marrow crystal growths |
| `monster_thornback_s.png` | Thornback | 64×64 | Massive boar, bone-white spikes erupting from dorsal ridge |
| `monster_ivory_stampede_s.png` | Ivory Stampede (single) | 64×64 | Marrow-tainted elephant, bone-white tusks, gold vein patterns |
| `monster_shriek_s.png` | Shriek | 64×64 | Horse-sized great horned owl, Marrow-mutated, hollow eye sockets glowing |
| `monster_rotmother_s.png` | Rotmother | 64×64 | Massive brown bear, fungal growth erupting across shoulders and spine |
| `monster_gilded_serpent_s.png` | Gilded Serpent | 64×64 | Coiled strike pose, gold iridescent scales weeping golden resin |
| `monster_spite_s.png` | The Spite | 64×64 | Massive Marrow-enhanced honey badger, low and wide, Marrow gold running along the dorsal stripe — looks deceptively small until it moves |

> ⚑ `monster_gaunt_s.png` was already imported in Stage 7-B. Verify its settings match before continuing.

---

### Overlords — all 96×96

**Save path:** `Assets/_Game/Art/Generated/Overlords/`

| File | Overlord | Canvas Size | Key Visual Check |
|---|---|---|---|
| `overlord_siltborn_s.png` | The Siltborn (OVR-01) | 96×96 | Ancient crocodilian, scales mineralized to stone plates, settlers mistook it for terrain — visibly massive |
| `overlord_penitent_s.png` | The Penitent (OVR-02) | 96×96 | Massive Marrow-corrupted primate, hunched in a permanent bowing posture — looks like it's supplicating something. Does not look like it hunts for food |
| `overlord_pale_stag_ascendant_s.png` | The Pale Stag Ascendant (OVR-03) | 96×96 | Ancient stag-like creature with a full second skeleton grown outside its body — outer bone lattice encasing the inner form. No known relation to any living monster in the roster |
| `overlord_suture_s.png` | The Suture (OVR-04) | 96×96 | Colossal prehistoric predator, vast and blind, Marrow gold veins like a map across its body — does not look like it hunts food |

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

- [ ] All 8 standard monster sprites (`monster_*_s.png`) exist in `Assets/_Game/Art/Generated/Monsters/`
- [ ] All 4 overlord sprites (`overlord_*_s.png`) exist in `Assets/_Game/Art/Generated/Overlords/`
- [ ] All 12 sprites imported with Point (No Filter), PPU 16, Compression None
- [ ] All standard monster sprites are 64×64
- [ ] All overlord sprites are 96×96 — visually larger and more imposing than standard monsters
- [ ] The Spite — Marrow gold dorsal stripe is clearly visible
- [ ] Pale Stag Ascendant has visible outer bone skeleton layered over the creature — inner form visible beneath

---

## Next Session

**File:** `_Docs/Stage_07/STAGE_07_E.md`  
**Covers:** Import UI art and all 5 settlement structure sprites, then wire structures into SettlementScreenController
