using Microsoft.Geospatial;
using UnityEngine;
using System;

[Serializable]
public class LocationQuest : Quest {
    public QuestLocation Location {
        get => _location;
        private set => _location = value;
    }
    
    [SerializeField]
    private QuestLocation _location;

    public LocationQuest(string label, Texture2D referenceImage, LatLon location) : base(label, referenceImage) {
        Location = new QuestLocation(location);
    }
}
