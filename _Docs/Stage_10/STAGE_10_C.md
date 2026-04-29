<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 10-C | Monster Sprite Generation — All 8 Standard + 4 Overlords
Status: Stage 10-B complete. Lifecycle cards wired into combat.
Task: Generate sprite art for every monster using CoPlay's image
generation MCP tool. All monsters currently use placeholder grey
tokens in combat. This session replaces every placeholder with
a generated sprite, imports it into Unity, assigns it to the
correct MonsterSO, and confirms it renders on the combat grid.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_10/STAGE_10_C.md
- Assets/_Game/Scripts/Core.Data/MonsterSO.cs      ← tokenSprite field
- Assets/_Game/Data/Monsters/                       ← all 12 MonsterSO assets

Then confirm:
- MonsterSO has a Sprite field named tokenSprite (or combatSprite)
- Monster tokens in CombatScene read from MonsterSO.tokenSprite
- You have access to CoPlay MCP (mcp__coplay-mcp__generate_or_edit_images)
- Art style: dark, hand-painted fantasy pixel art; 128×128 per token
- Output folder: Assets/_Game/Art/Monsters/Generated/

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 10-C: Monster Sprite Generation — All 8 Standard + 4 Overlords

**Resuming from:** Stage 10-B complete — lifecycle card effects enforced in combat
**Done when:** All 12 monsters have generated sprites assigned to their MonsterSO; each displays correctly on the combat grid
**Commit:** `"10C: Monster sprites generated and assigned — all 12 monsters visual"`
**Next session:** STAGE_10_D.md

---

## Pre-Flight: Confirm tokenSprite Field Exists

Open `MonsterSO.cs` and confirm a Sprite field exists:

```csharp
[Header("Visual")]
public Sprite tokenSprite;      // Combat grid token (128×128)
public Sprite portraitSprite;   // Settlement codex display (64×64)
```

If either is missing, add it now. Then confirm the MonsterTokenController in CombatScene reads `monsterSO.tokenSprite` and assigns it to the SpriteRenderer. If it doesn't:

```csharp
// In MonsterTokenController.Initialise(MonsterSO so):
var sr = GetComponent<SpriteRenderer>();
if (so.tokenSprite != null) sr.sprite = so.tokenSprite;
```

---

## Art Style Brief (Apply to Every Generation Call)

Use this description as the base for every image generation prompt below:

> "Top-down view, dark fantasy, hand-painted style with visible brushwork, muted earth tones with deep shadows, single creature visible against a near-black background, small amount of warm amber/red atmospheric rim light from below, slightly painterly with sharp silhouette. Suitable for a 128×128 sprite token."

---

## Generation Instructions

For each monster, call `mcp__coplay-mcp__generate_or_edit_images` with the prompt below. Save the result to the specified path using `mcp__coplay-mcp__save_as` or equivalent. After saving, import into Unity by refreshing the Asset Database.

**IMPORTANT:** Generate one monster at a time. After each generation:
1. Save the file to `Assets/_Game/Art/Monsters/Generated/[filename]`
2. In Unity, set the Texture Type to **Sprite (2D and UI)**, Filter Mode **Point**, Compression **None**
3. Assign the sprite to the correct MonsterSO's `tokenSprite` field
4. Confirm it renders in a test scene or inspector preview before proceeding to the next

---

## Standard Monsters (8 total)

### Monster 1 — The Gaunt
**MonsterSO:** `Monster_TheGaunt.asset`
**Sprite filename:** `Monster_TheGaunt_Token.png`

**Generation prompt:**
> "Top-down view dark fantasy hand-painted sprite token. A gaunt humanoid creature, emaciated and tall, with elongated limbs and hollow eye sockets glowing faint amber. Wrapped in dark dried sinew and frayed leather strips. Skull-like face, hands ending in bone-white claws. Deep black background with warm amber rim lighting from below. 128x128, no background detail."

---

### Monster 2 — Thornback
**MonsterSO:** `Thornback_Standard.asset`
**Sprite filename:** `Monster_Thornback_Token.png`

**Generation prompt:**
> "Top-down view dark fantasy hand-painted sprite token. A quadruped beast the size of a bear, covered in overlapping bone-white thorns along its back and shoulders. Dark brown leathery hide, four heavy limbs with clawed feet, a broad flat head with small deep-set eyes. Deep black background with cool blue-white rim light highlighting the thorns. 128x128, no background detail."

---

### Monster 3 — The Ivory Stampede
**MonsterSO:** `IvoryStampede_Standard.asset`
**Sprite filename:** `Monster_IvoryStampede_Alpha_Token.png`

**Generation prompt:**
> "Top-down view dark fantasy hand-painted sprite token. A massive ivory-coloured horned beast resembling a prehistoric bull, muscle-bound with yellowed ivory horns and a thick skull plate. Dusty ivory and grey hide, eyes red with fury. Deep black background with gold-warm rim light catching the horns. 128x128, no background detail."

Also generate a flanker variant (smaller, same species):
**Sprite filename:** `Monster_IvoryStampede_Flanker_Token.png`
**Generation prompt:** Same as above but noticeably smaller and lighter-coloured, suggesting a younger animal.

---

### Monster 4 — Bog Caller
**MonsterSO:** `BogCaller_Standard.asset`
**Sprite filename:** `Monster_BogCaller_Token.png`

**Generation prompt:**
> "Top-down view dark fantasy hand-painted sprite token. A bloated amphibian creature with mottled dark green and grey mottled skin, a wide fanged mouth, and thick stubby limbs. Patches of bioluminescent teal on its flanks and belly. Trails of dark mist wisping from its skin. Deep black background with sickly teal-green rim light. 128x128, no background detail."

---

### Monster 5 — The Shriek
**MonsterSO:** `Shriek_Standard.asset`
**Sprite filename:** `Monster_Shriek_Token.png`

**Generation prompt:**
> "Top-down view dark fantasy hand-painted sprite token. A sleek winged predator viewed from above, bat-like wingspan folded, elongated neck and narrow skull, hollow black eyes, thin membrane wings with pale veins. Matte black hide. Deep black background with cold white-silver rim light catching wing tips and skull ridge. 128x128, no background detail."

---

### Monster 6 — The Rotmother
**MonsterSO:** `Rotmother_Nightmare.asset`
**Sprite filename:** `Monster_Rotmother_Token.png`

**Generation prompt:**
> "Top-down view dark fantasy hand-painted sprite token. A massive bloated insectoid creature with a bulging abdomen full of rot-spawn larvae, six jointed legs, a wide chitinous carapace mottled black and dark olive-green, mandibles dripping dark ichor. Deep black background with sickly yellow-green rim light. 128x128, no background detail."

---

### Monster 7 — The Gilded Serpent
**MonsterSO:** `GildedSerpent_Standard.asset`
**Sprite filename:** `Monster_GildedSerpent_Token.png`

**Generation prompt:**
> "Top-down view dark fantasy hand-painted sprite token. A massive serpent coiled, golden-bronze overlapping scales catching light, flat angular head with reflective amber eyes, forked tongue, muscular body. The scales have a faint mirrored sheen. Deep black background with gold warm rim lighting. 128x128, no background detail."

---

### Monster 8 — The Ironhide
**MonsterSO:** `Ironhide_Standard.asset`
**Sprite filename:** `Monster_Ironhide_Token.png`

**Generation prompt:**
> "Top-down view dark fantasy hand-painted sprite token. A heavily armoured quadruped, slate-grey plated hide like riveted iron, thick neck with a heavy brow ridge, four powerful legs. The armour plating has visible dents and old battle damage. Deep black background with cool steel-blue rim light. 128x128, no background detail."

---

## Overlord Monsters (4 total)

### Overlord 1 — The Siltborn
**MonsterSO:** `Siltborn_Overlord.asset`
**Sprite filename:** `Monster_Siltborn_Token.png`

**Generation prompt:**
> "Top-down view dark fantasy hand-painted sprite token. A colossal bipedal colossus formed from layered compressed silt and ancient stone, roughly humanoid, immense and slow. Three glowing node-clusters visible on its torso and shoulders like embedded amber lanterns. Cracked earth-grey and deep brown surface. Deep black background with pale amber glow from its node-lights. 128x128, no background detail."

---

### Overlord 2 — The Penitent
**MonsterSO:** `Penitent_Overlord.asset`
**Sprite filename:** `Monster_Penitent_Token.png`

**Generation prompt:**
> "Top-down view dark fantasy hand-painted sprite token. A skeletal humanoid figure wrapped in rusted chains and barbed iron bands, kneeling slightly, head bowed. No visible flesh on its torso — just exposed dark bones and binding metal. A faint self-inflicted wound across its ribcage glows dull red. Deep black background with dark crimson rim lighting. 128x128, no background detail."

---

### Overlord 3 — The Suture
**MonsterSO:** `Monster_Suture.asset`
**Sprite filename:** `Monster_Suture_Token.png`

**Generation prompt:**
> "Top-down view dark fantasy hand-painted sprite token. A horrific mass of stitched-together limbs and torsos, barely recognisable as individual people, pulled into a lumbering heap. Visible crude sutures across every seam, dark ichor leaking at the joins. Multiple mismatched arms extend outward. Deep black background with sickly pale grey-blue rim light. 128x128, no background detail."

---

### Overlord 4 — The Pale Stag Ascendant
**MonsterSO:** `PaleStag_Overlord.asset`
**Sprite filename:** `Monster_PaleStag_Phase1_Token.png`

**Generation prompt (Phase 1 — Physical Form):**
> "Top-down view dark fantasy hand-painted sprite token. An enormous white stag viewed from above, antlers impossibly wide and branching, pale white-ivory body with dark vein-like markings across the hide, eyes like burning white coals. Regal and terrifying. Deep black background with cold white-silver rim light. 128x128, no background detail."

Also generate a Phase 2 Ascendant form:
**Sprite filename:** `Monster_PaleStag_Phase2_Token.png`

**Generation prompt (Phase 2 — Ascendant Form):**
> "Dark fantasy hand-painted image. An abstract radiant form — the outline of a great stag dissolving upward into white light and shattered bone fragments, no longer fully physical. White and pale gold energy, dark void background, the antlers remain visible as the only solid element. Dramatic and otherworldly. 128x128."

---

## Assigning Sprites to MonsterSO Assets

After all sprites are generated and imported, run the following Editor script to assign them all at once rather than clicking through the Inspector manually.

**Path:** `Assets/_Game/Editor/AssignMonsterSprites.cs`

```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using MnM.Core.Data;

namespace MnM.Editor
{
    public static class AssignMonsterSprites
    {
        [MenuItem("MnM/Assign Monster Sprites")]
        public static void AssignAll()
        {
            Assign("Monsters/Monster_TheGaunt",        "Art/Monsters/Generated/Monster_TheGaunt_Token");
            Assign("Monsters/Thornback_Standard",      "Art/Monsters/Generated/Monster_Thornback_Token");
            Assign("Monsters/IvoryStampede_Standard",  "Art/Monsters/Generated/Monster_IvoryStampede_Alpha_Token");
            Assign("Monsters/BogCaller_Standard",      "Art/Monsters/Generated/Monster_BogCaller_Token");
            Assign("Monsters/Shriek_Standard",         "Art/Monsters/Generated/Monster_Shriek_Token");
            Assign("Monsters/Rotmother_Nightmare",     "Art/Monsters/Generated/Monster_Rotmother_Token");
            Assign("Monsters/GildedSerpent_Standard",  "Art/Monsters/Generated/Monster_GildedSerpent_Token");
            Assign("Monsters/Ironhide_Standard",       "Art/Monsters/Generated/Monster_Ironhide_Token");
            Assign("Monsters/Siltborn_Overlord",       "Art/Monsters/Generated/Monster_Siltborn_Token");
            Assign("Monsters/Penitent_Overlord",       "Art/Monsters/Generated/Monster_Penitent_Token");
            Assign("Monsters/Monster_Suture",          "Art/Monsters/Generated/Monster_Suture_Token");
            Assign("Monsters/PaleStag_Overlord",       "Art/Monsters/Generated/Monster_PaleStag_Phase1_Token");
            AssetDatabase.SaveAssets();
            Debug.Log("[AssignMonsterSprites] Done.");
        }

        private static void Assign(string soPath, string spritePath)
        {
            var so = AssetDatabase.LoadAssetAtPath<MonsterSO>(
                $"Assets/_Game/Data/{soPath}.asset");
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                $"Assets/_Game/{spritePath}.png");

            if (so == null)     { Debug.LogWarning($"MonsterSO not found: {soPath}");   return; }
            if (sprite == null) { Debug.LogWarning($"Sprite not found: {spritePath}"); return; }

            so.tokenSprite = sprite;
            EditorUtility.SetDirty(so);
        }
    }
}
#endif
```

Run via **MnM → Assign Monster Sprites** after all sprites are imported.

---

## Pale Stag Phase 2 — Ascendant Sprite Swap

The Pale Stag switches to its Ascendant form visually at Phase 2 transition. Wire this in `CombatManager`:

```csharp
// In the Pale Stag phase 2 transition block (from Stage 9-O):
// After _hud?.ShowPhaseBanner("ASCENDANT FORM"):

var paleStagSO = _activeMonster as MonsterSO;
if (paleStagSO != null)
{
    var phase2Sprite = Resources.Load<Sprite>(
        "Art/Monsters/Generated/Monster_PaleStag_Phase2_Token");
    _monsterToken.GetComponent<SpriteRenderer>().sprite = phase2Sprite;
    Debug.Log("[Combat] Pale Stag: Ascendant sprite applied.");
}
```

---

## Verification Checklist

- [ ] All 12 MonsterSO assets have `tokenSprite` assigned (not null)
- [ ] Open CombatScene in Editor → place The Gaunt → token shows generated art (not grey square)
- [ ] All 8 standard monster tokens visible and distinct from each other
- [ ] All 4 overlord tokens visible and appropriately larger/more imposing than standard tokens
- [ ] Pale Stag token swaps to Phase 2 sprite when Phase 2 triggers
- [ ] No missing sprite warnings in Unity Console
- [ ] Sprites are set to Sprite (2D and UI), Point filter, No compression

---

## Next Session

**File:** `_Docs/Stage_10/STAGE_10_D.md`
**Covers:** Hunter sprite generation — generating all 8 hunter build sprites (Aldric already exists; generate the 7 remaining builds), all 4 directional variants per build, importing into Unity, and assigning to animator controllers
