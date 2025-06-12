using UnityEngine;

public class Laser : DeathTrap
{
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        // As long as the laser is visible in the camera view, pressing Shift will destroy the laser
        if (rend != null && rend.isVisible)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            {
                Destroy(gameObject);
            }
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        // 只有在可见时才触发致死逻辑
        if (rend != null && rend.isVisible)
        {
            base.OnTriggerEnter2D(other); // 复用 DeathTrap 的致死逻辑
        }
    }
}