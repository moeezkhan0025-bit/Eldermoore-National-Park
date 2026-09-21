using System;
using System.Collections.Generic;
using UnityEngine;

// The spells the player OWNS (unlocked/crafted). The spellbook's collection side
// displays these. Put on the player alongside SpellcasterController.
// The DECK (active loadout) lives on SpellcasterController; this is the full pool.
public class SpellCollection : MonoBehaviour
{
    [Tooltip("Spells the player owns. Seed test spells here; later, unlocks add to it.")]
    [SerializeField] private List<SpellDefinition> owned = new List<SpellDefinition>();

    public event Action CollectionChanged;

    public IReadOnlyList<SpellDefinition> Owned => owned;

    public void Unlock(SpellDefinition spell)
    {
        if (spell == null || owned.Contains(spell)) return;
        owned.Add(spell);
        CollectionChanged?.Invoke();
    }

    public bool Has(SpellDefinition spell) => owned.Contains(spell);
}