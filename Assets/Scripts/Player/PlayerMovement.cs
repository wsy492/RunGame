using UnityEngine;

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

    public bool isAlive = true;
    public bool canJump = true;

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
    }

    private void Start()
    {
        PlayerManager.Register(this);
    }

    private void OnDestroy()
    {
        PlayerManager.Unregister(this);
    }

    private void Update()
    {
        UpdateAnimation();
    }

    // public void Move(float direction)
    // {
    //     if (!isAlive || rb.bodyType != RigidbodyType2D.Dynamic) return; // 检查是否为 Dynamic

    //     // 计算与重力方向垂直的移动方向
    //     Vector2 gravityDir = Physics2D.gravity.normalized;
    //     Vector2 moveDir = new Vector2(-gravityDir.y, gravityDir.x); // 与重力垂直

    //     // 只设置沿moveDir方向的速度分量，保留原有沿重力方向的速度
    //     float currentGravitySpeed = Vector2.Dot(rb.linearVelocity, gravityDir);
    //     rb.linearVelocity = moveDir * (direction * moveSpeed) + gravityDir * currentGravitySpeed;
    // }

    public void Move(float direction)
    {
        if (!isAlive || rb.bodyType != RigidbodyType2D.Dynamic) return;

        Vector2 gravityDir = Physics2D.gravity.normalized;
        Vector2 moveDir = new Vector2(-gravityDir.y, gravityDir.x);

        // 当前速度在两个方向上的分量
        float currentGravitySpeed = Vector2.Dot(rb.linearVelocity, gravityDir);
        float targetHorizontalSpeed = direction * moveSpeed;

        // 检查是否贴墙，贴墙时减小水平速度
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

        // 只改变水平速度，竖直速度保持原值
        Vector2 velocity = rb.linearVelocity;
        // 计算当前水平速度分量
        float currentHorizontalSpeed = Vector2.Dot(velocity, moveDir);
        // 用目标水平速度替换原有水平分量
        velocity += moveDir * (targetHorizontalSpeed - currentHorizontalSpeed);
        rb.linearVelocity = velocity;
    }

    public void TryJump()
    {
        if (!canJump)
            return;

        rb.linearVelocity = Vector2.zero;

        // 根据当前重力方向跳跃
        Vector2 jumpDirection = -Physics2D.gravity.normalized; // 反向于重力方向
        rb.AddForce(jumpDirection * jumpForce, ForceMode2D.Impulse);

        canJump = false;
        Invoke(nameof(Recevory_jump), 1f);
    }

    // public bool IsGrounded()
    // {
    //     // 使用当前重力方向检测地面
    //     Vector2 gravityDirection = Physics2D.gravity.normalized; // 获取当前重力方向
    //     float distance = 0.2f; // 检测距离
    //     RaycastHit2D hit = Physics2D.Raycast(coll.bounds.center, -gravityDirection, distance, jumpableGround);

    //     return hit.collider != null;
    // }
    public void Recevory_jump()
    {
        canJump = true;
    }

    private void UpdateAnimation()
    {
        MovementState state;

        // 获取当前重力方向
        Vector2 gravityDir = Physics2D.gravity.normalized;
        // 获取与重力方向垂直的移动方向
        Vector2 moveDir = new Vector2(-gravityDir.y, gravityDir.x);

        // 计算沿移动方向的速度分量
        float horizontal = Vector2.Dot(rb.linearVelocity, moveDir);

        // 根据速度分量设置状态和朝向
        if (horizontal > 0.1f)
        {
            state = MovementState.running;
            spriteRenderer.flipX = false; // 面向右
        }
        else if (horizontal < -0.1f)
        {
            state = MovementState.running;
            spriteRenderer.flipX = true; // 面向左
        }
        else
        {
            state = MovementState.idle;
        }

        // 根据垂直速度设置跳跃或下落状态
        if (rb.linearVelocity.y > 0.1f)
        {
            state = MovementState.jumping;
        }
        else if (rb.linearVelocity.y < -0.1f)
        {
            state = MovementState.falling;
        }

        // 更新动画状态
        animator.SetInteger("state", (int)state);
    }

    // private void FixedUpdate()
    // {
    //     if (rb != null)
    //     {
    //         Vector2 gravityDir = Physics2D.gravity.normalized;
    //         Vector2 moveDir = new Vector2(-gravityDir.y, gravityDir.x);

    //         // 分别限制两个方向的速度
    //         float gravitySpeed = Vector2.Dot(rb.linearVelocity, gravityDir);
    //         float moveSpeedVal = Vector2.Dot(rb.linearVelocity, moveDir);

    //         // 限制沿重力方向的速度
    //         if (Mathf.Abs(gravitySpeed) > maxSpeed)
    //         {
    //             gravitySpeed = Mathf.Sign(gravitySpeed) * maxSpeed;
    //         }
    //         // 限制沿移动（吸引）方向的速度
    //         if (Mathf.Abs(moveSpeedVal) > maxSpeed)
    //         {
    //             moveSpeedVal = Mathf.Sign(moveSpeedVal) * maxSpeed;
    //         }

    //         rb.linearVelocity = gravityDir * gravitySpeed + moveDir * moveSpeedVal;
    //     }
    //     UpdateAnimation();
    // }

    private void FixedUpdate()
    {
        // 只做动画和速度限制，不再赋值rb.linearVelocity
        UpdateAnimation();

        // 可选：只在超速时修正速度
        if (rb != null)
        {
            Vector2 gravityDir = Physics2D.gravity.normalized;
            Vector2 moveDir = new Vector2(-gravityDir.y, gravityDir.x);

            float gravitySpeed = Vector2.Dot(rb.linearVelocity, gravityDir);
            float moveSpeedVal = Vector2.Dot(rb.linearVelocity, moveDir);

            bool changed = false;
            if (Mathf.Abs(gravitySpeed) > maxSpeed)
            {
                gravitySpeed = Mathf.Sign(gravitySpeed) * maxSpeed;
                changed = true;
            }
            if (Mathf.Abs(moveSpeedVal) > maxSpeed)
            {
                moveSpeedVal = Mathf.Sign(moveSpeedVal) * maxSpeed;
                changed = true;
            }
            if (changed)
            {
                rb.linearVelocity = gravityDir * gravitySpeed + moveDir * moveSpeedVal;
            }
        }
    }

    public void Die()
    {
        isAlive = false;
        gameObject.SetActive(false);
    }


    public void RotateGravity(float direction)
    {
        if (rb.bodyType != RigidbodyType2D.Dynamic) return; // 检查是否为 Dynamic

        // 旋转玩家
        transform.Rotate(0, 0, 90 * direction);

        // 唤醒 Rigidbody
        rb.WakeUp();

        // 让速度直接对齐新重力方向，给予一个“下落”速度
        Vector2 gravityDir = Physics2D.gravity.normalized;
        rb.linearVelocity = gravityDir * -2f; // -2f表示朝重力方向下落，数值可根据手感调整
    }

}
