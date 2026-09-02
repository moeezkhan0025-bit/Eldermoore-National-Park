using System.Collections.Generic;
using UnityEngine;

// Put this on the player. Tracks interactables in range, shows the nearest one's
// prompt, and calls Interact() on it when the player presses Triangle (InteractPressed).
[RequireComponent(typeof(Collider2D))]
public class PlayerInteractor : MonoBehaviour
{
    private IPlayerInput input;
    private readonly List<Interactable> inRange = new List<Interactable>();
    private Interactable current;

    void Awake()
    {
        input = GetComponent<IPlayerInput>();
        if (input == null)
            Debug.LogError($"[{name}] PlayerInteractor needs an IPlayerInput on the player.", this);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var it = other.GetComponent<Interactable>();
        if (it != null && !inRange.Contains(it)) inRange.Add(it);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var it = other.GetComponent<Interactable>();
        if (it == null) return;
        inRange.Remove(it);
        if (it == current) { it.HidePrompt(); current = null; }
    }

    void Update()
    {
        Interactable nearest = GetNearest();
        if (nearest != current)
        {
            if (current != null) current.HidePrompt();
            current = nearest;
            if (current != null) current.ShowPrompt();
        }

        if (current != null && input != null && input.InteractPressed)
            current.Interact(gameObject);
    }

    Interactable GetNearest()
    {
        Interactable best = null;
        float bestSqr = float.MaxValue;
        Vector3 p = transform.position;
        for (int i = inRange.Count - 1; i >= 0; i--)
        {
            if (inRange[i] == null) { inRange.RemoveAt(i); continue; }
            float d = (inRange[i].transform.position - p).sqrMagnitude;
            if (d < bestSqr) { bestSqr = d; best = inRange[i]; }
        }
        return best;
    }
}
