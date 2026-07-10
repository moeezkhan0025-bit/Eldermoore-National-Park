using UnityEngine;

// Makes the player persist across scene loads, and guarantees only ONE player
// ever exists. Put this on the player object in the RangerHQ scene (the first
// gameplay scene the player appears in). Any other scene that also contains a
// player with this component will have its copy destroyed on arrival, so the
// original carried-in player is the one that survives.
public class PlayerPersistence : MonoBehaviour
{
    public static PlayerPersistence Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // A persistent player already exists — this is a duplicate. Remove it.
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
