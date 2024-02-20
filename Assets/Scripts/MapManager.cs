using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Maps.Unity;
using Microsoft.Geospatial;
using System;
using Newtonsoft.Json;
using System.Linq;

public class MapManager : MonoBehaviour
{
    // Map manager is a singleton class
    public static MapManager selfReference;
    private GPSManager gpsManager;
    private LocationInfo location;
    private bool permissionGranted = false;
    public GameObject map;
    private bool mapCenterSet = false;
    private bool follow = true;

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
    private const float defaultZoomLevel = 19;

    [SerializeField]
    private float interactDistance = 40; // in meters

    // Pin Constants
    private const float minPinScale = 0.025f;
    private const float maxPinScale = 0.120f;

    public static MapManager Instance {
        get {
            if (selfReference == null) {
                selfReference = new MapManager();
            }
            return selfReference;
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        // Initialisation
        selfReference = this;
        map = gameObject;
        network = NetworkManager.Instance.NetworkUtils;
        gpsManager = GPSManager.Instance;

        Debug.Log("MapManager Awake");

        // Disable full blocker
        mapBlocker.SetActive(false);

        // Get Components
        mapRenderer = GetComponent<MapRenderer>();
        mapTouchInteractionHandler = GetComponent<MapTouchInteractionHandler>();

        if (playerPinObject != null && playerRadiusObject != null) {
            playerPin = playerPinObject.GetComponent<MapPin>();
            playerRadiusPin = playerRadiusObject.GetComponent<MapPin>();
        }

        // Set the map's zoom level
        mapRenderer.MinimumZoomLevel = minZoomLevel;
        mapRenderer.MaximumZoomLevel = maxZoomLevel;

        // Set Default zoom
        mapRenderer.ZoomLevel = defaultZoomLevel;
    }

    // Update is called once per frame
    void Update()
    {
        // Get GPS location
        location = gpsManager.GetLocation();

        // Set the map's center to the current location
        if (!mapCenterSet || follow) {
            mapRenderer.Center = new LatLon(location.latitude, location.longitude);
            mapCenterSet = true;
        }

         // Update player pin location
        if (playerPin != null) {
            playerPin.Location = new LatLon(location.latitude, location.longitude);
            playerRadiusPin.Location = new LatLon(location.latitude, location.longitude);
        }

        // Check if map sharing is needed
        if (GameState.Instance.foundMediumEncounters.Count > previousFoundEncounterCount
            || NetworkManager.Instance.ChangeInConnectedPlayers()) {
            // Send map info to other players
            MapMessage mapMessage = new MapMessage(MapMessageType.FOUND_ENCOUNTERS, GameState.Instance.foundMediumEncounters.ToList(), new Dictionary<string, Dictionary<string, double>>());
            network.broadcast(mapMessage.toJson());
        }
        
    }

    public CallbackStatus HandleMessage(Message message) {
        MapMessage mapMessage = (MapMessage) message.messageInfo;
        switch (mapMessage.type) {
            // Receive new found encounters from other players
            case MapMessageType.FOUND_ENCOUNTERS:
                // Add to list of found encounters
                HashSet<string> newFoundEncounters = new HashSet<string>(mapMessage.foundEncounterIds);
                GameState.Instance.foundMediumEncounters.UnionWith(newFoundEncounters);
                // Add pins for the found encounters
                break;
            // Receive medium encounter locations from leader
            case MapMessageType.MAP_INFO:
                GameState.Instance.mediumEncounterLocations = MapMessage.DictToLatLon(mapMessage.mediumEncounterLocations);
                // Add pins for the medium encounters
                break;
        }
        return CallbackStatus.PROCESSED;
        
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

    // Called when player clicks on the button.
    public void SnapBack() {
        mapRenderer.Center = new LatLon(location.latitude, location.longitude);
        follow = true;
    }

    // Called when user moves the map.
    public void StopFollowing() {
        follow = false;
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

        // Assert that location must have been initialised if latitude and longitude are not provided
        if (latitude == -1 && longitude == -1 && !mapCenterSet) {
            throw new ArgumentException("Location must be initialised or latitude and longitude must be provided");
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
}

public class MapMessage : MessageInfo 
{
    public MapMessageType type {get; set;}
    public MessageType messageType {get; set;}
    public List<string> foundEncounterIds;
    public Dictionary<string, Dictionary<string, double>> mediumEncounterLocations;

    [JsonConstructor]
    public MapMessage(MapMessageType type, List<string> foundEncounterIds, Dictionary<string, Dictionary<string, double>> mediumEncounterLocations) {
        this.messageType = MessageType.MAP;
        this.foundEncounterIds = foundEncounterIds;
        this.type = type;
        this.mediumEncounterLocations = mediumEncounterLocations;
    }

    public static Dictionary<string, Dictionary<string, double>> LatLonToDict(Dictionary<string, LatLon> latLonDict) {
        Dictionary<string, Dictionary<string, double>> dict = new();
        foreach (var entry in latLonDict) {
            dict.Add(entry.Key, new Dictionary<string, double> {
                {"latitude", entry.Value.LatitudeInRadians},
                {"longitude", entry.Value.LongitudeInRadians}
            });
        }
        return dict;
    }

    public static Dictionary<string, LatLon> DictToLatLon(Dictionary<string, Dictionary<string, double>> dict) {
        Dictionary<string, LatLon> latLonDict = new();
        foreach (var entry in dict) {
            latLonDict.Add(entry.Key, new LatLon(entry.Value["latitude"], entry.Value["longitude"]));
        }
        return latLonDict;
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
