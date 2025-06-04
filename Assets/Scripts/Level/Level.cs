using UnityEngine;
using System.Collections.Generic;

public class Level : MonoBehaviour
{
    public List<RoomModule> rooms = new List<RoomModule>();

    public void AddRoom(RoomModule room)
    {
        rooms.Add(room);
    }

    public void RemoveRoom(RoomModule room)
    {
        if (rooms.Contains(room))
        {
            rooms.Remove(room);
            Destroy(room.gameObject); // 销毁房间的 GameObject
        }
    }
}