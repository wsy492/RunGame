using UnityEngine;

public class SpikedBall : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints; // 路线上的点
    [SerializeField] private float speed = 2f; // 移动速度
    private int currentWaypointIndex = 0;

    private void Update()
    {
        MoveAlongPath();
    }

    private void MoveAlongPath()
    {
        if (waypoints.Length == 0) return;

        // 获取当前目标点
        Transform targetWaypoint = waypoints[currentWaypointIndex];

        // 移动到目标点
        transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, speed * Time.deltaTime);

        // 如果到达目标点，切换到下一个点
        if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length; // 循环路线
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // 触发玩家死亡逻辑
            collision.gameObject.GetComponent<PlayerLife>().Die();
        }
    }
}