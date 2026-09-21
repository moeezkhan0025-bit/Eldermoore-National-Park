using System;
using UnityEngine;

// A temporary damage-absorbing shield on the player. When active, it soaks incoming
// damage up to its shield HP before the player's real health is touched. Put on the
// player alongside PlayerHealth. PlayerHealth.TakeDamage routes damage through here first.
public class PlayerShield : MonoBehaviour
{
    [SerializeField] private GameObject shieldVisual;   // optional bubble sprite, shown while active

    private float shieldHP;
    private float shieldMax;

    public bool IsActive => shieldHP > 0f;
    public float Current => shieldHP;
    public float Max => shieldMax;

    public event Action<float, float> ShieldChanged;   // (current, max)
    public event Action ShieldBroke;

    void Awake()
    {
        if (shieldVisual != null) shieldVisual.SetActive(false);
    }

    // Cast the shield: set/refresh it to the given amount.
    public void Activate(float amount)
    {
        shieldHP = amount;
        shieldMax = amount;
        if (shieldVisual != null) shieldVisual.SetActive(true);
        ShieldChanged?.Invoke(shieldHP, shieldMax);
    }

    // Returns the LEFTOVER damage that the shield couldn't absorb (0 if fully absorbed).
    // PlayerHealth calls this first and only applies the remainder to health.
    public float Absorb(float damage)
    {
        if (shieldHP <= 0f) return damage;        // no shield -> all damage passes through

        if (damage <= shieldHP)
        {
            shieldHP -= damage;
            ShieldChanged?.Invoke(shieldHP, shieldMax);
            if (shieldHP <= 0f) Break();
            return 0f;                            // fully absorbed
        }
        else
        {
            float leftover = damage - shieldHP;   // shield breaks, remainder hits health
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
}
