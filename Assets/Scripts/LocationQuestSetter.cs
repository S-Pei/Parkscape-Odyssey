using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class LocationQuestUISetter : MonoBehaviour {
    [SerializeField]
    private TMP_Text questText;

    [SerializeField]
    private GameObject referenceImage;

    [SerializeField]
    private TMP_Text progressValueText;

    [SerializeField]
    private TMP_Text progressTargetText;

    [SerializeField]
    private GameObject completedOverlay;

    public void Set(LocationQuest quest) {
        questText.text = quest.ToString();
        referenceImage.GetComponent<Image>().sprite = CreateRefSprite(quest.ReferenceImage);
        progressValueText.GetComponent<TMP_Text>().text = quest.Progress.ToString();
        progressTargetText.GetComponent<TMP_Text>().text = quest.Target.ToString();
        completedOverlay.SetActive(quest.IsCompleted());
    }

    private Sprite CreateRefSprite(Texture2D refImage) {
        Debug.Log("Creating image sprite" + refImage);
        Vector3 size = referenceImage.GetComponent<RectTransform>().sizeDelta;
        return Sprite.Create(refImage, new Rect(0, 0, size.x, size.y), new Vector2(0.5f, 0.5f));
    }
}
