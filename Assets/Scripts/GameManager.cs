using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;
using System;


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

    // Start is called before the first frame update
    void Start() {
        databaseManager = GameObject.FindWithTag("Database").GetComponent<DatabaseManager>();
        gameInterfaceManager = GetComponent<GameInterfaceManager>();
        gameInterfaceManager.SetUpInterface();
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
