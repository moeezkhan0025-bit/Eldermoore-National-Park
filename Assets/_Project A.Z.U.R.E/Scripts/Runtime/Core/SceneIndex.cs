using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Single source of truth for scene references.
///
/// YOUR CURRENT BUILD ORDER:
///   0 → Bootstrap
///   1 → Persistent Systems
///   2 → Main Menu
///   3 → SaveFileSelect
///   4 → Character Select
///   5 → Demo Showcase
///   6 → Settings         ← ADD THIS to Build Settings
/// </summary>
public static class SceneIndex
{
    // ── Build Index Constants ─────────────────────────────────────────────────
    // Only update these numbers if you reorder scenes in Build Settings.

    public const int BOOTSTRAP          = 0;
    public const int PERSISTENT_SYSTEMS = 1;
    public const int MAIN_MENU          = 2;
    public const int SAVE_FILE_SELECT   = 3;
    public const int CHARACTER_SELECT   = 4;
    public const int DEMO_SHOWCASE      = 5;
    public const int SETTINGS           = 6;

    // ── Dynamic Name Lookup ───────────────────────────────────────────────────

    /// <summary>
    /// Derives the scene name from the build index at runtime by reading
    /// Unity's registered scene path. Rename a scene in the Editor and
    /// this automatically reflects — no string changes needed anywhere.
    /// </summary>
    public static string NameOf(int buildIndex)
    {
        string path = SceneUtility.GetScenePathByBuildIndex(buildIndex);

        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError($"[SceneIndex] No scene at build index {buildIndex}. " +
                           "Check File → Build Settings.");
            return string.Empty;
        }

        string nameWithExtension = System.IO.Path.GetFileName(path);
        return System.IO.Path.GetFileNameWithoutExtension(nameWithExtension);
    }

    // ── Convenience Properties ────────────────────────────────────────────────

    public static string Bootstrap         => NameOf(BOOTSTRAP);
    public static string PersistentSystems => NameOf(PERSISTENT_SYSTEMS);
    public static string MainMenu          => NameOf(MAIN_MENU);
    public static string SaveFileSelect    => NameOf(SAVE_FILE_SELECT);
    public static string CharacterSelect   => NameOf(CHARACTER_SELECT);
    public static string DemoShowcase      => NameOf(DEMO_SHOWCASE);
    public static string Settings          => NameOf(SETTINGS);

    // ── Load Helpers ─────────────────────────────────────────────────────────

    public static void Load(int buildIndex,
        LoadSceneMode mode = LoadSceneMode.Single)
    {
        string name = NameOf(buildIndex);
        if (!string.IsNullOrEmpty(name))
            SceneManager.LoadScene(name, mode);
    }

    public static AsyncOperation LoadAsync(int buildIndex,
        LoadSceneMode mode = LoadSceneMode.Single)
    {
        string name = NameOf(buildIndex);
        if (string.IsNullOrEmpty(name)) return null;
        return SceneManager.LoadSceneAsync(name, mode);
    }

    // ── Editor Validation ─────────────────────────────────────────────────────

#if UNITY_EDITOR
    [MenuItem("Tools/Scene Index/Validate All Scenes")]
    private static void ValidateInEditor()
    {
        int count = SceneManager.sceneCountInBuildSettings;
        Debug.Log($"[SceneIndex] {count} scene(s) in Build Settings:");
        for (int i = 0; i < count; i++)
            Debug.Log($"  [{i}] {NameOf(i)}");
    }
#endif
}
