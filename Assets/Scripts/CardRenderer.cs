using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CardRenderer : MonoBehaviour
{
    private float FRAME_WIDTH = 64;
    private float FRAME_HEIGHT = 128;

    private float enforcedWidth = 224;
    private float enforcedHeight = 448;

    public bool inventoryCardFocus = false;

    private Sprite cardImage;
    private string cardStats;

    public int cardIndex;

    public void renderCard(Sprite cardImg, string cardStat) {
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

    public void scaleCardSize(float scale) {
       GetComponent<RectTransform>().localScale = new Vector3(scale, scale, scale);

    //    GameObject cardImgObj = transform.GetChild(0).gameObject;
    //    Vector2 currImgSize = cardImgObj.GetComponent<RectTransform>().sizeDelta;
    //    cardImgObj.GetComponent<RectTransform>().sizeDelta = new Vector2(currImgSize.x * scale, currImgSize.y * scale);
    }

    public void hardAdjustCardDetailsSize() {
        float scale = enforcedWidth / FRAME_WIDTH;

        GameObject cardImgObj = transform.GetChild(1).gameObject;
        Vector2 currImgSize = cardImgObj.GetComponent<RectTransform>().sizeDelta;
        cardImgObj.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(currImgSize.x * scale, currImgSize.y * scale);
    }

    public (Sprite, string) getCardImgAndStats() {
        return (cardImage, cardStats);
    }
}
