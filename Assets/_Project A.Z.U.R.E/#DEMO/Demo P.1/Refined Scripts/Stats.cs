// The character stats your game tracks. Extend freely.
//
// Two flavors of stat here:
//  • Flat combat stats (Attack, Defense, MaxHealth...) — added up.
//  • Movement MULTIPLIERS (SpeedMult, JumpMult...) — these scale your
//    MovementController's tuned base values. A multiplier of 1.0 = normal.
//    Because base is 0 for a stat you don't list, movement stats need a
//    sensible default of 1 — PlayerStats handles that (see GetMultiplier).
public enum StatType
{
    // Combat / flat
    MaxHealth,
    Attack,
    Defense,
    SpellPower,

    // Movement multipliers (1.0 = your normal tuned feel)
    SpeedMult,
    JumpMult,
    WarpChargesBonus,   // flat add to warp charges (not a multiplier)
    ClimbSpeedMult
}

// One stat change (additive). e.g. +5 Attack, or +0.5 SpeedMult.
[System.Serializable]
public class StatModifier
{
    public StatType stat;
    public float value;
}