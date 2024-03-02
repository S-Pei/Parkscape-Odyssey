using UnityEngine;
using Microsoft.Geospatial;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour
{
    private static QuestManager instance;
    private GPSManager gpsManager;

    public static QuestManager Instance { 
        get {
            if (instance == null) {
                // To make sure that script is persistent across scenes
                GameObject go = new GameObject("QuestManager");
                instance = go.AddComponent<QuestManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private void Start()
    {
        gpsManager = GPSManager.Instance;
        List<Texture2D> referenceImages = new();
        referenceImages.Add(Resources.Load<Texture2D>("Assets/Resources/peter_pan_test_img.jpeg"));
        referenceImages.Add(Resources.Load<Texture2D>("Assets/Resources/albert_memorial_test.jpeg"));
        referenceImages.Add(Resources.Load<Texture2D>("Assets/Resources/speke-monument.jpg"));
        GameState.Instance.InitialiseQuests(referenceImages);
    }

    private void Update()
    {
        // Add update logic here
    }

    public void GetNextLocationQuest() {
        LatLon currentLocation = gpsManager.GetLocation();
        double minDistance = double.MaxValue;
        LocationQuest nearestLocationQuest = null;
        // Get nearest unattempted location quest
        for (int i = 0; i < GameState.Instance.locationQuests.Count; i++) {
            LocationQuest locationQuest = GameState.Instance.locationQuests[i];
            if (locationQuest.HasNotStarted()) {
                double distance = 
                    MapManager.DistanceBetweenCoordinates(
                        currentLocation.LatitudeInDegrees, currentLocation.LongitudeInDegrees, 
                        locationQuest.Location.LatitudeInDegrees, locationQuest.Location.LongitudeInDegrees);
                if (distance < minDistance) {
                    minDistance = distance;
                    nearestLocationQuest = locationQuest;
                }
            }
        }
        if (nearestLocationQuest != null) {
            // Add quest to ongoing quests
            nearestLocationQuest.SetOngoing();
            Debug.Log("Adding quest: " + nearestLocationQuest.ToString());
        } else {
            // No more location quests
            Debug.Log("No more location quests");
        }
    }

    // ----------------------------- REWARDS ------------------------------
    // Checks if any basic quests have been completed.
    public BasicQuest CheckBasicQuests(List<string> labels) {
        foreach (BasicQuest quest in GameState.Instance.basicQuests) {
            if (quest.IsOnGoing() && labels.Contains(quest.Label)) {
                quest.IncrementProgress();
                return quest;
            }
        }
        return null;
    }

    // Checks if any location quests have been completed, returns a quest if progressed or completed.
    public LocationQuest CheckLocationQuests(Texture2D image) {
        foreach (LocationQuest quest in GameState.Instance.locationQuests) {
            // Only one ongoing location quest at a time
            if (quest.IsOnGoing() && quest.AttemptQuest(image)) {
                if (quest.IsCompleted()) {
                    GetNextLocationQuest();
                }
                return quest;
            }
        }
        return null;
    }
}
