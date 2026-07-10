using UnityEngine;
using UnityEngine.SceneManagement;

// Lives in the Bootstrap scene. After the persistent managers here initialize,
// it loads the main menu. This object doesn't need to persist — its job is done
// once the menu is loaded.
public class Bootstrapper : MonoBehaviour
{
    [SerializeField] private string firstScene = "MainMenu";

    private void Start()
    {
        SceneManager.LoadScene(firstScene);
    }
}