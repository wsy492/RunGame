using UnityEngine;
using UnityEngine.SceneManagement;

public class Finish : MonoBehaviour
{
    private bool isFinished = false;

    void Update()
    {
        // Only check if there are any alive players after the game starts
        if (PlayerManager.Instance.isGameStarted && !isFinished && PlayerManager.GetAlivePlayers().Count == 0)
        {
            isFinished = true;
            GameOver();
        }
    }

    private void GameOver()
    {
        // Debug.Log("Game Over: All players are dead!");
        SceneManager.LoadScene("Finish");

    }
}