using UnityEngine;

// Base for a spell's behaviour (a ScriptableObject asset referenced by a
// SpellDefinition). Cast() runs the effect and returns a result so the caster/UI
// can react (e.g. "no enemy found" for an offensive spell with no target).
public abstract class SpellEffect : ScriptableObject
{
    // Set by SpellcasterController just before Cast(): the hovering charge visual
    // (e.g. Bolt's missile) to fire, if this spell spawned one.
    public static HomingProjectile PendingChargeVisual;

    // Return true if the effect actually did something; false + a message if not.
    public abstract CastResult Cast(GameObject caster);
}

public struct CastResult
{
    public bool success;
    public string message;   // shown when success is false (e.g. "No enemy found")

    public static CastResult Ok() => new CastResult { success = true };
    public static CastResult Fail(string msg) => new CastResult { success = false, message = msg };
}