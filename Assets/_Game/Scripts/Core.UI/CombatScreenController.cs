using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using MnM.Core.Systems;
using MnM.Core.Data;
using MnM.Core.Logic;

namespace MnM.Core.UI
{
    public class CombatScreenController : MonoBehaviour
    {
        // ── Inspector References ─────────────────────────────────
        [SerializeField] private UIDocument              _uiDocument;
        [SerializeField] private CombatManager           _combatManager;
        [SerializeField] private VisualTreeAsset         _resultModalAsset;
        [SerializeField] private CardAnimationController _cardAnim;

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
        private VisualElement _monsterPartsContainer;
        private VisualElement _behaviorCardsContainer;

        // Part HP bars — built once, updated on damage (avoids DOM rebuilds)
        private readonly Dictionary<string, PartHealthBar> _partBars = new();

        // Phase transition banner
        private VisualElement _phaseBanner;
        private Label         _phaseBannerLabel;
        private Coroutine     _bannerCoroutine;

        // End Turn button — cached for enable/disable during auto-advance phases
        private Button    _endTurnBtn;
        // Auto-advance coroutine for non-interactive phases (BehaviorRefresh, MonsterPhase)
        private Coroutine _autoAdvanceCoroutine;

        // Card hand
        private VisualElement _handCards;
        private Label         _deckCountLabel;

        // Animation overlay — absolute-positioned layer for cards mid-flight
        private VisualElement _animationOverlay;

        // Maps card name → its current VisualElement in the hand (for animation lookup)
        private readonly Dictionary<string, VisualElement> _handCardElements     = new();
        // Maps behavior card name → its current VisualElement in the monster panel
        private readonly Dictionary<string, VisualElement> _behaviorCardElements = new();
        // Tracks which behavior card names are currently rendered (skip rebuild when unchanged)
        private readonly HashSet<string> _renderedBehaviorCardNames = new();

        // ── Card Selection State ─────────────────────────────────
        private string        _pendingCardName = null;   // Card selected, awaiting target
        private VisualElement _selectedCardEl  = null;   // Currently highlighted card element

        // ── Grid ─────────────────────────────────────────────────
        private VisualElement    _gridContainer;
        private VisualElement[,] _gridCells;              // [x, y] — 22×16

        [SerializeField] private GridManager _gridManager;

        // ── Keyboard / Grid Cursor ───────────────────────────────
        private Vector2Int _gridCursor = new Vector2Int(-1, -1); // -1 = no selection

        // ── Deployment State ──────────────────────────────────────────
        [SerializeField] private SpawnZoneSO[] _spawnZones;
        private string _deployingHunterId = null;

        // ── Movement State ────────────────────────────────────────────
        private HashSet<Vector2Int> _validMoveCells = new();

        // ── Lifecycle ────────────────────────────────────────────
        private void OnEnable()
        {
            if (_uiDocument == null)
            {
                Debug.LogError("[CombatUI] UIDocument not assigned");
                return;
            }

            // Force fresh DOM builds — ensures hot-reloads pick up new layouts
            // (Domain Reload keeps non-serialized fields alive across recompiles)
            _partBars.Clear();
            _behaviorCardsContainer = null;
            _handCardElements.Clear();
            _behaviorCardElements.Clear();
            _renderedBehaviorCardNames.Clear();

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
            _phaseLabel          = _root.Q<Label>("phase-label");
            _roundLabel          = _root.Q<Label>("round-label");

            // Transparent overlay for cards mid-animation (play/discard fly-outs)
            _animationOverlay = new VisualElement();
            _animationOverlay.style.position = Position.Absolute;
            _animationOverlay.style.left     = 0;
            _animationOverlay.style.top      = 0;
            _animationOverlay.style.right    = 0;
            _animationOverlay.style.bottom   = 0;
            _animationOverlay.pickingMode    = PickingMode.Ignore;
            _root.Add(_animationOverlay);

            _handCards           = _root.Q<VisualElement>("hand-cards");
            _deckCountLabel      = _root.Q<Label>("deck-count");
            _monsterPanel        = _root.Q<VisualElement>("monster-panel");
            _monsterPartsContainer = _root.Q<VisualElement>("monster-parts-container");
            _phaseBanner         = _root.Q<VisualElement>("phase-banner");
            _phaseBannerLabel    = _root.Q<Label>("phase-banner-label");

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
            _endTurnBtn = _root.Q<Button>("end-turn-btn");
            if (_endTurnBtn != null)
                _endTurnBtn.clicked += OnEndTurnClicked;
            else
                Debug.LogWarning("[CombatUI] end-turn-btn not found in UXML");
        }

        // ── Event Wiring ─────────────────────────────────────────
        private void WireEvents()
        {
            if (_combatManager == null) return;
            _combatManager.OnPhaseChanged          += OnPhaseChanged;
            _combatManager.OnDamageDealt           += OnDamageDealt;
            _combatManager.OnEntityCollapsed       += OnEntityCollapsed;
            _combatManager.OnCombatEnded           += OnCombatEnded;
            _combatManager.OnBehaviorCardActivated += OnBehaviorCardActivated;
            _combatManager.OnBehaviorCardRemoved   += OnBehaviorCardRemoved;
        }

        private void UnwireEvents()
        {
            if (_combatManager == null) return;
            _combatManager.OnPhaseChanged          -= OnPhaseChanged;
            _combatManager.OnDamageDealt           -= OnDamageDealt;
            _combatManager.OnEntityCollapsed       -= OnEntityCollapsed;
            _combatManager.OnCombatEnded           -= OnCombatEnded;
            _combatManager.OnBehaviorCardActivated -= OnBehaviorCardActivated;
            _combatManager.OnBehaviorCardRemoved   -= OnBehaviorCardRemoved;
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
            // ── Deployment Phase ──────────────────────────────────────
            if (phase == CombatPhase.DeploymentPhase)
            {
                if (_endTurnBtn != null) _endTurnBtn.SetEnabled(false);

                var next = GetNextUnplacedHunter();
                _deployingHunterId = next?.hunterId;
                ShowSpawnZone();

                if (_phaseLabel != null)
                    _phaseLabel.text = next != null
                        ? $"DEPLOY: Place {next.hunterName}"
                        : "DEPLOY: All placed";

                if (_roundLabel != null && _combatManager.CurrentState != null)
                    _roundLabel.text = $"ROUND {_combatManager.CurrentState.currentRound + 1}";

                RefreshAll();
                Debug.Log($"[CombatUI] Phase → {phase} — placing: {next?.hunterName ?? "none"}");
                return;
            }

            // ── Normal Phase Handling ─────────────────────────────────
            ClearSpawnZone();
            _deployingHunterId = null;

            string phaseText = phase switch
            {
                CombatPhase.VitalityPhase   => "VITALITY PHASE",
                CombatPhase.HunterPhase     => "HUNTER PHASE",
                CombatPhase.BehaviorRefresh => "BEHAVIOR REFRESH",
                CombatPhase.MonsterPhase    => "MONSTER PHASE",
                _                           => phase.ToString().ToUpper(),
            };

            if (_phaseLabel != null)
                _phaseLabel.text = phaseText;

            if (_roundLabel != null && _combatManager.CurrentState != null)
                _roundLabel.text = $"ROUND {_combatManager.CurrentState.currentRound + 1}";

            if (phase == CombatPhase.VitalityPhase || phase == CombatPhase.MonsterPhase)
                TriggerPhaseBanner(phaseText);

            // Discard the hand when hunters are done acting and the round moves on.
            // BehaviorRefresh is the first phase where the hand is truly finished.
            if (phase == CombatPhase.BehaviorRefresh && _handCardElements.Count > 0)
                AnimateDiscardHand();

            // Enable End Turn only during the interactive phases
            bool interactive = phase == CombatPhase.VitalityPhase || phase == CombatPhase.HunterPhase;
            if (_endTurnBtn != null) _endTurnBtn.SetEnabled(interactive);

            // Auto-advance non-interactive phases so monster acts without player input
            if (_autoAdvanceCoroutine != null) StopCoroutine(_autoAdvanceCoroutine);
            if (phase == CombatPhase.BehaviorRefresh)
                _autoAdvanceCoroutine = StartCoroutine(AutoAdvancePhase(0.4f));
            else if (phase == CombatPhase.MonsterPhase)
                _autoAdvanceCoroutine = StartCoroutine(AutoAdvancePhase(1.5f));

            RefreshAll();

            // Clear stale highlights first, then show movement range if entering Hunter Phase
            ClearMovementRange();
            if (phase == CombatPhase.HunterPhase && _pendingCardName == null)
            {
                var active = GetActiveHunter();
                if (active != null) ShowMovementRange(active);
            }

            Debug.Log($"[CombatUI] Phase → {phase}");
        }

        /// <summary>
        /// Moves all current hand card elements to the animation overlay and
        /// fades them out. Called at round boundary before the hand is rebuilt.
        /// </summary>
        private void AnimateDiscardHand()
        {
            foreach (var kvp in _handCardElements)
            {
                var el = kvp.Value;
                if (el.parent == _handCards)
                {
                    _handCards.Remove(el);
                    _animationOverlay?.Add(el);
                }
                _cardAnim?.AnimateDiscard(el);
            }
            _handCardElements.Clear();
        }

        private IEnumerator AutoAdvancePhase(float delay)
        {
            yield return new WaitForSeconds(delay);
            _autoAdvanceCoroutine = null;
            _combatManager.AdvancePhase();
        }

        private void OnBehaviorCardActivated(string cardName)
        {
            if (_cardAnim == null) return;
            if (_behaviorCardElements.TryGetValue(cardName, out var el))
            {
                _cardAnim.AnimateBehaviorActivation(el);
                Debug.Log($"[CombatUI] Behavior card pulse: {cardName}");
            }
            else
            {
                Debug.LogWarning($"[CombatUI] Behavior card element not found for pulse: {cardName}");
            }
        }

        private void OnBehaviorCardRemoved()
        {
            // Invalidate the cache so RefreshBehaviorCards always rebuilds after a removal
            _renderedBehaviorCardNames.Clear();
            if (_combatManager?.CurrentState != null)
                RefreshMonsterPanel(_combatManager.CurrentState.monster);
        }

        private void TriggerPhaseBanner(string text)
        {
            if (_phaseBanner == null) return;
            if (_phaseBannerLabel != null) _phaseBannerLabel.text = text;
            if (_bannerCoroutine != null) StopCoroutine(_bannerCoroutine);
            _bannerCoroutine = StartCoroutine(AnimatePhaseBanner());
        }

        private IEnumerator AnimatePhaseBanner()
        {
            // Slide down from off-screen top
            float t = 0f;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                _phaseBanner.style.top = Mathf.Lerp(-80f, 0f, t / 0.3f);
                yield return null;
            }
            _phaseBanner.style.top = 0f;

            yield return new WaitForSeconds(1.5f);

            // Slide back up
            t = 0f;
            while (t < 0.25f)
            {
                t += Time.deltaTime;
                _phaseBanner.style.top = Mathf.Lerp(0f, -80f, t / 0.25f);
                yield return null;
            }
            _phaseBanner.style.top = -80f;
            _bannerCoroutine = null;
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
            if (_autoAdvanceCoroutine != null)
            {
                StopCoroutine(_autoAdvanceCoroutine);
                _autoAdvanceCoroutine = null;
            }
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
            if (SceneTransitionManager.Instance != null)
                StartCoroutine(SceneTransitionManager.Instance.SlideIn(overlay));

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

        private HunterCombatState GetActiveHunter()
        {
            var state = _combatManager?.CurrentState;
            if (state == null) return null;
            return System.Array.Find(state.hunters, h => !h.hasActedThisPhase && !h.isCollapsed);
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

            // Label so it's not confused with the AP display above it
            var lbl = new Label("GRIT");
            lbl.style.color      = new Color(0.54f, 0.54f, 0.54f);
            lbl.style.fontSize   = 7;
            lbl.style.marginRight = 4;
            row.Add(lbl);

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
                deckLabel.text = $"Removable: {_combatManager.MonsterRemainingRemovableCount}";

            var stanceLabel = _monsterPanel.Q<Label>("monster-stance");
            if (stanceLabel != null)
                stanceLabel.text = string.IsNullOrEmpty(monster.currentStanceTag)
                    ? "" : $"Stance: {monster.currentStanceTag}";

            if (_monsterPartsContainer != null)
            {
                if (_partBars.Count == 0)
                    BuildMonsterPartBars(monster.parts);
                else
                    UpdateMonsterPartBars(monster.parts);
            }

            RefreshBehaviorCards();
        }

        private void RefreshBehaviorCards()
        {
            if (_behaviorCardsContainer == null)
            {
                var deckRoot = _root.Q<VisualElement>("behavior-deck-container");
                if (deckRoot == null) return;

                var header = new Label("BEHAVIOR DECK");
                header.style.color       = new Color(0.72f, 0.52f, 0.04f);
                header.style.fontSize    = 9;
                header.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
                header.style.paddingLeft   = 6;
                header.style.paddingTop    = 6;
                header.style.paddingBottom = 4;
                deckRoot.Add(header);

                var scroll = new ScrollView(ScrollViewMode.Vertical);
                scroll.style.flexGrow = 1;
                deckRoot.Add(scroll);

                _behaviorCardsContainer = scroll.contentContainer;
                _behaviorCardsContainer.style.flexDirection = FlexDirection.Column;
                _behaviorCardsContainer.style.paddingLeft   = 6;
                _behaviorCardsContainer.style.paddingRight  = 6;
                _behaviorCardsContainer.style.paddingBottom = 8;
            }

            var cards = _combatManager.GetActiveBehaviorCards();

            // Skip rebuild if the deck hasn't changed — preserves elements mid-pulse
            var currentNames = new HashSet<string>(cards.Select(c => c.cardName));
            if (currentNames.SetEquals(_renderedBehaviorCardNames)) return;
            _renderedBehaviorCardNames.Clear();
            foreach (var n in currentNames) _renderedBehaviorCardNames.Add(n);

            _behaviorCardsContainer.Clear();
            _behaviorCardElements.Clear();

            if (cards.Length == 0)
            {
                var empty = new Label("No cards remaining");
                empty.style.color    = new Color(0.54f, 0.54f, 0.54f);
                empty.style.fontSize = 8;
                _behaviorCardsContainer.Add(empty);
                return;
            }

            foreach (var card in cards)
            {
                var el = CardRenderer.BuildBehaviorCard(card);
                el.style.marginBottom = 8;
                _behaviorCardsContainer.Add(el);
                _behaviorCardElements[card.cardName] = el;
            }
        }

        private void BuildMonsterPartBars(MonsterPartState[] parts)
        {
            _monsterPartsContainer.Clear();
            _partBars.Clear();
            if (parts == null) return;

            // Wrap in a ScrollView so parts scroll within the max-height cap
            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.style.flexGrow = 1;
            _monsterPartsContainer.Add(scroll);

            var content = scroll.contentContainer;
            content.style.flexDirection = FlexDirection.Column;
            content.style.paddingLeft   = 6;
            content.style.paddingRight  = 6;

            foreach (var part in parts)
            {
                var container = new VisualElement();
                container.style.flexDirection     = FlexDirection.Column;
                container.style.borderBottomWidth = 1;
                container.style.borderBottomColor = new StyleColor(new Color(0.20f, 0.17f, 0.13f));
                content.Add(container);

                var bar = new PartHealthBar(container, part.partName, part.shellMax, part.fleshMax);
                bar.SetValues(part.shellCurrent, part.fleshCurrent, part.isRevealed, part.isExposed, part.isBroken);
                _partBars[part.partName] = bar;
            }
        }

        private void UpdateMonsterPartBars(MonsterPartState[] parts)
        {
            if (parts == null) return;
            foreach (var part in parts)
            {
                if (_partBars.TryGetValue(part.partName, out var bar))
                    bar.SetValues(part.shellCurrent, part.fleshCurrent, part.isRevealed, part.isExposed, part.isBroken);
            }
        }

        // ── Card Hand ─────────────────────────────────────────────
        private static ActionCardRegistrySO _cardRegistry;

        private static ActionCardSO LoadCard(string cardName)
        {
            if (_cardRegistry == null)
            {
                _cardRegistry = Resources.Load<ActionCardRegistrySO>("ActionCardRegistry");
                if (_cardRegistry == null)
                {
                    Debug.LogError("[CombatUI] ActionCardRegistry not found — " +
                                   "create it at Assets/_Game/Data/Resources/ActionCardRegistry.asset");
                    return null;
                }
            }
            return _cardRegistry.Get(cardName);
        }

        private void RefreshCardHand(CombatState state)
        {
            if (_handCards == null) return;

            // Record which card names were in the hand before this refresh so
            // we can skip the draw animation for cards that haven't changed.
            var previousNames = new HashSet<string>(_handCardElements.Keys);

            _handCards.Clear();
            _handCardElements.Clear();
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

            if (_deckCountLabel != null)
                _deckCountLabel.text = activeHunter.deckCardNames.Length.ToString();

            for (int i = 0; i < activeHunter.handCardNames.Length; i++)
            {
                string cardName = activeHunter.handCardNames[i];
                var card        = LoadCard(cardName);
                bool canPlay    = card == null || activeHunter.apRemaining >= (card.apCost - card.apRefund);

                VisualElement el;
                if (card != null)
                {
                    el = CardRenderer.BuildActionCard(card, isPlayable: canPlay);
                }
                else
                {
                    // Fallback for missing SO asset — plain label card
                    el = new VisualElement();
                    el.AddToClassList("card");
                    el.AddToClassList("stone-panel");
                    el.Add(new Label(cardName));
                }

                el.EnableInClassList("card--unplayable", !canPlay);
                string capturedName = cardName;
                el.RegisterCallback<ClickEvent>(_ => OnCardClicked(capturedName, el, canPlay));

                _handCards.Add(el);
                _handCardElements[cardName] = el;

                // Hover lift on all cards; draw animation only for newly drawn cards
                CardAnimationController.RegisterHoverLift(el);
                if (!previousNames.Contains(cardName))
                    _cardAnim?.AnimateDraw(el, delay: i * 0.05f);
            }
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

                // Restore movement range when no card is pending
                ClearMovementRange();
                var active = GetActiveHunter();
                if (active != null && _combatManager.CurrentPhase == CombatPhase.HunterPhase)
                    ShowMovementRange(active);

                RefreshGrid();
                return;
            }

            _pendingCardName = cardName;
            _selectedCardEl  = cardEl;
            cardEl.EnableInClassList("card--selected", true);

            // Hide movement highlights — attack targets take priority visually
            ClearMovementRange();

            Debug.Log($"[CombatUI] Card selected: {cardName} — click a grid cell to target");
            RefreshGrid();
        }

        // ── Deployment Helpers ────────────────────────────────────────
        private HunterCombatState GetNextUnplacedHunter()
        {
            var state = _combatManager?.CurrentState;
            if (state == null) return null;
            return System.Array.Find(state.hunters, h => h.isUnplaced && !h.isCollapsed);
        }

        private void ShowSpawnZone()
        {
            if (_spawnZones == null || _gridCells == null) return;
            foreach (var zone in _spawnZones)
            {
                if (zone == null) continue;
                foreach (var cell in zone.GetAllCells())
                {
                    if (cell.x < 0 || cell.x >= 22 || cell.y < 0 || cell.y >= 16) continue;
                    _gridCells[cell.x, cell.y]?.EnableInClassList("grid-cell--spawn", true);
                }
            }
        }

        private void ClearSpawnZone()
        {
            if (_gridCells == null) return;
            for (int x = 0; x < 22; x++)
            for (int y = 0; y < 16; y++)
                _gridCells[x, y]?.EnableInClassList("grid-cell--spawn", false);
        }

        private bool CellInAnySpawnZone(Vector2Int cell)
        {
            if (_spawnZones == null) return false;
            foreach (var z in _spawnZones)
                if (z != null && z.ContainsCell(cell)) return true;
            return false;
        }

        private void ShowMovementRange(HunterCombatState hunter)
        {
            _validMoveCells.Clear();
            if (hunter.hasMovedThisPhase) return;
            if (_gridManager == null || _gridCells == null) return;

            int effectiveMove = hunter.movement;
            int effectiveAcc  = hunter.accuracy;
            StatusEffectResolver.ApplyStatusPenalties(hunter, ref effectiveAcc, ref effectiveMove);

            var grid   = _gridManager as IGridManager;
            var origin = new Vector2Int(hunter.gridX, hunter.gridY);

            // BFS — only expand through passable cells so the monster blocks paths behind it
            var visited = new Dictionary<Vector2Int, int> { [origin] = 0 };
            var queue   = new Queue<(Vector2Int cell, int cost)>();
            queue.Enqueue((origin, 0));

            var dirs = new[] {
                new Vector2Int( 1,  0),
                new Vector2Int(-1,  0),
                new Vector2Int( 0,  1),
                new Vector2Int( 0, -1),
            };

            while (queue.Count > 0)
            {
                var (cell, cost) = queue.Dequeue();
                if (cost > 0) _validMoveCells.Add(cell);
                if (cost >= effectiveMove) continue;

                foreach (var dir in dirs)
                {
                    var next = cell + dir;
                    if (visited.ContainsKey(next))    continue;
                    if (!grid.IsInBounds(next))       continue;
                    if (grid.IsOccupied(next))        continue;
                    if (grid.IsDenied(next))          continue;

                    visited[next] = cost + 1;
                    queue.Enqueue((next, cost + 1));
                }
            }

            foreach (var pos in _validMoveCells)
            {
                var el = _gridCells[pos.x, pos.y];
                if (el != null) el.EnableInClassList("grid-cell--movable", true);
            }

            Debug.Log($"[CombatUI] Movement range: {_validMoveCells.Count} cells reachable from " +
                      $"({hunter.gridX},{hunter.gridY}) move={effectiveMove}");
        }

        private void ClearMovementRange()
        {
            foreach (var pos in _validMoveCells)
            {
                var el = _gridCells?[pos.x, pos.y];
                if (el != null) el.EnableInClassList("grid-cell--movable", false);
            }
            _validMoveCells.Clear();
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
                cell.EnableInClassList("grid-cell--movable",
                    _validMoveCells.Contains(new Vector2Int(x, y)));

                bool inSpawnZone = _combatManager?.CurrentPhase == CombatPhase.DeploymentPhase
                    && CellInAnySpawnZone(new Vector2Int(x, y));
                cell.EnableInClassList("grid-cell--spawn", inSpawnZone);

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
                if (!h.isCollapsed && !h.isUnplaced && h.gridX == x && h.gridY == y) return true;
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

            // ── Deployment mode ───────────────────────────────────────
            if (_combatManager?.CurrentPhase == CombatPhase.DeploymentPhase)
            {
                if (_deployingHunterId == null)
                {
                    Debug.Log("[CombatUI] No hunter awaiting placement");
                    return;
                }

                bool placed = _combatManager.TryPlaceHunter(
                    _deployingHunterId, new Vector2Int(x, y), _spawnZones);

                if (placed)
                {
                    ClearSpawnZone();
                    var next = GetNextUnplacedHunter();
                    _deployingHunterId = next?.hunterId;
                    if (next != null) ShowSpawnZone();
                    RefreshAll();
                }
                else
                {
                    Debug.Log($"[CombatUI] Cannot place here: ({x},{y}) — outside zone or occupied");
                }

                RefreshGrid();
                return;
            }

            // ── Card targeting mode ───────────────────────────────────
            if (_pendingCardName != null)
            {
                ResolveCardAtCell(x, y);
                return;
            }

            // ── Movement mode (Hunter Phase, no card selected) ────────
            if (_combatManager?.CurrentPhase == CombatPhase.HunterPhase)
            {
                var activeHunter = GetActiveHunter();
                if (activeHunter == null) { RefreshGrid(); return; }

                var destination = new Vector2Int(x, y);

                // Clicking own cell: skip move — stay in place, clear highlights
                if (destination.x == activeHunter.gridX && destination.y == activeHunter.gridY)
                {
                    _combatManager.SkipHunterMove(activeHunter.hunterId);
                    ClearMovementRange();
                    RefreshGrid();
                    return;
                }

                if (_validMoveCells.Contains(destination))
                {
                    bool moved = _combatManager.TryMoveHunter(activeHunter.hunterId, destination);
                    if (moved)
                    {
                        ClearMovementRange();
                        ShowMovementRange(activeHunter); // Recalculate from new position
                        RefreshAll();
                    }
                    else
                    {
                        Debug.Log($"[CombatUI] TryMoveHunter rejected: ({x},{y})");
                    }
                }
                else
                {
                    var state = _combatManager.CurrentState;
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

            // Capture card element before game state or DOM changes
            string playedName = _pendingCardName;
            VisualElement playedEl = null;
            if (_handCardElements.TryGetValue(playedName, out var found))
            {
                playedEl = found;
                _handCards.Remove(playedEl);
                _animationOverlay?.Add(playedEl);
            }

            var targetCell = new Vector2Int(x, y);
            bool success = _combatManager.TryPlayCard(activeHunter.hunterId, playedName, targetCell);

            if (success)
            {
                Debug.Log($"[CombatUI] Card played: {playedName} → ({x},{y})");
                if (playedEl != null && _cardAnim != null)
                    _cardAnim.AnimatePlay(playedEl, _animationOverlay);
                else if (playedEl != null)
                    _animationOverlay?.Remove(playedEl);
            }
            else
            {
                // Put the card back — TryPlayCard rejected (invalid target, etc.)
                if (playedEl != null)
                {
                    _animationOverlay?.Remove(playedEl);
                    _handCards.Add(playedEl);
                }
                Debug.Log($"[CombatUI] TryPlayCard failed: {playedName} → ({x},{y}) — invalid target or insufficient AP");
            }

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
