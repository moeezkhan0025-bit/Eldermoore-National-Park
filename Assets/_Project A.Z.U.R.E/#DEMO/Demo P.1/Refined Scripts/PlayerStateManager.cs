using System;
using UnityEngine;

// The full set of states the MovementController drives. If your animation
// system references these by name, keep the names identical.
public enum PlayerState
{
    Idle,
    Running,
    Jumping,
    DoubleJumping,
    Falling,
    Landing,
    WallSliding,
    WallJumping
}

// Tracks the player's current state and notifies listeners (e.g. an animation
// controller) when it changes.
public class PlayerStateManager : MonoBehaviour
{
    public PlayerState CurrentState { get; private set; } = PlayerState.Idle;

    // Fired whenever the state actually changes. Subscribe from your animator script:
    //   psm.OnStateChanged += HandleAnim;
    public event Action<PlayerState> OnStateChanged;

    // True when the player is off the ground in any airborne state.
    public bool IsAirborne =>
        CurrentState == PlayerState.Jumping ||
        CurrentState == PlayerState.DoubleJumping ||
        CurrentState == PlayerState.Falling ||
        CurrentState == PlayerState.WallSliding ||
        CurrentState == PlayerState.WallJumping;

    public void ChangeState(PlayerState newState)
    {
        if (newState == CurrentState) return;   // ignore redundant transitions
        CurrentState = newState;
        OnStateChanged?.Invoke(newState);
    }
}
