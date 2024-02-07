using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameInterfaceManager : MonoBehaviour
{
    private readonly GameState GameState = GameState.Instance;
    private GameObject inventoryObject;

    [SerializeField]
    private GameObject inventoryPrefab;
    
    [SerializeField]
    private GameObject playerViewPrefab;

    [SerializeField]
    private GameObject playerIcon;

    [SerializeField]
    private List<Sprite> playerIcons;

    private Dictionary<string, int> roleToIcon = new Dictionary<string, int>()
        {
            { "Rogue", 0 },
            { "Mage", 1 },
            { "Faerie", 2 },
            { "Cleric", 3 },
            { "Scout", 4 },
            { "Warrior", 5 }
        };

    void Awake() {
        Player myPlayer = GameState.Instance.MyPlayer;
        myPlayer.Icon = GetIcon(myPlayer.Role);
        List<Player> otherPlayers = GameState.Instance.OtherPlayers;
        foreach (Player p in otherPlayers) {
            p.Icon = GetIcon(p.Role);
        }
    }

    // Open with actual inventory stored in GameManager
    public void OpenInventory(List<CardName> cards) {
        inventoryObject = Instantiate(inventoryPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        InventoryController inventoryController = inventoryObject.GetComponent<InventoryController>();
        inventoryController.inventoryCards = cards;
    }

    public void CloseInventory() {
        InventoryUIManager inventoryUIManager = inventoryObject.GetComponent<InventoryUIManager>();
        inventoryUIManager.DestroySelf();
    }

    public void SetUpInterface() {
        // Set up player icon.
        ((Image) playerIcon.GetComponent(typeof(Image))).sprite = GameState.MyPlayer.Icon;
    }

    // Opens the player view with the player's role informationa and stats
    public void OpenPlayerView() {
        GameObject playerView = Instantiate(playerViewPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        playerView.SetActive(false);
        PlayerViewManager playerViewManager = playerView.GetComponent<PlayerViewManager>();

        // Role information
        playerViewManager.SetPlayer(GameState.MyPlayer);
        playerViewManager.SetPlayerIcon(GameState.MyPlayer.Icon);

        playerView.SetActive(true);
    }

    private Sprite GetIcon(string role) {
        if (!roleToIcon.ContainsKey(role)) {
            throw new Exception("Role Icon not found");
        }
        return playerIcons[roleToIcon[role]];
    }
}
