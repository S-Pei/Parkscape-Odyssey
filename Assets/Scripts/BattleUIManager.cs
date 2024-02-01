using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BattleUIManager : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler
{
    private BattleManager battleManager;

    [SerializeField]
    private GameObject cardsManagerPrefab;
    [SerializeField]
    private GameObject cardDisplayPrefab;
    [SerializeField]
    private List<GameObject> displayCards = new List<GameObject>();
    private GameObject cardsManager;
    private CardsUIManager cardsUIManager;

    // Start is called before the first frame update
    // Wait until the BattleManager has fully initialised
    IEnumerator Start()
    {
        battleManager = (BattleManager) GetComponent(typeof(BattleManager));

        if (GameObject.FindGameObjectsWithTag("CardsManager").Length <= 0) {
            cardsManager = Instantiate(cardsManagerPrefab);
            cardsManager.tag = "CardsManager";
        } else {
            cardsManager = GameObject.FindGameObjectWithTag("CardsManager");
        }
        cardsUIManager = cardsManager.GetComponent<CardsUIManager>();

        // Wait until the hand is generated before displaying the cards
        yield return new WaitUntil(
            () => battleManager.Hand.Count == BattleManager.HAND_SIZE
        );

        DisplayHand(battleManager.Hand);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisplayHand(List<string> cards) {
        // Instantiate the hand and add the card objects to displayCards
        for (int i = 0; i < cards.Count; i++) {
            string card = cards[i];
            GameObject newCard = displayCardAndApplyIndex(card, i);
            if (newCard != null) {
                displayCards.Add(newCard);
            }
        }

        // Fan out the cards
        int handSize = displayCards.Count;
        float zRot = 1.5f;
        float xOffset = (Screen.width / (1.5f *handSize)) - 80;
        float yOffset = 5.0f;

        for (int i = 0; i < handSize; i++) {
            GameObject card = displayCards[i];
            float align = i / (handSize - 1.0f);
            float rotZ = Mathf.Lerp(handSize * zRot, handSize * -zRot, align);
            float xPos = Mathf.Lerp(handSize * -xOffset, handSize * xOffset, align);
            float yPos = -Mathf.Abs(Mathf.Lerp(handSize * -yOffset, handSize * yOffset, align));
            card.transform.position = new Vector3((Screen.width / 2) + xPos, yPos + 300, 0);
            card.transform.rotation = Quaternion.Euler(0, 0, rotZ);
        }
    
    }

    private GameObject displayCardAndApplyIndex(string cardName, int i) {
        (Sprite img, string stats)? cardDetails = cardsUIManager.findCardDetails(cardName);
        if (cardDetails.HasValue) {
            // Create a new instance of the card prefab, with parent BattleCanvas
            GameObject newCard = Instantiate(cardDisplayPrefab);
            newCard.transform.SetParent(
                GameObject.FindGameObjectWithTag("BattleCanvas").transform, true
            );
            CardRenderer cardRenderer = newCard.GetComponentInChildren<CardRenderer>();
            cardRenderer.cardIndex = i;
            cardRenderer.renderCard(cardDetails.Value.img, cardDetails.Value.stats);
            // cardRenderer.hardAdjustCardDetailsSize();
            cardRenderer.scaleCardSize(6);
            return newCard;
        } else {
            Debug.LogWarning($"InventoryUIManager: Card not found in CardsManager - {cardName}");
            return null;
        }
    }

    public void OnBeginDrag(PointerEventData eventData) {
        Debug.Log("OnBeginDrag");
    }

    public void OnEndDrag(PointerEventData eventData) {
        Debug.Log("OnEndDrag");
    }

    public void OnPointerDown(PointerEventData eventData) {
        Debug.Log("OnPointerDown");
    }
}
