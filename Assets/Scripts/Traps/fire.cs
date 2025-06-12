using UnityEngine;

public class Fire : DeathTrap
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        TriggerDeath(collision.collider); // Calls the superclass's lethal logic
    }
}