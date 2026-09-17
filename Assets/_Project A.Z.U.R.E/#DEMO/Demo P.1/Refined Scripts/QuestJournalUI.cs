using System.Text;
using TMPro;
using UnityEngine;

// Fills the Journal panel with quests, split into Active and Completed.
// Put on JournalPanel; assign one TMP text element it writes into.
public class QuestJournalUI : MonoBehaviour
{
    [Tooltip("The TMP text element that displays the quest list.")]
    [SerializeField] private TMP_Text questListText;

    void OnEnable()
    {
        Refresh();
        if (QuestManager.Instance != null)
            QuestManager.Instance.QuestsChanged += Refresh;
    }

    void OnDisable()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.QuestsChanged -= Refresh;
    }

    public void Refresh()
    {
        if (questListText == null) return;

        var qm = QuestManager.Instance;
        if (qm == null) { questListText.text = "<i>No quest system found.</i>"; return; }

        var sb = new StringBuilder();

        sb.AppendLine("<b>ACTIVE</b>");
        if (qm.Active.Count == 0) sb.AppendLine("<i>  None</i>");
        else foreach (var q in qm.Active) sb.AppendLine($"  \u2022 {q.title}");

        sb.AppendLine();

        sb.AppendLine("<b>COMPLETED</b>");
        if (qm.Completed.Count == 0) sb.AppendLine("<i>  None</i>");
        else foreach (var q in qm.Completed) sb.AppendLine($"  <s>{q.title}</s>");

        questListText.text = sb.ToString();
    }
}
