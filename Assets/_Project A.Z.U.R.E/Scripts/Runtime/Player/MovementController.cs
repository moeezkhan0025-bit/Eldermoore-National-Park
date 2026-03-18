using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(Rigidbody2D))]
public class MovementController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 8f;
    [SerializeField] float acceleration = 15f;
    [SerializeField] float deceleration = 20f;

    [Header("Jump")]
    [SerializeField] float jumpForce = 16f;
    [SerializeField] float fallMultiplier = 2.5f;
    [SerializeField] float lowJumpMultiplier = 2f;
    [SerializeField] float coyoteTime = 0.12f;
    [SerializeField] float jumpBufferTime = 0.12f;

    [Header("Double Jump")]
    [SerializeField] float doubleJumpForce = 12f;
    [SerializeField] int maxJumpCount = 2;    // 2 = double jump, 3 = triple etc
    int currentJumpCount;
    float doubleJumpBufferTimer;

    [Header("Check Colliders")]
    [SerializeField] Collider2D groundCheck;
    [SerializeField] Collider2D leftWallCheck;
    [SerializeField] Collider2D rightWallCheck;
    [SerializeField] LayerMask groundLayer;

    [Header("Visual")]
    [SerializeField] Transform spriteChild;
    // Drag the Sprite child object here — we flip this, not the root

    Rigidbody2D rb;
    PlayerStateManager psm;
    PlayerInputHandler input;

    // ── Public read-only state for other systems ──
    public bool IsGrounded { get; private set; }
    public bool IsTouchingLeft { get; private set; }
    public bool IsTouchingRight { get; private set; }
    public bool FacingRight { get; private set; } = true;

    float coyoteTimer;
    float jumpBufferTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        psm = GetComponent<PlayerStateManager>();
        input = GetComponent<PlayerInputHandler>();
    }

    void Update()
    {
        UpdateChecks();
        UpdateTimers();
        HandleJumpBuffer();
        UpdateState();
    }

    void FixedUpdate()
    {
        HandleMovement();
        ApplyBetterGravity();
    }

    // ── Check all three colliders ─────────────────
    void UpdateChecks()
    {
        bool wasGrounded = IsGrounded;

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(groundLayer);
        filter.useTriggers = true;

        IsGrounded = groundCheck.IsTouching(filter);
        IsTouchingLeft = leftWallCheck.IsTouching(filter);
        IsTouchingRight = rightWallCheck.IsTouching(filter);

        if (IsGrounded)
        {
            coyoteTimer = coyoteTime;
            currentJumpCount = 0; // reset fully on landing
        }

        if (!wasGrounded && IsGrounded)
            psm.ChangeState(PlayerState.Landing);
    }

    void UpdateTimers()
    {
        coyoteTimer -= Time.deltaTime;
        jumpBufferTimer -= Time.deltaTime;
        doubleJumpBufferTimer -= Time.deltaTime;
    }

    void HandleJumpBuffer()
    {
        if (input.JumpPressed)
        {
            jumpBufferTimer = jumpBufferTime;
            doubleJumpBufferTimer = jumpBufferTime;
            Debug.Log($"[Jump] Pressed · Count:{currentJumpCount}/{maxJumpCount} · IsAirborne:{psm.IsAirborne}");
        }

        // First jump — from ground with coyote time
        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            ExecuteJump();
            jumpBufferTimer = 0f;
            doubleJumpBufferTimer = 0f;
            return;
        }

        // Additional jumps — counter check
        if (doubleJumpBufferTimer > 0f &&
            psm.IsAirborne &&
            currentJumpCount < maxJumpCount)
        {
            ExecuteDoubleJump();
        }
    }

    void ExecuteJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        coyoteTimer = 0f;
        jumpBufferTimer = 0f;
        currentJumpCount = 1; // first jump used
        psm.ChangeState(PlayerState.Jumping);
    }

    void ExecuteDoubleJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce);
        doubleJumpBufferTimer = 0f;
        currentJumpCount++;   // increment counter
        psm.ChangeState(PlayerState.DoubleJumping);
    }

    void HandleMovement()
    {
        float target = input.MoveInput * moveSpeed;
        float rate = Mathf.Abs(input.MoveInput) > 0.01f
                       ? acceleration : deceleration;
        float newX = Mathf.MoveTowards(
            rb.velocity.x, target, rate * Time.fixedDeltaTime);
        rb.velocity = new Vector2(newX, rb.velocity.y);
        HandleFlip();
    }

    // ── Flip the Sprite CHILD — not the root ─────
    void HandleFlip()
    {
        if (input.MoveInput > 0f && !FacingRight) Flip();
        else if (input.MoveInput < 0f && FacingRight) Flip();
    }

    void Flip()
    {
        FacingRight = !FacingRight;
        // Only flip the sprite child — colliders stay correct
        Vector3 s = spriteChild.localScale;
        spriteChild.localScale = new Vector3(-s.x, s.y, s.z);
    }

    void ApplyBetterGravity()
    {
        if (rb.velocity.y < 0f)
            rb.velocity += Vector2.up * Physics2D.gravity.y
                * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        else if (rb.velocity.y > 0f && !input.JumpHeld)
            rb.velocity += Vector2.up * Physics2D.gravity.y
                * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
        // PHASE 2: wall slide gravity override goes here
    }

    void UpdateState()
    {
        switch (psm.CurrentState)
        {
            case PlayerState.Landing:
                if (IsGrounded)
                    psm.ChangeState(Mathf.Abs(input.MoveInput) > 0.01f
                        ? PlayerState.Running : PlayerState.Idle);
                break;
            case PlayerState.Idle:
            case PlayerState.Running:
                if (!IsGrounded && coyoteTimer <= 0f)
                    psm.ChangeState(PlayerState.Falling);
                else
                    psm.ChangeState(Mathf.Abs(input.MoveInput) > 0.01f
                        ? PlayerState.Running : PlayerState.Idle);
                break;
            case PlayerState.Jumping:
                // Only go to Falling when actually moving downward
                if (rb.velocity.y < -0.1f)
                    psm.ChangeState(PlayerState.Falling);
                break;
            case PlayerState.Falling:
                if (IsGrounded)
                    psm.ChangeState(PlayerState.Landing);
                break;

            case PlayerState.DoubleJumping:
                if (rb.velocity.y < -0.1f)
                    psm.ChangeState(PlayerState.Falling);
                break;
        }
    }
}