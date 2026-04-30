using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using MnM.Core.Systems;

namespace MnM.Core.UI
{
    public class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;

        private VisualElement _pauseBg;
        private bool          _isPaused;
        private bool          _initialized;

        private const string BackSceneKey    = "settings_back_scene";
        private const string ReturnPausedKey = "settings_return_paused";

        private void Awake()
        {
            Debug.Log($"[Pause] Awake in {SceneManager.GetActiveScene().name}");
        }

        private void Update()
        {
            if (!_initialized)
            {
                TryInit();
                return;
            }

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                SetPause(!_isPaused);
        }

        private void TryInit()
        {
            if (_uiDocument == null)
            {
                Debug.LogError("[Pause] _uiDocument is null — check Inspector assignment");
                return;
            }

            var root = _uiDocument.rootVisualElement;
            if (root == null)
            {
                // UIDocument not ready yet — will retry next frame
                return;
            }

            _pauseBg = root.Q("pause-bg");
            if (_pauseBg == null)
            {
                // Log once only (first frame root exists but element missing)
                Debug.LogError($"[Pause] Q(\"pause-bg\") returned null. " +
                               $"Root child count: {root.childCount}. " +
                               $"UXML: {_uiDocument.visualTreeAsset?.name ?? "NULL"}");
                return;
            }

            // Own initial hidden state entirely in C#
            _pauseBg.style.display = DisplayStyle.None;

            var saveBtn = root.Q<Button>("btn-save");
            if (SceneManager.GetActiveScene().name == "CombatScene")
                saveBtn.text = "SAVE & QUIT TO SETTLEMENT";

            root.Q<Button>("btn-resume")  ?.RegisterCallback<ClickEvent>(_ => SetPause(false));
            root.Q<Button>("btn-settings")?.RegisterCallback<ClickEvent>(_ => OpenSettings());
            saveBtn                       ?.RegisterCallback<ClickEvent>(_ => SaveAndClose());
            root.Q<Button>("btn-menu")    ?.RegisterCallback<ClickEvent>(_ => ReturnToMenu());

            _initialized = true;
            Debug.Log($"[Pause] Ready in {SceneManager.GetActiveScene().name}");

            // If returning from Settings, restore the paused overlay immediately
            if (PlayerPrefs.GetInt(ReturnPausedKey, 0) == 1)
            {
                PlayerPrefs.DeleteKey(ReturnPausedKey);
                SetPause(true);
            }
        }

        public void SetPause(bool paused)
        {
            _isPaused              = paused;
            Time.timeScale         = paused ? 0f : 1f;
            _pauseBg.style.display = paused ? DisplayStyle.Flex : DisplayStyle.None;
            Debug.Log($"[Pause] {(paused ? "Paused" : "Resumed")}");
        }

        private void OpenSettings()
        {
            PlayerPrefs.SetString(BackSceneKey,    SceneManager.GetActiveScene().name);
            PlayerPrefs.SetInt(ReturnPausedKey,    1);
            PlayerPrefs.Save();
            Time.timeScale = 1f;
            SceneTransitionManager.Instance.LoadScene("Settings");
        }

        private void SaveAndClose()
        {
            var state = GameStateManager.Instance?.CampaignState;
            if (state != null)
                SaveManager.Save(state);
            else
                Debug.LogWarning("[Pause] SaveAndClose: no CampaignState");

            Time.timeScale = 1f;

            if (SceneManager.GetActiveScene().name == "CombatScene")
                SceneTransitionManager.Instance.LoadScene("Settlement");
            else
                SetPause(false);
        }

        private void ReturnToMenu()
        {
            Time.timeScale = 1f;
            SceneTransitionManager.Instance.LoadScene("MainMenu");
        }
    }
}
