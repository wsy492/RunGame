using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField] private float smoothTime = 0.05f; // Smoothing time
    [SerializeField] private float rotationSpeed = 200f; // Camera rotation speed
    [SerializeField] private float minOrthographicSize = 2f; // Minimum camera orthographic size
    [SerializeField] private float maxOrthographicSize = 30f; // Maximum camera orthographic size
    [SerializeField] private float sizePadding = 2f; // Extra padding for camera size

    private Vector3 velocity = Vector3.zero;
    private bool isRotating = false; // Is the camera rotating
    private Quaternion targetRotation; // Target rotation
    private Camera cam; // Camera component
    private bool isZooming = false;
    private float zoomLockTimer = 0f;
    private float zoomLockDuration = 5f; // Lock zoom for 5 seconds after zooming
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0.3f;
    private float shakeFadeSpeed = 2f;
    private Vector3 originalPos;
    private Vector3 shakeOffset = Vector3.zero;

    private void Start()
    {
        targetRotation = transform.rotation; // Initialize target rotation
        cam = GetComponent<Camera>(); // Get camera component
        originalPos = transform.localPosition;
    }

    private void FixedUpdate()
    {
        if (!PlayerManager.Instance || !PlayerManager.Instance.isGameStarted)
            return;

        List<PlayerMovement> players = PlayerManager.GetAlivePlayers();
        if (players.Count == 0)
            return;

        // Get the maximum speed of all players
        float maxPlayerSpeed = 0f;
        foreach (var player in players)
        {
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                float speed = rb.linearVelocity.magnitude;
                if (speed > maxPlayerSpeed)
                    maxPlayerSpeed = speed;
            }
        }

        // Follow the center point of all players
        Vector3 center = GetCenterPoint(players);
        Vector3 targetPosition = center + offset;

        // Dynamically adjust smoothTime: the faster the speed, the smaller the smoothTime
        float minSmoothTime = 0.01f;
        float maxSmoothTime = 0.15f;
        float speedThreshold = 10f; // Adjust as needed
        float t = Mathf.Clamp01(maxPlayerSpeed / speedThreshold);
        float dynamicSmoothTime = Mathf.Lerp(maxSmoothTime, minSmoothTime, t);

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, dynamicSmoothTime);

        // Lock zoom for 5 seconds, do not auto adjust during this period
        if (isZooming)
        {
            zoomLockTimer -= Time.fixedDeltaTime;
            if (zoomLockTimer <= 0f)
            {
                isZooming = false;
            }
        }
        else
        {
            AdjustCameraSize(players);
        }

        // Smoothly rotate the camera
        if (isRotating)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                transform.rotation = targetRotation;
                isRotating = false;
            }
        }

        Vector3 targetPos = GetCenterPoint(players) + offset;

        // 2. Calculate shake offset
        if (shakeDuration > 0)
        {
            shakeOffset = Random.insideUnitSphere * shakeMagnitude;
            shakeDuration -= Time.fixedDeltaTime * shakeFadeSpeed;
            if (shakeDuration <= 0)
            {
                shakeDuration = 0;
                shakeOffset = Vector3.zero;
            }
        }
        else
        {
            shakeOffset = Vector3.zero;
        }

        // 3. Apply shake
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime) + shakeOffset;
    }

    public void ShakeCamera(float duration = 0.2f, float magnitude = 0.3f)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
    }

    private void AdjustCameraSize(List<PlayerMovement> players)
    {
        if (cam.orthographic)
        {
            // Calculate the center point of all players
            Vector3 center = GetCenterPoint(players);

            // Ignore players too far from the center and remove them
            float maxDistance = 15f; // Distance threshold
            List<PlayerMovement> closePlayers = new List<PlayerMovement>();
            foreach (var player in players)
            {
                if (Vector3.Distance(player.transform.position, center) <= maxDistance)
                {
                    closePlayers.Add(player);
                }
                else
                {
                    // If a player is too far from the center, remove or mark as dead
                    Debug.Log($"Player {player.name} is too far and has been removed");
                    PlayerManager.Unregister(player); // Remove from player manager
                    Destroy(player.gameObject); // Destroy player object
                }
            }

            if (closePlayers.Count == 0)
                return; // If no players meet the condition, do not adjust camera size

            // Calculate bounds
            Bounds bounds = new Bounds(closePlayers[0].transform.position, Vector3.zero);
            foreach (var player in closePlayers)
            {
                bounds.Encapsulate(player.transform.position);
            }

            // Adjust camera orthographic size based on bounds
            float requiredSize = Mathf.Max(bounds.size.x, bounds.size.y) / 2f + sizePadding;
            cam.orthographicSize = Mathf.Clamp(requiredSize, minOrthographicSize, maxOrthographicSize);
        }
    }

    private Vector3 GetCenterPoint(List<PlayerMovement> players)
    {
        if (players.Count == 1)
            return players[0].transform.position;

        // Calculate the center point of all players
        Bounds bounds = new Bounds(players[0].transform.position, Vector3.zero);
        for (int i = 1; i < players.Count; i++)
        {
            bounds.Encapsulate(players[i].transform.position);
        }

        // Ignore players too far from the center
        float maxDistance = 10f; // Distance threshold
        Vector3 center = bounds.center;
        List<PlayerMovement> closePlayers = players.FindAll(player =>
            Vector3.Distance(player.transform.position, center) <= maxDistance);

        if (closePlayers.Count == 0)
            return center; // If no players meet the condition, return the original center

        // Recalculate center point
        bounds = new Bounds(closePlayers[0].transform.position, Vector3.zero);
        for (int i = 1; i < closePlayers.Count; i++)
        {
            bounds.Encapsulate(closePlayers[i].transform.position);
        }

        return bounds.center;
    }

    public void RotateCamera()
    {
        // Set the target rotation to rotate 90 degrees counterclockwise
        targetRotation *= Quaternion.Euler(0, 0, 90);
        isRotating = true;
    }


    public void AdjustCamera(float adjustValue)
    {
        if (cam.orthographic)
        {
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize + adjustValue, minOrthographicSize, maxOrthographicSize);
            isZooming = true;
            zoomLockTimer = zoomLockDuration;
        }
    }

}