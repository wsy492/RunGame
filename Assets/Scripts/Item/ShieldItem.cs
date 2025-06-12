using UnityEngine;

public class ShieldItem : MonoBehaviour
{
    public float shieldDuration = 3f; // Shield duration

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Give shield to all alive players
            var players = PlayerManager.GetAlivePlayers();
            foreach (var player in players)
            {
                var playerLife = player.GetComponent<PlayerLife>();
                if (playerLife != null)
                {
                    playerLife.ActivateShield(shieldDuration);
                }
            }
            Destroy(gameObject); // Destroy the item after picking up
        }
    }
}