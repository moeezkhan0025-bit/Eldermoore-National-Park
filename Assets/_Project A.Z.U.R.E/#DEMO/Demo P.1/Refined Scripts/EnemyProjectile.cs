using UnityEngine;

// A projectile fired BY an enemy AT the player. Travels in a set direction (or at
// a target), damages the player's PlayerHealth on hit, and can apply a status
// effect. Spawned by EnemyShooter. Swap the sprite/VFX freely.
public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 7f;
    [SerializeField] private float damage = 8f;
    [SerializeField] private float maxLifetime = 5f;
    [SerializeField] private GameObject impactVfx;

    [Header("Status effect (optional)")]
    [SerializeField] private StatusEffectType statusOnHit = StatusEffectType.None;
    [SerializeField] private float statusDuration = 3f;
    [SerializeField] private float statusMagnitude = 1f;   // e.g. dmg/sec for poison/burn

    private Vector2 velocity;
    private float life;

    // Aim it when spawned.
    public void Launch(Vector2 direction)
    {
        velocity = direction.normalized * speed;
        if (velocity.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg);
    }

    void Update()
    {
        life += Time.deltaTime;
        if (life > maxLifetime) { Destroy(gameObject); return; }
        transform.position += (Vector3)(velocity * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponentInParent<PlayerHealth>();
        if (player == null) return;

        player.TakeDamage(damage);

        // Apply status effect if set.
        if (statusOnHit != StatusEffectType.None)
        {
            var status = player.GetComponent<StatusEffectReceiver>();
            if (status != null) status.Apply(statusOnHit, statusDuration, statusMagnitude);
        }

        if (impactVfx != null) Instantiate(impactVfx, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
