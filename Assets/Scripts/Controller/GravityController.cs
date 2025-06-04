using UnityEngine;

public class GravityController : MonoBehaviour
{
    private CameraFollow cameraFollow;

    private void Start()
    {
        cameraFollow = Camera.main.GetComponent<CameraFollow>();
    }

    private void Update()
    {
        if (!PlayerManager.Instance || !PlayerManager.Instance.isGameStarted)
            return;

        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            // 旋转相机
            cameraFollow.RotateCamera();

            // 旋转全局重力方向
            RotateGlobalGravity(1); // 逆时针旋转 90 度
        }
    }

    private void RotateGlobalGravity(float direction)
    {
        // 旋转全局重力方向
        Physics2D.gravity = Quaternion.Euler(0, 0, 90 * direction) * Physics2D.gravity;

        foreach (var player in PlayerManager.GetAlivePlayers())
        {
            player.RotateGravity(direction);
        }
    }

}