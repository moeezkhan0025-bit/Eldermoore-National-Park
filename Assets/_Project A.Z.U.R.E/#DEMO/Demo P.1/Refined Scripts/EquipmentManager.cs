using System;
using System.Collections.Generic;
using UnityEngine;

// Manages what the player has equipped (one item per EquipmentSlot) and applies
// each piece's stat modifiers to PlayerStats while it's worn. Put this on the
// persistent player alongside PlayerStats.
[RequireComponent(typeof(PlayerStats))]
public class EquipmentManager : MonoBehaviour
{
    private PlayerStats stats;

    private readonly Dictionary<EquipmentSlot, EquipmentDefinition> equipped
        = new Dictionary<EquipmentSlot, EquipmentDefinition>();

    // Fired when a slot changes, so the equipment UI can refresh. (slot, itemOrNull)
    public event Action<EquipmentSlot, EquipmentDefinition> EquipmentChanged;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    public EquipmentDefinition GetEquipped(EquipmentSlot slot)
        => equipped.TryGetValue(slot, out var item) ? item : null;

    // Equip a piece of gear. Returns whatever was previously in that slot (or null).
    public EquipmentDefinition Equip(EquipmentDefinition gear)
    {
        if (gear == null) return null;

        EquipmentDefinition previous = GetEquipped(gear.slot);
        if (previous != null) RemoveModifiers(previous);

        equipped[gear.slot] = gear;
        ApplyModifiers(gear);

        EquipmentChanged?.Invoke(gear.slot, gear);
        return previous;
    }

    // Unequip whatever's in a slot. Returns the removed item (or null).
    public EquipmentDefinition Unequip(EquipmentSlot slot)
    {
        EquipmentDefinition current = GetEquipped(slot);
        if (current == null) return null;

        RemoveModifiers(current);
        equipped.Remove(slot);

        EquipmentChanged?.Invoke(slot, null);
        return current;
    }

    private void ApplyModifiers(EquipmentDefinition gear)
    {
        foreach (var mod in gear.modifiers)
            stats.AddPermanent(mod.stat, mod.value, gear);
    }

    private void RemoveModifiers(EquipmentDefinition gear)
    {
        stats.RemoveFromSource(gear);
    }

    public IEnumerable<KeyValuePair<EquipmentSlot, EquipmentDefinition>> AllEquipped => equipped;
}
