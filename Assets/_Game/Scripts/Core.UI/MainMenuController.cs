using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using MnM.Core.Systems;

namespace MnM.Core.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;

        private void OnEnable()
        {
            var root = _uiDocument.rootVisualElement;

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
                if (state != null)
                    GameStateManager.Instance.LoadCampaign(state);
                else
                    Debug.LogError("[MainMenu] Continue clicked but save failed to load");
            };

            // Codex — stub
            root.Q<Button>("btn-codex").clicked += () =>
                Debug.Log("[MainMenu] Codex — implement in Stage 6-G");

            // Settings / Credits — stubs
            root.Q<Button>("btn-settings").clicked += () =>
                Debug.Log("[MainMenu] Settings — post-MVP");
            root.Q<Button>("btn-credits").clicked += () =>
                Debug.Log("[MainMenu] Credits — post-MVP");

            // Version label
            var versionLabel = root.Q<Label>("version-label");
            if (versionLabel != null)
                versionLabel.text = $"v{Application.version}";
        }
    }
}
