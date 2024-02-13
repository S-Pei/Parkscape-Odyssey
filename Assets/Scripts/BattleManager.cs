using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Newtonsoft.Json;
using TMPro;

public class BattleManager : MonoBehaviour {
    public const int HAND_SIZE = 5;
    
    private GameManager gameManager;
    private GameInterfaceManager gameInterfaceManager;
    private BattleUIManager battleUIManager;
    private MonsterController monsterController;
    private CardsUIManager cardsUIManager;
    private List<CardName> allCards;
    private List<CardName> hand;

    [SerializeField]
    private GameObject playerCurrentHealth;

    [SerializeField]
    private GameObject playerCurrentMana;

    [SerializeField]
    private GameObject playerMaxMana;

    [SerializeField]
    private GameObject playerDrawPileNumber;

    [SerializeField]
    private GameObject playerDiscardPileNumber;

    private List<GameObject> otherPlayerStat = new List<GameObject>();

    // monster
    private List<Monster> monsters;
    private List<List<SkillName>> skillsSequences;



    // p2p networking
    private NetworkUtils network;
    private readonly float msgHandlingFreq = 0.1f;
    private readonly int msgFreq = 0;
    private int msgFreqCounter = 0;
    private bool AcceptMessages = false;
    

    public List<CardName> Hand {
        get { return hand; }
        private set {}
    }
    private Queue<CardName> drawPile;

    private Monster monster;

    private List<string> playerOrderIds;

    private List<CardName> cardsToPlay;

    void Awake() {
        gameManager = FindObjectOfType<GameManager>();
        gameInterfaceManager = (GameInterfaceManager) FindObjectOfType(typeof(GameInterfaceManager));
        battleUIManager = (BattleUIManager) GetComponent(typeof(BattleUIManager));
        monsterController = (MonsterController) GetComponent(typeof(MonsterController));

        monsters = GameState.Instance.encounterMonsters;
        skillsSequences = GameState.Instance.skillSequences;

        // Setup p2p network
        // network = NetworkManager.Instance.NetworkUtils;
        // InvokeRepeating("HandleMessages", 0.0f, msgHandlingFreq);
    }

    void Start() {
        // Search for the CardsUIManager here because in Awake() it is not initialised yet
        cardsUIManager = (CardsUIManager) FindObjectOfType(typeof(CardsUIManager));

        // Initialise the list of selected cards
        cardsToPlay = new List<CardName>();
        
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

        // Select a random monster to fight
        // MonsterName monsterName = monsterController.GetRandomMonster();
        // monster = monsterController.createMonster(monsterName);

        // Print which monster the player is fighting
        string monsterName = monsters[0].name.ToString();
        Debug.Log(string.Format("Fighting {0}.", monsterName));

        // Start the turn
        StartTurn();

        // Display the hand and monster
        battleUIManager.DisplayHand(hand); 
        battleUIManager.DisplayMonster(monsters[0]);

        // Initializes Player's health, current mana and max mana
        UpdatesPlayerStats();

        // Initializes Player's card number
        UpdateCardNumber();

        // Initializes the Other Players panel and Speed panel
        otherPlayerStat = battleUIManager.DisplayOtherPlayers(GameState.Instance.OtherPlayers);
        List<Player> allPlayers = new List<Player>(GameState.Instance.OtherPlayers);
        allPlayers.Add(GameState.Instance.MyPlayer);
        battleUIManager.DisplaySpeedPanel(allPlayers);

        Debug.Log("Length of otherPlayerStat: " + otherPlayerStat.Count);

        // Initializes Player's Stat
        InitializesPlayerStat();

        // StartCoroutine(UnloadTheScene());
        UpdatePlayerOrder();
    }

    // TODO: Implement the logic for playing a card, including mana checking
    public void PlayCard(int cardIndex) {
        CardName card = hand[cardIndex];

        // Play the card with the given index, if possible
        if (GameState.Instance.MyPlayer.Mana < cardsUIManager.findCardDetails(card).cost) {
            // The card is too expensive
            Debug.Log("Too expensive");
            battleUIManager.RepositionCards();
            return;
        }

        Debug.Log(string.Format("Playing card: {0}.", card));
        cardsToPlay.Add(card);

        // Remove the card from the hand
        hand.RemoveAt(cardIndex);

        // Update the hand on-screen
        battleUIManager.RemoveCardFromHand(cardIndex);

        // Reduce the player's mana by the card cost
        GameState.Instance.MyPlayer.PlayCard(cardsUIManager.findCardDetails(card));

        // Update the player's stats in the UI
        UpdatesPlayerStats();

        // Update the player's card number UI
        UpdateCardNumber();

        // Update the other players' stats UI
        UpdateOtherPlayerStats();

        // Update the player order UI
        UpdatePlayerOrder();

        // // Draw 5 cards if the hand is empty
        // if (hand.Count == 0) {
        //     for (int i = 0; i < HAND_SIZE; i++) {
        //         DrawCard();
        //     }
        // }
    }

    public void StartTurn() {
        Debug.Log("Started turn");

        Player myPlayer = GameState.Instance.MyPlayer;

        myPlayer.ResetMana();
    
        GenerateHand();
        Debug.Log(string.Format("Generated hand: ({0}).", string.Join(", ", this.hand)));
    }

    public void EndTurn() {
        Debug.Log("Ended turn");
        
        foreach (CardName card in cardsToPlay) {
            Debug.Log("Selected " + card);
        }
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
        CardName drawnCard = drawPile.Dequeue();
        hand.Add(drawnCard);
        battleUIManager.AddToHand(drawnCard);
    }

    // Updates Player's health, current mana and max mana
    private void UpdatesPlayerStats() {
        Player myPlayer = GameState.Instance.MyPlayer;
        playerCurrentHealth.GetComponent<TextMeshProUGUI>().text = myPlayer.CurrentHealth.ToString();
        playerCurrentMana.GetComponent<TextMeshProUGUI>().text = myPlayer.Mana.ToString();
        playerMaxMana.GetComponent<TextMeshProUGUI>().text = myPlayer.MaxMana.ToString();
    }

    private void UpdateCardNumber() {
        playerDrawPileNumber.GetComponent<TextMeshProUGUI>().text = (drawPile.Count).ToString();
        playerDiscardPileNumber.GetComponent<TextMeshProUGUI>().text = (allCards.Count - drawPile.Count - hand.Count).ToString();
    }

    // Initializes other Player's health and icon.
    private void InitializesPlayerStat() {
        List<Player> otherPlayers = GameState.Instance.OtherPlayers;
        Debug.Log(otherPlayers);
        for (int i = 0; i < GameState.Instance.maxPlayerCount - 1; i++) {
            if (i < otherPlayers.Count) {
                if (otherPlayers[i].Icon == null) {
                    Debug.Log(gameInterfaceManager);
                    otherPlayers[i].Icon = gameInterfaceManager.GetIcon(otherPlayers[i].Role);
                }
                otherPlayerStat[i].transform.GetChild(0).GetComponent<Image>().sprite = otherPlayers[i].Icon;
                otherPlayerStat[i].transform.GetChild(1)
                                  .transform.GetChild(0)
                                  .GetComponent<TextMeshProUGUI>().text = otherPlayers[i].CurrentHealth.ToString();
            }
        }
    }

    // Updates other Player's health.
    private void UpdateOtherPlayerStats() {
        List<Player> otherPlayers = GameState.Instance.OtherPlayers;
        for (int i = 0; i < otherPlayers.Count; i++) {
            otherPlayerStat[i].transform.GetChild(1)
                                .transform.GetChild(0)
                                .GetComponent<TextMeshProUGUI>().text = otherPlayers[i].CurrentHealth.ToString();
        }
    }

    // Update the order of players based on their speed
    private void UpdatePlayerOrder() {
        List<Player> players = new List<Player>(GameState.Instance.OtherPlayers);
        players.Add(GameState.Instance.MyPlayer);
        players = players.OrderByDescending(player => player.Speed).ToList();
        playerOrderIds = players.Select(player => player.Id).ToList();
        
        // rearrange the order of otherPlayerStats based on the playerOrderIds, exluding the current player
        List<string> otherPlayerIds = playerOrderIds.Where(id => id != GameState.Instance.MyPlayer.Id).ToList();
        battleUIManager.arrangeOtherPlayersInOrder(otherPlayerIds);
        battleUIManager.arrangeSpeedPanelInOrder(playerOrderIds);
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
        // If this has been done, just clear the list
        if (hand is null) {
            hand = new List<CardName>();
        } else {
            hand.Clear();
        }

        while (hand.Count < HAND_SIZE) {
            // Check whether the draw pile is empty, and reshuffle if so
            if (drawPile.Count == 0) {
                Shuffle(allCards);
                foreach (CardName card in allCards.Except(hand).ToList()) {
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


    // ------------------------------ P2P NETWORK ------------------------------
    private void HandleMessages() {
        Func<Message, CallbackStatus> callback = (Message msg) => {
            return HandleMessage(msg);
        };
        network.onReceive(callback);

        // Every msgFreq seconds, send messages.
        if (msgFreqCounter >= msgFreq) {
            SendMessages();
            msgFreqCounter = 0;
        } else {
            msgFreqCounter++;
        }
    }

    private void SendMessages() {
        Debug.Log("Attempting to send battle messages.");
        if (network == null)
            return;

        if (!AcceptMessages) {
            Debug.Log("Not accepting messages.");
            return;
        }

        Debug.Log("Sending Battle Messages.");

        // TDDO: Send messages to connected devices.
    }
    
    private CallbackStatus HandleMessage(Message message) {
        // TODO
        return CallbackStatus.NOT_PROCESSED;
    }
}

public enum BattleMessageType {

}

public class BattleMessage : MessageInfo
{
    public MessageType messageType {get; set;}
    public BattleMessageType Type {get; set;}

    [JsonConstructor]
    public BattleMessage(BattleMessageType type) {
        messageType = MessageType.BATTLEMESSAGE;
        Type = type;
    }

    public string toJson() {
        return JsonConvert.SerializeObject(this);
    }

    public string processMessageInfo() {
        return "";
    }
}