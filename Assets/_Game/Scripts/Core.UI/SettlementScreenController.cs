using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using MnM.Core.Systems;
using MnM.Core.Data;

namespace MnM.Core.UI
{
    public class SettlementScreenController : MonoBehaviour
    {
        [SerializeField] private UIDocument  _uiDocument;
        [SerializeField] private CampaignSO  _campaignSO;

        private VisualElement  _root;
        private VisualElement  _tabContent;
        private SettlementManager _settlement;
        private string         _activeTab = "characters";

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

            _tabContent = _root.Q<ScrollView>("tab-content");

            WireButtons();
            RefreshEraHeader();
            RefreshResourceSummary();
            BuildCharactersTab();  // Default tab

            // Apply any pending hunt result
            if (state.pendingHuntResult.monsterName != null)
            {
                _settlement.ApplyHuntResults(state.pendingHuntResult);
                RefreshEraHeader();
                RefreshResourceSummary();
            }
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

        // ── Crafters Tab — Stub ───────────────────────────────────
        private void BuildCraftersTab()
        {
            _tabContent.Clear();
            var stub = new Label("Crafters tab — implemented Session 6-D");
            stub.AddToClassList("proficiency-label");
            _tabContent.Add(stub);
            Debug.Log("[Settlement] Crafters tab stub — implement 6-D");
        }

        // ── Innovations Tab — Stub ────────────────────────────────
        private void BuildInnovationsTab()
        {
            _tabContent.Clear();
            var stub = new Label("Innovations tab — implemented Session 6-D");
            stub.AddToClassList("proficiency-label");
            _tabContent.Add(stub);
            Debug.Log("[Settlement] Innovations tab stub — implement 6-D");
        }

        // ── Action Bar Handlers — Stubs ───────────────────────────
        private void OpenGearGrid(string characterId)
        {
            Debug.Log($"[Settlement] Open Gear Grid for: {characterId} — implement 6-E");
        }

        private void OnCodexClicked()
        {
            Debug.Log("[Settlement] Codex — implement 6-G");
        }

        private void OnHuntClicked()
        {
            Debug.Log("[Settlement] Hunt selection — implement 6-F");
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
            RefreshEraHeader();
            RefreshResourceSummary();
            BuildCharactersTab();
            Debug.Log($"[Settlement] Year advanced to {state.currentYear}");
        }
    }
}
