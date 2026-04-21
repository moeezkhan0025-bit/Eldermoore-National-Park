using System.Collections;
using UnityEngine;

public class PlayerDropThrough : MonoBehaviour
{
    [Header("Layers")]
    public string platformLayer = "OneWayPlatform";

    [Header("Settings")]
    public float dropCooldown = 0.4f;
    public KeyCode dropKey = KeyCode.S;

    private bool isDropping = false;
    private int platformLayerIndex;
    private int playerLayerIndex;

    void Start()
    {
        platformLayerIndex = LayerMask.NameToLayer(platformLayer);
        playerLayerIndex = gameObject.layer;
    }

    void Update()
    {
        if (Input.GetKeyDown(dropKey))
        {
            Debug.Log("Drop key pressed");
            Debug.Log("IsOnPlatform: " + IsOnPlatform());
            Debug.Log("isDropping: " + isDropping);

            if (!isDropping && IsOnPlatform())
            {
                StartCoroutine(DropThrough());
            }
        }
    }

    private IEnumerator DropThrough()
    {
        isDropping = true;

        Physics2D.IgnoreLayerCollision(playerLayerIndex, platformLayerIndex, true);

        yield return new WaitForSeconds(dropCooldown);

        Physics2D.IgnoreLayerCollision(playerLayerIndex, platformLayerIndex, false);

        isDropping = false;
    }

    private bool IsOnPlatform()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            Vector2.down,
            0.2f,
            LayerMask.GetMask(platformLayer)
        );
        return hit.collider != null;
    }
}