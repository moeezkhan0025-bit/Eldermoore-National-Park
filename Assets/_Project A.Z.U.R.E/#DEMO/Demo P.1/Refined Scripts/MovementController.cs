using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerStateManager))]
public class MovementController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 8f;
    [SerializeField] float acceleration = 60f;
    [SerializeField] float deceleration = 80f;
    [SerializeField] float airAcceleration = 30f;

    [Header("Jump — tune THESE, not gravity")]
    [SerializeField] float jumpHeight = 3.2f;
    [SerializeField] float timeToApex = 0.38f;
    [SerializeField] float fallMultiplier = 2.0f;
    [SerializeField] float lowJumpMultiplier = 2.5f;
    [SerializeField] float maxFallSpeed = 24f;
    [SerializeField] float coyoteTime = 0.12f;
    [SerializeField] float jumpBufferTime = 0.12f;

    // Computed in Awake — do NOT set in the Inspector.
    float jumpForce;
    float baseGravityScale;

    [Header("Double Jump")]
    [SerializeField] float doubleJumpForce = 13f;
    [SerializeField] int maxJumpCount = 2;
    int currentJumpCount;
    float doubleJumpBufferTimer;

    [Header("Wall Slide")]
    [SerializeField] float wallSlideSpeed = 1.5f;

    [Header("Wall Jump")]
    [SerializeField] float wallJumpForceY = 15f;
    [SerializeField] float wallJumpForceX = 12f;
    [SerializeField] float wallJumpLockTime = 0.18f;
    float wallJumpLockTimer;
    float jumpCutoffTimer;

    [Header("Climb")]
    [SerializeField] float climbSpeed = 5f;
    [SerializeField] float climbHorizontalFactor = 0.5f; // sideways freedom on a climb surface (0 = pure vertical)
    [SerializeField] LayerMask climbableLayer;
    bool isClimbing;
    public bool IsTouchingClimbable { get; private set; }
    public bool IsClimbing => isClimbing;

    [Header("Warp (charged blink)")]
    [SerializeField] int maxWarpCharges = 2;
    [SerializeField] float warpDistance = 4f;
    [SerializeField] float warpCooldown = 0.25f;     // min time between warps
    [SerializeField] float warpRechargeTime = 2f;    // seconds to regen one charge
    [SerializeField] bool refillOnLanding = true;    // grounded refills all charges
    [SerializeField] float warpLockTime = 0.06f;     // brief input lock after a blink
    [SerializeField] float warpSkin = 0.1f;          // gap kept from walls when clamped
    [SerializeField] LayerMask warpObstacleLayer;    // set to Ground + Wall layers
    int currentWarpCharges;
    float warpCooldownTimer;
    float warpRechargeTimer;
    float warpLockTimer;
    public int WarpCharges => currentWarpCharges;    // handy for a future UI

    [Header("Drop Through")]
    [SerializeField] string platformLayer = "OneWayPlatform";
    [SerializeField] float dropCooldown = 0.4f;

    [Header("Check Colliders (assign in Inspector)")]
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
    ContactFilter2D platformFilter;
    bool setupValid = true;   // becomes false if a required reference is missing

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        psm = GetComponent<PlayerStateManager>();
        input = GetComponent<PlayerInputHandler>();

        // --- Loud, specific validation so nothing fails silently ---
        if (groundCheck == null) { Debug.LogError($"[{name}] 'Ground Check' collider is not assigned.", this); setupValid = false; }
        if (leftWallCheck == null) { Debug.LogError($"[{name}] 'Left Wall Check' collider is not assigned.", this); setupValid = false; }
        if (rightWallCheck == null) { Debug.LogError($"[{name}] 'Right Wall Check' collider is not assigned.", this); setupValid = false; }
        if (spriteChild == null) { Debug.LogError($"[{name}] 'Sprite Child' transform is not assigned.", this); setupValid = false; }
        if (groundLayer == 0) { Debug.LogWarning($"[{name}] 'Ground Layer' mask is empty — you'll never be grounded.", this); }
        if (LayerMask.NameToLayer(platformLayer) == -1)
            Debug.LogWarning($"[{name}] Layer '{platformLayer}' doesn't exist — drop-through is disabled. Create it under Tags & Layers.", this);

        // Recommended rigidbody settings (also worth setting in the Inspector).
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Derive the jump arc from the feel values.
        float gravityStrength = (2f * jumpHeight) / (timeToApex * timeToApex);
        baseGravityScale = gravityStrength / Mathf.Abs(Physics2D.gravity.y);
        jumpForce = gravityStrength * timeToApex;
        rb.gravityScale = baseGravityScale;

        platformFilter = new ContactFilter2D();
        platformFilter.SetLayerMask(LayerMask.GetMask(platformLayer));
        platformFilter.useTriggers = true;

        currentWarpCharges = maxWarpCharges;
        warpRechargeTimer = warpRechargeTime;
    }

    void Update()
    {
        if (!setupValid) return;   // don't spam NREs if something's unassigned

        UpdateChecks();
        UpdateTimers();

        // Climb takes priority over normal movement.
        HandleClimb();
        if (isClimbing) { UpdateState(); return; }

        // Warp is an instant action; may reposition the player this frame.
        HandleWarp();

        UpdateWallSlide();
        HandleJumpBuffer();
        HandleDropThrough();
        UpdateState();
    }

    void FixedUpdate()
    {
        if (!setupValid) return;

        if (isClimbing) { HandleClimbMovement(); return; }

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
        IsTouchingClimbable = rb.IsTouchingLayers(climbableLayer);
        if (IsTouchingLeft) wallDirection = -1;
        else if (IsTouchingRight) wallDirection = 1;
        else wallDirection = 0;
        if (IsGrounded && jumpCutoffTimer <= 0f)
        {
            coyoteTimer = coyoteTime;
            currentJumpCount = 0;
            if (refillOnLanding) currentWarpCharges = maxWarpCharges;
        }
        if (!wasGrounded && IsGrounded)
            psm.ChangeState(PlayerState.Landing);
    }

    void HandleDropThrough()
    {
        if (input.DropPressed && !isDropping && IsOnPlatform())
            StartCoroutine(DropThrough());
    }

    bool IsOnPlatform() => groundCheck.IsTouching(platformFilter);

    IEnumerator DropThrough()
    {
        isDropping = true;

        Collider2D[] results = new Collider2D[5];
        int count = groundCheck.OverlapCollider(platformFilter, results);

        for (int i = 0; i < count; i++) results[i].enabled = false;
        yield return new WaitForSeconds(dropCooldown);
        for (int i = 0; i < count; i++) if (results[i]) results[i].enabled = true;

        isDropping = false;
    }

    void UpdateTimers()
    {
        coyoteTimer -= Time.deltaTime;
        jumpBufferTimer -= Time.deltaTime;
        doubleJumpBufferTimer -= Time.deltaTime;
        wallJumpLockTimer -= Time.deltaTime;
        jumpCutoffTimer -= Time.deltaTime;
        warpCooldownTimer -= Time.deltaTime;
        warpLockTimer -= Time.deltaTime;

        // Regenerate one warp charge over time.
        if (currentWarpCharges < maxWarpCharges)
        {
            warpRechargeTimer -= Time.deltaTime;
            if (warpRechargeTimer <= 0f)
            {
                currentWarpCharges++;
                warpRechargeTimer = warpRechargeTime;
            }
        }
        else warpRechargeTimer = warpRechargeTime;
    }

    // ---------------- CLIMB ----------------

    void HandleClimb()
    {
        // Grab on: touching a climbable surface, off the ground, and pressing up/down or grab.
        if (!isClimbing && IsTouchingClimbable && !IsGrounded &&
            (Mathf.Abs(input.VerticalInput) > 0.01f || input.GrabHeld))
        {
            isClimbing = true;
            currentJumpCount = 0;                 // refresh jumps when you grab on
            psm.ChangeState(PlayerState.Climbing);
        }

        if (!isClimbing) return;

        // Let go: off the surface or back on the ground.
        if (!IsTouchingClimbable || IsGrounded) { isClimbing = false; return; }

        // Jump off the surface.
        if (input.JumpPressed) { isClimbing = false; ExecuteJump(); }
    }

    void HandleClimbMovement()
    {
        rb.gravityScale = 0f;                      // no gravity while gripping
        float v = input.VerticalInput;
        float h = input.MoveInput;
        rb.velocity = new Vector2(h * climbSpeed * climbHorizontalFactor, v * climbSpeed);
        HandleFlip();
    }

    // ---------------- WARP ----------------

    void HandleWarp()
    {
        if (!input.WarpPressed) return;
        if (currentWarpCharges <= 0 || warpCooldownTimer > 0f) return;
        ExecuteWarp();
    }

    void ExecuteWarp()
    {
        // Direction from movement input; fall back to facing when neutral.
        Vector2 dir = new Vector2(input.MoveInput, input.VerticalInput);
        if (dir.sqrMagnitude < 0.01f) dir = new Vector2(FacingRight ? 1f : -1f, 0f);
        dir.Normalize();

        // Clamp against obstacles so we don't blink inside geometry.
        float dist = warpDistance;
        RaycastHit2D hit = Physics2D.Raycast(rb.position, dir, warpDistance, warpObstacleLayer);
        if (hit.collider != null) dist = Mathf.Max(0f, hit.distance - warpSkin);

        rb.position = rb.position + dir * dist;
        rb.velocity = Vector2.zero;

        currentWarpCharges--;
        warpCooldownTimer = warpCooldown;
        warpLockTimer = warpLockTime;
        psm.ChangeState(PlayerState.Warping);
    }

    // ---------------- CORE MOVEMENT ----------------

    void UpdateWallSlide()
    {
        bool touchingWall = IsTouchingLeft || IsTouchingRight;
        bool holdingTowardWall = (IsTouchingLeft && input.MoveInput < -0.01f) ||
                                 (IsTouchingRight && input.MoveInput > 0.01f);

        IsWallSliding = touchingWall && !IsGrounded && holdingTowardWall && rb.velocity.y < 0f;

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

        if (doubleJumpBufferTimer > 0f && psm.IsAirborne && !IsWallSliding && currentJumpCount < maxJumpCount)
            ExecuteDoubleJump();
    }

    void ExecuteJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        coyoteTimer = 0f;
        currentJumpCount = 1;
        psm.ChangeState(PlayerState.Jumping);
        jumpCutoffTimer = .1f;
    }

    void ExecuteDoubleJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce);
        doubleJumpBufferTimer = 0f;
        currentJumpCount++;
        psm.ChangeState(PlayerState.DoubleJumping);
        jumpCutoffTimer = .1f;
    }

    void ExecuteWallJump()
    {
        rb.velocity = new Vector2(-wallDirection * wallJumpForceX, wallJumpForceY);
        wallJumpLockTimer = wallJumpLockTime;
        currentJumpCount = 1;
        psm.ChangeState(PlayerState.WallJumping);
        jumpCutoffTimer = .1f;
    }

    void HandleMovement()
    {
        if (wallJumpLockTimer > 0f || warpLockTimer > 0f) return;   // let the kick / blink carry

        float moveInput = input.MoveInput;
        float target = moveInput * moveSpeed;
        bool moving = Mathf.Abs(moveInput) > 0.01f;

        float rate = IsGrounded ? (moving ? acceleration : deceleration) : airAcceleration;
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
            rb.gravityScale = baseGravityScale;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -wallSlideSpeed));
            return;
        }

        if (rb.velocity.y < 0f)
            rb.gravityScale = baseGravityScale * fallMultiplier;
        else if (rb.velocity.y > 0f && !input.JumpHeld)
            rb.gravityScale = baseGravityScale * lowJumpMultiplier;
        else
            rb.gravityScale = baseGravityScale;

        if (rb.velocity.y < -maxFallSpeed)
            rb.velocity = new Vector2(rb.velocity.x, -maxFallSpeed);
    }

    void UpdateState()
    {
        switch (psm.CurrentState)
        {
            case PlayerState.Landing:
                if (IsGrounded)
                    psm.ChangeState(Mathf.Abs(input.MoveInput) > 0.01f ? PlayerState.Running : PlayerState.Idle);
                break;

            case PlayerState.Idle:
            case PlayerState.Running:
                if (!IsGrounded && coyoteTimer <= 0f)
                    psm.ChangeState(PlayerState.Falling);
                else
                    psm.ChangeState(Mathf.Abs(input.MoveInput) > 0.01f ? PlayerState.Running : PlayerState.Idle);
                break;

            case PlayerState.Jumping:
            case PlayerState.DoubleJumping:
                if (IsWallSliding) psm.ChangeState(PlayerState.WallSliding);
                else if (rb.velocity.y < -0.1f) psm.ChangeState(PlayerState.Falling);
                break;

            case PlayerState.Falling:
                if (IsGrounded) psm.ChangeState(PlayerState.Landing);
                else if (IsWallSliding) psm.ChangeState(PlayerState.WallSliding);
                break;

            case PlayerState.WallSliding:
                if (IsGrounded) psm.ChangeState(PlayerState.Landing);
                else if (!IsWallSliding) psm.ChangeState(PlayerState.Falling);
                break;

            case PlayerState.WallJumping:
                if (IsWallSliding) psm.ChangeState(PlayerState.WallSliding);
                else if (IsGrounded) psm.ChangeState(PlayerState.Landing);
                else if (rb.velocity.y < -0.1f) psm.ChangeState(PlayerState.Falling);
                break;

            case PlayerState.Climbing:
                if (!isClimbing)
                    psm.ChangeState(IsGrounded ? PlayerState.Idle : PlayerState.Falling);
                break;

            case PlayerState.Warping:
                if (warpLockTimer > 0f) break;               // hold the warp state briefly
                if (IsGrounded)
                    psm.ChangeState(Mathf.Abs(input.MoveInput) > 0.01f ? PlayerState.Running : PlayerState.Idle);
                else if (IsWallSliding) psm.ChangeState(PlayerState.WallSliding);
                else psm.ChangeState(PlayerState.Falling);
                break;
        }
    }
}