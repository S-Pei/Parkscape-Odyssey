using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.UI;
using System.Linq;


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

    private EncounterStatus encounterStatus = EncounterStatus.IDLE;

    // current encounter monster details
    private List<Monster> monsters;
    private List<List<Skill>> skillSequences;
    private string encounterId;

    // p2p network
    private NetworkUtils network;
    private bool AcceptMessages = true;
    private readonly float baseFreq = 0.1f;
    private int msgFreq = 0;
    private int msgFreqCounter = 0;

    private bool receivedEncounter = false;

    public static EncounterController selfReference;

    void Awake() {
        if (!selfReference) {
			selfReference = this;
			DontDestroyOnLoad(gameObject);
		} else 
            Destroy(gameObject);
    }
    
    void Start() {
        // Setup p2p network
        msgFreq = GameState.Instance.maxPlayerCount;
        network = NetworkManager.Instance.NetworkUtils;
        // InvokeRepeating("HandleMessages", 0.0f, baseFreq);
        encounterUIManager = GetComponent<EncounterUIManager>();
        monsterController = monsterManager.GetComponent<MonsterController>();

        CreateMonsterSpawn(); // TEMPORARY
    }

    private void CreateMonsterSpawn() {
        // Generate monsters for the encounter.
        List<Monster> monsters = GenerateEncounterMonsters();

        // Generates skill sequences for the monsters.
        List<List<Skill>> skillSequences = GenerateMonsterSkillSequences(monsters);

        // Generate unique id for the encounter.
        string encounterId = Guid.NewGuid().ToString();

        // Create a new enemy spawn
        GameObject monsterSpawn = Instantiate(encounterSpawn, interfacePanel.transform);
        EncounterSpawnManager encounterSpawnManager = monsterSpawn.GetComponent<EncounterSpawnManager>();
        encounterSpawnManager.EncounterSpawnInit(encounterId, monsters, skillSequences);

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

    private List<List<Skill>> GenerateMonsterSkillSequences(List<Monster> monsters) {
        List<List<Skill>> skillSequences = new List<List<Skill>>();

        foreach (Monster monster in monsters) {
            List<Skill> sequence = monster.skills.OrderBy(i => Guid.NewGuid()).ToList();
            skillSequences.Add(sequence);
        }

        return skillSequences;
    }

    // Shows encounter lobby on the UI
    public void SpawnEncounterLobby(string encounterId, List<Monster> monsters, List<List<Skill>> skillSequences) {
        this.monsters = monsters;
        this.skillSequences = skillSequences;
        this.encounterId = encounterId;

        GameObject encounterLobby = Instantiate(encounterLobbyOverlay);
        
        encounterLobbyUIManager = encounterLobby.GetComponent<EncounterLobbyUIManager>();
        encounterLobbyUIManager.LeaderEncounterLobbyInit(encounterId, monsters);
    }

    // Called from encounter spawn manager when leader initiates the encounter lobby
    public void CreateEncounterLobby(string encounterId, List<Monster> monsters, List<List<Skill>> skillSequences) {
        // AcceptMessages = true;
        SpawnEncounterLobby(encounterId, monsters, skillSequences);
        
        // Broadcast to all players that an encounter has been found.
        encounterStatus = EncounterStatus.START_LOBBY;
    }

    // ------------------------------ P2P NETWORK ------------------------------
    public CallbackStatus HandleMessage(Message message) {
        // Ignore if no longer accepting messages.
        if (!AcceptMessages)
            return CallbackStatus.DORMANT;

        EncounterMessage encounterMessage = (EncounterMessage) message.messageInfo;
        switch (encounterMessage.Type) {
            case EncounterMessageType.FOUND_ENCOUNTER:
                // Leader broadcast to all players that an encounter has been found.
                ShowEncounterFoundPopup();
                break;
            case EncounterMessageType.JOIN_ENCOUNTER:
                // Leader adds player to the encounter lobby
                string playerName = GameState.Instance.PlayersDetails[message.sentFrom].Name;
                encounterLobbyUIManager.MemberJoinedParty(playerName);
                // sends monster info to other players 
                SendJoinedEncounterConfirmationMessage(message.sentFrom);
                break;
            case EncounterMessageType.JOINED_ENCOUNTER_CONFIRMATION:
                // Players stop sending join encounter messages to leader and processes monster info
                StopSendingJoinEncounterMessagesAndShowLobby(encounterMessage);
                break;
        }

        return CallbackStatus.PROCESSED;
    }

    public void SendMessages() {
        if (network == null)
            return;

        if (!AcceptMessages) {
            return;
        }

        if (encounterStatus == EncounterStatus.START_LOBBY) {
            EncounterMessage encounterMessage = new EncounterMessage(EncounterMessageType.FOUND_ENCOUNTER);
            network.broadcast(encounterMessage.toJson());
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
        if (encounterStatus == EncounterStatus.JOINING_LOBBY) {
            return;
        }
        GameObject popup = Instantiate(encounterFoundPopup, gameplayCanvas.transform);
        foreach (Transform child in popup.transform) {
            if (child.name == "AcceptButton") {
                child.GetComponent<Button>().onClick.AddListener(AcceptJoinEncounter);
            }
            if (child.name == "CancelButton") {
                child.GetComponent<Button>().onClick.AddListener(() => Destroy(popup));
            }
        }
    }

    // Sends confirmation to player for joining encounter lobby together with monster details
    private void SendJoinedEncounterConfirmationMessage(string sendTo) {
        List<MonsterName> monsterNames = new List<MonsterName>();
        List<int> health = new List<int>();
        List<int> defense = new List<int>();
        List<int> defenseAmount = new List<int>();
        List<int> baseDamage = new List<int>();
        List<EnemyLevel> levels = new List<EnemyLevel>();
        foreach (Monster monster in monsters) {
            monsterNames.Add(monster.name);
            health.Add(monster.Health);
            defense.Add(monster.Defense);
            defenseAmount.Add(monster.defenseAmount);
            baseDamage.Add(monster.BaseDamage);
            levels.Add(monster.level);
        }
        EncounterMessage encounterMessage 
          = new EncounterMessage(EncounterMessageType.JOINED_ENCOUNTER_CONFIRMATION,
                                monsterNames, health, defense, defenseAmount, baseDamage, skillSequences, levels, this.encounterId);
        network.send(encounterMessage.toJson(), sendTo);
    }

    private void StopSendingJoinEncounterMessagesAndShowLobby(EncounterMessage encounterMessage) {
        encounterStatus = EncounterStatus.JOINED_LOBBY;
        List<Monster> monsters = ProcessEncounterMessageWithMonsterInfo(encounterMessage);
        SpawnEncounterLobby(encounterMessage.encounterId, monsters, encounterMessage.skills);
    }

    // Process List of monster info to make a list of monsters
    private List<Monster> ProcessEncounterMessageWithMonsterInfo(EncounterMessage encounterMessage) {
        List<Monster> monsters = new List<Monster>();
        List<MonsterName> monsterNames = encounterMessage.names;
        List<int> health = encounterMessage.Health;
        List<int> defense = encounterMessage.Defense;
        List<int> defenseAmount = encounterMessage.defenseAmount;
        List<int> baseDamage = encounterMessage.BaseDamage;
        List<EnemyLevel> levels = encounterMessage.level;
        for (int i = 0; i < monsterNames.Count; i++) {
            Sprite img = monsterController.GetMonsterSprite(monsterNames[i]);
            Monster monster = MonsterFactory.CreateMonsterWithValues(
                monsterNames[i], img, health[i], defense[i], defenseAmount[i], 
                baseDamage[i], encounterMessage.skills[i], levels[i]);
            monsters.Add(monster);
        }
        return monsters;
    }
}


public enum EncounterMessageType {
    FOUND_ENCOUNTER,
    JOIN_ENCOUNTER,
    JOINED_ENCOUNTER_CONFIRMATION,
}

enum EncounterStatus {
    IDLE,
    START_LOBBY,
    JOINING_LOBBY,
    JOINED_LOBBY,
}


public class EncounterMessage : MessageInfo
{
    public MessageType messageType {get; set;}
    public EncounterMessageType Type {get; set;}
    public List<MonsterName> names;

    public List<int> Health { get; private set; } = new List<int>();

    public List<int> Defense { get; private set; } = new List<int>();

    public List<int> defenseAmount = new List<int>();

    public List<int> BaseDamage { get; private set; } = new List<int>();

    public List<List<Skill>> skills = new List<List<Skill>>();

    public List<EnemyLevel> level = new List<EnemyLevel>();
    public string encounterId = "";

    public EncounterMessage(EncounterMessageType type) {
        messageType = MessageType.ENCOUNTERMESSAGE;
        Type = type;
    }

    [JsonConstructor]
    public EncounterMessage(EncounterMessageType type, List<MonsterName> names, List<int> health, List<int> defense, List<int> defenseAmount, 
        List<int> baseDamage, List<List<Skill>> skills, List<EnemyLevel> level, string encounterId) {
        messageType = MessageType.ENCOUNTERMESSAGE;
        Type = type;
        this.names = names;
        Health = health;
        Defense = defense;
        this.defenseAmount = defenseAmount;
        BaseDamage = baseDamage;
        this.skills = skills;
        this.level = level;
        this.encounterId = encounterId;
    }

    public string toJson() {
        return JsonConvert.SerializeObject(this);
    }

    public string processMessageInfo() {
        return "";
    }
}