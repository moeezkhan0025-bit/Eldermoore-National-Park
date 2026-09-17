using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Equipment panel with fully custom navigation (no EventSystem). Implements
// IMenuPanel so MenuInputController drives it. A highlight frame moves over the
// grid cells; the info box updates to the highlighted item; Activate() equips.
public class EquipmentPanelUI : MonoBehaviour, IMenuPanel
{
    [Header("Inventory grid (right side)")]
    [SerializeField] private Transform inventoryGridParent;
    [SerializeField] private InventorySlotUI inventoryCellPrefab;
    [SerializeField] private int gridColumns = 4;

    [Header("Highlight")]
    [SerializeField] private RectTransform highlightFrame;   // an Image that marks the current cell

    [Header("Paper-doll slots (left side)")]
    [SerializeField] private EquipSlotUI[] equipSlots;

    [Header("Item info box")]
    [SerializeField] private TMP_Text infoNameText;
    [SerializeField] private TMP_Text infoDescriptionText;
    [SerializeField] private TMP_Text infoStatsText;

    [Header("Confirmation")]
    [SerializeField] private ConfirmPopup confirmPopup;

    private Inventory inventory;
    private EquipmentManager equipment;
    private readonly List<InventorySlotUI> cells = new List<InventorySlotUI>();
    private int highlightIndex = -1;

    void OnEnable()
    {
        Resolve();
        if (inventory != null) inventory.InventoryChanged += RefreshInventory;
        if (equipment != null) equipment.EquipmentChanged += OnEquipmentChanged;
        RefreshAll();
        if (highlightFrame != null) highlightFrame.gameObject.SetActive(false);
    }

    void OnDisable()
    {
        if (inventory != null) inventory.InventoryChanged -= RefreshInventory;
        if (equipment != null) equipment.EquipmentChanged -= OnEquipmentChanged;
    }

    void Resolve()
    {
        var player = PlayerPersistence.Instance;
        if (player != null)
        {
            inventory = player.GetComponent<Inventory>();
            equipment = player.GetComponent<EquipmentManager>();
        }
        if (equipSlots != null)
            foreach (var es in equipSlots) if (es != null) es.Init(this);
    }

    void RefreshAll() { RefreshInventory(); RefreshEquipSlots(); }

    void RefreshInventory()
    {
        if (inventory == null || inventoryCellPrefab == null || inventoryGridParent == null) return;

        foreach (var c in cells) if (c != null) Destroy(c.gameObject);
        cells.Clear();

        foreach (var entry in inventory.Entries)
        {
            var cell = Instantiate(inventoryCellPrefab, inventoryGridParent);
            cell.Bind(entry.item, this);
            cells.Add(cell);
        }

        // keep highlight valid after a refresh
        if (highlightIndex >= cells.Count) highlightIndex = cells.Count - 1;
        if (highlightIndex >= 0) MoveHighlightTo(highlightIndex);
    }

    void RefreshEquipSlots()
    {
        if (equipment == null || equipSlots == null) return;
        foreach (var es in equipSlots)
            if (es != null) es.Show(equipment.GetEquipped(es.Slot));
    }

    void OnEquipmentChanged(EquipmentSlot slot, EquipmentDefinition item) => RefreshEquipSlots();

    // ---------------- IMenuPanel ----------------

    public bool EnterPanel()
    {
        if (cells.Count == 0) return false;
        highlightIndex = 0;
        MoveHighlightTo(0);
        if (highlightFrame != null) highlightFrame.gameObject.SetActive(true);
        return true;
    }

    public bool Move(Vector2Int dir)
    {
        if (cells.Count == 0) return false;

        int row = highlightIndex / gridColumns;
        int col = highlightIndex % gridColumns;

        if (dir.x > 0) col++;
        else if (dir.x < 0) col--;
        else if (dir.y > 0) row--;   // up
        else if (dir.y < 0) row++;   // down

        // moving above the top row -> exit to tabs
        if (row < 0) return false;

        int target = row * gridColumns + col;

        // stay within valid cells / columns
        if (col < 0 || col >= gridColumns) return true;         // ignore off-side
        if (target < 0 || target >= cells.Count) return true;   // ignore empty spot

        highlightIndex = target;
        MoveHighlightTo(highlightIndex);
        return true;
    }

    public void Activate()
    {
        if (highlightIndex < 0 || highlightIndex >= cells.Count) return;
        var item = cells[highlightIndex].Item;
        TryEquip(item);
    }

    public void ExitPanel()
    {
        if (highlightFrame != null) highlightFrame.gameObject.SetActive(false);
    }

    // ---------------- highlight + info ----------------

    void MoveHighlightTo(int index)
    {
        if (index < 0 || index >= cells.Count) return;
        var cellRect = cells[index].GetComponent<RectTransform>();
        if (highlightFrame != null && cellRect != null)
        {
            highlightFrame.gameObject.SetActive(true);

            // Position the frame ON the cell WITHOUT parenting under the grid
            // (the Grid Layout Group would otherwise re-layout the frame).
            highlightFrame.position = cellRect.position;    // world-space center of the cell
            highlightFrame.sizeDelta = cellRect.rect.size;  // match the cell's size
        }
        ShowItemInfo(cells[index].Item);
    }

    public void ShowItemInfo(ItemDefinition item)
    {
        if (item == null)
        {
            if (infoNameText) infoNameText.text = "";
            if (infoDescriptionText) infoDescriptionText.text = "";
            if (infoStatsText) infoStatsText.text = "";
            return;
        }
        if (infoNameText) infoNameText.text = string.IsNullOrEmpty(item.displayName) ? item.name : item.displayName;
        if (infoDescriptionText) infoDescriptionText.text = item.description;
        if (infoStatsText)
        {
            var sb = new StringBuilder();
            if (item is EquipmentDefinition gear)
                foreach (var mod in gear.modifiers)
                    sb.AppendLine($"{mod.stat}: {(mod.value >= 0 ? "+" : "")}{mod.value:0.##}");
            infoStatsText.text = sb.ToString();
        }
    }

    // ---------------- equip logic ----------------

    public void TryEquip(ItemDefinition item)
    {
        if (equipment == null || inventory == null) return;
        if (item is not EquipmentDefinition gear) return;

        var occupying = equipment.GetEquipped(gear.slot);
        string itemName = string.IsNullOrEmpty(gear.displayName) ? gear.name : gear.displayName;
        string message = occupying != null
            ? $"Replace {(string.IsNullOrEmpty(occupying.displayName) ? occupying.name : occupying.displayName)} with {itemName}?"
            : $"Equip {itemName}?";

        if (confirmPopup != null) confirmPopup.Ask(message, () => DoEquip(gear));
        else DoEquip(gear);
    }

    private void DoEquip(EquipmentDefinition gear)
    {
        var previous = equipment.Equip(gear);
        inventory.Remove(gear, 1);
        if (previous != null) inventory.Add(previous, 1);
    }

    public void TryUnequip(EquipmentSlot slot)
    {
        if (equipment == null || inventory == null) return;
        var removed = equipment.Unequip(slot);
        if (removed != null) inventory.Add(removed, 1);
    }
}