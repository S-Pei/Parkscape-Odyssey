using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameInterfaceManager : MonoBehaviour
{
    private GameObject inventoryObject;
    private string role;

    [SerializeField]
    private GameObject inventoryPrefab;
    
    [SerializeField]
    private GameObject playerViewPrefab;

    [SerializeField]
    private GameObject playerIcon;

    [SerializeField]
    private List<Sprite> playerIcons;
    private Dictionary<string, int> roles = new Dictionary<string, int> { 
        { "Warrior",  0 },
        { "Mage",  1 },
        { "Faerie",  2 },
        { "Cleric",  3 },
        { "Scout",  4 },
        { "Guardian",  5 }, };

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Open with actual inventory stored in GameManager
    public void OpenInventory(List<string> cards) {
        inventoryObject = Instantiate(inventoryPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        InventoryController inventoryController = (InventoryController) inventoryObject.GetComponent<InventoryController>();
        inventoryController.inventoryCards = cards;
    }

    public void CloseInventory() {
        InventoryUIManager inventoryUIManager = (InventoryUIManager) inventoryObject.GetComponent<InventoryUIManager>();
        inventoryUIManager.DestroySelf();
    }

    public void setRole(string role) {
        this.role = role;
        ((Image) playerIcon.GetComponent(typeof(Image))).sprite = playerIcons[roles[role]];
    }

    // Opens the player view with the player's role informationa and stats
    public void OpenPlayerView(/* Player player */) {
        GameObject playerView = Instantiate(playerViewPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        playerView.SetActive(false);
        PlayerViewManager playerViewManager = (PlayerViewManager) playerView.GetComponent<PlayerViewManager>();

        // Role information
        playerViewManager.SetRole(role);
        playerViewManager.SetRoleIcon(playerIcons[roles[role]]);

        // Stats
        // playerViewManager.setStats(/* player */);

        playerView.SetActive(true);
    }
}
