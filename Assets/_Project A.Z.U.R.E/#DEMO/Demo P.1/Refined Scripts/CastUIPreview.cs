using System.Collections.Generic;
using UnityEngine;

// TEMP editor/preview helper: forces the CastModeUI to show a deck so you can
// build/style the card without the full R1 combat flow. Put it on any object,
// assign a few spell assets, press P in Play mode to show, [ and ] to cycle,
// O to hide. Delete once the real cast flow works.
public class CastUIPreview : MonoBehaviour
{
    [SerializeField] private List<SpellDefinition> previewDeck = new List<SpellDefinition>();
    private int index;

    void Update()
    {
        var ui = CastModeUI.Instance;
        if (ui == null) return;

        if (Input.GetKeyDown(KeyCode.P))
        {
            index = 0;
            ui.Show(previewDeck, index);
            Debug.Log("[CastUIPreview] Showing card 0. [ ] to cycle, O to hide.");
        }
        if (Input.GetKeyDown(KeyCode.RightBracket)) { index = Next(1); ui.SetSelected(index); }
        if (Input.GetKeyDown(KeyCode.LeftBracket)) { index = Next(-1); ui.SetSelected(index); }
        if (Input.GetKeyDown(KeyCode.O)) ui.Hide();
    }

    int Next(int dir)
    {
        if (previewDeck.Count == 0) return 0;
        return (index + dir + previewDeck.Count) % previewDeck.Count;
    }
}