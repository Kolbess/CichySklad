using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

/// <summary>
/// Main-menu controller: starts the game, toggles the instructions panel, plays a UI sound, and
/// quits. All public methods are wired to menu buttons (names kept stable for UnityEvent binding).
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class MainMenuUiManger : MonoBehaviour
{
    [Header("Scene Configuration")]
    [Tooltip("Scene loaded when the player starts the game.")]
    [FormerlySerializedAs("gameSceneName")]
    [SerializeField]
    private string _gameSceneName = "Storage";

    [Header("UI Elements")]
    [Tooltip(
        "Instructions panel, hidden at start and toggled by the instructions button. Required."
    )]
    [FormerlySerializedAs("instructionsPanel")]
    [SerializeField]
    private GameObject _instructionsPanel;

    [Tooltip("Container of the main menu buttons, hidden while instructions are shown. Required.")]
    [FormerlySerializedAs("mainMenuButtons")]
    [SerializeField]
    private GameObject _mainMenuButtons;

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        Assert.IsNotNull(
            _instructionsPanel,
            $"[{nameof(MainMenuUiManger)}] Instructions panel unassigned on {name}!"
        );
        Assert.IsNotNull(
            _mainMenuButtons,
            $"[{nameof(MainMenuUiManger)}] Main menu buttons unassigned on {name}!"
        );
    }

    private void Start()
    {
        _instructionsPanel.SetActive(false);
        _mainMenuButtons.SetActive(true);
    }

    public void PlaySound() => _audioSource.Play();

    public void StartGame() => SceneManager.LoadScene(_gameSceneName);

    public void ToggleInstructions()
    {
        bool showInstructions = !_instructionsPanel.activeSelf;
        _instructionsPanel.SetActive(showInstructions);
        _mainMenuButtons.SetActive(!showInstructions);
    }

    public void QuitGame() => Application.Quit();
}
