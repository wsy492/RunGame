using UnityEngine;

public class RedWall : AttractWallBase
{
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            var player = collision.collider.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.Die();
            }
        }
    }
}