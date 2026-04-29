<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 10-E | Gear Overlay Sprite Generation — All 48 Items
Status: Stage 10-D complete. All 8 hunter build sprites done.
Task: Generate overlay sprites for every piece of gear across all
8 craft sets (48 items total + 2 consumables). The GearOverlayController
built in Stage 9-D supports 5 child SpriteRenderers per hunter
token (Arms, Chest, Head, Weapon, OffHand). Each gear item needs
a small overlay sprite that composites over the hunter token.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_10/STAGE_10_E.md
- _Docs/Stage_09/STAGE_09_D.md                     ← overlay system reference
- Assets/_Game/Scripts/Core.Systems/GearOverlayController.cs
- Assets/_Game/Data/Gear/                          ← all GearSO assets

Then confirm:
- GearOverlayController has 5 SpriteRenderer children: Arms, Chest, Head, Weapon, OffHand
- GearSO has an overlaySprite field (Sprite type)
- Overlay sprites are 64×64, transparent background, slot-specific silhouette
- Output folder: Assets/_Game/Art/Gear/Overlays/Generated/
- What you will NOT do (animations, settlement backgrounds — later sessions)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 10-E: Gear Overlay Sprite Generation — All 48 Items

**Resuming from:** Stage 10-D complete — all hunter build sprites generated and idle animations wired
**Done when:** All 48 gear items + 2 consumables have overlay sprites assigned; gear renders visibly on hunter tokens in combat
**Commit:** `"10E: Gear overlay sprites generated for all 48 gear items"`
**Next session:** STAGE_10_F.md

---

## Overlay Sprite Spec

Each gear overlay is a **64×64 transparent PNG** that layers over the hunter token. The sprite should:
- Represent only the **slot region** (e.g. a helmet over the head, gauntlets over the arms)
- Use **transparent background** (PNG with alpha channel)
- Be visually distinct per set (different silhouette and material read)
- Be subtle — the hunter token beneath should still read clearly

Art style brief for all overlays:
> "64×64 flat top-down gear overlay sprite, transparent background, dark fantasy aesthetic. Simple silhouette visible from above. Muted tones appropriate to the material. No shadow casting — this layers over a character token."

---

## Slot Lookup

| Slot name | What the sprite covers |
|-----------|------------------------|
| Head      | Top portion of token; helmet or hood shape |
| Chest     | Center/torso area; armour plate or coat |
| Arms      | Both arm/shoulder areas visible from above |
| Weapon    | Right hand area; weapon silhouette extending from grip |
| OffHand   | Left hand area; shield, off-hand item, or wrapped hand |
| Amulet    | Very small overlay centered on torso — a glint or small shape |

---

## Generation Batch — By Craft Set

Generate overlays in set batches. For each set, the visual language should be cohesive.

---

### Set 1 — Carapace Forge (6 items)

Material: bone-white chitinous plates, insect-like.

| gearId | Item | Slot | Overlay filename | Prompt addition |
|--------|------|------|-----------------|-----------------|
| CAR-01 | Bone Cleaver | Weapon | `Overlay_CAR-01_BoneCleaver.png` | "A crude cleaver blade of flat white bone, side view of blade from above" |
| CAR-02 | Carapace Helm | Head | `Overlay_CAR-02_CarapaceHelm.png` | "A domed insect carapace helmet, bone-white, viewed from directly above" |
| CAR-03 | Chitin Vest | Chest | `Overlay_CAR-03_ChitinVest.png` | "Overlapping flat chitin plates forming a chest cover, bone-white, viewed from above" |
| CAR-04 | Claw Bracers | Arms | `Overlay_CAR-04_ClawBracers.png` | "Two forearm bracers of segmented bone-white chitin, viewed from above on outstretched arms" |
| CAR-05 | Shell Shield | OffHand | `Overlay_CAR-05_ShellShield.png` | "A round shield made of flat insect carapace, bone-white, viewed from above at left hand" |
| CAR-06 | Marrow Charm | Amulet | `Overlay_CAR-06_MarrowCharm.png` | "A tiny bone pendant, two crossed bone fragments, viewed from above at torso center" |

---

### Set 2 — Membrane Loft (6 items)

Material: thin stretched translucent membrane, dark grey-blue.

| gearId | Item | Slot | Overlay filename | Prompt addition |
|--------|------|------|-----------------|-----------------|
| MEM-01 | Membrane Blades | Weapon | `Overlay_MEM-01_MembraneBlades.png` | "Twin thin membrane-bladed daggers, dark grey-blue translucent blades, from above" |
| MEM-02 | Membrane Hood | Head | `Overlay_MEM-02_MembraneHood.png` | "A thin stretched membrane hood, dark grey-blue, slightly translucent, from above" |
| MEM-03 | Membrane Vest | Chest | `Overlay_MEM-03_MembraneVest.png` | "A close-fitting vest of stretched grey-blue membrane panels, from above" |
| MEM-04 | Vein Wraps | Arms | `Overlay_MEM-04_VeinWraps.png` | "Forearms wrapped in thin grey-blue membrane strips showing vein-like texture, from above" |
| MEM-05 | Membrane Catcher | OffHand | `Overlay_MEM-05_MembraneCatcher.png` | "A small wing-membrane parabolic catcher attached to left hand, dark grey-blue, from above" |
| MEM-06 | Breath Stone | Amulet | `Overlay_MEM-06_BreathStone.png` | "A tiny oval stone that glows faint blue, resting on torso center, from above" |

---

### Set 3 — Mire Apothecary (6 items)

Material: dark wet clay, dark teal glass vials, organic shapes. (Unlocked via Siltborn kill.)

| gearId | Item | Slot | Overlay filename | Prompt addition |
|--------|------|------|-----------------|-----------------|
| MIR-01 | Mire Staff | Weapon | `Overlay_MIR-01_MireStaff.png` | "A gnarled staff of dark clay-covered wood with a teal glowing tip, from above" |
| MIR-02 | Silt Helm | Head | `Overlay_MIR-02_SiltHelm.png` | "A bulbous helmet made of hardened dark grey silt with breathing holes, from above" |
| MIR-03 | Clay Coat | Chest | `Overlay_MIR-03_ClayCoat.png` | "Thick layers of hardened dark clay armour plating on the torso, from above" |
| MIR-04 | Mire Gauntlets | Arms | `Overlay_MIR-04_MireGauntlets.png` | "Heavy dark grey clay gauntlets covering both forearms to the elbow, from above" |
| MIR-05 | Vial Brace | OffHand | `Overlay_MIR-05_VialBrace.png` | "A brace holding three small teal glass vials on the left forearm, from above" |
| MIR-06 | Silt Compass | Amulet | `Overlay_MIR-06_SiltCompass.png` | "A disc of compressed silt with a tiny teal needle, worn at torso center, from above" |

---

### Set 4 — Ichor Works (6 items)

Material: amber-black dripping ichor, slick dark surfaces. (Unlocked via Penitent kill.)

| gearId | Item | Slot | Overlay filename | Prompt addition |
|--------|------|------|-----------------|-----------------|
| ICH-01 | Ichor Blade | Weapon | `Overlay_ICH-01_IchorBlade.png` | "A long narrow blade dripping with dark amber ichor, from above" |
| ICH-02 | Ichor Mask | Head | `Overlay_ICH-02_IchorMask.png` | "A sleek face mask coated in hardened amber-black ichor, from above" |
| ICH-03 | Slick Coat | Chest | `Overlay_ICH-03_SlickCoat.png` | "A dark coat with amber-black ichor-coated surface plates on the chest, from above" |
| ICH-04 | Drip Bracers | Arms | `Overlay_ICH-04_DripBracers.png` | "Both forearms coated in dripping amber-black ichor hardened into arm guards, from above" |
| ICH-05 | Ichor Flask | OffHand | `Overlay_ICH-05_IchorFlask.png` | "A small dark flask of amber ichor held in the left hand, from above" |
| ICH-06 | Ichor Locket | Amulet | `Overlay_ICH-06_IchorLocket.png` | "A tiny locket filled with amber ichor visible through a cracked glass face, from above" |

---

### Set 5 — Auric Scales (6 items)

Material: golden-bronze overlapping scales, warm tones. (Unlocked via Gilded Serpent kill.)

| gearId | Item | Slot | Overlay filename | Prompt addition |
|--------|------|------|-----------------|-----------------|
| AUR-01 | Scale Sword | Weapon | `Overlay_AUR-01_ScaleSword.png` | "A sword with golden-bronze scale-patterned blade, from above" |
| AUR-02 | Scale Crown | Head | `Overlay_AUR-02_ScaleCrown.png` | "A head covering of overlapping golden-bronze serpent scales, from above" |
| AUR-03 | Scale Coat | Chest | `Overlay_AUR-03_ScaleCoat.png` | "Overlapping golden-bronze scales covering the torso from above, like a coat of fish-mail" |
| AUR-04 | Scale Vambraces | Arms | `Overlay_AUR-04_ScaleVambraces.png` | "Both forearms covered in overlapping golden-bronze scales, from above" |
| AUR-05 | Mirror Ward | OffHand | `Overlay_AUR-05_MirrorWard.png` | "A small oval mirror-polished golden scale shield at the left hand, from above" |
| AUR-06 | Serpent Eye | Amulet | `Overlay_AUR-06_SerpentEye.png` | "A tiny golden serpent eye gemstone mounted on a pin at torso center, from above" |

---

### Set 6 — Rot Garden (6 items)

Material: dark organic growth, moss, rotten wood, spore clusters. (Unlocked via Rotmother kill.)

| gearId | Item | Slot | Overlay filename | Prompt addition |
|--------|------|------|-----------------|-----------------|
| ROT-01 | Blight Spear | Weapon | `Overlay_ROT-01_BlightSpear.png` | "A spear shaft wrapped in dark rot-mushroom growth, spore clusters at the tip, from above" |
| ROT-02 | Spore Cap | Head | `Overlay_ROT-02_SporeCap.png` | "A helmet made of hardened dark fungal growth with small spore vents, from above" |
| ROT-03 | Mycelium Coat | Chest | `Overlay_ROT-03_MyceliumCoat.png` | "A dark coat with mycelium growth covering the chest in woven dark grey-green strands, from above" |
| ROT-04 | Root Wraps | Arms | `Overlay_ROT-04_RootWraps.png` | "Both forearms wrapped in twisted dark root-like organic growth, from above" |
| ROT-05 | Spore Sac | OffHand | `Overlay_ROT-05_SporeSac.png` | "A pulsing spore sac held in the left hand, dark olive green with visible bulges, from above" |
| ROT-06 | Mycelium Cord | Amulet | `Overlay_ROT-06_MyceliumCord.png` | "A tiny knotted cord of dark mycelium thread at torso center, from above" |

---

### Set 7 — Ivory Hall (6 items)

Material: yellowed ivory, carved tusk fragments. (Unlocked via Ivory Stampede kill.)

| gearId | Item | Slot | Overlay filename | Prompt addition |
|--------|------|------|-----------------|-----------------|
| IVY-01 | Tusk Blade | Weapon | `Overlay_IVY-01_TuskBlade.png` | "A heavy blade carved from a single ivory tusk, yellowed, from above" |
| IVY-02 | Ivory Skullcap | Head | `Overlay_IVY-02_IvorySkullcap.png` | "A polished ivory skull-cap helmet, yellowed with age, from above" |
| IVY-03 | Ivory Vest | Chest | `Overlay_IVY-03_IvoryVest.png` | "Carved ivory plates laced together forming a chest vest, yellowed, from above" |
| IVY-04 | Tusk Bracers | Arms | `Overlay_IVY-04_TuskBracers.png` | "Both forearms covered in curved ivory tusk-fragment bracers, yellowed, from above" |
| IVY-05 | Ivory Buckler | OffHand | `Overlay_IVY-05_IvoryBuckler.png` | "A small round buckler of solid carved ivory, yellowed, at the left hand, from above" |
| IVY-06 | Stampede Relic | Amulet | `Overlay_IVY-06_StampedeRelic.png` | "A tiny ivory stamp carved with a charging bull silhouette at torso center, from above" |

---

### Set 8 — Sinew Works (6 items)

Material: stitched sinew and dark suture thread, flesh-toned patches. (Unlocked via Suture kill.)

| gearId | Item | Slot | Overlay filename | Prompt addition |
|--------|------|------|-----------------|-----------------|
| SIN-01 | Sinew Hood | Head | `Overlay_SIN-01_SinewHood.png` | "A tightly stitched hood of dark sinew and hide, suture marks visible, from above" |
| SIN-02 | Stitched Coat | Chest | `Overlay_SIN-02_StitchedCoat.png` | "A coat of layered stitched sinew panels on the torso, crude thick sutures, from above" |
| SIN-03 | Bind Wraps | Arms | `Overlay_SIN-03_BindWraps.png` | "Both forearms tightly wrapped in sinew cord and suture thread, from above" |
| SIN-04 | Bone-Stitch Blade | Weapon | `Overlay_SIN-04_BoneStitchBlade.png` | "A blade wrapped in sinew at the hilt, a bone shard fused along the edge, from above" |
| SIN-05 | Suture Ward | OffHand | `Overlay_SIN-05_SutureWard.png` | "A small targe made of stitched sinew on a bone frame at the left hand, from above" |
| SIN-06 | Marrow Cord | Amulet | `Overlay_SIN-06_MarrowCord.png` | "A knotted cord of dark sinew thread at torso center, from above" |

---

### Consumables (2 items)

| Item | Slot | Overlay filename | Prompt |
|------|------|-----------------|--------|
| Bone Splint | OffHand | `Overlay_Consumable_BoneSplint.png` | "A wrapped bone splint held in the left hand, clean white bone with linen wrapping, from above, transparent bg. 64×64." |
| Ichor Flask | OffHand | `Overlay_Consumable_IchorFlask.png` | "A small glass flask filled with amber-dark ichor held in the left hand, from above, transparent bg. 64×64." |

---

## Gaunt Set Overlays

The Gaunt set (built in Stage 7) also needs overlay sprites. Generate these using the Gaunt art style (sinew, dark leather, exposed bone):

| Item | Slot | Overlay filename | Prompt addition |
|------|------|-----------------|-----------------|
| Gaunt Skull Cap | Head | `Overlay_Gaunt_SkullCap.png` | "A cap made of a polished skull dome, dark and worn, viewed from above" |
| Gaunt Hide Vest | Chest | `Overlay_Gaunt_HideVest.png` | "Dark dried hide panels stitched together covering the torso, viewed from above" |
| Gaunt Sinew Wrap | Arms | `Overlay_Gaunt_SinewWrap.png` | "Both forearms wrapped in dried sinew cord, dark ochre-brown, from above" |
| Gaunt Bone Bracers | Arms | `Overlay_Gaunt_BoneBracers.png` | "Forearm guards made of flat bone plates, aged and cracked, from above" |
| Gaunt Hide Boots | (skip — not visible top-down) | — | Not generated; feet not visible in top-down view |
| Gaunt Eye Pendant | Amulet | `Overlay_Gaunt_EyePendant.png` | "A tiny carved eye amulet in bone, worn at torso center, from above" |

---

## Bulk Assignment Editor Script

After all overlays are generated and imported as Sprites:

**Path:** `Assets/_Game/Editor/AssignGearOverlays.cs`

```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using MnM.Core.Data;

namespace MnM.Editor
{
    public static class AssignGearOverlays
    {
        [MenuItem("MnM/Assign Gear Overlays")]
        public static void AssignAll()
        {
            var gearGuids = AssetDatabase.FindAssets("t:GearSO", new[] { "Assets/_Game/Data/Gear" });
            int count = 0;
            foreach (var guid in gearGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var gear = AssetDatabase.LoadAssetAtPath<GearSO>(path);
                if (gear == null || string.IsNullOrEmpty(gear.gearId)) continue;

                var spritePath = $"Assets/_Game/Art/Gear/Overlays/Generated/Overlay_{gear.gearId}_{SanitiseName(gear.gearName)}.png";
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                if (sprite == null)
                {
                    Debug.LogWarning($"[AssignGearOverlays] No overlay sprite found for {gear.gearId}: {spritePath}");
                    continue;
                }
                gear.overlaySprite = sprite;
                EditorUtility.SetDirty(gear);
                count++;
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[AssignGearOverlays] Assigned overlays to {count} gear items.");
        }

        private static string SanitiseName(string name)
            => name.Replace(" ", "").Replace("-", "").Replace("'", "");
    }
}
#endif
```

Run via **MnM → Assign Gear Overlays**.

---

## Texture Import Settings

All overlay sprites must be imported with:
- **Texture Type:** Sprite (2D and UI)
- **Sprite Mode:** Single
- **Filter Mode:** Point (no blur)
- **Compression:** None
- **Alpha Source:** Input Texture Alpha
- **Alpha Is Transparency:** ✓ (checked)

Write an editor script to apply this to the whole folder:

```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MnM.Editor
{
    public class GearOverlayImporter : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (!assetPath.Contains("Art/Gear/Overlays/Generated")) return;
            var ti = assetImporter as TextureImporter;
            if (ti == null) return;
            ti.textureType            = TextureImporterType.Sprite;
            ti.spriteImportMode       = SpriteImportMode.Single;
            ti.filterMode             = FilterMode.Point;
            ti.textureCompression     = TextureImporterCompression.Uncompressed;
            ti.alphaSource            = TextureImporterAlphaSource.FromInput;
            ti.alphaIsTransparency    = true;
        }
    }
}
#endif
```

Place this in `Assets/_Game/Editor/GearOverlayImporter.cs`. It runs automatically on every import from the overlays folder.

---

## Verification Checklist

- [ ] All 48 gear items + 2 consumables have overlay sprites generated and imported
- [ ] Gaunt set (5 relevant items) have overlays assigned
- [ ] GearOverlayImporter script auto-applies correct texture settings on import
- [ ] AssignGearOverlays script runs without warnings (all sprites found)
- [ ] In CombatScene: equip Bone Cleaver (CAR-01) on a hunter → weapon overlay appears on token
- [ ] Equip Carapace Helm + Chitin Vest → both overlays stack correctly (sorting orders 1–5 respected)
- [ ] Unequip an item → overlay disappears immediately
- [ ] No z-fighting or rendering artifacts on any overlay combination
- [ ] Overlay sprites are all transparent-background (no white rectangles behind them)

---

## Next Session

**File:** `_Docs/Stage_10/STAGE_10_F.md`
**Covers:** Settlement scene visual art — generating background art for the settlement hub, hunt travel scene, and game over / victory screens; wiring backgrounds into the scene's SpriteRenderer backgrounds; generating individual building slot illustrations for the settlement hub
