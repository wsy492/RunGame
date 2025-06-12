using UnityEngine;
using System.Collections;

public class ZoomItem : MonoBehaviour
{
    // Scale ratios
    private const float scaleUp = 1.4f; // Scale up ratio
    private const float scaleDown = 0.75f; // Scale down ratio

    // Camera adjustment values
    private const float cameraAdjustUp = 2.5f; // Camera zoom out value
    private const float cameraAdjustDown = -100f; // Camera zoom in value

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            int random = Random.Range(0, 2); // 0 or 1
            float targetScale = random == 0 ? scaleUp : scaleDown;
            float cameraAdjust = random == 0 ? cameraAdjustUp : cameraAdjustDown;
            PlayerManager.ScaleAllPlayers(targetScale);
            Camera.main.GetComponent<CameraFollow>().AdjustCamera(cameraAdjust);
            Destroy(gameObject);

            // Use global timer to delay restore and destroy
            TimerManager.Instance.StartTimer(5f, RestoreAndDestroy);
        }
    }

    private void RestoreAndDestroy()
    {
        Debug.Log("Restoring player scales after zoom item effect.");
        PlayerManager.RestoreAllPlayersScale();
        // This is the last line
    }

}