using UnityEngine;

public class SpikeHead : DeathTrap
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        TriggerDeath(collision.collider); // Reuse the DeathTrap logic
    }
}