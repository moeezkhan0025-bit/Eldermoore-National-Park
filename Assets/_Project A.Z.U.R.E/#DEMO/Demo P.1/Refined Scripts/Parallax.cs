using UnityEngine;

// Put one of these on each BACKGROUND LAYER parent (a GameObject holding that
// layer's sprites). The layer drifts a fraction of the camera's movement to fake depth.
// Scale-independent: works the same at any PPU / world scale because it's delta-based.
//
// parallaxFactor:
//   1.0  = locked to the camera, never appears to move (infinitely far, e.g. sky)
//   0.7–0.9 = far background (scrolls slowly)
//   0.3–0.6 = mid background
//   0.1–0.3 = near background (scrolls almost like the world)
//   0.0  = sits in the world, scrolls at full speed like a gameplay object
//   < 0  = foreground, scrolls FASTER than the world (rushes past in front of the player)
public class Parallax : MonoBehaviour
{
    [Tooltip("See header. 1 = far/locked to camera, 0 = world speed, negative = foreground.")]
    [SerializeField, Range(-1f, 1f)] float parallaxFactor = 0.7f;

    [Tooltip("Also parallax vertically (useful for tall rooms / climbing). Off = horizontal only.")]
    [SerializeField] bool vertical = false;

    Transform cam;
    Vector3 lastCamPos;

    void Start()
    {
        cam = Camera.main != null ? Camera.main.transform : null;
        if (cam == null) { Debug.LogError($"[{name}] Parallax: no Camera tagged 'MainCamera' found.", this); enabled = false; return; }
        lastCamPos = cam.position;
    }

    // LateUpdate so it runs after the Cinemachine brain has moved the camera this frame.
    void LateUpdate()
    {
        Vector3 delta = cam.position - lastCamPos;
        transform.position += new Vector3(
            delta.x * parallaxFactor,
            vertical ? delta.y * parallaxFactor : 0f,
            0f);
        lastCamPos = cam.position;
    }
}
