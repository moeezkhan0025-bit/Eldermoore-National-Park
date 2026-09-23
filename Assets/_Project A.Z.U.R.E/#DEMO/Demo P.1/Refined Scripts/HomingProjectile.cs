using UnityEngine;

// Magic-missile projectile. Two phases:
//   HOVER  — sits in front of the caster (bobbing) while the sequence is input.
//   FLYING — on Fire(), homes to the target, hits it, spawns impact VFX, despawns.
// Call Dismiss() to remove it (cancel / no target).
public class HomingProjectile : MonoBehaviour
{
    [Header("Flight")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float turnRate = 8f;        // higher = homes tighter
    [SerializeField] private float hitDistance = 0.4f;
    [SerializeField] private float maxLifetime = 4f;
    [SerializeField] private GameObject impactVfx;
    [Header("Facing")]
    [Tooltip("Degrees to offset rotation so the sprite's TIP points along travel. 0 if the icicle art points RIGHT; -90 if it points UP; 90 if DOWN; 180 if LEFT.")]
    [SerializeField] private float spriteForwardOffset = 0f;

    [Header("Hover")]
    [SerializeField] private float bobHeight = 0.12f;
    [SerializeField] private float bobSpeed = 5f;

    private bool flying;
    private SpriteRenderer sr;
    private MovementController casterMove;
    private Transform anchor;        // caster to hover in front of
    private Vector3 hoverOffset;
    private Transform target;
    private float damage;
    private Vector2 vel;
    private float life;
    private float bobT;

    void Awake() { sr = GetComponentInChildren<SpriteRenderer>(); }

    // Hover following an explicit spawn-point transform (position handled by the
    // point itself, which is a child of the player so it moves + flips with them).
    private Transform followPoint;
    public void HoverAt(Transform spawnPoint)
    {
        flying = false;
        followPoint = spawnPoint;
        anchor = null;
        casterMove = spawnPoint != null ? spawnPoint.GetComponentInParent<MovementController>() : null;
        if (spawnPoint != null) transform.position = spawnPoint.position;
    }

    // Start hovering in front of the caster (offset-based fallback).
    public void Hover(Transform caster, Vector3 offset)
    {
        flying = false;
        anchor = caster;
        hoverOffset = offset;
        casterMove = caster != null ? caster.GetComponent<MovementController>() : null;
        if (caster != null) transform.position = caster.position + offset;
    }

    // Launch it at a target.
    public void Fire(Transform enemy, float dmg)
    {
        flying = true;
        if (sr != null) sr.flipX = false;   // clear hover flip; rotation now controls facing
        target = enemy;
        damage = dmg;
        Vector2 dir = enemy != null
            ? ((Vector2)enemy.position - (Vector2)transform.position).normalized
            : (Vector2)transform.right;
        vel = dir * speed;
    }

    public void Dismiss() { Destroy(gameObject); }

    void Update()
    {
        if (!flying)
        {
            if (followPoint != null)
            {
                bobT += Time.deltaTime * bobSpeed;
                transform.position = followPoint.position + new Vector3(0, Mathf.Sin(bobT) * bobHeight, 0);
                if (sr != null && casterMove != null) sr.flipX = !casterMove.FacingRight;
                return;
            }
            if (anchor != null)
            {
                bool faceRight = casterMove == null || casterMove.FacingRight;
                float side = faceRight ? 1f : -1f;

                // Mirror the offset to the facing side, and flip the sprite to match.
                Vector3 off = new Vector3(Mathf.Abs(hoverOffset.x) * side, hoverOffset.y, hoverOffset.z);
                bobT += Time.deltaTime * bobSpeed;
                transform.position = anchor.position + off + new Vector3(0, Mathf.Sin(bobT) * bobHeight, 0);

                if (sr != null) sr.flipX = !faceRight;
            }
            return;
        }

        life += Time.deltaTime;
        if (life > maxLifetime) { Destroy(gameObject); return; }

        if (target != null)
        {
            Vector2 desired = ((Vector2)target.position - (Vector2)transform.position).normalized * speed;
            vel = Vector2.Lerp(vel, desired, turnRate * Time.deltaTime);

            if (((Vector2)target.position - (Vector2)transform.position).sqrMagnitude <= hitDistance * hitDistance)
            {
                Hit();
                return;
            }
        }

        transform.position += (Vector3)(vel * Time.deltaTime);
        if (vel.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg + spriteForwardOffset);
    }

    void Hit()
    {
        if (target != null)
        {
            var hp = target.GetComponent<Health>();
            if (hp != null) hp.TakeDamage(damage);
        }
        if (impactVfx != null) Instantiate(impactVfx, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    void OnDrawGizmos()
    {
        // Show which way the sprite's TIP points given the current offset, so you
        // can line it up with the art in the editor. Green arrow = tip direction.
        float ang = spriteForwardOffset * Mathf.Deg2Rad;
        // In flight the sprite is rotated so its +X (after offset) faces travel.
        // The "tip" is along the sprite's local right rotated by -offset from travel;
        // for setup we just show local-right adjusted by the offset from this transform.
        Vector3 tipDir = new Vector3(Mathf.Cos(-ang), Mathf.Sin(-ang), 0f);
        tipDir = transform.rotation * tipDir;

        Gizmos.color = Color.green;
        Vector3 start = transform.position;
        Vector3 end = start + tipDir * 0.8f;
        Gizmos.DrawLine(start, end);
        // arrowhead
        Vector3 back = -tipDir * 0.2f;
        Vector3 perp = new Vector3(-tipDir.y, tipDir.x, 0f) * 0.12f;
        Gizmos.DrawLine(end, end + back + perp);
        Gizmos.DrawLine(end, end + back - perp);

        // A faint circle marking the projectile origin.
        Gizmos.color = new Color(0f, 1f, 1f, 0.4f);
        Gizmos.DrawWireSphere(start, 0.1f);
    }
}