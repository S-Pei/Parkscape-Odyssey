using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.UI;


public class EncounterController : MonoBehaviour
{
    [SerializeField]
    private GameObject encounterLobbyOverlay;
    private EncounterUIManager encounterUIManager;
    private EncounterLobbyUIManager encounterLobbyUIManager;

    [SerializeField]
    private GameObject monsterManager;
    private MonsterController monsterController;

    [SerializeField]
    private GameObject encounterSpawn;

    [SerializeField]
    private GameObject interfacePanel;

    [SerializeField]
    private GameObject encounterFoundPopup;

    [SerializeField]
    private GameObject gameplayCanvas;

    private string leaderId;

    private EncounterStatus encounterStatus;

    // p2p network
    private NetworkUtils network;
    private bool AcceptMessages = true;
    private readonly float baseFreq = 0.1f;
    private int msgFreq = 0;
    private int msgFreqCounter = 0;

    
    void Start() {
        // Setup p2p network
        msgFreq = GameState.Instance.maxPlayerCount;
        network = NetworkManager.Instance.NetworkUtils;
        InvokeRepeating("HandleMessages", 0.0f, baseFreq);
        encounterUIManager = GetComponent<EncounterUIManager>();
        monsterController = monsterManager.GetComponent<MonsterController>();

        CreateMonsterSpawn(); // TEMPORARY
    }

    private void CreateMonsterSpawn() {
        // Generate monsters for the encounter.
        List<Monster> monsters = GenerateEncounterMonsters();

        // Generate unique id for the encounter.
        string encounterId = Guid.NewGuid().ToString();

        // Create a new enemy spawn
        GameObject monsterSpawn = Instantiate(encounterSpawn, interfacePanel.transform);
        EncounterSpawnManager encounterSpawnManager = monsterSpawn.GetComponent<EncounterSpawnManager>();
        encounterSpawnManager.EncounterSpawnInit(encounterId, monsters);

        // TODO: Set the position of the monster to predetermined position with an algorithm.
        monsterSpawn.transform.localPosition = new Vector3(0, 0, 0);
    }

    private List<Monster> GenerateEncounterMonsters() {
        List<Monster> monsters = new List<Monster>();

        // Select a random number between 1 and maxPlayerCount to determine the number of monsters in the encounter.
        int numMonsters = UnityEngine.Random.Range(1, GameState.Instance.maxPlayerCount);

        // Select a type of monster for the encounter.
        MonsterName monsterName = (MonsterName) UnityEngine.Random.Range(0, Enum.GetValues(typeof(MonsterName)).Length);

        // Create the monsters.
        for (int i = 0; i < numMonsters; i++) {
            Monster monster = monsterController.createMonster(monsterName);
            monsters.Add(monster);
        }
        return monsters;
    }

    public void CreateEncounterLobby(string encounterId, List<Monster> monsters) {
        // AcceptMessages = true;

        GameObject encounterLobby = Instantiate(encounterLobbyOverlay);
        
        encounterLobbyUIManager = encounterLobby.GetComponent<EncounterLobbyUIManager>();
        encounterLobbyUIManager.LeaderEncounterLobbyInit(encounterId, monsters);

        // Broadcast to all players that an encounter has been found.
        BroadcastFoundEncounterMessage();
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
                // Broadcast to all players that an encounter has been found.
                ShowEncounterFoundPopup();
                break;
            case EncounterMessageType.JOIN_ENCOUNTER:
                // Add player to the encounter lobby.
                string playerName = GameState.Instance.PlayersDetails[message.sentFrom].Name;
                encounterLobbyUIManager.MemberJoinedParty(playerName);
                SendJoinedEncounterConfirmationMessage(message.sentFrom);
                break;
            case EncounterMessageType.JOINED_ENCOUNTER_CONFIRMATION:
                // Stop sending join encounter messages.
                StopSendingJoinEncounterMessages();
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

        if (encounterStatus == EncounterStatus.JOINING_LOBBY) {
            EncounterMessage encounterMessage = new EncounterMessage(EncounterMessageType.JOIN_ENCOUNTER);
            Debug.Log("Sending join encounter message to leader: " + leaderId);
            network.send(encounterMessage.toJson(), leaderId);
        }
    }

    private void BroadcastFoundEncounterMessage() {
        EncounterMessage encounterMessage = new EncounterMessage(EncounterMessageType.FOUND_ENCOUNTER);
        network.broadcast(encounterMessage.toJson());
    }

    public void AcceptJoinEncounter() {
        // AcceptMessages = true;

        // Send a message to the leader to request to join encounter.
        SendJoinEncounterMessage();
    }

    public void SendJoinEncounterMessage() {
        encounterStatus = EncounterStatus.JOINING_LOBBY;
    }

    private void ShowEncounterFoundPopup() {
        GameObject popup = Instantiate(encounterFoundPopup, gameplayCanvas.transform);
        popup.transform.GetChild(1).gameObject.GetComponent<Button>().onClick.AddListener(AcceptJoinEncounter);
        popup.transform.GetChild(2).gameObject.GetComponent<Button>().onClick.AddListener(() => Destroy(popup));
    }

    private void SendJoinedEncounterConfirmationMessage(string sendTo) {
        EncounterMessage encounterMessage = new EncounterMessage(EncounterMessageType.JOINED_ENCOUNTER_CONFIRMATION);
        network.send(encounterMessage.toJson(), sendTo);
    }

    private void StopSendingJoinEncounterMessages() {
        encounterStatus = EncounterStatus.JOINED_LOBBY;
    }
}


public enum EncounterMessageType {
    FOUND_ENCOUNTER,
    JOIN_ENCOUNTER,
    JOINED_ENCOUNTER_CONFIRMATION,
}

enum EncounterStatus {
    JOINING_LOBBY,
    JOINED_LOBBY,
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