using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class TradeManager : MonoBehaviour {
    public static TradeManager selfReference;

    private Player tradeTo;
    private Card tradeCard;
    
    // Start is called before the first frame update
    void Awake() {
        if (!selfReference) {
            selfReference = this;
            CloseTrade();
        } else {
            Destroy(gameObject);
        }
    }

    // Start the trade and open the pop up.
    public void StartTrade(Player player, Card card) {
        // Open trade UI
        gameObject.SetActive(true);
        tradeTo = player;
        tradeCard = card;
    }

    // Happens at end of trade or when trade is cancelled.
    public void CloseTrade() {
        // Close trade UI
        gameObject.SetActive(false);
        tradeTo = null;
        tradeCard = null;
    }
}

public enum TradeMessageType {
    TRADE_REQUEST,
    TRADE_DECLINED,
    TRADE_RESPONSE,
    TRADE_COMPLETE,
}

public class TradeMessage : MessageInfo {
    public MessageType messageType { get; set; }

    
    public TradeMessage() {
        messageType = MessageType.TRADE;
    }

    public string toJson() {
        return JsonConvert.SerializeObject(this);
    }

    public string processMessageInfo() {
        return "";
    }
}
