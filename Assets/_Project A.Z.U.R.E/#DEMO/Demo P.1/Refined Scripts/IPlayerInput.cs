// The input contract the MovementController reads. Any handler that implements
// this can drive the player — legacy keyboard, new-Input-System controller, etc.
// Swap handlers freely; the controller only knows about this interface.
public interface IPlayerInput
{
    float MoveInput { get; }      // horizontal, -1..1
    float VerticalInput { get; }  // vertical, -1..1 (climb up/down)
    bool JumpHeld { get; }        // held (variable jump height)
    bool JumpPressed { get; }     // pressed this frame (not while holding down)
    bool DropPressed { get; }     // pressed this frame WHILE holding down (drop-through)
    bool GrabHeld { get; }        // held to grip a climb surface
    bool WarpPressed { get; }     // pressed this frame (blink)
    bool InteractPressed { get; }
}
