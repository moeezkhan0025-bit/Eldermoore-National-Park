using UnityEngine;

// UTILITY (revised): a long blink — warps the player DOUBLE their current warp
// distance in the direction they're facing. Clamped against walls via the
// MovementController's warp obstacle logic. Spawns a poof at leave + arrive.
[CreateAssetMenu(fileName = "Recall", menuName = "Spells/Effects/Recall")]
public class RecallEffect : SpellEffect
{
    [SerializeField] private float distanceMultiplier = 2f;   // double the base warp
    [SerializeField] private GameObject warpVfx;              // placeholder poof

    public override CastResult Cast(GameObject caster)
    {
        var move = caster.GetComponent<MovementController>();
        if (move == null) return CastResult.Fail("No movement controller");

        Vector3 from = caster.transform.position;
        float distance = move.WarpDistance * distanceMultiplier;

        Vector2 landed = move.WarpInFacing(distance);

        if (warpVfx != null)
        {
            Object.Instantiate(warpVfx, from, Quaternion.identity);              // leave
            Object.Instantiate(warpVfx, (Vector3)landed, Quaternion.identity);   // arrive
        }

        return CastResult.Ok();
    }
}