using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

// Plays an MP4 intro video, then hands off to Ranger HQ.
public class CutsceneController : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private Button skipButton;

    private bool finished;
    private bool started;

    private void Start()
    {
        if (skipButton != null) skipButton.onClick.AddListener(Skip);

        if (videoPlayer != null)
        {
            videoPlayer.isLooping = false;          // make sure it can end
            videoPlayer.loopPointReached += OnVideoFinished;  // keep as a backup
            videoPlayer.Play();
        }
        else
        {
            Finish();
        }
    }

    private void Update()
    {
        if (finished || videoPlayer == null) return;

        // Mark that playback has actually begun.
        if (videoPlayer.isPlaying) started = true;

        // Once it has played and reached (near) the end, finish.
        if (started && videoPlayer.frameCount > 0 &&
            videoPlayer.frame >= (long)videoPlayer.frameCount - 1)
        {
            Finish();
        }
    }

    private void OnVideoFinished(VideoPlayer vp) => Finish();

    public void Skip() => Finish();

    private void Finish()
    {
        if (finished) return;
        finished = true;
        Debug.Log("Cutscene finished — calling EnterRangerHQ");
        GameManager.Instance.EnterRangerHQ();
    }
}