using System;
using System.Collections.Generic;
using UnityEngine;

// One active modifier on the player: a stat change from some source, optionally timed.
// - Permanent (gear): remainingTime < 0  -> never expires; removed on unequip.
// - Timed (potion/debuff): remainingTime > 0 -> ticks down, auto-removed at 0.
public class ActiveModifier
{
    public StatType stat;
    public float value;
    public object source;        // e.g. the EquipmentDefinition or a potion id, for removal
    public float remainingTime;  // < 0 means permanent

    public bool IsTimed => remainingTime >= 0f;
}

// Holds the player's base stats and the live modifier stack, and computes the
// current (base + modifiers) value of any stat. Put this on the persistent player.
// Timed effects are runtime-only (they clear on save/load by design).
public class PlayerStats : MonoBehaviour
{
    [Serializable]
    public struct BaseStat
    {
        public StatType stat;
        public float value;
    }

    [Header("Base stats (naked character)")]
    [SerializeField] private List<BaseStat> baseStats = new List<BaseStat>();

    private readonly List<ActiveModifier> modifiers = new List<ActiveModifier>();

    // Fired whenever any current stat changes (equip, potion, expiry) so UI/health can react.
    public event Action StatsChanged;

    // ---- Reading stats ----

    public float GetBase(StatType stat)
    {
        foreach (var b in baseStats) if (b.stat == stat) return b.value;
        return 0f;
    }

    // Current value = base + all active modifiers for that stat.
    public float Get(StatType stat)
    {
        float total = GetBase(stat);
        for (int i = 0; i < modifiers.Count; i++)
            if (modifiers[i].stat == stat) total += modifiers[i].value;
        return total;
    }

    // For MULTIPLIER-style stats (SpeedMult, JumpMult, ClimbSpeedMult): the
    // effective multiplier is 1.0 + the sum of modifiers, so 0 bonus = normal,
    // +0.5 = 150% speed. Use this in MovementController instead of Get().
    public float GetMultiplier(StatType stat)
    {
        return 1f + Get(stat);   // base of that stat should be left at 0
    }

    // ---- Adding / removing modifiers ----

    // Permanent modifier (gear). Pass the source so you can remove it on unequip.
    public void AddPermanent(StatType stat, float value, object source)
    {
        modifiers.Add(new ActiveModifier { stat = stat, value = value, source = source, remainingTime = -1f });
        StatsChanged?.Invoke();
    }

    // Timed modifier (potion buff, or debuff with a negative value).
    public void AddTimed(StatType stat, float value, float seconds, object source = null)
    {
        modifiers.Add(new ActiveModifier { stat = stat, value = value, source = source, remainingTime = seconds });
        StatsChanged?.Invoke();
    }

    // Remove every modifier from a given source (e.g. all mods from one gear piece).
    public void RemoveFromSource(object source)
    {
        int removed = modifiers.RemoveAll(m => Equals(m.source, source));
        if (removed > 0) StatsChanged?.Invoke();
    }

    // ---- Timers ----

    void Update()
    {
        bool changed = false;
        for (int i = modifiers.Count - 1; i >= 0; i--)
        {
            if (!modifiers[i].IsTimed) continue;            // permanent, skip
            modifiers[i].remainingTime -= Time.deltaTime;
            if (modifiers[i].remainingTime <= 0f)
            {
                modifiers.RemoveAt(i);
                changed = true;
            }
        }
        if (changed) StatsChanged?.Invoke();
    }

    // Handy for a buffs/debuffs UI later.
    public IReadOnlyList<ActiveModifier> ActiveModifiers => modifiers;
}