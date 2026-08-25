using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Drives the pause menu shell: open/close, time freeze, and tab switching.
// Put this on MenuRoot. Front-end only — panels can be empty for now.
//
// Reads from your generated GameController wrapper:
//   Player.Menu   -> open / close (bind to Start/Options)
//   UI.Cancel     -> close while open (Circle)
//   UI.Navigate   -> cycle tabs left/right on the tab bar
public class MenuController : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject menuRoot;   // toggled on/off (defaults to this object)

    [Header("Tabs — panels and their buttons, same order")]
    [SerializeField] private GameObject[] panels;    // Journal, Map, Equipment, Spellbook
    [SerializeField] private Button[] tabButtons;    // Tab_Journal, Tab_Map, ... (same order)

    [Header("Selection")]
    [SerializeField] private GameObject firstSelected;

    private GameController controls;
    private int currentTab;
    private bool isOpen;
    private bool navLatched;   // so one stick push = one tab step

    void Awake()
    {
        controls = new GameController();
        if (menuRoot == null) menuRoot = gameObject;
        menuRoot.SetActive(false);
    }

    // Menu lives in the Player map; Cancel/Navigate in the UI map — enable both.
    void OnEnable() { controls?.Player.Enable(); controls?.UI.Enable(); }
    void OnDisable() { controls?.Player.Disable(); controls?.UI.Disable(); }
    void OnDestroy() { controls?.Dispose(); }

    void Update()
    {
        // Start/Options toggles the menu open and closed.
        if (controls.Player.Menu.WasPressedThisFrame())
        {
            if (isOpen) Close(); else Open();
            return;
        }

        if (!isOpen) return;

        // Circle backs out while open.
        if (controls.UI.Cancel.WasPressedThisFrame()) { Close(); return; }

        // Push Navigate left/right to cycle tabs (latched so it steps once per push).
        float x = controls.UI.Navigate.ReadValue<Vector2>().x;
        if (Mathf.Abs(x) < 0.5f) navLatched = false;
        else if (!navLatched)
        {
            navLatched = true;
            ShowTab(currentTab + (x > 0 ? 1 : -1));
        }
    }

    public void Open()
    {
        isOpen = true;
        menuRoot.SetActive(true);
        Time.timeScale = 0f;
        ShowTab(currentTab);
        SelectFirst();
    }

    public void Close()
    {
        isOpen = false;
        Time.timeScale = 1f;
        menuRoot.SetActive(false);
    }

    public void ShowTab(int index)
    {
        if (panels.Length == 0) return;
        currentTab = (index + panels.Length) % panels.Length;

        for (int i = 0; i < panels.Length; i++)
            if (panels[i] != null) panels[i].SetActive(i == currentTab);

        for (int i = 0; i < tabButtons.Length; i++)
            if (tabButtons[i] != null)
                tabButtons[i].interactable = (i != currentTab);
    }

    private void SelectFirst()
    {
        if (EventSystem.current == null) return;
        EventSystem.current.SetSelectedGameObject(null);
        if (firstSelected != null) EventSystem.current.SetSelectedGameObject(firstSelected);
    }
}