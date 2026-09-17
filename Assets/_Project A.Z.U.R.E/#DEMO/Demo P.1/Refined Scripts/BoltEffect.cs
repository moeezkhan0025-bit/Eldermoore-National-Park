using UnityEngine;

// OFFENSIVE: finds the nearest enemy in range and strikes it for damage.
//   Create > Spells/Effects > Bolt
[CreateAssetMenu(fileName = "Bolt", menuName = "Spells/Effects/Bolt")]
public class BoltEffect : SpellEffect
{
    [SerializeField] private float range = 10f;
    [SerializeField] private float damage = 20f;
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private GameObject strikeVfxPrefab;   // optional visual

    public override void Cast(GameObject caster)
    {
        Vector2 origin = caster.transform.position;
        GameObject target = null;
        float best = range * range;

        foreach (var enemy in GameObject.FindGameObjectsWithTag(enemyTag))
        {
            float d = ((Vector2)enemy.transform.position - origin).sqrMagnitude;
            if (d < best) { best = d; target = enemy; }
        }

        if (target == null) return;   // nothing in range

        if (strikeVfxPrefab != null)
            Instantiate(strikeVfxPrefab, target.transform.position, Quaternion.identity);

        var hp = target.GetComponent<Health>();
        if (hp != null) hp.TakeDamage(damage);
    }
}
