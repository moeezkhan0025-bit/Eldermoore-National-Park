using UnityEngine;

// Feeds your Animator's parameters every frame so your existing transition graph
// (Player_Idle / Player_Run / Player_Jump / Player_Fall) drives itself.
// Put this on the SAME object as the Animator (your 'Sprite' child).
// It reads the Rigidbody2D and MovementController from the parent automatically.
//
// Your Animator already has the matching parameters: Speed, IsGrounded, IsJumping, IsFalling.
[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    [Header("Parameter names (must match the Animator's Parameters tab exactly)")]
    [SerializeField] string speedParam = "Speed";
    [SerializeField] string groundedParam = "IsGrounded";
    [SerializeField] string jumpingParam = "IsJumping";
    [SerializeField] string fallingParam = "IsFalling";

    [Tooltip("Vertical speed below/above which we count as falling/rising.")]
    [SerializeField] float verticalDeadzone = 0.1f;

    Animator animator;
    Rigidbody2D rb;
    MovementController move;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponentInParent<Rigidbody2D>();
        move = GetComponentInParent<MovementController>();

        if (rb == null) Debug.LogError($"[{name}] PlayerAnimator couldn't find a Rigidbody2D on a parent.", this);
        if (move == null) Debug.LogError($"[{name}] PlayerAnimator couldn't find a MovementController on a parent.", this);
    }

    void Update()
    {
        if (rb == null || move == null) return;

        bool grounded = move.IsGrounded;
        float vy = rb.velocity.y;

        animator.SetFloat(speedParam, Mathf.Abs(rb.velocity.x));
        animator.SetBool(groundedParam, grounded);
        animator.SetBool(jumpingParam, !grounded && vy > verticalDeadzone);
        animator.SetBool(fallingParam, !grounded && vy < -verticalDeadzone);
    }
}