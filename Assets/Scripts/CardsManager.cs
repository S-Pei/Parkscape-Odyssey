using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class CardsManager : MonoBehaviour
{

    private Dictionary<string, (Sprite img, string stats)> cards = new Dictionary<string, (Sprite, string)>();

    public GameObject cardPrefab;

    public List<string> cardNames = new List<string>();
    public List<Sprite> cardImgs = new List<Sprite>();
    public List<string> cardStats = new List<string>();

    public int cardsToCreate = 0;


    // Start is called before the first frame update
    void Start()
    {
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
}
