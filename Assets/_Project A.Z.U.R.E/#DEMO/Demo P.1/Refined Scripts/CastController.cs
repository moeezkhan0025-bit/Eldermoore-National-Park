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
    [SerializeField] private Vector3 chargeOffset = new Vector3(1f, 0.5f, 0f);

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

        // First input of the sequence -> spawn the charge visual (e.g. Bolt's hovering missile).
        if (buffer.Count == 0 && spell.chargeVisual != null && hoverMissile == null)
        {
            var facingOffset = chargeOffset;
            var mc = GetComponent<MovementController>();
            if (mc != null && !mc.FacingRight) facingOffset.x = -facingOffset.x;
            hoverMissile = Instantiate(spell.chargeVisual);
            hoverMissile.Hover(transform, facingOffset);
        }

        buffer.Add(btn);
        SequenceChanged?.Invoke(buffer);

        // Compare buffer against the locked spell's sequence.
        var seq = spell.inputSequence;

        // If the buffer no longer matches the start of the sequence, reset it.
        for (int i = 0; i < buffer.Count; i++)
        {
            if (i >= seq.Count || buffer[i] != seq[i]) { buffer.Clear(); if (castUI != null) castUI.ShowSequenceProgress(buffer); return; }
        }

        if (castUI != null) castUI.ShowSequenceProgress(buffer);

        // Full match -> fire.
        if (buffer.Count == seq.Count)
        {
            bool fired = caster.TryCast(spell, hoverMissile);   // pass the hovering missile
            buffer.Clear();
            if (castUI != null) castUI.ShowSequenceProgress(buffer);
            if (fired)
            {
                hoverMissile = null;             // the effect took ownership (launched it)
                ExitCast();
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
}