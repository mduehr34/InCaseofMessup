using UnityEngine;
using MnM.Core.Systems;

// Add this to the FIRST scene (MainMenu) only.
// GameStateManager persists from there via DontDestroyOnLoad.
public class GameStateManagerBootstrap : MonoBehaviour
{
    [SerializeField] private GameObject _gameStateManagerPrefab;

    private void Awake()
    {
        if (GameStateManager.Instance == null)
        {
            if (_gameStateManagerPrefab != null)
                Instantiate(_gameStateManagerPrefab);
            else
            {
                var go = new GameObject("GameStateManager");
                go.AddComponent<GameStateManager>();
            }
        }
    }
}
