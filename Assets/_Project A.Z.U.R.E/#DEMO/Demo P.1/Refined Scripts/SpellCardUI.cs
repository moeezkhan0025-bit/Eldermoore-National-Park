using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// The card template. Renders a spell: art, name, effect, and a ROW OF BUTTON
// ICON IMAGES for its sequence. Icons brighten/darken to show input progress.
// Build once as a prefab; CastModeUI binds a SpellDefinition to it.
public class SpellCardUI : MonoBehaviour
{
    [Header("Card fields")]
    [SerializeField] private Image cardArt;
    [SerializeField] private TMP_Text cardName;
    [SerializeField] private TMP_Text cardEffect;

    [Header("Sequence icon row")]
    [Tooltip("One Image per slot — enough for your longest spell sequence.")]
    [SerializeField] private Image[] sequenceIcons;

    [Header("Button sprites (assign the 4 face buttons)")]
    [SerializeField] private Sprite northSprite;   // △
    [SerializeField] private Sprite southSprite;   // ✕
    [SerializeField] private Sprite eastSprite;    // ○
    [SerializeField] private Sprite westSprite;    // □

    [Header("Progress colors")]
    [SerializeField] private Color pendingColor = Color.white;                 // not pressed
    [SerializeField] private Color doneColor = new Color(0.4f, 0.4f, 0.4f);    // pressed / darkened

    private SpellDefinition spell;

    public void Bind(SpellDefinition s)
    {
        spell = s;
        if (spell == null) return;

        if (cardArt != null) { cardArt.sprite = spell.icon; cardArt.enabled = spell.icon != null; }
        if (cardName != null) cardName.text = spell.displayName;
        if (cardEffect != null) cardEffect.text = spell.description;

        BuildSequenceIcons();
        SetProgress(0);
    }

    public void SetProgress(int doneCount)
    {
        var seq = spell != null ? spell.inputSequence : null;
        for (int i = 0; i < sequenceIcons.Length; i++)
        {
            if (sequenceIcons[i] == null) continue;
            bool used = seq != null && i < seq.Count;
            if (!used) { sequenceIcons[i].enabled = false; continue; }
            sequenceIcons[i].color = (i < doneCount) ? doneColor : pendingColor;
        }
    }

    void BuildSequenceIcons()
    {
        var seq = spell.inputSequence;
        for (int i = 0; i < sequenceIcons.Length; i++)
        {
            if (sequenceIcons[i] == null) continue;
            if (i < seq.Count)
            {
                sequenceIcons[i].sprite = SpriteFor(seq[i]);
                sequenceIcons[i].enabled = true;
                sequenceIcons[i].color = pendingColor;
            }
            else sequenceIcons[i].enabled = false;   // hide unused slots
        }
    }

    Sprite SpriteFor(CastInput b)
    {
        switch (b)
        {
            case CastInput.North: return northSprite;
            case CastInput.South: return southSprite;
            case CastInput.East: return eastSprite;
            case CastInput.West: return westSprite;
        }
        return null;
    }
}