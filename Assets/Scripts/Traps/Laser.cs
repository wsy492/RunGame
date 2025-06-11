using UnityEngine;

public class Laser : MonoBehaviour
{
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        // 只要激光在摄像机视野内，按下Shift就销毁激光
        if (rend != null && rend.isVisible)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            {
                Destroy(gameObject);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 只有激光在摄像机视野内才生效
        if (rend != null && rend.isVisible)
        {
            if (other.CompareTag("Player"))
            {
                // 获取PlayerLife脚本并调用死亡方法
                PlayerLife playerLife = other.GetComponent<PlayerLife>();
                if (playerLife != null)
                {
                    playerLife.Die();
                }
            }
        }
    }
}