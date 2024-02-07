using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;


public class EncounterController : MonoBehaviour
{
    [SerializeField]
    private GameObject encounterLobbyOverlay;

    // p2p network
    private NetworkUtils network;
    private bool AcceptMessages = false;
    private readonly float baseFreq = 0.1f;
    private int msgFreq = 0;
    private int msgFreqCounter = 0;

    
    void Awake() {
        // Setup p2p network
        msgFreq = GameState.Instance.maxPlayerCount;
        network = NetworkManager.Instance.NetworkUtils;
        InvokeRepeating("HandleMessages", 0.0f, baseFreq);
    }

    public void CreateEncounterLobby() {
        AcceptMessages = true;
        Instantiate(encounterLobbyOverlay);
    }

    // ------------------------------ P2P NETWORK ------------------------------
    private void HandleMessages() {
        // Set up callback for handling incoming messages.
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

    private CallbackStatus HandleMessage(Message message) {
        // Ignore if not an encounter message.
        if (message.messageInfo.messageType != MessageType.ENCOUNTERMESSAGE)
            return CallbackStatus.NOT_PROCESSED;

        // Ignore if no longer accepting messages.
        if (!AcceptMessages)
            return CallbackStatus.DORMANT;

        EncounterMessage encounterMessage = (EncounterMessage) message.messageInfo;
        switch (encounterMessage.Type) {
            case EncounterMessageType.FOUND_ENCOUNTER:
                // openEncounterLobbyPopup();
                break;
        }

        return CallbackStatus.PROCESSED;
    }

    private void SendMessages() {
        if (network == null)
            return;

        if (!AcceptMessages) {
            return;
        }
    }
}


public enum EncounterMessageType {
    FOUND_ENCOUNTER,
}


public class EncounterMessage : MessageInfo
{
    public MessageType messageType {get; set;}
    public EncounterMessageType Type {get; set;}

    [JsonConstructor]
    public EncounterMessage(EncounterMessageType type) {
        messageType = MessageType.ENCOUNTERMESSAGE;
        Type = type;
    }

    public string toJson() {
        return JsonConvert.SerializeObject(this);
    }

    public string processMessageInfo() {
        return "";
    }
}