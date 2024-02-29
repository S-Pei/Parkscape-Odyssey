using UnityEngine;

public class Quest
{
    // name of object to find
    private string label;
    private Texture2D referenceImage;
    private double[] featureVector;
    private bool isCompleted;

    public Quest(string label, Texture2D referenceImage, double[] featureVector)
    {
        this.label = label;
        this.referenceImage = referenceImage;
        this.featureVector = featureVector;
        this.isCompleted = false;
    }
}
