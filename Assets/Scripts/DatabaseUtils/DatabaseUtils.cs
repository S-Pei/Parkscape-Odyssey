using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;

using Microsoft.Geospatial;

using UnityEngine;
using UnityEngine.SceneManagement;

using Firebase;
using Firebase.Firestore;
using Firebase.Storage;
using Firebase.Extensions; // for ContinueWithOnMainThread


/*
* The database MUST be initialized before calling any of the methods defined in
* this class. It is assumed that DatabaseManager.Instance exists.
*/

public static class DatabaseUtils {
    private static FirebaseFirestore database = DatabaseManager.Instance.Database;

    /*
     **************************************************************************
     * Public methods
     **************************************************************************
     */

    // Get the location quests from the database and return them asynchronously.
    // After the provided timeout, the method will return an empty list
    // as well as cancelling the database fetch.
    public static async Task<List<LocationQuest>> GetLocationQuestsWithTimeout(int timeoutSeconds) {
        var tokenSource = new CancellationTokenSource();
        var timeout = TimeSpan.FromSeconds(timeoutSeconds);
        try {
            return await CancelAfterAsync(
                GetLocationQuestsAsync,
                timeout,
                tokenSource.Token
            );
        } catch (TimeoutException) {
            Debug.LogWarning("GetLocationQuestsAsync timed out.");
            return new List<LocationQuest>();
        }
    }

    /*
     **************************************************************************
     * Private methods
     **************************************************************************
     */

    // Start the given task and cancel it after the given timeout
    // This is the wrapper which should be used for all DB operations,
    // so that we our updates are indeed opportunistic.
    private static async Task<TResult> CancelAfterAsync<TResult> (
        Func<CancellationToken, Task<TResult>> taskToRun,
        TimeSpan timeout, CancellationToken cancellationToken) {
        using (var timeoutCancellation = new CancellationTokenSource())

        // Link the two tokens so that when one is cancelled, the other is too
        using (var combinedCancellation = CancellationTokenSource
            .CreateLinkedTokenSource(cancellationToken, timeoutCancellation.Token))
        {
            // Start the two tasks
            var originalTask = taskToRun(combinedCancellation.Token);
            var delayTask = Task.Delay(timeout, timeoutCancellation.Token);
            
            // Wait for either to complete
            var completedTask = await Task.WhenAny(originalTask, delayTask);

            // At this point either originalTask or delayTask has completed
            // Cancel the timeout to stop the remaining task
            // (Cancelling does not affect completed tasks)
            timeoutCancellation.Cancel();

            if (completedTask == originalTask) {
                // Original task completed
                return await originalTask;
            }
            else {
                // Timeout
                throw new TimeoutException();
            }
        }
    }

    
    // Get all the location quests from the database and return them asynchronously
    // The cancellation token is used to cancel the operation if it takes too long
    private static async Task<List<LocationQuest>> GetLocationQuestsAsync(
        CancellationToken token) {
        try {
            // Get a reference to the locationQuests collection
            Query locationQuestsQuery = database.Collection("locationQuests");

            // First get the locationQuests collection from the database asynchronously
            // 'await' will return control to the caller while the query is in progress
            QuerySnapshot snapshot = await locationQuestsQuery.GetSnapshotAsync();
            Debug.LogWarning("3. Got location quests from the database.");
            
            
            // Keep track of the asynchronous tasks we are about to start
            // so we can wait for them all to complete
            var tasks = new List<Task<LocationQuest>>();

            // Start a Task for each location quest fetch
            foreach (DocumentSnapshot locationQuestDocument in snapshot.Documents) {
                Debug.LogWarning("4. Getting location quest from " + locationQuestDocument.Id);
                tasks.Add(GetLocationQuestAsync(locationQuestDocument, token));
            }

            // Create a list to store the results
            List<LocationQuest> locationQuests = new List<LocationQuest>();

            // Pause this method until all the tasks complete, then add the results to the list
            foreach (LocationQuest locationQuest in await Task.WhenAll(tasks)) {
                if (locationQuest != null) {
                    Debug.LogWarning("7. Added location quest: " + locationQuest.Label);
                    locationQuests.Add(locationQuest);
                } else {
                    Debug.LogWarning("7. Location quest was null");
                }
            }

            // Return the list of LocationQuest objects
            return locationQuests;
        } catch (AggregateException ex) {
            foreach (var innerException in ex.InnerExceptions) {
                Debug.LogException(innerException);
            }
            return new List<LocationQuest>();
        }
    }

    private static async Task<LocationQuest> GetLocationQuestAsync(
        DocumentSnapshot locationQuestDocument, CancellationToken token) {
        
        try {
            // Fetch the reference image from Firebase Storage
            Debug.LogWarning("5. Getting image from " + locationQuestDocument.GetValue<string>("imageUrl"));
            StorageReference storageReference = FirebaseStorage
                .DefaultInstance
                .GetReferenceFromUrl(
                    locationQuestDocument.GetValue<string>("imageUrl")
            );

            // Maximum image size is 1MB
            const long maxAllowedSize = 1 * 1024 * 1024;
            
            // Wait for the image download and then convert it to a Texture2D
            byte[] fileContents = await storageReference.GetBytesAsync(maxAllowedSize);
            Debug.LogWarning("6. Got image from " + locationQuestDocument.Id);

            // Convert the downloaded byte array to a Texture2D
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileContents);

            // Extract the other fields from the document to construct a LocationQuest object
            Dictionary<string, object> locationQuestData = locationQuestDocument.ToDictionary();

            string label = locationQuestDocument.Id;
            
            // Convert the feature vector string to a double array
            // TODO: The feature vectors will be stored in a file instead.
            string featureVectorString = (string) locationQuestData["featureVector"];
            double[] featureVector = featureVectorString
                .TrimStart('[')
                .TrimEnd(']')
                .Split(',')
                .Select(double.Parse)
                .ToArray();
            
            GeoPoint geoPoint = (GeoPoint) locationQuestData["location"];
            LatLon location = new LatLon(geoPoint.Latitude, geoPoint.Longitude);
            
            // Create the LocationQuest object to be returned
            return new LocationQuest(QuestType.FIND, label, texture, location);
        } catch (StorageException ex) {
            Debug.LogWarning("Failed to fetch a location quest.");
        } catch (OperationCanceledException ex) {
            Debug.LogWarning("Fetching location quest was cancelled.");
        }

        return null;
    }


}