using UnityEngine;
using Microsoft.Geospatial;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour
{
    private static QuestManager instance;
    private GPSManager gpsManager;
    private List<LocationQuest> availableLocationQuests;

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
        availableLocationQuests = QuestFactory.CreateInitialLocationQuests();
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
    public bool CheckBasicQuests(string label) {
        foreach (BasicQuest quest in GameState.Instance.basicQuests) {
            if (quest.IsOnGoing() && quest.Label == label) {
                quest.IncrementProgress();
                if (quest.IsCompleted()) {
                    return true;
                }
                return false;
            }
        }
        return false;
    }

    // Checks if any location quests have been completed.
    public bool CheckLocationQuests(Texture2D image) {
        foreach (LocationQuest quest in GameState.Instance.locationQuests) {
            // Only one ongoing location quest at a time
            if (quest.IsOnGoing() && quest.AttemptQuest(image)) {
                if (quest.IsCompleted()) {
                    return true;
                }
                return false;
            }
        }
        return false;
    }
}
