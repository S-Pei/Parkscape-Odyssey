using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootController : MonoBehaviour
{
    private GameObject gameManager;

    private LootUIManager lootUIManager;

    private Destroyer destroyer;

    [SerializeField]
    private GameObject cardsManagerPrefab;

    private GameObject cardsManager;

    private CardsUIManager cardsUIManager;

    [SerializeField]
    private int cardsNumber = 2;
    
    private Card focusedCard; 

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager");

        if (GameObject.FindGameObjectsWithTag("CardsManager").Length <= 0) {
            cardsManager = Instantiate(cardsManagerPrefab);
            cardsManager.tag = "CardsManager";
        } else {
            cardsManager = GameObject.FindGameObjectWithTag("CardsManager");
        }
        cardsUIManager = cardsManager.GetComponent<CardsUIManager>();
        
        lootUIManager = GetComponent<LootUIManager>();

        destroyer = GetComponent<Destroyer>();

        // Generate cards loot and display on screen
        Card[] cardsLoot = generateCardsLoot();
        lootUIManager.displayCardsLoot(cardsLoot);
    }


    //TODO: Implement proper loot drop system, now it just randoms the card drops
    private Card[] generateCardsLoot() {
        Card[] allCards = cardsUIManager.getAllAvailableCards();
        List<Card> cardsLoot = new();

        System.Random random = new();
        for (int i = 0; i < cardsNumber; i++) {
            Card cardLoot = allCards[random.Next(allCards.Length)];
            cardsLoot.Add(cardLoot);
        }

        return cardsLoot.ToArray();
    }


    // Loot selection confirmation
    public void confirmCardSelection() {
        if (focusedCard != null) {
            addCardToPlayerDeck(focusedCard);
            lootUIManager.closeCardConfirmationPopUp();
            destroyer.DestroySelf();
            return;
        }
        Debug.LogWarning("LootController: no focused card set.");
    }

    public void denyCardSelection() {
        lootUIManager.closeCardConfirmationPopUp();
    }

    private void addCardToPlayerDeck(Card card) {
        GameManager gameManagerScript = gameManager.GetComponent<GameManager>();
        gameManagerScript.addCardToDeck(card);
    }

    public void setFocusedCard(Card card) {
        focusedCard = card;
    }
}
