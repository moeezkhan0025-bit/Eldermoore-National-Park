using UnityEngine;

// Any menu tab's content panel implements this so the one MenuInputController can
// navigate it the same way — no EventSystem involved. The panel owns its own list
// of navigable items and a highlight frame it moves around.
public interface IMenuPanel
{
    // Called when focus enters this panel (player pressed Down from the tabs).
    // Returns false if the panel has nothing to navigate (so focus stays on tabs).
    bool EnterPanel();

    // Move the highlight by a grid direction. Returns false if the move would go
    // "out the top" (so the controller knows to return focus to the tabs).
    bool Move(Vector2Int dir);

    // Confirm / activate the currently highlighted item (A / Cross).
    void Activate();

    // Called when focus leaves this panel (back to tabs).
    void ExitPanel();
}
