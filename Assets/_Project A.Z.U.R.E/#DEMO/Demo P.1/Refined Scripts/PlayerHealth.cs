using System;
using UnityEngine;

// Current HP for the player. Max HP comes from PlayerStats (StatType.MaxHealth),
// so gear/potions that boost MaxHealth raise the ceiling automatically.
// Hearts are just a display of HP: heartValue HP per heart.
[RequireComponent(typeof(PlayerStats))]
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float heartValue = 4f;   // HP per heart shown in UI
    [SerializeField] private bool startFull = true;

    private PlayerStats stats;
    public float Current { get; private set; }
    public float Max => stats.Get(StatType.MaxHealth);

    // UI/game hooks.
    public event Action<float, float> HealthChanged; // (current, max)
    public event Action Died;

    // Hearts read-outs for the HUD.
    public float CurrentHearts => Current / heartValue;
    public int   MaxHearts     => Mathf.CeilToInt(Max / heartValue);

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    void Start()
    {
        if (startFull) Current = Max;
        // If MaxHealth changes (gear/potion), keep current within the new ceiling.
        stats.StatsChanged += ClampToMax;
        HealthChanged?.Invoke(Current, Max);
    }

    void OnDestroy()
    {
        if (stats != null) stats.StatsChanged -= ClampToMax;
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f || Current <= 0f) return;
        Current = Mathf.Max(0f, Current - amount);
        HealthChanged?.Invoke(Current, Max);
        if (Current <= 0f) Died?.Invoke();
    }

    // Heal by an amount (from a potion, etc.), never above Max.
    public void Heal(float amount)
    {
        if (amount <= 0f) return;
        Current = Mathf.Min(Max, Current + amount);
        HealthChanged?.Invoke(Current, Max);
    }

    public void SetCurrent(float value)   // for load / respawn
    {
        Current = Mathf.Clamp(value, 0f, Max);
        HealthChanged?.Invoke(Current, Max);
    }

    private void ClampToMax()
    {
        if (Current > Max) Current = Max;
        HealthChanged?.Invoke(Current, Max);
    }
}
