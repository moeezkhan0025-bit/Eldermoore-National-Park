using System;
using UnityEngine;

// A temporary damage-absorbing shield on the player. Soaks incoming damage up to
// its shield HP before real health is touched. Ends when depleted OR when its
// duration expires (whichever first). Put on the player alongside PlayerHealth.
public class PlayerShield : MonoBehaviour
{
    [SerializeField] private GameObject shieldVisual;   // optional bubble sprite
    [SerializeField] private float duration = 4f;       // seconds before it expires (0 = no timer)

    private float shieldHP;
    private float shieldMax;
    private float expireAt;
    private MovementController move;
    private SpriteRenderer shieldSr;

    public bool IsActive => shieldHP > 0f;
    public float Current => shieldHP;
    public float Max => shieldMax;

    public event Action<float, float> ShieldChanged;   // (current, max)
    public event Action ShieldBroke;

    void Awake()
    {
        if (shieldVisual != null) { shieldVisual.SetActive(false); shieldSr = shieldVisual.GetComponentInChildren<SpriteRenderer>(); }
        move = GetComponent<MovementController>();
    }

    // Cast the shield: set/refresh it to the given amount and (re)start the timer.
    public void Activate(float amount)
    {
        shieldHP = amount;
        shieldMax = amount;
        expireAt = (duration > 0f) ? Time.time + duration : float.MaxValue;
        if (shieldVisual != null) shieldVisual.SetActive(true);
        ShieldChanged?.Invoke(shieldHP, shieldMax);
    }

    // Returns leftover damage the shield couldn't absorb (0 if fully absorbed).
    public float Absorb(float damage)
    {
        if (shieldHP <= 0f) return damage;

        if (damage <= shieldHP)
        {
            shieldHP -= damage;
            ShieldChanged?.Invoke(shieldHP, shieldMax);
            if (shieldHP <= 0f) Break();
            return 0f;
        }
        else
        {
            float leftover = damage - shieldHP;
            shieldHP = 0f;
            Break();
            return leftover;
        }
    }

    void Break()
    {
        shieldHP = 0f;
        if (shieldVisual != null) shieldVisual.SetActive(false);
        ShieldChanged?.Invoke(0f, shieldMax);
        ShieldBroke?.Invoke();
    }

    void Update()
    {
        // Expire after its duration, even if not fully depleted.
        if (IsActive && Time.time >= expireAt) Break();

        // Mirror the shield visual to the player's facing while active.
        if (IsActive && shieldSr != null && move != null)
            shieldSr.flipX = !move.FacingRight;
    }
}