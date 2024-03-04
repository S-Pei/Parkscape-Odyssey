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
        // referenceImage.GetComponent<Image>().sprite = CreateRefSprite(quest.ReferenceImage);
        referenceImage.GetComponent<Image>().sprite = CreateRefSprite(quest);
        progressValueText.GetComponent<TMP_Text>().text = quest.Progress.ToString();
        progressTargetText.GetComponent<TMP_Text>().text = quest.Target.ToString();
        completedOverlay.SetActive(quest.IsCompleted());
    }

    // private Sprite CreateRefSprite(Texture2D refImage) {
    private Sprite CreateRefSprite(LocationQuest quest) {
        Debug.Log("Creating image sprite for" + quest.Label);

        // Load the image as byte[] from disk
        byte[] refImage = FileUtils.Load<byte[]>(quest.Label + ".jpg", "referenceImages");

        // Create a Texture2D from the byte[]
        Texture2D refImageTexture = new Texture2D(2, 2);
        refImageTexture.LoadImage(refImage); 
        Texture2D newRef = VecSearchManager.ResizeImage(refImageTexture, 150, 150);

        Vector3 size = referenceImage.GetComponent<RectTransform>().sizeDelta;
        return Sprite.Create(newRef, new Rect(0, 0, size.x, size.y), new Vector2(0.5f, 0.5f));
    }
}
