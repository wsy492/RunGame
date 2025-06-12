using UnityEngine;

public abstract class DeathTrap : MonoBehaviour
{
    // 公共致死逻辑
    protected virtual void KillPlayer(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var playerLife = other.GetComponent<PlayerLife>();
            if (playerLife != null)
            {
                playerLife.Die();
            }
        }
    }

    // 子类在 OnTriggerEnter2D 里调用
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        KillPlayer(other);
    }

    public void TriggerDeath(Collider2D other)
    {
        KillPlayer(other);
    }
}