using System.Collections;
using UnityEngine;

/// <summary>
/// Runs once at application start before any other scene loads.
/// Handles first-time initialization, then hands off to Main Menu.
///
/// Attach to: Bootstrapper GameObject in the Bootstrap scene.
/// </summary>
public class Bootstrapper : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Optional delay in seconds before loading Main Menu. Useful for splash screens.")]
    [SerializeField] private float loadDelay = 0f;

    private void Awake()
    {
        Debug.Log("[Bootstrap] Initializing...");

        // First-time defaults — only writes if the key doesn't exist yet
        InitPlayerPrefsDefaults();
    }

    private IEnumerator Start()
    {
        Debug.Log("[Bootstrap] Systems ready. Loading Main Menu...");

        if (loadDelay > 0f)
            yield return new WaitForSeconds(loadDelay);

        SceneIndex.Load(SceneIndex.MAIN_MENU);
    }

    /// <summary>
    /// Writes default PlayerPrefs values on first ever launch.
    /// Safe to call every boot — PlayerPrefs.HasKey guards each value.
    /// </summary>
    private void InitPlayerPrefsDefaults()
    {
        SetDefaultFloat("Vol_Master", 1f);
        SetDefaultFloat("Vol_Music", 1f);
        SetDefaultFloat("Vol_SFX", 1f);
        SetDefaultFloat("Vol_Ambient", 0.5f);
        SetDefaultInt("Fullscreen", 1);
        SetDefaultInt("VSync", 1);
        SetDefaultInt("ShowHints", 1);
        SetDefaultInt("ScreenShake", 1);
        SetDefaultInt("AutoSave", 1);
        SetDefaultInt("Difficulty", 1);
        SetDefaultInt("GraphicsQuality", QualitySettings.GetQualityLevel());

        Debug.Log("[Bootstrap] PlayerPrefs defaults confirmed.");
    }

    private static void SetDefaultFloat(string key, float value)
    {
        if (!PlayerPrefs.HasKey(key)) PlayerPrefs.SetFloat(key, value);
    }

    private static void SetDefaultInt(string key, int value)
    {
        if (!PlayerPrefs.HasKey(key)) PlayerPrefs.SetInt(key, value);
    }
}