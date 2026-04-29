using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using MnM.Core.Systems;
using MnM.Core.Data;

namespace MnM.Core.UI
{
    public class CombatScreenController : MonoBehaviour
    {
        // ── Inspector References ─────────────────────────────────
        [SerializeField] private UIDocument       _uiDocument;
        [SerializeField] private CombatManager    _combatManager;
        [SerializeField] private VisualTreeAsset  _resultModalAsset;

        // ── Cached Root Elements ─────────────────────────────────
        private VisualElement _root;
        private Label         _phaseLabel;
        private Label         _roundLabel;

        // Hunter panels — indexed 0–3
        private VisualElement[] _hunterPanels        = new VisualElement[4];
        private Label[]         _hunterNames         = new Label[4];
        private Label[]         _aggroIndicators     = new Label[4];
        private VisualElement[] _bodyZoneContainers  = new VisualElement[4];
        private VisualElement[] _statusEffectRows    = new VisualElement[4];
        private VisualElement[] _activeInfoPanels    = new VisualElement[4];
        private Label[]         _apLabels            = new Label[4];
        private VisualElement[] _gritPipRows         = new VisualElement[4];

        // Status effect displays — one per hunter slot
        private StatusEffectDisplay[] _statusDisplays = new StatusEffectDisplay[4];

        // Monster panel
        private VisualElement _monsterPanel;

        // Card hand
        private VisualElement _handCards;

        // ── Card Selection State ─────────────────────────────────
        private string        _pendingCardName = null;   // Card selected, awaiting target
        private VisualElement _selectedCardEl  = null;   // Currently highlighted card element

        // ── Grid ─────────────────────────────────────────────────
        private VisualElement    _gridContainer;
        private VisualElement[,] _gridCells;              // [x, y] — 22×16

        [SerializeField] private GridManager _gridManager;

        // ── Keyboard / Grid Cursor ───────────────────────────────
        private Vector2Int _gridCursor = new Vector2Int(-1, -1); // -1 = no selection

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
            AudioManager.Instance?.SetMusicContext(AudioContext.CombatStandard);
            Debug.Log("[CombatUI] Controller enabled — elements cached and events wired");
        }

        private void OnDisable()
        {
            UnwireEvents();
        }

        private void Start()
        {
            if (_combatManager?.CurrentState != null)
            {
                RefreshAll();
                BuildGrid();
                InitialiseStatusDisplays();
            }
        }

        // ── Element Caching ──────────────────────────────────────
        private void CacheElements()
        {
            _phaseLabel   = _root.Q<Label>("phase-label");
            _roundLabel   = _root.Q<Label>("round-label");
            _handCards    = _root.Q<VisualElement>("hand-cards");
            _monsterPanel = _root.Q<VisualElement>("monster-panel");

            for (int i = 0; i < 4; i++)
            {
                _hunterPanels[i]       = _root.Q<VisualElement>($"hunter-panel-{i}");
                _hunterNames[i]        = _root.Q<Label>($"hunter-name-{i}");
                _aggroIndicators[i]    = _root.Q<Label>($"aggro-{i}");
                _bodyZoneContainers[i] = _root.Q<VisualElement>($"body-zones-{i}");
                _statusEffectRows[i]   = _root.Q<VisualElement>($"status-effects-{i}");
                _activeInfoPanels[i]   = _root.Q<VisualElement>($"active-info-{i}");
                _apLabels[i]           = _root.Q<Label>($"ap-label-{i}");
                _gritPipRows[i]        = _root.Q<VisualElement>($"grit-pips-{i}");

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

        // ── Status Display Setup ──────────────────────────────────
        private void InitialiseStatusDisplays()
        {
            var state = _combatManager.CurrentState;
            for (int i = 0; i < 4 && i < state.hunters.Length; i++)
            {
                var iconStrip = _statusEffectRows[i];
                if (iconStrip == null)
                {
                    Debug.LogWarning($"[CombatUI] status-effects-{i} not found — StatusEffectDisplay skipped");
                    continue;
                }
                var display = gameObject.AddComponent<StatusEffectDisplay>();
                display.Initialise(iconStrip, state.hunters[i].hunterId, _combatManager);
                _statusDisplays[i] = display;
            }
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
            AudioManager.Instance?.OnDamageDealt(type);
            RefreshAll();
            // Flash effect deferred to Stage 6 polish pass
        }

        private void OnEntityCollapsed(string entityId)
        {
            Debug.Log($"[CombatUI] Entity collapsed: {entityId}");
            AudioManager.Instance?.OnEntityCollapsed(entityId);
            RefreshAll();
        }

        private void OnCombatEnded(CombatResult result)
        {
            Debug.Log($"[CombatUI] Combat ended — Victory:{result.isVictory}");
            if (result.isVictory) AudioManager.Instance?.OnMonsterDefeated();

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
            // TemplateContainer has no implicit size — must be stretched to fill root
            // so the position:absolute modal-overlay inside can cover the full screen.
            overlay.style.position = Position.Absolute;
            overlay.style.left     = 0;
            overlay.style.top      = 0;
            overlay.style.right    = 0;
            overlay.style.bottom   = 0;
            _root.Add(overlay);

            var state = _combatManager.CurrentState;

            // Title and outcome
            overlay.Q<Label>("result-title").text = result.isVictory ? "HUNT COMPLETE" : "HUNT FAILED";
            var outcomeLabel = overlay.Q<Label>("result-outcome");
            outcomeLabel.text = result.isVictory ? "VICTORY" : "DEFEAT";
            outcomeLabel.style.color = result.isVictory
                ? new StyleColor(new Color(0.40f, 0.80f, 0.40f))
                : new StyleColor(new Color(0.80f, 0.25f, 0.25f));

            overlay.Q<Label>("result-rounds").text  = $"{result.roundsElapsed} rounds fought";
            overlay.Q<Label>("result-monster").text = state.monster.monsterName;

            // Loot list (victory only — resolved in Settlement in Stage 7)
            var lootList = overlay.Q<VisualElement>("loot-list");
            if (result.isVictory)
            {
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
                    ? $"\u2691 {hunter.hunterName} \u2014 COLLAPSED"
                    : $"\u2713 {hunter.hunterName} \u2014 survived");
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
            var state = _combatManager.CurrentState;

            var huntResult = new HuntResult
            {
                isVictory          = result.isVictory,
                monsterName        = state.monster.monsterName,
                monsterDifficulty  = state.monster.difficulty,
                roundsFought       = result.roundsElapsed,
                collapsedHunterIds = result.collapsedHunterIds ?? new string[0],
                survivingHunterIds = state.hunters
                    .Where(h => !h.isCollapsed)
                    .Select(h => h.hunterId)
                    .ToArray(),
                lootGained             = new ResourceEntry[0],
                injuryCardNamesApplied = new string[state.hunters.Length],
            };

            Debug.Log($"[CombatUI] Building HuntResult — Victory:{huntResult.isVictory} " +
                      $"Collapsed:{huntResult.collapsedHunterIds.Length} " +
                      $"Surviving:{huntResult.survivingHunterIds.Length}");

            GameStateManager.Instance.ReturnFromHunt(huntResult);
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
                // Hunter still has their turn — end it
                _combatManager.EndHunterTurn(activeHunter.hunterId);
                Debug.Log($"[CombatUI] End Turn: {activeHunter.hunterName} done");
            }
            else
            {
                // All hunters have acted — advance to next phase
                _combatManager.AdvancePhase();
                Debug.Log("[CombatUI] All hunters acted — phase advanced");
            }
        }

        // ── Full Refresh ─────────────────────────────────────────
        public void RefreshAll()
        {
            var state = _combatManager?.CurrentState;
            if (state == null) return;

            RefreshHunterPanels(state.hunters, state.aggroHolderId);
            RefreshMonsterPanel(state.monster);
            RefreshCardHand(state);
            RefreshGrid();
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

            // AP and Grit — only visible during Hunter Phase
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

            string[] zones  = { "Head", "Torso", "LeftArm", "RightArm", "LeftLeg", "RightLeg" };
            string[] labels = { "HEAD", "TORSO", "L.ARM", "R.ARM", "L.LEG", "R.LEG" };

            for (int z = 0; z < zones.Length; z++)
            {
                var zone = System.Array.Find(hunter.bodyZones, b => b.zone == zones[z]);

                var row = new VisualElement();
                row.AddToClassList("body-zone-row");

                // Zone label
                var zoneLabel = new Label(labels[z]);
                zoneLabel.AddToClassList("zone-label");
                row.Add(zoneLabel);

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

        // ── Monster Panel ─────────────────────────────────────────
        private void RefreshMonsterPanel(MonsterCombatState monster)
        {
            if (_monsterPanel == null) return;

            var nameLabel = _monsterPanel.Q<Label>("monster-name");
            if (nameLabel != null) nameLabel.text = monster.monsterName;

            var diffLabel = _monsterPanel.Q<Label>("monster-difficulty");
            if (diffLabel != null) diffLabel.text = monster.difficulty;

            var deckLabel = _monsterPanel.Q<Label>("monster-deck-count");
            if (deckLabel != null)
            {
                int removable = monster.activeDeckCardNames?.Length ?? 0;
                deckLabel.text = $"Removable: {removable}";
            }

            var stanceLabel = _monsterPanel.Q<Label>("monster-stance");
            if (stanceLabel != null)
                stanceLabel.text = string.IsNullOrEmpty(monster.currentStanceTag)
                    ? "" : $"Stance: {monster.currentStanceTag}";

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
            if (part.isBroken) row.AddToClassList("monster-part-row--broken");

            var nameLabel = new Label(part.isRevealed ? part.partName : "???");
            nameLabel.AddToClassList("part-name");
            row.Add(nameLabel);

            var bars = new VisualElement();
            bars.AddToClassList("part-bars");

            var shellTrack = new VisualElement();
            shellTrack.AddToClassList("shell-bar-track");
            var shellFill = new VisualElement();
            shellFill.AddToClassList("shell-bar-fill");
            float shellPct = part.shellMax > 0 ? (float)part.shellCurrent / part.shellMax : 0f;
            shellFill.style.width = Length.Percent(shellPct * 100f);
            shellTrack.Add(shellFill);
            bars.Add(shellTrack);

            var fleshTrack = new VisualElement();
            fleshTrack.AddToClassList("flesh-bar-track");
            var fleshFill = new VisualElement();
            fleshFill.AddToClassList("flesh-bar-fill");
            float fleshPct = part.fleshMax > 0 ? (float)part.fleshCurrent / part.fleshMax : 0f;
            fleshFill.style.width = Length.Percent(fleshPct * 100f);
            fleshTrack.Add(fleshFill);
            bars.Add(fleshTrack);

            row.Add(bars);

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

        // ── Card Hand ─────────────────────────────────────────────
        private ActionCardSO LoadCard(string cardName) =>
            Resources.Load<ActionCardSO>($"Data/Cards/Action/{cardName}");

        private void RefreshCardHand(CombatState state)
        {
            if (_handCards == null) return;
            _handCards.Clear();
            _selectedCardEl  = null;
            _pendingCardName = null;

            var activeHunter = System.Array.Find(
                state.hunters, h => !h.hasActedThisPhase && !h.isCollapsed);

            if (activeHunter == null)
            {
                Debug.Log("[CombatUI] No active hunter — card hand empty");
                return;
            }

            var apDisplay = _root.Q<Label>("ap-display");
            if (apDisplay != null)
                apDisplay.text = $"AP: {activeHunter.apRemaining}";

            var gritDisplay = _root.Q<Label>("grit-display");
            if (gritDisplay != null)
                gritDisplay.text = $"Grit: {activeHunter.currentGrit}";

            foreach (var cardName in activeHunter.handCardNames)
                _handCards.Add(BuildCardElement(cardName, activeHunter));
        }

        private VisualElement BuildCardElement(string cardName, HunterCombatState hunter)
        {
            var card = LoadCard(cardName);

            var el = new VisualElement();
            el.AddToClassList("card");
            el.AddToClassList("stone-panel");

            // Header: name + Loud tag
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

            // Category
            if (card != null)
            {
                var categoryLabel = new Label(card.category.ToString().ToUpper());
                categoryLabel.AddToClassList("card-category");
                el.Add(categoryLabel);
            }

            // Effect text
            var effectLabel = new Label(card != null ? card.effectDescription : "");
            effectLabel.AddToClassList("card-effect");
            el.Add(effectLabel);

            // Footer: AP cost
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

            bool canPlay = card == null || hunter.apRemaining >= (card.apCost - card.apRefund);
            el.EnableInClassList("card--unplayable", !canPlay);

            string capturedName = cardName;
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

            if (_selectedCardEl != null)
                _selectedCardEl.EnableInClassList("card--selected", false);

            if (_pendingCardName == cardName)
            {
                // Toggle off — clicking same card again cancels selection
                _pendingCardName = null;
                _selectedCardEl  = null;
                Debug.Log($"[CombatUI] Card deselected: {cardName}");
                RefreshGrid();
                return;
            }

            _pendingCardName = cardName;
            _selectedCardEl  = cardEl;
            cardEl.EnableInClassList("card--selected", true);

            Debug.Log($"[CombatUI] Card selected: {cardName} — click a grid cell to target");
            RefreshGrid();
        }

        // ── Grid Building ─────────────────────────────────────────
        public void BuildGrid()
        {
            _gridContainer = _root.Q<VisualElement>("grid-container");
            if (_gridContainer == null)
            {
                Debug.LogError("[CombatUI] grid-container not found in UXML");
                return;
            }

            _gridContainer.Clear();
            _gridCells = new VisualElement[22, 16];

            for (int y = 0; y < 16; y++)
            {
                var row = new VisualElement();
                row.AddToClassList("grid-row");

                for (int x = 0; x < 22; x++)
                {
                    var cell = new VisualElement();
                    cell.AddToClassList("grid-cell");

                    int cx = x, cy = y;
                    cell.RegisterCallback<ClickEvent>(_ => OnGridCellClicked(cx, cy));

                    _gridCells[x, y] = cell;
                    row.Add(cell);
                }

                _gridContainer.Add(row);
            }

            Debug.Log("[CombatUI] Grid built: 22×16 cells");
            RefreshGrid();
        }

        public void RefreshGrid()
        {
            if (_gridCells == null || _combatManager?.CurrentState == null) return;

            var state = _combatManager.CurrentState;
            IGridManager grid = _gridManager;

            for (int y = 0; y < 16; y++)
            for (int x = 0; x < 22; x++)
            {
                var cell = _gridCells[x, y];
                if (cell == null) continue;

                var pos = new Vector2Int(x, y);

                cell.EnableInClassList("grid-cell--denied",   false);
                cell.EnableInClassList("grid-cell--marrow",   false);
                cell.EnableInClassList("grid-cell--hunter",   false);
                cell.EnableInClassList("grid-cell--monster",  false);
                cell.EnableInClassList("grid-cell--selected", false);
                cell.EnableInClassList("grid-cell--valid",    false);

                if (grid != null)
                {
                    if (grid.IsDenied(pos))     cell.AddToClassList("grid-cell--denied");
                    if (grid.IsMarrowSink(pos)) cell.AddToClassList("grid-cell--marrow");
                }

                bool isHunterCell  = IsHunterAtCell(state.hunters, x, y);
                bool isMonsterCell = IsMonsterAtCell(state.monster, x, y);
                if (isHunterCell)  cell.AddToClassList("grid-cell--hunter");
                if (isMonsterCell) cell.AddToClassList("grid-cell--monster");

                if (_gridCursor.x == x && _gridCursor.y == y)
                    cell.AddToClassList("grid-cell--selected");

                if (_pendingCardName != null && isMonsterCell)
                    cell.AddToClassList("grid-cell--valid");
            }
        }

        private bool IsHunterAtCell(HunterCombatState[] hunters, int x, int y)
        {
            foreach (var h in hunters)
                if (!h.isCollapsed && h.gridX == x && h.gridY == y) return true;
            return false;
        }

        private bool IsMonsterAtCell(MonsterCombatState monster, int x, int y)
        {
            return x >= monster.gridX && x < monster.gridX + monster.footprintW &&
                   y >= monster.gridY && y < monster.gridY + monster.footprintH;
        }

        // ── Grid Cell Clicks ──────────────────────────────────────
        private void OnGridCellClicked(int x, int y)
        {
            _gridCursor = new Vector2Int(x, y);
            Debug.Log($"[CombatUI] Grid cell clicked: ({x},{y})");

            if (_pendingCardName != null)
            {
                ResolveCardAtCell(x, y);
            }
            else
            {
                var state = _combatManager?.CurrentState;
                if (state != null)
                {
                    var hunter = System.Array.Find(
                        state.hunters, h => !h.isCollapsed && h.gridX == x && h.gridY == y);
                    if (hunter != null)
                        Debug.Log($"[CombatUI] Hunter at ({x},{y}): {hunter.hunterName}");
                }
            }

            RefreshGrid();
        }

        private void ResolveCardAtCell(int x, int y)
        {
            var state = _combatManager?.CurrentState;
            if (state == null || _pendingCardName == null) return;

            var activeHunter = System.Array.Find(
                state.hunters, h => !h.hasActedThisPhase && !h.isCollapsed);
            if (activeHunter == null)
            {
                Debug.LogWarning("[CombatUI] No active hunter to play card");
                return;
            }

            var targetCell = new Vector2Int(x, y);
            bool success = _combatManager.TryPlayCard(
                activeHunter.hunterId, _pendingCardName, targetCell);

            if (success)
                Debug.Log($"[CombatUI] Card played: {_pendingCardName} → ({x},{y})");
            else
                Debug.Log($"[CombatUI] TryPlayCard failed: {_pendingCardName} → ({x},{y}) — invalid target or insufficient AP");

            _pendingCardName = null;
            if (_selectedCardEl != null)
            {
                _selectedCardEl.EnableInClassList("card--selected", false);
                _selectedCardEl = null;
            }

            RefreshAll();
            RefreshGrid();
        }

        // ── Keyboard Input ────────────────────────────────────────
        private void Update()
        {
            HandleKeyboardInput();
        }

        private void HandleKeyboardInput()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            // Number keys 1–6: select card by index
            Key[] digitKeys = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5, Key.Digit6 };
            for (int i = 0; i < digitKeys.Length; i++)
            {
                if (kb[digitKeys[i]].wasPressedThisFrame)
                {
                    SelectCardByIndex(i);
                    return;
                }
            }

            // Escape: cancel card selection
            if (kb[Key.Escape].wasPressedThisFrame)
            {
                if (_pendingCardName != null)
                {
                    _pendingCardName = null;
                    if (_selectedCardEl != null)
                    {
                        _selectedCardEl.EnableInClassList("card--selected", false);
                        _selectedCardEl = null;
                    }
                    RefreshGrid();
                    Debug.Log("[CombatUI] Card selection cancelled");
                }
                return;
            }

            // DEBUG ONLY — Ctrl+W: force victory, Ctrl+L: force loss
            // Must be checked before WASD so Ctrl+W doesn't also move the cursor.
            // Remove these shortcuts before Stage 7 content begins.
            bool ctrlHeld = kb[Key.LeftCtrl].isPressed || kb[Key.RightCtrl].isPressed;
            if (ctrlHeld)
            {
                if (kb[Key.W].wasPressedThisFrame)
                {
                    Debug.Log("[CombatUI] DEBUG: Force victory triggered (Ctrl+W)");
                    var state = _combatManager?.CurrentState;
                    OnCombatEnded(new CombatResult
                    {
                        isVictory          = true,
                        roundsElapsed      = state?.currentRound ?? 0,
                        collapsedHunterIds = new string[0],
                    });
                    return;
                }
                if (kb[Key.L].wasPressedThisFrame)
                {
                    Debug.Log("[CombatUI] DEBUG: Force loss triggered (Ctrl+L)");
                    var state = _combatManager?.CurrentState;
                    OnCombatEnded(new CombatResult
                    {
                        isVictory          = false,
                        roundsElapsed      = state?.currentRound ?? 0,
                        collapsedHunterIds = state != null
                            ? System.Array.ConvertAll(state.hunters, h => h.hunterId)
                            : new string[0],
                    });
                    return;
                }
            }

            // WASD / Arrow: move grid cursor (Ctrl modifier excluded above)
            Vector2Int delta = Vector2Int.zero;
            if (kb[Key.W].wasPressedThisFrame || kb[Key.UpArrow].wasPressedThisFrame)    delta.y = -1;
            if (kb[Key.S].wasPressedThisFrame || kb[Key.DownArrow].wasPressedThisFrame)  delta.y =  1;
            if (kb[Key.A].wasPressedThisFrame || kb[Key.LeftArrow].wasPressedThisFrame)  delta.x = -1;
            if (kb[Key.D].wasPressedThisFrame || kb[Key.RightArrow].wasPressedThisFrame) delta.x =  1;

            if (delta != Vector2Int.zero)
            {
                int nx = Mathf.Clamp((_gridCursor.x < 0 ? 11 : _gridCursor.x) + delta.x, 0, 21);
                int ny = Mathf.Clamp((_gridCursor.y < 0 ?  8 : _gridCursor.y) + delta.y, 0, 15);
                _gridCursor = new Vector2Int(nx, ny);
                RefreshGrid();
                return;
            }

            // Enter / Space: confirm grid cursor selection
            if (kb[Key.Enter].wasPressedThisFrame || kb[Key.Space].wasPressedThisFrame)
            {
                if (_gridCursor.x >= 0)
                    OnGridCellClicked(_gridCursor.x, _gridCursor.y);
            }
        }

        private void SelectCardByIndex(int index)
        {
            var state = _combatManager?.CurrentState;
            if (state == null) return;

            var activeHunter = System.Array.Find(
                state.hunters, h => !h.hasActedThisPhase && !h.isCollapsed);
            if (activeHunter == null || index >= activeHunter.handCardNames.Length) return;

            string cardName = activeHunter.handCardNames[index];
            var cardEl = _handCards?.ElementAt(index) as VisualElement;

            Debug.Log($"[CombatUI] Keyboard card select [{index + 1}]: {cardName}");
            if (cardEl != null) OnCardClicked(cardName, cardEl, true);
        }
    }
}
