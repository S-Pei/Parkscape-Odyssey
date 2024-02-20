using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Maps.Unity;
using Microsoft.Geospatial;
using System;
using Newtonsoft.Json;

public class MapManager : MonoBehaviour
{
    // Map manager is a singleton class
    public static MapManager selfReference;
    private LocationInfo location;
    private LocationServiceStatus locationServiceStatus;
    private bool permissionGranted = false;
    public GameObject map;
    private bool mapCenterSet = false;

    // Player pins
    [SerializeField]
    private GameObject playerPinObject;
    private MapPin playerPin;

    [SerializeField]
    private GameObject playerRadiusObject;
    private MapPin playerRadiusPin;
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
    private const float defaultZoomLevel = 18.25f;
    private const float interactDistance = 40; // in meters

    // Pin Constants
    private const float minPinScale = 0.025f;
    private const float maxPinScale = 0.120f;

    public static MapManager Instance {
        get {
            if (selfReference == null) {
                selfReference = new MapManager();
                selfReference.network = NetworkManager.Instance.NetworkUtils;
            }
            return selfReference;
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        // Initialisation
        map = gameObject;

        Debug.Log("MapManager Awake");

        // Disable full blocker
        mapBlocker.SetActive(false);

        // Get Components
        mapRenderer = GetComponent<MapRenderer>();
        mapTouchInteractionHandler = GetComponent<MapTouchInteractionHandler>();

        playerPin = playerPinObject.GetComponent<MapPin>();
        playerRadiusPin = playerRadiusObject.GetComponent<MapPin>();

        // Set the map's zoom level
        mapRenderer.MinimumZoomLevel = minZoomLevel;
        mapRenderer.MaximumZoomLevel = maxZoomLevel;

        // Set Default zoom
        mapRenderer.ZoomLevel = defaultZoomLevel;

        // Start GPS location service
        StartCoroutine(InitialiseAndUpdateGPS());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator InitialiseAndUpdateGPS() {
        yield return StartCoroutine(InitialiseGPS());
        yield return StartCoroutine(GPSLoc());
    }

    IEnumerator InitialiseGPS() {
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
        yield return null;
    }

    IEnumerator GPSLoc() {
        while (true) {
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
                UpdateGPSData();
                // Input.location.Stop();
            }
            yield return new WaitForSecondsRealtime(1);
        }
        
    }

    private void UpdateGPSData() {

        if (Input.location.status == LocationServiceStatus.Running) {
            // Access granted and location value could be retrieved
            location = Input.location.lastData;
            Debug.Log("Location: (" + location.latitude + ", " + location.longitude + ")");

            // Set the map's center to the current location
            if (!mapCenterSet) {
                mapRenderer.Center = new LatLon(location.latitude, location.longitude);
                mapCenterSet = true;
            }
        } else {
            // GPS service stopped
        }
        locationServiceStatus = Input.location.status;
        Debug.Log("Location Service Status: " + locationServiceStatus);

        // Update player pin location
        playerPin.Location = new LatLon(location.latitude, location.longitude);
        playerRadiusPin.Location = new LatLon(location.latitude, location.longitude);

        // Check if map sharing is needed
        if (GameState.Instance.foundMediumEncounters.Count > previousFoundEncounterCount
            || NetworkManager.Instance.ChangeInConnectedPlayers()) {
            // Send map info to other players
            MapMessage mapMessage = new MapMessage(MapMessageType.FOUND_ENCOUNTERS, GameState.Instance.foundMediumEncounters, new());
            network.broadcast(mapMessage.toJson());
        }

    }

    public CallbackStatus HandleMessage(Message message) {
        MapMessage mapMessage = (MapMessage) message.messageInfo;
        switch (mapMessage.type) {
            // Receive new found encounters from other players
            case MapMessageType.FOUND_ENCOUNTERS:
                // Add to list of found encounters
                GameState.Instance.foundMediumEncounters.UnionWith(mapMessage.foundEncounterIds);
                // Add pins for the found encounters
                break;
            // Receive medium encounter locations from leader
            case MapMessageType.MAP_INFO:
                GameState.Instance.mediumEncounterLocations = mapMessage.mediumEncounterLocations;
                // Add pins for the medium encounters
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

        // Set any other properties of the pin
        if (pin.TryGetComponent(out SpriteButtonLocationBounded spriteButton)) {
            spriteButton.SetLocation(latitude, longitude);
        }

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

    // Get distance between two points in metres.
    public static double DistanceBetweenCoordinates(double lat1, double lon1, double lat2, double lon2) {
        double dLat = (lat2 - lat1) * (Math.PI / 180);
        double dLon = (lon2 - lon1) * (Math.PI / 180);
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(lat1 * (Math.PI / 180)) * Math.Cos(lat2 * (Math.PI / 180)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadius * c;
    }

    public double GetDistanceToPlayer(double latitude, double longitude) {
        return DistanceBetweenCoordinates(location.latitude, location.longitude, latitude, longitude);
    }

    public bool WithinDistanceToPlayer(double latitude, double longitude) {
        return GetDistanceToPlayer(latitude, longitude) <= interactDistance;
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

    // ENCOUNTER SPAWNING
    // Leader gets medium encounter locations from web authoring tool
    public void GetMediumEncounters() {
        if (GameState.Instance.MyPlayer.IsLeader) {
            // TODO: get list from web authoring tool
            // Hardcoded for now
            GameState.Instance.mediumEncounterLocations.Add("1", new LatLon(51.49355, -0.1924046));
            GameState.Instance.mediumEncounterLocations.Add("2", new LatLon(51.39355, -0.1924046));

            Debug.Log("Sending encounter info to players in lobby");
            // Send medium encounters to players
            network.broadcast(new MapMessage(MapMessageType.MAP_INFO, new(), GameState.Instance.mediumEncounterLocations).toJson());
        }
    }
}

public class MapMessage : MessageInfo 
{
    public MapMessageType type {get; set;}
    public MessageType messageType {get; set;}
    public HashSet<string> foundEncounterIds;
    public Dictionary<string, LatLon> mediumEncounterLocations;

    public MapMessage(MapMessageType type, HashSet<string> foundEncounterIds, Dictionary<string, LatLon> mediumEncounterLocations) {
        this.foundEncounterIds = foundEncounterIds;
        this.messageType = MessageType.MAP;
        this.type = type;
        this.mediumEncounterLocations = mediumEncounterLocations;
    }
    
    public string toJson() {
        return JsonConvert.SerializeObject(this);
    }

    public string processMessageInfo() {
        throw new NotImplementedException();
    }

}

public enum MapMessageType {
    FOUND_ENCOUNTERS,
    MAP_INFO
}
