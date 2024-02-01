using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour {
    private LobbyUIManager lobbyUIManager;
    private Dictionary<string, string> players = new Dictionary<string, string>();
    private int maxPlayerCount = 6;
    private string roomCode = "";

    private bool isLeader = false;
    
    // Initialisation
    void Start() {
        lobbyUIManager = (LobbyUIManager) GetComponent(typeof(LobbyUIManager));
    }

    // Listen for any changes in the lobby or start of game.
    void Update() {
        
    }

    public void SetUpLobby(string roomCode, bool isLeader) {
        string myName = PlayerPrefs.GetString("name");
        this.isLeader = isLeader;
        this.roomCode = roomCode;
        if (isLeader) {
            // Add yourself to the list of players.
            AddPlayer(SystemInfo.deviceUniqueIdentifier, myName + " (Leader)");
        } else {
            // Tell the leader to add you to the list of players 
            // and get the list of players.
        }
        lobbyUIManager.SetPlayers(new List<string>(players.Values));
        lobbyUIManager.SetUpLobby(roomCode, isLeader);
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
        // Load the game scene.
        SceneManager.LoadScene("Gameplay");
    }
}
