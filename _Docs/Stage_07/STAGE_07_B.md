<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 7-B | Style Lock — Generate & Approve Canonical Prompt
Status: Stage 7-A complete. Art Generator window opens and
calls the API successfully.
Task: Generate 3–5 variants of Aldric (Aethel build, idle
frame). Review against the GDD style description. Tune the
prompt until the output matches. Lock that prompt as the
canonical style template for all subsequent generation.
Also generate and approve the Gaunt Standard idle sprite.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_07/STAGE_07_B.md

Then confirm:
- That you will generate multiple variants, not just one
- That approved sprites are saved to the correct folders
- That the locked prompt is recorded at the end
  of this session for use in all future generation
- What you will NOT generate yet (gear, other monsters —
  those are Sessions 7-C and 7-D)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 7-B: Style Lock — Generate & Approve Canonical Prompt

**Resuming from:** Stage 7-A complete — Art Generator window works  
**Done when:** At least one approved character sprite and one approved monster sprite saved to disk; locked prompt template recorded at the bottom of this session  
**Commit:** `"7B: Style lock — canonical prompt template approved, Aldric and Gaunt sprites saved"`  
**Next session:** STAGE_07_C.md  

---

## Why This Session Exists

Every other art generation session depends on having a locked style. If the first batch of characters looks wrong, fixing it later means regenerating everything. Spending one session getting the prompt exactly right pays off across all 17 art sessions that follow.

---

## Generation Target 1: Aldric (Male, Aethel Build)

Generate 3 variants with these prompts in the Art Generator tool. Use the tool's Quick Prompt or type the suffix directly.

**Variant A — Base prompt:**
```
32x64 pixel sprite sheet, male warrior survivor, lean athletic build, 
primitive leather wraps around torso and legs, bone bead necklace, 
short dark hair, desaturated tanned skin, standing idle pose facing 
right, single frame, transparent background, pixel art
```

**Variant B — More specific to GDD:**
```
32x64 pixel sprite, SNES-era RPG character, male hunter, lean frame, 
crude leather armor strips, sinew-wrapped wrists, bone talisman, 
torch-lit warm shadow on left side, dark desaturated skin tones, 
idle stance slight weight on right foot, facing right, no weapons, 
pixel art bold outline, transparent background
```

**Variant C — Pushing the aesthetic:**
```
32x64 pixel art character sprite, primitive survivor male, lean build, 
bone-white loincloth wrap, leather chest binding, crude knee wraps, 
single bone earring, scar on left cheek, high contrast ash grey 
shadows, minimal color, Marrow gold accent on belt bone clasp, 
idle animation frame facing right, transparent background
```

Save each to: `Assets/_Game/Art/Generated/Characters/`
- `aldric_variant_a.png`
- `aldric_variant_b.png`
- `aldric_variant_c.png`

**Review against GDD Appendix B.1:**
- ✓ Dark pixel art, 16-bit era detail
- ✓ Desaturated palette — no bright colours
- ✓ Bold pixel outlines
- ✓ Torchlight primary (warm shadow on one side)
- ✓ No warm sunlight

Pick the best variant. Note what worked and what didn't.

---

## Generation Target 2: The Gaunt (Standard, Idle)

**Variant A:**
```
64x64 pixel art creature sprite, enormous wolf, skeletal emaciated 
frame, no eyes — sealed over with scar tissue, oversized jaw with 
exposed fang rows, Marrow-blackened fur with gold (#B8860B) vein 
traces, facing left idle pose, menacing stance, dark pixel art, 
bold outline, transparent background
```

**Variant B:**
```
64x64 pixel art monster sprite, blind wolf creature, grotesquely 
large, ribs visible through patchy fur, eyeless head with 
vibration-sensing whiskers, crouched hunting stance, facing left, 
ash grey and bone white palette, Marrow gold veins on haunches, 
pixel art SNES era, bold pixel outlines, transparent background
```

Save to: `Assets/_Game/Art/Generated/Monsters/`
- `gaunt_standard_variant_a.png`
- `gaunt_standard_variant_b.png`

---

## Tuning Guide

If variants don't match the target style, adjust these elements:

| Problem | Fix |
|---|---|
| Too colourful | Add: "extremely desaturated, near-greyscale with only Marrow gold accent" |
| Lines too thin | Add: "thick 1-2px bold pixel outline on all edges" |
| Wrong lighting | Add: "torchlight from right side, deep shadow on left, no ambient light" |
| Too detailed / painterly | Add: "strict pixel art, visible pixel grid, no anti-aliasing, 16-bit console era" |
| Wrong scale feel | Add: "proportioned for 32x64px canvas, chunky readable silhouette" |
| Warm/sunny look | Add: "cold dim lighting, no sunlight, underground or night setting" |

---

## Session Output — Record the Locked Prompts

After reviewing and approving, record the winning prompts here so all future sessions use the same template:

**LOCKED CHARACTER PROMPT SUFFIX:**
```
[FILL IN AFTER APPROVAL — copy the variant that best matched the GDD]
```

**LOCKED MONSTER PROMPT SUFFIX:**
```
[FILL IN AFTER APPROVAL — copy the variant that best matched the GDD]
```

**Also update ArtGeneratorWindow.cs QuickPrompts array** with the two locked prompts so they appear as the top options in the tool.

---

## Verification Test

- [ ] At least 3 character variants generated and saved
- [ ] At least 2 monster variants generated and saved
- [ ] One character sprite approved — saved as `aldric_approved.png`
- [ ] One monster sprite approved — saved as `gaunt_standard_approved.png`
- [ ] Locked prompt template recorded at the bottom of this session file
- [ ] ArtGeneratorWindow QuickPrompts updated with locked prompts

---

## Next Session

**File:** `_Docs/Stage_07/STAGE_07_C.md`  
**Covers:** Batch generate all 8 character base sprites (all builds, both genders) using the locked prompt
