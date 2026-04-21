<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 7-E | Art Batch — Import UI Elements & Settlement Structures
Status: Stage 7-D complete. All monster sprites imported
with Point filtering verified.
Task: Import UI art (stone panel texture, card frame,
button states, aggro token) and all 5 settlement structure
sprites. Then wire structure sprites into SettlementScreenController.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_07/STAGE_07_E.md
- Assets/_Game/Scripts/Core.UI/SettlementScreenController.cs
- Assets/_Game/Scripts/Core.Data/CrafterSO.cs

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 7-E: Art Batch — Import UI Elements & Settlement Structures

**Resuming from:** Stage 7-D complete — all monster sprites imported  
**Done when:** All 5 UI element sprites and all 8 settlement structure sprites imported; The Ossuary appears in settlement scene when built in a test playthrough  
**Commit:** `"7E: UI textures and settlement structure sprites — wired into settlement scene"`  
**Next session:** STAGE_07_F.md  

---

## UI Elements to Import

Use the Art Importer (Window → MnM → Art Importer), subfolder: **UI**.

**Save path:** `Assets/_Game/Art/Generated/UI/`

| File | Canvas Size | Key Visual Check |
|---|---|---|
| `ui_stone_panel_bg.png` | 64×64 (tileable) | Stone tablet surface, fine chisel marks, relief texture, no seams |
| `ui_card_frame.png` | 160×220 | Stone-carved border, transparent interior — only the border is opaque |
| `ui_button_normal.png` | 200×48 | Stone button, carved border, slightly raised look |
| `ui_button_hover.png` | 200×48 | Same as normal with Marrow gold (#B8860B) border highlight |
| `ui_aggro_token.png` | 32×32 | Skull-and-flame icon, bone white, clear silhouette |

---

## Settlement Structures to Import

Use subfolder: **Settlement**.

**Save path:** `Assets/_Game/Art/Generated/Settlement/`  
*(Sprites are assigned directly on each CrafterSO via the `structureSprite` Inspector field — no Resources folder required.)*

Each structure appears additively in the settlement scene when unlocked.

| File | Crafter Name | Unlocked By | Canvas Size | Key Visual Check |
|---|---|---|---|---|
| `the_ossuary.png` | The Ossuary | The Gaunt | 80×96 | Stone chamber with bone racks and marrow extraction tools. Faint smoke from rendering pot. |
| `the_carapace_forge.png` | The Carapace Forge | Thornback | 80×96 | Heavy stone forge with shell plates stacked outside. Chimney with orange glow at opening. |
| `the_mire_apothecary.png` | The Mire Apothecary | Bog Caller | 64×80 | Small hut, vines/reeds on exterior, hanging bundles of swamp herbs at eaves. |
| `the_membrane_loft.png` | The Membrane Loft | The Shriek | 64×80 | Tall narrow building, stretched membranes visible on exterior drying frames. |
| `the_ichor_works.png` | The Ichor Works | The Spite | 64×80 | Low dark building, dripping vials/vessels on exterior, ominous dark-stained walls. |
| `the_auric_scales.png` | The Auric Scales | Gilded Serpent | 80×96 | Refined stone workshop, gilded scale decorations on exterior, warm lamplight glow. |
| `the_rot_garden.png` | The Rot Garden | Rotmother | 64×80 | Open-sided structure with fungal growths and hanging decay matter visible. Sickly green ambient. |
| `the_ivory_hall.png` | The Ivory Hall | The Ivory Stampede | 96×96 | Grand wide building, massive ivory tusks flanking the entrance, heaviest structure. |

**Shared style notes for all structures:**
- Side-profile view (not top-down) — consistent with settlement scene perspective
- Dark pixel art matching game palette (ash grey, bone white, dried blood brown, Marrow gold)
- Warm torch-glow light from windows and openings
- No text or labels in the sprite

---

## Wire Structures into SettlementScreenController

Add `RefreshSettlementScene()` to the **existing** `SettlementScreenController.cs` and call it from `OnEnable()` after the initial refresh:

```csharp
private void RefreshSettlementScene()
{
    var scene = _root.Q<VisualElement>("settlement-scene");
    if (scene == null || _campaignDataSO?.crafterPool == null) return;

    // Clear existing structure images (rebuild fresh each time)
    scene.Clear();

    // Keep the placeholder label if no crafters built yet
    var state = GameStateManager.Instance.CampaignState;
    bool anyBuilt = state.builtCrafterNames.Length > 0;

    if (!anyBuilt)
    {
        var placeholder = new UnityEngine.UIElements.Label("SETTLEMENT");
        placeholder.AddToClassList("settlement-scene-placeholder");
        scene.Add(placeholder);
        return;
    }

    foreach (var crafter in _campaignDataSO.crafterPool)
    {
        if (crafter == null) continue;
        bool isBuilt = System.Array.IndexOf(
            state.builtCrafterNames, crafter.crafterName) >= 0;
        if (!isBuilt) continue;

        var sprite = Resources.Load<Sprite>(
            $"Art/Generated/Settlement/{crafter.crafterName.Replace(" ", "_").ToLower()}");
        if (sprite == null)
        {
            Debug.LogWarning($"[Settlement] Sprite not found for: {crafter.crafterName}");
            continue;
        }

        var img = new UnityEngine.UIElements.Image { sprite = sprite };
        img.style.position = UnityEngine.UIElements.Position.Absolute;
        img.style.left     = crafter.settlementScenePosition.x;
        img.style.top      = crafter.settlementScenePosition.y;
        scene.Add(img);

        Debug.Log($"[Settlement] Structure placed: {crafter.crafterName} " +
                  $"at ({crafter.settlementScenePosition.x}, {crafter.settlementScenePosition.y})");
    }
}
```

Also call `RefreshSettlementScene()` inside `OnEndYearClicked()` so the scene updates when new crafters are built during the year.

---

## Import Settings (Apply to All UI and Settlement Sprites)

```
Texture Type:     Sprite (2D and UI)
Pixels Per Unit:  16
Filter Mode:      Point (No Filter)
Compression:      None
```

---

## Verification Test

- [ ] Stone panel texture tiles correctly in UI Builder (no seams visible)
- [ ] Card frame has transparent interior — only the border is opaque
- [ ] All 8 settlement structures exist in `Assets/_Game/Art/Generated/Settlement/`
- [ ] Each CrafterSO has its `structureSprite` field assigned in the Inspector
- [ ] All 8 settlement structures are visually distinct from each other
- [ ] All sprites imported with Point (No Filter)
- [ ] Unlock The Ossuary in a test playthrough → the_ossuary.png appears in settlement scene
- [ ] Settlement scene placeholder text hidden when at least one structure is present

---

## Next Session

**File:** `_Docs/Stage_07/STAGE_07_F.md`  
**Covers:** Aldric animation frames (Walk 4f, Attack 3f, Collapse 2f) + AudioManager with music context switching and death sting
