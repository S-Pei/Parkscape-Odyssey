using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CardRenderer : MonoBehaviour
{
    public void renderCard(Sprite cardImg, string cardStat) {
        // Render card image
        GameObject cardImgObj = transform.GetChild(0).gameObject;
        Image cardImgRenderer = cardImgObj.GetComponentInChildren<Image>();
        cardImgRenderer.sprite = cardImg;


        // Render card stats
        GameObject cardStatsObj = transform.GetChild(1).gameObject;
        TextMeshProUGUI textComp = cardStatsObj.GetComponentInChildren<TextMeshProUGUI>();
        textComp.text = cardStat;
    }

    public void scaleCardSize(int scale) {
       Vector2 currSize = GetComponent<RectTransform>().sizeDelta;
       GetComponent<RectTransform>().sizeDelta = new Vector2(currSize.x * scale, currSize.y * scale);

       GameObject cardImgObj = transform.GetChild(0).gameObject;
       Vector2 currImgSize = cardImgObj.GetComponent<RectTransform>().sizeDelta;
       cardImgObj.GetComponent<RectTransform>().sizeDelta = new Vector2(currImgSize.x * scale, currImgSize.y * scale);

    }
}
