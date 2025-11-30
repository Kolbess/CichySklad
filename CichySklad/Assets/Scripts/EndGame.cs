using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGame : MonoBehaviour
{
    public void EndGameB()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
