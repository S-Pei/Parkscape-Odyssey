using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private GameInterfaceManager gameInterfaceManager;

    private Player player = PlayerFactory.CreateWarrior("Player");

    private List<string> playerCards = new List<string> { "baseAtk", "baseAtk", "baseDef", "sprint", "baseDef", "sodaAid", "bigHeal", "baseAtk", "warCry", "baseDef", "enrage", "sprint"};

    // Start is called before the first frame update
    void Start() {
        gameInterfaceManager = (GameInterfaceManager) GetComponent(typeof(GameInterfaceManager));

        // Set image according to role via player class
        gameInterfaceManager.SetPlayer(player);
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
        gameInterfaceManager.OpenPlayerView(player);
    }
}
