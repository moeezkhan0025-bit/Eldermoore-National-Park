using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;   // Cinemachine 2.x

// Put this on the CinemachineVirtualCamera in each scene that uses one.
// Binds the vcam to the persistent player and snaps straight onto them (no ease-in
// that reads as a jump when the fade lifts). Now re-binds on every scene load too,
// so it works even if the vcam existed before the player arrived.
[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CameraTargetBinder : MonoBehaviour
{
    private CinemachineVirtualCamera vcam;

    private void Awake() => vcam = GetComponent<CinemachineVirtualCamera>();

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void Start() => Bind();

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Bind();

    private void Bind()
    {
        var player = PlayerPersistence.Instance;
        if (player == null) return;

        vcam.Follow = player.transform;

        // No continuity to preserve across a scene change — forget the old position
        // and place the camera directly on the player now.
        vcam.PreviousStateIsValid = false;
        vcam.ForceCameraPosition(player.transform.position, vcam.transform.rotation);
    }
}