using UnityEngine;
using UnityEngine.UI;

// One paper-doll equipment slot, tagged with which EquipmentSlot it represents.
// Place one per slot on the character diagram; set its Slot in the Inspector.
// Shows the equipped item's icon; clicking it unequips.
public class EquipSlotUI : MonoBehaviour
{
    [SerializeField] private EquipmentSlot slot;
    [SerializeField] private Image iconImage;      // where the equipped icon shows
    [SerializeField] private Sprite emptySprite;   // optional placeholder when empty

    private EquipmentPanelUI panel;
    public EquipmentSlot Slot => slot;

    public void Init(EquipmentPanelUI owner) { panel = owner; }

    public void Show(EquipmentDefinition gear)
    {
        if (iconImage == null) return;
        if (gear != null) { iconImage.sprite = gear.icon; iconImage.enabled = true; }
        else { iconImage.sprite = emptySprite; iconImage.enabled = emptySprite != null; }
    }

    // Wire the slot's Button OnClick to this.
    public void OnClicked()
    {
        if (panel != null) panel.TryUnequip(slot);
    }
}
