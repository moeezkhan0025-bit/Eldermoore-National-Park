using UnityEngine;
using UnityEngine.SceneManagement;

// Attach to a save-location object in the Game scene.
// Calling Save() is the first (and only) time a file is written for a new game.
public class SavePoint : MonoBehaviour
{
    [SerializeField] private Transform player;          // whose position to record (optional)
    [SerializeField] private bool saveOnTriggerEnter = true;

    // Call this from a trigger, an interact key, or a UI "Save" button.
    public void Save()
    {
        var gm = GameManager.Instance;
        if (gm == null || gm.CurrentSave == null) return;

        // Capture current state into the in-memory save...
        gm.CurrentSave.sceneName = SceneManager.GetActiveScene().name;
        if (player != null)
            gm.CurrentSave.playerPosition = player.position;

        // ...then write it to disk.
        gm.SaveCurrent();
        Debug.Log($"Game saved to slot {gm.CurrentSlot}.");
    }

    // 3D physics. If your game is 2D, rename to OnTriggerEnter2D(Collider2D other).
    private void OnTriggerEnter(Collider other)
    {
        if (!saveOnTriggerEnter) return;
        if (other.CompareTag("Player")) Save();
    }
}
