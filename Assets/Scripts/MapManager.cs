using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Maps.Unity;
using Microsoft.Geospatial;
using System;

public class MapManager : MonoBehaviour
{
    // Map manager is a singleton class
    public static MapManager selfReference;
    private LocationInfo location;
    private LocationServiceStatus locationServiceStatus;
    private bool permissionGranted = false;
    public GameObject map;

    // Network
    private NetworkUtils network;
    private int previousFoundEncounterCount = 0;
    // Map Blocker
    [SerializeField]
    private GameObject mapBlocker;

    // Map Components
    private MapRenderer mapRenderer;
    private MapTouchInteractionHandler mapTouchInteractionHandler;

    // Map Constants
    private const int earthRadius = 6371000;
    private const float granularity = 0.1f;
    private const float minZoomLevel = 16;
    private const float maxZoomLevel = 20;

    // Pin Constants
    private const float minPinScale = 0.025f;
    private const float maxPinScale = 0.120f;

    public static MapManager Instance {
        get {
            if (instance == null) {
                throw new Exception("MapManager has not been initialised.");
            }
            return selfReference;
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        // Initialisation
        map = gameObject;
        network = NetworkManager.Instance.networkUtils;
        instance = GetComponent<MapManager>();
        DontDestroyOnLoad(map);

        // Disable full blocker
        mapBlocker.SetActive(false);

        // Get Components
        mapRenderer = GetComponent<MapRenderer>();
        mapTouchInteractionHandler = GetComponent<MapTouchInteractionHandler>();

        // Set the map's zoom level
        mapRenderer.MinimumZoomLevel = minZoomLevel;
        mapRenderer.MaximumZoomLevel = maxZoomLevel;

        // Start GPS location service
        StartCoroutine(GPSLoc());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator GPSLoc() {

        #if UNITY_EDITOR
            // No permission handling needed in Editor
        #elif UNITY_ANDROID
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.FineLocation)) {
                Debug.Log("Requesting Fine Location Permission");
                UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.FineLocation);
            }
           

            // Wait for the user to interact with the permission dialog
            while (!permissionGranted)
            {
                // Check the permission status
                permissionGranted = UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.FineLocation);
                Debug.Log("Permission Granted: " + permissionGranted);

                // Yielding allows other parts of the program to execute while waiting for user interaction
                yield return null;
            }

            // Check if location service is initializing
            if (UnityEngine.Input.location.status == LocationServiceStatus.Initializing)
            {
                // Wait for initialization to complete
                Debug.Log("Waiting for location service to initialize");
                yield return new WaitUntil(() => UnityEngine.Input.location.status != LocationServiceStatus.Initializing);
            }

            // First, check if user has location service enabled
            if (!UnityEngine.Input.location.isEnabledByUser) {
                Debug.LogError("Android and Location not enabled");
                Debug.Log("Editor Location Service Status: " + UnityEngine.Input.location.status);
                yield break;
            }

        #elif UNITY_IOS
            if (!UnityEngine.Input.location.isEnabledByUser) {
                Debug.LogError("IOS and Location not enabled");
                yield break;
            }
        #endif

        // Start service before querying location
        UnityEngine.Input.location.Start(500f, 500f);
        Debug.Log("Location Service Status: " + UnityEngine.Input.location.status);
                
        // Wait until service initializes
        int maxWait = 20;
        while (UnityEngine.Input.location.status == LocationServiceStatus.Initializing && maxWait > 0) {
            yield return new WaitForSecondsRealtime(1);
            maxWait--;
        }

        // Editor has a bug which doesn't set the service status to Initializing. So extra wait in Editor.
        #if UNITY_EDITOR
            int editorMaxWait = 30;
            while (UnityEngine.Input.location.status == LocationServiceStatus.Stopped && editorMaxWait > 0) {
                yield return new WaitForSecondsRealtime(1);
                Debug.Log("Editor Wait: " + editorMaxWait);
                Debug.Log("Editor Location Service Status: " + UnityEngine.Input.location.status);
                editorMaxWait--;
            }
        #endif

        // Service didn't initialize in 20 seconds
        if (maxWait < 1)
        {
            Debug.LogError("Timed out");
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("Unable to determine device location");
            yield break;
        }
        else
        {
            // Access granted and location value could be retrieved
            InvokeRepeating("UpdateGPSData", 0.0f, 1.0f);
        }
    }

    private void UpdateGPSData() {

        if (Input.location.status == LocationServiceStatus.Running) {
            // Access granted and location value could be retrieved
            location = Input.location.lastData;
            Debug.Log("Location: (" + location.latitude + ", " + location.longitude + ")");
        } else {
            // GPS service stopped
        }
        locationServiceStatus = Input.location.status;
        Debug.Log("Location Service Status: " + locationServiceStatus);
        mapRenderer.Center = new LatLon(location.latitude, location.longitude);

        // Check if map sharing is needed
        if (GameState.Instance.MyPlayer.isLeader 
            && (GameState.Instance.foundMediumEncounters.Count > previousFoundEncounterCount
            || NetworkManager.Instance.ChangeInConnectedPlayers())) {
            // Send map info to other players
            MapMessage mapMessage = new MapMessage(MapMessageType.RECEIVE_MAP_INFO, GameState.Instance.foundMediumEncounters);
            network.broadcast(mapMessage);
        }

    }

    private CallbackStatus HandleMessage(Message message) {
        MapMessage mapMessage = (MapMessage) message.messageInfo;
        switch (mapMessage.type) {
            case MapMessageType.RECEIVE_MAP_INFO:
                // Add to list of found encounters
                GameState.Instance.foundMediumEncounters.UnionWith(mapMessage.foundEncounterIds);
                // Add pins for the found encounters
                break;
        }
        return CallbackStatus.PROCESSED;
        
    }

    private void OnDestroy()
    {
        // Stop location services when the script is destroyed
        Input.location.Stop();
    }

    // Getter functions for GPS location
    public string getLatitude() {
        return location.latitude.ToString();
    }

    public string getLongitude() {
        return location.longitude.ToString();
    }

    /*** Map Pins ***/
    // Add Pin to some location
    public GameObject AddPin(GameObject prefab, double latitude = -1, double longitude = -1) {
        GameObject pin = Instantiate(prefab, map.transform);
        // Add MapPin component to the pin if not already there
        if (!pin.TryGetComponent(out MapPin mapPinComponent)) {
            mapPinComponent = pin.AddComponent<MapPin>();
        }
        mapPinComponent.Location = new LatLon(latitude, longitude);

        // Adjust scaling of the pin
        mapPinComponent.Altitude = 1;
        mapPinComponent.ScaleCurve = AnimationCurve.Linear(minZoomLevel, minPinScale, maxZoomLevel, maxPinScale);

        // Set pin rotation to face the camera
        pin.transform.LookAt(Camera.main.transform);
        return pin;
    }

    // Add Pin near current location or provided location based on max and min radius, in some direction.
    public GameObject AddPinNearLocation(GameObject prefab, float maxRadius, float minRadius = 0,
                                   double direction = -1, double latitude = -1, double longitude = -1) {
        //  Assert that direction must be between 0 and 360
        if (direction != -1 && (direction < 0 || direction > 360)) {
            throw new ArgumentException("Direction must be between 0 and 360");
        }

        // Assert that maxRadius must be greater than minRadius
        if (maxRadius < minRadius) {
            throw new ArgumentException("maxRadius must be greater than minRadius");
        }

        // Assert that minRadius must be greater than or equal to 0
        if (minRadius < 0) {
            throw new ArgumentException("minRadius must be greater than or equal to 0");
        }

        // Get random distance within maxRadius that is not within minRadius with a granularity of 0.1 meters
        double randRadius = UnityEngine.Random.Range(minRadius / granularity, maxRadius / granularity) * granularity;

        // Get random direction if not provided
        direction = (direction == -1) ? UnityEngine.Random.Range(0, 360) : direction;

        // Convert direction to radians and get the change in latitude and longitude
        double directionInRadians = direction / 180 * Math.PI;
        double dLon = randRadius * Math.Cos(directionInRadians);
        double dLat = randRadius * Math.Sin(directionInRadians);

        // If latitude and longitude are not provided, use current location
        latitude = (latitude == -1) ? location.latitude : latitude;
        longitude = (longitude == -1) ? location.longitude : longitude;

        (double, double) newLocation = AddMetersToCoordinate(latitude, longitude, dLat, dLon);
        return AddPin(prefab, newLocation.Item1, newLocation.Item2);
    }

    public static (double, double) AddMetersToCoordinate(double latitude, double longitude, double dLat, double dLon) {
        double newLatitude  = latitude  + dLat / earthRadius * (180 / Math.PI);
        double newLongitude = longitude + dLon / earthRadius * (180 / Math.PI) / Math.Cos(latitude * Math.PI / 180);
        return (newLatitude, newLongitude);
    }

    /*** Map Interactions ***/
    public void DisableMapInteraction() {
        mapTouchInteractionHandler.enabled = false;
        mapBlocker.SetActive(true);
    }

    public void EnableMapInteraction() {
        mapTouchInteractionHandler.enabled = true;
        mapBlocker.SetActive(false);
    }
}

public class MapMessage : MessageInfo 
{
    public MapMessageType type {get; set;}
    public MessageType messageType {get; set;}
    public Set<string> foundEncounterIds;

    public MapMessage(MapMessageType type, Set<string> foundEncounterIds) {
        this.foundEncounterIds = foundEncounterIds;
        this.messageType = MessageType.MAP;
        this.type = type;
    }

}

public enum MapMessageType {
    RECEIVE_MAP_INFO,
}
