using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BattleUIManager : MonoBehaviour {
    private BattleManager battleManager;

    [SerializeField]
    private GameObject cardsManagerPrefab;
    [SerializeField]
    private GameObject cardDisplayPrefab;
    [SerializeField]
    private GameObject monsterDisplayPrefab;
    [SerializeField]
    private List<GameObject> displayCards = new List<GameObject>();
    [SerializeField]
    private GameObject enemyPanel;
    private GameObject cardsManager;
    private CardsUIManager cardsUIManager;

    void Awake () {
        // Find the BattleManager
        battleManager = (BattleManager) GetComponent(typeof(BattleManager));

        // Find an existing CardManager or instantiate one if not found
        if (GameObject.FindGameObjectsWithTag("CardsManager").Length <= 0) {
            cardsManager = Instantiate(cardsManagerPrefab);
            cardsManager.tag = "CardsManager";
        } else {
            cardsManager = GameObject.FindGameObjectWithTag("CardsManager");
        }

        // Extract the CardsUIManager from the CardManager
        cardsUIManager = cardsManager.GetComponent<CardsUIManager>();
    }

    public void DisplayHand(List<CardName> cards) {
        // Instantiate the hand and add the card objects to displayCards
        for (int i = 0; i < cards.Count; i++) {
            CardName card = cards[i];
            GameObject newCard = displayCardAndApplyIndex(card, i);
            if (newCard != null) {
                displayCards.Add(newCard);
            }
        }

        for (int i = 0; i < BattleManager.HAND_SIZE; i++) {
            GameObject card = displayCards[i];
            (Vector3 cardPosition, Quaternion cardRotation) = getCardPositionAtIndex(i);
            card.transform.position = cardPosition;
            card.transform.rotation = cardRotation;
        }
    }

    public void RemoveCardFromHand(int index) {
        // Remove the card from the displayCards list and destroy it
        GameObject card = displayCards[index];
        displayCards.RemoveAt(index);

        // Update the card index of the remaining cards
        for (int i = 0; i < displayCards.Count; i++) {
            GameObject cardInstance = displayCards[i];
            CardRenderer cardRenderer = cardInstance.GetComponentInChildren<CardRenderer>();
            cardRenderer.cardIndex = i;
        }

        // Call the repositioning coroutine in each card's renderer
        for (int i = 0; i < displayCards.Count; i++) {
            Debug.Log("Repositioning card " + i + " of " + displayCards.Count);
            GameObject cardInstance = displayCards[i];
            BattleCardRenderer cardRenderer = cardInstance.GetComponentInChildren<BattleCardRenderer>();
            StartCoroutine(cardRenderer.ResetCardPosition(0.2f));
        }

        Destroy(card);
    }

    public void DisplayMonster(Monster monster) {
        GameObject monsterInstance = Instantiate(monsterDisplayPrefab, enemyPanel.transform, false);
        MonsterRenderer monsterRenderer = monsterInstance.GetComponentInChildren<MonsterRenderer>();
        monsterRenderer.renderMonster(monster);
    }

    private GameObject displayCardAndApplyIndex(CardName cardName, int i) {
        Card card = cardsUIManager.findCardDetails(cardName);
        
        // Create a new instance of the card prefab, with parent BattleCanvas
        GameObject cardInstance = Instantiate(cardDisplayPrefab);
        cardInstance.transform.SetParent(
            GameObject.FindGameObjectWithTag("BattleCanvas").transform, true
        );

        // Set the rendered card's index and render the card
        CardRenderer cardRenderer = cardInstance.GetComponentInChildren<CardRenderer>();
        cardRenderer.cardIndex = i;
        cardRenderer.renderCard(card);
        cardRenderer.scaleCardSize(6);

        return cardInstance;
    }

    public (Vector3, Quaternion) getCardPositionAtIndex(int i) {
        Debug.Log("Getting card position for index" + i + "; hand size:" + this.displayCards.Count);
        int handSize = this.displayCards.Count;
        float zRot = 1.5f;
        float xOffset = handSize >= 5
            ? ((Screen.width / (1.5f * handSize)) - 80)
            : 100.0f;
        float yOffset = 5.0f;

        float align = handSize > 1 ? (i / (handSize - 1.0f)) : 0.5f;
        float rotZ = Mathf.Lerp(handSize * zRot, handSize * -zRot, align);
        float xPos = Mathf.Lerp(handSize * -xOffset, handSize * xOffset, align);
        float yPos = -Mathf.Abs(Mathf.Lerp(handSize * -yOffset, handSize * yOffset, align));
        return (
            new Vector3((Screen.width / 2) + xPos, yPos + 325, 0),
            Quaternion.Euler(0, 0, rotZ)
        );
    }
}
