using UnityEngine;

// Marks a place the player can spawn when entering a scene through a doorway.
// The Doorway names a spawnPointId; the matching SpawnPoint here receives the player.
public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private string id = "FromRangerHQ";

    public string Id => id;

    // Optional: draw a marker so you can see spawn points in the editor.
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.25f);
    }
}
