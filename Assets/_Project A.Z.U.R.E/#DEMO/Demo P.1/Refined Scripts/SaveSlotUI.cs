using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One save slot in the load/overwrite panel. Make this a prefab and place three of them.
// (Swap Text -> TMP_Text / using TMPro if you use TextMeshPro.)
public class SaveSlotUI : MonoBehaviour
{
    [SerializeField] private TMP_Text slotLabel;         // e.g. "Slot 1"
    [SerializeField] private TMP_Text characterText;     // character name
    [SerializeField] private TMP_Text detailText;        // playtime + last saved
    [SerializeField] private GameObject emptyState;  // shown when the slot is empty
    [SerializeField] private GameObject filledState; // shown when the slot has a save
    [SerializeField] private Button slotButton;      // the whole slot is clickable
    [SerializeField] private Button deleteButton;    // optional; hidden on empty slots

    private int slotIndex;
    private Action<int> onSelected;
    private Action<int> onDelete;

    public void Bind(int index, SaveData data, Action<int> onSelected, Action<int> onDelete)
    {
        slotIndex = index;
        this.onSelected = onSelected;
        this.onDelete = onDelete;

        if (slotLabel != null) slotLabel.text = $"Slot {index + 1}";

        bool empty = data == null;
        if (emptyState != null)  emptyState.SetActive(empty);
        if (filledState != null) filledState.SetActive(!empty);

        if (!empty)
        {
            if (characterText != null) characterText.text = data.characterName;
            if (detailText != null)
                detailText.text = $"{FormatPlaytime(data.playtimeSeconds)}  \u2022  {FormatDate(data.lastSavedUnixTime)}";
        }

        slotButton.onClick.RemoveAllListeners();
        slotButton.onClick.AddListener(() => this.onSelected?.Invoke(slotIndex));

        if (deleteButton != null)
        {
            deleteButton.gameObject.SetActive(!empty);
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(() => this.onDelete?.Invoke(slotIndex));
        }
    }

    private static string FormatPlaytime(float seconds)
    {
        var t = TimeSpan.FromSeconds(seconds);
        return $"{(int)t.TotalHours:D2}:{t.Minutes:D2}";
    }

    private static string FormatDate(long unixTime)
    {
        if (unixTime <= 0) return "";
        return DateTimeOffset.FromUnixTimeSeconds(unixTime).LocalDateTime.ToString("g");
    }
}
