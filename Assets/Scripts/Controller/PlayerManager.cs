using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    private static List<PlayerMovement> players = new List<PlayerMovement>();
    private float spawnCooldown = 0.1f; // Interval time (seconds) for spawning each player
    private float lastSpawnTime = 0f;  // Last spawn time

    private int currentPrefabIndex = 0; // Current prefab index

    public bool isGameStarted = false; // Whether the game has started
    [SerializeField] private GameObject[] basePlayerPrefab; // Player prefabs
    private Transform spawnPoint; // Player spawn point
    private static Dictionary<PlayerMovement, Vector3> originalScales = new Dictionary<PlayerMovement, Vector3>();
    private const int MaxPlayerCount = 50; // Maximum number of players
    [SerializeField] private RuntimeAnimatorController[] playerOverrideControllers;
    private static int initialPlayerCount = 0;
    [SerializeField] private AudioSource spawnSound;
    public Text tipText;

    public static void Register(PlayerMovement player)
    {
        if (!players.Contains(player))
            players.Add(player);
    }

    public static void Unregister(PlayerMovement player)
    {
        players.Remove(player);
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!isGameStarted)
        {
            if (tipText != null)
                tipText.gameObject.SetActive(true); // Show tip
            HandleInitialPhase();
        }
        else
        {
            if (tipText != null)
                tipText.gameObject.SetActive(false); // Hide tip after game starts
            HandleGameplayPhase();
        }
    }

    private void HandleInitialPhase()
    {
        // Hold left mouse button to spawn players
        if (Input.GetMouseButton(0)) // Left mouse button pressed
        {
            if (players.Count < MaxPlayerCount && Time.time - lastSpawnTime >= spawnCooldown) // Check count and interval
            {
                SpawnPlayer();
                lastSpawnTime = Time.time; // Update last spawn time
            }
        }

        // Automatically start game when reaching max count
        if (players.Count >= MaxPlayerCount)
        {
            isGameStarted = true;
            initialPlayerCount = players.Count;
        }
        // Release left mouse button to officially start the game
        else if (Input.GetMouseButtonUp(0)) // Left mouse button released
        {
            isGameStarted = true;
            initialPlayerCount = players.Count;
        }
    }

    private void HandleGameplayPhase()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        foreach (var player in players)
        {
            player.Move(moveInput);
        }

        if (Input.GetButtonDown("Jump"))
        {
            foreach (var player in players)
                player.TryJump();
        }

        // Apply a small force towards the center to all players every frame
        ApplyCenterForceToPlayers();
    }

    private void SpawnPlayer()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        worldPosition.z = 0;

        // Only detect ReleaseLimitation layer
        int layerMask = LayerMask.GetMask("ReleaseLimitation");
        RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero, 0f, layerMask);

        if (hit.collider == null)
        {
            // Not clicked on the correct square, just return
            return;
        }

        // No offset, use worldPosition directly
        GameObject newPlayer = Instantiate(basePlayerPrefab[0], worldPosition, Quaternion.identity);

        int randomIndex = Random.Range(0, playerOverrideControllers.Length);
        Animator animator = newPlayer.GetComponent<Animator>();
        if (animator != null && playerOverrideControllers.Length > randomIndex)
        {
            animator.runtimeAnimatorController = playerOverrideControllers[randomIndex];
        }
        else
        {
            Debug.LogWarning("Animator or Override Controller not set correctly");
        }

        Register(newPlayer.GetComponent<PlayerMovement>());

        if (spawnSound != null)
            spawnSound.Play();
    }

    public static List<PlayerMovement> GetAlivePlayers()
    {
        return players.FindAll(player => player != null && player.isActiveAndEnabled);
    }

    public static void CheckAllPlayersDead()
    {
        // Check if all players are dead
        bool allDead = players.TrueForAll(player => !player.isAlive);
        if (allDead)
        {
            GameOver();
        }
    }

    private static void GameOver()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public static void ScaleAllPlayers(float scaleFactor)
    {
        Vector3 originalScale = new Vector3(2.77f, 2.77f, 4.05f);
        foreach (var player in players)
        {
            if (player != null && player.isActiveAndEnabled)
            {
                player.transform.localScale = originalScale * scaleFactor;
            }
        }
    }


    public static void RestoreAllPlayersScale()
    {
        Vector3 originalScale = new Vector3(2.77f, 2.77f, 4.05f);
        players = GetAlivePlayers(); // Ensure only alive players are operated on
        foreach (var player in players)
        {
            if (player != null)
            {
                player.transform.localScale = originalScale;
            }
        }
    }

    // Apply a force towards the center to all players
    private void ApplyCenterForceToPlayers()
    {
        var alivePlayers = GetAlivePlayers();
        if (alivePlayers.Count == 0) return;

        // Calculate the center point of all players
        Vector3 center = Vector3.zero;
        foreach (var player in alivePlayers)
        {
            center += player.transform.position;
        }
        center /= alivePlayers.Count;

        // Apply a force towards the center to each player
        float forceStrength = 2f; // Force magnitude can be adjusted
        foreach (var player in alivePlayers)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 dir = center - player.transform.position;
                rb.AddForce(dir.normalized * forceStrength);
            }
        }
    }

    public static int GetInitialPlayerCount()
    {
        return initialPlayerCount;
    }
}