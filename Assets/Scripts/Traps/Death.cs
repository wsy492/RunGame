using UnityEngine;

public class DifficultyManager_IncrementsDifficulty_WithRooms : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Trigger the logic for player death
            collision.gameObject.GetComponent<PlayerLife>().Die();
        }
    }
}