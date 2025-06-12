using UnityEngine;

public class SpikedBall : DeathTrap
{
    [SerializeField] private Transform[] waypoints; // Points along the path
    [SerializeField] private float speed = 2f; // Movement speed
    private int currentWaypointIndex = 0;

    private void Update()
    {
        MoveAlongPath();
    }

    private void MoveAlongPath()
    {
        if (waypoints.Length == 0) return;

        // Get the current target point
        Transform targetWaypoint = waypoints[currentWaypointIndex];

        // Move to the target point
        transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, speed * Time.deltaTime);

        // If reached the target point, switch to the next point
        if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length; // Loop the path
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TriggerDeath(collision.collider); // Reuse the DeathTrap logic
    }
}