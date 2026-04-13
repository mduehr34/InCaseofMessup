<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 6-D | Chronicle Events, Guiding Principals, Crafters & Innovations
Status: Stage 6-C complete. Settlement screen loads with
correct era header and Characters tab working.
Task: Build event-modal.uxml and wire it to SettlementManager.
Build guiding-principal-modal.uxml. Complete Crafters tab
(built/available crafters, unlock button). Complete
Innovations tab (draw 3, adopt 1).

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_06/STAGE_06_D.md
- Assets/_Game/Scripts/Core.UI/SettlementScreenController.cs
- Assets/_Game/Scripts/Core.Systems/SettlementManager.cs
- Assets/_Game/Scripts/Core.Data/EventSO.cs
- Assets/_Game/Scripts/Core.Data/GuidingPrincipalSO.cs
- Assets/_Game/Scripts/Core.Data/InnovationSO.cs
- Assets/_Game/Scripts/Core.Data/CrafterSO.cs
- Assets/_Game/UI/USS/settlement-shared.uss

Then confirm:
- That modals are overlays on top of the Settlement screen
- That you will ADD to SettlementScreenController.cs
- That Chronicle Event auto-fires on Settlement load if
  eligible events exist
- What you will NOT touch this session

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 6-D: Chronicle Events, Guiding Principals, Crafters & Innovations

**Resuming from:** Stage 6-C complete — settlement screen with Characters tab  
**Done when:** Chronicle Event modal fires on settlement load (if eligible); player can make choices A/B or acknowledge mandatory events; Guiding Principal modal fires when triggered; Crafters tab shows built/available crafters with unlock button; Innovations tab shows 3 draw options and adopts correctly  
**Commit:** `"6D: Chronicle event modal, guiding principal modal, crafters tab, innovations tab"`  
**Next session:** STAGE_06_E.md  

---

## Step 1: event-modal.uxml

**Path:** `Assets/_Game/UI/UXML/event-modal.uxml`

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements" editor-extension-mode="False">
    <Style src="../USS/tokens.uss"/>
    <Style src="../USS/stone-panel.uss"/>
    <Style src="../USS/settlement-shared.uss"/>

    <ui:VisualElement name="event-overlay" class="modal-overlay">
        <ui:VisualElement name="event-modal" class="modal-panel stone-panel">

            <ui:Label name="event-id"        text="EVT-01"      class="proficiency-label"/>
            <ui:Label name="event-name"      text="Event Name"  class="stone-panel__header"/>
            <ui:Label name="event-mandatory" text="MANDATORY"   class="loud-tag"/>
            <ui:Label name="event-narrative" text="..."         class="event-narrative"/>

            <!-- Choice buttons — hidden for mandatory events -->
            <ui:VisualElement name="event-choices" class="event-choices">
                <ui:Button name="btn-choice-a" text="A: ..." class="action-btn"/>
                <ui:Button name="btn-choice-b" text="B: ..." class="action-btn"/>
            </ui:VisualElement>

            <!-- Acknowledge — shown for mandatory events only -->
            <ui:Button name="btn-acknowledge" text="ACKNOWLEDGE" class="action-btn action-btn--primary"/>

        </ui:VisualElement>
    </ui:VisualElement>
</ui:UXML>
```

---

## Step 2: guiding-principal-modal.uxml

**Path:** `Assets/_Game/UI/UXML/guiding-principal-modal.uxml`

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements" editor-extension-mode="False">
    <Style src="../USS/tokens.uss"/>
    <Style src="../USS/stone-panel.uss"/>
    <Style src="../USS/settlement-shared.uss"/>

    <ui:VisualElement name="gp-overlay" class="modal-overlay">
        <ui:VisualElement name="gp-modal" class="modal-panel stone-panel">

            <ui:Label text="GUIDING PRINCIPAL" class="proficiency-label"/>
            <ui:Label name="gp-name"           class="stone-panel__header"/>
            <ui:Label name="gp-trigger"        class="event-narrative"/>
            <ui:Label text="This choice is permanent." class="loud-tag"/>

            <ui:VisualElement name="gp-choices" class="event-choices">
                <ui:Button name="btn-gp-a" text="Choice A" class="action-btn"/>
                <ui:Button name="btn-gp-b" text="Choice B" class="action-btn"/>
            </ui:VisualElement>

        </ui:VisualElement>
    </ui:VisualElement>
</ui:UXML>
```

---

## Step 3: Add these USS rules to settlement-shared.uss

```css
/* ── Event Modal ─────────────────────────────────────────────── */
.event-narrative {
    white-space:  normal;
    font-size:    var(--font-size-body);
    color:        var(--color-text-primary);
    margin:       var(--spacing-md) 0;
    flex:         1;
}

.event-choices {
    flex-direction: row;
    justify-content: center;
    margin-top: var(--spacing-md);
}

.event-choices .action-btn {
    flex: 1;
    margin: 0 var(--spacing-sm);
}
```

---

## Step 4: Add to SettlementScreenController.cs

Add these fields and methods to the **existing** controller:

### New Fields

```csharp
[SerializeField] private VisualTreeAsset _eventModalAsset;
[SerializeField] private VisualTreeAsset _gpModalAsset;
[SerializeField] private CampaignSO      _campaignDataSO; // rename from _campaignSO if needed
private VisualElement _activeModal = null;
```

### Chronicle Event Flow

```csharp
// Call this at end of OnEnable(), after applying pending hunt result
private void CheckAndFireChronicleEvent()
{
    var evt = _settlement.DrawChronicleEvent();
    if (evt != null) ShowEventModal(evt);
}

private void ShowEventModal(EventSO evt)
{
    if (_eventModalAsset == null)
    {
        Debug.LogWarning("[Settlement] Event modal UXML asset not assigned");
        return;
    }

    var overlay = _eventModalAsset.Instantiate();
    _root.Add(overlay);
    _activeModal = overlay;

    overlay.Q<Label>("event-id").text        = evt.eventId;
    overlay.Q<Label>("event-name").text      = evt.eventName;
    overlay.Q<Label>("event-narrative").text = evt.narrativeText;

    bool isMandatory = evt.isMandatory || evt.choices == null || evt.choices.Length == 0;
    overlay.Q<Label>("event-mandatory").style.display =
        isMandatory ? DisplayStyle.Flex : DisplayStyle.None;

    var choicesEl    = overlay.Q<VisualElement>("event-choices");
    var ackBtn       = overlay.Q<Button>("btn-acknowledge");

    if (isMandatory)
    {
        choicesEl.style.display = DisplayStyle.None;
        ackBtn.style.display    = DisplayStyle.Flex;
        ackBtn.clicked += () => { _settlement.ResolveEvent(evt, -1); CloseModal(); };
    }
    else
    {
        choicesEl.style.display = DisplayStyle.Flex;
        ackBtn.style.display    = DisplayStyle.None;

        var btnA = overlay.Q<Button>("btn-choice-a");
        var btnB = overlay.Q<Button>("btn-choice-b");

        if (evt.choices.Length > 0)
        {
            btnA.text    = $"{evt.choices[0].choiceLabel}: {evt.choices[0].outcomeText}";
            btnA.clicked += () =>
            {
                _settlement.ResolveEvent(evt, 0);
                CloseModal();
                CheckAndFireGuidingPrincipal(); // May have been triggered by choice
            };
        }

        if (evt.choices.Length > 1)
        {
            btnB.text    = $"{evt.choices[1].choiceLabel}: {evt.choices[1].outcomeText}";
            btnB.clicked += () =>
            {
                _settlement.ResolveEvent(evt, 1);
                CloseModal();
                CheckAndFireGuidingPrincipal();
            };
        }
        else
        {
            btnB.style.display = DisplayStyle.None;
        }
    }

    Debug.Log($"[Settlement] Showing event: {evt.eventId} — {evt.eventName}");
}
```

### Guiding Principal Flow

```csharp
private void CheckAndFireGuidingPrincipal()
{
    var state = GameStateManager.Instance.CampaignState;
    if (state.activeGuidingPrincipalIds.Length == 0) return;

    // Show first active GP
    string gpId = state.activeGuidingPrincipalIds[0];
    var gpSO = FindGuidingPrincipal(gpId);
    if (gpSO != null) ShowGuidingPrincipalModal(gpSO);
}

private void ShowGuidingPrincipalModal(GuidingPrincipalSO gp)
{
    if (_gpModalAsset == null)
    {
        Debug.LogWarning("[Settlement] GP modal UXML asset not assigned");
        return;
    }

    var overlay = _gpModalAsset.Instantiate();
    _root.Add(overlay);
    _activeModal = overlay;

    overlay.Q<Label>("gp-name").text    = gp.principalName;
    overlay.Q<Label>("gp-trigger").text = gp.triggerCondition;

    var btnA = overlay.Q<Button>("btn-gp-a");
    var btnB = overlay.Q<Button>("btn-gp-b");

    btnA.text = $"A: {gp.choiceA.outcomeText}";
    btnB.text = $"B: {gp.choiceB.outcomeText}";

    btnA.clicked += () => { _settlement.ResolveGuidingPrincipal(gp.principalId, 0); CloseModal(); };
    btnB.clicked += () => { _settlement.ResolveGuidingPrincipal(gp.principalId, 1); CloseModal(); };

    Debug.Log($"[Settlement] Showing Guiding Principal: {gp.principalId}");
}

private void CloseModal()
{
    if (_activeModal != null)
    {
        _root.Remove(_activeModal);
        _activeModal = null;
    }
    RefreshResourceSummary();
    if (_activeTab == "characters") BuildCharactersTab();
}

private GuidingPrincipalSO FindGuidingPrincipal(string id)
{
    if (_campaignDataSO?.guidingPrincipals == null) return null;
    return System.Array.Find(_campaignDataSO.guidingPrincipals, gp => gp.principalId == id);
}
```

### Complete Crafters Tab

```csharp
private void BuildCraftersTab()
{
    _tabContent.Clear();
    var state = GameStateManager.Instance.CampaignState;
    if (_campaignDataSO?.crafterPool == null)
    {
        _tabContent.Add(new Label("No crafters defined in CampaignSO"));
        return;
    }

    foreach (var crafter in _campaignDataSO.crafterPool)
    {
        if (crafter == null) continue;
        bool isBuilt = System.Array.IndexOf(state.builtCrafterNames, crafter.crafterName) >= 0;

        var row = new VisualElement();
        row.AddToClassList("character-row");
        row.AddToClassList(isBuilt ? "stone-panel--raised" : "stone-panel");

        var nameLabel = new Label(crafter.crafterName);
        nameLabel.AddToClassList("character-name");
        if (!isBuilt) nameLabel.style.color = new UnityEngine.UIElements.StyleColor(
            new Color(0.54f, 0.50f, 0.44f)); // dim if not built
        row.Add(nameLabel);

        if (isBuilt)
        {
            var builtTag = new Label("BUILT");
            builtTag.AddToClassList("status-badge");
            builtTag.style.color = new UnityEngine.UIElements.StyleColor(
                new Color(0.40f, 0.72f, 0.40f));
            row.Add(builtTag);

            var recipeLabel = new Label($"{crafter.recipeList?.Length ?? 0} recipes");
            recipeLabel.AddToClassList("proficiency-label");
            row.Add(recipeLabel);
        }
        else
        {
            // Show unlock cost
            string cost = BuildCostString(crafter);
            var costLabel = new Label(cost);
            costLabel.AddToClassList("proficiency-label");
            row.Add(costLabel);

            var crafterRef = crafter;
            var unlockBtn = new Button(() => OnUnlockCrafter(crafterRef)) { text = "UNLOCK" };
            unlockBtn.AddToClassList("small-btn");
            row.Add(unlockBtn);
        }

        _tabContent.Add(row);
    }
}

private string BuildCostString(CrafterSO crafter)
{
    if (crafter.unlockCost == null || crafter.unlockCost.Length == 0) return "Free";
    var parts = new System.Collections.Generic.List<string>();
    for (int i = 0; i < crafter.unlockCost.Length; i++)
    {
        int amt = (crafter.unlockCostAmounts != null && i < crafter.unlockCostAmounts.Length)
            ? crafter.unlockCostAmounts[i] : 0;
        parts.Add($"{amt}× {crafter.unlockCost[i].resourceName}");
    }
    return string.Join(", ", parts);
}

private void OnUnlockCrafter(CrafterSO crafter)
{
    bool success = _settlement.TryUnlockCrafter(crafter);
    if (success)
    {
        Debug.Log($"[Settlement] Unlocked: {crafter.crafterName}");
        BuildCraftersTab(); // Refresh
    }
}
```

### Complete Innovations Tab

```csharp
private InnovationSO[] _drawnInnovations = null;

private void BuildInnovationsTab()
{
    _tabContent.Clear();
    var state = GameStateManager.Instance.CampaignState;

    // Already adopted innovations
    var adoptedHeader = new Label("ADOPTED INNOVATIONS");
    adoptedHeader.AddToClassList("stone-panel__header");
    _tabContent.Add(adoptedHeader);

    if (state.adoptedInnovationIds.Length == 0)
    {
        _tabContent.Add(new Label("None yet") { });
    }
    else
    {
        foreach (var id in state.adoptedInnovationIds)
        {
            var label = new Label($"• {id}");
            label.AddToClassList("proficiency-label");
            _tabContent.Add(label);
        }
    }

    // Draw options
    var drawHeader = new Label("AVAILABLE TO ADOPT");
    drawHeader.AddToClassList("stone-panel__header");
    _tabContent.Add(drawHeader);

    if (_drawnInnovations == null)
        _drawnInnovations = _settlement.DrawInnovationOptions(3);

    if (_drawnInnovations.Length == 0)
    {
        _tabContent.Add(new Label("No innovations available"));
        return;
    }

    foreach (var inn in _drawnInnovations)
    {
        var row = new VisualElement();
        row.AddToClassList("character-row");
        row.AddToClassList("stone-panel");

        var nameLabel = new Label(inn.innovationName);
        nameLabel.AddToClassList("character-name");
        row.Add(nameLabel);

        var effectLabel = new Label(inn.effect);
        effectLabel.AddToClassList("proficiency-label");
        effectLabel.style.whiteSpace = WhiteSpace.Normal;
        effectLabel.style.flexShrink = 1;
        row.Add(effectLabel);

        var innRef = inn;
        var adoptBtn = new Button(() => OnAdoptInnovation(innRef)) { text = "ADOPT" };
        adoptBtn.AddToClassList("small-btn");
        row.Add(adoptBtn);

        _tabContent.Add(row);
    }
}

private void OnAdoptInnovation(InnovationSO innovation)
{
    _settlement.AdoptInnovation(innovation);
    _drawnInnovations = null; // Reset draw — each settlement phase draws fresh
    BuildInnovationsTab();
    Debug.Log($"[Settlement] Adopted: {innovation.innovationName}");
}
```

Also update `OnEnable()` to call `CheckAndFireChronicleEvent()` at the end:

```csharp
// Add at end of OnEnable()
CheckAndFireChronicleEvent();
if (GameStateManager.Instance.CampaignState.activeGuidingPrincipalIds.Length > 0)
    CheckAndFireGuidingPrincipal();
```

---

## Verification Test

1. Load Settlement scene (start new campaign from MainMenu)
2. If `CampaignSO.eventPool` has eligible Year 1 events, event modal fires automatically
3. Mandatory event shows ACKNOWLEDGE button only — no A/B choices
4. Choice event shows both buttons — selecting one resolves correctly
5. After event resolved, Guiding Principal modal fires if triggered
6. Crafters tab shows all crafters from SO with BUILT/UNLOCK states
7. Innovations tab shows up to 3 draw options; ADOPT adopts and refreshes

---

## Next Session

**File:** `_Docs/Stage_06/STAGE_06_E.md`  
**Covers:** Gear Grid screen — 4×4 layout, item placement, drag-and-drop, link resolver, stats summary
