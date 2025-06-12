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
            // Rotate the camera
            cameraFollow.RotateCamera();

            // Rotate the global gravity direction
            RotateGlobalGravity(1); // Rotate 90 degrees counterclockwise
        }
    }

    private void RotateGlobalGravity(float direction)
    {
        // Rotate the global gravity direction
        Physics2D.gravity = Quaternion.Euler(0, 0, 90 * direction) * Physics2D.gravity;

        foreach (var player in PlayerManager.GetAlivePlayers())
        {
            player.RotateGravity(direction);
        }
    }

}