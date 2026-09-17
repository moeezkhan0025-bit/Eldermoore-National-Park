using UnityEngine;

// A damage / kill zone (stalagmites, stalactites, spikes, lava). Put on an object
// with a trigger Collider2D. On player contact it either deals damage, or respawns
// the player at the last checkpoint (set respawnInstead for pits / instant-death).
[RequireComponent(typeof(Collider2D))]
public class Hazard : MonoBehaviour
{
    [Header("Effect")]
    [SerializeField] private float damage = 10f;
    [Tooltip("If true, teleport the player to the last Checkpoint instead of damaging.")]
    [SerializeField] private bool respawnInstead = false;

    [Header("Contact")]
    [SerializeField] private float retriggerDelay = 0.5f;   // avoid multi-hits per frame
    private float nextHitTime;

    void OnTriggerEnter2D(Collider2D other) => TryHit(other);
    void OnTriggerStay2D(Collider2D other)  => TryHit(other);   // damages while standing in it

    void TryHit(Collider2D other)
    {
        if (Time.time < nextHitTime) return;

        var player = other.GetComponentInParent<MovementController>();
        if (player == null) return;

        nextHitTime = Time.time + retriggerDelay;

        if (respawnInstead && Checkpoint.Active != null)
        {
            Vector3 spot = Checkpoint.Active.RespawnPoint;
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) { rb.position = spot; rb.velocity = Vector2.zero; }
            else player.transform.position = spot;
        }
        else
        {
            var hp = player.GetComponent<PlayerHealth>();
            if (hp != null) hp.TakeDamage(damage);
        }
    }
}
