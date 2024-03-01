using UnityEngine;

public enum QuestType {
    FIND
}

public enum QuestStatus {
    NOT_STARTED,
    IN_PROGRESS,
    COMPLETED
}

public class Quest
{
    // name of object to find
    public string Label { get; private set; }
    public Texture2D ReferenceImage { get; private set; }
    public double[] FeatureVector { get; private set; }
    public QuestStatus QuestStatus { get; private set; }
    public QuestType QuestType { get; private set; }
    public int Progress { get; private set; }
    public int Target { get; private set; }

    public Quest(QuestType questType, string label, int target, Texture2D referenceImage)
    {
        QuestType = questType;
        Label = label;
        ReferenceImage = referenceImage;
        QuestStatus = QuestStatus.NOT_STARTED;
        Progress = 0;
        Target = target;
    }

    public void IncrementProgress() {
        if (Progress < Target)
            Progress++;
        if (Progress == Target)
            SetCompleted();
    }

    public void SetOngoing() {
        QuestStatus = QuestStatus.IN_PROGRESS;
    }

    public void SetCompleted() {
        QuestStatus = QuestStatus.COMPLETED;
    }

    public bool IsOnGoing() {
        return QuestStatus == QuestStatus.IN_PROGRESS;
    }

    public bool IsCompleted() {
        return QuestStatus == QuestStatus.COMPLETED;
    }

    public bool HasNotStarted() {
        return QuestStatus == QuestStatus.NOT_STARTED;
    }
}
