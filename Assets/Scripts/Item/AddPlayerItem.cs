using UnityEngine;

public class AddPlayerItem : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab; // 小人预制体
    [SerializeField] private Vector2 spawnOffset = new Vector2(5f, 10f); // 生成位置的偏移量，x 和 y 都有随机范围
    [SerializeField] private LayerMask Ground; // 用于检测障碍物的图层
    [SerializeField] private float spawnCheckRadius = 0.5f; // 检测生成位置的半径

    [SerializeField] private Collider2D increaseItemArea; // 生成区域的 Collider

    [SerializeField] private RuntimeAnimatorController[] playerOverrideControllers;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 随机生成 2 到 8 个小人
            int playerCount = Random.Range(2, 9);
            for (int i = 0; i < playerCount; i++)
            {
                SpawnNewPlayer(collision.transform.position);
            }

            // 销毁道具
            Destroy(gameObject);
        }
    }

    private void SpawnNewPlayer(Vector3 basePosition)
    {
        Vector3 spawnPosition;
        int maxAttempts = 50; // 最大尝试次数
        int attempts = 0;
        bool foundValidPosition = false;

        // 获取 IncreaseItemArea 的边界
        Bounds areaBounds = increaseItemArea.bounds;

        // 获取玩家 Collider 尺寸
        Vector2 checkSize = new Vector2(2.77f, 2.77f); // 默认尺寸（后面如果已存在小人，可以从它身上动态获取）

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
            // 随机生成位置，限制在 IncreaseItemArea 的边界内
            float randomX = Random.Range(areaBounds.min.x, areaBounds.max.x);
            float randomY = Random.Range(areaBounds.min.y, areaBounds.max.y);
            spawnPosition = new Vector3(randomX, randomY, basePosition.z);

            // 检查生成位置是否有效（没有与障碍物重叠）
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
            Debug.LogWarning("无法找到合适的生成位置，放弃生成小人");
            return;
        }

        // 保留现有的 localScale 逻辑
        float currentZRotation = 0f;
        Vector3 currentScale = Vector3.one;

        if (existPlayer != null)
        {
            currentZRotation = existPlayer.transform.eulerAngles.z;
            currentScale = existPlayer.transform.localScale;
        }

        // 生成小人并设置朝向和缩放
        GameObject newPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.Euler(0, 0, currentZRotation));
        newPlayer.transform.localScale = currentScale;

        // 随机赋值不同样式
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
                Debug.LogWarning("Animator 未正确设置，无法更换样式");
            }
        }
        else
        {
            Debug.LogWarning("playerOverrideControllers 未设置");
        }

        // 注册新小人到 PlayerManager
        PlayerManager.Register(newPlayer.GetComponent<PlayerMovement>());
    }

}