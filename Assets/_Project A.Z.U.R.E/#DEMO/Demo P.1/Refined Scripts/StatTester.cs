using UnityEngine;

// Interactive tester. Put it on the player (needs PlayerStats + PlayerHealth).
// Shows live stats + how they translate to movement, and lets you poke values
// with keys while playing so you can FEEL the effect. Delete when done.
//
// Keys (while playing):
//   1 : +0.25 SpeedMult (faster) for 5s      2 : -0.25 SpeedMult (slower) for 5s
//   3 : +0.5 JumpMult for 5s                  4 : +1 warp charge (permanent test)
//   5 : take 8 damage                         6 : heal 8
//   0 : clear all timed effects (re-press to reset)
public class StatTester : MonoBehaviour
{
    PlayerStats stats;
    PlayerHealth health;
    MovementController move;

    void Awake()
    {
        stats  = GetComponent<PlayerStats>();
        health = GetComponent<PlayerHealth>();
        move   = GetComponent<MovementController>();
    }

    void Update()
    {
        // NOTE: legacy Input for quick testing. If on the new Input System, these
        // key reads may need swapping — tell me and I'll convert them.
        if (Input.GetKeyDown(KeyCode.Alpha1)) stats.AddTimed(StatType.SpeedMult,  0.25f, 5f);
        if (Input.GetKeyDown(KeyCode.Alpha2)) stats.AddTimed(StatType.SpeedMult, -0.25f, 5f);
        if (Input.GetKeyDown(KeyCode.Alpha3)) stats.AddTimed(StatType.JumpMult,   0.5f,  5f);
        if (Input.GetKeyDown(KeyCode.Alpha4)) stats.AddPermanent(StatType.WarpChargesBonus, 1f, this);
        if (Input.GetKeyDown(KeyCode.Alpha5)) health.TakeDamage(8f);
        if (Input.GetKeyDown(KeyCode.Alpha6)) health.Heal(8f);
        if (Input.GetKeyDown(KeyCode.Alpha0)) stats.RemoveFromSource(this);
    }

    void OnGUI()
    {
        var style = new GUIStyle(GUI.skin.label) { fontSize = 16, richText = true };
        float x = 12, y = 12, w = 460, lh = 22;

        GUI.Box(new Rect(x - 6, y - 6, w + 12, lh * 12 + 12), "");

        void Line(string s) { GUI.Label(new Rect(x, y, w, lh), s, style); y += lh; }

        Line("<b>— STATS —</b>");
        Line($"Attack: {stats.Get(StatType.Attack):0.0}   Defense: {stats.Get(StatType.Defense):0.0}");
        Line($"SpeedMult: x{stats.GetMultiplier(StatType.SpeedMult):0.00}   " +
             $"JumpMult: x{stats.GetMultiplier(StatType.JumpMult):0.00}");
        Line($"MaxHealth: {stats.Get(StatType.MaxHealth):0}   WarpBonus: +{stats.Get(StatType.WarpChargesBonus):0}");

        Line("<b>— HEALTH —</b>");
        Line($"HP: {health.Current:0}/{health.Max:0}   Hearts: {health.CurrentHearts:0.0}/{health.MaxHearts}");

        Line("<b>— ACTIVE EFFECTS —</b>");
        int shown = 0;
        foreach (var m in stats.ActiveModifiers)
        {
            if (!m.IsTimed) continue;
            Line($"  {m.stat} {(m.value >= 0 ? "+" : "")}{m.value:0.00}  ({m.remainingTime:0.0}s)");
            shown++;
        }
        if (shown == 0) Line("  (none)");

        Line("<b>— KEYS —</b>");
        Line("1/2 speed±  3 jump+  4 warp+  5 dmg  6 heal  0 clear");
    }
}
