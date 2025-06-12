using UnityEngine;

public class Laser : MonoBehaviour
{
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        // As long as the laser is visible in the camera view, pressing Shift will destroy the laser
        if (rend != null && rend.isVisible)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            {
                Destroy(gameObject);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // The laser only works when it is visible in the camera view
        if (rend != null && rend.isVisible)
        {
            if (other.CompareTag("Player"))
            {
                // Get the PlayerLife script and call the death method
                PlayerLife playerLife = other.GetComponent<PlayerLife>();
                if (playerLife != null)
                {
                    playerLife.Die();
                }
            }
        }
    }
}