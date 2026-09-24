using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerInputHandler : MonoBehaviour, IPlayerInput
{
    [SerializeField] float downThreshold = 0.5f;

    GameController controls;

    public GameController Controls => controls;

    void Awake() { if (controls == null) controls = new GameController(); }
    void OnEnable() { if (controls == null) controls = new GameController(); controls.Player.Enable(); }
    void OnDisable() { controls?.Player.Disable(); }
    void OnDestroy() { controls?.Dispose(); }

    Vector2 Move => controls.Player.Move.ReadValue<Vector2>();

    public float MoveInput => Move.x;
    public float VerticalInput => Move.y;
    public bool JumpHeld => controls.Player.Jump.IsPressed();
    public bool JumpPressed => controls.Player.Jump.WasPressedThisFrame() && Move.y > -downThreshold;
    public bool DropPressed => controls.Player.Jump.WasPressedThisFrame() && Move.y <= -downThreshold;
    public bool GrabHeld => controls.Player.Climb.IsPressed();
    public bool GrabPressed => controls.Player.Climb.WasPressedThisFrame();
    public bool WarpPressed => controls.Player.Warp.WasPressedThisFrame();
    public bool InteractPressed => controls.Player.Interact.WasPressedThisFrame();
}