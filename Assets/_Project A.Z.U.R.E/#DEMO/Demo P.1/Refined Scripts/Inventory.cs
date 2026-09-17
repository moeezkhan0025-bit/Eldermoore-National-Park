using System;
using System.Collections.Generic;
using UnityEngine;

// The player's bag. Holds items with counts. Put on the persistent player,
// alongside PlayerStats / EquipmentManager. Pickups call Add(); equipping pulls from here.
public class Inventory : MonoBehaviour
{
    [Serializable]
    public class Entry
    {
        public ItemDefinition item;
        public int count;
    }

    private readonly List<Entry> entries = new List<Entry>();
    public IReadOnlyList<Entry> Entries => entries;

    public event Action InventoryChanged;

    public void Add(ItemDefinition item, int amount = 1)
    {
        if (item == null || amount <= 0) return;
        var e = Find(item);
        if (e != null) e.count += amount;
        else entries.Add(new Entry { item = item, count = amount });
        InventoryChanged?.Invoke();
    }

    public bool Remove(ItemDefinition item, int amount = 1)
    {
        var e = Find(item);
        if (e == null || e.count < amount) return false;
        e.count -= amount;
        if (e.count <= 0) entries.Remove(e);
        InventoryChanged?.Invoke();
        return true;
    }

    public bool Has(ItemDefinition item) => Find(item) != null;

    Entry Find(ItemDefinition item)
    {
        for (int i = 0; i < entries.Count; i++)
            if (entries[i].item == item) return entries[i];
        return null;
    }
}
