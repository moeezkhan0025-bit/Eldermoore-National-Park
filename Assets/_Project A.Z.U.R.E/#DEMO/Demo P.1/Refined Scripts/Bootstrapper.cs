using UnityEngine;
using UnityEngine.SceneManagement;

// Lives in the Bootstrap scene. Sets up persistent managers, then loads the menu.
public class Bootstrapper : MonoBehaviour
{
    [SerializeField] private string firstScene = "MainMenu";

    [Header("Dev Testing")]
    [Tooltip("If set, boot straight to this scene instead of the main menu. Leave EMPTY for normal play.")]
    [SerializeField] private string devStartScene = "";

    private void Start()
    {
#if UNITY_EDITOR
        if (!string.IsNullOrEmpty(devStartScene))
        {
            Debug.LogWarning($"DEV BOOT: skipping menu, loading '{devStartScene}'");
            SceneManager.LoadScene(devStartScene);
            return;
        }
#endif
        SceneManager.LoadScene(firstScene);
    }
}