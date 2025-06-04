using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance;

    private int difficultyLevel = 1; // 初始难度
    private float initialThreshold = 3f; // 初始难度阈值
    private float thresholdDecayRate = 0.1f; // 每通过一个房间降低的阈值
    private float difficultyThreshold; // 当前难度阈值
    private int passedRoomCount = 1; // 已通过的房间数，初始为1避免除以0

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
        AdjustDifficulty();
    }

    public void IncrementPassedRoomCount()
    {
        passedRoomCount++;

        // 动态调整难度阈值
        difficultyThreshold = Mathf.Max(1f, initialThreshold - passedRoomCount * thresholdDecayRate);
    }

    private void AdjustDifficulty()
    {
        int alivePlayers = PlayerManager.GetAlivePlayers().Count;

        // 计算难度比值
        float difficultyRatio = (float)alivePlayers / passedRoomCount;

        // 根据难度比值调整难度等级
        if (difficultyRatio > difficultyThreshold)
        {
            difficultyLevel = 3; // 困难
        }
        else if (difficultyRatio > difficultyThreshold / 2)
        {
            difficultyLevel = 2; // 中等
        }
        else
        {
            difficultyLevel = 1; // 简单
        }

        //Debug.Log($"当前难度等级: {difficultyLevel}, 存活小人: {alivePlayers}, 已通过房间: {passedRoomCount}, 难度比值: {difficultyRatio:F2}, 当前阈值: {difficultyThreshold:F2}");
    }

    public int GetDifficultyLevel()
    {
        return difficultyLevel;
    }
}