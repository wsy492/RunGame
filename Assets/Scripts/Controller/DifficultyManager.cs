using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance;

    private int difficultyLevel = 1; // 初始难度
    private float initialThreshold = 3f; // 初始难度阈值
    private float thresholdDecayRate = 0.1f; // 每通过一个房间降低的阈值
    private float difficultyThreshold; // 当前难度阈值
    private int passedRoomCount = 1; // 已通过的房间数，初始为1避免除以0
    private int initialPlayerCount; // 记录初始玩家数

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        // 初始化难度阈值
        difficultyThreshold = initialThreshold;
    }

    private void Update()
    {
        if (initialPlayerCount == 0 && PlayerManager.Instance != null && PlayerManager.Instance.isGameStarted)
        {
            initialPlayerCount = PlayerManager.GetInitialPlayerCount();
            //Debug.Log($"初始玩家数: {initialPlayerCount}");
        }
        AdjustDifficulty();
    }

    public void IncrementPassedRoomCount()
    {
        passedRoomCount++;

        // 动态调整难度阈值
        difficultyThreshold = Mathf.Max(0.3f, initialThreshold - passedRoomCount * thresholdDecayRate);
    }

    private void AdjustDifficulty()
    {
        int alivePlayers = PlayerManager.GetAlivePlayers().Count;

        // 1. 基础难度分段
        int baseDifficulty = 1;
        if (passedRoomCount > 30) baseDifficulty = 3;
        else if (passedRoomCount > 20) baseDifficulty = 3;
        else if (passedRoomCount > 10) baseDifficulty = 2;

        // 2. 存活率影响因素
        float survivalRatio = (float)alivePlayers / Mathf.Max(1, initialPlayerCount);

        // 3. 30关以后，难度下限锁定为3，只能提升
        if (passedRoomCount > 30)
        {
            if (survivalRatio > 0.7f)
                difficultyLevel = 4; // 存活率高，提升难度到4（你可以根据实际情况设定最大值）
            else
                difficultyLevel = 3; // 不再降低
        }
        else
        {
            if (survivalRatio < 0.3f)
                difficultyLevel = Mathf.Max(1, baseDifficulty - 1); // 存活率低，降低难度
            else if (survivalRatio > 0.7f)
                difficultyLevel = Mathf.Min(3, baseDifficulty + 1); // 存活率高，提升难度
            else
                difficultyLevel = baseDifficulty; // 其余情况保持基础难度
        }
    }

    public int GetDifficultyLevel()
    {
        return difficultyLevel;
    }
}