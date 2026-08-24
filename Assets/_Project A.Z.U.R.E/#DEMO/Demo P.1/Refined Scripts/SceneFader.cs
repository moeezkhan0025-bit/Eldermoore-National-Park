using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Persistent fade overlay. Lives in the Bootstrap scene so it survives scene
// changes — it must be alive during BOTH the fade-out (old scene) and the
// fade-in (new scene).
//
// Setup: an object in Bootstrap with a Canvas (Screen Space - Overlay, high
// Sort Order), a full-screen black Image, and a CanvasGroup.
public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance { get; private set; }

    [SerializeField] private CanvasGroup fadeGroup;   // full-screen black overlay
    [SerializeField] private float fadeDuration = 0.4f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeGroup != null)
        {
            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = false;
        }
    }

    // Fade to black, load the scene (placing the player at spawnId), fade back in.
    public void TransitionToScene(string sceneName, string spawnId)
    {
        StartCoroutine(TransitionRoutine(sceneName, spawnId));
    }

    private IEnumerator TransitionRoutine(string sceneName, string spawnId)
    {
        // 1. Fade OUT (to black) in the current scene.
        yield return Fade(0f, 1f);

        // 2. Load the new scene while the screen is black.
        //    GameManager places the player at the matching spawn point.
        GameManager.Instance.LoadSceneWithSpawn(sceneName, spawnId);

        // 3. Let the new scene FULLY settle before revealing it. This is what
        //    kills the snap: the player gets placed, CameraTargetBinder runs,
        //    and Cinemachine's LateUpdate lands the camera on target — all
        //    while the screen is still black.
        yield return null;                     // new scene is up
        yield return new WaitForEndOfFrame();  // Cinemachine's LateUpdate has run
        yield return null;                     // one more frame to be safe

        // 4. Fade IN (from black) in the new scene.
        yield return Fade(1f, 0f);
    }

    private IEnumerator Fade(float from, float to)
    {
        if (fadeGroup == null) yield break;

        fadeGroup.blocksRaycasts = true;   // block input mid-transition
        float t = 0f;
        fadeGroup.alpha = from;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;   // unscaled: works even if the game is paused
            fadeGroup.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }

        fadeGroup.alpha = to;
        fadeGroup.blocksRaycasts = to > 0.5f;
    }
}