using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInterfaceManager : MonoBehaviour
{
    public GameObject inventoryPrefab;
    private GameObject inventoryObject;

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
}
