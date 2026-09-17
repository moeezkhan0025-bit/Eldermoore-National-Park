using UnityEngine;

// TEMP: drops starter items into the player's Inventory on Start, so the
// equipment grid has gear to show while testing. Put on the player; drag your
// test gear assets into Start Items. Delete once real pickups exist.
[RequireComponent(typeof(Inventory))]
public class InventorySeeder : MonoBehaviour
{
    [SerializeField] private ItemDefinition[] startItems;

    void Start()
    {
        var inv = GetComponent<Inventory>();
        if (inv == null) return;
        foreach (var item in startItems)
            if (item != null) inv.Add(item, 1);
    }
}
