using UnityEngine;

public class SwordShooter : MonoBehaviour
{
    public GameObject swordPrefab;      // Sword prefab
    public Transform firePoint;         // Firing point
    public float fireInterval = 2f;     // Firing interval (seconds)
    public float swordSpeed = 8f;       // Sword speed

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
            rb.linearVelocity = firePoint.right * swordSpeed; // firePoint's right direction is the firing direction
        }
    }
}