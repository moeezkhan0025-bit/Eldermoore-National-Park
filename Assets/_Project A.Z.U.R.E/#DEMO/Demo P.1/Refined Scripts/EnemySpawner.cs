using System.Collections.Generic;
using UnityEngine;

// Enemy behavior: spawns other enemies (e.g. Flyers) on a cooldown, up to a cap,
// while alive. When this spawner is destroyed, it stops (its spawns remain).
// Add to a Spawner variant.
[RequireComponent(typeof(Health))]
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyToSpawn;    // the Flyer prefab
    [SerializeField] private float spawnCooldown = 4f;
    [SerializeField] private int maxAlive = 3;            // don't exceed this many living spawns
    [SerializeField] private Transform[] spawnPoints;     // where they appear (optional; else at self)
    [SerializeField] private bool onlyWhenPlayerNear = true;

    private EnemyController ctrl;
    private float timer;
    private readonly List<GameObject> spawned = new List<GameObject>();

    void Awake() => ctrl = GetComponent<EnemyController>();

    void Update()
    {
        if (enemyToSpawn == null) return;

        // Only spawn when the player's detected, if that option is on.
        if (onlyWhenPlayerNear && (ctrl == null || ctrl.State == EnemyState.Patrolling)) return;

        // Clean up dead/destroyed spawns from the list.
        spawned.RemoveAll(e => e == null);
        if (spawned.Count >= maxAlive) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        Spawn();
        timer = spawnCooldown;
    }

    void Spawn()
    {
        Vector3 pos = transform.position;
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            var pt = spawnPoints[Random.Range(0, spawnPoints.Length)];
            if (pt != null) pos = pt.position;
        }
        var e = Instantiate(enemyToSpawn, pos, Quaternion.identity);
        spawned.Add(e);
        Debug.Log($"[{name}] spawned {e.name} ({spawned.Count}/{maxAlive})");
    }
}
