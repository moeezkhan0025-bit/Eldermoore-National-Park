// Status effects an enemy attack can impose on the player.
public enum StatusEffectType
{
    None,
    Stun,     // can't act — yellow flash
    Poison,   // damage over time — purple flash
    Burn,     // damage over time (faster) — red flash
    Frozen,   // can't act + slowed — cyan flash
    Slow      // reduced movement — (uses Frozen-like cyan, or its own)
}
