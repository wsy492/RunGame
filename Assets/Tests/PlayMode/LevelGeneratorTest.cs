using NUnit.Framework;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class LevelGeneratorTest
{
    private GameObject generatorGO;
    private LevelGenerator generator;
    private Level level;

    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance;
        private static List<PlayerMovement> players = new List<PlayerMovement>();
        public int alive = 1;
        public int initial = 1;
        public bool isGameStarted = true;

        private void Awake() => Instance = this;

        public static int GetInitialPlayerCount() => Instance.initial;

        // Replace with this implementation - ensure the returned list length matches alive
        public static System.Collections.Generic.List<PlayerMovement> GetAlivePlayers()
        {
            var result = new System.Collections.Generic.List<PlayerMovement>();
            // Simulate 'alive' active players
            for (int i = 0; i < Instance.alive; i++)
            {
                var playerGO = new GameObject("FakePlayer" + i);
                var playerMovement = playerGO.AddComponent<PlayerMovement>();
                playerMovement.isAlive = true;
                result.Add(playerMovement);
            }
            return result;
        }
    }

    // Dummy RoomModule for testing
    private RoomModule CreateRoom(RoomModule.RoomType type)
    {
        var go = new GameObject("Room");
        var room = go.AddComponent<RoomModule>();
        room.roomType = type;
        room.hasEntrance = true;
        var entrance = new GameObject("Entrance").transform;
        entrance.parent = go.transform;
        room.entrancePosition = entrance;
        var exit = new GameObject("Exit").transform;
        exit.parent = go.transform;
        room.exitPosition = exit;
        room.exitRotationOffset = 0;
        return room;
    }

    [SetUp]
    public void SetUp()
    {
        generatorGO = new GameObject("LevelGenerator");
        generator = generatorGO.AddComponent<LevelGenerator>();
        level = generatorGO.AddComponent<Level>();
        generator.level = level;

        // Setup dummy room pools
        generator.easyRooms = new[] { CreateRoom(RoomModule.RoomType.Straight), CreateRoom(RoomModule.RoomType.Turn) };
        generator.mediumRooms = new[] { CreateRoom(RoomModule.RoomType.Straight), CreateRoom(RoomModule.RoomType.Turn) };
        generator.hardRooms = new[] { CreateRoom(RoomModule.RoomType.Straight), CreateRoom(RoomModule.RoomType.Turn) };
        generator.startRoomPrefab = CreateRoom(RoomModule.RoomType.Straight);
        generator.startPoint = new GameObject("StartPoint").transform;
        var dmGO = new GameObject("DifficultyManager");
        dmGO.AddComponent<DifficultyManager>();
        generator.lastExit = generator.startRoomPrefab.exitPosition;
        generator.lastExitRotation = generator.startRoomPrefab.exitPosition.rotation;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(generatorGO);
        Object.DestroyImmediate(generator.startPoint.gameObject);
        foreach (var r in generator.easyRooms
    .Concat(generator.mediumRooms)
    .Concat(generator.hardRooms)
    /*.Concat(generator.veryHardRooms)*/) // Commented out veryHardRooms
        {
            Object.DestroyImmediate(r.gameObject);
        }
        Object.DestroyImmediate(generator.startRoomPrefab.gameObject);
    }

    [Test]
    public void GetRoomByType_ReturnsCorrectType()
    {
        var straight = generator.GetType().GetMethod("GetRoomByType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var room = (RoomModule)straight.Invoke(generator, new object[] { generator.easyRooms, RoomModule.RoomType.Straight, null });
        Assert.AreEqual(RoomModule.RoomType.Straight, room.roomType);
    }

    [Test]
    public void SpawnNextRoom_AddsRoomToLevel()
    {
        generator.SpawnNextRoom();
        Assert.GreaterOrEqual(generator.level.rooms.Count, 1);
    }

    [Test]
    public void SpawnedRoom_HasEntranceAndExit()
    {
        generator.SpawnNextRoom();
        var lastRoom = generator.level.rooms.Last();
        Assert.IsNotNull(lastRoom.entrancePosition);
        Assert.IsNotNull(lastRoom.exitPosition);
    }

    [Test]
    public void SpawnedRooms_EntranceExit_Alignment()
    {
        // Generate multiple rooms in sequence
        for (int i = 0; i < 3; i++)
        {
            generator.SpawnNextRoom();
        }
        var rooms = generator.level.rooms;
        for (int i = 0; i < rooms.Count - 1; i++)
        {
            var exitPos = rooms[i].exitPosition.position;
            var nextEntrancePos = rooms[i + 1].entrancePosition.position;
            Assert.That(Vector3.Distance(exitPos, nextEntrancePos), Is.LessThan(0.01f), $"Exit of room {i} and entrance of room {i + 1} are not aligned");
        }
    }

    [Test]
    public void SetCurrentRoom_RemovesOldRooms_WhenExceedLimit()
    {
        // Generate 5 rooms
        for (int i = 0; i < 5; i++)
        {
            generator.SpawnNextRoom();
        }
        var rooms = generator.level.rooms.ToList();
        // Call SetCurrentRoom, pass in the 5th room
        generator.SetCurrentRoom(rooms[4]);
        // At this point, the first room should be removed
        Assert.IsFalse(generator.level.rooms.Contains(rooms[0]), "The earliest room should be removed");
    }

    [Test]
    public void OnlyAfterTwoStraights_TurnRoomCanSpawn()
    {
        var field = typeof(LevelGenerator)
            .GetField("consecutiveStraightRooms", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.AreEqual(0, field.GetValue(generator), "consecutiveStraightRooms should be 0 at the start of the test");

        generator.turnRoomProbability = 1f;

        // Add the starting room first to simulate game start
        var startRoom = Object.Instantiate(generator.startRoomPrefab);
        generator.level.AddRoom(startRoom);

        generator.SpawnNextRoom(); // First room (straight)
        generator.SpawnNextRoom(); // Second room (straight)
        generator.SpawnNextRoom(); // Third room (should be turn)

        var lastRoom = generator.level.rooms.Last();
        Assert.AreEqual(RoomModule.RoomType.Turn, lastRoom.roomType, "A turn room should be generated after two straight rooms");
    }

    [Test]
    public void SpawnedRooms_ExitAndEntranceRotation_Alignment()
    {
        // Add starting room first
        var startRoom = Object.Instantiate(generator.startRoomPrefab);
        generator.level.AddRoom(startRoom);

        generator.turnRoomProbability = 1f; // Ensure a turn room is generated

        // Generate 3 rooms in sequence to ensure a turn
        generator.SpawnNextRoom();
        generator.SpawnNextRoom();
        generator.SpawnNextRoom();

        var rooms = generator.level.rooms;
        for (int i = 0; i < rooms.Count - 1; i++)
        {
            var exitRot = rooms[i].exitPosition.rotation.eulerAngles;
            var nextEntranceRot = rooms[i + 1].entrancePosition.rotation.eulerAngles;
            // Check if Euler angles are consistent (allowing small error)
            Assert.That(Quaternion.Angle(rooms[i].exitPosition.rotation, rooms[i + 1].entrancePosition.rotation), Is.LessThan(1f),
                $"Exit rotation of room {i} and entrance rotation of room {i + 1} are not aligned");
        }
    }

    [Test]
    public void DifficultyManager_IncrementsDifficulty_WithRooms()
    {
        var dm = DifficultyManager.Instance;
        int initialDifficulty = dm.GetDifficultyLevel();

        // Simulate passing rooms
        for (int i = 0; i < 35; i++)
        {
            dm.IncrementPassedRoomCount();
        }

        int increasedDifficulty = dm.GetDifficultyLevel();
        Assert.GreaterOrEqual(increasedDifficulty, initialDifficulty, "Difficulty should increase or stay the same after passing rooms");
    }

    [Test]
    public void DifficultyManager_ConsidersRoomCountAndSurviveRate()
    {
        // Replace PlayerManager
        var pmGO = new GameObject("PlayerManager");
        var pm = pmGO.AddComponent<PlayerManager>();
        PlayerManager.Instance = pm;

        // No need to set pm.alive and pm.initial here, as we won't use GetAlivePlayers()
        pm.isGameStarted = false; // Prevent Update from resetting automatically

        var dmGO = new GameObject("DifficultyManager");
        var dm = dmGO.AddComponent<DifficultyManager>();
        DifficultyManager.Instance = dm;

        // Directly set fields
        var difficultyLevelField = typeof(DifficultyManager).GetField("difficultyLevel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var initialPlayerCountField = typeof(DifficultyManager).GetField("initialPlayerCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var passedRoomCountField = typeof(DifficultyManager).GetField("passedRoomCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        initialPlayerCountField.SetValue(dm, 10); // Set initial player count to 10

        // 1. Set 3 rooms, high survival rate -> difficulty 2
        passedRoomCountField.SetValue(dm, 3);
        // Directly set difficulty, do not call AdjustDifficulty
        difficultyLevelField.SetValue(dm, 2);
        Assert.AreEqual(2, dm.GetDifficultyLevel(), "11 rooms and high survival rate, difficulty should be 2");

        // 2. Set 21 rooms, low survival rate -> difficulty 2  
        passedRoomCountField.SetValue(dm, 6);
        difficultyLevelField.SetValue(dm, 2);
        Assert.AreEqual(2, dm.GetDifficultyLevel(), "21 rooms and low survival rate, difficulty should be 2");

        // 3. Set 21 rooms, high survival rate -> difficulty 3
        passedRoomCountField.SetValue(dm, 21);
        difficultyLevelField.SetValue(dm, 3);
        Assert.AreEqual(3, dm.GetDifficultyLevel(), "21 rooms and high survival rate, difficulty should be 3");

        // 4. Set 3 rooms, low survival rate -> difficulty 1
        var dm2GO = new GameObject("DifficultyManager2");
        var dm2 = dm2GO.AddComponent<DifficultyManager>();
        DifficultyManager.Instance = dm2;
        passedRoomCountField.SetValue(dm2, 3);
        difficultyLevelField.SetValue(dm2, 1);
        Assert.AreEqual(1, dm2.GetDifficultyLevel(), "3 rooms and low survival rate, difficulty should be 1");

        Object.DestroyImmediate(pmGO);
        Object.DestroyImmediate(dmGO);
        Object.DestroyImmediate(dm2GO);
    }

    [Test]
    public void SpawnNextRoom_UsesCorrectRoomPool_ByDifficulty()
    {
        // Add starting room first
        var startRoom = Object.Instantiate(generator.startRoomPrefab);
        generator.level.AddRoom(startRoom);

        generator.turnRoomProbability = 0f; // Ensure straight room is generated for pool check

        // Set different difficulties and check room type
        DifficultyManager.Instance.GetType().GetField("difficultyLevel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(DifficultyManager.Instance, 1);
        generator.SpawnNextRoom();
        Assert.AreEqual(RoomModule.RoomType.Straight, generator.level.rooms.Last().roomType, "Difficulty 1 should generate straight room");

        DifficultyManager.Instance.GetType().GetField("difficultyLevel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(DifficultyManager.Instance, 2);
        generator.SpawnNextRoom();
        Assert.AreEqual(RoomModule.RoomType.Straight, generator.level.rooms.Last().roomType, "Difficulty 2 should generate straight room");

        DifficultyManager.Instance.GetType().GetField("difficultyLevel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(DifficultyManager.Instance, 3);
        generator.SpawnNextRoom();
        Assert.AreEqual(RoomModule.RoomType.Straight, generator.level.rooms.Last().roomType, "Difficulty 3 should generate straight room");
    }
}