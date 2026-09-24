using UnityEngine;

// Put this on ANY object in the RangerHQ scene (an empty GameObject). When that
// scene loads, this runs and turns the persistent HUD on. Simple, explicit, no
// scene-name matching — the HUD turns on because this object exists in the scene.
public class HUDActivator : MonoBehaviour
{
    void Start()
    {
        if (HUDVisibility.Instance != null)
            HUDVisibility.Instance.ShowHUD();
        else
            Debug.LogWarning("[HUDActivator] No HUDVisibility.Instance found — is the HUD in the bootstrap scene?");
    }
}
