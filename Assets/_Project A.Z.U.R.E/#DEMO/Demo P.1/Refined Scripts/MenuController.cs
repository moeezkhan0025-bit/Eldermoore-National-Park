using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Drives the pause menu shell: open/close, time freeze, and tab switching.
// Put this on MenuRoot. Front-end only — panels can be empty for now.
//
// Reads from your generated GameController wrapper, using ONLY actions that exist
// in your asset today: UI.Cancel (open/close) and UI.Navigate (cycle tabs).
// See notes below to upgrade to a dedicated Menu button + shoulder-button tabs.
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

    void OnEnable() { controls?.UI.Enable(); }
    void OnDisable() { controls?.UI.Disable(); }
    void OnDestroy() { controls?.Dispose(); }

    void Update()
    {
        // TEMP toggle: Cancel opens and closes. Swap for a dedicated Menu button later.
        if (!isOpen && controls.UI.Cancel.WasPressedThisFrame()) { Open(); return; }
        if (!isOpen) return;

        if (controls.UI.Cancel.WasPressedThisFrame()) { Close(); return; }

        // Cycle tabs by pushing Navigate left/right, latched so it steps once per push.
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