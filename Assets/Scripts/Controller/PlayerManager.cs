using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    private static List<PlayerMovement> players = new List<PlayerMovement>();
    private float spawnCooldown = 0.1f; // 每次生成小人的间隔时间（秒）
    private float lastSpawnTime = 0f;  // 上次生成小人的时间、

    private int currentPrefabIndex = 0; // 当前预制体索引

    public bool isGameStarted = false; // 游戏是否已经开始
    [SerializeField] private GameObject[] basePlayerPrefab; // 小人预制体数
    private Transform spawnPoint; // 小人生成点
    private static Dictionary<PlayerMovement, Vector3> originalScales = new Dictionary<PlayerMovement, Vector3>();
    private const int MaxPlayerCount = 50; // 最大小人数量
    [SerializeField] private RuntimeAnimatorController[] playerOverrideControllers;
    private static int initialPlayerCount = 0;

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
            HandleInitialPhase();
        }
        else
        {
            HandleGameplayPhase();
        }
    }

    private void HandleInitialPhase()
    {
        // 长按鼠标左键生成小人
        if (Input.GetMouseButton(0)) // 鼠标左键按下
        {
            if (players.Count < MaxPlayerCount && Time.time - lastSpawnTime >= spawnCooldown) // 检查数量和生成间隔
            {
                SpawnPlayer();
                lastSpawnTime = Time.time; // 更新上次生成时间
            }
        }

        // 达到最大数量自动开始游戏
        if (players.Count >= MaxPlayerCount)
        {
            isGameStarted = true;
            initialPlayerCount = players.Count;
        }
        // 松开鼠标左键，游戏正式开始
        else if (Input.GetMouseButtonUp(0)) // 鼠标左键松开
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

        // 在每帧给所有小人一个指向中心的微小力
        ApplyCenterForceToPlayers();
    }

    private void SpawnPlayer()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        worldPosition.z = 0;

        // 只检测ReleaseLimitation层
        int layerMask = LayerMask.GetMask("ReleaseLimitation");
        RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero, 0f, layerMask);

        if (hit.collider == null)
        {
            // 没有点在正确的square上，直接返回
            return;
        }

        // 不加偏移，直接用 worldPosition
        GameObject newPlayer = Instantiate(basePlayerPrefab[0], worldPosition, Quaternion.identity);

        int randomIndex = Random.Range(0, playerOverrideControllers.Length);
        Animator animator = newPlayer.GetComponent<Animator>();
        if (animator != null && playerOverrideControllers.Length > randomIndex)
        {
            animator.runtimeAnimatorController = playerOverrideControllers[randomIndex];
        }
        else
        {
            Debug.LogWarning("Animator 或 Override Controller 未正确设置");
        }

        Register(newPlayer.GetComponent<PlayerMovement>());
    }

    public static List<PlayerMovement> GetAlivePlayers()
    {
        return players.FindAll(player => player != null && player.isActiveAndEnabled);
    }

    public static void CheckAllPlayersDead()
    {
        // 检查是否所有玩家都死亡
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
        players = GetAlivePlayers(); // 确保只对存活的小人进行操作
        foreach (var player in players)
        {
            if (player != null)
            {
                player.transform.localScale = originalScale;
            }
        }
    }

    // 给所有小人施加指向中心的力
    private void ApplyCenterForceToPlayers()
    {
        var alivePlayers = GetAlivePlayers();
        if (alivePlayers.Count == 0) return;

        // 计算所有小人的中心点
        Vector3 center = Vector3.zero;
        foreach (var player in alivePlayers)
        {
            center += player.transform.position;
        }
        center /= alivePlayers.Count;

        // 给每个小人施加一个指向中心的力
        float forceStrength = 2f; // 力的大小可以调整
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