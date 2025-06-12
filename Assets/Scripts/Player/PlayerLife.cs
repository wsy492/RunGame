using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerLife : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    [SerializeField] private AudioSource deathSound;
    [SerializeField] private AudioSource shieldBlockSound;
    private bool isShielded = false;
    private float shieldTimer = 0f;
    [SerializeField] private GameObject shieldPrefab; // Drag your shield prefab here
    private GameObject shieldInstance;
    private float spawnInvincibleTime = 0.5f;
    private float spawnTimer = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spawnTimer = spawnInvincibleTime;
    }

    void Update()
    {
        if (spawnTimer > 0f)
            spawnTimer -= Time.deltaTime;

        if (isShielded)
        {
            shieldTimer -= Time.deltaTime;
            if (shieldTimer <= 0f)
            {
                isShielded = false;
                if (shieldInstance != null)
                    shieldInstance.SetActive(false);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (spawnTimer > 0f) return;

        if (collision.gameObject.CompareTag("Trap"))
        {
            if (isShielded)
            {
                if (shieldBlockSound != null)
                    shieldBlockSound.Play();
                return;
            }
            if (deathSound != null)
                deathSound.Play();
            Die();
        }
    }

    public void ActivateShield(float duration)
    {
        isShielded = true;
        shieldTimer = duration;

        if (shieldInstance == null && shieldPrefab != null)
        {
            shieldInstance = Instantiate(shieldPrefab, transform);
            shieldInstance.transform.localPosition = Vector3.zero;
            shieldInstance.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
        }
        else if (shieldInstance != null)
        {
            shieldInstance.SetActive(true);
            shieldInstance.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
        }
    }

    public bool IsShielded()
    {
        return isShielded;
    }

    public void Die()
    {
        if (isShielded)
        {
            if (shieldBlockSound != null)
                shieldBlockSound.Play();
            return;
        }

        // Play death sound effect
        deathSound.Play();

        // Play death animation
        animator.SetTrigger("death");

        CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.ShakeCamera();
        }

        // Notify PlayerManager to check if all players are dead
        PlayerManager.CheckAllPlayersDead();

        // Delay destroying the player to ensure animation and sound finish playing
        Destroy(gameObject, 0.5f); // Destroy after 0.5 seconds, adjust according to animation length
    }
}