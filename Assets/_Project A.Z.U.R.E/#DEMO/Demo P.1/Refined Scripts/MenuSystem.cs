using UnityEngine;

// Put this on the MenuSystem root in the bootstrap scene. It keeps the whole
// menu (Canvas + EventSystem) alive across scene loads and ensures there's only
// ever one — same pattern as your GameManager singleton.
public class MenuSystem : MonoBehaviour
{
    public static MenuSystem Instance { get; private set; }

    void Awake()
    {
        // If one already exists (e.g. this scene also had a MenuSystem), destroy the duplicate.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
