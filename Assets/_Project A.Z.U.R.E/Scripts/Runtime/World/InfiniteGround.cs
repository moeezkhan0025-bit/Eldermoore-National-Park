using UnityEngine;

// Attach to Ground_Exploration
// Keeps the ground centred under the camera at all times
// by snapping its position to a grid — feels infinite
public class InfiniteGround : MonoBehaviour
{
    [SerializeField] Transform cameraTransform;
    [SerializeField] float groundWidth = 20f;

    void LateUpdate()
    {
        // Snap ground X position to stay under camera
        float camX = cameraTransform.position.x;
        float snapX = Mathf.Round(camX / groundWidth) * groundWidth;

        transform.position = new Vector3(
            snapX,
            transform.position.y,
            transform.position.z
        );
    }
}