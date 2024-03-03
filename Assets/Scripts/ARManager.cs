using System;
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
    private GameObject semanticsRawImage;

    [SerializeField]
    private GameObject semanticsLabel;

    [SerializeField]
    private List<(LatLon latlon, ARLocation location)> arSpawnLocations = new();

    [SerializeField]
    private GameObject scannerLinePrefab;
    [SerializeField]
    private GameObject canvas;

    private List<LatLon> latlons = new() {
        new LatLon(51.493553, -0.192372),
        new LatLon(51.493492, -0.192765),
        new LatLon(51.494637, -0.192280),
        new LatLon(51.498760, -0.179450)
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
        gameManager = gameManagerObj.GetComponent<GameManager>();
        objectDetectionManager = GetComponent<ObjectDetectionManager>();

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
            // Debug.Log("Lat: " + latlon.LatitudeInDegrees + " Lon: " + latlon.LongitudeInDegrees);

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
    }

    public void StopAR() {
        Debug.Log("Stopping AR session.");
        arCamera.SetActive(false);
        semanticsRawImage.SetActive(false);
        semanticsLabel.SetActive(false);
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
        // Trigger Scanning animation
        ScannerController scannerController = TriggerScannerEffect();
        Texture2D screenCapture = TakeScreenCapture();
        gameManager.LogTxt("Screen capture taken.");
        Quest successQuest = null;
        LocationQuest locationQuest = QuestManager.Instance.CheckLocationQuests(screenCapture);
        if (locationQuest != null) {
            successQuest = locationQuest;
            gameManager.LogTxt("Location quest :" + locationQuest.Label + " progress: " + locationQuest.Progress);
        } else {
            gameManager.LogTxt("No location quest progress.");
            // Attempt basic Quests if location quest not fulfilled
            List<string> labels = objectDetectionManager.GetLabels();
            gameManager.LogTxt("Labels: " + string.Join(", ", labels));
            BasicQuest basicQuest = QuestManager.Instance.CheckBasicQuests(labels);
            if (basicQuest != null) {
                successQuest = basicQuest;
                gameManager.LogTxt("Basic quest :" + basicQuest.Label + " progress: " + basicQuest.Progress);
                if (basicQuest.IsCompleted()) {
                    gameManager.LogTxt("Basic quest completed.");
                }
            } else {
                gameManager.LogTxt("No basic quest progress.");
            }
            
        }
        scannerController.SetSuccessQuest(successQuest);
        scannerController.SetReady();
        // FUTURE: Save images.
    }

    private ScannerController TriggerScannerEffect() {
        Debug.Log("Triggering scanner effect.");
        GameObject scannerLine = Instantiate(scannerLinePrefab, canvas.transform);
        ScannerController scannerController = scannerLine.GetComponent<ScannerController>(); 
        Debug.Log("Instantiated scanner line.");
        return scannerController;
    }

    public void ShowQuestResultPopUp(Quest quest) {
        Debug.Log("Showing quest result pop up.");
        QuestsProgressPopUpManager.Instance.ShowQuestResultPopUp(quest);
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
