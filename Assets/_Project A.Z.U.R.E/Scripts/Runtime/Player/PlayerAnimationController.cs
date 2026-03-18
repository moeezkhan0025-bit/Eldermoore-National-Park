using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerStateManager))]
public class PlayerAnimationController : MonoBehaviour
{
    Animator anim;
    PlayerStateManager psm;
    Rigidbody2D rb;

    static readonly int SPEED = Animator.StringToHash("Speed");
    static readonly int IS_GROUNDED = Animator.StringToHash("IsGrounded");
    static readonly int IS_JUMPING = Animator.StringToHash("IsJumping");
    static readonly int IS_FALLING = Animator.StringToHash("IsFalling");

    void Awake()
    {
        // These are on the Sprite child (same GameObject)
        anim = GetComponent<Animator>();

        // These are on the Player root (parent)
        psm = GetComponentInParent<PlayerStateManager>();
        rb = GetComponentInParent<Rigidbody2D>();
    }

    void Start()
    {
        Debug.Log($"[Anim] Start running on: {gameObject.name}");
        Debug.Log($"[Anim] PSM found: {psm != null}");
        Debug.Log($"[Anim] RB found: {rb != null}");
        // Subscribe here instead of OnEnable
        // Guarantees psm is found before subscribing
        if (psm != null)
            psm.OnStateChanged += HandleStateChanged;
        else
            Debug.LogError("[Anim] PlayerStateManager not found in parent!");
    }

    void OnDestroy()
    {
        // Unsubscribe here instead of OnDisable
        if (psm != null)
            psm.OnStateChanged -= HandleStateChanged;
    }

    void Update()
    {
        if (rb != null)
            anim.SetFloat(SPEED, Mathf.Abs(rb.velocity.x));
    }

    void HandleStateChanged(PlayerState prev, PlayerState next)
    {
        Debug.Log($"[Anim] State changed: {prev} → {next}");

        anim.SetBool(IS_GROUNDED, false);
        anim.SetBool(IS_JUMPING, false);
        anim.SetBool(IS_FALLING, false);

        switch (next)
        {
            case PlayerState.Idle:
            case PlayerState.Running:
            case PlayerState.Landing:
                anim.SetBool(IS_GROUNDED, true);
                break;
            case PlayerState.Jumping:
            case PlayerState.DoubleJumping:
                anim.SetBool(IS_JUMPING, true);
                break;
            case PlayerState.Falling:
                anim.SetBool(IS_FALLING, true);
                break;
        }
    }
}