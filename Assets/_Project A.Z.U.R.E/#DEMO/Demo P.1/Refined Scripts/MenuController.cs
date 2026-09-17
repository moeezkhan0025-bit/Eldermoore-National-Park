using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Drives the pause menu shell. IMPORTANT: this does NOT create/enable its own copy
// of the UI action map — that conflicts with the EventSystem's navigation. It only
// enables the Player map (for the Menu button) and READS the shared UI actions by
// reference, so the EventSystem stays the sole owner of UI navigation.
public class MenuController : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject menuRoot;

    [Header("Tabs — panels and their buttons, same order")]
    [SerializeField] private GameObject[] panels;
    [SerializeField] private Button[] tabButtons;

    [Header("Selection")]
    [SerializeField] private GameObject firstSelected;

    private GameController controls;
    private int currentTab;
    private bool isOpen;
    private bool navLatched;

    public bool InContentMode { get; set; }
    public bool IsOpen => isOpen;
    public int CurrentTab => currentTab;

    void Awake()
    {
        controls = new GameController();
        if (menuRoot == null) menuRoot = gameObject;
        menuRoot.SetActive(false);
    }

    // Only the Player map here. The UI map is owned/enabled by the EventSystem.
    void OnEnable() { controls?.Player.Enable(); }
    void OnDisable() { controls?.Player.Disable(); }
    void OnDestroy() { controls?.Dispose(); }

    void Update()
    {
        if (controls.Player.Menu.WasPressedThisFrame())
        {
            if (isOpen) Close(); else Open();
            return;
        }
        if (!isOpen) return;

        // Read UI actions WITHOUT enabling our own copy — read the action's live value.
        // (These actions are enabled by the EventSystem, so ReadValue works.)
        if (controls.UI.Cancel.WasPressedThisFrame()) { Close(); return; }

        if (InContentMode) return;

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
        InContentMode = false;
        menuRoot.SetActive(true);
        Time.timeScale = 0f;
        ShowTab(currentTab);
        SelectFirst();
    }

    public void Close()
    {
        isOpen = false;
        InContentMode = false;
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

    public void SelectCurrentTab()
    {
        if (EventSystem.current == null) return;
        if (tabButtons != null && currentTab < tabButtons.Length && tabButtons[currentTab] != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(tabButtons[currentTab].gameObject);
        }
    }

    public void CycleTab(int dir) => ShowTab(currentTab + dir);

    public IMenuPanel GetCurrentPanelInterface()
    {
        if (panels == null || currentTab >= panels.Length || panels[currentTab] == null) return null;
        return panels[currentTab].GetComponent<IMenuPanel>();
    }

    public GameObject GetCurrentPanelFirstSelectable()
    {
        if (panels == null || currentTab >= panels.Length || panels[currentTab] == null) return null;
        var sel = panels[currentTab].GetComponentInChildren<Selectable>(false);
        return sel != null ? sel.gameObject : null;
    }

    // Read the current Navigate value from the SHARED (EventSystem-owned) UI action.
    public Vector2 ReadNavigate() => controls.UI.Navigate.ReadValue<Vector2>();
}