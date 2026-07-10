using UnityEngine;
using Cinemachine;   // Cinemachine 2.x. For 3.x use: using Unity.Cinemachine;

// Put this on the CinemachineVirtualCamera in the Game scene. When the scene
// loads, it points the camera's Follow target at the persistent player that
// carried in from the previous scene.
//
// Cinemachine 3.x note: change the type below to CinemachineCamera and the
// using directive to Unity.Cinemachine.
[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CameraTargetBinder : MonoBehaviour
{
    private void Start()
    {
        var player = PlayerPersistence.Instance;
        if (player == null) return;

        var vcam = GetComponent<CinemachineVirtualCamera>();
        vcam.Follow = player.transform;
        // For a 2D game you usually only need Follow. If your vcam uses an Aim
        // that needs a look target, also set: vcam.LookAt = player.transform;
    }
}
