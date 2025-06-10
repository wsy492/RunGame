using UnityEngine;

public class RoomExitTrigger : MonoBehaviour
{
    public LevelGenerator levelGenerator;
    [HideInInspector] public bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;
            if (levelGenerator != null)
            {
                var score = FindObjectOfType<Score>();
                if (score != null)
                {
                    // 直接加上当前房间的分数
                    var thisRoom = transform.parent.GetComponent<RoomModule>();
                    score.totalScore += thisRoom.score;
                    score.lastRoom = thisRoom;
                    score.currentRoom = thisRoom;
                }

                DifficultyManager.Instance.IncrementPassedRoomCount();

                // 先更新当前房间
                levelGenerator.SetCurrentRoom(transform.parent.GetComponent<RoomModule>());
                // 再生成新房间
                levelGenerator.SpawnNextRoom();
                gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError($"Exit触发器上的LevelGenerator为空: {gameObject.name}, ID: {gameObject.GetInstanceID()}");
            }
        }
    }
}