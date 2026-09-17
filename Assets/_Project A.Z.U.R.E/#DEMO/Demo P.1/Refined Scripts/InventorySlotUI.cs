using UnityEngine;
using UnityEngine.UI;

// One inventory grid cell. Shows an item's icon. In the custom-navigation setup,
// the panel reads Item and drives equipping itself (no Button OnClick needed),
// but OnClicked is kept in case you still wire a mouse click.
public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;

    private ItemDefinition item;
    private EquipmentPanelUI panel;

    // Exposed so EquipmentPanelUI can read the highlighted cell's item.
    public ItemDefinition Item => item;

    public void Bind(ItemDefinition boundItem, EquipmentPanelUI owner)
    {
        item = boundItem;
        panel = owner;
        if (iconImage != null)
        {
            iconImage.sprite = item != null ? item.icon : null;
            iconImage.enabled = item != null && item.icon != null;
        }
    }

    // Optional: still works if you wire a Button OnClick to it.
    public void OnClicked()
    {
        if (item != null && panel != null) panel.TryEquip(item);
    }
}