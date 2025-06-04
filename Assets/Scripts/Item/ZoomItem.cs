using UnityEngine;
using System.Collections;

public class ZoomItem : MonoBehaviour
{
    // 缩放比例
    private const float scaleUp = 1.4f; // 变大比例
    private const float scaleDown = 0.75f; // 变小比例

    // 摄像机调整值
    private const float cameraAdjustUp = 2.5f; // 摄像机远离值
    private const float cameraAdjustDown = -100f; // 摄像机靠近值

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            int random = Random.Range(0, 2); // 0 或 1
            float targetScale = random == 0 ? scaleUp : scaleDown;
            float cameraAdjust = random == 0 ? cameraAdjustUp : cameraAdjustDown;
            PlayerManager.ScaleAllPlayers(targetScale);
            Camera.main.GetComponent<CameraFollow>().AdjustCamera(cameraAdjust);
            Destroy(gameObject);

            // 使用全局计时器延迟恢复和销毁
            TimerManager.Instance.StartTimer(5f, RestoreAndDestroy);
        }
    }

    private void RestoreAndDestroy()
    {
        Debug.Log("Restoring player scales after zoom item effect.");
        PlayerManager.RestoreAllPlayersScale();
        // 这里是最后一句
    }

}