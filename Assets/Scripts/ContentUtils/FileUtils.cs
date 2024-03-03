using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System;

using UnityEngine;

using Microsoft.Geospatial;

public static class FileUtils {
    // This is the path where data files (e.g. for OR location quests) should be stored.
    // TODO: For complete documentation of the structure of this directory, see XXX. 
    public static readonly string dataPath = Path.Combine(Application.persistentDataPath, "data/");

    // Flag to indicate whether file saving has completed on a separate thread
    private static volatile bool filesSaved_Threaded = false;

    // Return the full filepath in the Application's persistant data folder
    public static string GetFilePath(string fileName, string folder="", string root=null) {
        // return Path.Combine(DataPath, folder, fileName);
        return Path.Combine(root != null ? root : dataPath, folder, fileName);
    }

    /*
     *************************************************************************
     * Utility functions for the handling of location quest files
     *************************************************************************
     */

    // Check if the default quest files should be used (i.e. if no quest files
    // have been downloaded from Firebase yet)
    public static bool ShouldUseDefaultQuestFiles() {
        return !PlayerPrefs.HasKey("LastQuestFileUpdate");
    }

    // Save the three given quest files (as byte arrays) to the GameState instance, and write them to disk
    // asynchronously. Also, update the last quest file update time in PlayerPrefs.
    public static IEnumerator ProcessNewQuestFiles(
        byte[] locationQuestVectors, byte[] locationQuestGraph,
        byte[] locationQuestLabels, string folder="quests", string root=null) {
        // Save the quest files to the GameState instance
        GameState.Instance.locationQuestVectors = locationQuestVectors;
        GameState.Instance.locationQuestGraph = locationQuestGraph;
        GameState.Instance.locationQuestLabels = locationQuestLabels;

        // Write the quest files to disk in a separate thread to avoid
        // blocking the main thread with slow disk I/O
        SaveFilesThreaded(new Dictionary<string, byte[]> {
            {"locationQuestVectors", locationQuestVectors},
            {"locationQuestGraph", locationQuestGraph},
            {"locationQuestLabels", locationQuestLabels}
        }, folder, root);

        while (!filesSaved_Threaded) {
            yield return null;
        }

        filesSaved_Threaded = false;

        // Update the last quest file update time in PlayerPrefs
        PlayerPrefs.SetString("LastQuestFileUpdate", DateTime.Now.ToString());
    }

    /*
     *************************************************************************
     * Utility functions for saving files to disk
     *************************************************************************
     */

    // Save the given data as a serialised JSON string to the persistent data folder
    // TODO: Explanation about serializability: https://docs.unity3d.com/ScriptReference/JsonUtility.ToJson.html
    public static void Save<TData>(TData data, string fileName, string folder="", string root=null) {
        string filePath = GetFilePath(fileName, folder, root); 

        byte[] byteData;

        // If the data is already a byte array, we can save it directly
        if (typeof(TData) != typeof(byte[])) {
            string jsonData = JsonUtility.ToJson(data, false);
            if (typeof(TData) == typeof(List<int>)) {
                // Print each element of the list
                List<int> list = (List<int>)Convert.ChangeType(data, typeof(List<int>));
                Debug.Log("List: " + string.Join(", ", list.Select(x => x.ToString()).ToArray()));
            }
            Debug.Log("json: " + jsonData);
            byteData = Encoding.ASCII.GetBytes(jsonData);
        } else {
            byteData = (byte[])Convert.ChangeType(data, typeof(byte[]));
        }
        

        // Create the file in the path if it doesn't exist
        if (!Directory.Exists(Path.GetDirectoryName(filePath))) {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        }

        // Attempt to save data
        try {
            File.WriteAllBytes(filePath, byteData);
            Debug.Log("Save data to: " + filePath);
        } catch (Exception e) {
            Debug.LogError("Failed to save data to: " + filePath);
            Debug.LogError("Error " + e.Message);
        }
    }

    // Save the given files (with associated filenames) on a new thread
    private static void SaveFilesThreaded(Dictionary<string, byte[]> files, string folder="quests", string root=null) {
        new Thread(() => {
            foreach (var file in files) {
                Save(file.Value, file.Key, folder, root != null ? root : dataPath);
            }
            Debug.LogWarning("Saved quest files to disk");

            // Set the flag to indicate that the files have been saved
            filesSaved_Threaded = true;
        }).Start();
        }

    /*
     *************************************************************************
     * Utility functions for loading files from disk
     *************************************************************************
     */
    
    // Load the data from a file in the data folder and return it as the specified type
    public static TData Load<TData>(string fileName, string folder="", string root=null) {
        string filePath = GetFilePath(fileName, folder, root);

        // Return default if the requested file does not exist
        if (!Directory.Exists(Path.GetDirectoryName(filePath))) {
            Debug.LogWarning("File or path does not exist! " + filePath);
            return default(TData);
        }

        // Load in the save data as byte array
        byte[] jsonDataAsBytes = null;

        try {
            jsonDataAsBytes = File.ReadAllBytes(filePath);
            Debug.Log("Loaded all data from: " + filePath);
        } catch (Exception e) {
            Debug.LogError("Failed to load data from: " + filePath);
            Debug.LogError("Error: " + e.Message);
            return default(TData);
        }

        if (jsonDataAsBytes == null)
            return default(TData);

        // If the requested datatype is a byte array, no need for processing
        if (typeof(TData) == typeof(byte[])) {
            return (TData)Convert.ChangeType(jsonDataAsBytes, typeof(TData));
        }

        // Convert the byte array to json
        string jsonData;
        jsonData = Encoding.ASCII.GetString(jsonDataAsBytes);

        // Convert to the specified object type
        TData returnedData = JsonUtility.FromJson<TData>(jsonData);

        // return the casted json object to use
        return (TData)Convert.ChangeType(returnedData, typeof(TData));
    }

    // Load the bytes of a file from the Resources folder
    // In order to be loaded correctly:
    //  - The fileName argument must NOT have an extension, and
    //  - The file in /Resources should have a .bytes extension
    public static byte[] LoadBytesFromResources(string fileName, string folder="") {
        string fullPath = Path.Combine(folder, fileName);
        TextAsset textAsset = Resources.Load(fullPath) as TextAsset;
        if (textAsset == null) {
            Debug.LogWarning("Failed to load file from Resources: " + fullPath);
            return null;
        }
        return textAsset.bytes;
    }
}
