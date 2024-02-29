using Microsoft.Geospatial;
using UnityEngine;

public class BasicQuest : Quest {
    public BasicQuest(string label, Texture2D referenceImage, float[] featureVector) 
        : base(label, referenceImage, featureVector) {}
}