using UnityEngine;
using Cinemachine;   // Cinemachine 2.x

// Put this on the CinemachineVirtualCamera in each scene that uses one.
// On scene load it binds the camera to the persistent player AND snaps the
// camera straight onto them, instead of easing in from its old position
// (that ease is what reads as a "snap" when the fade lifts).
[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CameraTargetBinder : MonoBehaviour
{
    private void Start()
    {
        var player = PlayerPersistence.Instance;
        if (player == null) return;

        var vcam = GetComponent<CinemachineVirtualCamera>();
        vcam.Follow = player.transform;

        // Drop any memory of the previous position — there's no continuity to
        // preserve across a scene change — then place the camera on target now.
        vcam.PreviousStateIsValid = false;
        vcam.ForceCameraPosition(player.transform.position, vcam.transform.rotation);
    }
}