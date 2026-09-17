using System;
using System.Collections.Generic;
using UnityEngine;

// Runtime quest tracking. Put this on its own object in the BOOTSTRAP scene
// (persists like GameManager/MenuSystem). Whole-quest completion model.
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    private readonly List<QuestDefinition> active = new List<QuestDefinition>();
    private readonly List<QuestDefinition> completed = new List<QuestDefinition>();

    public event Action QuestsChanged;   // the Journal UI subscribes to this

    public IReadOnlyList<QuestDefinition> Active => active;
    public IReadOnlyList<QuestDefinition> Completed => completed;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void StartQuest(QuestDefinition quest)
    {
        if (quest == null) return;
        if (active.Contains(quest) || completed.Contains(quest)) return;
        active.Add(quest);
        Debug.Log($"[Quest] Started: {quest.title}");
        QuestsChanged?.Invoke();
    }

    public void CompleteQuest(QuestDefinition quest)
    {
        if (quest == null) return;
        if (!active.Remove(quest)) return;
        completed.Add(quest);
        Debug.Log($"[Quest] Completed: {quest.title}");
        QuestsChanged?.Invoke();
    }

    public bool IsActive(QuestDefinition quest) => active.Contains(quest);
    public bool IsCompleted(QuestDefinition quest) => completed.Contains(quest);

    // Complete by id, for triggers that only know the string.
    public void CompleteQuestById(string questId)
    {
        for (int i = 0; i < active.Count; i++)
            if (active[i].id == questId) { CompleteQuest(active[i]); return; }
    }
}
