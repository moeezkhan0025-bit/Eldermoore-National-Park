using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Disables the Tilemap Collider 2D entirely for the drop duration.
/// Most reliable approach for Tilemap one-way platform setups.
///
/// Inspector wiring:
///   Platform Tilemap Collider → drag TilemapCollider2D from One-Way Platforms here
/// </summary>
public class PlatformDropThrough : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode dropKey = KeyCode.S;

    [Header("Settings")]
    [SerializeField] private float dropDuration = 0.35f;

    [Header("References")]
    [SerializeField] private TilemapCollider2D platformTilemapCollider;

    private bool _isDropping;

    private void Awake()
    {
        if (platformTilemapCollider == null)
            Debug.LogError("[DropThrough] Tilemap Collider not assigned.");
    }

    private void Update()
    {
        if (Input.GetKeyDown(dropKey) && !_isDropping)
            StartCoroutine(DropThrough());
    }

    private IEnumerator DropThrough()
    {
        _isDropping = true;

        platformTilemapCollider.enabled = false;
        Debug.Log("[DropThrough] Collider disabled.");

        yield return new WaitForSeconds(dropDuration);

        platformTilemapCollider.enabled = true;
        Debug.Log("[DropThrough] Collider restored.");

        _isDropping = false;
    }
}
