using UnityEngine;

public class RoomModule : MonoBehaviour
{
    public Transform entrancePosition;
    public Transform exitPosition;
    public GameObject[] obstacles;
    public GameObject[] coins;
    public GameObject[] items;
    public RoomType roomType;
    public bool hasEntrance = true;
    [Tooltip("出口相对于入口的旋转角度（例如 90、-90、180）")]
    public float exitRotationOffset = 0f;

    [Header("房间分数")]
    public int score = 100; // 新增：每个房间的分数

    public enum RoomType
    {
        Start,
        Straight,
        Turn,
    }
}