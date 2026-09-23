using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// The card template. Renders a spell (art, name, sequence icons, effect) and greys
// the whole card out via a CanvasGroup while the spell is on cooldown.
public class SpellCardUI : MonoBehaviour
{
    [Header("Card fields")]
    [SerializeField] private Image cardArt;
    [SerializeField] private TMP_Text cardName;
    [SerializeField] private TMP_Text cardEffect;

    [Header("Sequence icon row")]
    [SerializeField] private Image[] sequenceIcons;

    [Header("Button sprites (assign the 4 face buttons)")]
    [SerializeField] private Sprite northSprite;   // △
    [SerializeField] private Sprite southSprite;   // ✕
    [SerializeField] private Sprite eastSprite;    // ○
    [SerializeField] private Sprite westSprite;    // □

    [Header("Sequence progress colors")]
    [SerializeField] private Color pendingColor = Color.white;
    [SerializeField] private Color doneColor = new Color(0.4f, 0.4f, 0.4f);

    [Header("Cooldown")]
    [SerializeField] private CanvasGroup dimGroup;      // dims the whole card while on cooldown
    [SerializeField] private float cooldownAlpha = 0.35f;

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
            else sequenceIcons[i].enabled = false;
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

    // Called each frame while the card is shown: grey the whole card while the
    // spell is on cooldown, restore it when ready.
    public void UpdateCooldown(SpellcasterController caster)
    {
        if (spell == null) return;
        if (caster == null) { Debug.LogWarning("[CardCD] caster is NULL — CastModeUI didn't resolve it"); return; }
        if (dimGroup == null) { Debug.LogWarning("[CardCD] dimGroup NOT assigned on SpellCardUI!"); return; }
        float rem = caster.CooldownRemaining(spell);
        bool onCd = rem > 0f;
        dimGroup.alpha = onCd ? cooldownAlpha : 1f;
        if (onCd) Debug.Log($"[CardCD] {spell.displayName} rem={rem:0.0}s, set alpha={dimGroup.alpha}");
    }

    public bool IsOnCooldown(SpellcasterController caster) =>
        spell != null && caster != null && caster.CooldownRemaining(spell) > 0f;
}