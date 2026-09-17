// The input contract the MovementController, PlayerInteractor, etc. read. Any
// handler implementing this can drive the player — legacy keyboard or new-Input-
// System controller. Both handlers must implement EVERY member here.
public interface IPlayerInput
{
    float MoveInput { get; }      // horizontal, -1..1
    float VerticalInput { get; }  // vertical, -1..1 (climb up/down)
    bool JumpHeld { get; }        // held (variable jump height)
    bool JumpPressed { get; }     // pressed this frame (not while holding down)
    bool DropPressed { get; }     // pressed this frame WHILE holding down (drop-through)
    bool GrabHeld { get; }        // held to grip a climb surface
    bool GrabPressed { get; }     // pressed this frame to latch onto a climb surface
    bool WarpPressed { get; }     // pressed this frame (blink)
    bool InteractPressed { get; } // pressed this frame (Triangle — interact with doors/NPCs/items)
}