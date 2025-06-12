using UnityEngine;
using System.Collections; // Required for coroutines

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

    // Properties in water
    [SerializeField] private float waterGravityScale = 4f;
    [SerializeField] private float waterMoveSpeed = 4f;
    [SerializeField] private float waterMaxSpeed = 7f;

    // Store original properties
    private float originalGravityScale;
    private float originalMoveSpeed;
    private float originalMaxSpeed;

    // Check if in water
    private bool isInWater = false;

    public bool isAlive = true;
    public bool canJump = true;
    private Coroutine speedEffectCoroutine;

    // Bubble effect
    [Header("Water Effects")]
    [SerializeField] private GameObject bubblePrefab; // Drag your bubble prefab here
    [SerializeField] private Transform bubbleSpawnPoint; // Bubble spawn position (optional, can be a child of the player)
    [SerializeField] private float bubbleSpawnInterval = 0.5f; // Bubble spawn interval (seconds)

    private Coroutine bubbleCoroutine; // Coroutine for bubble spawning

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
        // If no bubble spawn point is specified, use the player's own position
        if (bubbleSpawnPoint == null)
        {
            bubbleSpawnPoint = transform;
        }
    }

    private void OnDestroy()
    {
        PlayerManager.Unregister(this);
        StopBubbleEffect(); // Stop coroutine when object is destroyed
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
        // Current effective move speed (considering if in water)
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
        Debug.Log("Try to jump");

        if (jumpSound != null)
            jumpSound.Play(); // Play jump sound

        rb.linearVelocity = Vector2.zero;

        Vector2 jumpDirection;
        float force = jumpForce;

        if (isInWater)
        {
            Debug.Log("Jump in water");
            // Jump in water, direction is the same as on land, force can be reduced (e.g. 0.7x), or not
            jumpDirection = -Physics2D.gravity.normalized;
            force = jumpForce * 0.6f; // Less force in water
        }
        else
        {
            jumpDirection = -Physics2D.gravity.normalized;
            force = jumpForce;
        }

        rb.AddForce(jumpDirection * force, ForceMode2D.Impulse);

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

        // Note: The vertical speed judgment here is based on the Y axis of the world coordinate system.
        // If your game gravity rotates, the jump/fall state judgment here may need to be adjusted to the velocity component along the current gravity direction.
        float verticalVelocityAlongGravity = Vector2.Dot(rb.linearVelocity, -Physics2D.gravity.normalized);
        if (verticalVelocityAlongGravity > 0.1f) // Moving up (relative to anti-gravity direction)
        {
            state = MovementState.jumping;
        }
        else if (verticalVelocityAlongGravity < -0.1f) // Moving down (relative to anti-gravity direction, i.e. falling)
        {
            state = MovementState.falling;
        }
        // If gravity direction is standard (0, -g), then rb.linearVelocity.y > 0.1f is jumping
        // if (Physics2D.gravity.y < 0 && rb.linearVelocity.y > 0.1f)
        // {
        //     state = MovementState.jumping;
        // }
        // else if (Physics2D.gravity.y < 0 && rb.linearVelocity.y < -0.1f)
        // {
        //     state = MovementState.falling;
        // }
        // You may need to add more complex logic for other gravity directions

        animator.SetInteger("state", (int)state);
    }

    private void FixedUpdate()
    {
        // UpdateAnimation(); // Already called in Update

        if (rb != null)
        {
            Vector2 gravityDir = Physics2D.gravity.normalized;
            Vector2 moveDir = new Vector2(-gravityDir.y, gravityDir.x);
            float gravitySpeed = Vector2.Dot(rb.linearVelocity, gravityDir);
            float currentMoveComponentSpeed = Vector2.Dot(rb.linearVelocity, moveDir); // Renamed for clarity

            bool changed = false;
            // Use the maximum speed for the current environment (water or land)
            float currentEffectiveMaxSpeed = isInWater ? waterMaxSpeed : this.maxSpeed;

            if (Mathf.Abs(gravitySpeed) > currentEffectiveMaxSpeed) // Should max speed apply to total speed or component? Here it applies to component
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
        StopBubbleEffect(); // Stop bubble effect on death
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
            // moveSpeed and maxSpeed are now dynamically used in Move() and FixedUpdate() based on isInWater
            // So direct assignment here is not critical if those methods always check isInWater.
            // But for clarity or other system dependencies, explicit assignment is also fine.
            // this.moveSpeed = waterMoveSpeed; // This would overwrite the base moveSpeed
            // this.maxSpeed = waterMaxSpeed;   // This would overwrite the base maxSpeed
            StartBubbleEffect(); // Start bubble effect when entering water
            //Debug.Log("Entered Water");
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
            StopBubbleEffect(); // Stop bubble effect when exiting water
                                // Debug.Log("Exited Water");
        }
    }

    // --- Bubble effect methods ---
    private void StartBubbleEffect()
    {
        // Make sure there is a bubble prefab and the coroutine is not already running
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
            bubbleCoroutine = null; // Set to null so it can be restarted next time
        }
    }

    private IEnumerator SpawnBubblesCoroutine()
    {
        float soundTimer = 0f;
        while (true)
        {
            Vector3 spawnPos = bubbleSpawnPoint != null ? bubbleSpawnPoint.position : transform.position;
            Instantiate(bubblePrefab, spawnPos, Quaternion.identity);

            yield return new WaitForSeconds(bubbleSpawnInterval);
        }
    }

    public void ChangeSpeedTemp(float newMoveSpeed, float newMaxSpeed, float duration)
    {
        Debug.Log($"ChangeSpeedTemp called: moveSpeed={newMoveSpeed}, maxSpeed={newMaxSpeed}, duration={duration}");
        if (speedEffectCoroutine != null)
            StopCoroutine(speedEffectCoroutine);
        speedEffectCoroutine = StartCoroutine(SpeedEffectCoroutine(newMoveSpeed, newMaxSpeed, duration));
    }

    private IEnumerator SpeedEffectCoroutine(float newMoveSpeed, float newMaxSpeed, float duration)
    {
        float oldMoveSpeed = moveSpeed;
        float oldMaxSpeed = maxSpeed;
        moveSpeed = newMoveSpeed;
        maxSpeed = newMaxSpeed;
        yield return new WaitForSeconds(duration);
        moveSpeed = oldMoveSpeed;
        maxSpeed = oldMaxSpeed;
        speedEffectCoroutine = null;
    }
}