using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathManager : MonoBehaviour
{
    [SerializeField] private GameObject deathScreen;

    private void Start()
    {
        deathScreen.SetActive(false);
    }

    private void OnEnable()
    {
        EventSystem.OnArrest += Die;
    }

    private void OnDisable()
    {
        EventSystem.OnArrest -= Die;
    }

    public void QuitToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void TryAgain()
    {
        SceneManager.LoadScene("Storage");
    }

    private void Die()
    {
        Debug.Log("Arrested!");
        deathScreen.SetActive(true);
    }
}
