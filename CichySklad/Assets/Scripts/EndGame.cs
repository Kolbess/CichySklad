using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Tiny end-of-game screen controller: its button returns the player to the main menu.
/// </summary>
public class EndGame : MonoBehaviour
{
    [Tooltip("Scene loaded when the end-game button is pressed.")]
    [SerializeField]
    private string _menuSceneName = "MainMenu";

    // Name kept as-is: this is wired to a UI Button OnClick (UnityEvents bind by method name).
    public void EndGameB() => SceneManager.LoadScene(_menuSceneName);
}
