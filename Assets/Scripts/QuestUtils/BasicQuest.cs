using UnityEngine;

public class BasicQuest : Quest {
    public BasicQuest(QuestType questType, string label, int target, Texture2D referenceImage) 
        : base(questType, label, target, referenceImage) {}

    public override string ToString() {
        return QuestType switch {
            QuestType.FIND => "Find " + Target + " " + Label + ((Target > 1) ? "s" : ""),
            _ => "Unknown Quest Type",
        };
    }
}