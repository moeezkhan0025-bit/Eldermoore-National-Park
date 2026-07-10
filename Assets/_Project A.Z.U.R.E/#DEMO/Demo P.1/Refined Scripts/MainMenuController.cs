using UnityEngine;
using UnityEngine.UI;

// Put this on your Canvas (or a "MenuRoot" object) in the MainMenu scene.
// Swap Text -> TMP_Text and "using UnityEngine.UI" additions accordingly if you use TextMeshPro.
public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject loadPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Buttons")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("References")]
    [SerializeField] private LoadMenuController loadMenu;

    private void Start()
    {
        // Continue only makes sense if a save exists.
        continueButton.interactable = SaveSystem.HasAnySave();

        newGameButton.onClick.AddListener(OnNewGame);
        continueButton.onClick.AddListener(OnContinue);
        loadButton.onClick.AddListener(OpenLoad);
        settingsButton.onClick.AddListener(OpenSettings);
        quitButton.onClick.AddListener(OnQuit);

        ShowMainPanel();
    }

    private void OnNewGame()
    {
        int slot = SaveSystem.GetFirstEmptySlot();
        if (slot >= 0)
        {
            // Normal case: an empty slot exists -> straight to character select.
            GameManager.Instance.StartNewGame(slot);
        }
        else
        {
            // All three slots full: let the player pick one to overwrite.
            mainPanel.SetActive(false);
            settingsPanel.SetActive(false);
            loadPanel.SetActive(true);
            loadMenu.SetMode(LoadMenuController.Mode.NewGame);
        }
    }

    private void OnContinue() => GameManager.Instance.Continue();

    private void OpenLoad()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(false);
        loadPanel.SetActive(true);
        loadMenu.SetMode(LoadMenuController.Mode.Load);
    }

    private void OpenSettings()
    {
        mainPanel.SetActive(false);
        loadPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    // Public so the Load/Settings "Back" buttons can return here.
    public void ShowMainPanel()
    {
        mainPanel.SetActive(true);
        loadPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    private void OnQuit() => GameManager.Instance.QuitGame();
}
