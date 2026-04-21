using UnityEngine;
using Cinemachine;

/// <summary>
/// Attaches to the same GameObject as your CinemachineVirtualCamera.
/// Shifts the Framing Transposer's Screen Y downward when the player
/// is falling, revealing more of what's below them.
///
/// Attach to: CM vcam1
/// </summary>
public class CinemachineFallBias : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D playerRb;

    [Header("Fall Settings")]
    [Tooltip("Downward velocity the player must exceed before bias activates.")]
    [SerializeField] private float fallThreshold = -2f;

    [Tooltip("Screen Y value when falling. Lower = shows more below. 0.3 is a good start.")]
    [SerializeField] private float fallScreenY = 0.3f;

    [Tooltip("Normal Screen Y value — should match what you set in the vcam Inspector.")]
    [SerializeField] private float normalScreenY = 0.4f;

    [Tooltip("How smoothly Screen Y transitions between normal and fall positions.")]
    [SerializeField] private float transitionSpeed = 2f;

    // ── Runtime ───────────────────────────────────────────────────────────────

    private CinemachineVirtualCamera _vcam;
    private CinemachineFramingTransposer _transposer;
    private float _currentScreenY;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        _vcam = GetComponent<CinemachineVirtualCamera>();
        _transposer = _vcam.GetCinemachineComponent<CinemachineFramingTransposer>();

        if (_transposer == null)
        {
            Debug.LogError("[CinemachineFallBias] No Framing Transposer found on vcam. " +
                           "Set Body to Framing Transposer in the Virtual Camera.");
            enabled = false;
            return;
        }

        if (playerRb == null)
        {
            Debug.LogError("[CinemachineFallBias] No Rigidbody2D assigned. " +
                           "Drag your Player into the Player Rb field.");
            enabled = false;
            return;
        }

        _currentScreenY = normalScreenY;
    }

    private void Update()
    {
        // Determine target Screen Y based on fall state
        bool isFalling = playerRb.velocity.y < fallThreshold;
        float targetScreenY = isFalling ? fallScreenY : normalScreenY;

        // Smoothly interpolate Screen Y
        _currentScreenY = Mathf.Lerp(
            _currentScreenY,
            targetScreenY,
            transitionSpeed * Time.deltaTime
        );

        // Apply to the Framing Transposer
        _transposer.m_ScreenY = _currentScreenY;
    }
}
