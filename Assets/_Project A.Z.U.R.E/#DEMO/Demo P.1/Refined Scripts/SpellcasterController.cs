using System.Collections.Generic;
using UnityEngine;

// Holds the player's active spell loadout, tracks per-spell cooldowns, and casts
// spells. Put on the player. CastController (the slow-mo combo window) calls
// TryCast() with the spell the player's combo matched.
public class SpellcasterController : MonoBehaviour
{
    [Tooltip("The player's active spells (later: built from the 15-card loadout).")]
    [SerializeField] private List<SpellDefinition> loadout = new List<SpellDefinition>();

    // spell id -> time (unscaled) when it becomes castable again
    private readonly Dictionary<string, float> readyAt = new Dictionary<string, float>();

    public IReadOnlyList<SpellDefinition> Loadout => loadout;

    public const int MaxDeckSize = 15;
    public int DeckCount => loadout.Count;
    public bool DeckFull => loadout.Count >= MaxDeckSize;

    public event System.Action DeckChanged;

    public bool IsInDeck(SpellDefinition spell) => spell != null && loadout.Contains(spell);

    // Add a spell to the active deck (the combat loadout). Capped at 15, no dupes.
    public bool AddToDeck(SpellDefinition spell)
    {
        if (spell == null || DeckFull || loadout.Contains(spell)) return false;
        loadout.Add(spell);
        DeckChanged?.Invoke();
        return true;
    }

    // Remove a spell from the active deck.
    public bool RemoveFromDeck(SpellDefinition spell)
    {
        if (spell == null) return false;
        bool removed = loadout.Remove(spell);
        if (removed) DeckChanged?.Invoke();
        return removed;
    }

    public bool IsOnCooldown(SpellDefinition spell)
    {
        if (spell == null) return true;
        return readyAt.TryGetValue(spell.id, out float t) && Time.unscaledTime < t;
    }

    public float CooldownRemaining(SpellDefinition spell)
    {
        if (spell == null || !readyAt.TryGetValue(spell.id, out float t)) return 0f;
        return Mathf.Max(0f, t - Time.unscaledTime);
    }

    // Match an input sequence (from the cast window) to a loadout spell.
    public SpellDefinition MatchSequence(List<CastInput> sequence)
    {
        foreach (var spell in loadout)
        {
            if (spell == null || spell.inputSequence.Count != sequence.Count) continue;
            bool same = true;
            for (int i = 0; i < sequence.Count; i++)
                if (spell.inputSequence[i] != sequence[i]) { same = false; break; }
            if (same) return spell;
        }
        return null;
    }

    // Attempt to cast a spell: checks cooldown (and later, materials), runs the
    // effect, and starts the cooldown. Returns true if it fired.
    public bool TryCast(SpellDefinition spell)
    {
        if (spell == null || spell.effect == null) return false;
        if (IsOnCooldown(spell)) { Debug.Log($"[Cast] {spell.displayName} on cooldown."); return false; }

        // (Material checks would go here later, using spell.requirements.)

        spell.effect.Cast(gameObject);
        readyAt[spell.id] = Time.unscaledTime + spell.cooldown;
        Debug.Log($"[Cast] {spell.displayName} cast. Cooldown {spell.cooldown}s.");
        return true;
    }

    // ---- Save / load the deck by spell id ----

    // Returns the current deck as a list of spell ids (for SaveData).
    public List<string> GetDeckIds()
    {
        var ids = new List<string>();
        foreach (var s in loadout) if (s != null) ids.Add(s.id);
        return ids;
    }

    // Rebuild the deck from saved ids, resolving each id against the given collection.
    public void LoadDeckFromIds(List<string> ids, SpellCollection collection)
    {
        loadout.Clear();
        if (ids == null || collection == null) { DeckChanged?.Invoke(); return; }
        foreach (var id in ids)
        {
            foreach (var owned in collection.Owned)
                if (owned != null && owned.id == id) { loadout.Add(owned); break; }
        }
        DeckChanged?.Invoke();
    }
}
