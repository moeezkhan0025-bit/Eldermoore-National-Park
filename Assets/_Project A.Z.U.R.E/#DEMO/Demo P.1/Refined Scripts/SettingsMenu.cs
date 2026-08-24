using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using System.Collections.Generic;

// SettingsMenu — the bridge between the settings UI, the systems it controls,
// and PlayerPrefs (so choices are remembered between sessions).
//
// WHERE IT GOES: on your Canvas (always active), so it can load & apply saved
// settings the instant the game starts. It does NOT open or close the panel —
// that's MainMenuController's job.
//
// THE PATTERN, repeated for every setting:
//   • Start()      reads the saved value, shows it, applies it   (the "load" beat)
//   • SetXxx(...)  applies the value live AND saves it            (the "change" beat)
// The public SetXxx methods are what you wire each control's event to.
public class SettingsMenu : MonoBehaviour
{
    [Header("Audio  (leave the mixer empty until you add audio)")]
    [SerializeField] private AudioMixer audioMixer;   // optional for now
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Video")]
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle vsyncToggle;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown qualityDropdown;

    // PlayerPrefs keys — the label each value is filed under on disk.
    const string K_MASTER     = "MasterVolume";
    const string K_MUSIC      = "MusicVolume";
    const string K_SFX        = "SFXVolume";
    const string K_FULLSCREEN = "Fullscreen";
    const string K_VSYNC      = "Vsync";
    const string K_RES        = "ResolutionIndex";
    const string K_QUALITY    = "QualityLevel";

    private Resolution[] resolutions;

    // ===============================================================
    //  START  —  the "load" beat, run once.
    // ===============================================================
    void Start()
    {
        BuildResolutionDropdown();
        BuildQualityDropdown();
        LoadSettings();
    }

    void LoadSettings()
    {
        // 1) Read each saved value. The 2nd argument is the default used the
        //    very first time, before the player has ever changed anything.
        float master   = PlayerPrefs.GetFloat(K_MASTER, 0.75f);
        float music    = PlayerPrefs.GetFloat(K_MUSIC, 0.75f);
        float sfx      = PlayerPrefs.GetFloat(K_SFX, 0.75f);
        bool fullscreen = PlayerPrefs.GetInt(K_FULLSCREEN, 1) == 1;
        bool vsync     = PlayerPrefs.GetInt(K_VSYNC, 1) == 1;
        int quality    = PlayerPrefs.GetInt(K_QUALITY, QualitySettings.GetQualityLevel());
        int resIndex   = PlayerPrefs.GetInt(K_RES, resolutions.Length - 1);

        // 2) Show them in the controls WITHOUT firing their change events.
        //    (SetValueWithoutNotify avoids a control re-triggering SetXxx here.)
        if (masterSlider)     masterSlider.SetValueWithoutNotify(master);
        if (musicSlider)      musicSlider.SetValueWithoutNotify(music);
        if (sfxSlider)        sfxSlider.SetValueWithoutNotify(sfx);
        if (fullscreenToggle) fullscreenToggle.SetIsOnWithoutNotify(fullscreen);
        if (vsyncToggle)      vsyncToggle.SetIsOnWithoutNotify(vsync);
        if (qualityDropdown)  { qualityDropdown.SetValueWithoutNotify(quality); qualityDropdown.RefreshShownValue(); }
        if (resolutionDropdown){ resolutionDropdown.SetValueWithoutNotify(resIndex); resolutionDropdown.RefreshShownValue(); }

        // 3) Apply them to the real systems.
        SetMasterVolume(master);
        SetMusicVolume(music);
        SetSFXVolume(sfx);
        SetFullscreen(fullscreen);
        SetVsync(vsync);
        SetQuality(quality);
        SetResolution(resIndex);
    }

    // ===============================================================
    //  AUDIO  —  the "change" beat. Wire each slider's On Value Changed
    //  to the matching method here. Sliders: Min 0.0001, Max 1.
    //  Volume is logarithmic, so we convert 0..1 to decibels.
    //  audioMixer is optional: with none assigned, values still save.
    // ===============================================================
    public void SetMasterVolume(float value)
    {
        if (audioMixer) audioMixer.SetFloat(K_MASTER, LinearToDb(value));
        PlayerPrefs.SetFloat(K_MASTER, value);
    }

    public void SetMusicVolume(float value)
    {
        if (audioMixer) audioMixer.SetFloat(K_MUSIC, LinearToDb(value));
        PlayerPrefs.SetFloat(K_MUSIC, value);
    }

    public void SetSFXVolume(float value)
    {
        if (audioMixer) audioMixer.SetFloat(K_SFX, LinearToDb(value));
        PlayerPrefs.SetFloat(K_SFX, value);
    }

    float LinearToDb(float value) => Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;

    // ===============================================================
    //  VIDEO  —  the "change" beat for the toggles and dropdowns.
    // ===============================================================
    public void SetFullscreen(bool on)
    {
        Screen.fullScreen = on;
        PlayerPrefs.SetInt(K_FULLSCREEN, on ? 1 : 0);
    }

    public void SetVsync(bool on)
    {
        QualitySettings.vSyncCount = on ? 1 : 0;
        PlayerPrefs.SetInt(K_VSYNC, on ? 1 : 0);
    }

    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt(K_QUALITY, index);
    }

    public void SetResolution(int index)
    {
        if (resolutions == null || index < 0 || index >= resolutions.Length) return;
        Resolution r = resolutions[index];
        Screen.SetResolution(r.width, r.height, Screen.fullScreenMode, r.refreshRateRatio);
        PlayerPrefs.SetInt(K_RES, index);
    }

    // ===============================================================
    //  Dropdown setup — fills the options lists at startup.
    // ===============================================================
    void BuildResolutionDropdown()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        for (int i = 0; i < resolutions.Length; i++)
        {
            // refreshRateRatio is Unity 2022+. On older versions use resolutions[i].refreshRate (an int).
            int hz = Mathf.RoundToInt((float)resolutions[i].refreshRateRatio.value);
            options.Add($"{resolutions[i].width} x {resolutions[i].height} @ {hz}Hz");
        }
        resolutionDropdown.AddOptions(options);
    }

    void BuildQualityDropdown()
    {
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
    }
}
