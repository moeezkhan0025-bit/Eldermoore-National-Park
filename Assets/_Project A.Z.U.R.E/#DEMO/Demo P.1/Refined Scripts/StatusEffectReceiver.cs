using System.Collections.Generic;
using UnityEngine;

// Receives and manages status effects on the player. Enemy attacks call Apply();
// this ticks active effects (poison/burn DoT, stun/slow timers) and expires them.
// Put on the player alongside PlayerHealth / MovementController.
public class StatusEffectReceiver : MonoBehaviour
{
    private class ActiveEffect
    {
        public StatusEffectType type;
        public float endTime;
        public float magnitude;
        public float tickTimer;
    }

    private readonly List<ActiveEffect> active = new List<ActiveEffect>();
    private PlayerHealth health;
    private MovementController move;

    [SerializeField] private float dotTickInterval = 1f;   // DoT ticks per second

    public bool IsStunned { get; private set; }
    public float SlowFactor { get; private set; } = 1f;    // 1 = normal, <1 = slowed

    void Awake()
    {
        health = GetComponent<PlayerHealth>();
        move = GetComponent<MovementController>();
    }

    public void Apply(StatusEffectType type, float duration, float magnitude)
    {
        if (type == StatusEffectType.None) return;

        // Refresh if already applied, else add.
        var existing = active.Find(e => e.type == type);
        if (existing != null)
        {
            existing.endTime = Time.time + duration;
            existing.magnitude = magnitude;
        }
        else
        {
            active.Add(new ActiveEffect { type = type, endTime = Time.time + duration, magnitude = magnitude, tickTimer = 0f });
        }
        Debug.Log($"[Status] Applied {type} for {duration}s (mag {magnitude})");
    }

    void Update()
    {
        IsStunned = false;
        SlowFactor = 1f;

        for (int i = active.Count - 1; i >= 0; i--)
        {
            var e = active[i];

            if (Time.time >= e.endTime) { active.RemoveAt(i); continue; }

            switch (e.type)
            {
                case StatusEffectType.Stun:
                    IsStunned = true;
                    break;

                case StatusEffectType.Frozen:
                    IsStunned = true;                          // frozen can't act
                    SlowFactor = Mathf.Min(SlowFactor, 0.3f);  // and is heavily slowed
                    break;

                case StatusEffectType.Slow:
                    SlowFactor = Mathf.Min(SlowFactor, e.magnitude);   // magnitude = slow multiplier
                    break;

                case StatusEffectType.Poison:
                case StatusEffectType.Burn:
                    e.tickTimer -= Time.deltaTime;
                    if (e.tickTimer <= 0f)
                    {
                        e.tickTimer = dotTickInterval;
                        if (health != null) health.TakeDamage(e.magnitude);
                    }
                    break;
            }
        }
    }

    public bool HasEffect(StatusEffectType type) => active.Exists(e => e.type == type);
}
