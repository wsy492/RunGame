using UnityEngine;

public abstract class AttractWallBase : MonoBehaviour
{
    [SerializeField] protected float horizontalAttractForce = 20f; // 水平方向吸引力
    [SerializeField] protected float verticalAttractForce = 15f;   // 垂直方向吸引力
    [SerializeField] protected float attractDistance = 5f;        // 吸引距离

    protected virtual void FixedUpdate()
    {
        // 吸引所有小人靠近墙体表面
        Collider2D wallCollider = GetComponent<Collider2D>();
        if (wallCollider == null) return;

        foreach (var player in PlayerManager.GetAlivePlayers())
        {
            if (player == null) continue;
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb == null) continue;

            // 找到小人到墙的最近点
            Vector2 playerPos = player.transform.position;
            Vector2 closestPoint = wallCollider.ClosestPoint(playerPos);

            float distance = Vector2.Distance(playerPos, closestPoint);
            if (distance <= attractDistance)
            {
                // 计算吸引方向（指向墙体表面）
                Vector2 dir = (closestPoint - playerPos).normalized;

                // 根据吸引方向调整吸引力大小
                float dotProduct = Vector2.Dot(dir, Physics2D.gravity.normalized); // 吸引方向与重力方向的点积
                float attractForce = dotProduct > 0 ? verticalAttractForce : horizontalAttractForce;

                // 施加吸引力
                rb.AddForce(dir * attractForce);

                Debug.Log("gravity: " + Physics2D.gravity + "" + dir + ", attractForce: " + attractForce);
            }
        }
    }

    protected abstract void OnCollisionEnter2D(Collision2D collision);
}