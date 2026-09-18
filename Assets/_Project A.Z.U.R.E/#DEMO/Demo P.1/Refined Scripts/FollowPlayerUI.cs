using UnityEngine;

// Makes a screen-space UI element hover above the player in the world. Put this
// on the CastModeUI panel (or the card). Each frame it projects the player's
// world position to screen space and positions the UI there, offset upward.
// Keeps crisp screen-space UI while tracking the player.
public class FollowPlayerUI : MonoBehaviour
{
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.5f, 0f);  // above the head
    [SerializeField] private Vector2 screenOffset = Vector2.zero;

    private RectTransform rect;
    private Camera cam;
    private Transform target;

    void Awake() => rect = GetComponent<RectTransform>();

    void LateUpdate()
    {
        if (target == null) AcquireTarget();
        if (cam == null) cam = Camera.main;
        if (target == null || cam == null) return;

        Vector3 worldPos = target.position + worldOffset;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
        rect.position = (Vector2)screenPos + screenOffset;
    }

    void AcquireTarget()
    {
        if (PlayerPersistence.Instance != null) target = PlayerPersistence.Instance.transform;
        else
        {
            var mc = FindObjectOfType<MovementController>();
            if (mc != null) target = mc.transform;
        }
    }
}
