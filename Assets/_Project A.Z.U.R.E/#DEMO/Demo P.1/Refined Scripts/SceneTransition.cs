using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// One persistent fader that loads scenes behind a black fade.
// Put this on a GameObject in your FIRST scene with a full-screen black UI.
public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }
    public static string PendingSpawnId;   // read by SceneEntryPoint in the next scene

    [Tooltip("Full-screen black Image's CanvasGroup. Start at alpha 0.")]
    [SerializeField] CanvasGroup fade;
    [SerializeField] float fadeTime = 0.4f;
    [SerializeField] float holdBlack = 0.1f;

    bool busy;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (fade == null)
            Debug.LogError($"[{name}] SceneTransition has no fade CanvasGroup assigned.", this);
    }

    public void Go(string sceneName, string spawnId)
    {
        if (busy) return;
        PendingSpawnId = spawnId;
        StartCoroutine(Routine(sceneName));
    }

    IEnumerator Routine(string sceneName)
    {
        busy = true;
        yield return Fade(1f);                 // to black
        yield return new WaitForSeconds(holdBlack);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone) yield return null;

        yield return null;                     // let the new scene place the player this frame
        yield return Fade(0f);                 // back in
        busy = false;
    }

    IEnumerator Fade(float target)
    {
        if (fade == null) yield break;
        fade.blocksRaycasts = true;
        float start = fade.alpha, t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;       // unscaled so it works even if you pause time
            fade.alpha = Mathf.Lerp(start, target, t / fadeTime);
            yield return null;
        }
        fade.alpha = target;
        fade.blocksRaycasts = target > 0.5f;
    }
}
