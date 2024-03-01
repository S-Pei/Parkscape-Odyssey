using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Geospatial;
using Niantic.Lightship.AR.LocationAR;
using UnityEngine;

public class ARManager : MonoBehaviour
{
    public static ARManager selfReference;

    [SerializeField]
    private GameObject gameManagerObj;
    private GameManager gameManager;
    
    [SerializeField]
    private GameObject xrInteractionManager;
    
    [SerializeField]
    private GameObject xrOrigin;

    [SerializeField]
    private GameObject arCamera;

    [SerializeField]
    private List<(LatLon latlon, ARLocation location)> arSpawnLocations = new();

    private List<LatLon> latlons = new() {
        new LatLon(51.493553, -0.192372),
        new LatLon(51.493492, -0.192765),
        new LatLon(51.494637, -0.192280),
        new LatLon(51.498760, -0.179450)
    };

    private ARLocationManager arLocationManager;

    private ARLocation activeLocation;

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
        gameManager = gameManagerObj.GetComponent<GameManager>();

        ARLocation[] arLocations = arLocationManager.ARLocations;
        int i = 0;
        foreach (ARLocation arLocation in arLocations) {
            arSpawnLocations.Add((latlons[i], arLocation));
            i ++;
        }
    }

    public void Update() {
        if (currCheckLoctionFreq == 0) {
            LatLon latlon = GPSManager.Instance.GetLocation();
            // gameManager.LogTxt("Lat: " + latlon.LatitudeInDegrees + " Lon: " + latlon.LongitudeInDegrees);
            Debug.Log("Lat: " + latlon.LatitudeInDegrees + " Lon: " + latlon.LongitudeInDegrees);

            double minDistance = 100000;
            ARLocation closestLocation = null;
            foreach ((LatLon locationLatLon, ARLocation location) in arSpawnLocations) {
                double distance = distanceToSpawnLocation(latlon, locationLatLon);
                if (distance < minDistance) {
                    minDistance = distance;
                    closestLocation = location;
                }
            }
            gameManager.LogTxt("Closest location: " + closestLocation.name);
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
    }

    public void StopAR() {
        Debug.Log("Stopping AR session.");
        arCamera.SetActive(false);
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
        // Attempt Location Quests if basic quests not fulfilled
        foreach (LocationQuest locationQuest in GameState.Instance.locationQuests) {
            if (locationQuest.IsOnGoing()) {
                if (locationQuest.AttemptQuest(screenCapture)) {
                    QuestManager.Instance.GetNextLocationQuest();
                    gameManager.LogTxt("Location quest completed: " + locationQuest.Label);
                    return;
                }
            }
        }
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
