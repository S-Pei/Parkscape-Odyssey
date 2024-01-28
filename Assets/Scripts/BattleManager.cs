using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class BattleManager : MonoBehaviour
{
    private GameManager gameManager;

    private BattleUIManager battleUIManager;
    private List<string> allCards;
    private List<string> hand; 
    private List<string> drawPile;
    private List<string> discardPile;

    // Start is called before the first frame update
    void Start() {
        // Delegate OnSceneUnloaded() to run when this scene unloads
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        // Shuffle the player's cards and produce a hand
        gameManager = FindObjectOfType<GameManager>();
        foreach(string card in gameManager.PlayerCards) {
            // Debug.Log(card);
        }

        StartCoroutine(UnloadTheScene());
    }

    private void OnSceneUnloaded(Scene current) {
        Debug.Log("Battle scene unloaded.");

        // Remove this delegated function ref, or it will accumulate and run
        // multiple times the next time this scene unloads
        SceneManager.sceneUnloaded -= OnSceneUnloaded;

        // Inform the game manager the encounter has ended
        gameManager.EndEncounter(5);
    }

    // Update is called once per frame
    void Update() {

    }

    IEnumerator UnloadTheScene() {
        float secondsToWait = 10;
        yield return new WaitForSeconds(secondsToWait);
        Debug.Log("Waited 10s to end battle.");

        SceneManager.UnloadSceneAsync("Battle");
    }
}
