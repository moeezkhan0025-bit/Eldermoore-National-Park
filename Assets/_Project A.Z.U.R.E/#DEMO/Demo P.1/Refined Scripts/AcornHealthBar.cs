using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Segmented acorn health bar (scene-based). 5 acorns = 100 HP. Each acorn = top +
// bottom half = 10 HP each.
//
// EMPTYING (damage): removes halves right-to-left, BOTTOM before TOP per acorn.
// REGEN: when the player hasn't taken damage for 'regenStartDelay' seconds, halves
// regrow in reverse (TOP first, then BOTTOM), each taking its own configurable time.
// Taking damage cancels/pauses regen.
//
// Reads the persistent player's PlayerHealth via PlayerPersistence.Instance.
public class AcornHealthBar : MonoBehaviour
{
    [System.Serializable]
    public class Acorn
    {
        public Image topHalf;
        public Image bottomHalf;
    }

    [Header("Acorns LEFT to right (element 0 = leftmost).")]
    [SerializeField] private List<Acorn> acorns = new List<Acorn>();
    [SerializeField] private float hpPerSegment = 10f;

    [Header("Regeneration")]
    [Tooltip("Seconds of NO damage before regen begins.")]
    [SerializeField] private float regenStartDelay = 2f;
    [Tooltip("Seconds to regrow the TOP half of an acorn.")]
    [SerializeField] private float topRegenTime = 7f;
    [Tooltip("Seconds to regrow the BOTTOM half of an acorn.")]
    [SerializeField] private float bottomRegenTime = 7f;
    [Tooltip("Regen can't heal the player above this many segments (visual cap = full).")]
    [SerializeField] private bool regenAffectsHealth = true;

    private PlayerHealth health;
    private float lastDamageTime;
    private float lastKnownHealth;
    private float regenTimer;

    void Update()
    {
        // Acquire the persistent player's health.
        if (health == null)
        {
            var player = PlayerPersistence.Instance;
            if (player != null)
            {
                health = player.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.HealthChanged += OnHealth;
                    lastKnownHealth = health.Current;
                    Redraw();
                }
            }
            return;
        }

        // Regen: only when not recently damaged and not already full.
        if (Time.time - lastDamageTime >= regenStartDelay && health.Current < health.Max)
        {
            TickRegen();
        }
    }

    void OnDestroy()
    {
        if (health != null) health.HealthChanged -= OnHealth;
    }

    void OnHealth(float current, float max)
    {
        // Detect damage (health dropped) to reset the regen delay.
        if (current < lastKnownHealth)
        {
            lastDamageTime = Time.time;
            regenTimer = 0f;   // cancel in-progress regen segment
        }
        lastKnownHealth = current;
        Redraw();
    }

    void TickRegen()
    {
        // How many segments are currently filled, and what the next-to-regrow needs.
        int filled = FilledSegments(health.Current);
        int maxSegments = acorns.Count * 2;
        if (filled >= maxSegments) return;

        // Determine if the next segment to regrow is a TOP or BOTTOM half.
        // Segments fill order (matching emptying reverse): the next filled segment
        // index is 'filled'. Within an acorn, index parity tells top vs bottom.
        // Our layout per acorn: [bottom = even index, top = odd index] filled
        // bottom-then-top. Since BOTTOM empties first, TOP regrows first — so we
        // add the TOP of the current partial acorn before its BOTTOM.
        bool nextIsTop = (filled % 2 == 0) ? false : true;
        // Explanation: with bottom=even/top=odd fill order, an even 'filled' means
        // the next segment is a bottom (even index); odd means next is a top.
        // But we want TOP to come back first, so we regrow the top of the partial
        // acorn first. Use the appropriate timer:
        float needed = nextIsTop ? topRegenTime : bottomRegenTime;

        regenTimer += Time.deltaTime;
        if (regenTimer >= needed)
        {
            regenTimer = 0f;
            if (regenAffectsHealth && health != null)
                health.Heal(hpPerSegment);   // heal one segment's worth
            Redraw();
        }
    }

    int FilledSegments(float current) =>
        Mathf.Clamp(Mathf.CeilToInt(current / hpPerSegment), 0, acorns.Count * 2);

    void Redraw()
    {
        if (health == null) return;
        int filled = FilledSegments(health.Current);

        // Fill order chosen so that EMPTYING removes BOTTOM before TOP:
        //   per acorn, TOP is the lower segment index, BOTTOM is the higher.
        // So losing HP turns off the BOTTOM (higher index) first. Regen (adding)
        // brings back the TOP (lower index) first.
        int seg = 0;
        for (int i = 0; i < acorns.Count; i++)
        {
            bool topOn = seg < filled; seg++;   // top = lower index -> stays longer, regrows first
            bool bottomOn = seg < filled; seg++;   // bottom = higher index -> empties first
            if (acorns[i].topHalf != null) acorns[i].topHalf.enabled = topOn;
            if (acorns[i].bottomHalf != null) acorns[i].bottomHalf.enabled = bottomOn;
        }
    }
}