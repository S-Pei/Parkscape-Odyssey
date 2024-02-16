using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    // Map manager is a singleton class
    public static MapManager instance;
    private LocationInfo location;
    private LocationServiceStatus locationServiceStatus;
    private bool permissionGranted = false;

    public static MapManager Instance {
        get {
            if (instance == null) {
                // To make sure that script is persistent across scenes
                GameObject go = new GameObject("MapManager");
                instance = go.AddComponent<MapManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
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
}
