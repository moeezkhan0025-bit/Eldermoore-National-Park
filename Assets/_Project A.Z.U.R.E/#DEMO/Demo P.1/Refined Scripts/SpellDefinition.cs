using System.Collections.Generic;
using UnityEngine;

// One spell asset: its identity, the input sequence that casts it, its cooldown,
// its effect (what it does), and optional material requirements (for bigger spells
// later — the three test spells leave this empty = cooldown-only).
//   Create > Spells/Spell Definition
[CreateAssetMenu(fileName = "New Spell", menuName = "Spells/Spell Definition")]
public class SpellDefinition : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public string displayName;
    public Sprite icon;
    [TextArea] public string description;

    [Header("Casting")]
    [Tooltip("The combo the player inputs to select this spell in the cast window.")]
    public List<CastInput> inputSequence = new List<CastInput>();
    [Min(0f)] public float cooldown = 3f;

    [Header("Effect")]
    public SpellEffect effect;   // Repulse / Bolt / Recall, etc.

    [Header("Requirements (leave empty = cooldown only)")]
    public List<SpellRequirement> requirements = new List<SpellRequirement>();
}

// A material requirement for a spell (empty for the cooldown-only test spells).
[System.Serializable]
public class SpellRequirement
{
    public ItemDefinition material;
    [Min(1)] public int amount = 1;
}
