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

    [Header("Hover")]
    [SerializeField] private float bobHeight = 0.12f;
    [SerializeField] private float bobSpeed = 5f;

    private bool flying;
    private Transform anchor;        // caster to hover in front of
    private Vector3 hoverOffset;
    private Transform target;
    private float damage;
    private Vector2 vel;
    private float life;
    private float bobT;

    // Start hovering in front of the caster.
    public void Hover(Transform caster, Vector3 offset)
    {
        flying = false;
        anchor = caster;
        hoverOffset = offset;
        if (caster != null) transform.position = caster.position + offset;
    }

    // Launch it at a target.
    public void Fire(Transform enemy, float dmg)
    {
        flying = true;
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
            if (anchor != null)
            {
                bobT += Time.deltaTime * bobSpeed;
                transform.position = anchor.position + hoverOffset + new Vector3(0, Mathf.Sin(bobT) * bobHeight, 0);
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
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg);
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
}