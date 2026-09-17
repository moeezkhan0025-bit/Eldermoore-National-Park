using UnityEngine;

// Card = owned permanently (unlocks). Material = consumed. Gear = equippable.
public enum ItemKind { Card, Material, Gear }

// Base type for anything that can live in the inventory: cards, materials, gear.
// EquipmentDefinition inherits this, which is why gear can sit in the bag AND
// be equipped. Right-click in the Project:  Create > Casting > Item Definition
[CreateAssetMenu(fileName = "New Item", menuName = "Casting/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Stable unique id used in save files. Set once, don't change.")]
    public string id;
    public string displayName;
    public Sprite icon;
    [TextArea] public string description;

    [Header("Type")]
    [Tooltip("Card = permanent unlock. Material = consumed. Gear = equippable.")]
    public ItemKind kind = ItemKind.Material;
}
