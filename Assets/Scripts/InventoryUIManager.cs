using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;
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

    private GameObject cardsManager;

    CardsUIManager cardsUIManager;
    InventoryController inventoryController;

    public List<GameObject> cardsDisplaying = new();

    // Start is called before the first frame update
    void Start()
    {
        closeCardTradePopUp();

        if (GameObject.FindGameObjectsWithTag("CardsManager").Length <= 0) {
            cardsManager = Instantiate(cardsManagerPrefab);
            cardsManager.tag = "CardsManager";
        } else {
            cardsManager = GameObject.FindGameObjectWithTag("CardsManager");
        }
        cardsUIManager = cardsManager.GetComponent<CardsUIManager>();
        inventoryController = GetComponent<InventoryController>();
        displayAllCards();
        addListenerForCards();
    }

    private void displayAllCards() {
        List<CardName> cardsNames = inventoryController.inventoryCards;
        int i = 0;
        foreach (CardName cardName in cardsNames) {
            GameObject newCard = displayCardAndApplyIndex(cardName, i);
            if (newCard != null) {
                cardsDisplaying.Add(newCard);
                i ++;
            }
        }
    }

    private GameObject displayCardAndApplyIndex(CardName cardName, int i) {
        Card cardDetails = cardsUIManager.findCardDetails(cardName);
        if (cardDetails == null) {
            Debug.LogWarning($"InventoryUIManager: Card not found in CardsManager - {cardName}");
            return null;
        }

        GameObject newCard = Instantiate(cardDisplayPrefab);
        CardRenderer cardRenderer = newCard.GetComponentInChildren<CardRenderer>();
        cardRenderer.cardIndex = i;
        cardRenderer.renderCard(cardDetails);
        newCard.transform.parent = cardsInventoryContent.transform;
        cardRenderer.hardAdjustCardDetailsSize();
        cardRenderer.scaleCardSize(1);
        return newCard;
    }

    private void addListenerForCards() {
        foreach (GameObject card in cardsDisplaying) {
            Button btn = card.AddComponent<Button>();
            btn.onClick.AddListener(() => { openCardTradePopUp(card); });
        }
    }

    private void openCardTradePopUp(GameObject card) {
        Card cardDetails = card.GetComponent<CardRenderer>().getCardDetails();

        GameObject popUpCardDisplayPanel = popUpPanel.transform.GetChild(1).gameObject;
        GameObject focusedCard = Instantiate(cardDisplayPrefab, popUpCardDisplayPanel.transform);
        focusedCard.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        CardRenderer cardRenderer = focusedCard.GetComponentInChildren<CardRenderer>();
        cardRenderer.renderCard(cardDetails);
        
        focusedCard.tag = "CardsInventoryFocusedCard";
        cardRenderer.scaleCardSize(10f);

        popUpPanel.SetActive(true);
    }

    public void closeCardTradePopUp() {
        GameObject[] focusedcard = GameObject.FindGameObjectsWithTag("CardsInventoryFocusedCard");
        if (focusedcard.Length != 0) {
            Destroy(focusedcard[0]);
        }

        popUpPanel.SetActive(false);
    }

    public void DestroySelf() {
        Destroy(cardsManager);
        Destroy(gameObject);
    }
}
