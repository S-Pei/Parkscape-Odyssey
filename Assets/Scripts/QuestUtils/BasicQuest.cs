using UnityEngine;

public class BasicQuest : Quest {
    public BasicQuest(QuestType questType, string label, Texture2D referenceImage, double[] featureVector) 
        : base(questType, label, referenceImage, featureVector) {}
}