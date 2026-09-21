using UnityEngine;

// OFFENSIVE magic missile — with tracing to diagnose why it fires or dismisses.
[CreateAssetMenu(fileName = "Bolt", menuName = "Spells/Effects/Bolt")]
public class BoltEffect : SpellEffect
{
    [SerializeField] private float range = 12f;
    [SerializeField] private float damage = 20f;
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private HomingProjectile fallbackProjectile;
    [SerializeField] private float spawnForwardOffset = 0.8f;

    public override CastResult Cast(GameObject caster)
    {
        Vector2 origin = caster.transform.position;

        var enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        Debug.Log($"[Bolt] Cast start. enemies tagged '{enemyTag}' = {enemies.Length}, range={range}");

        GameObject target = null;
        float best = range * range;
        foreach (var enemy in enemies)
        {
            float sq = ((Vector2)enemy.transform.position - origin).sqrMagnitude;
            Debug.Log($"[Bolt]   candidate {enemy.name} dist={Mathf.Sqrt(sq):0.0}");
            if (sq < best) { best = sq; target = enemy; }
        }

        if (target == null)
        {
            Debug.Log("[Bolt] RESULT: no target in range -> Fail (missile dismissed).");
            return CastResult.Fail("No enemy found");
        }

        HomingProjectile missile = PendingChargeVisual;
        Debug.Log($"[Bolt] target={target.name}, hovering missile passed in = {(missile != null ? missile.name : "NULL")}");

        if (missile == null)
        {
            if (fallbackProjectile == null)
            {
                Debug.LogWarning("[Bolt] No hovering missile AND no fallback -> Fail.");
                return CastResult.Fail("No projectile");
            }
            var move = caster.GetComponent<MovementController>();
            Vector2 facing = (move != null && !move.FacingRight) ? Vector2.left : Vector2.right;
            missile = Object.Instantiate(fallbackProjectile, origin + facing * spawnForwardOffset, Quaternion.identity);
            Debug.Log("[Bolt] Spawned fallback missile.");
        }

        Debug.Log($"[Bolt] FIRING {missile.name} at {target.name}");
        missile.Fire(target.transform, damage);
        return CastResult.Ok();
    }
}