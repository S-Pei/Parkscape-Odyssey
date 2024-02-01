using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameInterfaceManager : MonoBehaviour
{
    private GameState GameState = GameState.Instance;
    private GameObject inventoryObject;

    [SerializeField]
    private GameObject inventoryPrefab;
    
    [SerializeField]
    private GameObject playerViewPrefab;

    [SerializeField]
    private GameObject playerIcon;

    [SerializeField]
    private List<Sprite> playerIcons;
    private Dictionary<string, int> roles = new Dictionary<string, int> { 
        { "Rogue",  0 },
        { "Mage",  1 },
        { "Faerie",  2 },
        { "Cleric",  3 },
        { "Scout",  4 },
        { "Warrior",  5 }, };

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Open with actual inventory stored in GameManager
    public void OpenInventory(List<CardName> cards) {
        inventoryObject = Instantiate(inventoryPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        InventoryController inventoryController = (InventoryController) inventoryObject.GetComponent<InventoryController>();
        inventoryController.inventoryCards = cards;
    }

    public void CloseInventory() {
        InventoryUIManager inventoryUIManager = (InventoryUIManager) inventoryObject.GetComponent<InventoryUIManager>();
        inventoryUIManager.DestroySelf();
    }

    public void SetUpInterface() {
        // Set up player icon.
        ((Image) playerIcon.GetComponent(typeof(Image))).sprite = playerIcons[roles[GameState.MyPlayer.Role]];
    }

    // Opens the player view with the player's role informationa and stats
    public void OpenPlayerView() {
        GameObject playerView = Instantiate(playerViewPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        playerView.SetActive(false);
        PlayerViewManager playerViewManager = (PlayerViewManager) playerView.GetComponent<PlayerViewManager>();

        // Role information
        playerViewManager.SetPlayer(GameState.MyPlayer);
        playerViewManager.SetPlayerIcon(playerIcons[roles[GameState.MyPlayer.Role]]);

        playerView.SetActive(true);
    }
}
