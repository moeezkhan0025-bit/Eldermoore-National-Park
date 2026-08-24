using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public SaveData CurrentSave { get; private set; }
    public int CurrentSlot { get; private set; } = -1;

    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string characterSelectScene = "CharacterSelect";
    [SerializeField] private string introCutsceneScene = "IntroCutscene";
    [SerializeField] private string rangerHQScene = "RangerHQ";   // set to your HQ scene's exact name
    [SerializeField] private string gameplayScene = "Game";

    private string pendingSpawnId;
    private bool pendingRestore;   // set when loading a save, so OnSceneLoaded restores position

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log($"[Saves] Save folder: {Application.persistentDataPath}"); // handy while testing
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    // ---------- DOORWAY SCENE CHANGES ----------
    public void LoadSceneWithSpawn(string sceneName, string spawnId)
    {
        pendingSpawnId = spawnId;
        SceneManager.LoadScene(sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // A) Loading from a save file: drop the player at their saved position.
        if (pendingRestore)
        {
            pendingRestore = false;
            var p = FindObjectOfType<MovementController>();
            if (p != null) PlacePlayer(p, CurrentSave.playerPosition);
            return; // we just loaded this scene — don't immediately re-save it
        }

        // B) Doorway transition: your original spawn-point logic.
        if (!string.IsNullOrEmpty(pendingSpawnId))
        {
            foreach (var sp in FindObjectsOfType<SpawnPoint>())
            {
                if (sp.Id == pendingSpawnId)
                {
                    var player = FindObjectOfType<MovementController>();
                    if (player != null) PlacePlayer(player, sp.transform.position);
                    break;
                }
            }
            pendingSpawnId = null;
        }

        // C) Autosave whenever we arrive in a real, playable scene.
        if (CurrentSlot >= 0 && IsSavableScene(scene.name))
            SaveCurrent();
    }

    // Shared helper: move the player, respecting physics if there's a Rigidbody2D.
    private void PlacePlayer(MovementController player, Vector3 pos)
    {
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) { rb.position = pos; rb.velocity = Vector2.zero; }
        else player.transform.position = pos;
    }

    // Menus / cutscenes aren't gameplay, so we don't autosave in them.
    private bool IsSavableScene(string name) =>
        name != mainMenuScene && name != characterSelectScene && name != introCutsceneScene;

    // ---------- NEW GAME ----------
    public void StartNewGame(int slot)
    {
        CurrentSlot = slot;
        CurrentSave = new SaveData();
        SceneManager.LoadScene(characterSelectScene);
    }

    public void ConfirmCharacter(string name, int classId)
    {
        CurrentSave.characterName = name;
        CurrentSave.characterClassId = classId;
        CurrentSave.sceneName = gameplayScene;
        SceneManager.LoadScene(introCutsceneScene);
    }

    public void EnterRangerHQ()
    {
        Debug.Log($"Loading Ranger HQ scene: '{rangerHQScene}'");
        SceneManager.LoadScene(rangerHQScene);
    }

    public void EnterGameplay() => SceneManager.LoadScene(gameplayScene);

    // ---------- CONTINUE / LOAD ----------
    public void Continue()
    {
        int slot = SaveSystem.GetMostRecentSlot();
        if (slot < 0) return;
        LoadGame(slot);
    }

    public void LoadGame(int slot)
    {
        var data = SaveSystem.Load(slot);
        if (data == null) return;
        CurrentSlot = slot;
        CurrentSave = data;
        pendingRestore = true;   // tell OnSceneLoaded to restore the saved position
        SceneManager.LoadScene(
            string.IsNullOrEmpty(data.sceneName) ? gameplayScene : data.sceneName);
    }

    // ---------- SAVING ----------
    // Captures the live world state, then writes it to the current slot.
    public void SaveCurrent()
    {
        if (CurrentSlot < 0 || CurrentSave == null) return;

        CurrentSave.sceneName = SceneManager.GetActiveScene().name;

        var player = FindObjectOfType<MovementController>();
        if (player != null)
            CurrentSave.playerPosition = player.transform.position;

        SaveSystem.Save(CurrentSlot, CurrentSave);
    }

    // ---------- QUIT ----------
    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}