using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : MonoBehaviour {
    private LobbyUIManager lobbyUIManager;
    private Dictionary<string, string> players = new Dictionary<string, string>();
    private int maxPlayerCount = 6;
    
    // Initialisation
    void Start() {
        lobbyUIManager = (LobbyUIManager) GetComponent(typeof(LobbyUIManager));
    }

    // Listen for any changes in the lobby.
    void Update() {
        
    }

    public void SetUpLobby(string roomCode, bool isLeader) {
        // string myName = PlayerPrefs.GetString("name");
        string myName = "Player";
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
}
