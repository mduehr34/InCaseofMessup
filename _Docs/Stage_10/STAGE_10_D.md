<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 10-D | Hunter Sprite Generation — 7 Remaining Builds + Idle Animations
Status: Stage 10-C complete. All monster sprites generated.
Task: Generate sprites for the 7 hunter builds that are not yet
illustrated (Aldric's sprites exist from Stage 7). Each build
needs a south-facing token sprite (128×128) and a portrait
sprite (64×64). After generation, wire the idle animation for
each build so hunters are not static on the combat grid.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_10/STAGE_10_D.md
- Assets/_Game/Scripts/Core.Data/HunterBuildSO.cs     ← tokenSprite, portrait fields
- Assets/_Game/Scripts/Core.Systems/HunterTokenController.cs
- Assets/_Game/Art/Hunters/Generated/                  ← Aldric sprites already here
- _Docs/Stage_09/STAGE_09_B.md                         ← directional facing system reference
- _Docs/Stage_09/STAGE_09_C.md                         ← animator controller reference

Then confirm:
- Aldric sprites already exist (south/north/east/west + idle animation)
- 7 other builds: Brigand, Warden, Scholar, Ironclad, Ranger, Cultist, Outlander
- Each build generates a south-facing token and a portrait; north/east/west derived
  by runtime flip where possible (east = flip of west)
- Idle animation is a 2-frame bounce (frame 0 and frame 1 with slight offset)
- What you will NOT do (gear overlay sprites — that is Stage 10-E)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 10-D: Hunter Sprite Generation — 7 Remaining Builds + Idle Animations

**Resuming from:** Stage 10-C complete — all 12 monster sprites generated and assigned
**Done when:** All 7 remaining hunter builds have token + portrait sprites; idle animations play on all 8 builds in combat
**Commit:** `"10D: Hunter sprites generated for all 8 builds; idle animations wired"`
**Next session:** STAGE_10_E.md

---

## Hunter Build Reference

| Build | Token filename | Portrait filename |
|-------|----------------|-------------------|
| Aldric (exists) | `Hunter_Aldric_S_Token.png` | `Hunter_Aldric_Portrait.png` |
| Brigand | `Hunter_Brigand_S_Token.png` | `Hunter_Brigand_Portrait.png` |
| Warden | `Hunter_Warden_S_Token.png` | `Hunter_Warden_Portrait.png` |
| Scholar | `Hunter_Scholar_S_Token.png` | `Hunter_Scholar_Portrait.png` |
| Ironclad | `Hunter_Ironclad_S_Token.png` | `Hunter_Ironclad_Portrait.png` |
| Ranger | `Hunter_Ranger_S_Token.png` | `Hunter_Ranger_Portrait.png` |
| Cultist | `Hunter_Cultist_S_Token.png` | `Hunter_Cultist_Portrait.png` |
| Outlander | `Hunter_Outlander_S_Token.png` | `Hunter_Outlander_Portrait.png` |

All generated sprites go in `Assets/_Game/Art/Hunters/Generated/`.

---

## Art Style Brief

Use this as the base for every hunter generation call:

> "Top-down view, dark fantasy, hand-painted pixel art style, single human figure facing south (toward viewer), full body visible, simple dark background. Dark medieval aesthetic — worn leather, cloth wrappings, rough-hewn weapons. 128×128 sprite token, warm amber key light from above, deep shadows. No UI elements."

For portraits (64×64), use:
> "Dark fantasy portrait, head and shoulders, facing slightly right, hand-painted rough brushwork, muted tones, dark background. No UI elements. 64×64."

---

## Generation — The 7 Builds

Generate each with `mcp__coplay-mcp__generate_or_edit_images`. After each:
1. Save to `Assets/_Game/Art/Hunters/Generated/[filename].png`
2. Import as Sprite (2D and UI), Point filter, No compression
3. Assign to the HunterBuildSO's `tokenSprite` and `portraitSprite` fields

### Build 2 — Brigand

**Token prompt:**
> "Top-down view dark fantasy hand-painted sprite. A stocky human figure facing south, wearing patched leather armour with mismatched straps and pouches. Short-cropped hair, scarred face with a crooked nose. Holds two short blades crossed at belt. Worn brown and dark grey tones. 128×128, dark background, warm amber top-light."

**Portrait prompt:**
> "Dark fantasy portrait of a scarred stocky human, short cropped dark hair, crooked nose, suspicious eyes, worn leather collar, rough brushwork, muted tones, dark background. 64×64."

---

### Build 3 — Warden

**Token prompt:**
> "Top-down view dark fantasy hand-painted sprite. A tall broad human figure facing south, wearing layered plate-and-leather armour in dark steel and brown leather, a great sword on their back, heavy gauntlets. Calm and imposing posture. Dark grey-steel and deep brown tones. 128×128, dark background, cool steel rim light."

**Portrait prompt:**
> "Dark fantasy portrait of a broad tall human, square jaw, calm grey eyes, dark steel pauldrons visible at shoulders, rough brushwork, muted tones, dark background. 64×64."

---

### Build 4 — Scholar

**Token prompt:**
> "Top-down view dark fantasy hand-painted sprite. A lean human figure facing south, wearing dark robes with worn parchment scrolls tucked into a sash, a hand lantern in one hand, a short bone-handled knife at the hip. Spectacles. Pale complexion, careful posture. Dark olive-brown robes. 128×128, dark background, warm amber lantern glow."

**Portrait prompt:**
> "Dark fantasy portrait of a lean pale human wearing spectacles, dark robes, intelligent sharp eyes, olive-brown tones, rough brushwork, dark background. 64×64."

---

### Build 5 — Ironclad

**Token prompt:**
> "Top-down view dark fantasy hand-painted sprite. A massive heavily armoured human figure facing south, full-plate dark iron armour, tower shield strapped to back, war hammer at side. Slow and immovable posture. Battered dark iron, deep dents in the plate, worn heraldry scratched away. 128×128, dark background, cold steel rim light."

**Portrait prompt:**
> "Dark fantasy portrait of a massive human in dented iron full-plate, only the lower face visible below a visor, stern jaw, rough brushwork, dark background. 64×64."

---

### Build 6 — Ranger

**Token prompt:**
> "Top-down view dark fantasy hand-painted sprite. A lithe human figure facing south, wearing close-fitting dark green and brown leather, a short recurve bow on their back, quiver at hip, cloak with hood down. Alert posture, weight on toes. Earthy dark green and grey-brown tones. 128×128, dark background, cool forest-filtered rim light."

**Portrait prompt:**
> "Dark fantasy portrait of a lithe human with alert eyes, hood down, dark green leather collar, earthy tones, rough brushwork, dark background. 64×64."

---

### Build 7 — Cultist

**Token prompt:**
> "Top-down view dark fantasy hand-painted sprite. A thin human figure facing south, wearing rough-sewn black robes with bone and teeth woven into the hem, face half-hidden in deep cowl shadow, hands wrapped in dark cord, holding a gnarled bone staff. Unsettling posture. Near-black and dark grey tones with faint red-amber glow from beneath robes. 128×128, dark background."

**Portrait prompt:**
> "Dark fantasy portrait of a gaunt human with half-face hidden in a deep cowl, only hollow cheeks and dark eyes visible, bone beads at collar, rough brushwork, dark background. 64×64."

---

### Build 8 — Outlander

**Token prompt:**
> "Top-down view dark fantasy hand-painted sprite. A wiry human figure facing south, wearing furs and tanned hides, facial markings in ash or charcoal, carrying a long spear, a small bundle of totems at the belt. Hardened and self-reliant posture. Tan, ochre, and dark grey tones. 128×128, dark background, warm firelight rim."

**Portrait prompt:**
> "Dark fantasy portrait of a wiry human with ash facial markings, animal-hide clothing, intense eyes, warm ochre tones, rough brushwork, dark background. 64×64."

---

## Directional Sprite Strategy

Generating full 4-way sprites for all 8 builds is an extensive art session. Use this efficient strategy:

- **South** — fully generated (done above)
- **North** — generate separately: same prompt but "back of figure facing away from viewer, only back and shoulders visible"
- **East** — generate once per build; **West** is derived at runtime by flipping the East sprite (`SpriteRenderer.flipX = true`)
- This means you need: South, North, and East — 3 generations per build = 21 more generations for builds 2–8

### North Prompt Template
> "Top-down view dark fantasy hand-painted sprite. [Build description] facing north, away from viewer, only back visible. Same clothing as south-facing version. 128×128, dark background, warm amber top-light."

### East Prompt Template
> "Top-down view dark fantasy hand-painted sprite. [Build description] facing east (to the right), side profile visible. Same clothing as south-facing version. 128×128, dark background, warm amber side-light."

Generate North and East variants for each build using the same build descriptions from the south section above. Save as:
- `Hunter_[Build]_N_Token.png`
- `Hunter_[Build]_E_Token.png`

---

## Assigning Sprites to HunterBuildSO Assets

Confirm `HunterBuildSO` has these sprite fields:

```csharp
[Header("Sprites")]
public Sprite tokenSpriteSouth;
public Sprite tokenSpriteNorth;
public Sprite tokenSpriteEast;   // West derived by flipX at runtime
public Sprite portraitSprite;
```

If any are missing, add them. Then run this editor script to assign all sprites at once:

**Path:** `Assets/_Game/Editor/AssignHunterSprites.cs`

```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using MnM.Core.Data;

namespace MnM.Editor
{
    public static class AssignHunterSprites
    {
        private static readonly (string build, string assetPath)[] _builds =
        {
            ("Brigand",   "Brigand"),
            ("Warden",    "Warden"),
            ("Scholar",   "Scholar"),
            ("Ironclad",  "Ironclad"),
            ("Ranger",    "Ranger"),
            ("Cultist",   "Cultist"),
            ("Outlander", "Outlander"),
        };

        [MenuItem("MnM/Assign Hunter Sprites")]
        public static void AssignAll()
        {
            foreach (var (build, name) in _builds)
            {
                var so = AssetDatabase.LoadAssetAtPath<HunterBuildSO>(
                    $"Assets/_Game/Data/Hunters/Build_{name}.asset");
                if (so == null) { Debug.LogWarning($"HunterBuildSO not found: {name}"); continue; }

                so.tokenSpriteSouth = LoadSprite($"Hunter_{name}_S_Token");
                so.tokenSpriteNorth = LoadSprite($"Hunter_{name}_N_Token");
                so.tokenSpriteEast  = LoadSprite($"Hunter_{name}_E_Token");
                so.portraitSprite   = LoadSprite($"Hunter_{name}_Portrait");
                EditorUtility.SetDirty(so);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[AssignHunterSprites] Done.");
        }

        private static Sprite LoadSprite(string filename)
            => AssetDatabase.LoadAssetAtPath<Sprite>(
               $"Assets/_Game/Art/Hunters/Generated/{filename}.png");
    }
}
#endif
```

Run via **MnM → Assign Hunter Sprites**.

---

## Idle Animation — 2-Frame Bounce

Each hunter's idle animation is a 2-frame subtle vertical bounce. This is simple and effective for a top-down token game.

### Animation Clip Setup (repeat for each build)

For each build's animator controller (`Assets/_Game/Art/Hunters/Animators/[Build]_AnimController.controller`):

1. The Idle state already exists from Stage 9-C. If it is empty, add a clip.
2. Create `Hunter_[Build]_Idle.anim` in `Assets/_Game/Art/Hunters/Animations/`
3. The clip has 2 keyframes on the Y position of the root transform:
   - Frame 0: `transform.localPosition.y = 0.0`
   - Frame 1: `transform.localPosition.y = 0.03`  (3cm up — barely perceptible)
4. Set Loop Time = true, Wrap Mode = PingPong
5. Duration = 0.8 seconds total (slow, calm breathing feel)

Use `mcp__coplay-mcp__create_animation_clip` or `mcp__coplay-mcp__set_animation_curves` to create these programmatically:

```
For each build's animator controller:
  Create animation clip: Hunter_[Build]_Idle
  Curve path: "" (root)
  Curve property: m_LocalPosition.y
  Keyframes: [(time:0, value:0), (time:0.4, value:0.03), (time:0.8, value:0)]
  Loop: true
  Assign to Idle state in animator controller
```

### Aldric Idle (confirm existing)

Aldric's idle animation should already exist from Stage 9-C. Verify it plays in the Editor (select Aldric's animator controller → Preview Idle state).

---

## HunterTokenController — Facing Logic Update

Confirm `HunterTokenController` applies directional sprites from `HunterBuildSO`:

```csharp
// In HunterTokenController.SetFacing(Vector2Int direction)
private void SetFacing(Vector2Int dir)
{
    var sr = GetComponent<SpriteRenderer>();
    if (dir == Vector2Int.down || dir == Vector2Int.zero)
    {
        sr.sprite = _buildSO.tokenSpriteSouth;
        sr.flipX  = false;
    }
    else if (dir == Vector2Int.up)
    {
        sr.sprite = _buildSO.tokenSpriteNorth;
        sr.flipX  = false;
    }
    else if (dir == Vector2Int.right)
    {
        sr.sprite = _buildSO.tokenSpriteEast;
        sr.flipX  = false;
    }
    else if (dir == Vector2Int.left)
    {
        sr.sprite = _buildSO.tokenSpriteEast;
        sr.flipX  = true;  // Mirror east sprite for west
    }
}
```

Call `SetFacing()` whenever a hunter moves. Default to south-facing at hunt start.

---

## Verification Checklist

- [ ] All 7 builds have south-facing token sprites generated and imported
- [ ] All 7 builds have portrait sprites generated and imported
- [ ] North and East sprites generated for all 7 builds
- [ ] All 8 builds' HunterBuildSO assets have all four sprite fields assigned
- [ ] In CombatScene: place a hunter of each build → correct sprite shows
- [ ] Move a hunter east → east-facing sprite; move west → mirrored east sprite
- [ ] Idle animation plays on all 8 builds (slow bob, looping)
- [ ] Hunter detail panel in Settlement shows portrait for each build
- [ ] No missing sprite warnings in Unity Console

---

## Next Session

**File:** `_Docs/Stage_10/STAGE_10_E.md`
**Covers:** Gear overlay sprite generation — generating overlay sprites for all 48 gear items across 8 craft sets, importing them into Unity, assigning to GearSO assets, and confirming they render on hunter tokens via the GearOverlayController built in Stage 9-D
