using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Score : MonoBehaviour
{
    public Text scoreText;
    public RoomModule lastRoom;
    public RoomModule currentRoom;
    public int totalScore = 0;

    void Update()
    {
        if (scoreText != null)
        {
            scoreText.text = $"分数: {totalScore}";
        }
    }
}