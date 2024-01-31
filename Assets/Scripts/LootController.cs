using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootController : MonoBehaviour
{
    private LootUIManager lootUIManager;

    [SerializeField]
    private GameObject cardsManagerPrefab;

    private GameObject cardsManager;

    private CardsUIManager cardsUIManager;

    [SerializeField]
    private int cardsNumber = 2;

    // Start is called before the first frame update
    void Start()
    {
        if (GameObject.FindGameObjectsWithTag("CardsManager").Length <= 0) {
            cardsManager = Instantiate(cardsManagerPrefab);
            cardsManager.tag = "CardsManager";
        } else {
            cardsManager = GameObject.FindGameObjectWithTag("CardsManager");
        }
        cardsUIManager = cardsManager.GetComponent<CardsUIManager>();
        
        lootUIManager = GetComponent<LootUIManager>();

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
}
