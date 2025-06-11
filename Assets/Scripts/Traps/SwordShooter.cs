using UnityEngine;

public class SwordShooter : MonoBehaviour
{
    public GameObject swordPrefab;      // 剑的预制体
    public Transform firePoint;         // 发射点
    public float fireInterval = 2f;     // 发射间隔（秒）
    public float swordSpeed = 8f;       // 剑的速度

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= fireInterval)
        {
            FireSword();
            timer = 0f;
        }
    }

    void FireSword()
    {
        GameObject sword = Instantiate(swordPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = sword.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = firePoint.right * swordSpeed; // firePoint的右方向为发射方向
        }
    }
}