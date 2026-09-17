using System.Collections.Generic;
using UnityEngine;

// The bullet-time combo caster, now routed through SpellcasterController.
// Hold the cast key -> time slows -> tap the input keys to build a sequence ->
// release (or timer) -> the sequence is matched to a loadout spell and cast
// (respecting its cooldown). Put on the player alongside SpellcasterController.
public class CastController : MonoBehaviour
{
    [Header("Cast window")]
    [SerializeField] private KeyCode castKey = KeyCode.Space;
    [SerializeField] private float slowFactor = 0.15f;
    [SerializeField] private float maxCastSeconds = 3f;

    [Header("Cast inputs")]
    [SerializeField] private KeyCode fireKey = KeyCode.J;
    [SerializeField] private KeyCode frostKey = KeyCode.K;
    [SerializeField] private KeyCode boltKey = KeyCode.L;

    private SpellcasterController caster;
    private bool casting;
    private float windowStartUnscaled;
    private readonly List<CastInput> buffer = new List<CastInput>();
    private float defaultFixedDelta;

    void Awake()
    {
        caster = GetComponent<SpellcasterController>();
        defaultFixedDelta = Time.fixedDeltaTime;
        if (caster == null)
            Debug.LogError("[CastController] No SpellcasterController on the player.", this);
    }

    void Update()
    {
        if (!casting)
        {
            if (Input.GetKeyDown(castKey)) BeginCast();
            return;
        }

        CaptureInputs();

        bool released = !Input.GetKey(castKey);
        bool timeUp = Time.unscaledTime - windowStartUnscaled >= maxCastSeconds;
        if (released || timeUp) EndCast();
    }

    public void BeginCast()
    {
        casting = true;
        buffer.Clear();
        windowStartUnscaled = Time.unscaledTime;
        Time.timeScale = slowFactor;
        Time.fixedDeltaTime = defaultFixedDelta * slowFactor;
    }

    void CaptureInputs()
    {
        if (Input.GetKeyDown(fireKey))  Append(CastInput.Fire);
        if (Input.GetKeyDown(frostKey)) Append(CastInput.Frost);
        if (Input.GetKeyDown(boltKey))  Append(CastInput.Bolt);
    }

    void Append(CastInput input)
    {
        buffer.Add(input);
        Debug.Log($"[Cast] {string.Join(" ", buffer)}");
    }

    void EndCast()
    {
        casting = false;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = defaultFixedDelta;

        // Match the input sequence to a loadout spell and cast it.
        if (caster != null && buffer.Count > 0)
        {
            var spell = caster.MatchSequence(buffer);
            if (spell != null) caster.TryCast(spell);
            else Debug.Log("[Cast] No spell matches that sequence.");
        }

        buffer.Clear();
    }
}
