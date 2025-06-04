using UnityEngine;

public class CherryItem : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 检查碰撞的对象是否是玩家
        if (collision.CompareTag("Player"))
        {
            // 增加 Cherry 数量
            CherryManager.totalCherries++;

            // 销毁当前 Cherry
            Destroy(gameObject);
        }
    }
}