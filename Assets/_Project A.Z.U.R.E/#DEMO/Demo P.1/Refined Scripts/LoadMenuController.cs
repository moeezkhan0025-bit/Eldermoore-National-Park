using UnityEngine;
using UnityEngine.UI;

// Put this on your LoadPanel. Handles both loading an existing save
// and (when all slots are full) picking a slot to start a new game in.
public class LoadMenuController : MonoBehaviour
{
    public enum Mode { Load, NewGame }

    [SerializeField] private SaveSlotUI[] slots;   // size 3, assign in inspector
    [SerializeField] private Text titleText;
    [SerializeField] private Button backButton;
    [SerializeField] private MainMenuController mainMenu;

    private Mode mode = Mode.Load;

    private void Awake()
    {
        if (backButton != null)
            backButton.onClick.AddListener(() => mainMenu.ShowMainPanel());
    }

    public void SetMode(Mode m)
    {
        mode = m;
        if (titleText != null)
            titleText.text = mode == Mode.Load ? "Load Game" : "New Game \u2014 Choose a Slot";
        Refresh();
    }

    private void Refresh()
    {
        SaveData[] data = SaveSystem.LoadAllSlots();   // null entries = empty slots
        for (int i = 0; i < slots.Length; i++)
            slots[i].Bind(i, data[i], OnSlotSelected, OnSlotDelete);
    }

    private void OnSlotSelected(int slot)
    {
        if (mode == Mode.Load)
        {
            if (SaveSystem.SlotExists(slot))
                GameManager.Instance.LoadGame(slot);
        }
        else // NewGame / overwrite
        {
            // If SlotExists(slot), consider popping a "Overwrite this save?" confirm here.
            GameManager.Instance.StartNewGame(slot);
        }
    }

    private void OnSlotDelete(int slot)
    {
        SaveSystem.Delete(slot);
        Refresh();
    }
}
