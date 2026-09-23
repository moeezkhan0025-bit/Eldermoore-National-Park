using System.Collections.Generic;
using UnityEngine;

// Detects nearby interactables by ACTIVELY SCANNING each frame (not trigger events),
// so it always works regardless of how the player arrived near one. Shows the
// nearest interactable's prompt and calls Interact() on the interact button.
public class PlayerInteractor : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float interactRadius = 1.5f;
    [SerializeField] private Vector2 scanOffset = new Vector2(0f, 0.5f);   // raise the scan center off the feet
    [SerializeField] private LayerMask interactableLayer = ~0;   // set to your Interactable layer

    private IPlayerInput input;
    private Interactable current;
    private readonly Collider2D[] hits = new Collider2D[8];

    void Awake()
    {
        input = GetComponent<IPlayerInput>();
        if (input == null)
            Debug.LogError($"[{name}] PlayerInteractor needs an IPlayerInput on the player.", this);
    }

    void Update()
    {
        Interactable nearest = FindNearest();

        if (nearest != current)
        {
            if (current != null) current.HidePrompt();
            current = nearest;
            if (current != null) current.ShowPrompt();
        }

        if (current != null && input != null && input.InteractPressed)
        {
            current.FlashPrompt();          // gray the button as it's pressed
            current.Interact(gameObject);
        }
    }

    Interactable FindNearest()
    {
        Vector2 center = (Vector2)transform.position + scanOffset;
        int count = Physics2D.OverlapCircleNonAlloc(center, interactRadius, hits, interactableLayer);
        Interactable best = null;
        float bestSqr = float.MaxValue;
        Vector3 p = transform.position;

        for (int i = 0; i < count; i++)
        {
            if (hits[i] == null) continue;
            var it = hits[i].GetComponent<Interactable>();
            if (it == null) it = hits[i].GetComponentInParent<Interactable>();
            if (it == null) continue;

            float d = ((Vector2)it.transform.position - center).sqrMagnitude;
            if (d < bestSqr) { bestSqr = d; best = it; }
        }
        return best;
    }

    // Optional: expose whether interact was pressed this frame, for the prompt to gray out.
    public bool InteractHeldThisFrame => input != null && input.InteractPressed;

    [Header("Gizmo")]
    [SerializeField] private bool alwaysShowGizmo = true;

    void OnDrawGizmos()
    {
        if (!alwaysShowGizmo) return;
        DrawGizmo();
    }

    void OnDrawGizmosSelected()
    {
        if (alwaysShowGizmo) return;   // avoid double-draw
        DrawGizmo();
    }

    void DrawGizmo()
    {
        // The interact radius (cyan ring + faint fill).
        Vector3 c = transform.position + (Vector3)scanOffset;
        Gizmos.color = new Color(0f, 1f, 1f, 0.15f);
        Gizmos.DrawSphere(c, interactRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(c, interactRadius);

        // A line to the current interactable target while playing.
        if (Application.isPlaying && current != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, current.transform.position);
        }
    }
}