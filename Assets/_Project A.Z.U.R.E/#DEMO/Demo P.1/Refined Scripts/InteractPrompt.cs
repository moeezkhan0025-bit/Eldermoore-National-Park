using UnityEngine;
using UnityEngine.UI;

// A floating "press Triangle" prompt above an interactable. Shows the button icon
// when the player is near; grays/darkens it briefly when pressed for feedback.
// Put on a world-space child of the interactable (a small canvas or a sprite),
// and reference it from the Interactable's prompt hookup.
public class InteractPrompt : MonoBehaviour
{
    [SerializeField] private GameObject root;        // the visual to show/hide
    [SerializeField] private Image buttonIcon;       // the Triangle icon
    [SerializeField] private Color idleColor = Color.white;
    [SerializeField] private Color pressedColor = new Color(0.4f, 0.4f, 0.4f);
    [SerializeField] private float pressedFlashTime = 0.15f;

    private float pressedUntil;

    void Awake()
    {
        if (root == null) root = gameObject;
        root.SetActive(false);
    }

    public void Show()
    {
        root.SetActive(true);
        if (buttonIcon != null) buttonIcon.color = idleColor;
    }

    public void Hide() => root.SetActive(false);

    // Call when the player presses interact while this prompt is up (gray flash).
    public void FlashPressed()
    {
        if (buttonIcon != null) buttonIcon.color = pressedColor;
        pressedUntil = Time.time + pressedFlashTime;
    }

    void Update()
    {
        if (buttonIcon != null && Time.time >= pressedUntil && buttonIcon.color == pressedColor)
            buttonIcon.color = idleColor;
    }
}
