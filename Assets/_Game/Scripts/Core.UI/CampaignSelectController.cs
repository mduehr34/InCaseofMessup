using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using MnM.Core.Data;
using MnM.Core.Systems;

namespace MnM.Core.UI
{
    public class CampaignSelectController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private CampaignSO _tutorialCampaign;
        [SerializeField] private CampaignSO _standardCampaign;
        [SerializeField] private Sprite     _tutorialArt;
        [SerializeField] private Sprite     _standardArt;

        private CampaignSO _selectedCampaign;
        private string     _selectedDifficulty = "Medium";
        private bool       _ironman            = false;

        private void OnEnable()
        {
            _selectedCampaign = _tutorialCampaign;

            var root = _uiDocument.rootVisualElement;

            // Populate card labels from SO data
            root.Q<Label>("card-tutorial-title").text = _tutorialCampaign != null
                ? $"{_tutorialCampaign.campaignName} ({_tutorialCampaign.campaignLengthYears}y)"
                : "TUTORIAL";
            root.Q<Label>("card-standard-title").text = _standardCampaign != null
                ? $"{_standardCampaign.campaignName} ({_standardCampaign.campaignLengthYears}y)"
                : "STANDARD";

            // Card art
            if (_tutorialArt != null)
                root.Q("card-tutorial-art").style.backgroundImage = new StyleBackground(_tutorialArt);
            if (_standardArt != null)
                root.Q("card-standard-art").style.backgroundImage = new StyleBackground(_standardArt);

            // Campaign card selection
            root.Q("card-tutorial").RegisterCallback<ClickEvent>(_ => SelectCampaign(root, _tutorialCampaign));
            root.Q("card-standard").RegisterCallback<ClickEvent>(_ => SelectCampaign(root, _standardCampaign));
            HighlightCard(root, "card-tutorial");

            // Difficulty locked to Medium for Tutorial on start
            root.Q("difficulty-group").SetEnabled(false);

            // Difficulty buttons
            root.Q<Button>("btn-easy")  .RegisterCallback<ClickEvent>(_ => SetDifficulty(root, "Easy"));
            root.Q<Button>("btn-medium").RegisterCallback<ClickEvent>(_ => SetDifficulty(root, "Medium"));
            root.Q<Button>("btn-hard")  .RegisterCallback<ClickEvent>(_ => SetDifficulty(root, "Hard"));

            // Ironman toggle
            root.Q<Toggle>("toggle-ironman").RegisterValueChangedCallback(evt => _ironman = evt.newValue);

            // Navigation
            root.Q<Button>("btn-back")   .RegisterCallback<ClickEvent>(_ => SceneManager.LoadScene("MainMenu"));
            root.Q<Button>("btn-confirm").RegisterCallback<ClickEvent>(_ => OnConfirm());
        }

        private void SelectCampaign(VisualElement root, CampaignSO campaign)
        {
            _selectedCampaign = campaign;
            bool isTutorial   = campaign == _tutorialCampaign;
            string cardId     = isTutorial ? "card-tutorial" : "card-standard";

            HighlightCard(root, cardId);

            // Difficulty locked to Medium for Tutorial
            root.Q("difficulty-group").SetEnabled(!isTutorial);
            if (isTutorial) SetDifficulty(root, "Medium");
        }

        private void HighlightCard(VisualElement root, string selectedId)
        {
            foreach (string id in new[] { "card-tutorial", "card-standard" })
            {
                var card = root.Q(id);
                if (id == selectedId) card.AddToClassList("campaign-card--selected");
                else                  card.RemoveFromClassList("campaign-card--selected");
            }
        }

        private void SetDifficulty(VisualElement root, string diff)
        {
            _selectedDifficulty = diff;
            foreach (string id in new[] { "btn-easy", "btn-medium", "btn-hard" })
            {
                var btn = root.Q<Button>(id);
                if (btn.text.Equals(diff.ToUpper()))
                    btn.AddToClassList("diff-btn--selected");
                else
                    btn.RemoveFromClassList("diff-btn--selected");
            }
        }

        private void OnConfirm()
        {
            if (_selectedCampaign == null)
            {
                Debug.LogWarning("[CampaignSelect] No campaign selected");
                return;
            }

            if (GameStateManager.Instance == null)
            {
                Debug.LogError("[CampaignSelect] GameStateManager not found. " +
                               "Enter play mode from MainMenu so GSM bootstraps.");
                return;
            }

            Debug.Log($"[CampaignSelect] Confirmed: {_selectedCampaign.campaignName} / " +
                      $"{_selectedDifficulty} / Ironman={_ironman}");
            GameStateManager.Instance.PrepareNewCampaign(_selectedCampaign, _selectedDifficulty, _ironman);
            SceneManager.LoadScene("CharacterCreation");
        }

    }
}
