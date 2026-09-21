using UnityEngine;

// DEFENSIVE (revised): grants the player a damage-absorbing shield. The shield soaks
// up to shieldAmount of incoming damage before the player's health is touched.
[CreateAssetMenu(fileName = "Repulse", menuName = "Spells/Effects/Repulse")]
public class RepulseEffect : SpellEffect
{
    [SerializeField] private float shieldAmount = 20f;
    [SerializeField] private GameObject castVfx;   // optional burst when the shield goes up

    public override CastResult Cast(GameObject caster)
    {
        var shield = caster.GetComponent<PlayerShield>();
        if (shield == null) return CastResult.Fail("No shield component");

        shield.Activate(shieldAmount);

        if (castVfx != null)
            Object.Instantiate(castVfx, caster.transform.position, Quaternion.identity);

        return CastResult.Ok();
    }
}