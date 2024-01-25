using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;
using System.Linq;
using UnityEngine.UI;

[assembly:InternalsVisibleTo("EditMode")]


public class InventoryUIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject cardsManagerPrefab;

    [SerializeField]
    private GameObject cardDisplayPrefab;

    [SerializeField]
    private GameObject cardsInventoryContent;

    [SerializeField]
    private GameObject popUpPanel;


    CardsUIManager cardsUIManager;
    InventoryController inventoryController;

    public List<GameObject> cardsDisplaying = new();

    // Start is called before the first frame update
    void Start()
    {
        popUpPanel.SetActive(false);

        GameObject cardsManager;
        if (GameObject.FindGameObjectsWithTag("CardsManager").Length <= 0) {
            cardsManager = Instantiate(cardsManagerPrefab);
        } else {
            cardsManager = GameObject.FindGameObjectWithTag("CardsManager");
        }
        cardsUIManager = cardsManager.GetComponent<CardsUIManager>();
        inventoryController = GetComponent<InventoryController>();
        displayAllCards();
        addListenerForCards();
    }

    private void displayAllCards() {
        List<string> cardsNames = inventoryController.inventoryCards;
        int i = 0;
        foreach (string cardName in cardsNames) {
            GameObject newCard = displayCardAndApplyIndex(cardName, i);
            if (newCard != null) {
                cardsDisplaying.Add(newCard);
                i ++;
            }
        }
    }

    private GameObject displayCardAndApplyIndex(string cardName, int i) {
        (Sprite img, string stats)? cardDetails = cardsUIManager.findCardDetails(cardName);
        if (cardDetails.HasValue) {
            GameObject newCard = Instantiate(cardDisplayPrefab);
            CardRenderer cardRenderer = newCard.GetComponentInChildren<CardRenderer>();
            cardRenderer.cardIndex = i;
            cardRenderer.renderCard(cardDetails.Value.img, cardDetails.Value.stats);
            newCard.transform.parent = cardsInventoryContent.transform;
            cardRenderer.hardAdjustCardDetailsSize();
            return newCard;
        } else {
            Debug.LogWarning($"InventoryUIManager: Card not found in CardsManager - {cardName}");
            return null;
        }
    }

    private void addListenerForCards() {
        foreach (GameObject card in cardsDisplaying) {
            card.AddComponent<Button>();
        }
    }
}
