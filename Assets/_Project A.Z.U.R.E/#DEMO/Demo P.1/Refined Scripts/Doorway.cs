using UnityEngine;

// A doorway the player interacts with to change scenes, with a fade transition.
// Now driven by the shared interaction system: PlayerInteractor detects this
// doorway's trigger and calls Interact() on Triangle — so this script no longer
// reads input, tracks range, or manages its own prompt.
//
// WIRING (per doorway):
//   targetScene   -> the scene to load (e.g. "Game", "CabinInterior")
//   spawnPointId  -> the SpawnPoint.Id in that scene to arrive at
public class Doorway : Interactable
{
    [SerializeField] private string targetScene = "Game";
    [SerializeField] private string spawnPointId = "FromRangerHQ";

    private bool transitioning;   // guard so a mid-fade re-press does nothing

    public override void Interact(GameObject player)
    {
        if (transitioning) return;
        transitioning = true;

        // Route through the fader if present; otherwise load directly.
        if (SceneFader.Instance != null)
            SceneFader.Instance.TransitionToScene(targetScene, spawnPointId);
        else if (GameManager.Instance != null)
            GameManager.Instance.LoadSceneWithSpawn(targetScene, spawnPointId);
        else
            Debug.LogError($"[{name}] No SceneFader or GameManager to load '{targetScene}'.", this);
    }
}