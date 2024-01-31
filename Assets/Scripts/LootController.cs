using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private float[] generateCardsRarityProbability(EnemyLevel enemyLevel) {
        switch(enemyLevel) {
            case EnemyLevel.EASY:
                return new [] {0.7f, 0.3f, 0};
            case EnemyLevel.MEDIUM:
                return new [] {0.6f, 0.35f, 0.05f};
            case EnemyLevel.HARD:
                return new [] {0.30f, 0.58f, 0.12f};
            case EnemyLevel.BOSS:
                return new [] {0f, 0.70f, 0.30f};
            default:
                return null;
        }
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
