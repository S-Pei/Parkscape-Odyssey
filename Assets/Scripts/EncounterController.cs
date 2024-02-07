using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;


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
        encounterUIManager = GetComponent<EncounterUIManager>();
        monsterController = monsterManager.GetComponent<MonsterController>();
    }

    private void CreateMonsterSpawn() {
        // Generate monsters for the encounter.
        List<Monster> monsters = GenerateEncounterMonsters();

        // Generate unique id for the encounter.
        string encounterId = Guid.NewGuid().ToString();

        // Create a new enemy spawn
        GameObject monsterSpawn = Instantiate(encounterSpawn, interfacePanel.transform);

        // TODO: Set the position of the monster to predetermined position with an algorithm.
        monsterSpawn.transform.position = new Vector3(0, 0, 0);
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
        AcceptMessages = true;

        GameObject encounterLobby = Instantiate(encounterLobbyOverlay);
        
        encounterLobbyUIManager = encounterLobby.GetComponent<EncounterLobbyUIManager>();
        encounterLobbyUIManager.LeaderEncounterLobbyInit(encounterId, monsters);
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
                // openEncounterLobbyOverlay();
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