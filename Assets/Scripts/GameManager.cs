using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private GameState GameState = GameState.Instance;
    private GameInterfaceManager gameInterfaceManager;

    [SerializeField]
    private string role;

    private List<string> playerCards = new List<string> { "baseAtk", "baseAtk", "baseDef", "sprint", "baseDef", "sodaAid", "bigHeal", "baseAtk", "warCry", "baseDef", "enrage", "sprint", "bloodOffering", "femaleWarrior", "drawCards"};

    // Start is called before the first frame update
    void Start() {
        gameInterfaceManager = (GameInterfaceManager) GetComponent(typeof(GameInterfaceManager));

        // Initialise game state (temporary, move to lobby stage)
        GameState.Initialize("RoomCode", PlayerFactory.CreateRogue("Player 1"), new List<Player>(), playerCards);

        // Set up interface with GameState information
        gameInterfaceManager.SetUpInterface();
    }

    // Update is called once per frame
    void Update() {
        
    }

    public void OpenInventory() {
        gameInterfaceManager.OpenInventory(playerCards);
    }

    public void CloseInventory() {
        gameInterfaceManager.CloseInventory();
    }

    public void OpenPlayerView() {
        gameInterfaceManager.OpenPlayerView();
    }

    // Card Deck
    public void addCardToDeck(Card card) {
        playerCards.Add(card.name);
    }
}
