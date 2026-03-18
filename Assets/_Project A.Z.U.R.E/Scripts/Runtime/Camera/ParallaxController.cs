using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    ParallaxLayer[] layers;
    Vector3 previousCamPos;

    void Start()
    {
        layers = GetComponentsInChildren<ParallaxLayer>();
        previousCamPos = Camera.main.transform.position;

        if (layers.Length == 0)
            Debug.LogWarning("[ParallaxController] No ParallaxLayer components found in children!");
        else
            Debug.Log($"[ParallaxController] Found {layers.Length} parallax layers.");
    }

    void LateUpdate()
    {
        if (Camera.main == null) return;

        Vector3 delta = Camera.main.transform.position - previousCamPos;

        foreach (var layer in layers)
            layer.Move(delta);

        previousCamPos = Camera.main.transform.position;
    }
}