using UnityEngine;

public class RoomModule : MonoBehaviour
{
    public Transform entrancePosition;
    public Transform exitPosition;
    public RoomType roomType;
    public bool hasEntrance = true;
    [Tooltip("Rotation angle of the exit relative to the entrance (e.g. 90, -90, 180)")]
    public float exitRotationOffset = 0f;

    [Header("Room Score")]
    public int score = 100; // Added: score for each room

    public enum RoomType
    {
        Start,
        Straight,
        Turn,
    }
}