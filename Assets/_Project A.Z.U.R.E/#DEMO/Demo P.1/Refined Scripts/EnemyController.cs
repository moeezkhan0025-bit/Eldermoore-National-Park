using UnityEngine;

// The enemy "brain": owns health, detects the player (enter/leave radius), tracks
// state, and drives whatever IEnemyMovement component is attached. Attack and
// interference behaviors are SEPARATE components that read this controller's state.
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float detectRadius = 6f;
    [SerializeField] private float loseRadius = 9f;      // must get this far to lose the player
    [SerializeField] private string playerTag = "Player";

    public EnemyState State { get; private set; } = EnemyState.Patrolling;
    public Transform Player { get; private set; }
    public Rigidbody2D Body { get; private set; }

    private IEnemyMovement movement;

    void Awake()
    {
        Body = GetComponent<Rigidbody2D>();
        movement = GetComponent<IEnemyMovement>();   // whichever movement is attached
        if (movement == null)
            Debug.LogWarning($"[{name}] EnemyController has no IEnemyMovement component.", this);
    }

    void Update()
    {
        AcquirePlayer();
        UpdateState();
    }

    void FixedUpdate()
    {
        movement?.Tick(State, Player);
    }

    void AcquirePlayer()
    {
        // Keep a reference; re-find if lost/destroyed.
        if (Player == null)
        {
            var go = GameObject.FindGameObjectWithTag(playerTag);
            if (go != null) Player = go.transform;
        }
    }

    void UpdateState()
    {
        if (Player == null) { State = EnemyState.Patrolling; return; }

        float dist = Vector2.Distance(transform.position, Player.position);

        switch (State)
        {
            case EnemyState.Patrolling:
            case EnemyState.Returning:
                if (dist <= detectRadius) State = EnemyState.Chasing;
                break;

            case EnemyState.Chasing:
                if (dist > loseRadius) State = EnemyState.Returning;
                break;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, loseRadius);
    }
}
