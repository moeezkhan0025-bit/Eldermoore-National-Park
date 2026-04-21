using System.Collections;
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
    [SerializeField] int maxJumpCount = 2;
    int currentJumpCount;
    float doubleJumpBufferTimer;

    [Header("Wall Slide")]
    [SerializeField] float wallSlideSpeed = 1.5f;

    [Header("Wall Jump")]
    [SerializeField] float wallJumpForceY = 14f;

    [Header("Drop Through")]
    [SerializeField] string platformLayer = "OneWayPlatform";
    [SerializeField] float dropCooldown = 0.4f;

    [Header("Check Colliders")]
    [SerializeField] Collider2D groundCheck;
    [SerializeField] Collider2D leftWallCheck;
    [SerializeField] Collider2D rightWallCheck;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask wallLayer;

    [Header("Visual")]
    [SerializeField] Transform spriteChild;

    Rigidbody2D rb;
    PlayerStateManager psm;
    PlayerInputHandler input;

    public bool IsGrounded { get; private set; }
    public bool IsTouchingLeft { get; private set; }
    public bool IsTouchingRight { get; private set; }
    public bool FacingRight { get; private set; } = true;
    public bool IsWallSliding { get; private set; }

    float coyoteTimer;
    float jumpBufferTimer;
    int wallDirection;

    bool isDropping = false;
    int platformLayerIndex;
    int playerLayerIndex;
    ContactFilter2D platformFilter;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        psm = GetComponent<PlayerStateManager>();
        input = GetComponent<PlayerInputHandler>();

        platformLayerIndex = LayerMask.NameToLayer(platformLayer);
        playerLayerIndex = gameObject.layer;

        platformFilter = new ContactFilter2D();
        platformFilter.SetLayerMask(LayerMask.GetMask(platformLayer));
        platformFilter.useTriggers = true;
    }

    void Update()
    {
        UpdateChecks();
        UpdateTimers();
        UpdateWallSlide();
        HandleJumpBuffer();
        HandleDropThrough();
        UpdateState();
    }

    void FixedUpdate()
    {
        HandleMovement();
        ApplyBetterGravity();
    }

    void UpdateChecks()
    {
        bool wasGrounded = IsGrounded;

        ContactFilter2D groundFilter = new ContactFilter2D();
        groundFilter.SetLayerMask(groundLayer);
        groundFilter.useTriggers = true;

        ContactFilter2D wallFilter = new ContactFilter2D();
        wallFilter.SetLayerMask(wallLayer);
        wallFilter.useTriggers = true;

        IsGrounded = groundCheck.IsTouching(groundFilter);
        IsTouchingLeft = leftWallCheck.IsTouching(wallFilter);
        IsTouchingRight = rightWallCheck.IsTouching(wallFilter);

        if (IsTouchingLeft) wallDirection = -1;
        else if (IsTouchingRight) wallDirection = 1;
        else wallDirection = 0;

        if (IsGrounded)
        {
            coyoteTimer = coyoteTime;
            currentJumpCount = 0;
        }

        if (!wasGrounded && IsGrounded)
            psm.ChangeState(PlayerState.Landing);
    }

    void HandleDropThrough()
    {
        if (input.DropPressed && !isDropping && IsOnPlatform())
            StartCoroutine(DropThrough());
    }

    bool IsOnPlatform()
    {
        return groundCheck.IsTouching(platformFilter);
    }

    IEnumerator DropThrough()
    {
        isDropping = true;

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask(platformLayer));
        filter.useTriggers = true;

        Collider2D[] results = new Collider2D[5];
        int count = groundCheck.OverlapCollider(filter, results);

        for (int i = 0; i < count; i++)
            results[i].enabled = false;

        yield return new WaitForSeconds(dropCooldown);

        for (int i = 0; i < count; i++)
            results[i].enabled = true;

        isDropping = false;
    }

    void UpdateTimers()
    {
        coyoteTimer -= Time.deltaTime;
        jumpBufferTimer -= Time.deltaTime;
        doubleJumpBufferTimer -= Time.deltaTime;
    }

    void UpdateWallSlide()
    {
        bool touchingWall = IsTouchingLeft || IsTouchingRight;
        bool holdingTowardWall = (IsTouchingLeft && input.MoveInput < -0.01f) ||
                                 (IsTouchingRight && input.MoveInput > 0.01f);

        IsWallSliding = touchingWall &&
                        !IsGrounded &&
                        holdingTowardWall &&
                        rb.velocity.y < 0f;

        if (IsWallSliding && psm.CurrentState != PlayerState.WallSliding)
            psm.ChangeState(PlayerState.WallSliding);

        if (!IsWallSliding && psm.CurrentState == PlayerState.WallSliding)
            psm.ChangeState(PlayerState.Falling);
    }

    void HandleJumpBuffer()
    {
        if (input.JumpPressed)
        {
            jumpBufferTimer = jumpBufferTime;
            doubleJumpBufferTimer = jumpBufferTime;
        }

        if (jumpBufferTimer > 0f && IsWallSliding)
        {
            ExecuteWallJump();
            jumpBufferTimer = 0f;
            doubleJumpBufferTimer = 0f;
            return;
        }

        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            ExecuteJump();
            jumpBufferTimer = 0f;
            doubleJumpBufferTimer = 0f;
            return;
        }

        if (doubleJumpBufferTimer > 0f &&
            psm.IsAirborne &&
            !IsWallSliding &&
            currentJumpCount < maxJumpCount)
        {
            ExecuteDoubleJump();
        }
    }

    void ExecuteJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        coyoteTimer = 0f;
        currentJumpCount = 1;
        psm.ChangeState(PlayerState.Jumping);
    }

    void ExecuteDoubleJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce);
        doubleJumpBufferTimer = 0f;
        currentJumpCount++;
        psm.ChangeState(PlayerState.DoubleJumping);
    }

    void ExecuteWallJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, wallJumpForceY);
        currentJumpCount = 1;
        psm.ChangeState(PlayerState.WallJumping);
    }

    void HandleMovement()
    {
        float moveInput = input.MoveInput;
        float target = moveInput * moveSpeed;
        float rate = Mathf.Abs(moveInput) > 0.01f ? acceleration : deceleration;
        float newX = Mathf.MoveTowards(rb.velocity.x, target, rate * Time.fixedDeltaTime);
        rb.velocity = new Vector2(newX, rb.velocity.y);
        HandleFlip();
    }

    void HandleFlip()
    {
        if (input.MoveInput > 0f && !FacingRight) Flip();
        else if (input.MoveInput < 0f && FacingRight) Flip();
    }

    void Flip()
    {
        FacingRight = !FacingRight;
        Vector3 s = spriteChild.localScale;
        spriteChild.localScale = new Vector3(-s.x, s.y, s.z);
    }

    void ApplyBetterGravity()
    {
        if (IsWallSliding)
        {
            rb.velocity = new Vector2(
                rb.velocity.x,
                Mathf.Max(rb.velocity.y, -wallSlideSpeed)
            );
            return;
        }

        if (rb.velocity.y < 0f)
            rb.velocity += Vector2.up * Physics2D.gravity.y
                * (fallMultiplier - 1f) * Time.fixedDeltaTime;

        else if (rb.velocity.y > 0f && !input.JumpHeld)
            rb.velocity += Vector2.up * Physics2D.gravity.y
                * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
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
                if (IsWallSliding)
                    psm.ChangeState(PlayerState.WallSliding);
                else if (rb.velocity.y < -0.1f)
                    psm.ChangeState(PlayerState.Falling);
                break;

            case PlayerState.DoubleJumping:
                if (IsWallSliding)
                    psm.ChangeState(PlayerState.WallSliding);
                else if (rb.velocity.y < -0.1f)
                    psm.ChangeState(PlayerState.Falling);
                break;

            case PlayerState.Falling:
                if (IsGrounded)
                    psm.ChangeState(PlayerState.Landing);
                else if (IsWallSliding)
                    psm.ChangeState(PlayerState.WallSliding);
                break;

            case PlayerState.WallSliding:
                if (IsGrounded)
                    psm.ChangeState(PlayerState.Landing);
                else if (!IsWallSliding)
                    psm.ChangeState(PlayerState.Falling);
                break;

            case PlayerState.WallJumping:
                if (IsWallSliding)
                    psm.ChangeState(PlayerState.WallSliding);
                else if (IsGrounded)
                    psm.ChangeState(PlayerState.Landing);
                else if (rb.velocity.y < -0.1f)
                    psm.ChangeState(PlayerState.Falling);
                break;
        }
    }
}