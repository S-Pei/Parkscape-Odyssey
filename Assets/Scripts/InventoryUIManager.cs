using System.Collections.Generic;
using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject cardDisplayPrefab;

    [SerializeField]
    private GameObject cardsInventoryContent;


    CardsUIManager cardsUIManager;
    InventoryController inventoryController;

    // Start is called before the first frame update
    void Start()
    {
        cardsUIManager = GameObject.FindGameObjectWithTag("CardsManager").GetComponent<CardsUIManager>();
        inventoryController = GetComponent<InventoryController>();
        // cardsInventoryContent = transform.FindChild("Card Inventory/canvas/Panel/Scroll View/Viewport/Content");
        displayAllCards();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void displayAllCards() {
        List<string> cardsNames = inventoryController.inventoryCards;
        foreach (string cardName in cardsNames) {
            displayCard(cardName);
        }
    }

    private void displayCard(string cardName) {
        (Sprite img, string stats)? cardDetails = cardsUIManager.findCardDetails(cardName);
        if (cardDetails.HasValue) {
            GameObject newCard = Instantiate(cardDisplayPrefab);
            CardRenderer cardRenderer = newCard.GetComponentInChildren<CardRenderer>();
            cardRenderer.renderCard(cardDetails.Value.img, cardDetails.Value.stats);
            newCard.transform.parent = cardsInventoryContent.transform;
            newCard.transform.localScale = new Vector3(1, 1, 1);
        } else {
            Debug.LogWarning($"InventoryUIManager: Card not found in CardsManager - {cardName}");
        }
    }
}
