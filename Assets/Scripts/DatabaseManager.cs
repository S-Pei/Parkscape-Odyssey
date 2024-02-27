using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Firebase;
using Firebase.Firestore;
using Firebase.Extensions; // for ContinueWithOnMainThread


public class DatabaseManager : MonoBehaviour
{
    public bool FirebaseReady { get; private set; } = false;

    public Firebase.FirebaseApp App { get; private set; }
    private FirebaseFirestore db;

    void Awake() {
        // Make sure we only ever have one instance of this object
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Database");

        if (objs.Length > 1) {
            Debug.Log("Found more than one database object - destroying this one.");
            Destroy(this.gameObject);
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

        db = FirebaseFirestore.DefaultInstance;

        SceneManager.LoadScene("Main Menu");
    }

    public void GetLocations() {
        // Debug.Log("Jajajajaj");
        // Debug.LogWarning("Creating users/alovelace.");
        // DocumentReference docRef = db.Collection("users").Document("alovelace");
        // Dictionary<string, object> user = new Dictionary<string, object>
        // {
        //         { "First", "Ada" },
        //         { "Last", "Lovelace" },
        //         { "Born", 1815 },
        // };
        // docRef.SetAsync(user).ContinueWithOnMainThread(task => {
        //         Debug.Log("Added data to the alovelace document in the users collection.");
        // });
        // CollectionReference allLocationsQuery = Db.Collection("/locations");
        // Debug.LogWarning("Getting locations from Firebase.");

        // allLocationsQuery.GetSnapshotAsync().ContinueWithOnMainThread(task => {
        //     QuerySnapshot allLocationsQuerySnapshot = task.Result;
        //     Debug.LogWarning("Number of locations: " + allLocationsQuerySnapshot.Count);
        //     foreach (DocumentSnapshot documentSnapshot in allLocationsQuerySnapshot.Documents) {
        //         Debug.LogWarning("Document data for document " + documentSnapshot.Id + ":");
        //         Dictionary<string, object> city = documentSnapshot.ToDictionary();
        //         foreach (KeyValuePair<string, object> pair in city)
        //         {
        //         Debug.LogWarning(pair.Key + " : " + pair.Value);
        //         }
        //     }
        // });
        // // yield return null;

        Debug.Log("HEHEa");
        DocumentReference docRef = FirebaseFirestore.DefaultInstance.Collection("users").Document("alovelace");
        Dictionary<string, object> user = new Dictionary<string, object>
        {
                { "First", "Ada" },
                { "Last", "Lovelace" },
                { "Born", 1815 },
        };
        docRef.SetAsync(user);
        // .ContinueWithOnMainThread(task => {
            // DocumentReference addedDocRef = task.Result;
            // Debug.Log("HEHE");
            // Debug.Log(String.Format("Added document with ID: {0}.", addedDocRef.Id));
        // });
        // docRef.SetAsync(user).ContinueWithOnMainThread(task => {
        //         // Debug.Log("Added data to the alovelace document in the users collection.");
        // });
    }
}


