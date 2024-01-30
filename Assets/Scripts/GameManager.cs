using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private GameInterfaceManager gameInterfaceManager;

    [SerializeField]
    private string role;

    private List<string> playerCards = new List<string> { "baseAtk", "baseAtk", "baseDef", "baseDef", "baseAtk", "baseDef" };

    // Start is called before the first frame update
    void Start() {
        gameInterfaceManager = (GameInterfaceManager) GetComponent(typeof(GameInterfaceManager));

        // Set image according to role
        gameInterfaceManager.setRole(role);
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
}
