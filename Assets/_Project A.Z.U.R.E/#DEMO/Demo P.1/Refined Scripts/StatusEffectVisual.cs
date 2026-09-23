using UnityEngine;

// Flashes the player's sprite a color based on the active status effect:
//   Stun = yellow, Poison = purple, Burn = red, Frozen = cyan.
// Put on the player with a SpriteRenderer and a StatusEffectReceiver.
public class StatusEffectVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private StatusEffectReceiver status;

    [Header("Colors")]
    [SerializeField] private Color stunColor = Color.yellow;
    [SerializeField] private Color poisonColor = new Color(0.6f, 0.2f, 0.8f);  // purple
    [SerializeField] private Color burnColor = Color.red;
    [SerializeField] private Color frozenColor = Color.cyan;

    [Header("Flash")]
    [SerializeField] private float flashSpeed = 6f;    // pulses per second-ish
    [SerializeField] private float minAlpha = 0.4f;    // how much it dims at the pulse trough

    private Color baseColor;

    void Awake()
    {
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        if (status == null) status = GetComponent<StatusEffectReceiver>();
        if (sr != null) baseColor = sr.color;
    }

    void Update()
    {
        if (sr == null || status == null) return;

        // Priority: Frozen > Burn > Poison > Stun (pick the most "notable" active one).
        Color? tint = null;
        if (status.HasEffect(StatusEffectType.Frozen)) tint = frozenColor;
        else if (status.HasEffect(StatusEffectType.Burn)) tint = burnColor;
        else if (status.HasEffect(StatusEffectType.Poison)) tint = poisonColor;
        else if (status.HasEffect(StatusEffectType.Stun)) tint = stunColor;
        else if (status.HasEffect(StatusEffectType.Slow)) tint = frozenColor;

        if (tint == null)
        {
            sr.color = baseColor;   // no effect -> normal
            return;
        }

        // Pulse between the base color and the status tint.
        float t = (Mathf.Sin(Time.time * flashSpeed) + 1f) * 0.5f;  // 0..1
        Color flash = Color.Lerp(baseColor, tint.Value, Mathf.Lerp(minAlpha, 1f, t));
        sr.color = flash;
    }
}
