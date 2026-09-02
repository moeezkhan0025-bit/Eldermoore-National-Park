using UnityEngine;

// A fixed-link warp ZONE, now driven by the shared interaction system.
// The PlayerInteractor detects this pad's trigger and calls Interact() when the
// player presses Triangle — so this script no longer reads input, tracks range,
// or manages its own prompt (the interactor owns all that).
//
// WIRING (per pad), unchanged from before:
//   destination -> the OTHER pad   (these CROSS: A->B, B->A)
//   exitPoint   -> a child of THIS pad, on the ground at THIS pad's location
//                  (each pad's exit belongs to itself — these DON'T cross)
public class WarpPad : Interactable
{
    [SerializeField] private WarpPad destination;   // the partner zone (REQUIRED)
    [SerializeField] private Transform exitPoint;   // GROUND landing spot for THIS pad

    // Triangle pressed while standing on this pad -> travel to the partner.
    public override void Interact(GameObject player)
    {
        if (destination == null || destination == this)
        {
            Debug.LogWarning($"[{name}] WarpPad has no valid destination.", this);
            return;
        }
        destination.ReceiveWarp(player.transform);
    }

    // Called by the SOURCE pad to place the player at THIS pad's landing spot.
    public void ReceiveWarp(Transform player)
    {
        Vector3 target = exitPoint != null ? exitPoint.position : transform.position;
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) { rb.position = target; rb.velocity = Vector2.zero; }
        else player.position = target;
    }

    // --- Editor visualization ---
    // Green sphere = where players land HERE.  Cyan line = the partner link.
    private void OnDrawGizmos()
    {
        Vector3 landing = exitPoint != null ? exitPoint.position : transform.position;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(landing, 0.2f);
        if (destination != null && destination != this)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, destination.transform.position);
        }
    }
}