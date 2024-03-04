using Microsoft.Geospatial;
using UnityEngine;

public class LocationQuest : Quest {
    public LatLon Location { get; private set; }

    public LocationQuest(string label, Texture2D referenceImage, LatLon location) : base(label, referenceImage) {
        Location = location;
    }
}
