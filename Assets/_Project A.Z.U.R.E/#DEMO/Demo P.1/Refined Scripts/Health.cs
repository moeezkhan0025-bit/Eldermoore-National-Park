using System;
using UnityEngine;

// Health for enemies / destructible targets. Fires events when damaged/died so
// hit feedback (flash, health bar) can react. TakeDamage() is unchanged for
// existing callers (BoltEffect, etc.).
public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 30f;
    private float current;

    public float Current => current;
    public float Max => maxHealth;
    public float Normalized => maxHealth > 0f ? current / maxHealth : 0f;

    // (current, max) fired on any change; and on death.
    public event Action<float, float> HealthChanged;
    public event Action Damaged;     // fired specifically when hit (for the flash)
    public event Action Died;

    void Awake()
    {
        current = maxHealth;
    }

    void Start()
    {
        HealthChanged?.Invoke(current, maxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f || current <= 0f) return;
        current = Mathf.Max(0f, current - amount);

        Damaged?.Invoke();
        HealthChanged?.Invoke(current, maxHealth);
        Debug.Log($"{name} took {amount} → {current}/{maxHealth}");

        if (current <= 0f)
        {
            Died?.Invoke();
            Destroy(gameObject);
        }
    }
}