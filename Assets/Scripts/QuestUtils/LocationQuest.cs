using Microsoft.Geospatial;
using UnityEngine;

public class LocationQuest : Quest {
    private LatLon location;

    public LocationQuest(string label, Texture2D referenceImage, float[] featureVector, LatLon location) : base(label, referenceImage, featureVector)
    {
        this.location = location;
    }
}