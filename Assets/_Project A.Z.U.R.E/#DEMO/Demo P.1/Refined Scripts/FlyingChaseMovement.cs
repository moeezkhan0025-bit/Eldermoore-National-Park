using UnityEngine;

// FLYING: ignores gravity, hovers/patrols, and chases the player freely in 2D
// (both axes) when detected. Set the Rigidbody2D Gravity Scale to 0 on this enemy.
[RequireComponent(typeof(EnemyController))]
public class FlyingChaseMovement : MonoBehaviour, IEnemyMovement
{
    [Header("Patrol (hover)")]
    [SerializeField] private float hoverSpeed = 1.5f;
    [SerializeField] private float hoverRange = 2f;

    [Header("Chase")]
    [SerializeField] private float chaseMaxSpeed = 4f;
    [SerializeField] private float acceleration = 8f;

    private Rigidbody2D rb;
    private Vector2 spawn;
    private int dir = 1;
    private float currentSpeed;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;   // flying
        spawn = rb.position;
    }

    public void Tick(EnemyState state, Transform player)
    {
        if (state == EnemyState.Chasing && player != null)
        {
            Vector2 toPlayer = ((Vector2)player.position - rb.position).normalized;
            currentSpeed = Mathf.MoveTowards(currentSpeed, chaseMaxSpeed, acceleration * Time.fixedDeltaTime);
            rb.velocity = toPlayer * currentSpeed;   // free 2D pursuit
        }
        else // hover around spawn horizontally
        {
            currentSpeed = 0f;
            if (rb.position.x > spawn.x + hoverRange) dir = -1;
            else if (rb.position.x < spawn.x - hoverRange) dir = 1;
            rb.velocity = new Vector2(dir * hoverSpeed, 0f);
        }
    }
}
