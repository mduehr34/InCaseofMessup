using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using MnM.Core.Data;
using MnM.Core.Systems;

namespace MnM.Core.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private UIDocument   _uiDocument;
        [SerializeField] private Sprite       _bgSprite;
        [SerializeField] private Sprite       _logoSprite;
        [SerializeField] private CampaignSO[] _allCampaigns;  // All CampaignSOs — used to resolve save file back to its SO

        private void OnEnable()
        {
            var root = _uiDocument.rootVisualElement;

            // Background art
            if (_bgSprite != null)
                root.Q("bg").style.backgroundImage = new StyleBackground(_bgSprite);
            else
                Debug.LogWarning("[MainMenu] _bgSprite not assigned on MainMenuController");

            // Title logo
            if (_logoSprite != null)
                root.Q("title-logo").style.backgroundImage = new StyleBackground(_logoSprite);
            else
                Debug.LogWarning("[MainMenu] _logoSprite not assigned on MainMenuController");

            // New Campaign → Campaign Select
            root.Q<Button>("btn-new-campaign").clicked += () =>
            {
                Debug.Log("[MainMenu] New Campaign clicked");
                SceneManager.LoadScene("CampaignSelect");
            };

            // Continue → load save → Settlement (via GameStateManager)
            var continueBtn = root.Q<Button>("btn-continue");
            bool hasSave = GameStateManager.Instance != null
                ? GameStateManager.Instance.HasSave
                : SaveManager.HasSave();
            continueBtn.SetEnabled(hasSave);
            continueBtn.clicked += () =>
            {
                var state = SaveManager.Load();
                if (state == null) { Debug.LogError("[MainMenu] Continue clicked but save failed to load"); return; }
                var so = System.Array.Find(_allCampaigns, c => c != null && c.name == state.campaignSoName);
                if (so == null) Debug.LogWarning($"[MainMenu] CampaignSO '{state.campaignSoName}' not found in _allCampaigns — settlement will have no campaign data");
                GameStateManager.Instance.LoadCampaign(state, so);
            };

            // Codex — stub
            root.Q<Button>("btn-codex").clicked += () =>
                Debug.Log("[MainMenu] Codex — implement in Stage 6-G");

            root.Q<Button>("btn-settings").clicked += () =>
                SceneManager.LoadScene("Settings");
            root.Q<Button>("btn-credits").clicked += () =>
                Debug.Log("[MainMenu] Credits — post-MVP");

            // Version label
            var versionLabel = root.Q<Label>("version-label");
            if (versionLabel != null)
                versionLabel.text = $"v{Application.version}";

            if (AudioManager.Instance != null)
                AudioManager.Instance.SetMusicContext(AudioContext.MainMenu);
        }
    }
}
