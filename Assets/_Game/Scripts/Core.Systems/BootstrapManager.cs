using UnityEngine;
using UnityEngine.SceneManagement;

namespace MnM.Core.Systems
{
    /// <summary>
    /// Lives only in Bootstrap.unity (scene index 0). Loads MainMenu after all
    /// DontDestroyOnLoad managers have initialised. SceneTransitionManager handles
    /// the initial fade-in from black, so no transition wrapper is used here.
    /// </summary>
    public class BootstrapManager : MonoBehaviour
    {
        private void Start()
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
