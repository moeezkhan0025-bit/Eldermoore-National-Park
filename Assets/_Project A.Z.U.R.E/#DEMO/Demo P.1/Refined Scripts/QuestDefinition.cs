using System.Collections.Generic;
using UnityEngine;

// One step/objective within a quest (content only — no "done" flag; completion
// is per-player runtime state).
[System.Serializable]
public class QuestStep
{
    [TextArea] public string description;   // e.g. "Talk to Captain Vaan"
}

// A quest's fixed content, authored as an asset.
//   Create > Quests > Quest Definition
[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Quest Definition")]
public class QuestDefinition : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Stable unique id used in save files. Set once, never change.")]
    public string id;
    public string title;
    [TextArea] public string description;

    [Tooltip("Main quest or side quest (for journal grouping later).")]
    public bool isMainQuest = true;

    [Header("Steps (optional for whole-quest completion)")]
    public List<QuestStep> steps = new List<QuestStep>();
}
