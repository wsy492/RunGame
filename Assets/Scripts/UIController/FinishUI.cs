using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FinishScene : MonoBehaviour
{
    public Text scoreText;
    public Text cherryText;

    void Start()
    {
        // Display final score and cherry count
        if (scoreText != null)
            scoreText.text = "Score: " + Score.totalScore;
        if (cherryText != null)
            cherryText.text = CherryManager.totalCherries.ToString();
    }


    public void RestartGame()
    {
        Physics2D.gravity = new Vector2(0, -9.81f); // Restore default gravity
        Score.totalScore = 0; // Reset score
        CherryManager.totalCherries = 0; // Reset cherry count
        SceneManager.LoadScene("Game");
    }
    public void ExitToStart()
    {
        Physics2D.gravity = new Vector2(0, -9.81f); // Restore default gravity
        Score.totalScore = 0; // Reset score
        CherryManager.totalCherries = 0; // Reset cherry count
        SceneManager.LoadScene("Start");
    }
}