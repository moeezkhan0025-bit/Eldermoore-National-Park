using UnityEngine;

// Base for a spell's behaviour, authored as a ScriptableObject asset and
// referenced by a SpellDefinition. Cast() runs the effect from the caster's
// current position/context. No runtime scene state lives here (it's a shared
// asset) — everything needed is read at cast time from the caster.
public abstract class SpellEffect : ScriptableObject
{
    public abstract void Cast(GameObject caster);
}
