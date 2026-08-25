using UnityEngine;

// Makes the player persist across scenes and guarantees only ONE ever exists.
// The player is NOT placed in any scene — GameManager spawns it the first time
// you enter a gameplay scene, and it survives from then on.
public class PlayerPersistence : MonoBehaviour
{
    public static PlayerPersistence Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);   // duplicate — the existing persistent player wins
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}