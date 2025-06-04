using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerLife : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    [SerializeField] private AudioSource deathSound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Trap"))
        {
            deathSound.Play();
            Die();
        }
    }

    public void Die()
    {
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
