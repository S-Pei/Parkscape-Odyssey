using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

using Firebase;
using Firebase.Firestore;


public class GameManager : MonoBehaviour
{
    private GameInterfaceManager gameInterfaceManager;
    private DatabaseManager databaseManager;

    [SerializeField]
    private GameObject mainCamera;

    [SerializeField]
    private GameObject arSession;
    
    [SerializeField]
    private GameObject xrInteractionManager;
    
    [SerializeField]
    private GameObject xrOrigin;

    private Boolean inARMode = false;

    private volatile Boolean IsProcessingNewLocationQuests = false;

    private ListenerRegistration locationQuestListener;

    // Start is called before the first frame update
    void Start() {
        databaseManager = GameObject.FindWithTag("Database").GetComponent<DatabaseManager>();
        gameInterfaceManager = GetComponent<GameInterfaceManager>();
        gameInterfaceManager.SetUpInterface();
        AddLocationQuestListener();
    }

    // Update is called once per frame
    void Update() {
        // if (!isInEncounter) {
        //     isInEncounter = true;
        //     StartCoroutine(EncounterMonsterRandomly());
        // }
    }

    // Add listener for location quest updates
    public void AddLocationQuestListener() {
        Query query = DatabaseManager.Instance.Database.Collection("locationQuests");
        
        // On an update to the locationQuests collection, we will:
        //  - Fetch the reference images to construct locationQuest objects
        //  - Fetch the updated files for the object classifier
        //  - Update the map of locationQuests in the GameState
        //  - Update the byte[] fields in the GameState
        //  - Save the new files to disk
        // The above steps must be atomic, i.e. saves should only be done
        // once all the required data is obtained from the database.
        // We will continuously try to fetch said data in the background.
        locationQuestListener = query.Listen(snapshot => {
            Debug.LogWarning("LocationQuests collection updated.");
            StartCoroutine(ProcessLocationQuestsUpdate(snapshot.GetChanges()));
        });
    }

    public void DetachLocationQuestListener() {
        Debug.LogWarning("Detaching location quest listener.");
        locationQuestListener.Stop();
    }

    public IEnumerator ProcessLocationQuestsUpdate(IEnumerable<DocumentChange> changes) {
        // Detach the listener until this update is done - this seems to not work, and neither does setting a flag...
        // TODO: Discuss this/how to handle multiple listener triggers at once
        // DetachLocationQuestListener();
        
        Debug.LogWarning("Done waiting - processing new location quests.");

        // // Set flag to indicate that we are currently processing new location quests
        // IsProcessingNewLocationQuests = true;
        
        string previousUpdate = PlayerPrefs.GetString("LastQuestFileUpdate");
        Task task = DatabaseUtils.ProcessLocationQuestsUpdateAsync(changes, this);

        // Wait for the task to complete and for the last quest file update to change
        // The second condition is to ensure that the quest files have been saved to both
        // GameState and disk (we may run into read/write race conditions on the GameState
        // fields if we don't wait)
        while (!task.IsCompleted || previousUpdate == PlayerPrefs.GetString("LastQuestFileUpdate")) {
            yield return null;
        }

        // // Reset the flag
        // IsProcessingNewLocationQuests = false;
        // Add the listener back
        // AddLocationQuestListener();
    }

    public void OpenInventory() {
        gameInterfaceManager.OpenInventory();
    }

    public void CloseInventory() {
        gameInterfaceManager.CloseInventory();
    }

    public void EndEncounter(int pointsToAdd=0) {
        if (!GameState.Instance.IsInEncounter) {
            Debug.LogError("Attempted to end encounter when there was none.");
        }
        Debug.Log($"Ending the encounter ({GameState.Instance.Score} -> {GameState.Instance.Score + pointsToAdd}).");
        GameState.Instance.Score += pointsToAdd;
        GameState.Instance.IsInEncounter = false;
    }

    // public void StartEncounter() {
    //     GameState.Instance.IsInEncounter = true;
    //     Debug.Log("Starting the encounter (wrong).");
    //     SceneManager.LoadScene("Battle", LoadSceneMode.Additive);
    // }

    // IEnumerator EncounterMonsterRandomly() {
    //     // float secondsToWait = Random.Range(5, 20);
    //     float secondsToWait = 10;
    //     yield return new WaitForSeconds(secondsToWait);
    //     Debug.Log("Waited 10s to start battle.");
    //     StartEncounter();
    // }

    public void OpenPlayerView() {
        gameInterfaceManager.OpenPlayerView();
    }

    //------------------------------- AR CAMERA -------------------------------
    public void ToggleARCamera() {
        if (inARMode) {
            CloseARSession();
        } else {
            OpenARSession();
        }
    }

    private void OpenARSession() {
        mainCamera.SetActive(false);
        arSession.SetActive(true);
        xrInteractionManager.SetActive(true);
        xrOrigin.SetActive(true);

        // Disable map interactions
        MapManager.Instance.DisableMapInteraction();

        inARMode = true;
    }

    private void CloseARSession() {
        arSession.SetActive(false);
        xrInteractionManager.SetActive(false);
        xrOrigin.SetActive(false);
        mainCamera.SetActive(true);

        // Enable map interactions
        MapManager.Instance.EnableMapInteraction();

        inARMode = false;
    }
}
