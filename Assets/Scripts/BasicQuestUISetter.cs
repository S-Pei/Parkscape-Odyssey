using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class BasicQuestUISetter : MonoBehaviour {
    [SerializeField]
    private TMP_Text questText;

    [SerializeField]
    private GameObject referenceImage;

    [SerializeField]
    private GameObject progressBar;

    [SerializeField]
    private TMP_Text progressValueText;

    [SerializeField]
    private TMP_Text progressTargetText;

    [SerializeField]
    private GameObject completedOverlay;

    public void Set(BasicQuest quest) {
        questText.text = quest.ToString();
        referenceImage.GetComponent<Image>().sprite = CreateRefSprite(quest.ReferenceImage);
        progressBar.GetComponent<Slider>().maxValue = quest.Target;
        progressBar.GetComponent<Slider>().value = quest.Progress;
        progressValueText.GetComponent<TMP_Text>().text = quest.Progress.ToString();
        progressTargetText.GetComponent<TMP_Text>().text = quest.Target.ToString();
        completedOverlay.SetActive(quest.IsCompleted);
    }

    private Sprite CreateRefSprite(Texture2D refImage) {
        Vector3 size = referenceImage.GetComponent<RectTransform>().sizeDelta;
        return Sprite.Create(refImage, new Rect(0, 0, size.x, size.y), new Vector2(0.5f, 0.5f));
    }
}
