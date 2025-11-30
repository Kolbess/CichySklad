using System;
using UnityEngine;

public class DeathManager : MonoBehaviour
{
    public static DeathManager Instance { get; private set; }

    public event Action OnDeath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Die()
    {
        Debug.Log("Player has died.");
        OnDeath?.Invoke();
        // Additional logic like pausing the game or playing a sound can be added here
        Time.timeScale = 0f; // Pause the game
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            Die();
        }
    }
}
