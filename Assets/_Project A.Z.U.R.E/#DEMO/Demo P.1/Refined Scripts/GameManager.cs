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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
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
        if (string.IsNullOrEmpty(pendingSpawnId)) return;

        foreach (var sp in FindObjectsOfType<SpawnPoint>())
        {
            if (sp.Id == pendingSpawnId)
            {
                var player = FindObjectOfType<MovementController>();
                if (player != null)
                {
                    var rb = player.GetComponent<Rigidbody2D>();
                    if (rb != null) { rb.position = sp.transform.position; rb.velocity = Vector2.zero; }
                    else player.transform.position = sp.transform.position;
                }
                break;
            }
        }
        pendingSpawnId = null;
    }

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
        SceneManager.LoadScene(
            string.IsNullOrEmpty(data.sceneName) ? gameplayScene : data.sceneName);
    }

    public void SaveCurrent()
    {
        if (CurrentSlot >= 0 && CurrentSave != null)
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