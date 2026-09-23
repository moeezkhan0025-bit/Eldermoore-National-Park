using UnityEngine;

// Enemy behavior: while this enemy is alive and chasing the player, and the player
// is casting, it periodically "interferes" — forcing the player to input extra
// buttons to complete their spell. Add to a Flyer variant.
[RequireComponent(typeof(EnemyController))]
public class CastInterferer : MonoBehaviour
{
    [SerializeField] private int extraInputsPerInterfere = 1;
    [SerializeField] private float interfereCooldown = 3f;
    [SerializeField] private float range = 8f;
    [SerializeField] private GameObject interfereVfx;   // optional telegraph on the player/enemy

    private EnemyController ctrl;
    private float timer;

    void Awake() => ctrl = GetComponent<EnemyController>();

    void Update()
    {
        var player = ctrl.Player;
        if (player == null || ctrl.State != EnemyState.Chasing) return;
        if (Vector2.Distance(transform.position, player.position) > range) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        var cast = player.GetComponent<CastController>();
        if (cast != null && cast.IsCasting)
        {
            cast.AddInterference(extraInputsPerInterfere);
            timer = interfereCooldown;
            if (interfereVfx != null) Instantiate(interfereVfx, player.position, Quaternion.identity);
            Debug.Log($"[{name}] interfered with the player's cast (+{extraInputsPerInterfere} input).");
        }
    }
}
