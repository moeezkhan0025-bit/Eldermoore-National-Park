using UnityEngine;

// UTILITY: emergency warp back to the last checkpoint (the "previous safe zone").
//   Create > Spells/Effects > Recall
[CreateAssetMenu(fileName = "Recall", menuName = "Spells/Effects/Recall")]
public class RecallEffect : SpellEffect
{
    public override void Cast(GameObject caster)
    {
        if (Checkpoint.Active == null) { Debug.LogWarning("[Recall] No active checkpoint."); return; }

        Vector3 safe = Checkpoint.Active.RespawnPoint;
        var rb = caster.GetComponent<Rigidbody2D>();
        if (rb != null) { rb.position = safe; rb.velocity = Vector2.zero; }
        else caster.transform.position = safe;
    }
}
