using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using MnM.Core.Systems;

namespace MnM.Core.UI
{
    public class SettingsController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private string     _backScene = "MainMenu";

        // PlayerPrefs keys — shared with AudioManager.ApplySavedVolumePrefs
        public const string KeyMaster     = "vol_master";
        public const string KeyMusic      = "vol_music";
        public const string KeySfx        = "vol_sfx";
        public const string KeyFullscreen = "fullscreen";

        private void Start()
        {
            // If opened from the pause menu, return there; otherwise return to MainMenu
            string backScene = PlayerPrefs.GetString("settings_back_scene", "MainMenu");
            if (!string.IsNullOrEmpty(backScene))
            {
                _backScene = backScene;
                PlayerPrefs.DeleteKey("settings_back_scene");
            }

            var root = _uiDocument.rootVisualElement;

            float master = PlayerPrefs.GetFloat(KeyMaster,     1f);
            float music  = PlayerPrefs.GetFloat(KeyMusic,      0.8f);
            float sfx    = PlayerPrefs.GetFloat(KeySfx,        1f);
            bool  fs     = PlayerPrefs.GetInt(KeyFullscreen,   1) == 1;

            SetupSlider(root, "slider-master", "val-master", master, v =>
            {
                PlayerPrefs.SetFloat(KeyMaster, v);
                AudioManager.Instance?.SetMasterVolume(v);
            });

            SetupSlider(root, "slider-music", "val-music", music, v =>
            {
                PlayerPrefs.SetFloat(KeyMusic, v);
                AudioManager.Instance?.SetMusicVolume(v);
            });

            SetupSlider(root, "slider-sfx", "val-sfx", sfx, v =>
            {
                PlayerPrefs.SetFloat(KeySfx, v);
                AudioManager.Instance?.SetSfxVolume(v);
            });

            var fsToggle = root.Q<Toggle>("toggle-fullscreen");
            fsToggle.value = fs;
            Screen.fullScreen = fs;
            fsToggle.RegisterValueChangedCallback(evt =>
            {
                Screen.fullScreen = evt.newValue;
                PlayerPrefs.SetInt(KeyFullscreen, evt.newValue ? 1 : 0);
                PlayerPrefs.Save();
            });

            root.Q<Button>("btn-back").RegisterCallback<ClickEvent>(_ =>
                SceneManager.LoadScene(_backScene));
        }

        private void SetupSlider(VisualElement root, string sliderId, string labelId,
                                  float initial, System.Action<float> onChange)
        {
            var slider = root.Q<Slider>(sliderId);
            var label  = root.Q<Label>(labelId);
            if (slider == null || label == null) return;

            slider.value = initial;
            label.text   = Mathf.RoundToInt(initial * 100).ToString();

            slider.RegisterValueChangedCallback(evt =>
            {
                label.text = Mathf.RoundToInt(evt.newValue * 100).ToString();
                onChange(evt.newValue);
                PlayerPrefs.Save();
            });
        }
    }
}
