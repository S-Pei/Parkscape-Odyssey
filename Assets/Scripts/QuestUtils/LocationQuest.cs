using Microsoft.Geospatial;
using UnityEngine;

public class LocationQuest : Quest {
    public LatLon Location { get; private set; }

    public LocationQuest(QuestType questType, string label, Texture2D referenceImage, double[] featureVector, LatLon location) 
        : base(questType, label, 1, referenceImage, featureVector) {
        Location = location;
    }

    public override string ToString() {
        return QuestType switch {
            QuestType.FIND => "Find the " + Label,
            _ => "Unknown Quest Type",
        };
    }
}