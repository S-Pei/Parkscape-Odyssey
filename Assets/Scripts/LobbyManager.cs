using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour {
    private LobbyUIManager lobbyUIManager;
    private Dictionary<string, string> players = new();
    private int maxPlayerCount = 6;
    private string roomCode = "";
    private bool isLeader = false;
    private NetworkUtils network;
    private List<string> connectedDevices = new List<string>();
    private bool AcceptMessages = false;
    private string myID = "";
    
    // Initialisation
    void Start() {
        lobbyUIManager = GetComponent<LobbyUIManager>();
    }

    void Update() {
        if (network == null)
            return;

        if (!AcceptMessages)
            return;

        // Leader check if any devices have disconnected.
        if (isLeader) {
            List<string> newConnectedDevices = network.getConnectedDevices();
            foreach (string deviceID in connectedDevices) {
                if (!newConnectedDevices.Contains(deviceID)) {
                    RemovePlayer(deviceID);
                }
            }
            connectedDevices = newConnectedDevices;
            LobbyMessage disconnectedPlayersMessage = new LobbyMessage(isLeader, myID, players);
            network.broadcast(disconnectedPlayersMessage.toJson());
        }

        string message = network.processNewMessage();
        if (message.Equals(""))
            return;
        
        LobbyMessage lobbyMessage = LobbyMessage.fromJson(message);
        switch (lobbyMessage.Type) {
            case LobbyMessageType.JOIN:
                // Ignore if not leader.
                if (!isLeader)
                    break;
                AddPlayer(lobbyMessage.Id, lobbyMessage.Message);
                LobbyMessage newPlayerMessage = new LobbyMessage(isLeader, myID, players);
                network.broadcast(newPlayerMessage.toJson());
                break;
            case LobbyMessageType.LEADER:
                if (isLeader)
                    break;
                // Add all players to the list of players.
                foreach (KeyValuePair<string, string> player in lobbyMessage.Players) {
                    AddPlayer(player.Key, player.Value);
                }

                // Remove all players that are not in the list of players.
                foreach (KeyValuePair<string, string> player in players) {
                    if (!lobbyMessage.Players.ContainsKey(player.Key))
                        RemovePlayer(player.Key);
                }
                break;
            case LobbyMessageType.START:
                StartGame();
                break;
        }
        
    }

    public void SetUpLobby(string roomCode) {
        string myName = PlayerPrefs.GetString("name");
        this.roomCode = roomCode;
        this.myID = "1234";

        // Set up Networking
        #if UNITY_ANDROID
            network = new AndroidNetwork();
        #elif UNITY_IOS
            network = new iOSNetwork();
            network.initP2P();
        #endif

        // Start discovering. If no connections, then elect as leader.
        network.setRoomCode(roomCode);
        network.startDiscovering();

        this.isLeader = !FoundRoom(roomCode);

        // Start advertising.
        network.startAdvertising();

        if (isLeader) {
            // Add yourself to the list of players.
            AddPlayer(myID, myName + " (Leader)");

        } else {
            // Tell the leader to add you to the list of players
            LobbyMessage joinLobbyMessage = new LobbyMessage(LobbyMessageType.JOIN, false, SystemInfo.deviceUniqueIdentifier, myName); 
            network.broadcast(joinLobbyMessage.toJson());
        }
        AcceptMessages = true;
        lobbyUIManager.SetPlayers(new List<string>(players.Values));
        lobbyUIManager.SetUpLobby(roomCode, isLeader);
    }

    // Check if the room code is already being used.
    // Loops for 2 seconds searching for room.
    private bool FoundRoom(string roomCode) {
        float startTime = Time.time;
        while (Time.time - startTime < 2) {
            List<string> connections = network.getConnectedDevices();
            if (connections.Count != 0)
                return true;
        }
        return false;
    }

    public void AddPlayer(string id, string name) {
        if (players.ContainsKey(id))
            return;
        players.Add(id, name);
        lobbyUIManager.AddPlayer(name);
    }

    public void RemovePlayer(string id) {
        players.Remove(id);
        lobbyUIManager.RemovePlayer(players[id]);
    }

    public void ExitLobby() {
        // Tell the leader to remove you from the list of players.
        // If you are the leader, tell everyone to exit the lobby.
        // Reset the lobby.
        players = new Dictionary<string, string>();
        lobbyUIManager.ExitLobby();
    }

    public void LeaderStartGame() {
        if (!isLeader)
            return;

        // Initialise the game state.
        GameState gameState = GameState.Instance;
        gameState.Initialize(SystemInfo.deviceUniqueIdentifier, roomCode, players);

        // Share the Gamestate and to start the game.

        // Reset the lobby and disable scene.
        ExitLobby();
        StartGame();
    }

    // Common method to start the game for both leader and non-leader.
    public void StartGame() {
        AcceptMessages = false;
        // Load the game scene.
        SceneManager.LoadScene("Gameplay");
    }
}
