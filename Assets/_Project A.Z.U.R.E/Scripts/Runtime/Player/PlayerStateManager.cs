using UnityEngine;

public enum PlayerState
{
    Idle,
    Running,
    Jumping,
    Falling,
    Landing,
    DoubleJumping,
    // Dashing,       // Phase 2
    // WallSliding,   // Phase 2
    // Attacking,     // Phase 3


}

public class PlayerStateManager : MonoBehaviour
{
    public PlayerState CurrentState { get; private set; } = PlayerState.Idle;
    public PlayerState PreviousState { get; private set; }

    public event System.Action<PlayerState, PlayerState> OnStateChanged;

    public void ChangeState(PlayerState newState)
    {
        if (newState == CurrentState) return;
        PreviousState = CurrentState;
        CurrentState = newState;
        Debug.Log($"[PSM] Firing OnStateChanged: {PreviousState} → {CurrentState} · Subscribers: {OnStateChanged?.GetInvocationList().Length ?? 0}");
        OnStateChanged?.Invoke(PreviousState, CurrentState);
        Debug.Log($"[State] {PreviousState} → {CurrentState}");
    }

    public bool IsGrounded =>
        CurrentState == PlayerState.Idle ||
        CurrentState == PlayerState.Running ||
        CurrentState == PlayerState.Landing;

    public bool IsAirborne =>
        CurrentState == PlayerState.Jumping ||
        CurrentState == PlayerState.Falling;
}