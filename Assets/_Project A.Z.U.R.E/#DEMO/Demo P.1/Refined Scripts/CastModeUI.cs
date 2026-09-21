using System.Collections.Generic;
using UnityEngine;

// Cast-mode HUD: shows ONE spell card (the template) for the selected spell, with
// L1/L2 cycle-arrow prompts beside it. L1/L2 flips which card shows. Driven by
// CastController. Lives on a persistent HUD canvas.
public class CastModeUI : MonoBehaviour
{
    public static CastModeUI Instance { get; private set; }

    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("The card template")]
    [SerializeField] private SpellCardUI card;      // the reusable card that renders the spell

    [Header("Message (e.g. No enemy found)")]
    [SerializeField] private TMPro.TMP_Text messageText;
    [SerializeField] private float messageDuration = 1.5f;
    private float messageHideAt;

    [Header("Cycle prompts (optional)")]
    [SerializeField] private GameObject leftPrompt;  // L1 arrow (shown if more than one spell)
    [SerializeField] private GameObject rightPrompt; // L2 arrow

    private IReadOnlyList<SpellDefinition> deck;
    private int selected;

    void Awake()
    {
        Instance = this;
        if (root == null) root = gameObject;
        root.SetActive(false);
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    public void Show(IReadOnlyList<SpellDefinition> loadout, int selectedIndex)
    {
        deck = loadout;
        selected = selectedIndex;
        root.SetActive(true);
        ShowArrows();
        ShowCard();
    }

    public void Hide() => root.SetActive(false);

    public void SetSelected(int index)
    {
        selected = index;
        ShowCard();      // swap the card to the new spell
    }

    // CastController passes the count of correctly-entered inputs so the card's
    // sequence lights up; a wrong press sends 0 (reset).
    public void ShowSequenceProgress(List<CastInput> entered)
    {
        if (card != null) card.SetProgress(entered != null ? entered.Count : 0);
    }

    void ShowCard()
    {
        if (card == null || deck == null || selected < 0 || selected >= deck.Count) return;
        card.Bind(deck[selected]);
    }

    void ShowArrows()
    {
        bool many = deck != null && deck.Count > 1;
        if (leftPrompt != null) leftPrompt.SetActive(many);
        if (rightPrompt != null) rightPrompt.SetActive(many);
    }

    // Pop a short message (like "No enemy found") on the cast UI.
    public void ShowMessage(string msg)
    {
        if (messageText == null || string.IsNullOrEmpty(msg)) return;
        messageText.text = msg;
        messageText.gameObject.SetActive(true);
        messageHideAt = Time.unscaledTime + messageDuration;
    }

    void Update()
    {
        if (messageText != null && messageText.gameObject.activeSelf && Time.unscaledTime >= messageHideAt)
            messageText.gameObject.SetActive(false);
    }
}