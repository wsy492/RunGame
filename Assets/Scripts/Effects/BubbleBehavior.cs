using UnityEngine;

public class BubbleBehavior : MonoBehaviour
{
    public float riseSpeed = 1f; // 泡泡上升速度
    public float lifeTime = 2f;  // 泡泡存活时间 (在水中的最大存活时间)
    public Vector2 initialForceRangeX = new Vector2(-0.2f, 0.2f); // X轴初始随机力的范围

    private bool isInWater = false; // 默认泡泡不在水中，需要检测来确认

    void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = -0.1f;
        rb.linearDamping = 0.5f;

        float randomXForce = Random.Range(initialForceRangeX.x, initialForceRangeX.y);
        rb.AddForce(new Vector2(randomXForce, 0), ForceMode2D.Impulse);

        // 设置一个最大生命周期，如果泡泡一直在水中，则按此时间销毁
        // 如果泡泡从未进入水中或离开水，它会因为 Update() 中的检查而更快被销毁
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 如果检测到泡泡不在水中，立即销毁它
        if (!isInWater)
        {
            Destroy(gameObject);
            // Debug.Log("Bubble destroyed because it's not in water.");
        }
    }

    // 当泡泡的碰撞体进入其他触发器时调用
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Trap"))
        {
            Destroy(gameObject);
            // Debug.Log("Bubble hit a Trap and is being destroyed.");
            return; // 泡泡已销毁
        }
        if (other.CompareTag("Water"))
        {
            isInWater = true;
            // Debug.Log("Bubble entered Water.");
        }
    }

    // 当泡泡的碰撞体停留在其他触发器内时，每帧调用
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Water"))
        {
            // 持续确认泡泡在水中，这对于在水体内部生成泡泡的情况很重要
            if (!isInWater) // 只有在状态改变时才打印日志，避免刷屏
            {
                // Debug.Log("Bubble is confirmed to be in Water (OnTriggerStay).");
            }
            isInWater = true;
        }
    }

    // 当泡泡的碰撞体离开其他触发器时调用
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Water"))
        {
            isInWater = false;
            // Debug.Log("Bubble exited Water.");
            // Update() 方法会在下一帧处理销毁逻辑
        }
    }
}
