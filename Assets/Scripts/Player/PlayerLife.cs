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
    [SerializeField] private GameObject shieldPrefab; // 拖你的泡泡预制体到这里
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

        // 播放死亡音效
        deathSound.Play();

        // 播放死亡动画
        animator.SetTrigger("death");

        CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.ShakeCamera();
        }

        // 通知 PlayerManager 检查所有玩家是否死亡
        PlayerManager.CheckAllPlayersDead();

        // 延迟销毁小人，确保动画和音效播放完成
        Destroy(gameObject, 0.5f); // 0.5 秒后销毁，可根据动画时长调整
    }
}
