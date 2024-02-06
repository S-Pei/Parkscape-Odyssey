using System;
using System.Collections.Generic;


public class GameState {
    private static readonly Lazy<GameState> LazyGameState = new(() => new GameState());

    public static GameState Instance { get { return LazyGameState.Value; } }

    public int maxPlayerCount = 6;

    // Fields
    public bool Initialized = false;
    public string RoomCode = ""; 
    public Player MyPlayer = null;
    public List<Player> OtherPlayers = new();
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

    // Method will be called only during Game initialization.
    public void Initialize(string myId, string roomCode, Dictionary<string, string> players) {
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
            if (id == myId) {
                MyPlayer = player;
            } else {
                OtherPlayers.Add(player);
            }
        }

        // Set initial cards.
        MyCards = InitialCards;

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

}
