using UnityEngine;

public class Sword : DeathTrap
{
    [SerializeField] private float speed = 5f;

    void Start()
    {
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime, Space.Self);
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other); // Call the base class's lethal logic directly
    }
}