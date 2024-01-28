using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private GameInterfaceManager gameInterfaceManager;

    private List<string> playerCards = new List<string> {
        "baseAtk", "baseAtk", "baseDef", "baseDef", "baseAtk", "baseDef"
    };
  
    private bool isInEncounter = false;
    private int score = 0;

    // Allow public retrieval of the player's cards, but prevent it from being modified
    public List<string> PlayerCards {
        get { return playerCards; }
        private set { }
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
        // TODO: decide if we should allow encounters with inventory open
        gameInterfaceManager.OpenInventory(playerCards);
    }

    public void CloseInventory() {
        gameInterfaceManager.CloseInventory();
    }

    public void EndEncounter(int pointsToAdd=0) {
        if (!isInEncounter) {
            Debug.LogError("Attempted to end encounter when there was none.");
        }
        Debug.Log($"Ending the encounter ({score} -> {score + pointsToAdd}).");
        score += pointsToAdd;
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
