using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public class LobbyManager : MonoBehaviour {
    // Singleton
    public static LobbyManager selfReference;

    // UI
    private LobbyUIManager lobbyUIManager;
    
    // Constants
    private readonly int maxPlayerCount = 6;

    // Messaging and identification
    private Dictionary<string, string> players = new();
    private string myID;
    private string myName;
    private string roomCode = "";
    private bool isLeader = false;
    private string leaderID = "";

    // Messaging
    private NetworkUtils network;
    private List<string> connectedDevices = new(); /* Tracking only for disconnections. */
    private bool AcceptMessages = false;
    private int msgFreq = 0;
    private int msgFreqCounter = 0;
    private int roomFindingTimeout = 200;
    private bool joinedLobby = false;
    private bool iAmIn = false;
    private int disconnectCount = 0;
    private List<string> playerIDs;
    
    // Initialisation
	void Awake () {
		if(!selfReference) {
			selfReference = this;
            lobbyUIManager = GetComponent<LobbyUIManager>();
            myID = SystemInfo.deviceUniqueIdentifier;
            msgFreq = maxPlayerCount;
            network = NetworkManager.Instance.NetworkUtils;
			DontDestroyOnLoad(gameObject);
		}else 
            Destroy(gameObject);
	}

    // Exposed methods for buttons
    // When the player wants to leave the lobby and go back to the room selection screen.
    public void ExitLobby() {
        AcceptMessages = false;

        // Tell the leader to remove you from the list of players.
        if (isLeader) {
            // If you are the leader, tell everyone to exit the lobby.
            LobbyMessage leaveLobbyMessage = new(LobbyMessageType.LEADER_LEAVE, true, "");
            network.broadcast(leaveLobbyMessage.toJson());
        }
        network.stopAdvertising();
        network.stopDiscovering();

        // Reset the lobby.
        players = new Dictionary<string, string>();
        myID = SystemInfo.deviceUniqueIdentifier;
        leaderID = "";
        connectedDevices = new List<string>();
        iAmIn = false;
        joinedLobby = false;
        playerIDs = new();
        lobbyUIManager.ExitLobby();

        // Reset gamestate
        GameState.Instance.Reset();
    }

    // Start game as leader and broadcast to everyone.
    public void LeaderStartGame() {
        if (!isLeader)
            return;

        // Create a list of all the players id
        playerIDs = new(players.Keys);

        // Add myself as player
        players.Add(myID, myName);

        // Initialise the game state.
        GameState gameState = GameState.Instance;
        gameState.Initialize(myID, roomCode, players);

        // Single player start
        if (players.Count == 1) {
            StartGame();
        }
    }

    // Send round of messages.
    public void SendMessages() {
        if (msgFreqCounter < msgFreq) {
            msgFreqCounter++;
            return;
        }

        msgFreqCounter = 0;

        if (!AcceptMessages) {
            Debug.Log("Not accepting messages.");
            return;
        }

        if (isLeader) {
            // Leader check if any devices have disconnected.
            List<string> connectedDevices = network.getConnectedDevices();
            foreach (string id in players.Keys) {
                if (!connectedDevices.Contains(id) && !id.Equals(myID)) {
                    RemovePlayer(id);
                }
            }
            
            // Check if game has started and broadcast the game state to everyone.
            if (GameState.Instance.Initialized) {
                // If the game has started, broadcast the game state to everyone.
                LobbyMessage gameStateMessage = new(LobbyMessageType.LEADER_START, true, GameState.Instance.ToMessage().toJson());
                network.broadcast(gameStateMessage.toJson());
            } else {
                // Otherwise, broadcast the list of players to everyone in the lobby.
                foreach (string id in connectedDevices) {
                    LobbyMessage lobbyMessage = new(isLeader, players, myName, id);
                    network.send(lobbyMessage.toJson(), id);
                }
            }
        } else {
            Debug.Log("Connected Devices: " + network.getConnectedDevices().Count);

            // If I lost connection to the leader then I should exit the lobby after 10 seconds
            connectedDevices = network.getConnectedDevices();
            if (!leaderID.Equals("") && !connectedDevices.Contains(leaderID)) {
                if (disconnectCount < 100) {
                    disconnectCount++;
                } else {
                    disconnectCount = 0;
                    ExitLobby();
                }
            }

            // If I am in the lobby, send a message to the leader that I am in, don't stop until I am in the list.
            if (joinedLobby && !iAmIn) {
                Debug.Log("Sending I AM IN message to " + leaderID);
                LobbyMessage amIInMessage = new(LobbyMessageType.MEMBER_I_AM_IN, false, myName, leaderID);
                network.send(amIInMessage.toJson(), leaderID);
            }
        }
    }

    // Common method to start the game for both leader and non-leader.
    private void StartGame() {
        AcceptMessages = false;
        // Load the game scene.
        SceneManager.LoadScene("Gameplay");
    }

    private void AddPlayer(string id, string name) {
        if (players.ContainsKey(id))
            return;
        players.Add(id, name);
        lobbyUIManager.AddPlayer(name);
    }

    private void RemovePlayer(string id) {
        if (!players.ContainsKey(id)) {
            Debug.Log("Player not found in list of players.");
            return;
        }
        lobbyUIManager.RemovePlayer(players[id]);
        players.Remove(id);
    }

    public void SetUpLobby(string roomCode, bool isLeader) {
        this.roomCode = roomCode;
        myName = PlayerPrefs.GetString("name");

        // Start discovering and advertising.
        network.setRoomCode(roomCode);
        network.startDiscovering();

        this.isLeader = isLeader;
        if (!isLeader) {
            // Wait for room to be found.
            if (!FindRoom())
                throw new Exception("Room not found.");
        }

        network.startAdvertising();

        AcceptMessages = true;

        List<string> allPlayers = new(players.Values);
        allPlayers.Add(PlayerPrefs.GetString("name"));
        lobbyUIManager.SetPlayers(allPlayers);
        lobbyUIManager.SetUpLobby(roomCode, isLeader);
    }

    private bool FindRoom() {
        int count = 0;
        while (count < roomFindingTimeout) {
            List<string> connections = network.getConnectedDevices();
            Debug.Log("Connections: " + connections.Count);
            if (connections.Count != 0)
                return true;
            System.Threading.Thread.Sleep(50);
            count++;
        }
        return false;
    }

    public CallbackStatus HandleMessage(Message message) {
        LobbyMessage lobbyMessage = (LobbyMessage) message.messageInfo;

        Debug.Log("Accepting status: " + AcceptMessages);
        
        // Ignore if no longer accepting messages.
        if (!AcceptMessages)
            return CallbackStatus.DORMANT;
        
        Debug.Log("Lobby message is not dormanted, processing...");

        switch (lobbyMessage.Type) {
            case LobbyMessageType.MEMBER_I_AM_IN:
                // Ignore if not leader.
                if (!isLeader)
                    break;

                // Ignore if player already in list.
                if (players.ContainsKey(message.sentFrom))
                    break;

                // Ignore if max player count reached.
                if (players.Count >= maxPlayerCount)
                    break;

                Debug.Log("Player joined: " + message.sentFrom + "  "+lobbyMessage.Message);

                // Add player to the list of players.
                AddPlayer(message.sentFrom, lobbyMessage.Message);

                // Learn my id from this message
                if (lobbyMessage.SendTo != "")
                    myID = lobbyMessage.SendTo;
                break;
            case LobbyMessageType.LEADER_PLAYERS:
                if (isLeader)
                    break;

                // Learn my id and leader's id from this message
                myID = lobbyMessage.SendTo;
                leaderID = message.sentFrom;

                // Add leader to the list of players.
                AddPlayer(message.sentFrom, lobbyMessage.Message);

                // Add all players to the list of players.
                foreach (KeyValuePair<string, string> player in lobbyMessage.Players) {
                    // Check if the player is me.
                    if (player.Key.Equals(myID)) {
                        iAmIn = true;
                        continue;
                    }
                        
                    AddPlayer(player.Key, player.Value);
                }

                // Remove all players that are not in the list of players.
                foreach (KeyValuePair<string, string> player in players) {
                    if (!lobbyMessage.Players.ContainsKey(player.Key) 
                        && !player.Key.Equals(myID) && !player.Key.Equals(leaderID))
                        RemovePlayer(player.Key);
                }

                joinedLobby = true;
                break;
            case LobbyMessageType.LEADER_START:
                if (isLeader)
                    break;
                
                // Member may have received this message before.
                GameState gameState = GameState.Instance;
                if (!gameState.Initialized) {
                    gameState.InitializeFromMessage(GameStateMessage.fromJson(lobbyMessage.Message), roomCode, myID);
                }

                // Send a message to the leader that I am ready to start the game.
                LobbyMessage memberStartMessage = new(LobbyMessageType.MEMBER_START, false, myName, leaderID);
                network.send(memberStartMessage.toJson(), leaderID);
                break;
            case LobbyMessageType.MEMBER_START:
                if (!isLeader)
                    break;

                // Send a message to say that their start message was received and it's okay to start the game.
                if (playerIDs.Contains(message.sentFrom))
                    playerIDs.Remove(message.sentFrom);
                LobbyMessage leaderStartMessage = new(LobbyMessageType.LEADER_START_OK, true, myName);
                network.send(leaderStartMessage.toJson(), message.sentFrom);

                // Once everyone has sent the start message, start the game.
                if (playerIDs.Count == 0)
                    StartGame();
                break;
            case LobbyMessageType.LEADER_LEAVE:
                if (isLeader)
                    break;

                ExitLobby();
                break;
            case LobbyMessageType.LEADER_START_OK:
                if (isLeader)
                    break;

                StartGame();
                break;
        }
        return CallbackStatus.PROCESSED;
    }
}

// Handlemessages with bool to tell it to stop
// do not kill thread to handle messages (track them tho) (will have 3 or 4 at end of game to kill)

public enum LobbyMessageType {
    MEMBER_I_AM_IN,
    LEADER_START,
    LEADER_PLAYERS,
    LEADER_LEAVE,
    MEMBER_START,
    LEADER_START_OK,
}

public class LobbyMessage : MessageInfo {
    public MessageType messageType {get; set;}
    public LobbyMessageType Type {get; set;}
    public string Message {get;}
    public Dictionary<string, string> Players {get;}
    public bool IsLeader {get;}
    public string SendTo {get;}

    public LobbyMessage(LobbyMessageType type, bool isLeader, string message, string sendTo = "") {
        messageType = MessageType.LOBBYMESSAGE;
        Type = type;
        IsLeader = isLeader;
        Message = message;
        SendTo = sendTo;
    }

    public LobbyMessage(bool isLeader, Dictionary<string, string> players, string myName, string sendTo = "") {
        if (!isLeader)
            throw new Exception("Only leader can send this message.");
        messageType = MessageType.LOBBYMESSAGE;
        Type = LobbyMessageType.LEADER_PLAYERS;
        IsLeader = isLeader;
        Players = players;
        Message = myName;
        SendTo = sendTo;
    }

    [JsonConstructor]
    public LobbyMessage(LobbyMessageType type, bool isLeader, string message, Dictionary<string, string> players, string sendTo) {
        messageType = MessageType.LOBBYMESSAGE;
        Type = type;
        IsLeader = isLeader;
        Message = message;
        Players = players;
        SendTo = sendTo;
    }

    public string toJson() {
        return JsonConvert.SerializeObject(this);
    }

    public string processMessageInfo() {
        return "";
    }
}
