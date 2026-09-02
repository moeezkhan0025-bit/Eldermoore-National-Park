using UnityEngine;
using UnityEngine.InputSystem;

// Controller input via the new Input System, reading from the generated
// GameController wrapper. Implements IPlayerInput, so it drop-in swaps with the
// legacy PlayerInputHandler. Put it on the sandbox player (one handler per player).
//
// Uses your actual Player map actions: Move, Jump, Warp, Climb, Cast.
// (Climb is what feeds GrabHeld — the climb-grip input.)
public class ControllerInputHandler : MonoBehaviour, IPlayerInput
{
    [SerializeField] float downThreshold = 0.5f;

    GameController controls;

    // Exposed so CastController and the UI can share this one instance/asset.
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
    public bool GrabHeld => controls.Player.Climb.IsPressed();   // Climb action = grip input
    public bool WarpPressed => controls.Player.Warp.WasPressedThisFrame();
    // in ControllerInputHandler, with the other reads:
    public bool InteractPressed => controls.Player.Interact.WasPressedThisFrame();
}