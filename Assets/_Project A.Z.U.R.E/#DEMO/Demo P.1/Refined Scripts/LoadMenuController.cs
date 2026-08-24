using UnityEngine;

// Drives the LoadPanel using your SaveSlotUI slots.
// Two modes, matching how MainMenuController calls SetMode():
//   Load    -> picking a used slot loads that game
//   NewGame -> picking a slot starts a new game there (overwrites on first save)
public class LoadMenuController : MonoBehaviour
{
    public enum Mode { Load, NewGame }

    [SerializeField] private SaveSlotUI[] slots; // assign your 3 SaveSlotUI objects, in order

    private Mode currentMode = Mode.Load;

    private void OnEnable() => Refresh();

    // Called by MainMenuController before showing the panel.
    public void SetMode(Mode mode)
    {
        currentMode = mode;
        Refresh();
    }

    // Re-read every slot from disk and rebuild its UI.
    public void Refresh()
    {
        if (slots == null) return;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;
            SaveData data = SaveSystem.Load(i);
            // Pass our own methods as the click / delete callbacks SaveSlotUI expects.
            slots[i].Bind(i, data, OnSlotSelected, OnSlotDelete);
        }
    }

    // Fired when the player clicks a slot (SaveSlotUI's onSelected callback).
    private void OnSlotSelected(int slot)
    {
        SaveData data = SaveSystem.Load(slot);

        if (currentMode == Mode.Load)
        {
            if (data == null) return;                 // empty slot in Load mode: ignore
            GameManager.Instance.LoadGame(slot);
        }
        else // NewGame: start fresh here (the old save is overwritten on first save)
        {
            GameManager.Instance.StartNewGame(slot);
        }
    }

    // Fired when the player clicks a slot's delete button (SaveSlotUI's onDelete callback).
    private void OnSlotDelete(int slot)
    {
        SaveSystem.Delete(slot);
        Refresh(); // update the panel so the slot now reads "Empty"
    }
}