using UnityEngine;
using UnityEngine.EventSystems;

// Put this on a content element that sits at the TOP of a panel (e.g. the first
// grid cell, or a panel-level handler). When the player presses UP and there's no
// element above to move to, it tells the MenuNavigator to pop back to the tabs.
//
// Simplest usage: put it on the panel root and call TryExitUp() from your own
// up-detection, OR put it on top-row selectables. This version listens on the
// object it's on via OnMove.
public class PanelExitZone : MonoBehaviour, IMoveHandler
{
    [SerializeField] private MenuNavigator navigator;

    void Awake()
    {
        if (navigator == null) navigator = FindObjectOfType<MenuNavigator>();
    }

    public void OnMove(AxisEventData eventData)
    {
        // If moving up and there's no explicit 'up' neighbor, exit to tabs.
        if (eventData.moveDir == MoveDirection.Up)
        {
            var selectable = GetComponent<UnityEngine.UI.Selectable>();
            bool hasUpNeighbor = selectable != null &&
                                 selectable.FindSelectableOnUp() != null;
            if (!hasUpNeighbor && navigator != null)
            {
                navigator.ReturnToTabs();
                eventData.Use();
            }
        }
    }
}
