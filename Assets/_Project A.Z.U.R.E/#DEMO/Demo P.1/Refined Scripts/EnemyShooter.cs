using UnityEngine;

// Fires EnemyProjectiles at the player on a cooldown, while the EnemyController is
// Chasing (player detected). A telegraph delay signals before firing. Add this to
// any enemy that should shoot — ground or flying.
[RequireComponent(typeof(EnemyController))]
public class EnemyShooter : MonoBehaviour
{
    [Header("Firing")]
    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField] private Transform firePoint;         // where projectiles spawn (child, optional)
    [SerializeField] private float fireCooldown = 2f;
    [SerializeField] private float windup = 0.4f;         // telegraph before firing
    [SerializeField] private float fireRange = 10f;       // only fire if player within this

    [Header("Telegraph")]
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private Color windupColor = Color.yellow;

    private EnemyController ctrl;
    private float cooldownTimer;
    private float windupTimer = -1f;
    private Color baseColor;

    void Awake()
    {
        ctrl = GetComponent<EnemyController>();
        if (bodyRenderer == null) bodyRenderer = GetComponentInChildren<SpriteRenderer>();
        if (bodyRenderer != null) baseColor = bodyRenderer.color;
    }

    void Update()
    {
        var player = ctrl.Player;
        if (player == null || ctrl.State != EnemyState.Chasing) { CancelWindup(); return; }
        if (Vector2.Distance(transform.position, player.position) > fireRange) { CancelWindup(); return; }

        cooldownTimer -= Time.deltaTime;

        // Mid-windup -> count down, then fire.
        if (windupTimer >= 0f)
        {
            windupTimer -= Time.deltaTime;
            if (windupTimer <= 0f)
            {
                Fire(player);
                windupTimer = -1f;
                if (bodyRenderer != null) bodyRenderer.color = baseColor;
            }
            return;
        }

        // Start a shot when ready.
        if (cooldownTimer <= 0f)
        {
            Debug.Log($"[Shooter] {name} starting windup (player in range, chasing).");
            windupTimer = windup;
            if (bodyRenderer != null) bodyRenderer.color = windupColor;
            cooldownTimer = fireCooldown;
        }
    }

    void Fire(Transform player)
    {
        if (projectilePrefab == null) { Debug.LogWarning($"[Shooter] {name}: no projectile prefab assigned!"); return; }
        Debug.Log($"[Shooter] {name} FIRING at player.");
        Vector2 origin = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
        Vector2 dir = ((Vector2)player.position - origin).normalized;
        var proj = Instantiate(projectilePrefab, origin, Quaternion.identity);
        proj.Launch(dir);
    }

    void CancelWindup()
    {
        if (windupTimer >= 0f)
        {
            windupTimer = -1f;
            if (bodyRenderer != null) bodyRenderer.color = baseColor;
        }
    }
}