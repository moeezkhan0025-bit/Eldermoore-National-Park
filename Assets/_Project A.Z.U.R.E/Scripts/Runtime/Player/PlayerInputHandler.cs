using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public float MoveInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool JumpHeld { get; private set; }

    PlayerInputActions actions;

    void Awake() => actions = new PlayerInputActions();
    void OnEnable() => actions.Enable();
    void OnDisable() => actions.Disable();

    void Update()
    {
        MoveInput = actions.Player.Move.ReadValue<Vector2>().x;
        JumpHeld = actions.Player.Jump.IsPressed();
        JumpPressed = actions.Player.Jump.WasPressedThisFrame();
    }
}