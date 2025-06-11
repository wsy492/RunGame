using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishScene : MonoBehaviour
{
    public void RestartGame()
    {
        Physics2D.gravity = new Vector2(0, -9.81f); // 恢复默认重力
        SceneManager.LoadScene("Game");
    }
    public void ExitToStart()
    {
        Physics2D.gravity = new Vector2(0, -9.81f); // 恢复默认重力
        SceneManager.LoadScene("Start");
    }
}