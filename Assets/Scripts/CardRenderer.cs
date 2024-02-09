using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CardRenderer : MonoBehaviour
{
    private Card card;

    private float FRAME_WIDTH = 64;
    private float FRAME_HEIGHT = 128;

    private float enforcedWidth = 224;
    private float enforcedHeight = 448;

    private int fontSize = 30;

    public bool inventoryCardFocus = false;

    private Sprite cardImage;
    private string cardStats;

    public int cardIndex;

    public void RenderCard(Sprite cardImg, string cardStat) {
        // Render card image
        GameObject cardImgObj = transform.GetChild(1).gameObject;
        Image cardImgRenderer = cardImgObj.GetComponentInChildren<Image>();
        cardImgRenderer.sprite = cardImg;
        cardImage = cardImg;


        // Render card stats
        GameObject cardStatsObj = transform.GetChild(2).gameObject;
        TextMeshProUGUI textComp = cardStatsObj.GetComponentInChildren<TextMeshProUGUI>();
        textComp.text = cardStat;
        cardStats = cardStat;
    }

    public void RenderCard(Card card) {
        this.card = card;

        // Render card image
        GameObject cardImgObj = transform.GetChild(1).gameObject;
        Image cardImgRenderer = cardImgObj.GetComponentInChildren<Image>();
        cardImgRenderer.sprite = card.img;
        cardImage = card.img;


        // Render card stats
        GameObject cardStatsObj = transform.GetChild(2).gameObject;
        TextMeshProUGUI textComp = cardStatsObj.GetComponentInChildren<TextMeshProUGUI>();
        textComp.text = card.stats;
        cardStats = card.stats;
    }

    public void ScaleCardSize(float scale) {
       GetComponent<RectTransform>().localScale = new Vector3(scale, scale, scale);

    //    GameObject cardImgObj = transform.GetChild(0).gameObject;
    //    Vector2 currImgSize = cardImgObj.GetComponent<RectTransform>().sizeDelta;
    //    cardImgObj.GetComponent<RectTransform>().sizeDelta = new Vector2(currImgSize.x * scale, currImgSize.y * scale);
    }

    public void HardAdjustCardDetailsSize() {
        float scale = enforcedWidth / FRAME_WIDTH;

        GameObject cardImgObj = transform.GetChild(1).gameObject;
        Vector2 currImgSize = cardImgObj.GetComponent<RectTransform>().sizeDelta;
        cardImgObj.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(currImgSize.x * scale, currImgSize.y * scale);

        GameObject cardsStatsObj = transform.GetChild(2).gameObject;
        cardsStatsObj.GetComponent<TextMeshProUGUI>().fontSize = fontSize;
        cardsStatsObj.GetComponent<RectTransform>().sizeDelta = new Vector2(131.5f, 58.15f);
    }

    public (Sprite, string) GetCardImgAndStats() {
        return (cardImage, cardStats);
    }

    public Card GetCardDetails() {
        return card;
    }
}
