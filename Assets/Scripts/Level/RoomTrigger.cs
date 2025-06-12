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
                // Get reference to the Score component
                var score = FindObjectOfType<Score>();
                if (score != null)
                {
                    // Directly add the score of the current room
                    var thisRoom = transform.parent.GetComponent<RoomModule>();
                    Score.totalScore += thisRoom.score; // Modified here
                    score.lastRoom = thisRoom;
                    score.currentRoom = thisRoom;
                }

                DifficultyManager.Instance.IncrementPassedRoomCount();

                // First update the current room
                levelGenerator.SetCurrentRoom(transform.parent.GetComponent<RoomModule>());
                // Then generate a new room
                levelGenerator.SpawnNextRoom();
                gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError($"LevelGenerator is null on Exit trigger: {gameObject.name}, ID: {gameObject.GetInstanceID()}");
            }
        }
    }
}