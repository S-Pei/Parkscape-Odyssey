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
        ((Image) playerIcon.GetComponent(typeof(Image))).sprite = GetIcon(GameState.MyPlayer.Role);
    }

    // Opens the player view with the player's role informationa and stats
    public void OpenPlayerView() {
        GameObject playerView = Instantiate(playerViewPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        playerView.SetActive(false);
        PlayerViewManager playerViewManager = playerView.GetComponent<PlayerViewManager>();

        // Role information
        playerViewManager.SetPlayer(GameState.MyPlayer);
        playerViewManager.SetPlayerIcon(GetIcon(GameState.MyPlayer.Role));

        playerView.SetActive(true);
    }

    private Sprite GetIcon(string role) {
        foreach (Sprite icon in playerIcons) {
            if (icon.name.Contains(role)) {
                return icon;
            }
        }
        throw new Exception("Role Icon not found");
    }
}
