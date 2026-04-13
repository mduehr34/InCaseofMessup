<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 5-C | CombatScreenController — Hunter Panels & Phase Label
Status: Stage 5-B complete. Combat screen UXML verified in
UI Builder. Zero USS errors in Console.
Task: Create CombatScreenController.cs. Wire CombatManager
events to UI. Implement: phase label updates, round label,
all 4 hunter panels (name, Shell/Flesh bars, aggro indicator,
status badges, AP label, Grit pips, collapsed state).
Do NOT implement card hand or grid rendering yet — those
are Sessions 5-D and 5-E.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_05/STAGE_05_C.md
- Assets/_Game/Scripts/Core.Systems/ICombatManager.cs
- Assets/_Game/Scripts/Core.Systems/CombatManager.cs
- Assets/_Game/Scripts/Core.Data/CombatState.cs
- Assets/_Game/Scripts/Core.Data/Enums.cs
- Assets/_Game/UI/UXML/combat-screen.uxml
- Assets/_Game/UI/USS/stone-panel.uss

Then confirm:
- The single file you will create
- That no uGUI components are used anywhere
- That card hand and grid are stub methods only
- What you will NOT touch this session

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 5-C: CombatScreenController — Hunter Panels & Phase Label

**Resuming from:** Stage 5-B complete — UXML layout verified in UI Builder  
**Done when:** Running the game shows all 4 hunter panels populated with mock data; phase label changes on `AdvancePhase()`; aggro indicator visible on correct hunter; Shell/Flesh bars reflect correct percentages  
**Commit:** `"5C: CombatScreenController — hunter panels, phase label, aggro, status badges"`  
**Next session:** STAGE_05_D.md  

---

## Setup: Combat Scene

Before writing the controller, create a Unity scene:

1. Create `Assets/_Game/Scenes/CombatScene.unity`
2. Add a `GameObject` named `CombatUI`
3. Add a `UIDocument` component to it — assign `combat-screen.uxml` as the Source Asset
4. Add a `PanelSettings` asset (`Assets/_Game/UI/CombatPanelSettings.asset`) — assign it to the UIDocument
5. Add a `GameObject` named `CombatManager` — add the `CombatManager` MonoBehaviour
6. Add a `GameObject` named `GridManager` — add the `GridManager` MonoBehaviour

The `CombatScreenController` will be added to `CombatUI` in this session.

---

## CombatScreenController.cs

**Path:** `Assets/_Game/Scripts/Core.UI/CombatScreenController.cs`

```csharp
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using MnM.Core.Systems;
using MnM.Core.Data;

namespace MnM.Core.UI
{
    public class CombatScreenController : MonoBehaviour
    {
        // ── Inspector References ─────────────────────────────────
        [SerializeField] private UIDocument    _uiDocument;
        [SerializeField] private CombatManager _combatManager;

        // ── Cached Root Elements ─────────────────────────────────
        private VisualElement _root;
        private Label         _phaseLabel;
        private Label         _roundLabel;

        // Hunter panels — indexed 0–3
        private VisualElement[] _hunterPanels    = new VisualElement[4];
        private Label[]         _hunterNames     = new Label[4];
        private Label[]         _aggroIndicators = new Label[4];
        private VisualElement[] _bodyZoneContainers = new VisualElement[4];
        private VisualElement[] _statusEffectRows   = new VisualElement[4];
        private VisualElement[] _activeInfoPanels   = new VisualElement[4];
        private Label[]         _apLabels        = new Label[4];
        private VisualElement[] _gritPipRows     = new VisualElement[4];

        // Monster panel
        private VisualElement _monsterPanel;

        // Card hand — stub until Session 5-D
        private VisualElement _handCards;

        // ── Lifecycle ────────────────────────────────────────────
        private void OnEnable()
        {
            if (_uiDocument == null)
            {
                Debug.LogError("[CombatUI] UIDocument not assigned");
                return;
            }

            _root = _uiDocument.rootVisualElement;
            CacheElements();
            WireEvents();
            Debug.Log("[CombatUI] Controller enabled — elements cached and events wired");
        }

        private void OnDisable()
        {
            UnwireEvents();
        }

        private void Start()
        {
            // If CombatManager has an active state, do an initial refresh
            if (_combatManager?.CurrentState != null)
                RefreshAll();
        }

        // ── Element Caching ──────────────────────────────────────
        private void CacheElements()
        {
            _phaseLabel = _root.Q<Label>("phase-label");
            _roundLabel = _root.Q<Label>("round-label");
            _handCards  = _root.Q<VisualElement>("hand-cards");
            _monsterPanel = _root.Q<VisualElement>("monster-panel");

            for (int i = 0; i < 4; i++)
            {
                _hunterPanels[i]        = _root.Q<VisualElement>($"hunter-panel-{i}");
                _hunterNames[i]         = _root.Q<Label>($"hunter-name-{i}");
                _aggroIndicators[i]     = _root.Q<Label>($"aggro-{i}");
                _bodyZoneContainers[i]  = _root.Q<VisualElement>($"body-zones-{i}");
                _statusEffectRows[i]    = _root.Q<VisualElement>($"status-effects-{i}");
                _activeInfoPanels[i]    = _root.Q<VisualElement>($"active-info-{i}");
                _apLabels[i]            = _root.Q<Label>($"ap-label-{i}");
                _gritPipRows[i]         = _root.Q<VisualElement>($"grit-pips-{i}");

                if (_hunterPanels[i] == null)
                    Debug.LogWarning($"[CombatUI] hunter-panel-{i} not found in UXML");
            }

            // Wire End Turn button
            var endTurnBtn = _root.Q<Button>("end-turn-btn");
            if (endTurnBtn != null)
                endTurnBtn.clicked += OnEndTurnClicked;
            else
                Debug.LogWarning("[CombatUI] end-turn-btn not found in UXML");
        }

        // ── Event Wiring ─────────────────────────────────────────
        private void WireEvents()
        {
            if (_combatManager == null) return;
            _combatManager.OnPhaseChanged    += OnPhaseChanged;
            _combatManager.OnDamageDealt     += OnDamageDealt;
            _combatManager.OnEntityCollapsed += OnEntityCollapsed;
            _combatManager.OnCombatEnded     += OnCombatEnded;
        }

        private void UnwireEvents()
        {
            if (_combatManager == null) return;
            _combatManager.OnPhaseChanged    -= OnPhaseChanged;
            _combatManager.OnDamageDealt     -= OnDamageDealt;
            _combatManager.OnEntityCollapsed -= OnEntityCollapsed;
            _combatManager.OnCombatEnded     -= OnCombatEnded;
        }

        // ── Phase Events ─────────────────────────────────────────
        private void OnPhaseChanged(CombatPhase phase)
        {
            if (_phaseLabel != null)
            {
                _phaseLabel.text = phase switch
                {
                    CombatPhase.VitalityPhase   => "VITALITY PHASE",
                    CombatPhase.HunterPhase     => "HUNTER PHASE",
                    CombatPhase.BehaviorRefresh => "BEHAVIOR REFRESH",
                    CombatPhase.MonsterPhase    => "MONSTER PHASE",
                    _                           => phase.ToString().ToUpper(),
                };
            }

            if (_roundLabel != null && _combatManager.CurrentState != null)
                _roundLabel.text = $"Round {_combatManager.CurrentState.currentRound + 1}";

            RefreshAll();
            Debug.Log($"[CombatUI] Phase → {phase}");
        }

        // ── Damage / Collapse / End Events ───────────────────────
        private void OnDamageDealt(string targetId, int amount, DamageType type)
        {
            Debug.Log($"[CombatUI] Damage: {amount} {type} → {targetId}");
            RefreshAll();
            // Flash effect deferred to Stage 6 polish pass
        }

        private void OnEntityCollapsed(string entityId)
        {
            Debug.Log($"[CombatUI] Entity collapsed: {entityId}");
            RefreshAll();
        }

        private void OnCombatEnded(CombatResult result)
        {
            Debug.Log($"[CombatUI] Combat ended — Victory:{result.isVictory}");
            // Victory/defeat modal deferred to Stage 6
        }

        // ── End Turn ─────────────────────────────────────────────
        private void OnEndTurnClicked()
        {
            if (_combatManager?.CurrentState == null) return;
            var activeHunter = System.Array.Find(
                _combatManager.CurrentState.hunters,
                h => !h.hasActedThisPhase && !h.isCollapsed);
            if (activeHunter != null)
            {
                _combatManager.EndHunterTurn(activeHunter.hunterId);
                Debug.Log($"[CombatUI] End Turn clicked for {activeHunter.hunterName}");
            }
        }

        // ── Full Refresh ─────────────────────────────────────────
        public void RefreshAll()
        {
            var state = _combatManager?.CurrentState;
            if (state == null) return;

            RefreshHunterPanels(state.hunters, state.aggroHolderId);
            RefreshMonsterPanel(state.monster);
            // Card hand refresh — stub until Session 5-D
            // Grid refresh — stub until Session 5-E
        }

        // ── Hunter Panels ─────────────────────────────────────────
        private void RefreshHunterPanels(HunterCombatState[] hunters, string aggroHolderId)
        {
            for (int i = 0; i < 4; i++)
            {
                if (_hunterPanels[i] == null) continue;

                if (i < hunters.Length)
                {
                    _hunterPanels[i].style.display = DisplayStyle.Flex;
                    RefreshHunterPanel(i, hunters[i], hunters[i].hunterId == aggroHolderId);
                }
                else
                {
                    // Hide unused panels (fewer than 4 hunters)
                    _hunterPanels[i].style.display = DisplayStyle.None;
                }
            }
        }

        private void RefreshHunterPanel(int index, HunterCombatState hunter, bool hasAggro)
        {
            // Name
            if (_hunterNames[index] != null)
                _hunterNames[index].text = hunter.hunterName;

            // Aggro indicator
            if (_aggroIndicators[index] != null)
                _aggroIndicators[index].style.display =
                    hasAggro ? DisplayStyle.Flex : DisplayStyle.None;

            // Collapsed state
            _hunterPanels[index].EnableInClassList("hunter-panel--collapsed", hunter.isCollapsed);
            if (hunter.isCollapsed)
                _hunterPanels[index].EnableInClassList("stone-panel--danger", true);

            // Body zones
            BuildBodyZones(index, hunter);

            // Status badges
            BuildStatusBadges(index, hunter.activeStatusEffects);

            // AP and Grit — only meaningful during Hunter Phase
            bool isHunterPhase = _combatManager?.CurrentPhase == CombatPhase.HunterPhase;
            if (_activeInfoPanels[index] != null)
                _activeInfoPanels[index].style.display =
                    isHunterPhase ? DisplayStyle.Flex : DisplayStyle.None;

            if (_apLabels[index] != null)
                _apLabels[index].text = $"AP: {hunter.apRemaining}";

            BuildGritPips(index, hunter.currentGrit, hunter.maxGrit);
        }

        private void BuildBodyZones(int panelIndex, HunterCombatState hunter)
        {
            var container = _bodyZoneContainers[panelIndex];
            if (container == null) return;
            container.Clear();

            string[] zones = { "Head", "Torso", "LeftArm", "RightArm", "LeftLeg", "RightLeg" };
            string[] labels = { "HEAD", "TORSO", "L.ARM", "R.ARM", "L.LEG", "R.LEG" };

            for (int z = 0; z < zones.Length; z++)
            {
                var zone = System.Array.Find(hunter.bodyZones, b => b.zone == zones[z]);

                var row = new VisualElement();
                row.AddToClassList("body-zone-row");

                // Zone label
                var label = new Label(labels[z]);
                label.AddToClassList("zone-label");
                row.Add(label);

                // Bars column
                var bars = new VisualElement();
                bars.AddToClassList("body-zone-bars");

                // Shell bar
                var shellTrack = new VisualElement();
                shellTrack.AddToClassList("shell-bar-track");
                var shellFill = new VisualElement();
                shellFill.AddToClassList("shell-bar-fill");
                float shellPct = zone.shellMax > 0
                    ? (float)zone.shellCurrent / zone.shellMax : 0f;
                shellFill.style.width = Length.Percent(shellPct * 100f);
                shellTrack.Add(shellFill);
                bars.Add(shellTrack);

                // Flesh bar
                var fleshTrack = new VisualElement();
                fleshTrack.AddToClassList("flesh-bar-track");
                var fleshFill = new VisualElement();
                fleshFill.AddToClassList("flesh-bar-fill");
                float fleshPct = zone.fleshMax > 0
                    ? (float)zone.fleshCurrent / zone.fleshMax : 0f;
                fleshFill.style.width = Length.Percent(fleshPct * 100f);
                fleshTrack.Add(fleshFill);
                bars.Add(fleshTrack);

                row.Add(bars);
                container.Add(row);
            }
        }

        private void BuildStatusBadges(int panelIndex, string[] activeEffects)
        {
            var row = _statusEffectRows[panelIndex];
            if (row == null) return;
            row.Clear();

            if (activeEffects == null) return;
            foreach (var effect in activeEffects)
            {
                var badge = new Label(effect.ToUpper());
                badge.AddToClassList("status-badge");
                row.Add(badge);
            }
        }

        private void BuildGritPips(int panelIndex, int currentGrit, int maxGrit)
        {
            var row = _gritPipRows[panelIndex];
            if (row == null) return;
            row.Clear();

            int total = Mathf.Max(maxGrit, 1);
            for (int g = 0; g < total; g++)
            {
                var pip = new VisualElement();
                pip.AddToClassList("grit-pip");
                if (g >= currentGrit)
                    pip.AddToClassList("grit-pip--empty");
                row.Add(pip);
            }
        }

        // ── Monster Panel — Stub ─────────────────────────────────
        // Full implementation in Session 5-D
        private void RefreshMonsterPanel(MonsterCombatState monster)
        {
            // Stub — implemented Session 5-D
            var nameLabel = _monsterPanel?.Q<Label>("monster-name");
            if (nameLabel != null) nameLabel.text = monster.monsterName;
        }
    }
}
```

---

## Wiring It Up in the Scene

After creating the script:

1. Add `CombatScreenController` component to the `CombatUI` GameObject
2. Assign `UIDocument` (the UIDocument component on the same GameObject) in the Inspector
3. Assign `CombatManager` (the CombatManager component from your scene) in the Inspector
4. Add a temporary `CombatTestBootstrapper` MonoBehaviour that calls `StartCombat()` in `Start()` with mock data from `CombatStateFactory.BuildMockCombatState()`

---

## Verification Test

Play the scene and verify in the Game view:

- [ ] Phase label shows "VITALITY PHASE" on start
- [ ] Round label shows "Round 1"
- [ ] Hunter panel 0 shows "Aldric" (from mock state)
- [ ] Hunter panel 0 aggro indicator (⚑) is visible
- [ ] Hunter panels 1–3 are hidden (mock state has 1 hunter)
- [ ] Shell bars and flesh bars render at correct proportions
- [ ] Shell bars are visually shorter (8px) than flesh bars (12px)
- [ ] Calling `_combatManager.AdvancePhase()` from a test script changes the phase label

No card hand. No grid. No monster detail. Just hunter panels and phase label confirmed working.

---

## Next Session

**File:** `_Docs/Stage_05/STAGE_05_D.md`  
**Covers:** Card hand rendering, card element construction, card click → target-select mode, monster panel full implementation
