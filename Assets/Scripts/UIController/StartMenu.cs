using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    public void StartGame()
    {
        // Start the game
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
