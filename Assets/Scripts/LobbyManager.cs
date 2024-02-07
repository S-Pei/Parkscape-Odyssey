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
    private readonly float baseFreq = 0.1f; // per second

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
    
    // Initialisation
	void Awake () {
		if(!selfReference) {
			selfReference = this;
            lobbyUIManager = GetComponent<LobbyUIManager>();
            myID = SystemInfo.deviceUniqueIdentifier;
            msgFreq = maxPlayerCount;
            network = NetworkManager.Instance.NetworkUtils;
            InvokeRepeating("HandleMessages", 0.0f, 1.0f);
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
        lobbyUIManager.ExitLobby();
    }

    // Start game as leader and broadcast to everyone.
    public void LeaderStartGame() {
        if (!isLeader)
            return;

        // Add myself as player
        players.Add(myID, myName);

        // Initialise the game state.
        GameState gameState = GameState.Instance;
        gameState.Initialize(myID, roomCode, players);

        // Share the Gamestate and to start the game.
        LobbyMessage startGameMessage = new(LobbyMessageType.LEADER_START, true, gameState.ToMessage().toJson());
        network.broadcast(startGameMessage.toJson());

        // Reset the lobby and disable scene.
        StartGame();
    }

    void SendMessages() {
        if (network == null)
            return;

        if (!AcceptMessages) {
            Debug.Log("Not accepting messages.");
            return;
        }

        // Leader check if any devices have disconnected.
        if (isLeader) {
            bool changed = false;
            List<string> newConnectedDevices = network.getConnectedDevices();
            Debug.Log("Connected Devices: " + newConnectedDevices.Count);

            foreach (string deviceID in connectedDevices) {
                if (!newConnectedDevices.Contains(deviceID)) {
                    RemovePlayer(deviceID);
                    changed = true;
                }
            }
            
            connectedDevices = newConnectedDevices;
            if (changed) {
                LobbyMessage disconnectedPlayersMessage = new(isLeader, players, myName);
                network.broadcast(disconnectedPlayersMessage.toJson());
            }
        } else {
            // If i have not found the leader yet, broadcast my presence.
            Debug.Log("Connected Devices: " + network.getConnectedDevices().Count);
            if (leaderID.Equals("")) {
                foreach (string id in network.getConnectedDevices()) {
                    Debug.Log("IDK Leader Sending AM I IN message to " + id);
                    LobbyMessage amIInMessage = new(LobbyMessageType.MEMBER_AM_I_IN, false, myName, id);
                    network.send(amIInMessage.toJson(), id);
                }
            } else {
                // Check if I'm in the lobby every maxFreq seconds.
                Debug.Log("Sending AM I IN message to " + leaderID);
                LobbyMessage amIInMessage = new(LobbyMessageType.MEMBER_AM_I_IN, false, myName, leaderID);
                network.send(amIInMessage.toJson(), leaderID);

                // Check if the game has started every maxFreq seconds 
                LobbyMessage startedYetMessage = new(LobbyMessageType.MEMBER_STARTED_YET, false, myName, leaderID);
                network.send(startedYetMessage.toJson(), leaderID);
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

        // Add buffer time for advertising to work.
        System.Threading.Thread.Sleep(1000);

        network.startAdvertising();

        this.isLeader = isLeader;
        if (!isLeader) {
            // Wait for room to be found.
            if (!FindRoom())
                throw new Exception("Room not found.");
        }

        AcceptMessages = true;

        // Tell the leader to add you to the list of players
        if (!isLeader) {
            foreach (string id in network.getConnectedDevices()) {
                Debug.Log("Sending JOIN message to " + id);
                LobbyMessage joinLobbyMessage = new(LobbyMessageType.MEMBER_JOIN, false, myName, id); 
                network.send(joinLobbyMessage.toJson(), id);
            }
        }
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

    private void HandleMessages() {
        // Set up callback for handling incoming messages.
        Func<Message, CallbackStatus> callback = (Message msg) => {
            return HandleMessage(msg);
        };
        network.onReceive(callback);

        // Every msgFreq seconds, send messages.
        if (msgFreqCounter >= msgFreq || leaderID.Equals("")) {
            SendMessages();
            msgFreqCounter = 0;
        } else {
            msgFreqCounter++;
        }
    }

    private CallbackStatus HandleMessage(Message message) {
        // Ignore if not a lobby message.
        if (message.messageInfo.messageType != MessageType.LOBBYMESSAGE)
            return CallbackStatus.NOT_PROCESSED;
        
        
        // Ignore if no longer accepting messages.
        if (!AcceptMessages)
            return CallbackStatus.DORMANT;


        // Ignore if no longer accepting messages.
        if (!AcceptMessages)
            return CallbackStatus.DORMANT;

        LobbyMessage lobbyMessage = (LobbyMessage) message.messageInfo;
        switch (lobbyMessage.Type) {
            case LobbyMessageType.MEMBER_AM_I_IN:
            case LobbyMessageType.MEMBER_JOIN:
                // Ignore if not accepting
                if (!AcceptMessages)
                    break;

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
                
                // Send to each player, including their id.
                foreach (string id in players.Keys) {
                    LobbyMessage newPlayerMessage = new(isLeader, players, myName, id);
                    network.send(newPlayerMessage.toJson(), id);
                }

                // Learn my id from this message
                myID = lobbyMessage.SendTo;
                break;
            case LobbyMessageType.MEMBER_STARTED_YET:
                if (!isLeader)
                    break;

                // Ignore if player not in list.
                if (!players.ContainsKey(message.sentFrom))
                    break;

                // Ignore if game has not started.
                if (!GameState.Instance.Initialized) 
                    break;
                
                // Tell the player if the game has started.
                LobbyMessage startedYetMessage = new(LobbyMessageType.LEADER_START, true, GameState.Instance.ToMessage().toJson(), message.sentFrom);
                network.send(startedYetMessage.toJson(), message.sentFrom);
                break;
            case LobbyMessageType.LEADER_PLAYERS:
                if (!AcceptMessages)
                    break;

                if (isLeader)
                    break;

                // Learn my id and leader's id from this message
                myID = lobbyMessage.SendTo;
                leaderID = message.sentFrom;

                // Add leader to the list of players.
                AddPlayer(message.sentFrom, lobbyMessage.Message);

                bool iAmIn = false;
                // Add all players to the list of players.
                foreach (KeyValuePair<string, string> player in lobbyMessage.Players) {
                    // Check if the player is me.
                    if (player.Key.Equals(myID)) {
                        iAmIn = true;
                        continue;
                    }
                        
                    AddPlayer(player.Key, player.Value);
                }

                if (!iAmIn) {
                    // Tell the leader to add me to the list of players.
                    LobbyMessage joinLobbyMessage = new(LobbyMessageType.MEMBER_AM_I_IN, false, myName, leaderID);
                    network.send(joinLobbyMessage.toJson(), leaderID);
                }

                // Remove all players that are not in the list of players.
                foreach (KeyValuePair<string, string> player in players) {
                    if (!lobbyMessage.Players.ContainsKey(player.Key) 
                        && !player.Key.Equals(myID) && !player.Key.Equals(leaderID))
                        RemovePlayer(player.Key);
                }
                break;
            case LobbyMessageType.LEADER_START:
                if (!AcceptMessages)
                    break;

                if (isLeader)
                    break;

                GameState gameState = GameState.Instance;
                gameState.InitializeFromMessage(GameStateMessage.fromJson(lobbyMessage.Message), roomCode, myID);
                StartGame();
                break;
            case LobbyMessageType.LEADER_LEAVE:
                if (isLeader)
                    break;

                if (!AcceptMessages)
                    break;

                ExitLobby();
                break;
        }
        return CallbackStatus.PROCESSED;
    }
}

// Handlemessages with bool to tell it to stop
// do not kill thread to handle messages (track them tho) (will have 3 or 4 at end of game to kill)

public enum LobbyMessageType {
    MEMBER_JOIN,
    MEMBER_AM_I_IN,
    MEMBER_STARTED_YET,
    LEADER_START,
    LEADER_PLAYERS,
    LEADER_LEAVE
}

public class LobbyMessage : MessageInfo {
    public MessageType messageType {get; set;}
    public LobbyMessageType Type {get; set;}
    public string Message {get;}
    public Dictionary<string, string> Players {get;}
    public bool IsLeader {get;}
    public string SendTo {get;}

    public LobbyMessage(LobbyMessageType type, bool isLeader, string message, string sendTo = "") {
        this.messageType = MessageType.LOBBYMESSAGE;
        Type = type;
        IsLeader = isLeader;
        Message = message;
        SendTo = sendTo;
    }

    public LobbyMessage(bool isLeader, Dictionary<string, string> players, string myName, string sendTo = "") {
        if (!isLeader)
            throw new Exception("Only leader can send this message.");
        this.messageType = MessageType.LOBBYMESSAGE;
        Type = LobbyMessageType.LEADER_PLAYERS;
        IsLeader = isLeader;
        Players = players;
        Message = myName;
        SendTo = sendTo;
    }

    [JsonConstructor]
    public LobbyMessage(LobbyMessageType type, bool isLeader, string message, Dictionary<string, string> players, string sendTo) {
        this.messageType = MessageType.LOBBYMESSAGE;
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


// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.SceneManagement;
// using Newtonsoft.Json;

// public class LobbyManager : MonoBehaviour {
//     private LobbyUIManager lobbyUIManager;
//     private Dictionary<string, string> players = new();
//     private string myID;
//     private string myName;
//     private int maxPlayerCount = 6;
//     private string roomCode = "";
//     private bool isLeader = false;
//     private NetworkUtils network;
//     private List<string> connectedDevices = new List<string>();
//     private bool AcceptMessages = false;
//     public static LobbyManager selfReference;
//     private string leaderID = "";
//     private int msgFreq = 0;
//     private int msgFreqCounter = 0;
    
//     // Initialisation
//     void Start() {
//         lobbyUIManager = GetComponent<LobbyUIManager>();
//         myID = SystemInfo.deviceUniqueIdentifier;
//         myName = PlayerPrefs.GetString("name");
//         msgFreq = maxPlayerCount + 1;
//     }

// 	void Awake () {
// 		if(!selfReference) {
// 			selfReference = this;
//             network = NetworkManager.Instance.NetworkUtils;
//             InvokeRepeating("HandleMessages", 0.0f, 1.0f);
// 			DontDestroyOnLoad(gameObject);
// 		}else 
//             Destroy(gameObject);
// 	}

//     void SendMessages() {
//         Debug.Log("Attempting to send lobby messages.");
//         if (network == null)
//             return;

//         if (!AcceptMessages) {
//             Debug.Log("Not accepting messages.");
//             return;
//         }

//         Debug.Log("Sending Lobby Messages.");
//         // Leader check if any devices have disconnected.
//         if (isLeader) {
//             bool changed = false;
//             List<string> newConnectedDevices = network.getConnectedDevices();
//             Debug.Log("Connected Devices: " + network.getConnectedDevices().Count);
//             foreach (string deviceID in connectedDevices) {
//                 if (!newConnectedDevices.Contains(deviceID)) {
//                     RemovePlayer(deviceID);
//                     changed = true;
//                 }
//             }
//             connectedDevices = newConnectedDevices;
//             if (changed) {
//                 LobbyMessage disconnectedPlayersMessage = new(isLeader, players, myName);
//                 network.broadcast(disconnectedPlayersMessage.toJson());
//             }
//         } else {
//             // If i have not found the leader yet, broadcast my presence.
//             Debug.Log("Connected Devices: " + network.getConnectedDevices().Count);
//             if (leaderID.Equals("")) {
//                 foreach (string id in network.getConnectedDevices()) {
//                     Debug.Log("IDK Sending AMIIN message to " + id);
//                     LobbyMessage amIInMessage = new(LobbyMessageType.MEMBER_AM_I_IN, false, myName, id);
//                     network.send(amIInMessage.toJson(), id);
//                 }
//             } else {
//                 // Check if I'm in the lobby every maxFreq seconds.
//                 Debug.Log("Sending AMIIN message to " + leaderID);
//                 LobbyMessage amIInMessage = new(LobbyMessageType.MEMBER_AM_I_IN, false, myName, leaderID);
//                 network.send(amIInMessage.toJson(), leaderID);

//                 // Check if the game has started every 5 seconds after 60 
//                 LobbyMessage startedYetMessage = new(LobbyMessageType.MEMBER_STARTED_YET, false, myName, leaderID);
//                 network.send(startedYetMessage.toJson(), leaderID);
//             }
//         }
//     }

//     public void SetUpLobby(string roomCode, bool isLeader) {
//         this.roomCode = roomCode;

//         // Start discovering and advertising.
//         network.setRoomCode(roomCode);
//         network.startDiscovering();

//         // Add buffer time for advertising to work.
//         System.Threading.Thread.Sleep(1000);

//         network.startAdvertising();

//         this.isLeader = isLeader;
//         if (!isLeader) {
//             // Wait for room to be found.
//             if (!FindRoom())
//                 throw new Exception("Room not found.");
//         }

//         AcceptMessages = true;

//         // Tell the leader to add you to the list of players
//         if (!isLeader) {
//             foreach (string id in network.getConnectedDevices()) {
//                 Debug.Log("Sending JOIN message to " + id);
//                 LobbyMessage joinLobbyMessage = new(LobbyMessageType.MEMBER_JOIN, false, myName, id); 
//                 network.send(joinLobbyMessage.toJson(), id);
//             }
//         }
//         List<string> allPlayers = new(players.Values);
//         allPlayers.Add(PlayerPrefs.GetString("name"));
//         lobbyUIManager.SetPlayers(allPlayers);
//         lobbyUIManager.SetUpLobby(roomCode, isLeader);
//     }

//     // Check if the room code is already being used.
//     // Loops for 2 seconds searching for room.
//     private bool FindRoom() {
//         int count = 0;
//         while (count < 1000) {
//             List<string> connections = network.getConnectedDevices();
//             Debug.Log("Connections: " + connections.Count);
//             if (connections.Count != 0)
//                 return true;
//             System.Threading.Thread.Sleep(20);
//             count++;
//         }
//         return false;
//     }

//     public void AddPlayer(string id, string name) {
//         if (players.ContainsKey(id))
//             return;
//         players.Add(id, name);
//         lobbyUIManager.AddPlayer(name);
//     }

//     public void RemovePlayer(string id) {
//         if (!players.ContainsKey(id)) {
//             Debug.Log("Player not found in list of players.");
//             return;
//         }
//         lobbyUIManager.RemovePlayer(players[id]);
//         players.Remove(id);
//     }

//     public void ExitLobby() {
//         AcceptMessages = false;
//         // Tell the leader to remove you from the list of players.
//         if (isLeader) {
//             // If you are the leader, tell everyone to exit the lobby.
//             LobbyMessage leaveLobbyMessage = new(LobbyMessageType.LEADER_LEAVE, true, "");
//             network.broadcast(leaveLobbyMessage.toJson());
//         }
//         network.stopAdvertising();
//         network.stopDiscovering();
//         // Reset the lobby.
//         players = new Dictionary<string, string>();
//         lobbyUIManager.ExitLobby();
//     }

//     public void LeaderStartGame() {
//         if (!isLeader)
//             return;

//         // Add myself as player
//         players.Add(myID, myName);

//         // Initialise the game state.
//         GameState gameState = GameState.Instance;
//         gameState.Initialize(myID, roomCode, players);

//         // Share the Gamestate and to start the game.
//         LobbyMessage startGameMessage = new(LobbyMessageType.LEADER_START, true, gameState.ToMessage().toJson());
//         network.broadcast(startGameMessage.toJson());

//         // Reset the lobby and disable scene.
//         AcceptMessages = false;
//         StartGame();
//     }

//     // Common method to start the game for both leader and non-leader.
//     public void StartGame() {
//         AcceptMessages = false;
//         // Load the game scene.
//         SceneManager.LoadScene("Gameplay");
//     }

//     private void HandleMessages() {
//         Func<Message, CallbackStatus> callback = (Message msg) => {
//             return HandleMessage(msg);
//         };
//         network.onReceive(callback);

//         // Every msgFreq seconds, send messages.
//         if (msgFreqCounter >= msgFreq || leaderID.Equals("")) {
//             SendMessages();
//             msgFreqCounter = 0;
//         } else {
//             msgFreqCounter++;
//         }
//     }

//     private CallbackStatus HandleMessage(Message message) {
//         // Ignore if not a lobby message.
//         if (message.messageInfo.messageType != MessageType.LOBBYMESSAGE)
//             return CallbackStatus.NOT_PROCESSED;
        
        
//         // Ignore if no longer accepting messages.
//         if (!AcceptMessages)
//             return CallbackStatus.DORMANT;


//         // Ignore if no longer accepting messages.
//         if (!AcceptMessages)
//             return CallbackStatus.DORMANT;

//         LobbyMessage lobbyMessage = (LobbyMessage) message.messageInfo;
//         switch (lobbyMessage.Type) {
//             case LobbyMessageType.MEMBER_AM_I_IN:
//             case LobbyMessageType.MEMBER_JOIN:
//                 // Ignore if not accepting
//                 if (!AcceptMessages)
//                     break;

//                 // Ignore if not leader.
//                 if (!isLeader)
//                     break;

//                 // Ignore if player already in list.
//                 if (players.ContainsKey(message.sentFrom))
//                     break;

//                 // Ignore if max player count reached.
//                 if (players.Count >= maxPlayerCount)
//                     break;

//                 Debug.Log("Player joined: " + message.sentFrom + "  "+lobbyMessage.Message);

//                 // Add player to the list of players.
//                 AddPlayer(message.sentFrom, lobbyMessage.Message);
                
//                 // Send to each player, including their id.
//                 foreach (string id in players.Keys) {
//                     LobbyMessage newPlayerMessage = new(isLeader, players, myName, id);
//                     network.send(newPlayerMessage.toJson(), id);
//                 }

//                 // Learn my id from this message
//                 myID = lobbyMessage.SendTo;
//                 break;
//             case LobbyMessageType.MEMBER_STARTED_YET:
//                 if (!isLeader)
//                     break;

//                 // Ignore if player not in list.
//                 if (!players.ContainsKey(message.sentFrom))
//                     break;

//                 // Tell the player if the game has started.
//                 if (GameState.Instance.Initialized) {
//                     LobbyMessage startedYetMessage = new(LobbyMessageType.LEADER_START, true, GameState.Instance.ToMessage().toJson(), message.sentFrom);
//                     network.send(startedYetMessage.toJson(), message.sentFrom);
//                 }
//                 break;
//             case LobbyMessageType.LEADER_PLAYERS:
//                 if (!AcceptMessages)
//                     break;

//                 if (isLeader)
//                     break;

//                 // Learn my id and leader's id from this message
//                 myID = lobbyMessage.SendTo;
//                 leaderID = message.sentFrom;

//                 // Add leader to the list of players.
//                 AddPlayer(message.sentFrom, lobbyMessage.Message);

//                 bool iAmIn = false;
//                 // Add all players to the list of players.
//                 foreach (KeyValuePair<string, string> player in lobbyMessage.Players) {
//                     // Check if the player is me.
//                     if (player.Key.Equals(myID)) {
//                         iAmIn = true;
//                         continue;
//                     }
                        
//                     AddPlayer(player.Key, player.Value);
//                 }

//                 if (!iAmIn) {
//                     // Tell the leader to add me to the list of players.
//                     LobbyMessage joinLobbyMessage = new(LobbyMessageType.MEMBER_AM_I_IN, false, myName, leaderID);
//                     network.send(joinLobbyMessage.toJson(), leaderID);
//                 }

//                 // Remove all players that are not in the list of players.
//                 foreach (KeyValuePair<string, string> player in players) {
//                     if (!lobbyMessage.Players.ContainsKey(player.Key) 
//                         && !player.Key.Equals(myID) && !player.Key.Equals(leaderID))
//                         RemovePlayer(player.Key);
//                 }
//                 break;
//             case LobbyMessageType.LEADER_START:
//                 GameState gameState = GameState.Instance;
//                 gameState.InitializeFromMessage(GameStateMessage.fromJson(lobbyMessage.Message), roomCode, myID);
//                 StartGame();
//                 break;
//             case LobbyMessageType.LEADER_LEAVE:
//                 if (isLeader)
//                     break;

//                 if (!AcceptMessages)
//                     break;

//                 ExitLobby();
//                 break;
//         }
//         return CallbackStatus.PROCESSED;
//     }
// }

// // Handlemessages with bool to tell it to stop
// // do not kill thread to handle messages (track them tho) (will have 3 or 4 at end of game to kill)

// public enum LobbyMessageType {
//     MEMBER_JOIN,
//     MEMBER_AM_I_IN,
//     MEMBER_STARTED_YET,
//     LEADER_START,
//     LEADER_PLAYERS,
//     LEADER_LEAVE
// }

// public class LobbyMessage : MessageInfo {
//     public MessageType messageType {get; set;}
//     public LobbyMessageType Type {get; set;}
//     public string Message {get;}

//     public Dictionary<string, string> Players {get;}
//     public bool IsLeader {get;}
//     public string SendTo {get;}

//     public LobbyMessage(LobbyMessageType type, bool isLeader, string message, string sendTo = "") {
//         this.messageType = MessageType.LOBBYMESSAGE;
//         Type = type;
//         IsLeader = isLeader;
//         Message = message;
//         SendTo = sendTo;
//     }

//     public LobbyMessage(bool isLeader, Dictionary<string, string> players, string myName, string sendTo = "") {
//         if (!isLeader)
//             throw new Exception("Only leader can send this message.");
//         this.messageType = MessageType.LOBBYMESSAGE;
//         Type = LobbyMessageType.LEADER_PLAYERS;
//         IsLeader = isLeader;
//         Players = players;
//         Message = myName;
//         SendTo = sendTo;
//     }

//     [JsonConstructor]
//     public LobbyMessage(LobbyMessageType type, bool isLeader, string message, Dictionary<string, string> players, string sendTo) {
//         this.messageType = MessageType.LOBBYMESSAGE;
//         Type = type;
//         IsLeader = isLeader;
//         Message = message;
//         Players = players;
//         SendTo = sendTo;
//     }

//     public string toJson() {
//         return JsonConvert.SerializeObject(this);
//     }

//     public string processMessageInfo() {
//         return "";
//     }
// }
