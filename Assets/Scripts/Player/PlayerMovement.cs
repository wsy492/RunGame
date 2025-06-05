using UnityEngine;
using System.Collections; // 使用协程需要引入

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private BoxCollider2D coll;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    [SerializeField] private LayerMask jumpableGround;
    [SerializeField] private AudioSource jumpSound;
    [SerializeField] private AudioSource deathSound;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float maxSpeed = 10f;

    // 水域中的属性
    [SerializeField] private float waterGravityScale = 0.5f;
    [SerializeField] private float waterMoveSpeed = 2f;
    [SerializeField] private float waterMaxSpeed = 4f;

    // 用于存储原始属性
    private float originalGravityScale;
    private float originalMoveSpeed;
    private float originalMaxSpeed;

    // 判断是否在水中
    private bool isInWater = false;

    public bool isAlive = true;
    public bool canJump = true;

    // 新增：吐泡泡效果相关
    [Header("水下效果 (Water Effects)")] // 在Inspector中添加一个标题，方便区分
    [SerializeField] private GameObject bubblePrefab; // 将你的泡泡预制体拖拽到这里
    [SerializeField] private Transform bubbleSpawnPoint; // 泡泡生成的位置 (可选, 可以是玩家的一个子对象)
    [SerializeField] private float bubbleSpawnInterval = 0.5f; // 生成泡泡的间隔时间 (秒)

    private Coroutine bubbleCoroutine; // 用于控制泡泡生成的协程

    private enum MovementState
    {
        idle,
        running,
        jumping,
        falling
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        originalGravityScale = rb.gravityScale;
        originalMoveSpeed = moveSpeed;
        originalMaxSpeed = maxSpeed;
    }

    private void Start()
    {
        PlayerManager.Register(this);
        // 如果没有指定泡泡生成点，默认使用玩家自身的位置
        if (bubbleSpawnPoint == null)
        {
            bubbleSpawnPoint = transform;
        }
    }

    private void OnDestroy()
    {
        PlayerManager.Unregister(this);
        StopBubbleEffect(); // 确保对象销毁时停止协程
    }

    private void Update()
    {
        UpdateAnimation();
    }

    public void Move(float direction)
    {
        if (!isAlive || rb.bodyType != RigidbodyType2D.Dynamic) return;

        Vector2 gravityDir = Physics2D.gravity.normalized;
        Vector2 moveDir = new Vector2(-gravityDir.y, gravityDir.x);
        // 当前实际使用的移动速度 (考虑是否在水中)
        float currentEffectiveMoveSpeed = isInWater ? waterMoveSpeed : this.moveSpeed;
        float targetHorizontalSpeed = direction * currentEffectiveMoveSpeed;


        float wallCheckDistance = 0.1f;
        bool isTouchingWall = false;
        if (direction > 0)
        {
            isTouchingWall = Physics2D.Raycast(coll.bounds.center, moveDir, wallCheckDistance, jumpableGround);
        }
        else if (direction < 0)
        {
            isTouchingWall = Physics2D.Raycast(coll.bounds.center, -moveDir, wallCheckDistance, jumpableGround);
        }
        if (isTouchingWall)
        {
            targetHorizontalSpeed *= 0.2f;
        }

        Vector2 velocity = rb.linearVelocity;
        float currentHorizontalSpeed = Vector2.Dot(velocity, moveDir);
        velocity += moveDir * (targetHorizontalSpeed - currentHorizontalSpeed);
        rb.linearVelocity = velocity;
    }

    public void TryJump()
    {
        if (!canJump) return;

        rb.linearVelocity = Vector2.zero;
        Vector2 jumpDirection = -Physics2D.gravity.normalized;
        rb.AddForce(jumpDirection * jumpForce, ForceMode2D.Impulse);

        canJump = false;
        Invoke(nameof(Recevory_jump), 1f);
    }

    public void Recevory_jump()
    {
        canJump = true;
    }

    private void UpdateAnimation()
    {
        MovementState state;
        Vector2 gravityDir = Physics2D.gravity.normalized;
        Vector2 moveDir = new Vector2(-gravityDir.y, gravityDir.x);
        float horizontal = Vector2.Dot(rb.linearVelocity, moveDir);

        if (horizontal > 0.1f)
        {
            state = MovementState.running;
            spriteRenderer.flipX = false;
        }
        else if (horizontal < -0.1f)
        {
            state = MovementState.running;
            spriteRenderer.flipX = true;
        }
        else
        {
            state = MovementState.idle;
        }

        // 注意：这里的垂直速度判断是基于世界坐标系的Y轴。
        // 如果你的游戏重力会旋转，这里的跳跃/下落状态判断可能需要调整为基于当前重力方向的速度分量。
        float verticalVelocityAlongGravity = Vector2.Dot(rb.linearVelocity, -Physics2D.gravity.normalized);
        if (verticalVelocityAlongGravity > 0.1f) // 向上运动（相对于反重力方向）
        {
            state = MovementState.jumping;
        }
        else if (verticalVelocityAlongGravity < -0.1f) // 向下运动（相对于反重力方向，即掉落）
        {
            state = MovementState.falling;
        }
        // 如果重力方向是标准的 (0, -g), 那么 rb.linearVelocity.y > 0.1f 就是跳跃
        // if (Physics2D.gravity.y < 0 && rb.linearVelocity.y > 0.1f)
        // {
        //     state = MovementState.jumping;
        // }
        // else if (Physics2D.gravity.y < 0 && rb.linearVelocity.y < -0.1f)
        // {
        //     state = MovementState.falling;
        // }
        // // 你可能需要为其他重力方向添加更复杂的判断逻辑

        animator.SetInteger("state", (int)state);
    }

    private void FixedUpdate()
    {
        // UpdateAnimation(); // 已经在 Update 中调用了

        if (rb != null)
        {
            Vector2 gravityDir = Physics2D.gravity.normalized;
            Vector2 moveDir = new Vector2(-gravityDir.y, gravityDir.x);
            float gravitySpeed = Vector2.Dot(rb.linearVelocity, gravityDir);
            float currentMoveComponentSpeed = Vector2.Dot(rb.linearVelocity, moveDir); // 清晰起见重命名

            bool changed = false;
            // 使用当前环境下的最大速度 (水中或陆地)
            float currentEffectiveMaxSpeed = isInWater ? waterMaxSpeed : this.maxSpeed;

            if (Mathf.Abs(gravitySpeed) > currentEffectiveMaxSpeed) // 最大速度应该应用于整体速度还是分量？这里应用于分量
            {
                gravitySpeed = Mathf.Sign(gravitySpeed) * currentEffectiveMaxSpeed;
                changed = true;
            }
            if (Mathf.Abs(currentMoveComponentSpeed) > currentEffectiveMaxSpeed)
            {
                currentMoveComponentSpeed = Mathf.Sign(currentMoveComponentSpeed) * currentEffectiveMaxSpeed;
                changed = true;
            }
            if (changed)
            {
                rb.linearVelocity = gravityDir * gravitySpeed + moveDir * currentMoveComponentSpeed;
            }
        }
    }

    public void Die()
    {
        isAlive = false;
        gameObject.SetActive(false);
        StopBubbleEffect(); // 死亡时停止吐泡泡
    }

    public void RotateGravity(float direction)
    {
        if (rb.bodyType != RigidbodyType2D.Dynamic) return;

        transform.Rotate(0, 0, 90 * direction);
        rb.WakeUp();
        Vector2 gravityDir = Physics2D.gravity.normalized;
        rb.linearVelocity = gravityDir * -2f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Water") && !isInWater)
        {
            isInWater = true;
            rb.gravityScale = waterGravityScale;
            // moveSpeed 和 maxSpeed 现在会在 Move() 和 FixedUpdate() 中根据 isInWater 动态使用
            // 所以这里的直接赋值如果那些方法总是检查 isInWater，则不那么关键。
            // 但为了清晰或其他系统依赖，显式设置也可以。
            // this.moveSpeed = waterMoveSpeed; // 这会覆盖基础的 moveSpeed
            // this.maxSpeed = waterMaxSpeed;   // 这会覆盖基础的 maxSpeed
            StartBubbleEffect(); // 进入水中，开始吐泡泡
            Debug.Log("Entered Water");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Water") && isInWater)
        {
            isInWater = false;
            rb.gravityScale = originalGravityScale;
            // this.moveSpeed = originalMoveSpeed;
            // this.maxSpeed = originalMaxSpeed;
            StopBubbleEffect(); // 离开水中，停止吐泡泡
            Debug.Log("Exited Water");
        }
    }

    // --- 泡泡效果方法 ---
    private void StartBubbleEffect()
    {
        // 确保有泡泡预制体，并且协程当前没有在运行
        if (bubblePrefab != null && bubbleCoroutine == null)
        {
            bubbleCoroutine = StartCoroutine(SpawnBubblesCoroutine());
        }
    }

    private void StopBubbleEffect()
    {
        if (bubbleCoroutine != null)
        {
            StopCoroutine(bubbleCoroutine);
            bubbleCoroutine = null; // 将引用置空，以便下次可以重新启动
        }
    }

    private IEnumerator SpawnBubblesCoroutine()
    {
        while (true) // 只要协程在运行，就持续生成
        {
            // 如果指定了 bubbleSpawnPoint，则使用它；否则使用玩家的位置
            Vector3 spawnPos = bubbleSpawnPoint != null ? bubbleSpawnPoint.position : transform.position;
            Instantiate(bubblePrefab, spawnPos, Quaternion.identity); // 在生成点实例化泡泡
            yield return new WaitForSeconds(bubbleSpawnInterval); // 等待指定间隔
        }
    }
}
