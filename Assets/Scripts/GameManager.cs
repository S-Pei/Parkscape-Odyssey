using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using TMPro;


public class GameManager : MonoBehaviour
{
    public static GameManager selfReference;

    private GameInterfaceManager gameInterfaceManager;
    private DatabaseManager databaseManager;

    [SerializeField] private GameObject mainCamera;

    [SerializeField] private GameObject arSession;

    [SerializeField] private GameObject xrOrigin;
    private Depth_ScreenToWorldPosition depth_ScreenToWorldPosition;

    [SerializeField] private GameObject debugLogger;

    private Boolean inARMode = false;

    public static GameManager Instance {
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

    // Start is called before the first frame update
    void Start() {
        GameObject databseOjb = GameObject.FindWithTag("Database");
        if (databseOjb == null) {
            Debug.LogError("Database not found.");
        } else {
            databaseManager = databseOjb.GetComponent<DatabaseManager>();
        }

        gameInterfaceManager = GetComponent<GameInterfaceManager>();
        gameInterfaceManager.SetUpInterface();
        databaseManager = GameObject.FindWithTag("Database").GetComponent<DatabaseManager>();
        depth_ScreenToWorldPosition = xrOrigin.GetComponent<Depth_ScreenToWorldPosition>();
    }

    // Update is called once per frame
    void Update() {
        // if (!isInEncounter) {
        //     isInEncounter = true;
        //     StartCoroutine(EncounterMonsterRandomly());
        // }
    }

    public void OpenInventory() {
        gameInterfaceManager.OpenInventory();
        if (inARMode) {
            depth_ScreenToWorldPosition.DisableARInteraction();
        }
    }

    public void CloseInventory() {
        gameInterfaceManager.CloseInventory();
        if (inARMode) {
            depth_ScreenToWorldPosition.EnableARInteraction();
        }
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
        if (inARMode) {
            depth_ScreenToWorldPosition.DisableARInteraction();
        }
    }

    public void ClosePlayerView() {
        if (inARMode) {
            depth_ScreenToWorldPosition.EnableARInteraction();
        }
    }

    public void OpenQuests() {
        gameInterfaceManager.OpenQuests();
        if (inARMode) {
            depth_ScreenToWorldPosition.DisableARInteraction();
        }
    }

    public void CloseQuests() {
        if (inARMode) {
            depth_ScreenToWorldPosition.EnableARInteraction();
        }
    }

    //------------------------------- AR CAMERA -------------------------------
    public void ToggleARCamera() {
        if (inARMode) {
            CloseARSession();
            gameInterfaceManager.SetARCameraToggle(false);
        } else {
            OpenARSession();
            gameInterfaceManager.SetARCameraToggle(true);
        }
    }

    private void OpenARSession() {
        mainCamera.SetActive(false);
        arSession.SetActive(true);
        ARManager.Instance.StartAR();

        // Disable map interactions
        MapManager.Instance.DisableMapInteraction();

        inARMode = true;
    }

    private void CloseARSession() {
        ARManager.Instance.StopAR();
        arSession.SetActive(false);
        mainCamera.SetActive(true);

        // Enable map interactions
        MapManager.Instance.EnableMapInteraction();

        inARMode = false;
    }


    // ------------------------------ BUILD DEBUG ------------------------------
    public void LogTxt(string text) {
        debugLogger.GetComponent<TextMeshProUGUI>().text += "\n";
        debugLogger.GetComponent<TextMeshProUGUI>().text += text;
    }

    public void RelogTxt(string text) {
        debugLogger.GetComponent<TextMeshProUGUI>().text = text;
    }
}
