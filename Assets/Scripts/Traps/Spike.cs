using UnityEngine;

public class Spike : DeathTrap
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        TriggerDeath(collision.collider); // Reuse the DeathTrap logic
    }
}