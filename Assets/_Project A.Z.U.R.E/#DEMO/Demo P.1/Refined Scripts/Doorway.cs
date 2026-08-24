using UnityEngine;

// A doorway the player interacts with to change scenes, with a fade transition.
[RequireComponent(typeof(Collider2D))]
public class Doorway : MonoBehaviour
{
    [SerializeField] private string targetScene = "Game";
    [SerializeField] private string spawnPointId = "FromRangerHQ";
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private GameObject prompt;

    private bool playerInRange;
    private bool transitioning;   // don't fire twice mid-fade

    private void Start()
    {
        if (prompt != null) prompt.SetActive(false);
    }

    private void Update()
    {
        if (playerInRange && !transitioning && Input.GetKeyDown(interactKey))
            Enter();
    }

    private void Enter()
    {
        transitioning = true;
        if (prompt != null) prompt.SetActive(false);

        // Route through the fader if it exists; otherwise load directly.
        if (SceneFader.Instance != null)
            SceneFader.Instance.TransitionToScene(targetScene, spawnPointId);
        else
            GameManager.Instance.LoadSceneWithSpawn(targetScene, spawnPointId);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<MovementController>() == null) return;
        playerInRange = true;
        if (prompt != null) prompt.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponentInParent<MovementController>() == null) return;
        playerInRange = false;
        if (prompt != null) prompt.SetActive(false);
    }
}