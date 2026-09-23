using UnityEngine;
using UnityEngine.UI;

// A small health bar that floats above the enemy. Put this on a world-space
// child of the enemy (a small canvas or a couple of sprites), with a fill Image
// or a fill Transform that scales. Reads the enemy's Health and updates on change.
public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Health health;         // the enemy's Health (parent)
    [SerializeField] private Image fillImage;       // Image with Type=Filled, or...
    [SerializeField] private Transform fillTransform; // ...a bar whose X scale = fraction
    [SerializeField] private bool hideWhenFull = true;
    [SerializeField] private GameObject barRoot;    // to show/hide

    void Awake()
    {
        if (health == null) health = GetComponentInParent<Health>();
        if (barRoot == null) barRoot = gameObject;
    }

    void OnEnable()
    {
        if (health != null) health.HealthChanged += UpdateBar;
    }

    void OnDisable()
    {
        if (health != null) health.HealthChanged -= UpdateBar;
    }

    void Start()
    {
        if (health != null) UpdateBar(health.Current, health.Max);
    }

    void UpdateBar(float current, float max)
    {
        float frac = max > 0f ? current / max : 0f;

        if (fillImage != null) fillImage.fillAmount = frac;
        if (fillTransform != null)
        {
            var s = fillTransform.localScale;
            fillTransform.localScale = new Vector3(frac, s.y, s.z);
        }

        if (hideWhenFull && barRoot != null)
            barRoot.SetActive(frac < 0.999f && frac > 0f);
    }
}