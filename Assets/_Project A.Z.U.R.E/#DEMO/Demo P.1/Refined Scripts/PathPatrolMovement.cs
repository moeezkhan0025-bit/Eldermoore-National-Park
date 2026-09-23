using UnityEngine;

// GROUND/FLYING movement that follows a PatrolPath's waypoints, and chases the
// player when detected (returning to the nearest path point when it loses them).
// Speed is tunable here and reflected live. Works with the PatrolPath gizmo.
[RequireComponent(typeof(EnemyController))]
public class PathPatrolMovement : MonoBehaviour, IEnemyMovement
{
    [Header("Path")]
    [SerializeField] private PatrolPath path;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float reachDistance = 0.15f;

    [Header("Chase")]
    [SerializeField] private float chaseMaxSpeed = 5f;
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private bool chaseVertical = false;   // true for flyers (pursue on Y too)

    [Header("Facing")]
    [SerializeField] private SpriteRenderer spriteRenderer;   // flips to face movement
    [SerializeField] private bool faceRightIsDefault = true;  // does the sprite face right by default?

    private Rigidbody2D rb;
    private int targetIndex;
    private int dir = 1;      // for ping-pong
    private float currentSpeed;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void Tick(EnemyState state, Transform player)
    {
        if (state == EnemyState.Chasing && player != null)
        {
            Vector2 to = (Vector2)player.position - rb.position;
            currentSpeed = Mathf.MoveTowards(currentSpeed, chaseMaxSpeed, acceleration * Time.fixedDeltaTime);

            Vector2 vel;
            if (chaseVertical) vel = to.normalized * currentSpeed;
            else vel = new Vector2(Mathf.Sign(to.x) * currentSpeed, rb.velocity.y);
            rb.velocity = vel;
            UpdateFacing();
            return;
        }

        // Patrol along the path.
        currentSpeed = 0f;
        if (path == null || path.Count < 2) { rb.velocity = new Vector2(0, rb.velocity.y); return; }

        Vector2 target = path.Get(targetIndex);
        Vector2 toTarget = target - rb.position;

        // Ground enemies (chaseVertical off) only need to reach the waypoint's X —
        // their Y stays at ground level, never matching a floor-level waypoint's Y,
        // so measuring full 2D distance would never register as "reached".
        float reached = chaseVertical ? toTarget.magnitude : Mathf.Abs(toTarget.x);
        if (reached <= reachDistance) { Advance(); return; }

        Vector2 move = toTarget.normalized * patrolSpeed;
        // ground enemies keep their own Y (gravity); flyers move freely
        if (chaseVertical) rb.velocity = move;
        else rb.velocity = new Vector2(move.x, rb.velocity.y);
        UpdateFacing();
    }

    void UpdateFacing()
    {
        if (spriteRenderer == null) return;
        float vx = rb.velocity.x;
        if (Mathf.Abs(vx) < 0.05f) return;   // ignore tiny/no movement
        bool movingRight = vx > 0f;
        // flipX true means the (default-right) sprite faces left.
        spriteRenderer.flipX = faceRightIsDefault ? !movingRight : movingRight;
    }

    void Advance()
    {
        if (path.loop)
        {
            targetIndex = (targetIndex + 1) % path.Count;
        }
        else // ping-pong
        {
            if (targetIndex + dir >= path.Count || targetIndex + dir < 0) dir = -dir;
            targetIndex += dir;
        }
    }
}