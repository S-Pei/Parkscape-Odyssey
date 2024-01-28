using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    private GameInterfaceManager gameInterfaceManager;

    private List<string> playerCards = new List<string> {
        "baseAtk", "baseAtk", "baseDef", "baseDef", "baseAtk", "baseDef"
    };

    // Allow public retrieval of the player's cards, but prevent it from being modified
    public List<string> PlayerCards {
        get { return playerCards; }
        private set { }
    }

    
    private bool isInEncounter = false;

    // Awake is always called before any Start functions
    void Awake() {
        // Check if instance already exists
            if (instance == null) {
                // If not, set instance to this
                instance = this;
            }

            // If instance already exists and it's not this:
            else if (instance != this) {
                // Then destroy this to enforce the singleton pattern
                Debug.Log("Existing GameManager found - destroying new instance.");
                Destroy(gameObject);    
            }

            // Sets this to not be destroyed when reloading scene
            DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start() {
        gameInterfaceManager = (GameInterfaceManager) GetComponent(typeof(GameInterfaceManager));
    }

    // Update is called once per frame
    void Update() {
        if (!isInEncounter) {
            isInEncounter = true;
            StartCoroutine(EncounterMonsterRandomly());
        }
    }

    public void OpenInventory() {
        gameInterfaceManager.OpenInventory(playerCards);
    }

    public void CloseInventory() {
        gameInterfaceManager.CloseInventory();
    }

    public void EndEncounter() {
        if (!isInEncounter) {
            Debug.LogError("Attempted to end encounter when there was none.");
        }
        Debug.Log("Ending the encounter.");
        isInEncounter = false;
    }

    IEnumerator EncounterMonsterRandomly() {
        // float secondsToWait = Random.Range(5, 20);
        float secondsToWait = 10;
        yield return new WaitForSeconds(secondsToWait);
        Debug.Log("Waited 10s to start battle.");
        SceneManager.LoadScene("Battle", LoadSceneMode.Additive);
    }
}
