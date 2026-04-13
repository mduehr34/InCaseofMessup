<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 6-F | Hunt Selection Modal & Travel Phase Screen
Status: Stage 6-E complete. Gear Grid screen working.
Item equip, link resolver, stats summary all verified.
Task: Build the Hunt Selection modal (monster, difficulty,
hunter picker) wired to GameStateManager.PrepareHunt().
Build travel-screen.uxml and TravelScreenController.cs —
draws 3 travel events, resolves them in sequence, then
loads CombatScene.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_06/STAGE_06_F.md
- Assets/_Game/Scripts/Core.Systems/GameStateManager.cs
- Assets/_Game/Scripts/Core.Systems/SettlementManager.cs
- Assets/_Game/Scripts/Core.Data/CampaignState.cs
- Assets/_Game/Scripts/Core.Data/MonsterSO.cs
- Assets/_Game/Scripts/Core.Data/EventSO.cs
- Assets/_Game/UI/USS/settlement-shared.uss

Then confirm:
- That Hunt Selection is a modal overlay on Settlement
- That Travel screen draws events tagged "travel" from
  the campaign event pool
- That TravelScreenController applies pre-combat
  status effects before loading CombatScene
- What you will NOT touch this session

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 6-F: Hunt Selection Modal & Travel Phase Screen

**Resuming from:** Stage 6-E complete — Gear Grid verified  
**Done when:** Clicking SEND HUNTING PARTY shows hunt selection modal; selecting monster/difficulty/hunters and confirming loads Travel scene; Travel scene resolves 3 events in sequence; Continue to Hunt button loads CombatScene  
**Commit:** `"6F: Hunt selection modal, travel phase screen, pre-combat event flow"`  
**Next session:** STAGE_06_G.md  

---

## Step 1: Hunt Selection Modal

Add a hunt-selection overlay UXML template. This is shown as a modal over the Settlement screen (same pattern as Chronicle Event modal).

**Path:** `Assets/_Game/UI/UXML/hunt-select-modal.uxml`

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements" editor-extension-mode="False">
    <Style src="../USS/tokens.uss"/>
    <Style src="../USS/stone-panel.uss"/>
    <Style src="../USS/settlement-shared.uss"/>

    <ui:VisualElement name="hunt-overlay" class="modal-overlay">
        <ui:VisualElement name="hunt-modal" class="modal-panel stone-panel">

            <ui:Label text="SEND HUNTING PARTY" class="stone-panel__header"/>

            <!-- Monster Selection -->
            <ui:Label text="TARGET" class="proficiency-label"/>
            <ui:VisualElement name="monster-list" class="tab-content" style="max-height:180px; overflow:hidden;"/>

            <!-- Difficulty -->
            <ui:Label text="DIFFICULTY" class="proficiency-label" style="margin-top:8px;"/>
            <ui:VisualElement name="difficulty-row" class="event-choices">
                <ui:Button name="btn-diff-standard" text="STANDARD" class="action-btn tab-btn--active"/>
                <ui:Button name="btn-diff-hardened" text="HARDENED" class="action-btn"/>
                <ui:Button name="btn-diff-apex"     text="APEX"     class="action-btn"/>
            </ui:VisualElement>

            <!-- Hunter Selection (up to 4) -->
            <ui:Label text="HUNTERS (select 4)" class="proficiency-label" style="margin-top:8px;"/>
            <ui:VisualElement name="hunter-select-list" class="tab-content" style="max-height:200px;"/>

            <!-- Confirm / Cancel -->
            <ui:VisualElement class="event-choices" style="margin-top:12px;">
                <ui:Button name="btn-cancel-hunt" text="CANCEL"   class="action-btn"/>
                <ui:Button name="btn-confirm-hunt" text="HUNT"    class="action-btn action-btn--primary"/>
            </ui:VisualElement>

        </ui:VisualElement>
    </ui:VisualElement>
</ui:UXML>
```

### HuntSelectionModal.cs

**Path:** `Assets/_Game/Scripts/Core.UI/HuntSelectionModal.cs`

```csharp
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using MnM.Core.Systems;
using MnM.Core.Data;

namespace MnM.Core.UI
{
    public class HuntSelectionModal
    {
        private VisualElement    _overlay;
        private CampaignSO       _campaignSO;
        private MonsterSO        _selectedMonster;
        private string           _selectedDifficulty = "Standard";
        private List<string>     _selectedHunterIds  = new List<string>();

        public void Show(VisualElement root, VisualTreeAsset modalAsset, CampaignSO campaignSO)
        {
            _campaignSO = campaignSO;
            _overlay    = modalAsset.Instantiate();
            root.Add(_overlay);

            BuildMonsterList();
            BuildHunterList();
            WireDifficultyButtons();

            _overlay.Q<Button>("btn-cancel-hunt").clicked  += () => root.Remove(_overlay);
            _overlay.Q<Button>("btn-confirm-hunt").clicked += OnConfirm;
        }

        private void BuildMonsterList()
        {
            var container = _overlay.Q<VisualElement>("monster-list");
            if (container == null || _campaignSO.monsterRoster == null) return;
            container.Clear();

            foreach (var monster in _campaignSO.monsterRoster)
            {
                if (monster == null) continue;
                var row  = new VisualElement();
                row.AddToClassList("character-row");
                row.AddToClassList("stone-panel");

                var label = new Label(monster.monsterName);
                label.AddToClassList("character-name");
                row.Add(label);

                var tierLabel = new Label($"Tier {monster.materialTier}");
                tierLabel.AddToClassList("proficiency-label");
                row.Add(tierLabel);

                var monsterRef = monster;
                row.RegisterCallback<ClickEvent>(_ =>
                {
                    _selectedMonster = monsterRef;
                    RefreshMonsterSelection(container, monsterRef.monsterName);
                    Debug.Log($"[HuntSelect] Monster selected: {monsterRef.monsterName}");
                });
                container.Add(row);
            }

            // Auto-select first
            if (_campaignSO.monsterRoster.Length > 0)
                _selectedMonster = _campaignSO.monsterRoster[0];
        }

        private void RefreshMonsterSelection(VisualElement container, string selectedName)
        {
            foreach (var child in container.Children())
            {
                bool isSelected = child.Q<Label>()?.text == selectedName;
                child.EnableInClassList("stone-panel--active", isSelected);
            }
        }

        private void BuildHunterList()
        {
            var container = _overlay.Q<VisualElement>("hunter-select-list");
            if (container == null) return;
            container.Clear();

            var state = GameStateManager.Instance.CampaignState;
            foreach (var ch in state.characters.Where(c => !c.isRetired))
            {
                var row = new VisualElement();
                row.AddToClassList("character-row");
                row.AddToClassList("stone-panel");

                var nameLabel = new Label(ch.characterName);
                nameLabel.AddToClassList("character-name");
                row.Add(nameLabel);

                var profLabel = new Label($"T{ch.proficiencyTiers[0]} {ch.proficiencyWeaponTypes[0]}");
                profLabel.AddToClassList("proficiency-label");
                row.Add(profLabel);

                string capturedId = ch.characterId;
                row.RegisterCallback<ClickEvent>(_ => ToggleHunter(capturedId, row));
                container.Add(row);
            }

            // Auto-select first 4
            var first4 = state.characters.Where(c => !c.isRetired).Take(4);
            foreach (var ch in first4) _selectedHunterIds.Add(ch.characterId);
            RefreshHunterSelection(container);
        }

        private void ToggleHunter(string hunterId, VisualElement row)
        {
            if (_selectedHunterIds.Contains(hunterId))
            {
                _selectedHunterIds.Remove(hunterId);
                row.EnableInClassList("stone-panel--active", false);
            }
            else if (_selectedHunterIds.Count < 4)
            {
                _selectedHunterIds.Add(hunterId);
                row.EnableInClassList("stone-panel--active", true);
            }
            else
            {
                Debug.Log("[HuntSelect] Max 4 hunters");
            }
        }

        private void RefreshHunterSelection(VisualElement container)
        {
            foreach (var child in container.Children())
            {
                var label  = child.Q<Label>();
                var state  = GameStateManager.Instance.CampaignState;
                var ch     = System.Array.Find(state.characters, c => c.characterName == label?.text);
                bool sel   = ch != null && _selectedHunterIds.Contains(ch.characterId);
                child.EnableInClassList("stone-panel--active", sel);
            }
        }

        private void WireDifficultyButtons()
        {
            foreach (var (btnName, diff) in new[] {
                ("btn-diff-standard", "Standard"),
                ("btn-diff-hardened", "Hardened"),
                ("btn-diff-apex",     "Apex") })
            {
                string capturedDiff = diff;
                _overlay.Q<Button>(btnName).clicked += () =>
                {
                    _selectedDifficulty = capturedDiff;
                    foreach (var b in new[] { "btn-diff-standard", "btn-diff-hardened", "btn-diff-apex" })
                        _overlay.Q<Button>(b)?.EnableInClassList("tab-btn--active", b == btnName);
                };
            }
        }

        private void OnConfirm()
        {
            if (_selectedMonster == null) { Debug.LogWarning("[HuntSelect] No monster"); return; }
            if (_selectedHunterIds.Count == 0) { Debug.LogWarning("[HuntSelect] No hunters"); return; }

            var state   = GameStateManager.Instance.CampaignState;
            var hunters = state.characters
                .Where(c => _selectedHunterIds.Contains(c.characterId))
                .ToArray();

            GameStateManager.Instance.PrepareHunt(_selectedMonster, _selectedDifficulty, hunters);
        }
    }
}
```

Update `SettlementScreenController.OnHuntClicked()`:

```csharp
[SerializeField] private VisualTreeAsset _huntSelectModalAsset;
private HuntSelectionModal _huntModal = new HuntSelectionModal();

private void OnHuntClicked()
{
    _huntModal.Show(_root, _huntSelectModalAsset, _campaignDataSO);
}
```

---

## Step 2: travel-screen.uxml

**Path:** `Assets/_Game/UI/UXML/travel-screen.uxml`

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements" editor-extension-mode="False">
    <Style src="../USS/tokens.uss"/>
    <Style src="../USS/stone-panel.uss"/>
    <Style src="../USS/settlement-shared.uss"/>

    <ui:VisualElement name="travel-root" class="fullscreen-bg">

        <!-- Header -->
        <ui:VisualElement class="era-bar stone-panel--raised">
            <ui:Label name="hunt-target-label" text="Hunting: ---"     class="era-year"/>
            <ui:VisualElement style="flex:1"/>
            <ui:Label name="events-remaining"  text="3 events remain"  class="proficiency-label"/>
        </ui:VisualElement>

        <!-- Path Visual -->
        <ui:VisualElement name="path-visual" class="path-visual stone-panel">
            <ui:Label text="TRAVELLING..." class="settlement-scene-placeholder"/>
        </ui:VisualElement>

        <!-- Hunter Condition Bar -->
        <ui:VisualElement name="hunter-condition-bar" class="hunter-condition-bar stone-panel--raised">
            <!-- Hunter status strips — built dynamically -->
        </ui:VisualElement>

        <!-- Event Modal Area (overlay) -->
        <!-- Reuses event-modal.uxml via VisualTreeAsset.Instantiate() -->

        <!-- Continue Button -->
        <ui:VisualElement class="action-bar stone-panel--raised">
            <ui:Button name="btn-continue-hunt" text="CONTINUE TO HUNT"
                       class="action-btn action-btn--primary" style="display:none;"/>
        </ui:VisualElement>

    </ui:VisualElement>
</ui:UXML>
```

Add to `settlement-shared.uss`:

```css
.path-visual {
    flex: 1;
    align-items: center;
    justify-content: center;
    margin: 2px;
}

.hunter-condition-bar {
    height:         100px;
    flex-direction: row;
    align-items:    center;
    padding:        var(--spacing-sm);
    flex-shrink:    0;
}
```

---

## Step 3: TravelScreenController.cs

**Path:** `Assets/_Game/Scripts/Core.UI/TravelScreenController.cs`

```csharp
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using MnM.Core.Systems;
using MnM.Core.Data;

namespace MnM.Core.UI
{
    public class TravelScreenController : MonoBehaviour
    {
        [SerializeField] private UIDocument       _uiDocument;
        [SerializeField] private VisualTreeAsset  _eventModalAsset;
        [SerializeField] private CampaignSO       _campaignSO;

        private VisualElement _root;
        private Queue<EventSO> _travelEvents = new Queue<EventSO>();
        private int _totalEvents;

        private void OnEnable()
        {
            _root = _uiDocument.rootVisualElement;

            var gsm = GameStateManager.Instance;
            if (gsm?.SelectedMonster == null)
            {
                Debug.LogError("[Travel] No hunt prepared in GameStateManager");
                return;
            }

            // Header
            _root.Q<Label>("hunt-target-label").text =
                $"Hunting: {gsm.SelectedMonster.monsterName} ({gsm.SelectedDifficulty})";

            // Build hunter condition bar
            BuildHunterConditionBar(gsm.CampaignState, gsm.SelectedHunters);

            // Draw travel events
            DrawTravelEvents(gsm.CampaignState);

            // Wire Continue button
            _root.Q<Button>("btn-continue-hunt").clicked += OnContinueToHunt;

            // Show first event (or go straight to hunt if none)
            ShowNextEvent();
        }

        private void DrawTravelEvents(CampaignState state)
        {
            _travelEvents.Clear();

            if (_campaignSO?.eventPool == null) return;

            // Travel events: tagged "travel" or seasonTag = "travel"
            // Year 1 rule: only non-damaging events (no injury effects)
            var eligible = _campaignSO.eventPool
                .Where(e => e != null &&
                    !state.resolvedEventIds.Contains(e.eventId) &&
                    (e.campaignTag == "travel" || e.seasonTag == "travel") &&
                    state.currentYear >= e.yearRangeMin &&
                    state.currentYear <= e.yearRangeMax)
                .OrderBy(_ => Random.value)
                .Take(3);

            foreach (var evt in eligible)
                _travelEvents.Enqueue(evt);

            _totalEvents = _travelEvents.Count;
            UpdateEventsRemaining();

            Debug.Log($"[Travel] {_totalEvents} travel events queued");
        }

        private void ShowNextEvent()
        {
            UpdateEventsRemaining();

            if (_travelEvents.Count == 0)
            {
                // All events resolved — show Continue button
                _root.Q<Button>("btn-continue-hunt").style.display = DisplayStyle.Flex;
                Debug.Log("[Travel] All travel events resolved — Continue button shown");
                return;
            }

            var evt = _travelEvents.Dequeue();
            ShowEventModal(evt);
        }

        private void ShowEventModal(EventSO evt)
        {
            if (_eventModalAsset == null) { ShowNextEvent(); return; }

            var overlay = _eventModalAsset.Instantiate();
            _root.Add(overlay);

            overlay.Q<Label>("event-id").text        = evt.eventId;
            overlay.Q<Label>("event-name").text      = evt.eventName;
            overlay.Q<Label>("event-narrative").text = evt.narrativeText;

            bool isMandatory = evt.isMandatory || evt.choices == null || evt.choices.Length == 0;
            overlay.Q<Label>("event-mandatory").style.display =
                isMandatory ? DisplayStyle.Flex : DisplayStyle.None;

            var choicesEl = overlay.Q<VisualElement>("event-choices");
            var ackBtn    = overlay.Q<Button>("btn-acknowledge");

            if (isMandatory)
            {
                choicesEl.style.display = DisplayStyle.None;
                ackBtn.style.display    = DisplayStyle.Flex;
                ackBtn.clicked += () => { _root.Remove(overlay); ShowNextEvent(); };
            }
            else
            {
                choicesEl.style.display = DisplayStyle.Flex;
                ackBtn.style.display    = DisplayStyle.None;

                void Resolve(int choiceIdx)
                {
                    ApplyTravelEventEffect(evt, choiceIdx);
                    _root.Remove(overlay);
                    ShowNextEvent();
                }

                var btnA = overlay.Q<Button>("btn-choice-a");
                var btnB = overlay.Q<Button>("btn-choice-b");
                if (evt.choices.Length > 0) { btnA.text = $"A: {evt.choices[0].outcomeText}"; btnA.clicked += () => Resolve(0); }
                if (evt.choices.Length > 1) { btnB.text = $"B: {evt.choices[1].outcomeText}"; btnB.clicked += () => Resolve(1); }
                else btnB.style.display = DisplayStyle.None;
            }
        }

        private void ApplyTravelEventEffect(EventSO evt, int choiceIndex)
        {
            // Mark resolved in campaign state
            var state    = GameStateManager.Instance.CampaignState;
            var resolved = new List<string>(state.resolvedEventIds) { evt.eventId };
            state.resolvedEventIds = resolved.ToArray();

            // Mechanical effects: per .cursorrules, stop and ask if complex
            // Simple stat/injury effects logged for manual review
            if (evt.choices != null && choiceIndex < evt.choices.Length)
                Debug.Log($"[Travel] Event effect (apply manually if needed): " +
                          $"{evt.choices[choiceIndex].mechanicalEffect}");
        }

        private void UpdateEventsRemaining()
        {
            int remaining = _travelEvents.Count;
            _root.Q<Label>("events-remaining").text =
                remaining == 0 ? "All events resolved" : $"{remaining} event{(remaining == 1 ? "" : "s")} remain";
        }

        private void BuildHunterConditionBar(CampaignState state, RuntimeCharacterState[] hunters)
        {
            var bar = _root.Q<VisualElement>("hunter-condition-bar");
            if (bar == null || hunters == null) return;
            bar.Clear();

            foreach (var h in hunters)
            {
                var strip = new VisualElement();
                strip.AddToClassList("character-row");
                strip.AddToClassList("stone-panel");
                strip.style.flex = 1;

                var name = new Label(h.characterName);
                name.AddToClassList("character-name");
                strip.Add(name);

                if (h.injuryCardNames.Length > 0)
                {
                    var inj = new Label($"⚑ {h.injuryCardNames.Length}");
                    inj.AddToClassList("injury-indicator");
                    strip.Add(inj);
                }

                bar.Add(strip);
            }
        }

        private void OnContinueToHunt()
        {
            Debug.Log("[Travel] Continuing to combat");
            GameStateManager.Instance.BeginCombat();
        }
    }
}
```

---

## Verification Test

1. From Settlement → SEND HUNTING PARTY
2. Hunt selection modal appears over settlement screen
3. Monster list populates from CampaignSO.monsterRoster
4. Difficulty buttons toggle (Standard/Hardened/Apex)
5. Hunter list shows all active characters; first 4 pre-selected
6. Click HUNT → Travel scene loads
7. Hunt target label shows "Hunting: The Gaunt (Standard)"
8. Travel events display (if any tagged "travel" in CampaignSO)
9. After all events resolved → "CONTINUE TO HUNT" button appears
10. Click Continue → CombatScene loads

---

## Next Session

**File:** `_Docs/Stage_06/STAGE_06_G.md`  
**Covers:** Codex screen — all three tabs (Monsters, Artifacts, Settlement Records)
