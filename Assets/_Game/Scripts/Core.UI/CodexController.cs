using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using MnM.Core.Systems;
using MnM.Core.Data;

namespace MnM.Core.UI
{
    public class CodexController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private CampaignSO _campaignSO;

        private VisualElement _root;
        private ScrollView    _list;
        private Label         _detailTitle;
        private Label         _detailMeta;
        private Label         _detailBody;
        private string        _activeTab = "monsters";

        private void OnEnable()
        {
            _root        = _uiDocument.rootVisualElement;
            _list        = _root.Q<ScrollView>("codex-list");
            _detailTitle = _root.Q<Label>("detail-title");
            _detailMeta  = _root.Q<Label>("detail-meta");
            _detailBody  = _root.Q<Label>("detail-body");

            WireTabs();
            BuildMonstersTab(); // Default tab
        }

        private void WireTabs()
        {
            _root.Q<Button>("tab-monsters").clicked  += () => SwitchTab("monsters");
            _root.Q<Button>("tab-artifacts").clicked += () => SwitchTab("artifacts");
            _root.Q<Button>("tab-records").clicked   += () => SwitchTab("records");
            _root.Q<Button>("btn-close").clicked     += OnClose;
        }

        private void SwitchTab(string tab)
        {
            _activeTab = tab;

            foreach (var pair in new[] {
                ("tab-monsters",  "monsters"),
                ("tab-artifacts", "artifacts"),
                ("tab-records",   "records") })
            {
                _root.Q<Button>(pair.Item1)?.EnableInClassList("tab-btn--active", pair.Item2 == tab);
            }

            ClearDetail();
            switch (tab)
            {
                case "monsters":  BuildMonstersTab();  break;
                case "artifacts": BuildArtifactsTab(); break;
                case "records":   BuildRecordsTab();   break;
            }
        }

        // ── Monsters Tab ─────────────────────────────────────────
        private void BuildMonstersTab()
        {
            _list.Clear();
            if (_campaignSO?.monsterRoster == null) return;

            var state = GameStateManager.Instance?.CampaignState;

            foreach (var monster in _campaignSO.monsterRoster)
            {
                if (monster == null) continue;

                // A monster is "known" if its name appears anywhere in the chronicle log
                bool isKnown = state?.chronicleLog?.Any(
                    entry => entry.Contains(monster.monsterName)) ?? false;

                var entry = new VisualElement();
                entry.AddToClassList("codex-entry");
                if (!isKnown) entry.AddToClassList("codex-entry--locked");

                var nameLabel = new Label(isKnown ? monster.monsterName : "???");
                nameLabel.AddToClassList("codex-entry-name");
                entry.Add(nameLabel);

                var tierTag = new Label($"Tier {monster.materialTier}");
                tierTag.AddToClassList("codex-entry-tag");
                if (!isKnown) tierTag.text = "???";
                entry.Add(tierTag);

                if (isKnown)
                {
                    var monsterRef = monster;
                    entry.RegisterCallback<ClickEvent>(_ => ShowMonsterDetail(monsterRef));
                }

                _list.Add(entry);
            }
        }

        private void ShowMonsterDetail(MonsterSO monster)
        {
            _detailTitle.text = monster.monsterName;
            _detailMeta.text  = $"Material Tier {monster.materialTier}";

            var woundLocationNames = new System.Collections.Generic.List<string>();
            if (monster.standardWoundDeck != null)
                foreach (var loc in monster.standardWoundDeck)
                    if (loc != null) woundLocationNames.Add(loc.locationName);

            var facingLines = new System.Collections.Generic.List<string>();
            if (monster.facingBonuses != null)
                foreach (var fb in monster.facingBonuses)
                    facingLines.Add($"{fb.arc} +{fb.accuracyModifier} accuracy");

            var weaknesses  = monster.weaknesses?.Select(e => e.ToString()) ?? new[] { "None" };
            var resistances = monster.resistances?.Select(e => e.ToString()) ?? new[] { "None" };

            _detailBody.text =
                $"{monster.animalBasis}\n\n" +
                $"Combat: {monster.combatEmotion}\n\n" +
                $"Wound locations: {string.Join(", ", woundLocationNames)}\n\n" +
                $"Facing: {string.Join(" | ", facingLines)}\n\n" +
                $"Weaknesses: {string.Join(", ", weaknesses)}\n" +
                $"Resistances: {string.Join(", ", resistances)}";
        }

        // ── Artifacts Tab ─────────────────────────────────────────
        private void BuildArtifactsTab()
        {
            _list.Clear();
            var state = GameStateManager.Instance?.CampaignState;
            if (state == null) return;

            if (state.unlockedArtifactIds == null || state.unlockedArtifactIds.Length == 0)
            {
                var none = new Label("No artifacts discovered yet.");
                none.AddToClassList("chronicle-entry");
                _list.Add(none);
                return;
            }

            foreach (var artifactId in state.unlockedArtifactIds)
            {
                var artifact = Resources.Load<ArtifactSO>($"Data/Artifacts/{artifactId}");
                var entry    = new VisualElement();
                entry.AddToClassList("codex-entry");

                var nameLabel = new Label(artifact != null ? artifact.artifactName : artifactId);
                nameLabel.AddToClassList("codex-entry-name");
                entry.Add(nameLabel);

                if (artifact != null)
                {
                    var artRef = artifact;
                    entry.RegisterCallback<ClickEvent>(_ => ShowArtifactDetail(artRef));
                }

                _list.Add(entry);
            }
        }

        private void ShowArtifactDetail(ArtifactSO artifact)
        {
            _detailTitle.text = artifact.artifactName;
            _detailMeta.text  = artifact.codexCategory.ToString();
            _detailBody.text  = artifact.loreText;
        }

        // ── Settlement Records Tab ────────────────────────────────
        private void BuildRecordsTab()
        {
            _list.Clear();
            var state = GameStateManager.Instance?.CampaignState;
            if (state == null || state.chronicleLog == null) return;

            foreach (var entry in state.chronicleLog)
            {
                // Entries starting with "---" are year dividers
                if (entry.StartsWith("---"))
                {
                    var divider = new Label(entry.Replace("---", "").Trim());
                    divider.AddToClassList("chronicle-divider");
                    _list.Add(divider);
                }
                else
                {
                    var logEntry = new Label(entry);
                    logEntry.AddToClassList("chronicle-entry");
                    _list.Add(logEntry);
                }
            }

            // Show campaign summary in the detail panel
            _detailTitle.text = "Settlement Record";
            _detailMeta.text  = $"Year {state.currentYear} — {state.characters?.Length ?? 0} active settlers";
            _detailBody.text  =
                $"This is the record of our settlement.\n\n" +
                $"Active settlers: {state.characters?.Length ?? 0}\n" +
                $"Retired: {state.retiredCharacters?.Length ?? 0}\n" +
                $"Crafters built: {state.builtCrafterNames?.Length ?? 0}\n" +
                $"Innovations adopted: {state.adoptedInnovationIds?.Length ?? 0}\n" +
                $"Artifacts discovered: {state.unlockedArtifactIds?.Length ?? 0}\n" +
                $"Events witnessed: {state.resolvedEventIds?.Length ?? 0}";
        }

        private void ClearDetail()
        {
            _detailTitle.text = "Select an entry";
            _detailMeta.text  = "";
            _detailBody.text  = "";
        }

        private void OnClose()
        {
            // Codex was loaded additively — unload this scene only
            SceneManager.UnloadSceneAsync("Codex");
        }
    }
}
