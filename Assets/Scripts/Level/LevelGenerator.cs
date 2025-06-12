using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public RoomModule startRoomPrefab;
    public Transform startPoint;
    public Level level;
    public float turnRoomProbability = 0.3f;          // Probability of generating a turn room
    private int consecutiveStraightRooms = 0; // Number of consecutive straight rooms generated
    public Quaternion lastExitRotation = Quaternion.identity;
    private RoomModule currentRoom; // The room where the player is currently located
    public Transform lastExit;
    public RoomModule[] easyRooms;
    public RoomModule[] mediumRooms;
    public RoomModule[] hardRooms;
    private RoomModule lastRoomPrefab = null;

    void Start()
    {
        RoomModule startRoom = Instantiate(startRoomPrefab, startPoint.position, Quaternion.identity);
        lastExit = startRoom.exitPosition;
        lastExitRotation = startRoom.exitPosition.rotation;

        if (level != null)
        {
            level.AddRoom(startRoom);
        }

        var score = FindObjectOfType<Score>();
        if (score != null)
        {
            score.currentRoom = startRoom;
        }

        // Generate the 2nd room
        SpawnNextRoom(); // Easy difficulty

        // Assign the exit trigger for the starting room
        var startTrigger = startRoom.exitPosition.GetComponentInChildren<RoomExitTrigger>();
        if (startTrigger != null)
        {
            startTrigger.levelGenerator = this;
            startTrigger.triggered = false;
        }
        else
        {
            Debug.LogError("[Start] StartRoom's RoomExitTrigger component not found");
        }
    }

    public void SpawnNextRoom()
    {
        RoomModule roomPrefab;

        // Special handling for the first 3 rooms
        if (level.rooms.Count == 1) // The first room (easy, straight room)
        {
            roomPrefab = GetRoomByType(easyRooms, RoomModule.RoomType.Straight, lastRoomPrefab);
            consecutiveStraightRooms++; // Increase straight room count
        }
        else if (level.rooms.Count == 2) // The second room (medium, straight room)
        {
            roomPrefab = GetRoomByType(mediumRooms, RoomModule.RoomType.Straight, lastRoomPrefab);
            consecutiveStraightRooms++; // Increase straight room count
        }
        else
        {
            // From the 3rd room, generate rooms adaptively by difficulty
            RoomModule[] roomPool = DifficultyManager.Instance.GetDifficultyLevel() switch
            {
                1 => easyRooms,   // Easy room pool
                2 => mediumRooms, // Medium room pool
                3 => hardRooms,   // Hard room pool
                _ => hardRooms   // Default to hard room pool
            };
            Debug.Log($"Current difficulty level: {DifficultyManager.Instance.GetDifficultyLevel()}");

            // Determine if a turn room can be generated:
            // 1. At least 2 consecutive straight rooms
            // 2. At least 2 straight rooms since the last turn
            bool canSpawnTurn = consecutiveStraightRooms >= 2;
            bool shouldSpawnTurnRoom = canSpawnTurn && Random.value < turnRoomProbability;

            if (shouldSpawnTurnRoom)
            {
                // Select a turn room from the pool
                roomPrefab = GetRoomByType(roomPool, RoomModule.RoomType.Turn, lastRoomPrefab);
                consecutiveStraightRooms = 0; // Reset straight room count
                                              //  Debug.Log("Generated turn room, reset counter");
            }
            else
            {
                // Select a straight room from the pool
                roomPrefab = GetRoomByType(roomPool, RoomModule.RoomType.Straight, lastRoomPrefab);
                consecutiveStraightRooms++; // Increase straight room count
            }
        }

        // Instantiate the room
        RoomModule room = Instantiate(roomPrefab, Vector3.zero, lastExitRotation);

        lastRoomPrefab = roomPrefab;

        // Adjust room position
        if (room.hasEntrance && room.entrancePosition != null)
        {
            Vector3 offset = room.entrancePosition.position - room.transform.position;
            room.transform.position = lastExit.position - offset;
        }
        else
        {
            room.transform.position = lastExit.position;
        }

        // Update exit position and rotation
        lastExit = room.exitPosition;
        lastExitRotation *= Quaternion.Euler(0, 0, room.exitRotationOffset);

        // Set the exit trigger for the room
        var trigger = room.exitPosition.GetComponentInChildren<RoomExitTrigger>();
        if (trigger != null)
        {
            trigger.levelGenerator = this;
            trigger.triggered = false;
        }

        // Add the room to the level manager
        if (level != null)
        {
            level.AddRoom(room);
        }
    }

    private RoomModule GetRoomByType(RoomModule[] roomPool, RoomModule.RoomType type, RoomModule exclude = null)
    {
        var filtered = System.Array.FindAll(roomPool, r => r.roomType == type && r != exclude);
        if (filtered.Length == 0)
        {
            // If there are no available rooms after exclusion, allow duplicates
            filtered = System.Array.FindAll(roomPool, r => r.roomType == type);
        }
        if (filtered.Length == 0)
        {
            Debug.LogWarning($"No room of type {type} found!");
            return roomPool[0]; // Fallback
        }
        return filtered[Random.Range(0, filtered.Length)];
    }

    public void SetCurrentRoom(RoomModule room)
    {
        if (currentRoom != null && currentRoom != room)
        {
            // Find the index of the incoming room in the list
            var rooms = level.rooms;
            int currentIndex = rooms.IndexOf(room);

            // Remove rooms that are more than three rooms away from the incoming room
            if (currentIndex >= 2) // Ensure there are at least four rooms
            {
                level.RemoveRoom(rooms[currentIndex - 2]);
            }
        }

        // Update the current room
        currentRoom = room;
        while (level.rooms.Count > 4)
        {
            level.RemoveRoom(level.rooms[0]);
        }
    }
}