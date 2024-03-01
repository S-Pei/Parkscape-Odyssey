using UnityEngine;

public enum QuestType {
    FIND
}

public class Quest
{
    // name of object to find
    public string Label { get; private set; }
    public Texture2D ReferenceImage { get; private set; }
    public double[] FeatureVector { get; private set; }
    public bool IsCompleted { get; private set; }
    public QuestType QuestType { get; private set; }
    public int Progress { get; private set; }
    public int Target { get; private set; }

    public Quest(QuestType questType, string label, int target, Texture2D referenceImage, double[] featureVector)
    {
        QuestType = questType;
        Label = label;
        ReferenceImage = referenceImage;
        FeatureVector = featureVector;
        IsCompleted = false;
        Progress = 0;
        Target = target;
    }

    public void IncrementProgress() {
        if (Progress < Target)
            Progress++;
        if (Progress == Target)
            IsCompleted = true;
    }
}
