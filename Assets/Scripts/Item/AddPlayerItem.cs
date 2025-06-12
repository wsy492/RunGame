using UnityEngine;

public class AddPlayerItem : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab; // Player prefab
    [SerializeField] private Vector2 spawnOffset = new Vector2(5f, 10f); // Offset range for spawn position (random x and y)
    [SerializeField] private LayerMask Ground; // Layer for obstacle detection
    [SerializeField] private float spawnCheckRadius = 0.5f; // Radius for checking spawn position

    [SerializeField] private Collider2D increaseItemArea; // Collider for spawn area

    [SerializeField] private RuntimeAnimatorController[] playerOverrideControllers;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Randomly spawn 2 to 8 players
            int playerCount = Random.Range(2, 9);
            for (int i = 0; i < playerCount; i++)
            {
                SpawnNewPlayer(collision.transform.position);
            }

            // Destroy the item
            Destroy(gameObject);
        }
    }

    private void SpawnNewPlayer(Vector3 basePosition)
    {
        Vector3 spawnPosition;
        int maxAttempts = 50; // Maximum attempts
        int attempts = 0;
        bool foundValidPosition = false;

        // Get the bounds of IncreaseItemArea
        Bounds areaBounds = increaseItemArea.bounds;

        // Get player collider size
        Vector2 checkSize = new Vector2(2.77f, 2.77f); // Default size (can be dynamically obtained from an existing player)

        var existPlayer = FindObjectOfType<PlayerMovement>();
        if (existPlayer != null)
        {
            var collider = existPlayer.GetComponent<Collider2D>();
            if (collider != null)
            {
                checkSize = collider.bounds.size;
            }
        }

        do
        {
            // Randomly generate position within IncreaseItemArea bounds
            float randomX = Random.Range(areaBounds.min.x, areaBounds.max.x);
            float randomY = Random.Range(areaBounds.min.y, areaBounds.max.y);
            spawnPosition = new Vector3(randomX, randomY, basePosition.z);

            // Check if the spawn position is valid (not overlapping with obstacles)
            if (!Physics2D.OverlapBox(spawnPosition, checkSize, 0f, Ground))
            {
                foundValidPosition = true;
                break;
            }

            attempts++;
        }
        while (attempts < maxAttempts);

        if (!foundValidPosition)
        {
            Debug.LogWarning("Could not find a valid spawn position, giving up spawning player");
            return;
        }

        // Keep the existing localScale logic
        float currentZRotation = 0f;
        Vector3 currentScale = Vector3.one;

        if (existPlayer != null)
        {
            currentZRotation = existPlayer.transform.eulerAngles.z;
            currentScale = existPlayer.transform.localScale;
        }

        // Instantiate player and set rotation and scale
        GameObject newPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.Euler(0, 0, currentZRotation));
        newPlayer.transform.localScale = currentScale;

        // Randomly assign different styles
        if (playerOverrideControllers != null && playerOverrideControllers.Length > 0)
        {
            int randomIndex = Random.Range(0, playerOverrideControllers.Length);
            Animator animator = newPlayer.GetComponent<Animator>();
            if (animator != null)
            {
                animator.runtimeAnimatorController = playerOverrideControllers[randomIndex];
            }
            else
            {
                Debug.LogWarning("Animator is not set correctly, cannot change style");
            }
        }
        else
        {
            Debug.LogWarning("playerOverrideControllers is not set");
        }

        // Register the new player to PlayerManager
        PlayerManager.Register(newPlayer.GetComponent<PlayerMovement>());
    }

}