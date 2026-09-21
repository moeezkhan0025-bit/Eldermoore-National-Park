using UnityEngine;

// GROUND: patrols left/right forever, never chases (a passive hazard/obstacle enemy).
[RequireComponent(typeof(EnemyController))]
public class PatrolOnlyMovement : MonoBehaviour, IEnemyMovement
{
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float patrolRange = 3f;

    private Rigidbody2D rb;
    private Vector2 spawn;
    private int dir = 1;

    void Awake() { rb = GetComponent<Rigidbody2D>(); spawn = rb.position; }

    public void Tick(EnemyState state, Transform player)
    {
        // Ignores state entirely — just paces.
        if (rb.position.x > spawn.x + patrolRange) dir = -1;
        else if (rb.position.x < spawn.x - patrolRange) dir = 1;
        rb.velocity = new Vector2(dir * patrolSpeed, rb.velocity.y);
    }
}
