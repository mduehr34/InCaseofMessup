<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 10-F | Scene Background Art — Settlement, Hunt Travel, End Screens
Status: Stage 10-E complete. All gear overlay sprites generated.
Task: Generate background art for every scene that currently has
a placeholder or empty background: the Settlement hub, Hunt Travel
wilderness, Combat grid surround, Game Over screen, Victory
Epilogue screen, and Main Menu. Wire each background into the
scene via a SpriteRenderer or UIToolkit background image.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_10/STAGE_10_F.md
- Assets/_Game/Scripts/Core.UI/SettlementScreenController.cs
- Assets/_Game/Scripts/Core.UI/MainMenuController.cs
- Assets/_Game/Scripts/Core.UI/GameOverController.cs
- Assets/_Game/Scripts/Core.UI/VictoryEpilogueController.cs

Then confirm:
- Settlement scene uses UIToolkit; background is a USS background-image
  on the root VisualElement or a separate background VisualElement
- HuntTravel scene uses a SpriteRenderer in World Space (confirmed in Stage 8-O)
- CombatScene grid surround is a SpriteRenderer behind the grid
- Main menu background already exists (Stage 8-B) — regenerate if low quality
- Output folder: Assets/_Game/Art/Backgrounds/Generated/
- All backgrounds are 1920×1080 (16:9), PNG

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 10-F: Scene Background Art — Settlement, Hunt Travel, End Screens

**Resuming from:** Stage 10-E complete — all 48 gear overlay sprites generated
**Done when:** All 7 scenes have generated background art wired in; no scene shows a blank/grey background
**Commit:** `"10F: Scene background art generated and wired — settlement, travel, combat, end screens"`
**Next session:** STAGE_10_G.md

---

## Art Style Brief (All Backgrounds)

Every background uses this base style:
> "Dark fantasy digital painting, high resolution scene illustration, no UI elements, no text. Deep atmospheric shadows, warm amber and cool blue contrast lighting. Painterly brushwork with sharp focal details fading to soft edges. 1920×1080 landscape orientation."

---

## Background 1 — Main Menu

**File:** `BG_MainMenu.png`
**Path:** `Assets/_Game/Art/Backgrounds/Generated/BG_MainMenu.png`

**Generation prompt:**
> "Dark fantasy digital painting 1920×1080. A settlement viewed at dusk from outside — rough timber and stone buildings around a central bonfire, silhouetted against a dark amber twilight sky. A wide empty clearing in the foreground. The distant wilderness treeline is dark and threatening. Painterly, atmospheric. No characters, no text, no UI."

**Wiring:** In `MainMenuController`, the background is already a `VisualElement` with a background-image style. Update the USS or inline style:

```csharp
// In MainMenuController.Start() or OnEnable():
var root  = _uiDocument.rootVisualElement;
var bg    = root.Q("main-menu-background");
if (bg != null)
{
    var tex = Resources.Load<Texture2D>("Art/Backgrounds/Generated/BG_MainMenu");
    bg.style.backgroundImage = new StyleBackground(tex);
}
```

If `main-menu-background` element doesn't exist, add it as the first child of the root VisualElement in the UXML, full-size with `position: absolute`.

---

## Background 2 — Settlement Hub (Early Campaign, Years 1–10)

**File:** `BG_Settlement_Early.png`
**Path:** `Assets/_Game/Art/Backgrounds/Generated/BG_Settlement_Early.png`

**Generation prompt:**
> "Dark fantasy digital painting 1920×1080. Interior of a rough settlement — a central lantern-lit hall with wooden support beams, rough-hewn stone floor, maps and gear pinned to the walls, a long wooden table in the center. Warm amber lantern light, deep shadows at the edges. Early settlement — sparse, functional, survival-focused. No characters, no text, no UI."

---

## Background 3 — Settlement Hub (Late Campaign, Years 11–30)

**File:** `BG_Settlement_Late.png`
**Path:** `Assets/_Game/Art/Backgrounds/Generated/BG_Settlement_Late.png`

**Generation prompt:**
> "Dark fantasy digital painting 1920×1080. Interior of a more established settlement hall — same bones as before but with accumulated trophies: monster skulls mounted on walls, crafted gear displayed on weapon racks, candles and lanterns added over time. Richer amber lighting, still shadowy but lived-in. No characters, no text, no UI."

**Switching backgrounds by year:** In `SettlementScreenController`, on scene load:

```csharp
private void SetSettlementBackground()
{
    int year = GameStateManager.Instance.CampaignState.currentYear;
    string bgName = year <= 10 ? "BG_Settlement_Early" : "BG_Settlement_Late";
    var tex = Resources.Load<Texture2D>($"Art/Backgrounds/Generated/{bgName}");
    var root = _uiDocument.rootVisualElement;
    var bg   = root.Q("settlement-background");
    if (bg != null && tex != null)
        bg.style.backgroundImage = new StyleBackground(tex);
}
```

Ensure `Resources` folder contains a symlink or copy of the Generated folder, or use `AssetDatabase.LoadAssetAtPath` in a non-Resources approach:

```csharp
// Alternative using direct asset path:
var tex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(
    $"Assets/_Game/Art/Backgrounds/Generated/{bgName}.png");
// For runtime (builds), use Addressables or Resources folder
```

**For runtime builds:** Move the backgrounds folder to `Assets/_Game/Resources/Art/Backgrounds/Generated/` so `Resources.Load` works in builds. Update all background loading calls to use this path.

---

## Background 4 — Hunt Travel Wilderness

**File:** `BG_HuntTravel.png`
**Path:** `Assets/_Game/Art/Backgrounds/Generated/BG_HuntTravel.png`

**Generation prompt:**
> "Dark fantasy digital painting 1920×1080. A dark wilderness path between twisted dead trees, mist at ground level, a distant amber glow on the horizon suggesting something dangerous ahead. No characters. Threatening, atmospheric. Cool blue-grey tones, warm amber light source in the far distance. No text, no UI."

**Wiring:** In `HuntTravel` scene, a `SpriteRenderer` named `WildernessBackground` already exists. Convert to Texture and assign:

```csharp
// In TravelController.Start():
var bgObj = GameObject.Find("WildernessBackground");
if (bgObj != null)
{
    var sr = bgObj.GetComponent<SpriteRenderer>();
    var tex = Resources.Load<Texture2D>("Art/Backgrounds/Generated/BG_HuntTravel");
    if (tex != null)
    {
        sr.sprite = Sprite.Create(tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f),
            100f);
    }
}
```

---

## Background 5 — Combat Scene Surround

The combat grid is a 7×5 grid of cells. The area around the grid should feel like the specific monster's territory. Since monster-specific environments are scope-heavy, generate one universal "dark hunting ground" background used for all standard monsters, and a separate atmospheric one for overlord fights.

**File:** `BG_Combat_Standard.png`
**Path:** `Assets/_Game/Art/Backgrounds/Generated/BG_Combat_Standard.png`

**Generation prompt:**
> "Dark fantasy digital painting 1920×1080. A dark open hunting ground, cracked earth floor with deep shadows, mist at the edges, ancient stone formations at the periphery. The center is cleared — a battle arena feel. Oppressive, still. No characters, no text, no UI. Warm amber torch-light from unseen sources at the corners."

**File:** `BG_Combat_Overlord.png`
**Path:** `Assets/_Game/Art/Backgrounds/Generated/BG_Combat_Overlord.png`

**Generation prompt:**
> "Dark fantasy digital painting 1920×1080. An ancient ritual ground — vast stone floor with old carved markings faintly visible, deep red-amber sky visible in the far background, standing stones at the edges, a sense of terrible significance to the place. Much larger scale than a standard hunt. No characters, no text, no UI."

**Wiring in CombatScene:** Add a `CombatBackground` SpriteRenderer GameObject behind the grid. In `CombatScreenController.InitialiseCombat(MonsterSO monster)`:

```csharp
var bgName = monster.isOverlord ? "BG_Combat_Overlord" : "BG_Combat_Standard";
var tex    = Resources.Load<Texture2D>($"Art/Backgrounds/Generated/{bgName}");
var bgObj  = GameObject.Find("CombatBackground");
if (bgObj != null && tex != null)
{
    var sr = bgObj.GetComponent<SpriteRenderer>();
    sr.sprite = Sprite.Create(tex, new Rect(0,0,tex.width,tex.height),
                               new Vector2(0.5f,0.5f), 100f);
}
```

---

## Background 6 — Game Over Screen

**File:** `BG_GameOver.png`
**Path:** `Assets/_Game/Art/Backgrounds/Generated/BG_GameOver.png`

**Generation prompt:**
> "Dark fantasy digital painting 1920×1080. The aftermath of a failed hunt — smouldering ruins of a settlement in darkness, a few scattered embers on the ground, deep black sky with a faint sickly amber on the horizon. Silent and final. No characters, no text, no UI. Very dark with only ember glow as light source."

**Wiring:** In `GameOverController`, background VisualElement:

```csharp
var tex = Resources.Load<Texture2D>("Art/Backgrounds/Generated/BG_GameOver");
var bg  = _uiDocument.rootVisualElement.Q("game-over-background");
if (bg != null && tex != null)
    bg.style.backgroundImage = new StyleBackground(tex);
```

---

## Background 7 — Victory Epilogue Screen

**File:** `BG_Victory.png`
**Path:** `Assets/_Game/Art/Backgrounds/Generated/BG_Victory.png`

**Generation prompt:**
> "Dark fantasy digital painting 1920×1080. A settlement at dawn after a long campaign — the bonfire has burned down to warm coals, early grey-blue light on the horizon, the settlement structures show years of use and hard work. Quiet and earned. Amber warmth from the coals, cool blue dawn sky. No characters, no text, no UI."

**Wiring:** Same pattern as Game Over:

```csharp
var tex = Resources.Load<Texture2D>("Art/Backgrounds/Generated/BG_Victory");
var bg  = _uiDocument.rootVisualElement.Q("victory-background");
if (bg != null && tex != null)
    bg.style.backgroundImage = new StyleBackground(tex);
```

---

## Resources Folder Migration

All backgrounds used at runtime must be in the `Resources` folder. Add a post-import step in an `AssetPostprocessor` or simply move the generated folder:

```
Assets/_Game/Resources/Art/Backgrounds/Generated/    ← runtime-loadable
Assets/_Game/Art/Backgrounds/Generated/               ← keep as source, symlink or copy
```

The cleanest approach: keep generation output in `Assets/_Game/Art/Backgrounds/Generated/` but copy to `Resources` as part of the `AssignSceneBackgrounds` editor script below.

---

## Editor Script: Assign All Backgrounds

**Path:** `Assets/_Game/Editor/AssignSceneBackgrounds.cs`

```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MnM.Editor
{
    public static class AssignSceneBackgrounds
    {
        private static readonly string _srcBase  = "Assets/_Game/Art/Backgrounds/Generated/";
        private static readonly string _destBase = "Assets/_Game/Resources/Art/Backgrounds/Generated/";

        [MenuItem("MnM/Copy Backgrounds to Resources")]
        public static void CopyToResources()
        {
            System.IO.Directory.CreateDirectory(_destBase);
            var pngs = System.IO.Directory.GetFiles(_srcBase, "BG_*.png");
            foreach (var src in pngs)
            {
                var filename = System.IO.Path.GetFileName(src);
                var dest     = _destBase + filename;
                System.IO.File.Copy(src, dest, overwrite: true);
            }
            AssetDatabase.Refresh();
            Debug.Log($"[Backgrounds] Copied {pngs.Length} backgrounds to Resources.");
        }
    }
}
#endif
```

Run via **MnM → Copy Backgrounds to Resources** after any background regeneration.

---

## Verification Checklist

- [ ] `BG_MainMenu.png` visible in Main Menu scene — no grey or black empty background
- [ ] `BG_Settlement_Early.png` visible in Settlement when `currentYear <= 10`
- [ ] `BG_Settlement_Late.png` visible in Settlement when `currentYear > 10`
- [ ] `BG_HuntTravel.png` visible in Hunt Travel scene
- [ ] `BG_Combat_Standard.png` visible behind combat grid when fighting a standard monster
- [ ] `BG_Combat_Overlord.png` visible when fighting an overlord
- [ ] `BG_GameOver.png` visible in Game Over scene
- [ ] `BG_Victory.png` visible in Victory Epilogue scene
- [ ] All backgrounds fill the screen correctly (no letterboxing at 16:9)
- [ ] No backgrounds missing from Resources folder (runtime load works in Play Mode)

---

## Next Session

**File:** `_Docs/Stage_10/STAGE_10_G.md`
**Covers:** Combat visual polish — adding walk animations for hunters moving across the grid, monster movement smoothing, broken-part visual states (damaged appearance), overlord phase 2 visual transition, and post-combat death/victory visual flourishes
