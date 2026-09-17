using UnityEngine;
using UnityEngine.UI;

// One deck slot. Shows a spell's icon when filled, empty otherwise. The panel
// spawns 15 of these (or you place 15 fixed slots). Purely visual — the panel
// reads/writes the deck; the slot just displays what it's given.
public class SpellSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Sprite emptySprite;   // optional placeholder when empty

    public SpellDefinition Spell { get; private set; }

    public void Show(SpellDefinition spell)
    {
        Spell = spell;
        if (iconImage == null) return;

        if (spell != null && spell.icon != null)
        {
            iconImage.sprite = spell.icon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.sprite = emptySprite;
            iconImage.enabled = emptySprite != null;
        }
    }
}
