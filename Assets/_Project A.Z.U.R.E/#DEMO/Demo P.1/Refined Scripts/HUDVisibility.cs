using UnityEngine;

// Dead-simple persistent HUD toggle. Starts OFF. Something in the gameplay scene
// (a trigger object) calls ShowHUD() to turn it on; it stays on afterward.
// No scene-name detection — you control exactly when it turns on.
public class HUDVisibility : MonoBehaviour
{
    public static HUDVisibility Instance { get; private set; }

    [SerializeField] private GameObject hudRoot;

    void Awake()
    {
        Instance = this;
        if (hudRoot == null) hudRoot = gameObject;
        Debug.Log($"[HUDVis-boot] Awake ran. hudRoot='{hudRoot.name}'. Setting hidden.");
        hudRoot.SetActive(false);   // start hidden
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    // Call this to turn the HUD on (stays on).
    public void ShowHUD()
    {
        Debug.Log($"[HUDVis-boot] ShowHUD called. hudRoot='{(hudRoot != null ? hudRoot.name : "NULL")}' -> active true");
        if (hudRoot != null) hudRoot.SetActive(true);
    }

    // Call this to turn it off if ever needed.
    public void HideHUD()
    {
        if (hudRoot != null) hudRoot.SetActive(false);
    }
}