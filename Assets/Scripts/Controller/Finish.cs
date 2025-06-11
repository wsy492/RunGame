using UnityEngine;
using UnityEngine.SceneManagement;

public class Finish : MonoBehaviour
{
    private bool isFinished = false;

    void Update()
    {
        // 只有在游戏开始后才检测是否还有存活的小人
        if (PlayerManager.Instance.isGameStarted && !isFinished && PlayerManager.GetAlivePlayers().Count == 0)
        {
            isFinished = true;
            GameOver();
        }
    }

    private void GameOver()
    {
        // Debug.Log("游戏结束：所有小人都死亡！");
        SceneManager.LoadScene("Finish");

    }
}