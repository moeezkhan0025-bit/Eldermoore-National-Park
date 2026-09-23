using UnityEngine;

// Contract for a pluggable enemy movement behavior. EnemyController owns detection
// and state, then tells the attached movement what to do each state. Swap the
// component to change how an enemy moves (patrol-chase, stationary, flying, etc.).
public interface IEnemyMovement
{
    // Called every FixedUpdate with the enemy's current state + player position.
    void Tick(EnemyState state, Transform player);
}

public enum EnemyState
{
    Patrolling,   // no player detected — do the idle/patrol behavior
    Chasing,      // player in range — pursue
    Returning     // lost the player — go back to patrol
}