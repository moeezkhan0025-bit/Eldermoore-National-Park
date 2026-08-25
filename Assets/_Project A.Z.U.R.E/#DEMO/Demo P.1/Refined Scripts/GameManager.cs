using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public SaveData CurrentSave { get; private set; }
    public int CurrentSlot { get; private set; } = -1;

    [Header("Scenes")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string characterSelectScene = "CharacterSelect";
    [SerializeField] private string introCutsceneScene = "IntroCutscene";
    [SerializeField] private string rangerHQScene = "RangerHQ";
    [SerializeField] private string gameplayScene = "Game";

    [Header("Player")]
    [Tooltip("The player prefab (with PlayerPersistence). Spawned once on entering the first gameplay scene.")]
    [SerializeField] private GameObject playerPrefab;
    [Tooltip("Optional fallback spawn position if a scene has no matching SpawnPoint.")]
    [SerializeField] private Vector3 defaultSpawn = Vector3.zero;

    private string pendingSpawnId;
    private bool pendingRestore;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log($"[Saves] Save folder: {Application.persistentDataPath}");
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
        bool gameplay = IsGameplayScene(scene.name);

        // Gameplay scenes need a player. Spawn one if it doesn't already exist.
        if (gameplay) EnsurePlayer();

        // A) Loading from a save: drop the player at the saved position.
        if (pendingRestore)
        {
            pendingRestore = false;
            var p = PlayerPersistence.Instance;
            if (p != null) PlacePlayer(p.gameObject, CurrentSave.playerPosition);
            return;
        }

        // B) Doorway transition: place at the matching spawn point.
        if (!string.IsNullOrEmpty(pendingSpawnId))
        {
            var player = PlayerPersistence.Instance;
            foreach (var sp in FindObjectsOfType<SpawnPoint>())
            {
                if (sp.Id == pendingSpawnId)
                {
                    if (player != null) PlacePlayer(player.gameObject, sp.transform.position);
                    break;
                }
            }
            pendingSpawnId = null;
        }

        // C) Autosave on arriving in a real gameplay scene.
        if (CurrentSlot >= 0 && gameplay)
            SaveCurrent();
    }

    // Spawn the persistent player if none exists yet. Runs once — after that the
    // player survives via DontDestroyOnLoad and PlayerPersistence.Instance is set.
    private void EnsurePlayer()
    {
        if (PlayerPersistence.Instance != null) return;   // already have one
        if (playerPrefab == null)
        {
            Debug.LogError("[GameManager] Player Prefab not assigned — can't spawn the player.", this);
            return;
        }
        Instantiate(playerPrefab, defaultSpawn, Quaternion.identity);
    }

    private void PlacePlayer(GameObject player, Vector3 pos)
    {
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) { rb.position = pos; rb.velocity = Vector2.zero; }
        else player.transform.position = pos;
    }

    // Gameplay = anything that isn't a menu or cutscene.
    private bool IsGameplayScene(string name) =>
        name != mainMenuScene &&
        name != characterSelectScene &&
        name != introCutsceneScene &&
        name != "Bootstrap";

    // Kept for save-eligibility (same set as gameplay here).
    private bool IsSavableScene(string name) => IsGameplayScene(name);

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
        LoadSceneWithSpawn(rangerHQScene, "HQ_Entry");
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
        pendingRestore = true;
        SceneManager.LoadScene(
            string.IsNullOrEmpty(data.sceneName) ? gameplayScene : data.sceneName);
    }

    // ---------- SAVING ----------
    public void SaveCurrent()
    {
        if (CurrentSlot < 0 || CurrentSave == null) return;

        CurrentSave.sceneName = SceneManager.GetActiveScene().name;

        var player = PlayerPersistence.Instance;
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