using UnityEngine;
using UnityEngine.UI;

// Minimal character select with visual selection highlighting.
// Lives in the CharacterSelect scene.
public class CharacterSelectController : MonoBehaviour
{
    [System.Serializable]
    public class CharacterOption
    {
        public string className;
        public int classId;
        public GameObject highlight;   // the "selected" indicator for THIS option
        // public Sprite portrait;     // etc.
    }

    [SerializeField] private CharacterOption[] options;
    [SerializeField] private InputField nameInput;   // player-entered name (optional)
    [SerializeField] private Button confirmButton;

    private int selectedIndex = -1;

    private void Start()
    {
        confirmButton.onClick.AddListener(OnConfirm);
        confirmButton.interactable = false;

        // Make sure every highlight starts hidden.
        for (int i = 0; i < options.Length; i++)
            if (options[i].highlight != null)
                options[i].highlight.SetActive(false);
    }

    // Hook each character button's OnClick to this, passing its index (0,1,2...).
    public void SelectOption(int index)
    {
        selectedIndex = index;

        // Show the chosen highlight, hide the rest.
        for (int i = 0; i < options.Length; i++)
            if (options[i].highlight != null)
                options[i].highlight.SetActive(i == index);

        confirmButton.interactable = true;
    }

    private void OnConfirm()
    {
        if (selectedIndex < 0) return;

        string chosenName =
            (nameInput != null && !string.IsNullOrWhiteSpace(nameInput.text))
                ? nameInput.text.Trim()
                : options[selectedIndex].className;

        GameManager.Instance.ConfirmCharacter(chosenName, options[selectedIndex].classId);
    }
}