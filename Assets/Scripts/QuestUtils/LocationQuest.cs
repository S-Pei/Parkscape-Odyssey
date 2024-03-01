using Microsoft.Geospatial;
using UnityEngine;
using System.Linq;

public class LocationQuest : Quest {
    public LatLon Location { get; private set; }

    public LocationQuest(QuestType questType, string label, Texture2D referenceImage, LatLon location) 
        : base(questType, label, 1, referenceImage) {
        Location = location;
    }

    public override string ToString() {
        return QuestType switch {
            QuestType.FIND => "Find the " + Label,
            _ => "Unknown Quest Type",
        };
    }

    public void AttemptQuest(Texture2D image) {
        if (ImageIsCorrect(image)) {
            IncrementProgress();
        }
    }

    // Check if the image taken is the correct object
    public bool ImageIsCorrect(Texture2D image) {
        string[] searchResults = VecSearchManager.Instance.ClassifyImage(image);
        return searchResults.Contains(Label);
    }
}