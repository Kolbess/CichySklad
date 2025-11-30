using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathScreen : MonoBehaviour
{
    [SerializeField] private GameObject deathScreenPanel;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Start()
    {
        if (deathScreenPanel != null)
        {
            deathScreenPanel.SetActive(false);
        }
        
        if (DeathManager.Instance != null)
        {
            DeathManager.Instance.OnDeath += HandleDeath;
        }
    }

    private void OnDestroy()
    {
        if (DeathManager.Instance != null)
        {
            DeathManager.Instance.OnDeath -= HandleDeath;
        }
    }

    private void HandleDeath()
    {
        if (deathScreenPanel != null)
        {
            deathScreenPanel.SetActive(true);
        }
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f; // Resume time before changing scenes
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
