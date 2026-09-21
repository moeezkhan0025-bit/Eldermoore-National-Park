using UnityEngine;

// GROUND: doesn't patrol — holds position until the player is detected, then
// chases horizontally. Returns to a standstill (or drifts home) when lost.
[RequireComponent(typeof(EnemyController))]
public class StationaryChaseMovement : MonoBehaviour, IEnemyMovement
{
    [SerializeField] private float chaseMaxSpeed = 5f;
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private bool returnHome = true;

    private Rigidbody2D rb;
    private Vector2 spawn;
    private float currentSpeed;

    void Awake() { rb = GetComponent<Rigidbody2D>(); spawn = rb.position; }

    public void Tick(EnemyState state, Transform player)
    {
        float targetVX = 0f;

        if (state == EnemyState.Chasing && player != null)
        {
            float dir = Mathf.Sign(player.position.x - rb.position.x);
            currentSpeed = Mathf.MoveTowards(currentSpeed, chaseMaxSpeed, acceleration * Time.fixedDeltaTime);
            targetVX = dir * currentSpeed;
        }
        else if (state == EnemyState.Returning && returnHome)
        {
            float dir = Mathf.Sign(spawn.x - rb.position.x);
            if (Mathf.Abs(spawn.x - rb.position.x) > 0.1f) targetVX = dir * chaseMaxSpeed * 0.5f;
            currentSpeed = 0f;
        }
        else currentSpeed = 0f;

        rb.velocity = new Vector2(targetVX, rb.velocity.y);
    }
}
