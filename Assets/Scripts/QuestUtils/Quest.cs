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

    public Quest(QuestType questType, string label, Texture2D referenceImage, double[] featureVector)
    {
        QuestType = questType;
        Label = label;
        ReferenceImage = referenceImage;
        FeatureVector = featureVector;
        IsCompleted = false;
    }
}
