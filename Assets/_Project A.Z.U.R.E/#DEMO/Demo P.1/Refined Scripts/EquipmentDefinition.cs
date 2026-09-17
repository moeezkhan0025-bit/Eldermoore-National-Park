using System.Collections.Generic;
using UnityEngine;

// Your equipment slots. Add more later (e.g. Arm1, Arm2) — a new entry here plus
// a matching EquipSlotUI widget on the paper-doll is all it takes.
public enum EquipmentSlot
{
    Head,
    Body,
    Legs,
    Weapon1,
    Weapon2,
    Accessory1,
    Accessory2,
    Ring,
    Amulet
}

// Gear IS an item (inheriting ItemDefinition) plus a slot and stat modifiers.
[CreateAssetMenu(fileName = "New Gear", menuName = "Casting/Equipment Definition")]
public class EquipmentDefinition : ItemDefinition
{
    [Header("Equipment")]
    public EquipmentSlot slot = EquipmentSlot.Weapon1;
    public List<StatModifier> modifiers = new List<StatModifier>();

    void Reset() { kind = ItemKind.Gear; }
}
