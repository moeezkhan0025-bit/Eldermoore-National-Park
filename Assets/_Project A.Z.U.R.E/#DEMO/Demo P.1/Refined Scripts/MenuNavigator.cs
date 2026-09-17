using UnityEngine;
using UnityEngine.EventSystems;

// Down enters content, up-from-top exits. Reads Navigate THROUGH MenuController
// (one instance) rather than making its own, so there's one fewer enabled copy.
public class MenuNavigator : MonoBehaviour
{
    [SerializeField] private MenuController menu;

    private bool inContent;
    private bool downLatched;

    void Awake()
    {
        if (menu == null) menu = GetComponent<MenuController>();
    }

    void Update()
    {
        if (menu == null || !menu.IsOpen) { inContent = false; downLatched = false; return; }

        Vector2 nav = menu.ReadNavigate();
        if (nav.y > -0.5f) downLatched = false;

        if (!inContent && nav.y < -0.5f && !downLatched)
        {
            downLatched = true;
            GameObject entry = menu.GetCurrentPanelFirstSelectable();
            if (entry == null || EventSystem.current == null) return;

            inContent = true;
            menu.InContentMode = true;
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(entry);
        }
    }

    public void ReturnToTabs()
    {
        inContent = false;
        if (menu != null) menu.InContentMode = false;
        menu.SelectCurrentTab();
    }
}