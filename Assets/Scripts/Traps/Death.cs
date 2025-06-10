using UnityEngine;

public class DifficultyManager_IncrementsDifficulty_WithRooms : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // 触发玩家死亡逻辑
            collision.gameObject.GetComponent<PlayerLife>().Die();
        }
    }
}