<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 7-E | Art Batch — UI Elements & Settlement Structures
Status: Stage 7-D complete. All monster sprites saved and
imported with Point filtering.
Task: Generate UI art (stone panel texture, card frame,
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

# Stage 7-E: Art Batch — UI Elements & Settlement Structures

**Resuming from:** Stage 7-D complete — all monster sprites saved  
**Done when:** All 5 UI element sprites and all 5 settlement structure sprites saved; Boneworks appears in settlement scene when built in a test playthrough  
**Commit:** `"7E: UI textures and settlement structure sprites — wired into settlement scene"`  
**Next session:** STAGE_07_F.md  

---

## UI Elements to Generate

**Save path:** `Assets/_Game/Art/Generated/UI/`

| File | Size | Prompt Key Details |
|---|---|---|
| `ui_stone_panel_bg.png` | 64×64 (tileable) | Stone tablet surface, fine chisel marks, relief texture, dark grey, seamless tile |
| `ui_card_frame.png` | 160×220 | Stone-carved card border, relief-cut inner frame, worn edges, transparent interior fill |
| `ui_button_normal.png` | 200×48 | Stone button, carved border, slightly raised, dark grey |
| `ui_button_hover.png` | 200×48 | Same as normal but Marrow gold (#B8860B) carved border highlight |
| `ui_aggro_token.png` | 32×32 | Skull-and-flame icon, bone white, pixelated, simple clear silhouette |

---

## Settlement Structures to Generate

**Save path:** `Assets/_Game/Art/Generated/Settlement/`

Each structure appears additively in the settlement scene when unlocked.

| File | Structure | Size | Key Details |
|---|---|---|---|
| `building_boneworks.png` | Boneworks | 80×96 | Workshop built from bones and sinew, crude but sturdy, faint smoke |
| `building_herbalist.png` | Herbalist | 64×80 | Small hut with dried plants hanging from eaves, rough stone |
| `building_forge.png` | Forge | 80×96 | Stone forge with chimney, orange glow from fire visible in opening |
| `building_tannery.png` | Tannery | 64×80 | Hide-stretching frames visible outside, crude wooden structure |
| `building_armory.png` | Armory | 80×96 | Stone building, weapon silhouettes carved on exterior wall |

**Shared style notes for all structures:**
- Side-profile view (not top-down) — consistent with settlement scene perspective
- Dark pixel art matching game palette
- Warm torch-glow light from windows and openings
- Crude but intentional — these settlers are survivors, not architects
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

## Sprite Import Settings

All UI and settlement sprites:

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
- [ ] All 5 settlement structures visually distinct from each other
- [ ] All sprites imported with Point (No Filter)
- [ ] Unlock Boneworks in a test playthrough → building_boneworks.png appears in settlement scene
- [ ] Settlement scene placeholder text hidden when at least one structure is present

---

## Next Session

**File:** `_Docs/Stage_07/STAGE_07_F.md`  
**Covers:** Aldric animation frames (Walk 4f, Attack 3f, Collapse 2f) + AudioManager with music context switching and death sting
