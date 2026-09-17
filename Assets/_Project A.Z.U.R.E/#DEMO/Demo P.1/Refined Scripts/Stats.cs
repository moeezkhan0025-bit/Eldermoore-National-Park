// The character stats your game tracks. Extend freely.
//   • Flat stats (Attack, Defense, MaxHealth...) are added up.
//   • Multiplier stats (SpeedMult, JumpMult, WarpDistanceMult...) scale a base
//     value; 0 = normal, 0.25 = +25%. Leave their BASE at 0 in PlayerStats.
public enum StatType
{
    // Combat / flat
    MaxHealth,
    Attack,
    Defense,
    SpellPower,

    // Movement multipliers (0 = normal)
    SpeedMult,
    JumpMult,
    ClimbSpeedMult,
    WarpDistanceMult,     // NEW: scales how far a warp blinks

    // Flat movement add
    WarpChargesBonus
}

[System.Serializable]
public class StatModifier
{
    public StatType stat;
    public float value;
}