using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Yes/No confirmation popup, navigated by the custom MenuInputController (no
// EventSystem). Uses a highlight FRAME that slides between the No and Yes buttons,
// matching how the inventory grid highlights cells.
public class ConfirmPopup : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text messageText;

    [Header("Choices (order: No = 0, Yes = 1)")]
    [SerializeField] private RectTransform noTarget;    // the No button's RectTransform
    [SerializeField] private RectTransform yesTarget;   // the Yes button's RectTransform
    [SerializeField] private RectTransform highlightFrame;  // frame that moves onto the choice

    [SerializeField] private MenuInputController inputController;

    private Action onYes;
    private int choice;   // 0 = No, 1 = Yes
    public bool IsOpen { get; private set; }

    void Awake()
    {
        if (root == null) root = gameObject;
        root.SetActive(false);
        if (inputController == null) inputController = FindObjectOfType<MenuInputController>();
    }

    public void Ask(string message, Action onYesCallback)
    {
        onYes = onYesCallback;
        if (messageText != null) messageText.text = message;

        choice = 0;                 // default to No (safer)
        root.SetActive(true);
        IsOpen = true;
        MoveHighlight();

        if (inputController != null) inputController.OpenPopup(this);
    }

    // Called by MenuInputController while the popup has focus.
    public void MoveChoice(int dir)
    {
        if (dir > 0) choice = 1;        // right -> Yes
        else if (dir < 0) choice = 0;   // left  -> No
        MoveHighlight();
    }

    public void Confirm()
    {
        var cb = (choice == 1) ? onYes : null;
        Close();
        cb?.Invoke();
    }

    public void Cancel() => Close();

    private void Close()
    {
        IsOpen = false;
        onYes = null;
        root.SetActive(false);
        if (inputController != null) inputController.ClosePopup();
    }

    // Slide the highlight frame onto the current choice's button.
    private void MoveHighlight()
    {
        if (highlightFrame == null) return;
        RectTransform target = (choice == 1) ? yesTarget : noTarget;
        if (target == null) return;

        highlightFrame.gameObject.SetActive(true);
        highlightFrame.position = target.position;      // world-space center of the button
        highlightFrame.sizeDelta = target.rect.size;    // match its size
    }
}