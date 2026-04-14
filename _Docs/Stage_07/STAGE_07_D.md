<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 7-D | Art Batch — All Monster Sprites
Status: Stage 7-C complete. All 8 character sprites saved
with Point filtering verified.
Task: Generate idle sprites for all 5 huntable monsters
plus the Suture (Year 30 overlord). Use locked style template.
Also generate the Pack Wolf (single wolf — 3 used in Pack fight).

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_07/STAGE_07_D.md
- Assets/_Game/Art/Generated/Monsters/gaunt_standard_approved.png
  (use as visual reference for style consistency)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 7-D: Art Batch — All Monster Sprites

**Resuming from:** Stage 7-C complete — all 8 character sprites saved  
**Done when:** All 7 monster sprite files (6 monsters + pack wolf) approved, saved, and imported with Point filtering  
**Commit:** `"7D: All monster sprites generated and saved"`  
**Next session:** STAGE_07_E.md  

---

## Monsters to Generate

| File | Monster | Size | Key Visual Details |
|---|---|---|---|
| `monster_gaunt.png` | The Gaunt | 64×64 | Blind wolf, no eyes — sealed with scar tissue, oversized jaw, Marrow gold veins, skeletal frame |
| `monster_thornback.png` | Thornback Patriarch | 64×64 | Massive boar, row of bone-white spikes on spine, thick scarred hide, small angry eyes |
| `monster_pack_wolf.png` | Pack Wolf (×1) | 48×48 | Smaller than Gaunt, yellow eyes, Marrow-tainted fur patches |
| `monster_gilded_serpent.png` | Gilded Serpent | 80×48 | Vast serpent coiled in strike pose, gold (#B8860B) iridescent scales, Marrow-luminescent |
| `monster_pale_stag.png` | Pale Stag | 64×80 | Enormous albino stag, massive branching antlers with faint golden glow at tips |
| `monster_suture.png` | The Suture | 96×96 | Massive biological horror, many-limbed, Marrow gold veins covering entire body, no discernible face |

**Save path:** `Assets/_Game/Art/Generated/Monsters/`

---

## Prompt Suffixes Per Monster

**Thornback Patriarch:**
```
64x64 pixel art monster sprite, enormous boar creature, row of 
bone-white spikes erupting from spine, thick scarred hide, small 
angry eyes, Marrow gold traces along spine, facing left, aggressive 
stance, dark pixel art, bold outline, transparent background
```

**Gilded Serpent:**
```
80x48 pixel art monster sprite, vast serpent coiled in strike pose, 
gold (#B8860B) iridescent scales, faintly luminescent from Marrow 
saturation, elongated fangs, facing left, dark pixel art, bold 
outline, transparent background
```

**Pale Stag:**
```
64x80 pixel art monster sprite, enormous pale white stag standing 
tall, massive branching antlers with faint golden glow at tips, 
powerful muscular frame, facing right, regal and unsettling, dark 
pixel art, bold outline, transparent background
```

**The Suture:**
```
96x96 pixel art boss monster sprite, massive biological horror 
entity, many-limbed, Marrow gold veins covering entire body, no 
discernible face, ancient and vast, facing center, radiating dread, 
dark pixel art, extra bold outline, transparent background
```

---

## Sprite Import Settings

Same as Session 7-C for all monster sprites:

```
Texture Type:     Sprite (2D and UI)
Pixels Per Unit:  16
Filter Mode:      Point (No Filter)
Compression:      None
```

---

## Verification Test

- [ ] All 7 sprite files saved to `Assets/_Game/Art/Generated/Monsters/`
- [ ] All imported with Point (No Filter)
- [ ] Suture sprite is visually larger and more imposing than all other monsters
- [ ] Pack Wolf sprite is visually smaller than the Gaunt
- [ ] Pale Stag antlers have visible golden glow effect

---

## Next Session

**File:** `_Docs/Stage_07/STAGE_07_E.md`  
**Covers:** UI art — stone panel textures, card frame, button states, aggro token + all 5 settlement structure sprites
