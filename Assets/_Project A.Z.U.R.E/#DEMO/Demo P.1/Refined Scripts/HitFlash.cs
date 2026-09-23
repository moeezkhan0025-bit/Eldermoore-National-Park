using System.Collections;
using UnityEngine;

// Flashes the enemy's sprite (e.g. white/red) briefly when it takes damage.
// Put on the enemy alongside Health and a SpriteRenderer.
[RequireComponent(typeof(Health))]
public class HitFlash : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashTime = 0.08f;

    private Color baseColor;
    private Coroutine routine;

    void Awake()
    {
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) baseColor = sr.color;
        else Debug.LogWarning($"[HitFlash] {name}: no SpriteRenderer found — can't flash.", this);
    }

    void OnEnable() { GetComponent<Health>().Damaged += Flash; }
    void OnDisable() { GetComponent<Health>().Damaged -= Flash; }

    void Flash()
    {
        Debug.Log($"[HitFlash] {name} Flash() called.");
        if (sr == null) return;
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        sr.color = flashColor;
        yield return new WaitForSeconds(flashTime);
        sr.color = baseColor;
    }
}