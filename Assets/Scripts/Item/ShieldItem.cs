using UnityEngine;

public class ShieldItem : MonoBehaviour
{
    public float shieldDuration = 3f; // 护盾持续时间

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 给所有存活小人加护盾
            var players = PlayerManager.GetAlivePlayers();
            foreach (var player in players)
            {
                var playerLife = player.GetComponent<PlayerLife>();
                if (playerLife != null)
                {
                    playerLife.ActivateShield(shieldDuration);
                }
            }
            Destroy(gameObject); // 吃掉道具
        }
    }
}