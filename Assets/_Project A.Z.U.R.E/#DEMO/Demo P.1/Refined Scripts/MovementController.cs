using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
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
    [SerializeField] float climbHorizontalFactor = 0.5f;
    [SerializeField] LayerMask climbableLayer;
    bool isClimbing;
    public bool IsTouchingClimbable { get; private set; }
    public bool IsClimbing => isClimbing;

    [Header("Warp (charged blink)")]
    [SerializeField] int maxWarpCharges = 2;
    [SerializeField] float warpDistance = 4f;
    [SerializeField] float warpCooldown = 0.25f;
    [SerializeField] float warpRechargeTime = 2f;
    [SerializeField] bool refillOnLanding = true;
    [SerializeField] float warpLockTime = 0.06f;
    [SerializeField] float warpSkin = 0.1f;
    [SerializeField] LayerMask warpObstacleLayer;
    int currentWarpCharges;
    int effectiveMaxWarpCharges;   // base + WarpChargesBonus stat
    float warpCooldownTimer;
    float warpRechargeTimer;
    float warpLockTimer;
    public int WarpCharges => currentWarpCharges;

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
    IPlayerInput input;
    PlayerStats stats;   // optional — movement stats scale base values when present

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
    bool setupValid = true;

    // ---- stat multiplier helpers (default to 1x / +0 when no PlayerStats) ----
    float SpeedMult => stats != null ? stats.GetMultiplier(StatType.SpeedMult) : 1f;
    float JumpMult => stats != null ? stats.GetMultiplier(StatType.JumpMult) : 1f;
    float ClimbMult => stats != null ? stats.GetMultiplier(StatType.ClimbSpeedMult) : 1f;
    int WarpBonus => stats != null ? Mathf.RoundToInt(stats.Get(StatType.WarpChargesBonus)) : 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        psm = GetComponent<PlayerStateManager>();
        stats = GetComponent<PlayerStats>();   // optional; movement works without it

        input = GetComponent<IPlayerInput>();
        if (input == null)
        {
            Debug.LogError($"[{name}] No IPlayerInput found — add ControllerInputHandler or PlayerInputHandler.", this);
            setupValid = false;
        }

        if (groundCheck == null) { Debug.LogError($"[{name}] 'Ground Check' collider is not assigned.", this); setupValid = false; }
        if (leftWallCheck == null) { Debug.LogError($"[{name}] 'Left Wall Check' collider is not assigned.", this); setupValid = false; }
        if (rightWallCheck == null) { Debug.LogError($"[{name}] 'Right Wall Check' collider is not assigned.", this); setupValid = false; }
        if (spriteChild == null) { Debug.LogError($"[{name}] 'Sprite Child' transform is not assigned.", this); setupValid = false; }
        if (groundLayer == 0) { Debug.LogWarning($"[{name}] 'Ground Layer' mask is empty — you'll never be grounded.", this); }
        if (LayerMask.NameToLayer(platformLayer) == -1)
            Debug.LogWarning($"[{name}] Layer '{platformLayer}' doesn't exist — drop-through is disabled.", this);

        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        float gravityStrength = (2f * jumpHeight) / (timeToApex * timeToApex);
        baseGravityScale = gravityStrength / Mathf.Abs(Physics2D.gravity.y);
        jumpForce = gravityStrength * timeToApex;
        rb.gravityScale = baseGravityScale;

        platformFilter = new ContactFilter2D();
        platformFilter.SetLayerMask(LayerMask.GetMask(platformLayer));
        platformFilter.useTriggers = true;

        RefreshWarpCharges();
    }

    // Recompute max warp charges from base + stat bonus, keeping current in range.
    void RefreshWarpCharges()
    {
        int newMax = Mathf.Max(0, maxWarpCharges + WarpBonus);
        if (newMax != effectiveMaxWarpCharges)
        {
            effectiveMaxWarpCharges = newMax;
            currentWarpCharges = Mathf.Min(currentWarpCharges <= 0 ? newMax : currentWarpCharges, newMax);
        }
        if (currentWarpCharges == 0 && effectiveMaxWarpCharges > 0 && !Application.isPlaying)
            currentWarpCharges = effectiveMaxWarpCharges;
    }

    void Start()
    {
        currentWarpCharges = effectiveMaxWarpCharges;   // start full
        warpRechargeTimer = warpRechargeTime;
    }

    void Update()
    {
        if (!setupValid) return;

        RefreshWarpCharges();   // picks up gear/potion warp-charge bonuses live

        UpdateChecks();
        UpdateTimers();

        HandleClimb();
        if (isClimbing) { UpdateState(); return; }

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
            if (refillOnLanding) currentWarpCharges = effectiveMaxWarpCharges;
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

        if (currentWarpCharges < effectiveMaxWarpCharges)
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
        // LATCH: press grab while near a climbable surface (works grounded or not).
        if (!isClimbing && IsTouchingClimbable && input.GrabPressed)
        {
            isClimbing = true;
            currentJumpCount = 0;
            psm.ChangeState(PlayerState.Climbing);
        }

        if (!isClimbing) return;

        // STAY only while the grab button is HELD and still touching the surface.
        // Release the button, leave the surface, or jump off -> drop.
        if (!input.GrabHeld || !IsTouchingClimbable)
        {
            isClimbing = false;
            return;
        }

        // Jump off the climb surface.
        if (input.JumpPressed) { isClimbing = false; ExecuteJump(); }
    }

    void HandleClimbMovement()
    {
        rb.gravityScale = 0f;
        float v = input.VerticalInput;
        float h = input.MoveInput;
        float cs = climbSpeed * ClimbMult;   // <- climb speed scaled by stat
        rb.velocity = new Vector2(h * cs * climbHorizontalFactor, v * cs);
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
        Vector2 dir = new Vector2(input.MoveInput, input.VerticalInput);
        if (dir.sqrMagnitude < 0.01f) dir = new Vector2(FacingRight ? 1f : -1f, 0f);
        dir.Normalize();

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
        rb.velocity = new Vector2(rb.velocity.x, jumpForce * JumpMult);   // <- jump scaled
        coyoteTimer = 0f;
        currentJumpCount = 1;
        psm.ChangeState(PlayerState.Jumping);
        jumpCutoffTimer = .1f;
    }

    void ExecuteDoubleJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce * JumpMult);   // <- jump scaled
        doubleJumpBufferTimer = 0f;
        currentJumpCount++;
        psm.ChangeState(PlayerState.DoubleJumping);
        jumpCutoffTimer = .1f;
    }

    void ExecuteWallJump()
    {
        rb.velocity = new Vector2(-wallDirection * wallJumpForceX, wallJumpForceY * JumpMult);
        wallJumpLockTimer = wallJumpLockTime;
        currentJumpCount = 1;
        psm.ChangeState(PlayerState.WallJumping);
        jumpCutoffTimer = .1f;
    }

    void HandleMovement()
    {
        if (wallJumpLockTimer > 0f || warpLockTimer > 0f) return;

        float moveInput = input.MoveInput;
        float target = moveInput * moveSpeed * SpeedMult;   // <- run speed scaled
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
                if (warpLockTimer > 0f) break;
                if (IsGrounded)
                    psm.ChangeState(Mathf.Abs(input.MoveInput) > 0.01f ? PlayerState.Running : PlayerState.Idle);
                else if (IsWallSliding) psm.ChangeState(PlayerState.WallSliding);
                else psm.ChangeState(PlayerState.Falling);
                break;
        }
    }
}