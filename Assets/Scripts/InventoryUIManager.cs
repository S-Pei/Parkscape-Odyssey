using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;
using System.Linq;

[assembly:InternalsVisibleTo("EditMode")]


public class InventoryUIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject cardsManagerPrefab;

    [SerializeField]
    private GameObject cardDisplayPrefab;

    [SerializeField]
    private GameObject cardsInventoryContent;


    CardsUIManager cardsUIManager;
    InventoryController inventoryController;

    // Start is called before the first frame update
    void Start()
    {
        GameObject cardsManager;
        if (GameObject.FindGameObjectsWithTag("CardsManager").Length <= 0) {
            cardsManager = Instantiate(cardsManagerPrefab);
        } else {
            cardsManager = GameObject.FindGameObjectWithTag("CardsManager");
        }
        cardsUIManager = cardsManager.GetComponent<CardsUIManager>();
        inventoryController = GetComponent<InventoryController>();
        // cardsInventoryContent = transform.FindChild("Card Inventory/canvas/Panel/Scroll View/Viewport/Content");
        displayAllCards();
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
            // cardRenderer.scaleCardSize(1);
            newCard.transform.parent = cardsInventoryContent.transform;
            cardRenderer.hardAdjustCardDetailsSize();
        } else {
            Debug.LogWarning($"InventoryUIManager: Card not found in CardsManager - {cardName}");
        }
    }
}
