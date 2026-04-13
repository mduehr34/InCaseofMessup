<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 6-H | Combat Return, Victory/Defeat Modals & Stage 6 Final
Status: Stage 6-G complete. Codex all three tabs verified.
Task: Wire CombatManager.OnCombatEnded to show a
victory/defeat modal and then call GameStateManager.
ReturnFromHunt() with the correct HuntResult. Confirm the
full year loop works end-to-end: Settlement → Hunt → Travel
→ Combat → Return → Settlement.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_06/STAGE_06_H.md
- Assets/_Game/Scripts/Core.UI/CombatScreenController.cs
- Assets/_Game/Scripts/Core.Systems/GameStateManager.cs
- Assets/_Game/Scripts/Core.Systems/CombatManager.cs
- Assets/_Game/Scripts/Core.Data/CombatState.cs
- Assets/_Game/UI/USS/settlement-shared.uss

Then confirm:
- That you will ADD to CombatScreenController.cs —
  specifically completing the OnCombatEnded() stub
- That HuntResult is built from CombatState data
- That ReturnFromHunt() auto-saves before loading Settlement
- What you will NOT touch this session

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 6-H: Combat Return, Victory/Defeat Modals & Stage 6 Final

**Resuming from:** Stage 6-G complete — Codex screen verified  
**Done when:** Combat ends → modal shows victory/defeat → player dismisses → Settlement loads with pending hunt result applied; full year loop verified end-to-end  
**Commit:** `"6H: Combat return flow, victory/defeat modals — Stage 6 complete"`  
**Next session:** STAGE_07_A.md (Stage 7 begins)  

---

## Step 1: result-modal.uxml

**Path:** `Assets/_Game/UI/UXML/result-modal.uxml`

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements" editor-extension-mode="False">
    <Style src="../USS/tokens.uss"/>
    <Style src="../USS/stone-panel.uss"/>
    <Style src="../USS/settlement-shared.uss"/>

    <ui:VisualElement name="result-overlay" class="modal-overlay">
        <ui:VisualElement name="result-modal" class="modal-panel stone-panel">

            <ui:Label name="result-title"    text="HUNT COMPLETE"  class="stone-panel__header"/>
            <ui:Label name="result-outcome"  text="VICTORY"        class="result-outcome-label"/>
            <ui:Label name="result-rounds"   text="8 rounds"       class="proficiency-label"/>
            <ui:Label name="result-monster"  text="The Gaunt"      class="proficiency-label"/>

            <ui:Label text="LOOT GAINED" class="proficiency-label" style="margin-top:12px;"/>
            <ui:VisualElement name="loot-list" class="tab-content" style="max-height:160px;"/>

            <ui:Label text="HUNTERS" class="proficiency-label" style="margin-top:8px;"/>
            <ui:VisualElement name="hunter-results" class="tab-content" style="max-height:120px;"/>

            <ui:Button name="btn-return"
                       text="RETURN TO SETTLEMENT"
                       class="action-btn action-btn--primary"
                       style="margin-top:16px;"/>

        </ui:VisualElement>
    </ui:VisualElement>
</ui:UXML>
```

Add to `settlement-shared.uss`:

```css
.result-outcome-label {
    font-size:        36px;
    -unity-font-style: bold;
    margin-bottom:    var(--spacing-sm);
}
```

---

## Step 2: Complete OnCombatEnded() in CombatScreenController.cs

Add these fields and update the stub:

```csharp
[SerializeField] private VisualTreeAsset _resultModalAsset;
```

Replace the `OnCombatEnded` stub:

```csharp
private void OnCombatEnded(CombatResult result)
{
    Debug.Log($"[CombatUI] Combat ended — Victory:{result.isVictory}");

    if (_resultModalAsset == null)
    {
        Debug.LogWarning("[CombatUI] Result modal asset not assigned — returning directly");
        ReturnToSettlement(result);
        return;
    }

    ShowResultModal(result);
}

private void ShowResultModal(CombatResult result)
{
    var overlay = _resultModalAsset.Instantiate();
    _root.Add(overlay);

    var state = _combatManager.CurrentState;

    // Title and outcome
    overlay.Q<Label>("result-title").text   = result.isVictory ? "HUNT COMPLETE" : "HUNT FAILED";
    var outcomeLabel = overlay.Q<Label>("result-outcome");
    outcomeLabel.text  = result.isVictory ? "VICTORY" : "DEFEAT";
    outcomeLabel.style.color = result.isVictory
        ? new UnityEngine.UIElements.StyleColor(new Color(0.40f, 0.80f, 0.40f))
        : new UnityEngine.UIElements.StyleColor(new Color(0.80f, 0.25f, 0.25f));

    overlay.Q<Label>("result-rounds").text  = $"{result.roundsElapsed} rounds fought";
    overlay.Q<Label>("result-monster").text = state.monster.monsterName;

    // Loot (victory only)
    var lootList = overlay.Q<VisualElement>("loot-list");
    if (result.isVictory)
    {
        // Build loot from removed behavior cards (loot table not yet resolved here — stub)
        var stub = new Label("Loot resolved in Settlement phase");
        stub.AddToClassList("proficiency-label");
        lootList.Add(stub);
    }
    else
    {
        var noLoot = new Label("No loot — hunt failed");
        noLoot.AddToClassList("injury-indicator");
        lootList.Add(noLoot);
    }

    // Hunter summary
    var hunterResults = overlay.Q<VisualElement>("hunter-results");
    foreach (var hunter in state.hunters)
    {
        var row = new Label(hunter.isCollapsed
            ? $"⚑ {hunter.hunterName} — COLLAPSED"
            : $"✓ {hunter.hunterName} — survived");
        row.AddToClassList(hunter.isCollapsed ? "injury-indicator" : "proficiency-label");
        hunterResults.Add(row);
    }

    // Return button
    overlay.Q<Button>("btn-return").clicked += () =>
    {
        _root.Remove(overlay);
        ReturnToSettlement(result);
    };
}

private void ReturnToSettlement(CombatResult result)
{
    // Build HuntResult from combat state
    var state = _combatManager.CurrentState;

    var huntResult = new HuntResult
    {
        isVictory          = result.isVictory,
        monsterName        = state.monster.monsterName,
        monsterDifficulty  = state.monster.difficulty,
        roundsFought       = result.roundsElapsed,
        collapsedHunterIds = result.collapsedHunterIds ?? new string[0],
        survivingHunterIds = System.Array.FindAll(
            state.hunters, h => !h.isCollapsed)
            .Select(h => h.hunterId).ToArray(),
        // Loot: resolved by SettlementManager from LootTable in Stage 7
        // For now: empty — settlement will check pendingHuntResult
        lootGained              = new ResourceEntry[0],
        injuryCardNamesApplied  = new string[state.hunters.Length],
    };

    GameStateManager.Instance.ReturnFromHunt(huntResult);
}
```

---

## Stage 6 Final — Full Year Loop Verification

Work through this checklist manually as a complete playthrough:

**Year 1 — Full Loop:**

```
1. Launch game → MainMenu scene
2. Click NEW CAMPAIGN → Campaign Select
3. Select Mock_TutorialCampaign → click BEGIN
4. Settlement loads — Year 1, The Ember
5. Characters tab shows 8 characters (Medium difficulty)
6. Chronicle event modal fires automatically (if eligible events exist)
7. Resolve event (acknowledge or make choice)
8. Switch to Innovations tab → draw 3 → adopt 1
9. Click SEND HUNTING PARTY
10. Hunt selection modal opens
11. Select The Gaunt, Standard, first 4 hunters → click HUNT
12. Travel scene loads — "Hunting: The Gaunt (Standard)"
13. Travel events resolve (if any tagged "travel")
14. Click CONTINUE TO HUNT
15. CombatScene loads
16. Play at least 1 round manually (Vitality → card → End Turn → Monster Phase)
17. Phase label updates correctly
18. End combat (either win or lose — or call mock result)
19. Result modal appears — Victory or Defeat shown
20. Click RETURN TO SETTLEMENT
21. Settlement loads — auto-save fired
22. Characters tab shows same hunters (hunt counts +1)
23. Click END YEAR → Year 2 begins
24. Era name may still be "The Ember" (Year 2 is still 1–3)
25. Click CODEX → Codex loads additively
26. Settlement Records tab shows chronicle entries from Year 1
27. The Gaunt appears in Monsters tab (now known from hunt)
28. Close Codex → back to Settlement

LOOP VERIFIED.
```

**Definition of Done — Stage 6:**

- [ ] Full year loop completable: Settlement → Hunt → Travel → Combat → Return → Settlement
- [ ] Chronicle Events display with choice A/B and mandatory variants
- [ ] Guiding Principal modal triggers and records choice permanently
- [ ] Innovations tab draws 3, adopts correctly with cascade
- [ ] Crafters tab shows built/available states with unlock button
- [ ] Gear grid shows equipped items, stats summary, link count
- [ ] Hunt selection modal — monster, difficulty, hunter picker all work
- [ ] Travel scene resolves events in sequence
- [ ] Combat result modal shows victory/defeat with hunter summaries
- [ ] Codex: locked monsters show "???", unlocked show detail
- [ ] Settlement Records tab shows full chronicle log
- [ ] Auto-save fires on return from hunt and on year advance
- [ ] No Canvas or uGUI components in any scene

---

## Stage 6 Complete — What You Now Have

- `GameStateManager` singleton persisting across all scene loads
- 6 scenes wired in Build Settings: MainMenu, CampaignSelect, Settlement, GearGrid, Travel, CombatScene, Codex
- Complete screen chain: Main Menu → Campaign Select → Settlement → Hunt Select → Travel → Combat → Return → Settlement
- All modals: Chronicle Event, Guiding Principal, Hunt Selection, Combat Result
- Settlement screen: Era header, Characters tab, Crafters tab, Innovations tab
- Gear Grid: 4×4 layout, click equip, link resolver, stats summary
- Travel Phase: 3 event draw, sequential resolution, pre-combat condition bar
- Codex: 3 tabs, settler voice, locked/unlocked states
- Proper auto-save on return from hunt and year advance

---

## Next Session

**File:** `_Docs/Stage_07/STAGE_07_A.md`  
**First task of Stage 7:** All Gaunt behavior cards as SO assets — the canonical content template all other monsters follow
