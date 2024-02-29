using UnityEngine;

public enum QuestType {
    FIND
}

public class Quest
{
    // name of object to find
    private string label;
    private Texture2D referenceImage;
    private float[] featureVector;
    private bool isCompleted;
    private QuestType questType;

    public Quest(QuestType questType, string label, Texture2D referenceImage, float[] featureVector)
    {
        this.questType = questType;
        this.label = label;
        this.referenceImage = referenceImage;
        this.featureVector = featureVector;
        this.isCompleted = false;
    }
}
