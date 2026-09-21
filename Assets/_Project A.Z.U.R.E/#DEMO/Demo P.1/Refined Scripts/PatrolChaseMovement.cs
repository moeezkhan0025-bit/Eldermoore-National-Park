using UnityEngine;

// GROUND: patrols left/right between edges, chases the player horizontally when
// detected (with acceleration up to a cap), returns to patrol when lost.
[RequireComponent(typeof(EnemyController))]
public class PatrolChaseMovement : MonoBehaviour, IEnemyMovement
{
    [Header("Patrol")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float patrolRange = 3f;      // half-width of patrol from spawn

    [Header("Chase")]
    [SerializeField] private float chaseMaxSpeed = 5f;
    [SerializeField] private float acceleration = 12f;

    private EnemyController ctrl;
    private Rigidbody2D rb;
    private Vector2 spawn;
    private int patrolDir = 1;
    private float currentSpeed;

    void Awake()
    {
        ctrl = GetComponent<EnemyController>();
        rb = GetComponent<Rigidbody2D>();
        spawn = rb.position;
    }

    public void Tick(EnemyState state, Transform player)
    {
        float targetVX;

        if (state == EnemyState.Chasing && player != null)
        {
            float dir = Mathf.Sign(player.position.x - rb.position.x);
            currentSpeed = Mathf.MoveTowards(currentSpeed, chaseMaxSpeed, acceleration * Time.fixedDeltaTime);
            targetVX = dir * currentSpeed;
        }
        else // Patrolling or Returning -> patrol around spawn
        {
            currentSpeed = 0f;
            if (rb.position.x > spawn.x + patrolRange) patrolDir = -1;
            else if (rb.position.x < spawn.x - patrolRange) patrolDir = 1;
            targetVX = patrolDir * patrolSpeed;
        }

        rb.velocity = new Vector2(targetVX, rb.velocity.y);   // keep gravity on Y
    }
}
