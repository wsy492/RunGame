using UnityEngine;

public class BubbleBehavior : MonoBehaviour
{
    public float riseSpeed = 2f; // Bubble rising speed
    public float lifeTime = 5f;  // Maximum bubble lifetime (in water)
    public Vector2 initialForceRangeX = new Vector2(-0.2f, 0.2f); // Initial random X force range

    private bool isInWater = false; // By default, the bubble is not in water, needs detection

    void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = -0.1f;
        rb.linearDamping = 0.5f;

        float randomXForce = Random.Range(initialForceRangeX.x, initialForceRangeX.y);
        rb.AddForce(new Vector2(randomXForce, 0), ForceMode2D.Impulse);

        // Set a maximum lifetime, if the bubble stays in water, destroy after this time
        // If the bubble never enters or leaves water, it will be destroyed sooner by the check in Update()
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // If the bubble is detected not in water, destroy it immediately
        if (!isInWater)
        {
            Destroy(gameObject);
            // Debug.Log("Bubble destroyed because it's not in water.");
        }
    }

    // Called when the bubble's collider enters another trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Trap"))
        {
            Destroy(gameObject);
            // Debug.Log("Bubble hit a Trap and is being destroyed.");
            return; // Bubble destroyed
        }
        if (other.CompareTag("Water"))
        {
            isInWater = true;
            // Debug.Log("Bubble entered Water.");
        }
    }

    // Called every frame while the bubble's collider stays inside another trigger
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Water"))
        {
            // Continuously confirm the bubble is in water, important for bubbles spawned inside water
            if (!isInWater) // Only log when state changes to avoid spamming
            {
                // Debug.Log("Bubble is confirmed to be in Water (OnTriggerStay).");
            }
            isInWater = true;
        }
    }

    // Called when the bubble's collider exits another trigger
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Water"))
        {
            isInWater = false;
            // Debug.Log("Bubble exited Water.");
            // The Update() method will handle destroy logic in the next frame
        }
    }
}