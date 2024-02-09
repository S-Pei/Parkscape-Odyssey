using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class BattleUIManager : MonoBehaviour {
    private BattleManager battleManager;

    [SerializeField]
    private GameObject cardsManagerPrefab;

    [SerializeField]
    private GameObject cardDisplayPrefab;

    [SerializeField]
    private GameObject monsterDisplayPrefab;

    [SerializeField]
    private GameObject cardDescriptionPanel;

    [SerializeField]
    private List<GameObject> displayCards = new List<GameObject>();

    [SerializeField]
    private GameObject enemyPanel;
    
    [SerializeField]
    private GameObject handPanel;

    private GameObject cardsManager;
    private CardsUIManager cardsUIManager;

    private PopUpManager popUpManager;

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

        // Find the PopUpManager for the card description and initialise it to closed
        popUpManager = (PopUpManager) GameObject.FindGameObjectWithTag("CardDescription").GetComponent(typeof(PopUpManager));
        popUpManager.closePopUp();
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

        
        // The positions of the cards must be instantiated *after* displayCards is fully populated
        // This is because the card's index is used to calculate its position
        for (int i = 0; i < displayCards.Count; i++) {
            GameObject cardInstance = displayCards[i];
            (Vector3 cardPosition, Quaternion cardRotation) = getCardPositionAtIndex(i);
            cardInstance.transform.localPosition = cardPosition;
            cardInstance.transform.rotation = cardRotation;
            // StartCoroutine(displayCards[i].GetComponentInChildren<BattleCardRenderer>().ResetCardPosition(0.1f));
        }
    }

    // This is called only when displaying the hand for the first time
    private GameObject displayCardAndApplyIndex(CardName cardName, int i) {
        Card card = cardsUIManager.findCardDetails(cardName);

        // (Vector3 cardPosition, Quaternion cardRotation) = getCardPositionAtIndex(i);
        
        // Create a new instance of the card prefab in the correct world position, with parent handPanel
        GameObject cardInstance = Instantiate(cardDisplayPrefab, handPanel.transform, true);

        // Set the rendered card's index
        BattleCardRenderer cardRenderer = cardInstance.GetComponentInChildren<BattleCardRenderer>();
        cardRenderer.cardIndex = i;


        // Scale and render the card
        cardRenderer.renderCard(card);
        cardRenderer.scaleCardSize(6);
        
        return cardInstance;
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

    public (Vector3, Quaternion) getCardPositionAtIndex(int i) {
        Debug.Log("\n\n");
        int handSize = displayCards.Count;
        Debug.Log("i = " + i + " handSize: " + handSize);
        float zRot = 1.5f;
        float xOffset = handSize >= 5
            ? ((Screen.width / (1.5f * handSize)) - 80)
            : 100.0f;
        float yOffset = 5.0f;

        float align = handSize > 1 ? (i / (handSize - 1.0f)) : 0.5f;
        float rotZ = Mathf.Lerp(handSize * zRot, handSize * -zRot, align);
        float xPos = Mathf.Lerp(handSize * -xOffset, handSize * xOffset, align);
        float yPos = -Mathf.Abs(Mathf.Lerp(handSize * -yOffset, handSize * yOffset, align));
        Debug.Log("Final position for card " + i + " of " + displayCards.Count + " is " + new Vector3((Screen.width / 2) + xPos, yPos + 325, 0) + " with rotation " + rotZ);
        Debug.Log("\n\n");
        return (
            new Vector3((Screen.width / 2) + xPos, yPos + 325, 0),
            Quaternion.Euler(0, 0, rotZ)
        );
    }
    
    public void DisplayCardDescription(Card card) {
        Debug.Log(popUpManager);
        // Instantiate an inventory card prefab for the description
        // GameObject cardDescription = Instantiate(cardDescriptionPrefab, GameObject.FindGameObjectWithTag("BattleCanvas").transform, false);
        Transform cardDescription = cardDescriptionPanel.transform.GetChild(0);
        
        cardDescription.GetChild(1).gameObject.GetComponent<Image>().sprite = card.img;
        cardDescription.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>().text = card.stats;
        popUpManager.openPopUp();
        // CardRenderer renderer = enlargedCard.GetComponentInChildren<CardRenderer>();
        // renderer.renderCard(card);
    }

    public void CloseCardDescriptionPopUp() {
        Debug.Log("Hello WOrld");
        // GameObject closeButton = GameObject.FindGameObjectWithTag("CardDescription");
        // Destroy(closeButton.transform.parent.gameObject);
        popUpManager.closePopUp();
    }
}
