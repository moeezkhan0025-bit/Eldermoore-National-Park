using UnityEngine;

// GROUND patrol+chase with wall + ledge detection so it turns around at obstacles
// and edges instead of pushing into them (which caused the zigzag-down-and-fall).
[RequireComponent(typeof(EnemyController))]
public class PatrolChaseMovement : MonoBehaviour, IEnemyMovement
{
    [Header("Patrol")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float patrolRange = 3f;      // 0 = ignore range, roam until wall/ledge

    [Header("Chase")]
    [SerializeField] private float chaseMaxSpeed = 5f;
    [SerializeField] private float acceleration = 12f;

    [Header("Sensing")]
    [SerializeField] private LayerMask groundLayer;       // ground/walls the enemy walks on
    [SerializeField] private float wallCheckDist = 0.4f;  // how far ahead to look for a wall
    [SerializeField] private float ledgeCheckDown = 0.6f; // how far down to look for floor ahead
    [SerializeField] private Vector2 feetOffset = new Vector2(0f, -0.4f);  // near the feet
    [SerializeField] private float wallCheckHeight = 0.2f;

    private EnemyController ctrl;
    private Rigidbody2D rb;
    private Vector2 spawn;
    private int dir = 1;             // +1 right, -1 left
    private float currentSpeed;

    void Awake()
    {
        ctrl = GetComponent<EnemyController>();
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;    // never tumble
        spawn = rb.position;
        if (groundLayer.value == 0)
            Debug.LogError($"[{name}] PatrolChaseMovement: Ground Layer is NOT set! Wall/ledge rays will hit nothing and the enemy can't sense floor.", this);
    }

    public void Tick(EnemyState state, Transform player)
    {
        if (state == EnemyState.Chasing && player != null)
        {
            // Chase: face the player, but STILL stop at walls/ledges (don't walk off).
            int wantDir = player.position.x > rb.position.x ? 1 : -1;
            if (CanMove(wantDir)) dir = wantDir;
            // if blocked toward the player, just stop horizontally
            float vx = CanMove(dir) && dir == wantDir ? wantDir * Accelerate() : 0f;
            rb.velocity = new Vector2(vx, rb.velocity.y);
        }
        else
        {
            // Patrol: reverse at walls, ledges, or patrol-range edges.
            currentSpeed = 0f;

            bool blocked = !CanMove(dir);
            bool pastRange = patrolRange > 0f &&
                ((dir > 0 && rb.position.x > spawn.x + patrolRange) ||
                 (dir < 0 && rb.position.x < spawn.x - patrolRange));

            if (blocked || pastRange) dir = -dir;

            rb.velocity = new Vector2(dir * patrolSpeed, rb.velocity.y);
        }
    }

    float Accelerate()
    {
        currentSpeed = Mathf.MoveTowards(currentSpeed, chaseMaxSpeed, acceleration * Time.fixedDeltaTime);
        return currentSpeed;
    }

    // True if the enemy can safely move in dir: no wall ahead AND floor ahead.
    bool CanMove(int d)
    {
        Vector2 feet = rb.position + feetOffset;
        Vector2 ahead = feet + Vector2.right * d * wallCheckDist;

        // Wall ahead? (horizontal ray at feet height)
        Vector2 wallOrigin = rb.position + new Vector2(0f, feetOffset.y + wallCheckHeight);
        bool wall = Physics2D.Raycast(wallOrigin, Vector2.right * d, wallCheckDist, groundLayer);

        // Floor ahead? (downward ray from just ahead)
        bool floor = Physics2D.Raycast(ahead, Vector2.down, ledgeCheckDown, groundLayer);

        return !wall && floor;
    }

    void OnDrawGizmosSelected()
    {
        var body = Application.isPlaying ? (Vector2)rb.position : (Vector2)transform.position;
        Vector2 feet = body + feetOffset;
        Vector2 ahead = feet + Vector2.right * dir * wallCheckDist;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(body + new Vector2(0, feetOffset.y + wallCheckHeight),
                        body + new Vector2(dir * wallCheckDist, feetOffset.y + wallCheckHeight));
        Gizmos.color = Color.green;
        Gizmos.DrawLine(ahead, ahead + Vector2.down * ledgeCheckDown);
    }
}