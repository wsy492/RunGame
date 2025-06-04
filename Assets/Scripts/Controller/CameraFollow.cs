using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField] private float smoothTime = 0.05f; // 平滑时间
    [SerializeField] private float rotationSpeed = 200f; // 相机旋转速度
    [SerializeField] private float minOrthographicSize = 2f; // 摄像机最小范围
    [SerializeField] private float maxOrthographicSize = 20f; // 摄像机最大范围
    [SerializeField] private float sizePadding = 2f; // 额外的边距

    private Vector3 velocity = Vector3.zero;
    private bool isRotating = false; // 是否正在旋转
    private Quaternion targetRotation; // 目标旋转
    private Camera cam; // 摄像机组件
    private bool isZooming = false;
    private float zoomLockTimer = 0f;
    private float zoomLockDuration = 5f; // 缩放后锁定5秒
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0.3f;
    private float shakeFadeSpeed = 2f;
    private Vector3 originalPos;
    private Vector3 shakeOffset = Vector3.zero;

    private void Start()
    {
        targetRotation = transform.rotation; // 初始化目标旋转
        cam = GetComponent<Camera>(); // 获取摄像机组件
        originalPos = transform.localPosition;
    }

    private void FixedUpdate()
    {
        if (!PlayerManager.Instance || !PlayerManager.Instance.isGameStarted)
            return;

        List<PlayerMovement> players = PlayerManager.GetAlivePlayers();
        if (players.Count == 0)
            return;

        // 获取所有玩家的最大速度
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

        // 跟随小人中心点
        Vector3 center = GetCenterPoint(players);
        Vector3 targetPosition = center + offset;

        // 动态调整smoothTime，速度越快，smoothTime越小
        float minSmoothTime = 0.01f;
        float maxSmoothTime = 0.15f;
        float speedThreshold = 10f; // 你可以根据实际调整
        float t = Mathf.Clamp01(maxPlayerSpeed / speedThreshold);
        float dynamicSmoothTime = Mathf.Lerp(maxSmoothTime, minSmoothTime, t);

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, dynamicSmoothTime);

        // 5秒内锁定缩放，不自动调整
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

        // 平滑旋转相机
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

        // 2. 计算抖动偏移
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

        // 3. 应用抖动
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime) + shakeOffset;
    }

    public void ShakeCamera(float duration = 0.2f, float magnitude = 0.3f)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
    }
    // private Vector3 GetCenterPoint(List<PlayerMovement> players)
    // {
    //     if (players.Count == 1)
    //         return players[0].transform.position;

    //     // 计算所有玩家的中心点
    //     Bounds bounds = new Bounds(players[0].transform.position, Vector3.zero);
    //     for (int i = 1; i < players.Count; i++)
    //     {
    //         bounds.Encapsulate(players[i].transform.position);
    //     }

    //     // 忽略距离中心点过远的小人
    //     float maxDistance = 10f; // 距离阈值
    //     Vector3 center = bounds.center;
    //     List<PlayerMovement> closePlayers = players.FindAll(player =>
    //         Vector3.Distance(player.transform.position, center) <= maxDistance);

    //     if (closePlayers.Count == 0)
    //         return center; // 如果没有小人满足条件，返回原中心点

    //     // 重新计算中心点
    //     bounds = new Bounds(closePlayers[0].transform.position, Vector3.zero);
    //     for (int i = 1; i < closePlayers.Count; i++)
    //     {
    //         bounds.Encapsulate(closePlayers[i].transform.position);
    //     }

    //     return bounds.center;
    // }

    // private void AdjustCameraSize(List<PlayerMovement> players)
    // {
    //     if (cam.orthographic)
    //     {
    //         // 计算所有玩家的中心点
    //         Vector3 center = GetCenterPoint(players);

    //         // 忽略距离中心点过远的小人
    //         float maxDistance = 10f; // 距离阈值
    //         List<PlayerMovement> closePlayers = players.FindAll(player =>
    //             Vector3.Distance(player.transform.position, center) <= maxDistance);

    //         if (closePlayers.Count == 0)
    //             return; // 如果没有小人满足条件，不调整摄像机大小

    //         // 计算边界
    //         Bounds bounds = new Bounds(closePlayers[0].transform.position, Vector3.zero);
    //         foreach (var player in closePlayers)
    //         {
    //             bounds.Encapsulate(player.transform.position);
    //         }

    //         // 根据边界的大小调整摄像机的正交大小
    //         float requiredSize = Mathf.Max(bounds.size.x, bounds.size.y) / 2f + sizePadding;
    //         cam.orthographicSize = Mathf.Clamp(requiredSize, minOrthographicSize, maxOrthographicSize);
    //     }
    // }

    private void AdjustCameraSize(List<PlayerMovement> players)
    {
        if (cam.orthographic)
        {
            // 计算所有玩家的中心点
            Vector3 center = GetCenterPoint(players);

            // 忽略距离中心点过远的小人，并将其移除
            float maxDistance = 15f; // 距离阈值
            List<PlayerMovement> closePlayers = new List<PlayerMovement>();
            foreach (var player in players)
            {
                if (Vector3.Distance(player.transform.position, center) <= maxDistance)
                {
                    closePlayers.Add(player);
                }
                else
                {
                    // 如果小人距离中心点过远，直接移除或标记为死亡
                    Debug.Log($"小人 {player.name} 距离过远，被移除");
                    PlayerManager.Unregister(player); // 从玩家管理器中移除
                    Destroy(player.gameObject); // 销毁小人对象
                }
            }

            if (closePlayers.Count == 0)
                return; // 如果没有小人满足条件，不调整摄像机大小

            // 计算边界
            Bounds bounds = new Bounds(closePlayers[0].transform.position, Vector3.zero);
            foreach (var player in closePlayers)
            {
                bounds.Encapsulate(player.transform.position);
            }

            // 根据边界的大小调整摄像机的正交大小
            float requiredSize = Mathf.Max(bounds.size.x, bounds.size.y) / 2f + sizePadding;
            cam.orthographicSize = Mathf.Clamp(requiredSize, minOrthographicSize, maxOrthographicSize);
        }
    }

    private Vector3 GetCenterPoint(List<PlayerMovement> players)
    {
        if (players.Count == 1)
            return players[0].transform.position;

        // 计算所有玩家的中心点
        Bounds bounds = new Bounds(players[0].transform.position, Vector3.zero);
        for (int i = 1; i < players.Count; i++)
        {
            bounds.Encapsulate(players[i].transform.position);
        }

        // 忽略距离中心点过远的小人
        float maxDistance = 10f; // 距离阈值
        Vector3 center = bounds.center;
        List<PlayerMovement> closePlayers = players.FindAll(player =>
            Vector3.Distance(player.transform.position, center) <= maxDistance);

        if (closePlayers.Count == 0)
            return center; // 如果没有小人满足条件，返回原中心点

        // 重新计算中心点
        bounds = new Bounds(closePlayers[0].transform.position, Vector3.zero);
        for (int i = 1; i < closePlayers.Count; i++)
        {
            bounds.Encapsulate(closePlayers[i].transform.position);
        }

        return bounds.center;
    }

    public void RotateCamera()
    {
        // 设置目标旋转为逆时针旋转 90 度
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