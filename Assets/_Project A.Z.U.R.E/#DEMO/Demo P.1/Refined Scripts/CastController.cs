using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// The battle casting flow:
//   R1 (Player map) -> enter cast mode: enable the Cast map, show cast UI, slow time.
//   L1 / L2 -> cycle through the deck's spells (the highlighted one is "locked").
//   Face buttons (North/South/East/West) -> perform the locked spell's sequence.
//   On sequence match -> spell auto-fires (via SpellcasterController), cast mode exits.
//   R1 (Cast map) -> exit/cancel anytime.
//
// Put on the player alongside SpellcasterController. Requires a GameController asset
// with a "Cast" action map (ExitCast, CyclePrev, CycleNext, North, South, East, West)
// and an "EnterCast" action in the Player map.
public class CastController : MonoBehaviour
{
    [Header("Cast UI (optional)")]
    [SerializeField] private CastModeUI castUI;          // the above-player UI element

    private GameController controls;
    private SpellcasterController caster;

    private bool casting;
    private int selectedIndex;                           // which deck spell is locked
    private readonly List<CastInput> buffer = new List<CastInput>();
    private HomingProjectile hoverMissile;
    private int interferenceCount;   // extra inputs the player must add, set by enemies
    [SerializeField] private Vector3 chargeOffset = new Vector3(1f, 0.5f, 0f);
    [SerializeField] private Transform castSpawnPoint;   // optional: an empty child placed where the projectile spawns

    // Events for UI/feedback.
    public event Action<bool> CastModeChanged;           // (entered?)
    public event Action<SpellDefinition> SelectionChanged;
    public event Action<List<CastInput>> SequenceChanged;

    void Awake()
    {
        controls = new GameController();
        caster = GetComponent<SpellcasterController>();
        if (caster == null) Debug.LogError("[CastController] No SpellcasterController.", this);
        if (castUI == null) castUI = CastModeUI.Instance;   // find the persistent HUD if not assigned
    }

    void OnEnable()
    {
        controls.Player.Enable();     // EnterCast lives here
        // Cast map is enabled only while casting.
        controls.Player.EnterCast.performed += OnEnterCast;
        controls.Cast.ExitCast.performed += OnExitCast;
        controls.Cast.CyclePrev.performed += _ => Cycle(-1);
        controls.Cast.CycleNext.performed += _ => Cycle(1);
        controls.Cast.North.performed += _ => Input(CastInput.North);
        controls.Cast.South.performed += _ => Input(CastInput.South);
        controls.Cast.East.performed += _ => Input(CastInput.East);
        controls.Cast.West.performed += _ => Input(CastInput.West);
    }

    void OnDisable()
    {
        controls.Player.EnterCast.performed -= OnEnterCast;
        controls.Cast.ExitCast.performed -= OnExitCast;
        controls.Player.Disable();
        controls.Cast.Disable();
    }

    void OnDestroy() => controls.Dispose();

    // ---- enter / exit ----

    void OnEnterCast(InputAction.CallbackContext _)
    {
        if (casting) return;
        EnterCast();
    }

    void OnExitCast(InputAction.CallbackContext _) => ExitCast();

    void EnterCast()
    {
        if (caster == null || caster.Loadout.Count == 0) return;   // no spells, nothing to cast

        if (castUI == null) castUI = CastModeUI.Instance;
        casting = true;
        controls.Cast.Enable();                 // cast controls live now
        selectedIndex = 0;
        buffer.Clear();

        if (castUI != null) castUI.Show(caster.Loadout, selectedIndex);
        CastModeChanged?.Invoke(true);
        SelectionChanged?.Invoke(Selected);
    }

    void ExitCast()
    {
        if (!casting) return;
        casting = false;
        controls.Cast.Disable();
        buffer.Clear();
        ClearInterference();
        DismissHover();

        if (castUI != null) castUI.Hide();
        CastModeChanged?.Invoke(false);
    }

    // ---- selection ----

    SpellDefinition Selected =>
        (caster != null && selectedIndex >= 0 && selectedIndex < caster.Loadout.Count)
            ? caster.Loadout[selectedIndex] : null;

    void Cycle(int dir)
    {
        if (!casting || caster.Loadout.Count == 0) return;
        int n = caster.Loadout.Count;
        selectedIndex = (selectedIndex + dir + n) % n;
        buffer.Clear();
        DismissHover();                          // switching spells resets the sequence
        if (castUI != null) castUI.SetSelected(selectedIndex);
        SelectionChanged?.Invoke(Selected);
        SequenceChanged?.Invoke(buffer);
    }

    // ---- sequence input ----

    void Input(CastInput btn)
    {
        if (!casting) return;
        try { InputInner(btn); }
        catch (System.Exception e) { Debug.LogError($"[CastController] Input error: {e}"); }
    }

    void InputInner(CastInput btn)
    {
        var spell = Selected;
        if (spell == null) return;

        // If the selected spell is on cooldown, ignore inputs (can't cast it).
        if (caster != null && caster.CooldownRemaining(spell) > 0f)
        {
            DismissHover();
            return;
        }

        buffer.Add(btn);
        SequenceChanged?.Invoke(buffer);

        // Compare buffer against the locked spell's sequence.
        var seq = spell.inputSequence;

        // If the buffer no longer matches the start of the sequence, reset it
        // (and dismiss any charge visual, since the combo was broken).
        for (int i = 0; i < buffer.Count; i++)
        {
            if (i >= seq.Count || buffer[i] != seq[i])
            {
                buffer.Clear();
                DismissHover();                       // wrong input -> no missile
                if (castUI != null) castUI.ShowSequenceProgress(buffer);
                return;
            }
        }

        // The input was CORRECT. If this was the first correct input, spawn the
        // charge visual now (not on any random key — only on the right first press).
        if (buffer.Count == 1 && spell.chargeVisual != null && hoverMissile == null)
        {
            hoverMissile = Instantiate(spell.chargeVisual);
            if (castSpawnPoint != null)
            {
                hoverMissile.HoverAt(castSpawnPoint);
            }
            else
            {
                var mc = GetComponent<MovementController>();
                var facingOffset = chargeOffset;
                if (mc != null && !mc.FacingRight) facingOffset.x = -facingOffset.x;
                hoverMissile.Hover(transform, facingOffset);
            }
        }

        if (castUI != null) castUI.ShowSequenceProgress(buffer);

        // Full match -> fire. Interference requires EXTRA inputs beyond the sequence:
        // the player must press (sequence length + interferenceCount) correct-so-far.
        int required = seq.Count + interferenceCount;
        if (buffer.Count >= seq.Count && buffer.Count < required)
        {
            // sequence matched so far but interference demands more presses — keep waiting.
            if (castUI != null) castUI.ShowSequenceProgress(buffer);
            return;
        }
        if (buffer.Count == required)
        {
            bool fired = caster.TryCast(spell, hoverMissile);   // pass the hovering missile
            buffer.Clear();
            if (castUI != null) castUI.ShowSequenceProgress(buffer);
            if (fired)
            {
                hoverMissile = null;        // effect launched it
                ClearInterference();
                // STAY in cast mode so the player can cast again. Just reset the
                // sequence; the fired spell now shows greyed (on cooldown) on its card.
                buffer.Clear();
                if (castUI != null) castUI.ShowSequenceProgress(buffer);
            }
            else
            {
                if (hoverMissile != null) { hoverMissile.Dismiss(); hoverMissile = null; }  // no target -> vanish
                if (castUI != null) castUI.ShowMessage(caster.LastCastMessage);
            }
        }
    }

    void DismissHover()
    {
        if (hoverMissile != null) { hoverMissile.Dismiss(); hoverMissile = null; }
    }

    // --- Enemy interference: force the player to input extra buttons this cast ---
    public bool IsCasting => casting;
    public void AddInterference(int extra) { interferenceCount += Mathf.Max(0, extra); }
    public void ClearInterference() { interferenceCount = 0; }
    public int InterferenceCount => interferenceCount;
}