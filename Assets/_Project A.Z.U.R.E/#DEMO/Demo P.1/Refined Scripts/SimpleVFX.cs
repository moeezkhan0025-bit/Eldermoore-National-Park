using UnityEngine;

// A self-destroying placeholder visual effect. Put it on a VFX prefab (a sprite,
// a particle burst, an expanding ring, a flash). It scales/fades over its lifetime
// then destroys itself. Swap for real art/particles later — spawners don't change.
public class SimpleVFX : MonoBehaviour
{
    [SerializeField] private float lifetime = 0.4f;
    [SerializeField] private bool expand = true;          // grow over life (ring/burst feel)
    [SerializeField] private float startScale = 0.2f;
    [SerializeField] private float endScale = 2f;
    [SerializeField] private bool fade = true;
    [SerializeField] private SpriteRenderer sr;

    private float t;
    private Color baseColor;

    void Awake()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr != null) baseColor = sr.color;
        if (expand) transform.localScale = Vector3.one * startScale;
    }

    void Update()
    {
        t += Time.deltaTime / lifetime;
        if (expand) transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, t);
        if (fade && sr != null)
        {
            var c = baseColor; c.a = Mathf.Lerp(baseColor.a, 0f, t); sr.color = c;
        }
        if (t >= 1f) Destroy(gameObject);
    }
}
