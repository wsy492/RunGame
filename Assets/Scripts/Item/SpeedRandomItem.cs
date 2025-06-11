using UnityEngine;

public class SpeedRandomItem : MonoBehaviour
{
    public float speedUpValue = 10f;
    public float speedUpMax = 20f;
    public float speedDownValue = 2f;
    public float speedDownMax = 4f;
    public float effectDuration = 3f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            bool isSpeedUp = Random.value < 0.5f;
            var players = PlayerManager.GetAlivePlayers();
            foreach (var player in players)
            {
                var move = player.GetComponent<PlayerMovement>();
                if (move != null)
                {
                    if (isSpeedUp)
                        move.ChangeSpeedTemp(speedUpValue, speedUpMax, effectDuration);
                    else
                        move.ChangeSpeedTemp(speedDownValue, speedDownMax, effectDuration);
                }
            }
            Destroy(gameObject);
        }
    }
}