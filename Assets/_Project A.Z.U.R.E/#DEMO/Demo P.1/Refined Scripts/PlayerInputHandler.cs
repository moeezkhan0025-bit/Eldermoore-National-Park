using UnityEngine;

// Reads input and exposes it to the MovementController.
// This uses Unity's LEGACY Input Manager (Edit > Project Settings > Input Manager),
// which has "Horizontal", "Vertical", and "Jump" defined by default.
//
// Drop-through: hold DOWN + press Jump (so it doesn't also trigger a normal jump).
//
// If your project uses the new Input System package instead, replace the bodies
// of these properties with reads from your InputActions.
public class PlayerInputHandler : MonoBehaviour
{
    [Tooltip("How far down the stick/key must be held to count as 'pressing down' for drop-through.")]
    [SerializeField] float downThreshold = 0.5f;

    // Analog horizontal, -1..1.
    public float MoveInput => Input.GetAxisRaw("Horizontal");

    // True while the jump button is held (used for variable jump height).
    public bool JumpHeld => Input.GetButton("Jump");

    // True for the one frame jump is pressed — but NOT when holding down (that's a drop).
    public bool JumpPressed =>
        Input.GetButtonDown("Jump") && Input.GetAxisRaw("Vertical") > -downThreshold;

    // True for the one frame jump is pressed WHILE holding down (drop through one-way platforms).
    public bool DropPressed =>
        Input.GetButtonDown("Jump") && Input.GetAxisRaw("Vertical") <= -downThreshold;
}
