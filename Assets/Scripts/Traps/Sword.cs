using UnityEngine;

public class Sword : MonoBehaviour
{
    [SerializeField] private float speed = 5f; // 剑的飞行速度

    void Start()
    {
        Destroy(gameObject, 5f); // 5秒后自动销毁
    }

    void Update()
    {
        // 沿自身朝向匀速移动
        transform.Translate(Vector3.right * speed * Time.deltaTime, Space.Self);
    }

    // 检测玩家碰撞
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var playerLife = other.GetComponent<PlayerLife>();
            if (playerLife != null)
            {
                playerLife.Die();
            }
        }
        // 不销毁自己，不受任何影响
    }
}