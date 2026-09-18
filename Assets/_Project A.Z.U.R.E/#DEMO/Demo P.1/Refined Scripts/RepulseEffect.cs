using UnityEngine;

// DEFENSIVE: pushes enemies in a radius away. Spawns a burst-ring VFX at the caster.
[CreateAssetMenu(fileName = "Repulse", menuName = "Spells/Effects/Repulse")]
public class RepulseEffect : SpellEffect
{
    [SerializeField] private float radius = 5f;
    [SerializeField] private float pushForce = 15f;
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private GameObject burstVfx;   // placeholder expanding ring at the player

    public override CastResult Cast(GameObject caster)
    {
        Vector2 origin = caster.transform.position;

        if (burstVfx != null)
            Object.Instantiate(burstVfx, caster.transform.position, Quaternion.identity);

        int pushed = 0;
        foreach (var enemy in GameObject.FindGameObjectsWithTag(enemyTag))
        {
            Vector2 toEnemy = (Vector2)enemy.transform.position - origin;
            if (toEnemy.sqrMagnitude > radius * radius) continue;

            Vector2 dir = toEnemy.sqrMagnitude > 0.001f ? toEnemy.normalized : Vector2.up;
            var rb = enemy.GetComponent<Rigidbody2D>();
            if (rb != null) rb.AddForce(dir * pushForce, ForceMode2D.Impulse);
            else enemy.transform.position += (Vector3)(dir * pushForce * 0.1f);
            pushed++;
        }

        return pushed > 0 ? CastResult.Ok() : CastResult.Fail("No enemy in range");
    }
}
