using UnityEngine;

// OFFENSIVE: launches a homing magic-missile toward the nearest enemy.
// (Debug logging included to trace why the projectile may not spawn.)
[CreateAssetMenu(fileName = "Bolt", menuName = "Spells/Effects/Bolt")]
public class BoltEffect : SpellEffect
{
    [SerializeField] private float range = 12f;
    [SerializeField] private float damage = 20f;
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private HomingProjectile projectilePrefab;
    [SerializeField] private float spawnForwardOffset = 0.8f;

    public override CastResult Cast(GameObject caster)
    {
        Vector2 origin = caster.transform.position;

        GameObject target = null;
        float best = range * range;
        foreach (var enemy in GameObject.FindGameObjectsWithTag(enemyTag))
        {
            float d = ((Vector2)enemy.transform.position - origin).sqrMagnitude;
            if (d < best) { best = d; target = enemy; }
        }

        Debug.Log($"[Bolt] Cast. target={(target != null ? target.name : "NULL")}, prefab={(projectilePrefab != null ? projectilePrefab.name : "NULL")}");

        if (target == null) { Debug.Log("[Bolt] No target in range."); return CastResult.Fail("No enemy found"); }
        if (projectilePrefab == null) { Debug.LogWarning("[Bolt] Projectile prefab is NULL."); return CastResult.Fail("No projectile"); }

        var move = caster.GetComponent<MovementController>();
        Vector2 facing = (move != null && !move.FacingRight) ? Vector2.left : Vector2.right;
        Vector2 spawnPos = origin + facing * spawnForwardOffset;

        var proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        Debug.Log($"[Bolt] Spawned {proj.name} at {spawnPos}");
        Vector2 toEnemy = ((Vector2)target.transform.position - spawnPos).normalized;
        proj.Launch(target.transform, damage, toEnemy);

        return CastResult.Ok();
    }
}