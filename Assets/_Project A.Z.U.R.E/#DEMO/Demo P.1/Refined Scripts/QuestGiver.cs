using UnityEngine;

// An NPC/object that gives a quest when interacted with (Triangle).
// Put on the NPC alongside a trigger Collider2D; assign the quest to grant.
public class QuestGiver : Interactable
{
    [SerializeField] private QuestDefinition questToGive;
    [SerializeField] private bool onlyOnce = true;

    private bool given;

    public override void Interact(GameObject player)
    {
        if (questToGive == null) { Debug.LogWarning($"[{name}] No quest assigned.", this); return; }
        if (onlyOnce && given) return;
        if (QuestManager.Instance == null) { Debug.LogError("[QuestGiver] No QuestManager in scene.", this); return; }

        QuestManager.Instance.StartQuest(questToGive);
        given = true;
    }
}
