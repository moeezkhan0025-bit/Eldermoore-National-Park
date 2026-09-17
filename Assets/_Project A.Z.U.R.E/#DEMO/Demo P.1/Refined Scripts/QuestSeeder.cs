using UnityEngine;

// TEMP helper: starts one or more quests as active on scene load, so you can see
// them in the Journal before the NPC/QuestGiver flow is wired. Put on any object
// in a gameplay scene; drag your quest assets into Start Active. Delete later.
public class QuestSeeder : MonoBehaviour
{
    [SerializeField] private QuestDefinition[] startActive;

    void Start()
    {
        if (QuestManager.Instance == null) return;
        foreach (var q in startActive)
            QuestManager.Instance.StartQuest(q);
    }
}
