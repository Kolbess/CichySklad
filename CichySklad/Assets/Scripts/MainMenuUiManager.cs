using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUiManager : MonoBehaviour
{
    private AudioSource _audioSource;
    [Header("Scene Configuration")]
    [SerializeField] private string gameSceneName = "Storage";

    [Header("UI Elements")]
    [SerializeField] private GameObject instructionsPanel;
    [SerializeField] private GameObject mainMenuButtons;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        // Ensure instructions are hidden at start
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
        }
        if (mainMenuButtons != null)
        {
            mainMenuButtons.SetActive(true);
        }
    }
    
    public void PlaySound()
    {
        _audioSource.Play();
    }

    public void StartGame()
    {
        Debug.Log($"Loading scene: {gameSceneName}");
        SceneManager.LoadScene(gameSceneName);
    }

    public void ToggleInstructions()
    {
        if (instructionsPanel != null)
        {
            bool isActive = instructionsPanel.activeSelf;
            instructionsPanel.SetActive(!isActive);

            if (mainMenuButtons != null)
            {
                mainMenuButtons.SetActive(isActive); // If instructions are active, buttons should be inactive, and vice versa
            }
        }
        else
        {
            Debug.LogWarning("Instructions Panel is not assigned in the Inspector!");
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}
