using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance;

    private int difficultyLevel = 1; // Initial difficulty
    private float initialThreshold = 3f; // Initial difficulty threshold
    private float thresholdDecayRate = 0.1f; // Threshold decay rate per room
    private float difficultyThreshold; // Current difficulty threshold
    private int passedRoomCount = 1; // Number of rooms passed, start from 1 to avoid division by zero
    private int initialPlayerCount; // Record initial player count

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        // Initialize difficulty threshold
        difficultyThreshold = initialThreshold;
    }

    private void Update()
    {
        if (initialPlayerCount == 0 && PlayerManager.Instance != null && PlayerManager.Instance.isGameStarted)
        {
            initialPlayerCount = PlayerManager.GetInitialPlayerCount();
            //Debug.Log($"Initial player count: {initialPlayerCount}");
        }
        AdjustDifficulty();
    }

    public void IncrementPassedRoomCount()
    {
        passedRoomCount++;

        // Dynamically adjust difficulty threshold
        difficultyThreshold = Mathf.Max(0.3f, initialThreshold - passedRoomCount * thresholdDecayRate);

        int alivePlayers = PlayerManager.GetAlivePlayers().Count;
        float survivalRatio = (float)alivePlayers / Mathf.Max(1, initialPlayerCount);

        Debug.Log($"[Difficulty Debug] Current room: {passedRoomCount - 1}, Alive players: {alivePlayers}, Survival rate: {survivalRatio:P1}, Current difficulty: {difficultyLevel}");
    }

    private void AdjustDifficulty()
    {
        int alivePlayers = PlayerManager.GetAlivePlayers().Count;

        // 1. Base difficulty segmentation
        int baseDifficulty = 1;
        if (passedRoomCount > 7) baseDifficulty = 3;
        else if (passedRoomCount > 4) baseDifficulty = 2;

        // 2. Survival rate and alive player count
        float survivalRatio = (float)alivePlayers / Mathf.Max(1, initialPlayerCount);

        // 3. Use base difficulty as main, survival rate and alive count as fine-tuning
        int adjustedDifficulty = baseDifficulty;

        if (survivalRatio > 1.6f || alivePlayers > 25)
            adjustedDifficulty = Mathf.Min(3, baseDifficulty + 2);
        else if (survivalRatio > 0.7f || alivePlayers > 10)
            adjustedDifficulty = Mathf.Min(3, baseDifficulty + 1);
        else if (survivalRatio < 0.3f || alivePlayers < 3)
            adjustedDifficulty = Mathf.Max(1, baseDifficulty - 1);

        // 4. If room count exceeds 15, lock difficulty to 3
        if (passedRoomCount > 15)
            difficultyLevel = 3;
        else
            difficultyLevel = adjustedDifficulty;
    }

    public int GetDifficultyLevel()
    {
        return difficultyLevel;
    }
}