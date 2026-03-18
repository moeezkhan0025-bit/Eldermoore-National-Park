using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [Range(0f, 1f)]
    [Tooltip("0 = static, 1 = moves with world. Higher = closer to camera.")]
    public float scrollMultiplier = 0.5f;

    public void Move(Vector3 delta)
    {
        transform.position += new Vector3(
            delta.x * scrollMultiplier,
            0f,
            0f
        );
    }
}