using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Newtonsoft.Json;
using TMPro;
using System.Data.Common;

public class BattleManager : MonoBehaviour {
    public static BattleManager selfReference;

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
    
    [SerializeField]
    private GameObject endTurnButton;

    [SerializeField]
    private GameObject lootOverlay;

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
    private Queue<CardName> discardPile;

    private Monster monster;

    private List<string> playerOrderIds;

    private List<CardName> cardsToPlay;

    private Dictionary<string, List<CardName>> othersCardsToPlay = new();
    private Dictionary<string, string> partyMembers;
    private BattleStatus battleStatus = BattleStatus.TURN_IN_PROGRESS;
    private int monsterSkillIndex = 0;

    void Awake() {
        if (!selfReference) {
            selfReference = this;
            gameManager = FindObjectOfType<GameManager>();
            gameInterfaceManager = (GameInterfaceManager) FindObjectOfType(typeof(GameInterfaceManager));
            battleUIManager = (BattleUIManager) GetComponent(typeof(BattleUIManager));
            monsterController = (MonsterController) GetComponent(typeof(MonsterController));
        } else {
            Destroy(gameObject);
        }

        AcceptMessages = true;
        monsters = GameState.Instance.encounterMonsters;
        skillsSequences = GameState.Instance.skillSequences;
        partyMembers = GameState.Instance.partyMembers;
        // Setup p2p network
        network = NetworkManager.Instance.NetworkUtils;
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
        discardPile = new Queue<CardName>();

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

    void Update() {
        if (battleStatus != BattleStatus.TURN_IN_PROGRESS) {
            endTurnButton.SetActive(false);
        } else {
            endTurnButton.SetActive(true);
        }
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

        // Add the card to the discard pile
        discardPile.Enqueue(hand[cardIndex]);

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
        battleStatus = BattleStatus.TURN_IN_PROGRESS;

        Player myPlayer = GameState.Instance.MyPlayer;

        myPlayer.ResetMana();
        UpdatesPlayerStats();
    
        GenerateHand();
        Debug.Log(string.Format("Generated hand: ({0}).", string.Join(", ", this.hand)));
    }

    public void EndTurn() {
        Debug.Log("Ended turn");
        battleStatus = BattleStatus.TURN_ENDED;
        
        foreach (CardName card in cardsToPlay) {
            Debug.Log("Selected " + card);
        }

        BroadcastCardsPlayed();

        int notReceived = 0;
        foreach (string id in partyMembers.Keys) {
            Debug.Log("Processing ID: " + id);
            if (id == GameState.Instance.myID || othersCardsToPlay.ContainsKey(id)) {
                Debug.Log("Already received cards from " + id);
                continue;
            }
            Debug.Log("Not received cards from " + id);
            notReceived++;
        }

        if (notReceived == 0 ) {
            battleStatus = BattleStatus.RESOLVING_PLAYED_CARDS;
            EndOfTurnActions();
            UpdateCardNumber();
        }
    }

    // Resolve end of turn actions
    public void EndOfTurnActions() {
        ResolvePlayedCardsOrder();
        MonsterAttack();
        if (GameEnded()) {
            Debug.Log("Game ended");
            // End the encounter
            SceneManager.UnloadSceneAsync("Battle");
            Instantiate(lootOverlay);

        } else {
            StartTurn();
            battleUIManager.DisplayHand(hand);
        }
    }

    // Checks whether the game has ended
    private bool GameEnded() {
        if (monsters[0].Health <= 0) {
            return true;
        }
        // Check if all players have died
        foreach (string id in partyMembers.Keys) {
            if (GameState.Instance.PlayersDetails[id].CurrentHealth > 0) {
                return false;
            }
        }
        return true;
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

    // Resolve played cards order based on player's speed stats
    private void ResolvePlayedCardsOrder() {
        Debug.Log("Resolving cards");
        List<Player> players = new();
        foreach (string id in partyMembers.Keys) {
            players.Add(GameState.Instance.PlayersDetails[id]);
        } 
        // order players based on their speed stat
        players = players.OrderByDescending(player => player.Speed).ToList();
        playerOrderIds = players.Select(player => player.Id).ToList();
        foreach (string id in playerOrderIds) {
            List<CardName> cards  = new();
            if (id == GameState.Instance.MyPlayer.Id) {
                cards = cardsToPlay;
            } else {
                cards = othersCardsToPlay[id];
            }
            foreach (CardName card in cards) {
                // Resolve Played card
                Debug.Log("Resolving played card: " + card);
                Card cardPlayed = cardsUIManager.findCardDetails(card);
                cardPlayed.UseCard(players, monsters, id);
                UpdatesPlayerStats();
                UpdateOtherPlayerStats();
                UpdateMonsterStats();
            }
        }


        // Clear everyones card played
        othersCardsToPlay.Clear();
        cardsToPlay = new();
    }

    // Monster Attacks
    public void MonsterAttack() {
        SkillName skill = skillsSequences[0][monsterSkillIndex];

        List<Player> players = new();
        foreach (string id in partyMembers.Keys) {
            players.Add(GameState.Instance.PlayersDetails[id]);
        } 
        
        MonsterFactory.skillsController.Get(skill).Perform(monsters[0], players);
        Debug.Log("Monster attacking with " + skill);
        UpdatesPlayerStats();
        UpdateOtherPlayerStats();
        UpdateMonsterStats();
        monsterSkillIndex++;
        monsterSkillIndex %= skillsSequences[0].Count;
    }

    // Update monster stats based on played cards
    private void UpdateMonsterStats() {
        battleUIManager.UpdateMonsterStats(monsters);
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
        playerDiscardPileNumber.GetComponent<TextMeshProUGUI>().text = (discardPile.Count).ToString();
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
            Debug.Log($"Hand count: {hand.Count}");
            foreach (CardName card in hand) {
                discardPile.Enqueue(card);
            }
            for (int i = 0; i < hand.Count; i++) {
                battleUIManager.RemoveCardFromHand(0);
            }
            hand = new();
        }
        Debug.Log($"Hand count after reset: {hand.Count}");

        while (hand.Count < HAND_SIZE) {
            // Check whether the draw pile is empty, and reshuffle if so
            if (drawPile.Count == 0) {
                List<CardName> discardPileLs = discardPile.ToList();
                Shuffle(discardPileLs);
                foreach (CardName card in discardPileLs) {
                    drawPile.Enqueue(card);
                }
                discardPile = new();
            }

            // Add a card to the hand
            // DrawCard();
            hand.Add(drawPile.Dequeue());
            Debug.Log($"Hand count after draw one: {hand.Count}");
        }
        // battleUIManager.RepositionCards();
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
    public void SendMessages(Dictionary<string, string> connectedPlayers, List<string> disconnectedPlayers) {
        // Debug.Log("Attempting to send battle messages.");
        if (network == null)
            return;

        if (!AcceptMessages) {
            Debug.Log("Not accepting messages.");
            return;
        }
        
        if (battleStatus == BattleStatus.TURN_ENDED) {
            //  Debug.Log("Sending Battle Messages.");
            List<string> sendTos = new();

            foreach (string id in partyMembers.Keys) {
                if (id == GameState.Instance.myID || othersCardsToPlay.ContainsKey(id)) {
                    continue;
                }
                // Request played card from players that we have not received cards from
                sendTos.Add(id);
            }
            if (sendTos.Count() > 0) {
                BattleMessage battleMessageRequest = new BattleMessage(BattleMessageType.REQUEST_PLAYED_CARDS, new(), sendTos: sendTos);
                network.broadcast(battleMessageRequest.toJson());
                battleStatus = BattleStatus.SENT_PLAYED_CARDS; 
            }
        }

    }
    
    public CallbackStatus HandleMessage(Message message) {
        // Debug.Log("Received battle message.");
    
        BattleMessage battleMessage = (BattleMessage) message.messageInfo;

        List<string> sendTos = battleMessage.SendTos;
        if (sendTos.Count() != 0 && !sendTos.Contains(GameState.Instance.myID)) {
            return CallbackStatus.DORMANT;
        }
        if (battleMessage.Type == BattleMessageType.REQUEST_PLAYED_CARDS) {
            if (battleStatus == BattleStatus.TURN_IN_PROGRESS) {
                return CallbackStatus.DORMANT;
            }
            Debug.Log("Received request for my played cards.");
            // Send played cards
            BattleMessage playedCardsMessage = new BattleMessage(BattleMessageType.PLAYED_CARDS, cardsToPlay, sendTos : new(){battleMessage.SendFrom});
            network.broadcast(playedCardsMessage.toJson());
            return CallbackStatus.PROCESSED;
        } else if (battleMessage.Type == BattleMessageType.PLAYED_CARDS) {
            Debug.Log("Received other player played cards.");
            // Process played cards

            // Print all cards played from battleMessage.CardsPlayed
            foreach (CardName card in battleMessage.CardsPlayed) {
                Debug.Log("Received card: " + card);
            }

            Debug.Log("Printing othersCardsToPlay");
            foreach (KeyValuePair<string, List<CardName>> entry in othersCardsToPlay)
            {
                Debug.Log($"Key: {entry.Key}, Value: {entry.Value}");
            }


            if (!othersCardsToPlay.ContainsKey(battleMessage.SendFrom)) {
                othersCardsToPlay.Add(battleMessage.SendFrom,  battleMessage.CardsPlayed);
            }

            Debug.Log("Printing othersCardsToPlay AFTER adding");
            foreach (KeyValuePair<string, List<CardName>> entry in othersCardsToPlay)
            {
                Debug.Log($"Key: {entry.Key}, Value: {entry.Value}");
            }

            if (battleStatus != BattleStatus.TURN_IN_PROGRESS) {
                int notReceived = 0;
                foreach (string id in partyMembers.Keys) {
                    Debug.Log("Processing ID: " + id);
                    if (id == GameState.Instance.myID || othersCardsToPlay.ContainsKey(id)) {
                        Debug.Log("Already received cards from " + id);
                        continue;
                    }
                    Debug.Log("Not received cards from " + id);
                    notReceived++;
                }

                if (notReceived == 0 ) {
                    battleStatus = BattleStatus.RESOLVING_PLAYED_CARDS;
                    EndOfTurnActions();
                }
            }

            return CallbackStatus.PROCESSED;
        }
        Debug.Log("Unhandled battle message type");
        return CallbackStatus.DORMANT;
    }

    private void BroadcastCardsPlayed() {
        BattleMessage cardsPlayedMessage = new BattleMessage(BattleMessageType.PLAYED_CARDS, cardsToPlay, sendTos : new());
        network.broadcast(cardsPlayedMessage.toJson());
    }
}

public enum BattleMessageType {
    REQUEST_PLAYED_CARDS,
    PLAYED_CARDS,
}

public enum BattleStatus {
    TURN_IN_PROGRESS,
    TURN_ENDED,
    RESOLVING_PLAYED_CARDS,
    SENT_PLAYED_CARDS
}

public class BattleMessage : MessageInfo
{
    public MessageType messageType {get; set;}
    public BattleMessageType Type {get; set;}
    public List<CardName> CardsPlayed {get; set;}
    public string SendFrom {get; set;}
    public List<string> SendTos {get; set;}

    [JsonConstructor]
    public BattleMessage(BattleMessageType type, List<CardName> cardsPlayed, List<string> sendTos, string sendFrom = "") {
        messageType = MessageType.BATTLEMESSAGE;
        Type = type;
        CardsPlayed = cardsPlayed == null ? new() : cardsPlayed;
        SendTos = sendTos == null ? new() : sendTos;
        SendFrom = sendFrom == "" ? GameState.Instance.myID : sendFrom;
    }

    public string toJson() {
        return JsonConvert.SerializeObject(this);
    }

    public string processMessageInfo() {
        return "";
    }
}