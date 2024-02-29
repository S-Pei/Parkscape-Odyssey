using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Geospatial;

using UnityEngine;
using UnityEngine.SceneManagement;

using Firebase;
using Firebase.Firestore;
using Firebase.Storage;
using Firebase.Extensions; // for ContinueWithOnMainThread


public class DatabaseManager : MonoBehaviour
{
    public bool FirebaseReady { get; private set; } = false;

    public static DatabaseManager Instance { get; private set; }

    public Firebase.FirebaseApp App { get; private set; }
    public FirebaseFirestore Database { get; private set; }

    void Awake() {
        // Make sure we only ever have one instance of this object
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Database");

        if (objs.Length > 1) {
            Debug.Log("Found more than one database object - destroying this one.");
            Destroy(this.gameObject);
        } else {
            Instance = this;
        }
    }

    void Start() {
        Initialize();
        StartCoroutine(CompleteInitialisation());
    }

    private void Initialize() {
        // Initialize Firebase
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available) {
                // Create and hold a reference to the FirebaseApp
                App = Firebase.FirebaseApp.DefaultInstance;
                // Set a flag here to indicate whether Firebase is ready to use by your app.
                FirebaseReady = true;
            } else {
                UnityEngine.Debug.LogError(System.String.Format(
                    "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }

    // Load the Main Menu scene when either Firebase is ready
    // or the user has been waiting for more than 5 seconds.
    private IEnumerator CompleteInitialisation() {
        float timeWaited = 0;
        while (!FirebaseReady && timeWaited < 5) {
            timeWaited += Time.deltaTime;
            yield return null;
        }

        if (FirebaseReady) {
            Debug.Log("Firebase is ready! Loading Main Menu.");
        }
        else {
            Debug.Log("Waited 5 seconds for Firebase to be ready. Loading Main Menu.");
        }

        DontDestroyOnLoad(this.gameObject);

        Database = FirebaseFirestore.DefaultInstance;

        SceneManager.LoadScene("Main Menu");
    }

    // Get all the location quests from the database and return them asynchronously
    public async Task<List<LocationQuest>> GetLocationQuests() {
        Debug.Log("Getting locations from the database.");
        Query locationQuestsQuery = Database.Collection("locationQuests");

        // Wait up to 10 seconds to get the locationQuests from the database
        await Task.WhenAny(
            // First get the locationQuests collection from the database
            locationQuestsQuery.GetSnapshotAsync().ContinueWithOnMainThread(async task => {
                QuerySnapshot snapshot = task.Result;

                // Keep track of the asynchronous tasks we are about to start
                // so we can wait for them all to complete
                var tasks = new List<Task<LocationQuest>>();

                // Convert each document in the query to a LocationQuest object asynchronously
                foreach (DocumentSnapshot locationQuestDocument in snapshot.Documents) {
                    tasks.Add(GetLocationQuest(locationQuestDocument));
                }

                // Create a list to store the results
                List<LocationQuest> locationQuests = new List<LocationQuest>();

                // Wait for all the tasks to complete, and add the results to the list
                foreach (LocationQuest locationQuest in await Task.WhenAll(tasks)) {
                    if (locationQuest != null) {
                        Debug.LogWarning("Added location quest: " + locationQuest.Label);
                        locationQuests.Add(locationQuest);
                    }
                }

                // Return the list of LocationQuest objects
                return locationQuests;
            }),
            Task.Delay(10000)
        );

        return new List<LocationQuest>();
    }

    public async Task<LocationQuest> GetLocationQuest(DocumentSnapshot locationQuestDocument) {
        // Fetch the reference image from Firebase Storage
        StorageReference storageReference = FirebaseStorage
            .DefaultInstance
            .GetReferenceFromUrl(
                locationQuestDocument.GetValue<string>("imageUrl")
        );

        // Maximum image size is 1MB
        const long maxAllowedSize = 1 * 1024 * 1024;
        
        // Wait for the image download and then convert it to a Texture2D
        await storageReference.GetBytesAsync(maxAllowedSize).ContinueWithOnMainThread(task => {
            if (task.IsFaulted || task.IsCanceled) {
                Debug.LogException(task.Exception);
                return null;
            } else {
                // Convert the downloaded byte array to a Texture2D
                byte[] fileContents = task.Result;
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(fileContents);

                // Extract the other fields from the document to construct a LocationQuest object
                Dictionary<string, object> locationQuestData = locationQuestDocument.ToDictionary();

                string label = locationQuestDocument.Id;
                
                string featureVectorString = (string) locationQuestData["featureVector"];
                double[] featureVector = featureVectorString
                    .TrimStart('[')
                    .TrimEnd(']')
                    .Split(',')
                    .Select(double.Parse)
                    .ToArray();
                
                GeoPoint geoPoint = (GeoPoint) locationQuestData["location"];
                LatLon location = new LatLon(geoPoint.Latitude, geoPoint.Longitude);
                
                // Create the LocationQuest object and add it to the list (passed by reference)
                return new LocationQuest(label, texture, featureVector, location);
            }
        });

        return null;
    }

}


