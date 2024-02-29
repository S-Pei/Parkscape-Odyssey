using Microsoft.Geospatial;
using UnityEngine;

public class LocationQuest : Quest {
    private LatLon location;

    public LocationQuest(QuestType questType, string label, Texture2D referenceImage, float[] featureVector, LatLon location) 
        : base(questType, label, referenceImage, featureVector)
    {
        this.location = location;
    }
}