using UnityEngine;

public class Sword : MonoBehaviour
{
    [SerializeField] private float speed = 5f; // Sword flying speed

    void Start()
    {
        Destroy(gameObject, 5f); // Auto destroy after 5 seconds
    }

    void Update()
    {
        // Move forward at a constant speed along its own direction
        transform.Translate(Vector3.right * speed * Time.deltaTime, Space.Self);
    }

    // Detect player collision
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var playerLife = other.GetComponent<PlayerLife>();
            if (playerLife != null)
            {
                playerLife.Die();
            }
        }
        // Do not destroy itself, not affected by anything
    }
}