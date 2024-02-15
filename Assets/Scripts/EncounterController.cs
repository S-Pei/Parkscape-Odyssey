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
    private List<List<SkillName>> skillSequences;
    private string encounterId;
    private bool isLeader = false;

    // p2p network
    private NetworkUtils network;
    private bool AcceptMessages = true;
    private readonly float baseFreq = 0.1f;
    private int msgFreq = 0;
    private int msgFreqCounter = 0;

    private bool receivedEncounter = false;
    private bool inEncounterLobby = false;

    private readonly Dictionary<string, string> partyMembers = new();

    public static EncounterController selfReference;

    private GameObject encounterFoundPopupInstance;

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
        List<List<SkillName>> skillSequences = GenerateMonsterSkillSequences(monsters);

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

    private List<List<SkillName>> GenerateMonsterSkillSequences(List<Monster> monsters) {
        List<List<SkillName>> skillNameSequences = new List<List<SkillName>>();

        foreach (Monster monster in monsters) {
            List<SkillName> shuffledSkillNames = new List<SkillName>();
            List<int> shuffledIndex = Shuffle(monster.skills.Count - 1);
            foreach (int index in shuffledIndex) {
                shuffledSkillNames.Add(monster.skills[index].Name);
            }
            skillNameSequences.Add(shuffledSkillNames);
        }

        return skillNameSequences;
    }

    // Shows encounter lobby on the UI
    public void SpawnEncounterLobby(string encounterId, List<Monster> monsters, List<List<SkillName>> skillSequences) {
        this.monsters = monsters;
        this.skillSequences = skillSequences;
        this.encounterId = encounterId;

        GameObject encounterLobby = Instantiate(encounterLobbyOverlay);
        
        encounterLobbyUIManager = encounterLobby.GetComponent<EncounterLobbyUIManager>();
        encounterLobbyUIManager.EncounterLobbyInit(encounterId, monsters, isLeader);
    }

    // shuffle monster skill indexes
    private List<int> Shuffle(int listSize) {  
        List<int> indexes = Enumerable.Range(0, listSize + 1).ToList();

        while (listSize > 1) {
            // Select a random card from the front of the deck
            // (up to the current position to shuffle) to swap
            listSize--;
            int k = UnityEngine.Random.Range(0, listSize + 1);  
            
            // Swap cards[n] with cards[k]
            int toSwap = indexes[k];  
            indexes[k] = indexes[listSize];  
            indexes[listSize] = toSwap;  
        }
        return indexes;
    }

    // Called from encounter spawn manager when leader initiates the encounter lobby
    public void CreateEncounterLobby(string encounterId, List<Monster> monsters, List<List<SkillName>> skillSequences) {
        isLeader = true;
        SpawnEncounterLobby(encounterId, monsters, skillSequences);

        // Add self as a member of the party in the encounter lobby
        partyMembers.Add(GameState.Instance.myID, GameState.Instance.PlayersDetails[GameState.Instance.myID].Name);
        
        // Broadcast to all players that an encounter has been found.
        BroadcastFoundEncounterMessage();
    }


    // Member accept join encounter
    public void AcceptJoinEncounter() {
        // AcceptMessages = true;
        Debug.Log("Accepting join encounter");
        Destroy(encounterFoundPopupInstance);

        // Send a message to the leader to request to join encounter.
        SendJoinEncounterMessage();
    }

    public void LeaderStartEncounter() {
        // Save monster details for entering encounter
        GameState.Instance.StartEncounter(monsters, skillSequences, partyMembers);
        Debug.Log("Leader Starting encounter");

        SendStartEncounterMessage();
        // AcceptMessages = false;
    }

    private void MemberStartEncounter() {
        GameState.Instance.StartEncounter(monsters, skillSequences, partyMembers);
        Debug.Log("Member Starting encounter");
        GameObject.FindGameObjectWithTag("EncounterLobby").GetComponent<EncounterLobbyUIManager>().StartEncounter();
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
                if (isLeader) {
                    // Leader adds player to the encounter lobby
                    Debug.Log("adding player to encounter lobby: " + encounterMessage.playerId);
                    string playerName = GameState.Instance.PlayersDetails[encounterMessage.playerId].Name;
                    partyMembers.Add(encounterMessage.playerId, playerName);
                    encounterLobbyUIManager.MemberJoinedParty(playerName);
                    // sends monster info to other players 
                    SendJoinedEncounterConfirmationMessage();
                }
                break;
            case EncounterMessageType.JOINED_ENCOUNTER_CONFIRMATION:
                if (!isLeader) {
                    // Players stop sending join encounter messages to leader and processes monster info
                    StopSendingJoinEncounterMessagesAndShowLobby(encounterMessage);
                }
                break;
            case EncounterMessageType.START_ENCOUNTER:
                // member receives notification from encounter leader to start encounter
                if (!isLeader) {
                    MemberStartEncounter();
                }
                break;
        }

        return CallbackStatus.PROCESSED;
    }

    private void BroadcastFoundEncounterMessage() {
        EncounterMessage encounterMessage = new EncounterMessage(EncounterMessageType.FOUND_ENCOUNTER);
        network.broadcast(encounterMessage.toJson());
    }

    public void SendJoinEncounterMessage() {
        EncounterMessage encounterMessage = new EncounterMessage(EncounterMessageType.JOIN_ENCOUNTER, sendTo:leaderId);
        Debug.Log("Sending join encounter message to leader: " + leaderId);
        network.broadcast(encounterMessage.toJson());
    }

    private void ShowEncounterFoundPopup() {
        GameObject popup = Instantiate(encounterFoundPopup, gameplayCanvas.transform);
        foreach (Transform child in popup.transform) {
            if (child.name == "YesButton") {
                child.GetComponent<Button>().onClick.AddListener(AcceptJoinEncounter);
            }
        }
        encounterFoundPopupInstance = popup;
    }

    private void SendStartEncounterMessage() {
        EncounterMessage encounterMessage = new EncounterMessage(EncounterMessageType.START_ENCOUNTER);
        network.broadcast(encounterMessage.toJson());
    }

    // Sends confirmation to player for joining encounter lobby together with monster details
    private void SendJoinedEncounterConfirmationMessage() {
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
          = new EncounterMessage(EncounterMessageType.JOINED_ENCOUNTER_CONFIRMATION, partyMembers,
                                monsterNames, health, defense, defenseAmount, baseDamage, skillSequences, levels, this.encounterId);
        network.broadcast(encounterMessage.toJson());
    }

    private void StopSendingJoinEncounterMessagesAndShowLobby(EncounterMessage encounterMessage) {
        List<Monster> monsters = ProcessEncounterMessageWithMonsterInfo(encounterMessage);
        if (!inEncounterLobby) {
            SpawnEncounterLobby(encounterMessage.encounterId, monsters, encounterMessage.skills);
            inEncounterLobby = true;
        }
        ListPartyMembers(encounterMessage.members);
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
            List<Skill> skills = GetSkillsFromSkillNames(encounterMessage.skills[i]);
            Sprite img = monsterController.GetMonsterSprite(monsterNames[i]);
            Monster monster = MonsterFactory.CreateMonsterWithValues(
                monsterNames[i], img, health[i], defense[i], defenseAmount[i], 
                baseDamage[i], skills, levels[i]);
            monsters.Add(monster);
        }
        return monsters;
    }

    // get corresponding skills from skillname list
    private List<Skill> GetSkillsFromSkillNames(List<SkillName> skillNames) {
        List<Skill> skills = new List<Skill>();
        foreach (SkillName skillName in skillNames) {
            skills.Add(MonsterFactory.skillsController.Get(skillName));
        }
        return skills;
    }

    private void ListPartyMembers(Dictionary<string, string> members) {
        foreach (string id in members.Keys) {
            if (partyMembers.ContainsKey(id))
                continue;
            partyMembers.Add(id, members[id]);
            string playerName = GameState.Instance.PlayersDetails[id].Name;
            encounterLobbyUIManager.MemberJoinedParty(playerName);
        }
    }
}


public enum EncounterMessageType {
    FOUND_ENCOUNTER,
    JOIN_ENCOUNTER,
    JOINED_ENCOUNTER_CONFIRMATION,
    START_ENCOUNTER,
}

enum EncounterStatus {
    IDLE,
    START_LOBBY,
    JOINING_LOBBY,
    JOINED_LOBBY,
    RECEIVED_ENCOUNTER_POPUP,
}


public class EncounterMessage : MessageInfo
{
    public MessageType messageType {get; set;}
    public EncounterMessageType Type {get; set;}
    public Dictionary<string, string> members;
    public string playerId {get; set;}
    public List<MonsterName> names;

    public List<int> Health { get; private set; } = new List<int>();

    public List<int> Defense { get; private set; } = new List<int>();

    public List<int> defenseAmount = new List<int>();

    public List<int> BaseDamage { get; private set; } = new List<int>();

    public List<List<SkillName>> skills = new List<List<SkillName>>();

    public List<EnemyLevel> level = new List<EnemyLevel>();
    public string encounterId = "";

    public string sendTo {get; set;}
    public string sendFrom {get; set;}

    public EncounterMessage(EncounterMessageType type, string sendTo = "") {
        messageType = MessageType.ENCOUNTERMESSAGE;
        playerId = GameState.Instance.myID;
        Type = type;
        this.sendTo = sendTo;
    }

    [JsonConstructor]
    public EncounterMessage(EncounterMessageType type, Dictionary<string, string> members, List<MonsterName> names, List<int> health, List<int> defense, List<int> defenseAmount, 
        List<int> baseDamage, List<List<SkillName>> skills, List<EnemyLevel> level, string encounterId, string sendTo = "", string sendFrom = "") {
        messageType = MessageType.ENCOUNTERMESSAGE;
        Type = type;
        this.members = members;
        this.names = names;
        Health = health;
        Defense = defense;
        this.defenseAmount = defenseAmount;
        BaseDamage = baseDamage;
        this.skills = skills;
        this.level = level;
        this.encounterId = encounterId;
        this.sendTo = sendTo;
        this.sendFrom = GameState.Instance.myID;
    }

    public string toJson() {
        return JsonConvert.SerializeObject(this);
    }

    public string processMessageInfo() {
        return "";
    }
}