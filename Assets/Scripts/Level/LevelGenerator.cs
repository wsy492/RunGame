using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public RoomModule startRoomPrefab;
    public Transform startPoint;
    public Level level;
    public float turnRoomProbability = 0.3f;          // 新增：转弯房间出现的概率
    private int consecutiveStraightRooms = 0; // 连续生成的直线房间数量
    public Quaternion lastExitRotation = Quaternion.identity;
    private RoomModule currentRoom; // 玩家当前所在的房间
    public Transform lastExit;
    public RoomModule[] easyRooms;
    public RoomModule[] mediumRooms;
    public RoomModule[] hardRooms;
    public RoomModule[] veryHardRooms;

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

        // 生成第2个房间
        SpawnNextRoom(); // 简单难度

        // 给起始房间的出口触发器赋值
        var startTrigger = startRoom.exitPosition.GetComponentInChildren<RoomExitTrigger>();
        if (startTrigger != null)
        {
            startTrigger.levelGenerator = this;
            startTrigger.triggered = false;
        }
        else
        {
            Debug.LogError("[Start] 未找到StartRoom的RoomExitTrigger组件");
        }
    }

    public void SpawnNextRoom()
    {
        RoomModule roomPrefab;

        Debug.Log($"{level.rooms.Count}");

        // 特殊处理前3个房间
        if (level.rooms.Count == 1) // 第1个房间（简单难度，直线房间）
        {
            roomPrefab = GetRoomByType(easyRooms, RoomModule.RoomType.Straight);
            consecutiveStraightRooms++; // 增加直线房间计数
        }
        else if (level.rooms.Count == 2) // 第2个房间（中等难度，直线房间）
        {
            roomPrefab = GetRoomByType(mediumRooms, RoomModule.RoomType.Straight);
            consecutiveStraightRooms++; // 增加直线房间计数
        }
        else
        {
            // 从第3个房间开始，按难度自适应逻辑生成
            RoomModule[] roomPool = DifficultyManager.Instance.GetDifficultyLevel() switch
            {
                1 => easyRooms,   // 简单房间池
                2 => mediumRooms, // 中等房间池
                3 => hardRooms,   // 困难房间池
                4 => veryHardRooms,
                //_ => mediumRooms, // 默认中等房间池
            };
            Debug.Log($"当前难度等级: {DifficultyManager.Instance.GetDifficultyLevel()}");

            // 判断是否可以生成转弯房间：
            // 1. 要连续2个直线房间
            // 2. 且上次转弯后至少有2个直线房间
            bool canSpawnTurn = consecutiveStraightRooms >= 2;
            bool shouldSpawnTurnRoom = canSpawnTurn && Random.value < turnRoomProbability;

            if (shouldSpawnTurnRoom)
            {
                // 从房间池中选择一个转弯房间
                roomPrefab = GetRoomByType(roomPool, RoomModule.RoomType.Turn);
                consecutiveStraightRooms = 0; // 重置直线房间计数
                Debug.Log("生成转弯房间，重置计数器");
            }
            else
            {
                // 从房间池中选择一个直线房间
                roomPrefab = GetRoomByType(roomPool, RoomModule.RoomType.Straight);
                consecutiveStraightRooms++; // 增加直线房间计数
            }
        }

        // 实例化房间
        RoomModule room = Instantiate(roomPrefab, Vector3.zero, lastExitRotation);

        // 调整房间位置
        if (room.hasEntrance && room.entrancePosition != null)
        {
            Vector3 offset = room.entrancePosition.position - room.transform.position;
            room.transform.position = lastExit.position - offset;
        }
        else
        {
            room.transform.position = lastExit.position;
        }

        // 更新出口位置和旋转
        lastExit = room.exitPosition;
        lastExitRotation *= Quaternion.Euler(0, 0, room.exitRotationOffset);

        // 设置房间的出口触发器
        var trigger = room.exitPosition.GetComponentInChildren<RoomExitTrigger>();
        if (trigger != null)
        {
            trigger.levelGenerator = this;
            trigger.triggered = false;
        }

        // 将房间添加到关卡管理中
        if (level != null)
        {
            level.AddRoom(room);
        }
    }

    private RoomModule GetRoomByType(RoomModule[] roomPool, RoomModule.RoomType type)
    {
        // 只筛选出指定类型的房间
        var filtered = System.Array.FindAll(roomPool, r => r.roomType == type);
        if (filtered.Length == 0)
        {
            Debug.LogWarning($"没有找到类型为 {type} 的房间！");
            return roomPool[0]; // 兜底返回
        }
        return filtered[Random.Range(0, filtered.Length)];
    }

    public void SetCurrentRoom(RoomModule room)
    {
        if (currentRoom != null && currentRoom != room)
        {
            // 找到传入房间在列表中的索引
            var rooms = level.rooms;
            int currentIndex = rooms.IndexOf(room);

            // 删除距离传入房间超过三个房间的房间
            if (currentIndex >= 2) // 确保有至少四个房间
            {
                level.RemoveRoom(rooms[currentIndex - 2]);
            }
        }

        // 更新当前房间
        currentRoom = room;
        while (level.rooms.Count > 4)
        {
            level.RemoveRoom(level.rooms[0]);
        }
    }
}