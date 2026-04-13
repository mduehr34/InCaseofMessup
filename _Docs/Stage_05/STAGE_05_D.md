<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 5-D | Card Hand, Card Click & Monster Panel
Status: Stage 5-C complete. Hunter panels render correctly.
Phase label updates on phase change. Aggro indicator visible.
Task: Implement card hand rendering (card elements built
from ActionCardSO data), card click entering target-select
mode, and the full monster panel (all parts with Shell/Flesh
bars, broken state, exposed tag, deck count).

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_05/STAGE_05_D.md
- Assets/_Game/Scripts/Core.UI/CombatScreenController.cs
- Assets/_Game/Scripts/Core.Systems/ICombatManager.cs
- Assets/_Game/Scripts/Core.Data/CombatState.cs
- Assets/_Game/Scripts/Core.Data/ActionCardSO.cs
- Assets/_Game/UI/USS/combat-screen.uss

Then confirm:
- That you will ADD to CombatScreenController.cs,
  not replace it
- That clicking a card sets a pending-card state
  and does NOT immediately resolve the card
- That the grid click (Session 5-E) resolves the card
- What you will NOT touch this session

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 5-D: Card Hand, Card Click & Monster Panel

**Resuming from:** Stage 5-C complete — hunter panels and phase label verified  
**Done when:** Card hand shows mock cards with correct names, AP costs, Loud tags; clicking a card highlights it (pending state); monster panel shows all parts with Shell/Flesh bars  
**Commit:** `"5D: Card hand rendering, card click target-select mode, monster panel"`  
**Next session:** STAGE_05_E.md  

---

## Step 1: Add to CombatScreenController.cs

Add these fields and methods to the **existing** `CombatScreenController` class:

### New Fields

```csharp
// ── Card Selection State ─────────────────────────────────────
private string        _pendingCardName   = null;   // Card selected, awaiting target
private VisualElement _selectedCardEl    = null;   // Currently highlighted card element

// ── SO Registry (simple Resources.Load for now) ─────────────
// Stage 6 will replace this with a proper registry lookup
private ActionCardSO LoadCard(string cardName) =>
    Resources.Load<ActionCardSO>($"Data/Cards/Action/{cardName}");
```

### Replace RefreshAll() to include card hand and monster panel

```csharp
public void RefreshAll()
{
    var state = _combatManager?.CurrentState;
    if (state == null) return;

    RefreshHunterPanels(state.hunters, state.aggroHolderId);
    RefreshMonsterPanel(state.monster);     // Now fully implemented
    RefreshCardHand(state);                 // Now fully implemented
}
```

### Card Hand

```csharp
private void RefreshCardHand(CombatState state)
{
    if (_handCards == null) return;
    _handCards.Clear();
    _selectedCardEl  = null;
    _pendingCardName = null;

    // Find the active hunter (first who hasn't acted and isn't collapsed)
    var activeHunter = System.Array.Find(
        state.hunters, h => !h.hasActedThisPhase && !h.isCollapsed);

    if (activeHunter == null)
    {
        Debug.Log("[CombatUI] No active hunter — card hand empty");
        return;
    }

    // Update AP and Grit display in hand action panel
    var apDisplay = _root.Q<Label>("ap-display");
    if (apDisplay != null)
        apDisplay.text = $"AP: {activeHunter.apRemaining}";

    var gritDisplay = _root.Q<Label>("grit-display");
    if (gritDisplay != null)
        gritDisplay.text = $"Grit: {activeHunter.currentGrit}";

    // Build a card element for each card in hand
    foreach (var cardName in activeHunter.handCardNames)
    {
        var cardEl = BuildCardElement(cardName, activeHunter);
        _handCards.Add(cardEl);
    }
}

private VisualElement BuildCardElement(string cardName, HunterCombatState hunter)
{
    var card = LoadCard(cardName);

    var el = new VisualElement();
    el.AddToClassList("card");
    el.AddToClassList("stone-panel");

    // ── Header row: name + Loud tag ──────────────────────────
    var header = new VisualElement();
    header.AddToClassList("card-header");

    var nameLabel = new Label(card != null ? card.cardName : cardName);
    nameLabel.AddToClassList("card-name");
    header.Add(nameLabel);

    if (card != null && card.isLoud)
    {
        var loudTag = new Label("LOUD");
        loudTag.AddToClassList("loud-tag");
        header.Add(loudTag);
    }
    el.Add(header);

    // ── Category ─────────────────────────────────────────────
    if (card != null)
    {
        var categoryLabel = new Label(card.category.ToString().ToUpper());
        categoryLabel.AddToClassList("card-category");
        el.Add(categoryLabel);
    }

    // ── Effect text ──────────────────────────────────────────
    var effectLabel = new Label(card != null ? card.effectDescription : "");
    effectLabel.AddToClassList("card-effect");
    el.Add(effectLabel);

    // ── Footer: AP cost ──────────────────────────────────────
    var footer = new VisualElement();
    footer.AddToClassList("card-footer");

    if (card != null)
    {
        var apLabel = new Label($"AP: {card.apCost}");
        apLabel.AddToClassList("card-ap-cost");
        footer.Add(apLabel);

        if (card.apRefund > 0)
        {
            var refundLabel = new Label($"(refund {card.apRefund})");
            refundLabel.AddToClassList("card-refund");
            footer.Add(refundLabel);
        }
    }
    el.Add(footer);

    // ── Playability ──────────────────────────────────────────
    bool canPlay = card == null || hunter.apRemaining >= (card.apCost - card.apRefund);
    el.EnableInClassList("card--unplayable", !canPlay);

    // ── Click handler ─────────────────────────────────────────
    string capturedName = cardName; // Capture for lambda
    el.RegisterCallback<ClickEvent>(_ => OnCardClicked(capturedName, el, canPlay));

    return el;
}

private void OnCardClicked(string cardName, VisualElement cardEl, bool canPlay)
{
    if (!canPlay)
    {
        Debug.Log($"[CombatUI] Card unplayable: {cardName} (insufficient AP)");
        return;
    }

    // Deselect previous selection
    if (_selectedCardEl != null)
        _selectedCardEl.EnableInClassList("card--selected", false);

    if (_pendingCardName == cardName)
    {
        // Clicking same card again cancels selection
        _pendingCardName = null;
        _selectedCardEl  = null;
        Debug.Log($"[CombatUI] Card deselected: {cardName}");
        return;
    }

    // Select this card
    _pendingCardName = cardName;
    _selectedCardEl  = cardEl;
    cardEl.EnableInClassList("card--selected", true);

    Debug.Log($"[CombatUI] Card selected: {cardName} — click a grid cell to target");
    // Grid cells will now show valid target highlights — implemented Session 5-E
}
```

### Monster Panel (replace the stub)

```csharp
private void RefreshMonsterPanel(MonsterCombatState monster)
{
    if (_monsterPanel == null) return;

    // Header labels
    var nameLabel = _monsterPanel.Q<Label>("monster-name");
    if (nameLabel != null) nameLabel.text = monster.monsterName;

    var diffLabel = _monsterPanel.Q<Label>("monster-difficulty");
    if (diffLabel != null) diffLabel.text = monster.difficulty;

    // Deck count — removable cards remaining
    var deckLabel = _monsterPanel.Q<Label>("monster-deck-count");
    if (deckLabel != null)
    {
        int removable = monster.activeDeckCardNames?.Length ?? 0;
        deckLabel.text = $"Removable: {removable}";
    }

    // Stance
    var stanceLabel = _monsterPanel.Q<Label>("monster-stance");
    if (stanceLabel != null)
        stanceLabel.text = string.IsNullOrEmpty(monster.currentStanceTag)
            ? "" : $"Stance: {monster.currentStanceTag}";

    // Parts
    var partsContainer = _monsterPanel.Q<VisualElement>("monster-parts-container");
    if (partsContainer == null) return;
    partsContainer.Clear();

    if (monster.parts == null) return;
    foreach (var part in monster.parts)
        partsContainer.Add(BuildMonsterPartElement(part));
}

private VisualElement BuildMonsterPartElement(MonsterPartState part)
{
    var row = new VisualElement();
    row.AddToClassList("monster-part-row");
    if (part.isBroken)  row.AddToClassList("monster-part-row--broken");

    // Part name — hidden if trap zone not yet revealed
    var nameLabel = new Label(part.isRevealed ? part.partName : "???");
    nameLabel.AddToClassList("part-name");
    row.Add(nameLabel);

    // Bars
    var bars = new VisualElement();
    bars.AddToClassList("part-bars");

    // Shell bar
    var shellTrack = new VisualElement();
    shellTrack.AddToClassList("shell-bar-track");
    var shellFill = new VisualElement();
    shellFill.AddToClassList("shell-bar-fill");
    float shellPct = part.shellMax > 0 ? (float)part.shellCurrent / part.shellMax : 0f;
    shellFill.style.width = Length.Percent(shellPct * 100f);
    shellTrack.Add(shellFill);
    bars.Add(shellTrack);

    // Flesh bar
    var fleshTrack = new VisualElement();
    fleshTrack.AddToClassList("flesh-bar-track");
    var fleshFill = new VisualElement();
    fleshFill.AddToClassList("flesh-bar-fill");
    float fleshPct = part.fleshMax > 0 ? (float)part.fleshCurrent / part.fleshMax : 0f;
    fleshFill.style.width = Length.Percent(fleshPct * 100f);
    fleshTrack.Add(fleshFill);
    bars.Add(fleshTrack);

    row.Add(bars);

    // Tags
    if (part.isExposed)
    {
        var exposedTag = new Label("EXPOSED");
        exposedTag.AddToClassList("exposed-tag");
        row.Add(exposedTag);
    }

    if (part.isBroken && part.isRevealed)
    {
        var brokenTag = new Label("BROKEN");
        brokenTag.AddToClassList("status-badge");
        row.Add(brokenTag);
    }

    return row;
}
```

---

## Verification Test

Play the combat scene:

- [ ] Card hand shows cards from mock hunter's hand (Brace, Shove)
- [ ] Clicking Brace highlights it with gold border (card--selected)
- [ ] Clicking Brace again deselects it
- [ ] AP display in hand action panel shows "AP: 2"
- [ ] Monster panel shows "The Gaunt" as the name
- [ ] Monster panel shows "Standard" as difficulty
- [ ] All 7 Gaunt parts render with Shell/Flesh bar tracks (may be at full width since mock data starts at full health)
- [ ] Trap zone parts show "???" if `isRevealed` is false
- [ ] Broken parts show dimmed background
- [ ] Exposed parts show EXPOSED tag

---

## Next Session

**File:** `_Docs/Stage_05/STAGE_05_E.md`  
**Covers:** Grid rendering, card→grid cell click resolution, End Turn button, and the Stage 5 final playthrough verification
