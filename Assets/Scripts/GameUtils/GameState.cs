using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.Maps.Unity;
using Microsoft.Geospatial;

public class GameState {
    private static readonly bool DEBUGMODE = 
    #if UNITY_EDITOR 
        true;
    #else
        false;
    #endif

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
    private Dictionary<int, CardName> MyCards = new();
    public bool isLeader = false;
    public bool IsInEncounter = false;
    public int Score = 0;

    private readonly List<CardName> InitialCards = new() { 
        CardName.BASE_ATK, CardName.BASE_ATK, CardName.BASE_ATK, 
        CardName.BASE_DEF, CardName.BASE_DEF, CardName.BASE_DEF
    };

    private int cardID = 0;

    // ENCOUNTER
    public List<Monster> encounterMonsters;
    public List<List<SkillName>> skillSequences;

    public Dictionary<string, string> partyMembers;

    // MAP
    // Medium Encounter Locations broadcasted by the leader/ web authoring tool in the beginning of the game
    public Dictionary<string, LatLon> mediumEncounterLocations = new();
    // Medium Encounter IDs found by the player, to be shared with other players
    public HashSet<string> foundMediumEncounters = new();

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

        this.myID = myID;
        Initialized = true;
        InitialiseCards();
    }

    // Method to specify the initial state of the game.
    public void Initialize(string roomCode, Player myPlayer, List<Player> otherPlayers, List<CardName> initialCards) {
        CheckNotInitialised();
        RoomCode = roomCode;
        MyPlayer = myPlayer;
        OtherPlayers = otherPlayers;

        Initialized = true;
        InitialiseCards();
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

    public Player GetPlayerByID(string id) {
        CheckInitialised();
        if (MyPlayer.Id == id) {
            return MyPlayer;
        }
        foreach (Player player in OtherPlayers) {
            if (player.Id == id) {
                return player;
            }
        }
        return null;
    }

    public void AddCard(CardName card) {
        CheckInitialised();
        cardID++;
        MyCards.Add(cardID, card);
    }

    public void RemoveCard(int cardID) {
        CheckInitialised();
        MyCards.Remove(cardID);
    }

    public List<CardName> GetCards() {
        CheckInitialised();
        return new List<CardName>(MyCards.Values);
    }

    public bool HasCard(int cardID) {
        CheckInitialised();
        return MyCards.ContainsKey(cardID);
    }

    public CardName GetCard(int cardID) {
        CheckInitialised();
        return MyCards[cardID];
    }

    public List<int> GetCardIDs() {
        CheckInitialised();
        return new List<int>(MyCards.Keys);
    }

    public void InitialiseCards() {
        MyCards = new();
        cardID = 0;
        foreach (CardName card in InitialCards) {
            AddCard(card);
        }
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
        
        Initialized = true;
        InitialiseCards();
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
        IsInEncounter = false;
        Score = 0;
        InitialiseCards();
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