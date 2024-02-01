using System;
using System.Collections.Generic;

public class GameState {
    private static readonly Lazy<GameState> LazyGameState = new(() => new GameState());

    public static GameState Instance { get { return LazyGameState.Value; } }

    // Fields
    public bool Initialized = false;
    public string RoomCode = ""; 
    public Player MyPlayer = null;
    public List<Player> OtherPlayers = new();
    public List<CardName> Mycards = new();

    // Method will be called only during Game initialization.
    public void Initialize(string roomCode, Player myPlayer, List<Player> otherPlayers, List<CardName> initialCards) {
        if (Initialized) {
            throw new Exception("GameState already initialized.");
        }
        RoomCode = roomCode;
        MyPlayer = myPlayer;
        OtherPlayers = otherPlayers;
        Mycards = initialCards;
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
        Mycards.Add(card);
    }

    public void RemoveCard(CardName card) {
        CheckInitialised();
        Mycards.Remove(card);
    }

    private void CheckInitialised() {
        if (!Initialized) {
            throw new Exception("GameState not initialized.");
        }
    }

}
