using UnityEngine;

// Minimal health for sandbox dummies. Put it on an enemy (tagged "Enemy").
public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 30f;
    private float current;

    void Awake() => current = maxHealth;

    public void TakeDamage(float amount)
    {
        current -= amount;
        Debug.Log($"{name} took {amount} → {Mathf.Max(current, 0)}/{maxHealth}");
        if (current <= 0f)
        {
            Debug.Log($"{name} defeated");
            Destroy(gameObject);
        }
    }
}
