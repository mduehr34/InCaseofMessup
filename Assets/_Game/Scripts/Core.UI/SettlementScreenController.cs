using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using MnM.Core.Systems;
using MnM.Core.Data;

namespace MnM.Core.UI
{
    public class SettlementScreenController : MonoBehaviour
    {
        [SerializeField] private UIDocument       _uiDocument;
        [SerializeField] private CampaignSO       _campaignSO;
        [SerializeField] private VisualTreeAsset  _eventModalAsset;
        [SerializeField] private VisualTreeAsset  _gpModalAsset;
        [SerializeField] private VisualTreeAsset  _huntSelectModalAsset;

        private VisualElement      _root;
        private VisualElement      _tabContent;
        private SettlementManager  _settlement;
        private string             _activeTab = "characters";
        private VisualElement      _activeModal = null;
        private InnovationSO[]     _drawnInnovations = null;
        private HuntSelectionModal _huntModal = new HuntSelectionModal();

        private void OnEnable()
        {
            _root = _uiDocument.rootVisualElement;

            // Init SettlementManager with current campaign state
            var state = GameStateManager.Instance?.CampaignState;
            if (state == null)
            {
                Debug.LogError("[Settlement] No CampaignState in GameStateManager");
                return;
            }

            _settlement = new SettlementManager();
            _settlement.Initialize(state, _campaignSO);
            AudioManager.Instance?.SetContextForYear(state.currentYear);

            _tabContent = _root.Q<ScrollView>("tab-content");

            WireButtons();
            RefreshEraHeader();
            RefreshResourceSummary();
            RefreshSettlementScene();
            BuildCharactersTab();  // Default tab

            // Apply any pending hunt result
            if (state.pendingHuntResult.monsterName != null)
            {
                _settlement.ApplyHuntResults(state.pendingHuntResult);
                RefreshEraHeader();
                RefreshResourceSummary();
            }

            // Auto-fire Chronicle Event and any pending Guiding Principals
            CheckAndFireChronicleEvent();
            if (GameStateManager.Instance.CampaignState.activeGuidingPrincipalIds.Length > 0)
                CheckAndFireGuidingPrincipal();
        }

        // ── Era Header ────────────────────────────────────────────
        private void RefreshEraHeader()
        {
            var state = GameStateManager.Instance.CampaignState;
            _root.Q<Label>("year-label").text = $"Year {state.currentYear}";
            _root.Q<Label>("era-name").text   = GetEraName(state.currentYear);
        }

        private void RefreshResourceSummary()
        {
            var state    = GameStateManager.Instance.CampaignState;
            var topThree = state.resources
                .OrderByDescending(r => r.amount)
                .Take(3)
                .Select(r => $"{r.resourceName}: {r.amount}");
            var summary = string.Join("  |  ", topThree);
            var label   = _root.Q<Label>("resource-summary");
            if (label != null) label.text = summary;
        }

        private static string GetEraName(int year) => year switch
        {
            <= 3  => "The Ember",
            <= 8  => "The Refuge",
            <= 14 => "The Outpost",
            <= 22 => "The Settlement",
            _     => "The Stronghold",
        };

        // ── Tab Wiring ────────────────────────────────────────────
        private void WireButtons()
        {
            _root.Q<Button>("tab-characters").clicked  += () => SwitchTab("characters");
            _root.Q<Button>("tab-crafters").clicked    += () => SwitchTab("crafters");
            _root.Q<Button>("tab-innovations").clicked += () => SwitchTab("innovations");

            _root.Q<Button>("btn-codex").clicked    += OnCodexClicked;
            _root.Q<Button>("btn-hunt").clicked     += OnHuntClicked;
            _root.Q<Button>("btn-craft").clicked    += OnCraftClicked;
            _root.Q<Button>("btn-end-year").clicked += OnEndYearClicked;
        }

        private void SwitchTab(string tabName)
        {
            _activeTab = tabName;

            // Update tab button active states
            foreach (var btn in new[] { "tab-characters", "tab-crafters", "tab-innovations" })
            {
                var b = _root.Q<Button>(btn);
                b?.EnableInClassList("tab-btn--active",
                    btn == $"tab-{tabName}");
            }

            switch (tabName)
            {
                case "characters":  BuildCharactersTab();  break;
                case "crafters":    BuildCraftersTab();    break;
                case "innovations": BuildInnovationsTab(); break;
            }
        }

        // ── Characters Tab ────────────────────────────────────────
        private void BuildCharactersTab()
        {
            _tabContent.Clear();
            var state = GameStateManager.Instance.CampaignState;

            foreach (var ch in state.characters.Where(c => !c.isRetired))
            {
                var row = new VisualElement();
                row.AddToClassList("character-row");
                row.AddToClassList("stone-panel");

                // Name
                var nameLabel = new Label(ch.characterName);
                nameLabel.AddToClassList("character-name");
                row.Add(nameLabel);

                // Sex / build
                var buildLabel = new Label($"{ch.sex} · {ch.bodyBuild}");
                buildLabel.AddToClassList("proficiency-label");
                row.Add(buildLabel);

                // Injuries
                if (ch.injuryCardNames.Length > 0)
                {
                    var injLabel = new Label($"⚑ {ch.injuryCardNames.Length} injur{(ch.injuryCardNames.Length == 1 ? "y" : "ies")}");
                    injLabel.AddToClassList("injury-indicator");
                    row.Add(injLabel);
                }

                // Proficiency
                for (int i = 0; i < ch.proficiencyWeaponTypes.Length; i++)
                {
                    var pLabel = new Label(
                        $"{ch.proficiencyWeaponTypes[i]}: T{ch.proficiencyTiers[i]}");
                    pLabel.AddToClassList("proficiency-label");
                    row.Add(pLabel);
                }

                // Hunt count
                var huntLabel = new Label($"Hunts: {ch.huntCount}");
                huntLabel.AddToClassList("hunt-count-label");
                row.Add(huntLabel);

                // Loadout button (opens Gear Grid — Session 6-E)
                string capturedId = ch.characterId;
                var loadoutBtn = new Button(() => OpenGearGrid(capturedId))
                    { text = "LOADOUT" };
                loadoutBtn.AddToClassList("small-btn");
                row.Add(loadoutBtn);

                _tabContent.Add(row);
            }

            // Show retired count
            int retiredCount = state.retiredCharacters?.Length ?? 0;
            if (retiredCount > 0)
            {
                var retiredLabel = new Label($"Retired: {retiredCount}");
                retiredLabel.AddToClassList("proficiency-label");
                _tabContent.Add(retiredLabel);
            }
        }

        // ── Crafters Tab ──────────────────────────────────────────
        private void BuildCraftersTab()
        {
            _tabContent.Clear();
            var state = GameStateManager.Instance.CampaignState;
            if (_campaignSO?.crafterPool == null)
            {
                _tabContent.Add(new Label("No crafters defined in CampaignSO"));
                return;
            }

            foreach (var crafter in _campaignSO.crafterPool)
            {
                if (crafter == null) continue;
                bool isBuilt = System.Array.IndexOf(state.builtCrafterNames, crafter.crafterName) >= 0;

                var row = new VisualElement();
                row.AddToClassList("character-row");
                row.AddToClassList(isBuilt ? "stone-panel--raised" : "stone-panel");

                var nameLabel = new Label(crafter.crafterName);
                nameLabel.AddToClassList("character-name");
                if (!isBuilt) nameLabel.style.color = new StyleColor(new Color(0.54f, 0.50f, 0.44f));
                row.Add(nameLabel);

                if (isBuilt)
                {
                    var builtTag = new Label("BUILT");
                    builtTag.AddToClassList("status-badge");
                    builtTag.style.color = new StyleColor(new Color(0.40f, 0.72f, 0.40f));
                    row.Add(builtTag);

                    var recipeLabel = new Label($"{crafter.recipeList?.Length ?? 0} recipes");
                    recipeLabel.AddToClassList("proficiency-label");
                    row.Add(recipeLabel);
                }
                else
                {
                    var costLabel = new Label(BuildCostString(crafter));
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
                parts.Add($"{amt}\u00d7 {crafter.unlockCost[i].resourceName}");
            }
            return string.Join(", ", parts);
        }

        private void OnUnlockCrafter(CrafterSO crafter)
        {
            bool success = _settlement.TryUnlockCrafter(crafter);
            if (success)
            {
                Debug.Log($"[Settlement] Unlocked: {crafter.crafterName}");
                BuildCraftersTab();
                RefreshResourceSummary();
                RefreshSettlementScene();
            }
        }

        // ── Innovations Tab ───────────────────────────────────────
        private void BuildInnovationsTab()
        {
            _tabContent.Clear();
            var state = GameStateManager.Instance.CampaignState;

            var adoptedHeader = new Label("ADOPTED INNOVATIONS");
            adoptedHeader.AddToClassList("stone-panel__header");
            _tabContent.Add(adoptedHeader);

            if (state.adoptedInnovationIds.Length == 0)
            {
                _tabContent.Add(new Label("None yet"));
            }
            else
            {
                foreach (var id in state.adoptedInnovationIds)
                {
                    var label = new Label($"\u2022 {id}");
                    label.AddToClassList("proficiency-label");
                    _tabContent.Add(label);
                }
            }

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
            _drawnInnovations = null; // Reset — each settlement phase draws fresh
            BuildInnovationsTab();
            Debug.Log($"[Settlement] Adopted: {innovation.innovationName}");
        }

        // ── Action Bar Handlers — Stubs ───────────────────────────
        private void OpenGearGrid(string characterId)
        {
            var state = GameStateManager.Instance.CampaignState;
            var ch    = System.Array.Find(state.characters, c => c.characterId == characterId);
            if (ch == null)
            {
                Debug.LogError($"[Settlement] OpenGearGrid: character not found: {characterId}");
                return;
            }
            Debug.Log($"[Settlement] Opening gear grid for {ch.characterName}");
            GameStateManager.Instance.OpenGearGrid(characterId);
        }

        private void OnCodexClicked()
        {
            GameStateManager.Instance.OpenCodex();
        }

        private void OnHuntClicked()
        {
            if (_huntSelectModalAsset == null)
            {
                Debug.LogWarning("[Settlement] Hunt select modal UXML not assigned");
                return;
            }
            _huntModal.Show(_root, _huntSelectModalAsset, _campaignSO);
        }

        private void OnCraftClicked()
        {
            Debug.Log("[Settlement] Craft — implement 6-D");
        }

        private void OnEndYearClicked()
        {
            var state = GameStateManager.Instance.CampaignState;
            _settlement.CheckAllRetirements();
            _settlement.AdvanceYear();
            _drawnInnovations = null; // Year boundary — innovations refresh
            RefreshEraHeader();
            RefreshResourceSummary();
            RefreshSettlementScene();
            BuildCharactersTab();
            Debug.Log($"[Settlement] Year advanced to {state.currentYear}");
        }

        // ── Settlement Scene ──────────────────────────────────────
        private void RefreshSettlementScene()
        {
            var scene = _root.Q<VisualElement>("settlement-scene");
            if (scene == null || _campaignSO?.crafterPool == null) return;

            scene.Clear();

            var state = GameStateManager.Instance.CampaignState;
            if (state.builtCrafterNames == null || state.builtCrafterNames.Length == 0)
            {
                var placeholder = new Label("SETTLEMENT");
                placeholder.AddToClassList("settlement-scene-placeholder");
                scene.Add(placeholder);
                return;
            }

            foreach (var crafter in _campaignSO.crafterPool)
            {
                if (crafter == null || crafter.structureSprite == null) continue;
                bool isBuilt = System.Array.IndexOf(
                    state.builtCrafterNames, crafter.crafterName) >= 0;
                if (!isBuilt) continue;

                var img = new UnityEngine.UIElements.Image { sprite = crafter.structureSprite };
                img.style.position = Position.Absolute;
                img.style.left     = crafter.settlementScenePosition.x;
                img.style.top      = crafter.settlementScenePosition.y;
                img.style.width    = crafter.structureSprite.texture.width;
                img.style.height   = crafter.structureSprite.texture.height;
                scene.Add(img);

                Debug.Log($"[Settlement] Structure placed: {crafter.crafterName} " +
                          $"at ({crafter.settlementScenePosition.x}, {crafter.settlementScenePosition.y})");
            }
        }

        // ── Chronicle Event Flow ──────────────────────────────────
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
            // TemplateContainer must stretch to fill root so the absolute-positioned
            // modal-overlay inside it covers the full screen.
            overlay.style.position = Position.Absolute;
            overlay.style.left     = 0;
            overlay.style.top      = 0;
            overlay.style.right    = 0;
            overlay.style.bottom   = 0;
            _root.Add(overlay);
            _activeModal = overlay;

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
                ackBtn.clicked += () =>
                {
                    _settlement.ResolveEvent(evt, -1);
                    CloseModal();
                    CheckAndFireChronicleEvent(); // Pick up next eligible event
                };
            }
            else
            {
                choicesEl.style.display = DisplayStyle.Flex;
                ackBtn.style.display    = DisplayStyle.None;

                var btnA = overlay.Q<Button>("btn-choice-a");
                var btnB = overlay.Q<Button>("btn-choice-b");

                if (evt.choices.Length > 0)
                {
                    btnA.text = $"{evt.choices[0].choiceLabel}: {evt.choices[0].outcomeText}";
                    btnA.clicked += () =>
                    {
                        _settlement.ResolveEvent(evt, 0);
                        CloseModal();
                        CheckAndFireGuidingPrincipal();
                        CheckAndFireChronicleEvent(); // Pick up next eligible event
                    };
                }

                if (evt.choices.Length > 1)
                {
                    btnB.text = $"{evt.choices[1].choiceLabel}: {evt.choices[1].outcomeText}";
                    btnB.clicked += () =>
                    {
                        _settlement.ResolveEvent(evt, 1);
                        CloseModal();
                        CheckAndFireGuidingPrincipal();
                        CheckAndFireChronicleEvent(); // Pick up next eligible event
                    };
                }
                else
                {
                    btnB.style.display = DisplayStyle.None;
                }
            }

            Debug.Log($"[Settlement] Showing event: {evt.eventId} \u2014 {evt.eventName}");
        }

        // ── Guiding Principal Flow ────────────────────────────────
        private void CheckAndFireGuidingPrincipal()
        {
            var state = GameStateManager.Instance.CampaignState;
            if (state.activeGuidingPrincipalIds.Length == 0) return;

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
            overlay.style.position = Position.Absolute;
            overlay.style.left     = 0;
            overlay.style.top      = 0;
            overlay.style.right    = 0;
            overlay.style.bottom   = 0;
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
            if (_campaignSO?.guidingPrincipals == null) return null;
            return System.Array.Find(_campaignSO.guidingPrincipals, gp => gp.principalId == id);
        }
    }
}
