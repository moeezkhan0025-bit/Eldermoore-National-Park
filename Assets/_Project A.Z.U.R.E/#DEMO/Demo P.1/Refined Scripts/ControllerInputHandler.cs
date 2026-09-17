using UnityEngine;
using UnityEngine.InputSystem;

// Controller input via the new Input System, reading from the generated
// GameController wrapper. Implements IPlayerInput (all nine members), so it
// drop-in swaps with the legacy PlayerInputHandler. One handler per player.
//
// Player map actions used: Move, Jump, Warp, Climb, Cast, Interact.
public class ControllerInputHandler : MonoBehaviour, IPlayerInput
{
    [SerializeField] float downThreshold = 0.5f;

    GameController controls;

    public GameController Controls => controls;

    void Awake() { controls = new GameController(); }
    void OnEnable() { controls.Player.Enable(); }
    void OnDisable() { controls.Player.Disable(); }
    void OnDestroy() { controls.Dispose(); }

    Vector2 Move => controls.Player.Move.ReadValue<Vector2>();

    public float MoveInput => Move.x;
    public float VerticalInput => Move.y;
    public bool JumpHeld => controls.Player.Jump.IsPressed();
    public bool JumpPressed => controls.Player.Jump.WasPressedThisFrame() && Move.y > -downThreshold;
    public bool DropPressed => controls.Player.Jump.WasPressedThisFrame() && Move.y <= -downThreshold;
    public bool GrabHeld => controls.Player.Climb.IsPressed();          // Climb = grip
    public bool GrabPressed => controls.Player.Climb.WasPressedThisFrame(); // Climb press = latch
    public bool WarpPressed => controls.Player.Warp.WasPressedThisFrame();
    public bool InteractPressed => controls.Player.Interact.WasPressedThisFrame();
}