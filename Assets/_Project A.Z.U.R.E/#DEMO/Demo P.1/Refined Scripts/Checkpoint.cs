using UnityEngine;

// A respawn / safe-zone checkpoint. Walk into its trigger to make it the active
// one; the RecallEffect and respawn-hazards send the player to Checkpoint.Active.
// Put on an object with a trigger Collider2D.
public class Checkpoint : MonoBehaviour
{
    public static Checkpoint Active { get; private set; }

    [SerializeField] private Transform respawnPoint;   // where to reappear (defaults to this)

    public Vector3 RespawnPoint => respawnPoint != null ? respawnPoint.position : transform.position;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<MovementController>() != null)
            Active = this;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = (Active == this) ? Color.green : Color.gray;
        Gizmos.DrawWireCube(RespawnPoint, Vector3.one * 0.4f);
    }
}
