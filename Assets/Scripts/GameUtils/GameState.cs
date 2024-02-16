using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using UnityEngine.PlayerLoop;

public class GameState {
    private static bool DEBUGMODE = false;
    private static readonly Lazy<GameState> LazyGameState = new(() => new GameState());

    public static GameState Instance { get {
        GameState gameState = LazyGameState.Value;
        if (DEBUGMODE && !gameState.Initialized) {
            gameState.Initialize("1", "123", new Dictionary<string, string> {
                {"1", "Player 1"},
                {"2", "Player 2"},
                {"3", "Player 3"},
                {"4", "Player 4"},
            });
        }
        return gameState; } }

    public int maxPlayerCount = 6;

    // Fields
    public string myID;
    public bool Initialized = false;
    public string RoomCode = ""; 
    public Player MyPlayer = null;
    public List<Player> OtherPlayers = new();

    public Dictionary<string, Player> PlayersDetails = new();
    public List<CardName> MyCards = new() {
        CardName.BASE_ATK, CardName.BASE_ATK, CardName.BASE_ATK, 
        CardName.BASE_DEF, CardName.BASE_DEF, CardName.BASE_DEF
    };
    public bool IsInEncounter = false;
    public int Score = 0;

    private List<CardName> InitialCards = new List<CardName> { 
        CardName.BASE_ATK, CardName.BASE_ATK, CardName.BASE_ATK, 
        CardName.BASE_DEF, CardName.BASE_DEF, CardName.BASE_DEF
    };

    // ENCOUNTER
    public List<Monster> encounterMonsters;
    public List<List<SkillName>> skillSequences;

    public Dictionary<string, string> partyMembers;

    // Method will be called only during Game initialization.
    public void Initialize(string myID, string roomCode, Dictionary<string, string> players) {
        CheckNotInitialised();

        RoomCode = roomCode;

        // Random roles for each player.
        List<string> roles = PlayerFactory.GetRoles();
        Random random = new Random();
        foreach (string id in players.Keys) {
            string name = players[id];
            string role = roles[random.Next(roles.Count)];
            roles.Remove(role);
            Player player = PlayerFactory.CreatePlayer(id, name, role);
            PlayersDetails.Add(id, player);
            if (id == myID) {
                MyPlayer = player;
            } else {
                OtherPlayers.Add(player);
            }
        }

        // Set initial cards.
        MyCards = InitialCards;
        this.myID = myID;
        Initialized = true;
    }

    // Method to specify the initial state of the game.
    public void Initialize(string roomCode, Player myPlayer, List<Player> otherPlayers, List<CardName> initialCards) {
        CheckNotInitialised();
        RoomCode = roomCode;
        MyPlayer = myPlayer;
        OtherPlayers = otherPlayers;
        MyCards = initialCards;
        Initialized = true;
    }

    // This method returns a reference to a player with the given name.
    // If you want to update the player's stats, you should do so through the reference.
    public Player GetPlayer(string name) {
        CheckInitialised();
        if (MyPlayer.Name == name) {
            return MyPlayer;
        }
        foreach (Player player in OtherPlayers) {
            if (player.Name == name) {
                return player;
            }
        }
        return null;
    }

    public void AddCard(CardName card) {
        CheckInitialised();
        MyCards.Add(card);
    }

    public void RemoveCard(CardName card) {
        CheckInitialised();
        MyCards.Remove(card);
    }

    public GameStateMessage ToMessage() {
        CheckInitialised();
        Dictionary<string, string> playerRoles = new();
        playerRoles.Add(myID, MyPlayer.Role);
        foreach (Player player in OtherPlayers) {
            playerRoles.Add(player.Id, player.Role);
        }

        Dictionary<string, string> playerNames = new();
        playerNames.Add(myID, MyPlayer.Name);
        foreach (Player player in OtherPlayers) {
            playerNames.Add(player.Id, player.Name);
        }
        return new GameStateMessage(playerRoles, playerNames);
    }

    public void InitializeFromMessage(GameStateMessage message, string roomCode, string myID) {
        CheckNotInitialised();
        List<Player> players = new();
        foreach (string id in message.playerRoles.Keys) {
            string name = message.playerNames[id];
            string role = message.playerRoles[id];
            Player player = PlayerFactory.CreatePlayer(id, name, role);
            PlayersDetails.Add(id, player);
            if (id == myID) {
                MyPlayer = player;
            } else {
                players.Add(player);
            }
        }
        OtherPlayers = players;

        // Initialise other fields
        RoomCode = roomCode;
        this.myID = myID;
        MyCards = InitialCards;

        Initialized = true;
    }

    public void UpdateFromMessage(GameStateMessage message) {
        CheckInitialised();
        throw new NotImplementedException();
    }

    public void Reset() {
        RoomCode = "";
        MyPlayer = null;
        OtherPlayers = new();
        PlayersDetails = new();
        MyCards = InitialCards;
        IsInEncounter = false;
        Score = 0;
        Initialized = false;
    }

    private void CheckInitialised() {
        if (!Initialized) {
            throw new InvalidOperationException("GameState not initialized.");
        }
    }

    private void CheckNotInitialised() {
        if (Initialized) {
            throw new InvalidOperationException("GameState already initialized.");
        }
    }


    // ------------------------------- ENCOUNTER -------------------------------
    public void StartEncounter(List<Monster> monsters, List<List<SkillName>> skillSequences, Dictionary<string, string> partyMembers) {
        CheckInitialised();
        if (IsInEncounter) {
            return;
        }
        encounterMonsters = monsters;
        this.skillSequences = skillSequences;
        this.partyMembers = partyMembers;
        IsInEncounter = true;
    }

    public void ExitEncounter() {
        IsInEncounter = false;
    }
}

public class GameStateMessage : MessageInfo {
    public Dictionary<string, string> playerRoles;
    public Dictionary<string, string> playerNames;
    public MessageType messageType { get; set; }

    public GameStateMessage(Dictionary<string, string> playerRoles, Dictionary<string, string> playerNames) {
        this.playerRoles = playerRoles;
        this.playerNames = playerNames;
        this.messageType = MessageType.GAMESTATE;
    }

    public string processMessageInfo() {
        throw new NotImplementedException();
    }

    public static GameStateMessage fromJson(string json) {
        return JsonConvert.DeserializeObject<GameStateMessage>(json);
    }

    public string toJson() {
        return JsonConvert.SerializeObject(this);
    }
}