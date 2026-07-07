using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

/// <summary>
/// Shows the death/arrest screen when the global <see cref="GameEvents.OnArrest"/> event fires,
/// and exposes retry/quit buttons. Listens only to the event channel — nothing references it back.
/// </summary>
public class DeathManager : MonoBehaviour
{
    [Header("Scenes")]
    [Tooltip("Scene loaded by the 'quit to menu' button.")]
    [SerializeField]
    private string _menuSceneName = "MainMenu";

    [Tooltip("Scene reloaded by the 'try again' button.")]
    [SerializeField]
    private string _gameSceneName = "Storage";

    [Header("UI")]
    [Tooltip("Death screen root, shown on arrest. Required.")]
    [FormerlySerializedAs("deathScreen")]
    [SerializeField]
    private GameObject _deathScreen;

    private void Awake()
    {
        Assert.IsNotNull(
            _deathScreen,
            $"[{nameof(DeathManager)}] Death screen unassigned on {name}!"
        );
    }

    private void Start()
    {
        _deathScreen.SetActive(false);
    }

    private void OnEnable() => GameEvents.OnArrest += Die;

    private void OnDisable() => GameEvents.OnArrest -= Die;

    public void QuitToMenu() => SceneManager.LoadScene(_menuSceneName);

    public void TryAgain() => SceneManager.LoadScene(_gameSceneName);

    private void Die() => _deathScreen.SetActive(true);
}
