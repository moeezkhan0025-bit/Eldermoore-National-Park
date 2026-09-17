using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

// The Spellbook deck view: 15 slots showing the player's active deck. Implements
// IMenuPanel so MenuInputController drives navigation (down to enter, d-pad to move,
// Submit to select a slot). Selecting a slot will (next step) open the picker /
// replace-confirm. For now, selecting logs the slot so you can verify navigation.
public class SpellbookPanelUI : MonoBehaviour, IMenuPanel
{
    [Header("Deck slots (place 15, or spawn them)")]
    [SerializeField] private SpellSlotUI[] slots;      // 15 slot widgets
    [SerializeField] private int columns = 5;          // grid columns for navigation

    [Header("Highlight")]
    [SerializeField] private RectTransform highlightFrame;

    [Header("Info (optional)")]
    [SerializeField] private TMP_Text infoNameText;
    [SerializeField] private TMP_Text infoDescriptionText;
    [SerializeField] private TMP_Text deckCountText;    // e.g. "3 / 15"

    private SpellcasterController caster;
    private SpellCollection collection;
    private int highlightIndex;

    void OnEnable()
    {
        Resolve();
        if (caster != null) caster.DeckChanged += RefreshDeck;
        RefreshDeck();
        if (highlightFrame != null) highlightFrame.gameObject.SetActive(false);
    }

    void OnDisable()
    {
        if (caster != null) caster.DeckChanged -= RefreshDeck;
    }

    void Resolve()
    {
        var player = PlayerPersistence.Instance;
        if (player != null)
        {
            caster = player.GetComponent<SpellcasterController>();
            collection = player.GetComponent<SpellCollection>();
        }
        if (caster == null)
            Debug.LogWarning("[Spellbook] Player missing SpellcasterController.", this);
    }

    // Fill the 15 slots from the deck (loadout); empties show blank.
    void RefreshDeck()
    {
        if (slots == null) return;
        var deck = caster != null ? caster.Loadout : null;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;
            SpellDefinition spell = (deck != null && i < deck.Count) ? deck[i] : null;
            slots[i].Show(spell);
        }

        if (deckCountText != null && caster != null)
            deckCountText.text = $"{caster.DeckCount} / {SpellcasterController.MaxDeckSize}";
    }

    // ---------------- IMenuPanel ----------------

    public bool EnterPanel()
    {
        if (slots == null || slots.Length == 0) return false;
        highlightIndex = 0;
        MoveHighlightTo(0);
        if (highlightFrame != null) highlightFrame.gameObject.SetActive(true);
        return true;
    }

    public bool Move(Vector2Int dir)
    {
        if (slots == null || slots.Length == 0) return false;

        int row = highlightIndex / columns;
        int col = highlightIndex % columns;

        if (dir.x > 0) col++;
        else if (dir.x < 0) col--;
        else if (dir.y > 0) row--;   // up
        else if (dir.y < 0) row++;   // down

        if (row < 0) return false;   // out the top -> back to tabs

        int target = row * columns + col;
        if (col < 0 || col >= columns) return true;
        if (target < 0 || target >= slots.Length) return true;

        highlightIndex = target;
        MoveHighlightTo(highlightIndex);
        return true;
    }

    public void Activate()
    {
        if (highlightIndex < 0 || highlightIndex >= slots.Length) return;
        // NEXT STEP: open picker (empty slot) or replace-confirm (filled slot).
        var spell = slots[highlightIndex].Spell;
        Debug.Log($"[Spellbook] Selected slot {highlightIndex}, spell = {(spell != null ? spell.displayName : "EMPTY")}");
    }

    public void ExitPanel()
    {
        if (highlightFrame != null) highlightFrame.gameObject.SetActive(false);
    }

    // ---------------- highlight + info ----------------

    void MoveHighlightTo(int index)
    {
        if (index < 0 || index >= slots.Length || slots[index] == null) return;
        var slotRect = slots[index].GetComponent<RectTransform>();
        if (highlightFrame != null && slotRect != null)
        {
            highlightFrame.gameObject.SetActive(true);
            highlightFrame.position = slotRect.position;
            highlightFrame.sizeDelta = slotRect.rect.size;
        }
        ShowInfo(slots[index].Spell);
    }

    void ShowInfo(SpellDefinition spell)
    {
        if (infoNameText != null)
            infoNameText.text = spell != null ? spell.displayName : "";
        if (infoDescriptionText != null)
            infoDescriptionText.text = spell != null ? spell.description : "";
    }
}
