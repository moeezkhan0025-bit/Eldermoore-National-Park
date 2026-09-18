using UnityEngine;

// A magic-missile projectile: spawns, flies toward a target, and deals damage on
// arrival, then spawns an impact VFX and destroys itself. Bolt launches this from
// in front of the player toward the locked enemy.
public class HomingProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float turnRate = 720f;      // deg/sec — how sharply it homes
    [SerializeField] private float hitDistance = 0.3f;    // how close counts as a hit
    [SerializeField] private float maxLifetime = 3f;      // safety despawn
    [SerializeField] private GameObject impactVfx;        // spawned on hit

    private Transform target;
    private float damage;
    private Vector2 velocity;
    private float life;

    // Called right after Instantiate to aim it.
    public void Launch(Transform targetEnemy, float dmg, Vector2 initialDir)
    {
        target = targetEnemy;
        damage = dmg;
        velocity = initialDir.normalized * speed;
    }

    void Update()
    {
        life += Time.deltaTime;
        if (life > maxLifetime) { Destroy(gameObject); return; }

        // Home toward the target if it still exists.
        if (target != null)
        {
            Vector2 desired = ((Vector2)target.position - (Vector2)transform.position).normalized * speed;
            velocity = Vector2.MoveTowards(velocity, desired, turnRate * Mathf.Deg2Rad * speed * Time.deltaTime);

            if (((Vector2)target.position - (Vector2)transform.position).sqrMagnitude <= hitDistance * hitDistance)
            {
                Hit();
                return;
            }
        }

        transform.position += (Vector3)(velocity * Time.deltaTime);

        // Face travel direction (for a missile sprite).
        if (velocity.sqrMagnitude > 0.01f)
        {
            float ang = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, ang);
        }
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
