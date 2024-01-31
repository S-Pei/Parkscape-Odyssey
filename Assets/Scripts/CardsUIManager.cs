using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class CardsUIManager : MonoBehaviour
{

    private List<Card> cardsLs = new List<Card>();
    private Dictionary<string, (Sprite img, string stats)> cards = new Dictionary<string, (Sprite, string)>();

    [SerializeField]
    private GameObject cardPrefab;

    [SerializeField]
    private List<string> cardNames = new List<string>();

    [SerializeField]
    private List<Sprite> cardImgs = new List<Sprite>();
    
    [SerializeField]
    private List<string> cardStats = new List<string>();

    [SerializeField]
    private int cardsToCreate = 0;

    void Awake() {
        if (cardNames.Count != cardImgs.Count 
            || cardNames.Count != cardStats.Count 
            || cardImgs.Count != cardStats.Count) {
            Debug.LogWarning("Cards Manager: Card Names, Card Images and Card Stats provided do not have the same amount. Please check these fields.");
        }

        int i;
        for (i = 0; i < Math.Min(cardNames.Count, cardImgs.Count); i++) {
            cards.Add(cardNames[i], (cardImgs[i], cardStats[i]));
            Debug.Log($"Cards Manager: Card initialised - {cardNames[i]}");
        }        
    }

    // // Update is called once per frame
    // void Update()
    // {
    //     if (cardsToCreate > 0) {
    //         createCard("baseAtk");
    //         cardsToCreate -= 1;
    //     }
    // }

    public (Sprite, string)? findCardDetails(string cardName) {
        if (cards.ContainsKey(cardName)) {
            return cards[cardName];
        }
        return null;
    }


    private void createCard(string cardName) {
        if (cards.ContainsKey(cardName)) {
            // Render card image
            GameObject cardImgObj = cardPrefab.transform.GetChild(0).gameObject;
            Sprite cardImg = cards[cardName].img;
            SpriteRenderer cardImgRenderer = cardImgObj.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;
            cardImgRenderer.sprite = cardImg;

            // Render card stats
            GameObject cardStatsObj = cardPrefab.transform.GetChild(1).gameObject;
            string cardStat = cards[cardName].stats;
            TextMeshPro textComp = cardStatsObj.GetComponent(typeof(TextMeshPro)) as TextMeshPro;
            textComp.text = cardStat;

            Instantiate(cardPrefab);
        } else {
            Debug.Log($"Cards Manager: Card image not found - {cardName}. Cannot create card.");
        }
    }

    public Card[] getAllAvailableCards() {
        return cardsLs.ToArray();
    }
}
