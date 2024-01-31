using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootUIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject cardPrefab;

    [SerializeField]
    private GameObject cardsSelectionPanel;

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
    }

    private void displayCard(Card card) {
        GameObject newCard = Instantiate(cardPrefab, cardsSelectionPanel.transform);
        CardRenderer cardRenderer = newCard.GetComponent<CardRenderer>();
        cardRenderer.renderCard(card);
        cardRenderer.scaleCardSize(5);
    }
}
