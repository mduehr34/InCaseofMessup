<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 6-B | Main Menu & Campaign Select Screens
Status: Stage 6-A complete. GameStateManager persists across
scenes. All scenes in Build Settings. settlement-shared.uss
has zero errors.
Task: Create main-menu.uxml + MainMenuController.cs, and
campaign-select.uxml + CampaignSelectController.cs.
New Campaign → Campaign Select → starts campaign and loads
Settlement. Continue → loads save and goes to Settlement.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_06/STAGE_06_B.md
- Assets/_Game/Scripts/Core.Systems/GameStateManager.cs
- Assets/_Game/Scripts/Core.Systems/SaveManager.cs
- Assets/_Game/UI/USS/tokens.uss
- Assets/_Game/UI/USS/stone-panel.uss
- Assets/_Game/UI/USS/settlement-shared.uss

Then confirm:
- The 4 files you will create (2 UXML, 2 C#)
- That all navigation goes through GameStateManager
- That Continue button is disabled when HasSave is false
- What you will NOT touch this session

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 6-B: Main Menu & Campaign Select Screens

**Resuming from:** Stage 6-A complete  
**Done when:** Main menu renders with correct buttons; Continue disabled when no save; New Campaign navigates to Campaign Select; selecting a campaign starts it and loads Settlement scene  
**Commit:** `"6B: Main menu and campaign select screens wired"`  
**Next session:** STAGE_06_C.md  

---

## Step 1: main-menu.uxml

**Path:** `Assets/_Game/UI/UXML/main-menu.uxml`

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements" editor-extension-mode="False">
    <Style src="../USS/tokens.uss"/>
    <Style src="../USS/stone-panel.uss"/>
    <Style src="../USS/settlement-shared.uss"/>

    <ui:VisualElement name="main-menu-root" class="fullscreen-bg">

        <!-- Title Block -->
        <ui:VisualElement name="title-block" class="title-block">
            <ui:Label text="MARROW"        class="title-marrow"/>
            <ui:Label text="&amp; MYTH"    class="title-myth"/>
            <ui:Label text="THE UNMADE AGE" class="title-subtitle"/>
        </ui:VisualElement>

        <!-- Menu Buttons -->
        <ui:VisualElement name="menu-buttons" class="menu-buttons stone-panel">
            <ui:Button name="btn-new-campaign" text="NEW CAMPAIGN" class="menu-btn"/>
            <ui:Button name="btn-continue"     text="CONTINUE"     class="menu-btn"/>
            <ui:Button name="btn-codex"        text="CODEX"        class="menu-btn menu-btn--secondary"/>
            <ui:Button name="btn-settings"     text="SETTINGS"     class="menu-btn menu-btn--secondary"/>
            <ui:Button name="btn-credits"      text="CREDITS"      class="menu-btn menu-btn--secondary"/>
        </ui:VisualElement>

        <ui:Label name="version-label" text="v0.1.0" class="version-label"/>

    </ui:VisualElement>
</ui:UXML>
```

---

## Step 2: MainMenuController.cs

**Path:** `Assets/_Game/Scripts/Core.UI/MainMenuController.cs`

```csharp
using UnityEngine;
using UnityEngine.UIElements;
using MnM.Core.Systems;

namespace MnM.Core.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;

        private void OnEnable()
        {
            var root = _uiDocument.rootVisualElement;

            // New Campaign → Campaign Select
            root.Q<Button>("btn-new-campaign").clicked += () =>
            {
                Debug.Log("[MainMenu] New Campaign clicked");
                UnityEngine.SceneManagement.SceneManager.LoadScene("CampaignSelect");
            };

            // Continue → load save → Settlement
            var continueBtn = root.Q<Button>("btn-continue");
            bool hasSave = GameStateManager.Instance?.HasSave ?? SaveManager.HasSave();
            continueBtn.SetEnabled(hasSave);
            continueBtn.clicked += () =>
            {
                var state = SaveManager.Load();
                if (state != null)
                    GameStateManager.Instance.LoadCampaign(state);
                else
                    Debug.LogError("[MainMenu] Continue clicked but save failed to load");
            };

            // Codex — load additively (stub: just log for now)
            root.Q<Button>("btn-codex").clicked += () =>
                Debug.Log("[MainMenu] Codex — implement in Stage 6-G");

            // Settings / Credits — stub
            root.Q<Button>("btn-settings").clicked += () =>
                Debug.Log("[MainMenu] Settings — post-MVP");
            root.Q<Button>("btn-credits").clicked += () =>
                Debug.Log("[MainMenu] Credits — post-MVP");

            // Version label
            var versionLabel = root.Q<Label>("version-label");
            if (versionLabel != null)
                versionLabel.text = $"v{Application.version}";
        }
    }
}
```

---

## Step 3: campaign-select.uxml

**Path:** `Assets/_Game/UI/UXML/campaign-select.uxml`

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements" editor-extension-mode="False">
    <Style src="../USS/tokens.uss"/>
    <Style src="../USS/stone-panel.uss"/>
    <Style src="../USS/settlement-shared.uss"/>

    <ui:VisualElement name="campaign-select-root" class="fullscreen-bg">

        <!-- Header -->
        <ui:VisualElement class="era-bar stone-panel--raised">
            <ui:Label text="SELECT CAMPAIGN" class="era-year"/>
            <ui:VisualElement style="flex:1"/>
            <ui:Button name="btn-back" text="BACK" class="era-btn"/>
        </ui:VisualElement>

        <!-- Campaign Options -->
        <ui:VisualElement name="campaign-list" class="campaign-list"/>

        <!-- Difficulty Info Panel (shown on hover/select) -->
        <ui:VisualElement name="campaign-detail" class="campaign-detail stone-panel">
            <ui:Label name="campaign-name"        text="---"   class="stone-panel__header"/>
            <ui:Label name="campaign-difficulty"  text="---"   class="campaign-info-label"/>
            <ui:Label name="campaign-description" text="---"   class="campaign-description"/>
            <ui:Label name="campaign-characters"  text="---"   class="campaign-info-label"/>
            <ui:Label name="campaign-length"      text="---"   class="campaign-info-label"/>
            <ui:Button name="btn-start-campaign"  text="BEGIN" class="action-btn action-btn--primary"/>
        </ui:VisualElement>

        <!-- Tutorial toggle -->
        <ui:VisualElement class="action-bar stone-panel--raised">
            <ui:Button name="btn-tutorial" text="PLAY TUTORIAL FIRST" class="action-btn"/>
        </ui:VisualElement>

    </ui:VisualElement>
</ui:UXML>
```

---

## Step 4: CampaignSelectController.cs

**Path:** `Assets/_Game/Scripts/Core.UI/CampaignSelectController.cs`

```csharp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using MnM.Core.Systems;
using MnM.Core.Data;

namespace MnM.Core.UI
{
    public class CampaignSelectController : MonoBehaviour
    {
        [SerializeField] private UIDocument  _uiDocument;
        // Assign all available CampaignSO assets in the Inspector
        [SerializeField] private CampaignSO[] _availableCampaigns;
        [SerializeField] private CampaignSO   _tutorialCampaign;

        private CampaignSO _selectedCampaign;
        private VisualElement _root;

        private void OnEnable()
        {
            _root = _uiDocument.rootVisualElement;

            _root.Q<Button>("btn-back").clicked += () =>
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");

            _root.Q<Button>("btn-start-campaign").clicked += OnStartCampaign;

            _root.Q<Button>("btn-tutorial").clicked += () =>
            {
                if (_tutorialCampaign != null)
                    GameStateManager.Instance.StartNewCampaign(_tutorialCampaign);
                else
                    Debug.LogError("[CampaignSelect] Tutorial CampaignSO not assigned");
            };

            BuildCampaignList();

            // Pre-select first campaign
            if (_availableCampaigns != null && _availableCampaigns.Length > 0)
                SelectCampaign(_availableCampaigns[0]);
        }

        private void BuildCampaignList()
        {
            var list = _root.Q<VisualElement>("campaign-list");
            if (list == null || _availableCampaigns == null) return;
            list.Clear();

            foreach (var campaign in _availableCampaigns)
            {
                if (campaign == null) continue;
                var row = BuildCampaignRow(campaign);
                list.Add(row);
            }
        }

        private VisualElement BuildCampaignRow(CampaignSO campaign)
        {
            var row = new VisualElement();
            row.AddToClassList("character-row");
            row.AddToClassList("stone-panel");

            var nameLabel = new Label(campaign.campaignName);
            nameLabel.AddToClassList("character-name");
            row.Add(nameLabel);

            var diffLabel = new Label(campaign.difficulty.ToString());
            diffLabel.AddToClassList("proficiency-label");
            row.Add(diffLabel);

            row.RegisterCallback<ClickEvent>(_ => SelectCampaign(campaign));
            return row;
        }

        private void SelectCampaign(CampaignSO campaign)
        {
            _selectedCampaign = campaign;

            _root.Q<Label>("campaign-name").text =
                campaign.campaignName;
            _root.Q<Label>("campaign-difficulty").text =
                $"Difficulty: {campaign.difficulty}";
            _root.Q<Label>("campaign-description").text =
                $"{campaign.campaignLengthYears}-year campaign";
            _root.Q<Label>("campaign-characters").text =
                $"Starting characters: {campaign.startingCharacterCount}";
            _root.Q<Label>("campaign-length").text =
                $"Ironman: {(campaign.ironmanMode ? "Yes" : "No")}";

            Debug.Log($"[CampaignSelect] Selected: {campaign.campaignName}");
        }

        private void OnStartCampaign()
        {
            if (_selectedCampaign == null)
            {
                Debug.LogWarning("[CampaignSelect] No campaign selected");
                return;
            }
            GameStateManager.Instance.StartNewCampaign(_selectedCampaign);
        }
    }
}
```

Also add `CampaignSelect` to Build Settings (index 5).

---

## Verification Test

1. Play `MainMenu` scene
2. Confirm title text "MARROW", "& MYTH", "THE UNMADE AGE" renders
3. Confirm Continue button is disabled (greyed) when no save file exists
4. Click New Campaign → `CampaignSelect` scene loads
5. Confirm campaign list populates from assigned `CampaignSO` assets in Inspector
6. Click a campaign → detail panel updates with name, difficulty, character count
7. Click Back → returns to MainMenu
8. Click Begin → `GameStateManager.StartNewCampaign()` called → Settlement loads

---

## Next Session

**File:** `_Docs/Stage_06/STAGE_06_C.md`  
**Covers:** Settlement screen UXML + controller, era header, Characters tab
