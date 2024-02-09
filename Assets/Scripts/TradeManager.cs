using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.UI;

public class TradeManager : MonoBehaviour {
    public static TradeManager selfReference;

    // UI
    [SerializeField]
    private GameObject playerIcon;
    [SerializeField]
    private TMP_Text playerName;

    [SerializeField]
    private GameObject cardObject;

    [SerializeField]
    private TMP_Text inventoryMessage;

    [SerializeField]
    private TMP_Text tradeMessage;

    [SerializeField]
    private GameObject interfaceParent;
    
    [SerializeField]
    private GameObject cardsManagerPrefab;
    private GameObject cardsManager;
    private GameObject cardDisplay;

    private const string tradeDeclinedMessage = "Card was declined...";
    private const string tradeAcceptedMessage = "Card was accepted!";
    private const string tradeLostMessage = "The card was lost on the way...";
    private const string tradeCompleteMessage = "You received the card!";
    private const string tradeIgnoredMessage = "The trade was ignored...";

    // Both player fields.
    private bool tradeInProgress;
    private bool acceptTrades = true;
    private bool acceptTrade = false;
    private bool declineTrade = false;

    private NetworkUtils network;

    // Sender fields.
    private Player tradeTo;
    private bool tradeAccepted = false;
    private bool tradeDeclined = false;

    // Receiver fields.
    private string tradeFromID;
    private CardName cardName;

    private const int timeoutCount = 100; // 10 * 100ms = 1s
    private const int msgFreq = 100; // 100ms
    
    // Start is called before the first frame update
    void Awake() {
        if (!selfReference) {
            selfReference = this;
            network = NetworkManager.Instance.NetworkUtils;
            cardsManager = Instantiate(cardsManagerPrefab);
            gameObject.SetActive(false);
        } else {
            Destroy(gameObject);
        }
    }

    // Stop accepting trades.
    public void DisallowTrades() {
        acceptTrades = false;
    }

    // Allow trades to be accepted.
    public void AllowTrades() {
        acceptTrades = true;
    }

    // Start the trade. 1st player sends a card to 2nd player.
    public void StartTrade(Player player, Card card) {
        // Open trade UI
        OpenInterface();

        tradeTo = player;

        inventoryMessage.text = "";
        // Send message on a loop until reponse.
        TradeMessage message = TradeMessage.SendMessage(card.name, GameState.Instance.MyPlayer.Id, player.Id);
        int counter = 0;
        while (counter < timeoutCount && !tradeAccepted && !tradeDeclined) {
            network.broadcast(message.toJson());
            counter++;
            System.Threading.Thread.Sleep(msgFreq);
        }

        if (tradeAccepted) {
            inventoryMessage.text = tradeAcceptedMessage;
        } else if (tradeDeclined) {
            inventoryMessage.text = tradeDeclinedMessage;
            return;
        } else {
            inventoryMessage.text = tradeIgnoredMessage;
            return;
        }

        // Trade was accepted, remove card from inventory.
        GameState.Instance.RemoveCard(card.name);

        // MessageHandler will handle the rest of the trade.
    }

    // Happens at end of trade or when trade is cancelled.
    public void CloseTrade() {
        // Close trade UI
        gameObject.SetActive(false);
        tradeTo = null;
    }

    // For the second player to accept the trade with their card.
    public void AcceptTrade() {
        acceptTrade = true;
        declineTrade = false;
    }

    // For the second player to decline the trade. 
    public void DeclineTrade() {
        declineTrade = true;
        acceptTrade = false;

        if (!tradeInProgress) {
            declineTrade = false;
        }
        CloseTrade();
    }

    public void CloseInterface() {
        interfaceParent.SetActive(false);
    }

    public void OpenInterface() {
        interfaceParent.SetActive(true);
    }


    private int msgCounter = 0;
    private const int maxMsgCounter = 50;

    public CallbackStatus HandleMessage(Message message) {
        if (message.messageInfo.messageType != MessageType.TRADE) {
            return CallbackStatus.NOT_PROCESSED;
        }

        if (!acceptTrades) {
            return CallbackStatus.DORMANT;
        }

        TradeMessage tradeMessage = (TradeMessage) message.messageInfo;
        if (tradeMessage.type == TradeMessageType.TRADE_SEND) { // Receiver
            // Set up trade UI
            Player sender = GameState.Instance.GetPlayerByID(tradeMessage.sentFrom);
            playerName.text = sender.Name;
            ((Image) playerIcon.GetComponentInChildren(typeof(Image))).sprite = sender.Icon;

            // Get the card to be displayed in the trade UI
            Card card = cardsManager.GetComponent<CardsUIManager>().findCardDetails(tradeMessage.cardName);
            cardObject.GetComponent<CardRenderer>().renderCard(card);

            // Open trade UI
            gameObject.SetActive(true);
            tradeFromID = tradeMessage.sentFrom;
            cardName = tradeMessage.cardName;
            tradeInProgress = true;
        } else if (tradeMessage.type == TradeMessageType.TRADE_ACCEPT) { // Sender
            tradeAccepted = true;
        } else if (tradeMessage.type == TradeMessageType.TRADE_DECLINE) { // Sender
            tradeDeclined = true;
            tradeInProgress = false;
        } else if (tradeMessage.type == TradeMessageType.TRADE_COMPLETE) { // Receiver
            // Add card to inventory
            GameState.Instance.AddCard(tradeMessage.cardName);

            acceptTrade = false;
            tradeInProgress = false;
        }

        // Potential timeout for receiver when acceptTrade is true and have not received a TRADE_COMPLETE message.
        if (acceptTrade && tradeInProgress) {
            msgCounter++;
        }

        // Timeout occured, stop trade.
        if (msgCounter >= maxMsgCounter) {
            tradeInProgress = false;
            msgCounter = 0;

            // Trigger lost card
            if (tradeInProgress) {
                inventoryMessage.text = tradeLostMessage;
                CloseInterface();
            }
        }

        return CallbackStatus.PROCESSED;
    }

    private int msgFreqCounter = 0;

    public void SendMessages() {
        if (msgFreqCounter < msgFreq * 0.1) {
            msgFreqCounter++;
            return;
        }

        msgFreqCounter = 0;

        // Perform for the next 10 interations, otherwise stop
        string myID = GameState.Instance.MyPlayer.Id;
        if (tradeInProgress) {
            // Send message to the sender
            if (acceptTrade) {
                TradeMessage message = TradeMessage.AcceptMessage(cardName, myID, tradeFromID);
                network.broadcast(message.toJson());
            } else if (declineTrade) {
                TradeMessage message = TradeMessage.DeclineMessage(cardName, myID, tradeFromID);
                network.broadcast(message.toJson());
            } else if (tradeAccepted) {
                // Send message to the receiver
                TradeMessage message = TradeMessage.CompleteMessage(cardName, myID, tradeTo.Id);
                network.broadcast(message.toJson());
            } 
        }
    }
}

public enum TradeMessageType {
    TRADE_SEND,     // Sent when initiating a trade from 1st player
    TRADE_ACCEPT,   // Sent when accepting a trade from 2nd player
    TRADE_DECLINE,  // Sent when declining a trade from 2nd player
    TRADE_COMPLETE, // Sent when trade is complete from 1st player
}

public class TradeMessage : MessageInfo {
    public MessageType messageType { get; set; }
    public TradeMessageType type { get; set; }

    public CardName cardName { get; set; }

    public string sendTo { get; set; }
    public string sentFrom { get; set; }
    
    [JsonConstructor]
    public TradeMessage(TradeMessageType type, CardName cardName, string sendTo, string sentFrom) {
        messageType = MessageType.TRADE;
        this.type = type;
        this.cardName = cardName;
        this.sendTo = sendTo;
        this.sentFrom = sentFrom;
    }

    public static TradeMessage SendMessage(CardName cardName, string sentFrom, string sendTo) {
        return new TradeMessage(TradeMessageType.TRADE_SEND, cardName, sendTo, sentFrom);
    }

    public static TradeMessage AcceptMessage(CardName cardName, string sentFrom, string sendTo) {
        return new TradeMessage(TradeMessageType.TRADE_ACCEPT, cardName, sendTo, sentFrom);
    }

    public static TradeMessage DeclineMessage(CardName cardName, string sentFrom, string sendTo) {
        return new TradeMessage(TradeMessageType.TRADE_DECLINE, cardName, sendTo, sentFrom);
    }

    public static TradeMessage CompleteMessage(CardName cardName, string sentFrom, string sendTo) {
        return new TradeMessage(TradeMessageType.TRADE_COMPLETE, cardName, sendTo, sentFrom);
    }

    public string toJson() {
        return JsonConvert.SerializeObject(this);
    }

    public string processMessageInfo() {
        return "";
    }
}
