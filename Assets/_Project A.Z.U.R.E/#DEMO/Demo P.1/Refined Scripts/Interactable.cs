using UnityEngine;

// Base for anything Triangle can interact with. Subclass it (WarpPad, Doorway,
// item, NPC) and override Interact(). Needs a trigger Collider2D (interaction range);
// PlayerInteractor detects it and calls Interact() when Triangle is pressed.
//
// (Prompt hooks intentionally left out for now — add the floating "Triangle" hint
//  back once the interaction logic is proven. Keeps this decoupled from WorldPrompt.)
[RequireComponent(typeof(Collider2D))]
public class Interactable : MonoBehaviour
{
    [SerializeField] protected string promptText = "Interact";

    // Optional overrides for showing/hiding a prompt later. No-ops for now.
    public virtual void ShowPrompt() { }
    public virtual void HidePrompt() { }

    // Override with what the object does. Base is a harmless no-op.
    public virtual void Interact(GameObject player) { }
}