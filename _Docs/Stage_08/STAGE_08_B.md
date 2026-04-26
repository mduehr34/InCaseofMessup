<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 8-B | Campaign Select & New Game Flow
Status: Stage 8-A complete. Main menu opens on launch.
Task: Create the CampaignSelect scene. Show Tutorial and
Standard campaign cards. Add difficulty selector and ironman
toggle. CONFIRM starts a new campaign and loads Character
Creation (Stage 8-C). Wire CampaignSO data into the UI.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_08/STAGE_08_B.md
- Assets/_Game/Scripts/Core.Data/CampaignSO.cs
- Assets/_Game/Data/Campaigns/Campaign_Tutorial.asset
- Assets/_Game/Data/Campaigns/Campaign_Standard.asset

Then confirm:
- Campaign cards pull name and length from CampaignSO
- Selecting a campaign updates a preview panel
- Difficulty only affects Standard (Tutorial is always Medium)
- GameStateManager.Instance.StartNewCampaign() is called on CONFIRM
- What you will NOT build this session (character creation — 8-C)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 8-B: Campaign Select & New Game Flow

**Resuming from:** Stage 8-A complete — main menu working
**Done when:** Player can select Tutorial or Standard, adjust difficulty, toggle ironman, press CONFIRM, and land on the Character Creation scene
**Commit:** `"8B: Campaign select scene — campaign picker, difficulty, ironman toggle, CONFIRM flow"`
**Next session:** STAGE_08_C.md

---

## What You Are Building

After the player clicks NEW GAME they arrive here. They choose:
1. **Campaign type** — Tutorial (3 years, beginner-friendly) or Standard (30 years, full game)
2. **Difficulty** — Easy / Medium / Hard (Standard only; Tutorial is locked to Medium)
3. **Ironman** — a toggle: if on, there is only one save slot and no going back
4. Then they press **CONFIRM** and get taken to character creation

**New developer note:** We store the player's choices in `GameStateManager` (a singleton that persists between scenes) before loading the next scene. This way Character Creation knows which campaign was selected.

---

## Step 1: Generate Campaign Art

Use CoPlay `generate_or_edit_images`:

**Tutorial card art (160×200):**
```
Pixel art card illustration. A small stone settlement with
one lit building, a lone hunter silhouette at the gate.
Hopeful but sparse. Style: dark 16-bit pixel art,
bone white and ash grey palette, warm torch glow.
Label space at bottom. Transparent card border area.
```
Save to: `Assets/_Game/Art/Generated/UI/ui_campaign_tutorial.png`

**Standard card art (160×200):**
```
Pixel art card illustration. A larger settlement surrounded
by dark wilderness. Multiple hunters visible. Distant monster
silhouette on the horizon. Sense of scale and danger.
Style: dark 16-bit pixel art, bone white, ash grey, marrow gold.
```
Save to: `Assets/_Game/Art/Generated/UI/ui_campaign_standard.png`

Import settings: Sprite (2D and UI), Point (No Filter), PPU 16

---

## Step 2: Create Scene

1. **File → New Scene → Empty** → save as `Assets/CampaignSelect.unity`
2. Add to Build Settings after MainMenu
3. Add a UIDocument GameObject named `CampaignSelectUI`
4. Reuse `MainMenuPanelSettings` for the Panel Settings asset

---

## Step 3: UXML Layout

Create `Assets/_Game/UI/CampaignSelect.uxml`:

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
  <ui:VisualElement name="root" style="width:100%; height:100%; align-items:center;
      justify-content:center; background-color:#0A0A0C; flex-direction:column;">

    <ui:Label name="header" text="CHOOSE YOUR CAMPAIGN"
              style="color:#D4CCBA; font-size:18px; margin-bottom:32px;" />

    <!-- Campaign cards row -->
    <ui:VisualElement name="card-row" style="flex-direction:row; gap:32px; margin-bottom:32px;">

      <ui:VisualElement name="card-tutorial" class="campaign-card">
        <ui:VisualElement name="card-tutorial-art" class="campaign-card-art" />
        <ui:Label name="card-tutorial-title" text="TUTORIAL" class="campaign-card-title" />
        <ui:Label name="card-tutorial-desc"
                  text="3 years. Learn the hunt.&#10;One monster. No permanent death."
                  class="campaign-card-desc" />
      </ui:VisualElement>

      <ui:VisualElement name="card-standard" class="campaign-card">
        <ui:VisualElement name="card-standard-art" class="campaign-card-art" />
        <ui:Label name="card-standard-title" text="THE STANDARD CAMPAIGN" class="campaign-card-title" />
        <ui:Label name="card-standard-desc"
                  text="30 years. Build. Hunt. Survive.&#10;All monsters. Permanent consequences."
                  class="campaign-card-desc" />
      </ui:VisualElement>

    </ui:VisualElement>

    <!-- Options row -->
    <ui:VisualElement name="options-row" style="flex-direction:row; gap:48px; margin-bottom:32px; align-items:center;">

      <ui:VisualElement name="difficulty-group" style="flex-direction:column; align-items:center;">
        <ui:Label text="DIFFICULTY" style="color:#8A8A8A; font-size:10px; margin-bottom:8px;" />
        <ui:VisualElement name="difficulty-buttons" style="flex-direction:row; gap:8px;">
          <ui:Button name="btn-easy"   text="EASY"   class="diff-btn" />
          <ui:Button name="btn-medium" text="MEDIUM" class="diff-btn diff-btn--selected" />
          <ui:Button name="btn-hard"   text="HARD"   class="diff-btn" />
        </ui:VisualElement>
      </ui:VisualElement>

      <ui:VisualElement name="ironman-group" style="flex-direction:column; align-items:center;">
        <ui:Label text="IRONMAN MODE" style="color:#8A8A8A; font-size:10px; margin-bottom:8px;" />
        <ui:Toggle name="toggle-ironman" label="" />
        <ui:Label text="One save. No reloads." style="color:#4A2020; font-size:9px;" />
      </ui:VisualElement>

    </ui:VisualElement>

    <!-- Navigation -->
    <ui:VisualElement style="flex-direction:row; gap:24px;">
      <ui:Button name="btn-back"    text="← BACK"   class="mnm-btn-secondary" />
      <ui:Button name="btn-confirm" text="CONFIRM →" class="mnm-btn-primary" />
    </ui:VisualElement>

  </ui:VisualElement>
</ui:UXML>
```

Create `Assets/_Game/UI/CampaignSelect.uss`:

```css
.campaign-card {
    width: 180px;
    border-color: rgb(80, 70, 50);
    border-width: 2px;
    padding: 8px;
    background-color: rgb(20, 16, 12);
    flex-direction: column;
    align-items: center;
    cursor: pointer;
}

.campaign-card--selected {
    border-color: rgb(184, 134, 11);
    background-color: rgb(30, 24, 16);
}

.campaign-card-art {
    width: 160px;
    height: 120px;
    margin-bottom: 8px;
    -unity-background-scale-mode: scale-to-fit;
}

.campaign-card-title {
    color: rgb(212, 204, 186);
    font-size: 12px;
    margin-bottom: 6px;
    -unity-text-align: upper-center;
}

.campaign-card-desc {
    color: rgb(138, 138, 138);
    font-size: 9px;
    -unity-text-align: upper-center;
    white-space: normal;
}

.diff-btn {
    width: 72px; height: 32px;
    background-color: rgb(20, 16, 12);
    border-color: rgb(80, 70, 50);
    border-width: 1px;
    color: rgb(138, 138, 138);
    font-size: 10px;
}

.diff-btn--selected {
    border-color: rgb(184, 134, 11);
    color: rgb(212, 204, 186);
    background-color: rgb(40, 32, 20);
}

.mnm-btn-primary {
    width: 160px; height: 48px;
    background-color: rgb(74, 32, 32);
    border-color: rgb(184, 134, 11);
    border-width: 2px;
    color: rgb(212, 204, 186);
    font-size: 14px;
}

.mnm-btn-secondary {
    width: 120px; height: 48px;
    background-color: rgb(20, 16, 12);
    border-color: rgb(80, 70, 50);
    border-width: 1px;
    color: rgb(138, 138, 138);
    font-size: 12px;
}
```

---

## Step 4: CampaignSelectController.cs

**Path:** `Assets/_Game/Scripts/Core.UI/CampaignSelectController.cs`

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using MnM.Core.Data;
using MnM.Core.Systems;

namespace MnM.Core.UI
{
    public class CampaignSelectController : MonoBehaviour
    {
        [SerializeField] private UIDocument  _uiDocument;
        [SerializeField] private CampaignSO  _tutorialCampaign;
        [SerializeField] private CampaignSO  _standardCampaign;

        private CampaignSO   _selectedCampaign;
        private string        _selectedDifficulty = "Medium";
        private bool          _ironman            = false;

        private void OnEnable()
        {
            _selectedCampaign = _tutorialCampaign;  // default selection

            var root = _uiDocument.rootVisualElement;

            // Load card art
            SetCardArt(root, "card-tutorial-art", "Art/Generated/UI/ui_campaign_tutorial");
            SetCardArt(root, "card-standard-art",  "Art/Generated/UI/ui_campaign_standard");

            // Campaign card selection
            root.Q("card-tutorial").RegisterCallback<ClickEvent>(_ => SelectCampaign(root, _tutorialCampaign));
            root.Q("card-standard").RegisterCallback<ClickEvent>(_ => SelectCampaign(root, _standardCampaign));
            HighlightCard(root, "card-tutorial");

            // Difficulty buttons
            root.Q<Button>("btn-easy")  .RegisterCallback<ClickEvent>(_ => SetDifficulty(root, "Easy"));
            root.Q<Button>("btn-medium").RegisterCallback<ClickEvent>(_ => SetDifficulty(root, "Medium"));
            root.Q<Button>("btn-hard")  .RegisterCallback<ClickEvent>(_ => SetDifficulty(root, "Hard"));

            // Ironman toggle
            root.Q<Toggle>("toggle-ironman").RegisterValueChangedCallback(evt => _ironman = evt.newValue);

            // Navigation
            root.Q<Button>("btn-back")   .RegisterCallback<ClickEvent>(_ => SceneManager.LoadScene("MainMenu"));
            root.Q<Button>("btn-confirm").RegisterCallback<ClickEvent>(_ => OnConfirm());
        }

        private void SelectCampaign(VisualElement root, CampaignSO campaign)
        {
            _selectedCampaign = campaign;
            string cardId = campaign == _tutorialCampaign ? "card-tutorial" : "card-standard";
            HighlightCard(root, cardId);

            // Difficulty locked to Medium for Tutorial
            bool lockDiff = campaign == _tutorialCampaign;
            root.Q("difficulty-group").SetEnabled(!lockDiff);
            if (lockDiff) SetDifficulty(root, "Medium");
        }

        private void HighlightCard(VisualElement root, string selectedId)
        {
            foreach (string id in new[] { "card-tutorial", "card-standard" })
            {
                var card = root.Q(id);
                if (id == selectedId) card.AddToClassList("campaign-card--selected");
                else                  card.RemoveFromClassList("campaign-card--selected");
            }
        }

        private void SetDifficulty(VisualElement root, string diff)
        {
            _selectedDifficulty = diff;
            foreach (string id in new[] { "btn-easy", "btn-medium", "btn-hard" })
            {
                var btn = root.Q<Button>(id);
                string label = btn.text; // "EASY", "MEDIUM", "HARD"
                if (label.Equals(diff.ToUpper()))
                    btn.AddToClassList("diff-btn--selected");
                else
                    btn.RemoveFromClassList("diff-btn--selected");
            }
        }

        private void OnConfirm()
        {
            if (_selectedCampaign == null)
            {
                Debug.LogWarning("[CampaignSelect] No campaign selected");
                return;
            }
            Debug.Log($"[CampaignSelect] Starting: {_selectedCampaign.campaignName} / {_selectedDifficulty} / Ironman={_ironman}");
            GameStateManager.Instance.PrepareNewCampaign(_selectedCampaign, _selectedDifficulty, _ironman);
            SceneManager.LoadScene("CharacterCreation");
        }

        private void SetCardArt(VisualElement root, string elementId, string resourcePath)
        {
            var sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite != null)
                root.Q(elementId).style.backgroundImage = new StyleBackground(sprite);
        }
    }
}
```

**Wiring in Inspector:**
- Add `CampaignSelectController` component to `CampaignSelectUI` GameObject
- Drag `Campaign_Tutorial.asset` → **Tutorial Campaign** field
- Drag `Campaign_Standard.asset` → **Standard Campaign** field

**Add to GameStateManager** — add this method stub (implement logic later):
```csharp
public void PrepareNewCampaign(CampaignSO campaign, string difficulty, bool ironman)
{
    // Store pending campaign setup — CharacterCreation will call StartNewCampaign()
    _pendingCampaign   = campaign;
    _pendingDifficulty = difficulty;
    _pendingIronman    = ironman;
    Debug.Log($"[GSM] Pending campaign: {campaign.campaignName}");
}
```

---

## Verification Test

- [ ] CampaignSelect scene opens after clicking NEW GAME from main menu
- [ ] Tutorial card selected by default (gold border)
- [ ] Clicking Standard card moves gold border to Standard
- [ ] Difficulty buttons disabled for Tutorial; active for Standard
- [ ] Clicking EASY/HARD updates which button has gold highlight
- [ ] Ironman toggle toggles on/off
- [ ] BACK button returns to MainMenu
- [ ] CONFIRM logs correct campaign name, difficulty, ironman status, then loads CharacterCreation
- [ ] No Console errors on scene load

---

## Next Session

**File:** `_Docs/Stage_08/STAGE_08_C.md`
**Covers:** Character Creation scene — auto-generate 8 hunters from name pool, let player rename each one, show build silhouette art per build type, confirm starting party before launching the first settlement year
