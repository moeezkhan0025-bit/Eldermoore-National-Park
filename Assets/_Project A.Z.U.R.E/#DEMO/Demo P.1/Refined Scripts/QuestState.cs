using System.Collections.Generic;

// Per-player progress for one quest (this is what gets saved, separate from the
// shared QuestDefinition asset).
[System.Serializable]
public class QuestState
{
    public string questId;                              // matches QuestDefinition.id
    public List<int> completedSteps = new List<int>();  // finished step indices
    public bool completed;                              // whole quest done

    public bool IsStepDone(int index) => completedSteps.Contains(index);
}
