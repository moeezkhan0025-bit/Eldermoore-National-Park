using System.Text;
using UnityEngine;

// On-screen debug HUD for playtesting. Put it on any object in the scene (or the
// player). Shows live movement state, health, stats, and physics info so you can
// see WHY something happens (e.g. warping through a wall, wrong grounded state).
// Toggle with F1. Delete or disable for the real build.
public class DebugOverlay : MonoBehaviour
{
    [SerializeField] private KeyCode toggleKey = KeyCode.F1;
    [SerializeField] private bool startVisible = true;

    private bool visible;
    private static readonly System.Collections.Generic.List<string> recentEvents = new System.Collections.Generic.List<string>();
    private const int MaxEvents = 6;

    // Call from anywhere to log a debug event line (damage, warp, etc.).
    public static void Log(string line)
    {
        recentEvents.Add($"{Time.time:0.0}s  {line}");
        if (recentEvents.Count > MaxEvents) recentEvents.RemoveAt(0);
    }

    private MovementController move;
    private PlayerHealth health;
    private PlayerStats stats;
    private Rigidbody2D rb;

    void Start()
    {
        visible = startVisible;
        FindPlayer();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey)) visible = !visible;
        if (move == null) FindPlayer();   // re-find if the player spawned late
    }

    void FindPlayer()
    {
        // Persistence owner first, then tag, then any MovementController in the scene.
        GameObject p = null;
        if (PlayerPersistence.Instance != null) p = PlayerPersistence.Instance.gameObject;
        if (p == null) p = GameObject.FindGameObjectWithTag("Player");
        if (p == null)
        {
            var mc = FindObjectOfType<MovementController>();
            if (mc != null) p = mc.gameObject;
        }
        if (p != null)
        {
            move = p.GetComponent<MovementController>();
            health = p.GetComponent<PlayerHealth>();
            stats = p.GetComponent<PlayerStats>();
            rb = p.GetComponent<Rigidbody2D>();
        }
    }

    void OnGUI()
    {
        if (!visible) return;

        var style = new GUIStyle(GUI.skin.label) { fontSize = 14, richText = true };
        float x = 12, y = 12, w = 340, lh = 20;
        GUI.Box(new Rect(x - 6, y - 6, w + 12, lh * 16 + 12), "");
        void L(string s) { GUI.Label(new Rect(x, y, w, lh), s, style); y += lh; }

        L("<b>— DEBUG (F1 to toggle) —</b>");

        if (move == null) { L("<color=red>No player found</color>"); return; }

        // Movement state
        L($"Grounded: {Flag(move.IsGrounded)}   Facing: {(move.FacingRight ? "R" : "L")}");
        L($"Wall L/R: {Flag(move.IsTouchingLeft)} / {Flag(move.IsTouchingRight)}   Slide: {Flag(move.IsWallSliding)}");
        L($"Climbing: {Flag(move.IsClimbing)}   TouchClimb: {Flag(move.IsTouchingClimbable)}");
        L($"Warp charges: {move.WarpCharges}");

        if (rb != null)
            L($"Velocity: ({rb.velocity.x:0.0}, {rb.velocity.y:0.0})   Pos: ({rb.position.x:0.0}, {rb.position.y:0.0})");

        // Health
        if (health != null)
            L($"HP: {health.Current:0}/{health.Max:0}   Hearts: {health.CurrentHearts:0.0}/{health.MaxHearts}");

        // Stats
        if (stats != null)
        {
            L("<b>Stats</b>");
            L($"  ATK {stats.Get(StatType.Attack):0.0}  DEF {stats.Get(StatType.Defense):0.0}  SpellPwr {stats.Get(StatType.SpellPower):0.0}");
            L($"  SpeedMult x{stats.GetMultiplier(StatType.SpeedMult):0.00}  JumpMult x{stats.GetMultiplier(StatType.JumpMult):0.00}");
        }

        L("<b>— RECENT EVENTS —</b>");
        for (int i = recentEvents.Count - 1; i >= 0; i--)
            L($"  {recentEvents[i]}");
    }

    string Flag(bool b) => b ? "<color=lime>YES</color>" : "<color=grey>no</color>";
}