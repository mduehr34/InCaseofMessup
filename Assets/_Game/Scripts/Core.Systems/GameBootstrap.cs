using UnityEngine;
using UnityEngine.SceneManagement;

namespace MnM.Core.Systems
{
    /// <summary>
    /// Ensures critical managers exist regardless of which scene the Editor enters
    /// play mode from. In a built game Bootstrap.unity (index 0) always runs first,
    /// so this only fires as a safety net during development.
    /// </summary>
    public static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            // In a build Bootstrap.unity is always index 0 — real managers load from there
            if (SceneManager.GetActiveScene().name == "Bootstrap") return;
            // Safety check: already initialised
            if (GameStateManager.Instance != null) return;

            string startScene = SceneManager.GetActiveScene().name;
            Debug.Log($"[GameBootstrap] Entered play mode from '{startScene}' — " +
                      "creating fallback managers for Editor testing.");

            // GameStateManager
            var gsmGO = new GameObject("GameStateManager [dev-fallback]");
            Object.DontDestroyOnLoad(gsmGO);
            gsmGO.AddComponent<GameStateManager>();

            // AudioManager (no clips wired — silent in dev fallback, that's fine)
            var amGO = new GameObject("AudioManager [dev-fallback]");
            Object.DontDestroyOnLoad(amGO);
            amGO.AddComponent<AudioManager>();

            // SceneTransitionManager — creates a minimal overlay-less instance so
            // LoadScene() calls don't throw NullReferenceException. Transitions will
            // be instant (no UIDocument) but navigation will work.
            var stmGO = new GameObject("SceneTransitionManager [dev-fallback]");
            Object.DontDestroyOnLoad(stmGO);
            stmGO.AddComponent<SceneTransitionManager>();
        }
    }
}
