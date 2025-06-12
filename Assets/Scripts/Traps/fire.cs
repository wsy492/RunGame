using UnityEngine;

public class Fire : MonoBehaviour
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