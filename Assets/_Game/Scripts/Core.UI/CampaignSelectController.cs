using UnityEngine;
using UnityEngine.UIElements;
using MnM.Core.Systems;
using MnM.Core.Data;

namespace MnM.Core.UI
{
    public class CampaignSelectController : MonoBehaviour
    {
        [SerializeField] private UIDocument  _uiDocument;
        // Assign all available CampaignSO assets in the Inspector
        [SerializeField] private CampaignSO[] _availableCampaigns;
        [SerializeField] private CampaignSO   _tutorialCampaign;

        private CampaignSO    _selectedCampaign;
        private VisualElement _root;

        private void OnEnable()
        {
            _root = _uiDocument.rootVisualElement;

            // Back → Main Menu (via GameStateManager — also auto-saves if campaign active)
            _root.Q<Button>("btn-back").clicked += () =>
                GameStateManager.Instance.GoToMainMenu();

            _root.Q<Button>("btn-start-campaign").clicked += OnStartCampaign;

            _root.Q<Button>("btn-tutorial").clicked += () =>
            {
                if (_tutorialCampaign != null)
                    GameStateManager.Instance.StartNewCampaign(_tutorialCampaign);
                else
                    Debug.LogError("[CampaignSelect] Tutorial CampaignSO not assigned");
            };

            BuildCampaignList();

            // Pre-select first campaign
            if (_availableCampaigns != null && _availableCampaigns.Length > 0)
                SelectCampaign(_availableCampaigns[0]);
        }

        private void BuildCampaignList()
        {
            var list = _root.Q<VisualElement>("campaign-list");
            if (list == null || _availableCampaigns == null) return;
            list.Clear();

            foreach (var campaign in _availableCampaigns)
            {
                if (campaign == null) continue;
                list.Add(BuildCampaignRow(campaign));
            }
        }

        private VisualElement BuildCampaignRow(CampaignSO campaign)
        {
            var row = new VisualElement();
            row.AddToClassList("character-row");
            row.AddToClassList("stone-panel");

            var nameLabel = new Label(campaign.campaignName);
            nameLabel.AddToClassList("character-name");
            row.Add(nameLabel);

            var diffLabel = new Label(campaign.difficulty.ToString());
            diffLabel.AddToClassList("proficiency-label");
            row.Add(diffLabel);

            row.RegisterCallback<ClickEvent>(_ => SelectCampaign(campaign));
            return row;
        }

        private void SelectCampaign(CampaignSO campaign)
        {
            _selectedCampaign = campaign;

            _root.Q<Label>("campaign-name").text        = campaign.campaignName;
            _root.Q<Label>("campaign-difficulty").text  = $"Difficulty: {campaign.difficulty}";
            _root.Q<Label>("campaign-description").text = $"{campaign.campaignLengthYears}-year campaign";
            _root.Q<Label>("campaign-characters").text  = $"Starting characters: {campaign.startingCharacterCount}";
            _root.Q<Label>("campaign-length").text      = $"Ironman: {(campaign.ironmanMode ? "Yes" : "No")}";

            Debug.Log($"[CampaignSelect] Selected: {campaign.campaignName}");
        }

        private void OnStartCampaign()
        {
            if (_selectedCampaign == null)
            {
                Debug.LogWarning("[CampaignSelect] No campaign selected");
                return;
            }

            if (GameStateManager.Instance == null)
            {
                Debug.LogError("[CampaignSelect] GameStateManager not found. " +
                               "Always enter play mode from the MainMenu scene so GSM is bootstrapped.");
                return;
            }

            GameStateManager.Instance.StartNewCampaign(_selectedCampaign);
        }
    }
}
