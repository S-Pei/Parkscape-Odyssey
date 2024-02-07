using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class BattleManager : MonoBehaviour {
    public const int HAND_SIZE = 5;
    
    private GameManager gameManager;
    private BattleUIManager battleUIManager;
    private MonsterController monsterController;
    private List<CardName> allCards;
    private List<CardName> hand;

    public List<CardName> Hand {
        get { return hand; }
        private set {}
    }

    private Queue<CardName> drawPile;

    private Monster monster;

    void Awake() {
        gameManager = FindObjectOfType<GameManager>();
        battleUIManager = (BattleUIManager) GetComponent(typeof(BattleUIManager));
        monsterController = (MonsterController) GetComponent(typeof(MonsterController));
    }

    void Start() {
        // Set the encounter to be the active scene
        // Cannot be run in Awake() as the scene is not loaded at that point
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Battle"));

        // Delegate OnSceneUnloaded() to run when this scene unloads
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        // Shuffle the player's cards
        allCards = new List<CardName>(GameState.Instance.MyCards);
        Shuffle(this.allCards);

        // Add the shuffled cards to a queue to draw from
        drawPile = new Queue<CardName>(this.allCards);

        // Draw the initial hand
        GenerateHand();
        Debug.Log(string.Format("Initial hand: ({0}).", string.Join(", ", this.hand)));

        // Select a random monster to fight
        MonsterName monsterName = monsterController.GetRandomMonster();
        monster = monsterController.createMonster(monsterName);

        // Print which monster the player is fighting
        Debug.Log(string.Format("Fighting {0}.", monsterName));

        // Display the hand and monster
        battleUIManager.DisplayHand(hand);
        battleUIManager.DisplayMonster(monster);

        // StartCoroutine(UnloadTheScene());
    }

    private void DrawCard() {
        // Check whether the draw pile is empty, and reshuffle if so
        if (drawPile.Count == 0) {
            Shuffle(allCards);
            foreach (CardName card in allCards) {
                drawPile.Enqueue(card);
            }
        }

        // Add a card to the hand
        hand.Add(drawPile.Dequeue());
    }

    // Shuffle the list of cards from back to front
    private static void Shuffle(List<CardName> cards) {  
        int n = cards.Count;
        while (n > 1) {
            // Select a random card from the front of the deck
            // (up to the current position to shuffle) to swap
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);  
            
            // Swap cards[n] with cards[k]
            CardName toSwap = cards[k];  
            cards[k] = cards[n];  
            cards[n] = toSwap;  
        }
    }

    private void GenerateHand() {
        // Initialise the hand as an empty list if not already done
        if (hand is null) {
            hand = new List<CardName>();
        }

        while (hand.Count < HAND_SIZE) {
            // Check whether the draw pile is empty, and reshuffle if so
            if (drawPile.Count == 0) {
                Shuffle(allCards);
                foreach (CardName card in allCards) {
                    drawPile.Enqueue(card);
                }
            }

            // Add a card to the hand
            hand.Add(drawPile.Dequeue());
        }
    }

    private void OnSceneUnloaded(Scene current) {
        Debug.Log("Battle scene unloaded.");

        // Remove this delegated function ref, or it will accumulate and run
        // multiple times the next time this scene unloads
        SceneManager.sceneUnloaded -= OnSceneUnloaded;

        // Inform the game manager the encounter has ended
        gameManager.EndEncounter(5);
    }

    IEnumerator UnloadTheScene() {
        float secondsToWait = 10;
        yield return new WaitForSeconds(secondsToWait);
        Debug.Log("Waited 10s to end battle.");

        SceneManager.UnloadSceneAsync("Battle");
    }
}

public enum BattleMessageType {

}

public class BattleMessage : MessageInfo
{
    public MessageType messageType { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public string toJson() {
        throw new NotImplementedException();
        // return JsonConvert.SerializeObject(this);
    }

    public string processMessageInfo() {
        return "";
    }
}