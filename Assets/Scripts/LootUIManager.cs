using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LootUIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject cardDisplayPrefab;

    [SerializeField]
    private GameObject cardsSelectionPanel;

    [SerializeField]
    private GameObject popUpPanel;

    private List<GameObject> cardsDisplaying;

    // private LootController lootController;

    // Start is called before the first frame update
    void Start()
    {
        // lootController = GetComponent<LootController>();
    }

   
   // Display cards loot
    public void displayCardsLoot(Card[] cards) {
        foreach(Card card in cards) {
            displayCard(card);
        }

        addListenerForCards();
    }

    private void displayCard(Card card) {
        GameObject newCard = Instantiate(cardDisplayPrefab, cardsSelectionPanel.transform);
        CardRenderer cardRenderer = newCard.GetComponent<CardRenderer>();
        cardRenderer.renderCard(card);
        cardRenderer.scaleCardSize(5);
        cardsDisplaying.Add(newCard);
    }

    private void addListenerForCards() {
        foreach (GameObject card in cardsDisplaying) {
            Button btn = card.AddComponent<Button>();
            btn.onClick.AddListener(() => { openCardConfirmationPopUp(card); });
        }
    }

    private void openCardConfirmationPopUp(GameObject card) {
        (Sprite img, string stats) = card.GetComponent<CardRenderer>().getCardImgAndStats();

        GameObject focusedCard = Instantiate(cardDisplayPrefab);
        CardRenderer cardRenderer = focusedCard.GetComponentInChildren<CardRenderer>();
        cardRenderer.renderCard(img, stats);
        
        GameObject popUpCardDisplayPanel = popUpPanel.transform.GetChild(1).gameObject;
        focusedCard.transform.parent = popUpCardDisplayPanel.transform;
        focusedCard.tag = "CardsLootFocusedCard";
        cardRenderer.scaleCardSize(7.5f);

        popUpPanel.SetActive(true);
    }

    public void closeCardConfirmationPopUp() {
        GameObject[] focusedcard = GameObject.FindGameObjectsWithTag("CardsLootFocusedCard");
        if (focusedcard.Length != 0) {
            Destroy(focusedcard[0]);
        }

        popUpPanel.SetActive(false);
    }
}
