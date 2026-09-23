using UnityEngine;

// Base for anything Triangle can interact with. Subclass it (Doorway, WarpPad,
// TeleportPoint, NPC) and override Interact(). Shows an InteractPrompt when the
// player's near, and grays it when pressed.
[RequireComponent(typeof(Collider2D))]
public class Interactable : MonoBehaviour
{
    [SerializeField] protected InteractPrompt prompt;   // the floating Triangle prompt (optional)

    public virtual void ShowPrompt() { if (prompt != null) prompt.Show(); }
    public virtual void HidePrompt() { if (prompt != null) prompt.Hide(); }
    public virtual void FlashPrompt() { if (prompt != null) prompt.FlashPressed(); }

    // Override with what the object does. Base is a harmless no-op.
    public virtual void Interact(GameObject player) { }
}