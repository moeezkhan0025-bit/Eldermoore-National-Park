using UnityEngine;

// A fixed-link warp ZONE. The collider is a TRIGGER, so it's a walk-through
// hitbox, not a solid pad. Wire each pad's "destination" to its partner
// (A -> B and B -> A) and the player travels back and forth.
//
// WIRING (per pad):
//   destination -> the OTHER pad   (these CROSS: A->B, B->A)
//   exitPoint   -> a child of THIS pad, on the ground at THIS pad's location
//                  (these DO NOT cross — each pad's exit point belongs to itself)
[RequireComponent(typeof(Collider2D))]
public class WarpPad : MonoBehaviour
{
    [SerializeField] private WarpPad destination;      // the partner zone (REQUIRED)
    [SerializeField] private KeyCode activateKey = KeyCode.E;
    [SerializeField] private Transform exitPoint;      // GROUND landing spot for THIS pad
    [SerializeField] private GameObject prompt;        // "Press E to warp" hint (optional)

    private Transform playerInRange;
    private bool justArrived;   // guard: prevents instantly warping back on arrival

    private void Start()
    {
        if (prompt != null) prompt.SetActive(false);
    }

    private void Update()
    {
        if (playerInRange != null && !justArrived && Input.GetKeyDown(activateKey))
            Activate();
    }

    private void Activate()
    {
        if (destination == null || destination == this) return;
        destination.ReceiveWarp(playerInRange);
    }

    // Called by the SOURCE zone to place the player at THIS zone's landing spot.
    public void ReceiveWarp(Transform player)
    {
        Vector3 target = exitPoint != null ? exitPoint.position : transform.position;

        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.position = target;
            rb.velocity = Vector2.zero;   // don't carry momentum through the warp
        }
        else
        {
            player.position = target;
        }

        // FIX: only guard against an instant bounce-back if the player actually
        // landed INSIDE this pad's trigger. If the exit point is on the ground
        // below a floating hitbox, the player lands OUTSIDE the trigger — so
        // OnTriggerExit2D would never fire and the guard would stay stuck on,
        // permanently blocking the return trip.
        var col = GetComponent<Collider2D>();
        justArrived = col.OverlapPoint(target);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var mc = other.GetComponentInParent<MovementController>();
        if (mc == null) return;

        playerInRange = mc.transform;   // the PLAYER ROOT, not a child check-collider
        if (prompt != null && !justArrived) prompt.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponentInParent<MovementController>() == null) return;

        playerInRange = null;
        justArrived = false;            // leaving the zone clears the guard
        if (prompt != null) prompt.SetActive(false);
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