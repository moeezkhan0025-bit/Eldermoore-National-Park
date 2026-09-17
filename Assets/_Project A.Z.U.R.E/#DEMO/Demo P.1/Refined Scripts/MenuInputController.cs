using UnityEngine;

// The single navigation brain for the whole menu (no EventSystem). Three focus
// states:
//   TAB     : Left/Right cycles tabs, Down enters the active panel.
//   CONTENT : Left/Right/Up/Down move within the panel (IMenuPanel); out-the-top
//             returns to tabs; Confirm activates the highlighted item.
//   POPUP   : a ConfirmPopup is open — Left/Right pick No/Yes, Confirm chooses,
//             Cancel closes. Grid navigation is suspended until it closes.
public class MenuInputController : MonoBehaviour
{
    [SerializeField] private MenuController menu;

    private GameController controls;
    private bool inContent;
    private ConfirmPopup activePopup;   // non-null => POPUP focus
    private Vector2 lastNav;

    private IMenuPanel CurrentPanel =>
        menu != null ? menu.GetCurrentPanelInterface() : null;

    void Awake()
    {
        controls = new GameController();
        if (menu == null) menu = GetComponent<MenuController>();
    }

    void OnEnable() { controls?.UI.Enable(); }
    void OnDisable() { controls?.UI.Disable(); }
    void OnDestroy() { controls?.Dispose(); }

    void Update()
    {
        if (menu == null || !menu.IsOpen) { inContent = false; return; }

        Vector2 nav = controls.UI.Navigate.ReadValue<Vector2>();
        Vector2Int step = Step(nav);
        bool confirm = controls.UI.Submit.WasPressedThisFrame();
        bool cancel = controls.UI.Cancel.WasPressedThisFrame();

        // ---- POPUP focus (highest priority) ----
        if (activePopup != null)
        {
            if (cancel) { activePopup.Cancel(); return; }
            if (confirm) { activePopup.Confirm(); return; }
            if (step.x != 0) activePopup.MoveChoice(step.x);
            return;   // swallow all other input while popup is open
        }

        // ---- CONTENT focus ----
        if (inContent)
        {
            if (confirm) { CurrentPanel?.Activate(); return; }
            if (step != Vector2Int.zero)
            {
                bool stillInside = CurrentPanel != null && CurrentPanel.Move(step);
                if (!stillInside) ReturnToTabs();
            }
            return;
        }

        // ---- TAB focus ----
        if (step.x != 0) menu.CycleTab(step.x);
        else if (step.y < 0) TryEnterContent();
    }

    void TryEnterContent()
    {
        var panel = CurrentPanel;
        if (panel != null && panel.EnterPanel())
        {
            inContent = true;
            menu.InContentMode = true;
        }
    }

    void ReturnToTabs()
    {
        CurrentPanel?.ExitPanel();
        inContent = false;
        menu.InContentMode = false;
    }

    // Called by ConfirmPopup when it opens/closes.
    public void OpenPopup(ConfirmPopup popup) { activePopup = popup; }
    public void ClosePopup() { activePopup = null; }

    Vector2Int Step(Vector2 nav)
    {
        Vector2Int s = Vector2Int.zero;
        const float dead = 0.5f;

        bool wasNeutral = Mathf.Abs(lastNav.x) < dead && Mathf.Abs(lastNav.y) < dead;
        lastNav = nav;
        if (!wasNeutral) return s;

        if (nav.x > dead) s.x = 1;
        else if (nav.x < -dead) s.x = -1;
        else if (nav.y > dead) s.y = 1;
        else if (nav.y < -dead) s.y = -1;
        return s;
    }
}