using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace MnM.Core.Systems
{
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        [SerializeField] private UIDocument _uiDocument;

        private VisualElement _overlay;
        private bool          _isTransitioning;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            // Dev-fallback instance (GameBootstrap) has no UIDocument — skip overlay setup
            if (_uiDocument == null) return;
            _overlay = _uiDocument.rootVisualElement.Q("fade-overlay");
            if (_overlay == null) return;
            // Start fully black then fade in — reveals the first scene after Bootstrap loads it
            _overlay.style.display = DisplayStyle.Flex;
            _overlay.style.backgroundColor = new StyleColor(Color.black);
            StartCoroutine(FadeIn(0.4f));
        }

        // ── Public API ──────────────────────────────────────────────

        public void LoadScene(string sceneName)
        {
            if (_isTransitioning) return;
            StartCoroutine(TransitionTo(sceneName));
        }

        /// <summary>Slide a modal panel in from below. Caller pre-sets any initial state.</summary>
        public IEnumerator SlideIn(VisualElement panel, float duration = 0.25f)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float pct = Mathf.Lerp(100f, 0f, t / duration);
                panel.style.translate = new StyleTranslate(
                    new Translate(Length.Percent(0), Length.Percent(pct)));
                yield return null;
            }
            panel.style.translate = StyleKeyword.None;
        }

        /// <summary>Slide a modal panel back off the bottom. Caller removes the element afterward.</summary>
        public IEnumerator SlideOut(VisualElement panel, float duration = 0.2f)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float pct = Mathf.Lerp(0f, 100f, t / duration);
                panel.style.translate = new StyleTranslate(
                    new Translate(Length.Percent(0), Length.Percent(pct)));
                yield return null;
            }
        }

        // ── Private ──────────────────────────────────────────────────

        private IEnumerator TransitionTo(string sceneName)
        {
            _isTransitioning = true;
            yield return FadeOut(0.3f);
            SceneManager.LoadScene(sceneName);
            yield return null; // one frame for the scene to initialise
            yield return FadeIn(0.3f);
            _isTransitioning = false;
        }

        private IEnumerator FadeOut(float duration)
        {
            if (_overlay == null) yield break;
            _overlay.style.display = DisplayStyle.Flex;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(0f, 1f, t / duration);
                _overlay.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, alpha));
                yield return null;
            }
            _overlay.style.backgroundColor = new StyleColor(Color.black);
        }

        private IEnumerator FadeIn(float duration)
        {
            if (_overlay == null) yield break;
            _overlay.style.display = DisplayStyle.Flex;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(1f, 0f, t / duration);
                _overlay.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, alpha));
                yield return null;
            }
            _overlay.style.display = DisplayStyle.None;
        }
    }
}
