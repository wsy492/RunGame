using UnityEngine;

public class CherryItem : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the collided object is a player
        if (collision.CompareTag("Player"))
        {
            // Increase Cherry count
            CherryManager.totalCherries++;

            // Destroy the current Cherry
            Destroy(gameObject);
        }
    }
}