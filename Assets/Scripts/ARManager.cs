using System;
using System.Collections.Generic;
using Microsoft.Geospatial;
using Niantic.Lightship.AR.LocationAR;
using Niantic.Lightship.AR.PersistentAnchors;
using UnityEngine;

public class ARManager : MonoBehaviour
{
    public static ARManager selfReference;

    [SerializeField] private GameObject gameManagerObj;
    private GameManager gameManager;
    
    [SerializeField] private GameObject xrInteractionManager;
    
    [SerializeField] private GameObject xrOrigin;

    [SerializeField] private GameObject arCamera;

    [SerializeField] private GameObject semanticsRawImage;

    [SerializeField] private GameObject semanticsLabel;

    [SerializeField] private GameObject arEncounterSpawnManager;

    [SerializeField] private List<(LatLon latlon, ARLocation location)> arSpawnLocations = new();

    private Dictionary<string, LatLon> latlons = new() {
        {"AR Location (Kenway)", new LatLon(51.493553, -0.192372)},
        {"AR Location (Huxley)", new LatLon(51.498760, -0.179450)},
        {"AR Location (Feeding Fawn)", new LatLon(51.501621, -0.180658)},
        {"AR Location (Benny Hill)", new LatLon(51.500771, -0.180400)},
    };

    private ARLocationManager arLocationManager;

    private ARLocation activeLocation;

    private ObjectDetectionManager objectDetectionManager;

    private int checkLocationFreq = 100;
    private int currCheckLoctionFreq = 0;

    public static ARManager Instance {
        get {
            if (selfReference == null) {
                selfReference = new();
            }
            return selfReference;
        }
    }

    public void Awake() {
        selfReference = this;
    }

    public void Start() {
        arLocationManager = xrOrigin.GetComponent<ARLocationManager>();
        arLocationManager.enabled = true;
        // arLocationManager.locationTrackingStateChanged += LocationTrackedUpdated;

        gameManager = gameManagerObj.GetComponent<GameManager>();
        objectDetectionManager = GetComponent<ObjectDetectionManager>();

        ARLocation[] arLocations = arLocationManager.ARLocations;
        foreach (ARLocation arLocation in arLocations) {
            bool getRes = latlons.TryGetValue(arLocation.name, out LatLon latlon);
            if (getRes) {
                arSpawnLocations.Add((latlon, arLocation));
            } else {
                gameManager.LogTxt($"LatLon not found for {arLocation.name}");
            }
        }
    }

    // private void LocationTrackedUpdated(ARLocationTrackedEventArgs args) {
    //     var result = args.ARLocation;
    //     gameManager.LogTxt($"Location: {result.name}");
    // }

    public void Update() {
        if (currCheckLoctionFreq == 0) {
            LatLon latlon = GPSManager.Instance.GetLocation();

            double minDistance = 100000;
            ARLocation closestLocation = null;
            foreach ((LatLon locationLatLon, ARLocation location) in arSpawnLocations) {
                double distance = distanceToSpawnLocation(latlon, locationLatLon);
                if (distance < minDistance) {
                    minDistance = distance;
                    closestLocation = location;
                }
            }
            // gameManager.LogTxt("Closest location: " + closestLocation.name);
            if (activeLocation == null || activeLocation.name != closestLocation.name) {
                ARLocationManager locationManager = xrOrigin.GetComponent<ARLocationManager>();

                locationManager.StopTracking();
                locationManager.SetARLocations(closestLocation);
                locationManager.StartTracking();
                activeLocation = closestLocation;

                gameManager.LogTxt($"New active location: {activeLocation.name}");
            }

            currCheckLoctionFreq = checkLocationFreq;
        } else {
            currCheckLoctionFreq--;
        }
    }

    // public void ActivateTestLocation() {
    //     ARLocationManager locationManager = xrOrigin.GetComponent<ARLocationManager>();
    //     locationManager.StopTracking();
    //     locationManager.SetARLocations(arSpawnLocations[0].location);
    //     locationManager.StartTracking();
    //     gameManager.LogTxt("Test location activated.");
    // }

    // public void ActivateMuralLocation() {
    //     ARLocationManager locationManager = xrOrigin.GetComponent<ARLocationManager>();
    //     locationManager.StopTracking();
    //     locationManager.SetARLocations(arSpawnLocations[1].location);
    //     locationManager.StartTracking();
    //     gameManager.LogTxt("Mural location activated.");
    // }

    public void StartAR() {
        Debug.Log("Starting AR session.");
        arCamera.SetActive(true);
        semanticsRawImage.SetActive(true);
        semanticsLabel.SetActive(true);
        arEncounterSpawnManager.GetComponent<EncounterObjectManager>().SetARMode(true);
    }

    public void StopAR() {
        Debug.Log("Stopping AR session.");
        arCamera.SetActive(false);
        semanticsRawImage.SetActive(false);
        semanticsLabel.SetActive(false);
        arEncounterSpawnManager.GetComponent<EncounterObjectManager>().SetARMode(false);
    }

    private ARLocation[] GetAllSpawnLocations() {
        return arLocationManager.ARLocations;
    }

    private double distanceToSpawnLocation(LatLon latlon, LatLon locationLatLon) {
        double distance = Math.Sqrt(Math.Pow(latlon.LatitudeInDegrees - locationLatLon.LatitudeInDegrees, 2) + Math.Pow(latlon.LongitudeInDegrees - locationLatLon.LongitudeInDegrees, 2));
        // Debug.Log("Distance to spawn location: " + distance);
        return distance;
    }

    public Texture2D TakeScreenCapture() {
        Debug.Log("Taking a screen capture.");
        return ScreenCapture.CaptureScreenshotAsTexture();
    }

    // Onclick button for taking images
    public void TakeQuestImage() {
        Texture2D screenCapture = TakeScreenCapture();
        gameManager.LogTxt("Screen capture taken.");
        // Attempt Basic Quests
        List<string> labels = objectDetectionManager.GetLabels();
        gameManager.LogTxt("Labels: " + string.Join(", ", labels));
        BasicQuest basicQuest = QuestManager.Instance.CheckBasicQuests(labels);
        if (basicQuest != null) {
            gameManager.LogTxt("Basic quest :" + basicQuest.Label + " progress: " + basicQuest.Progress);
            if (basicQuest.IsCompleted()) {
                gameManager.LogTxt("Basic quest completed.");
            }
            
        } else {
            gameManager.LogTxt("No basic quest progress.");
            // Attempt Location Quests if basic quests not fulfilled
            LocationQuest locationQuest = QuestManager.Instance.CheckLocationQuests(screenCapture);
            if (locationQuest != null) {
                gameManager.LogTxt("Location quest :" + locationQuest.Label + " progress: " + locationQuest.Progress);
            } else {
                gameManager.LogTxt("No location quest progress.");
            }
        }
        // FUTURE: Save images.
    }

    // NOT USED FOR NOW
    // public void SpawnAllLocations() {
    //     foreach (GameObject arLocation in arSpawnLocations) {
    //         GameObject spawnedARLocation = Instantiate(arLocation, xrOrigin.transform);
    //         spawnedARLocation.transform.SetParent(xrOrigin.transform);
    //     }
    // }

    // NOT USED FOR NOW
    // public void DeSpawnAllLocations() {
    //     var i = 0;
    //     foreach (Transform child in xrOrigin.transform) {
    //         if (i != 0) {
    //             Destroy(child.gameObject);
    //         }
    //         i++;
    //     }
    // }
}
