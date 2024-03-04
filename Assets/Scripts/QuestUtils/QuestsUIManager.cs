using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestsUIManager : MonoBehaviour {
    [SerializeField] 
    private GameObject basicQuestsDisplay;

    [SerializeField]
    private GameObject locationQuestsDisplay;

    [SerializeField]
    private GameObject basicQuestsContent;

    [SerializeField]
    private GameObject locationQuestsContent;

    [SerializeField]
    private GameObject basicQuestPrefab;

    [SerializeField]
    private GameObject locationQuestPrefab;

    [SerializeField]
    private TMP_Text basicQuestsButton;

    [SerializeField]
    private TMP_Text locationQuestsButton;

    public void ToggleBasicQuestsDisplay() {
        basicQuestsDisplay.SetActive(true);
        locationQuestsDisplay.SetActive(false);
        locationQuestsButton.color = new Color(1, 1, 1, 0.5f);
        basicQuestsButton.color = new Color(1, 1, 1, 1f);
    }

    public void ToggleLocationQuestsDisplay() {
        basicQuestsDisplay.SetActive(false);
        locationQuestsDisplay.SetActive(true);
        locationQuestsButton.color = new Color(1, 1, 1, 1f);
        basicQuestsButton.color = new Color(1, 1, 1, 0.5f);
    }

    public void CreateBasicQuestDisplay(BasicQuest quest) {
        GameObject basicQuest = Instantiate(basicQuestPrefab, basicQuestsContent.transform);
        basicQuest.GetComponent<BasicQuestUISetter>().Set(quest);
    }

    public void CreateLocationQuestDisplay(LocationQuest quest) {
        GameObject locationQuest = Instantiate(locationQuestPrefab, locationQuestsContent.transform);
        locationQuest.GetComponent<LocationQuestUISetter>().Set(quest);
    }

    public void SetUp(List<BasicQuest> basicQuests, List<LocationQuest> locationQuests) {
        foreach (BasicQuest quest in basicQuests) {
            if (!quest.HasNotStarted()) {
                CreateBasicQuestDisplay(quest);
            }
        }

        foreach (LocationQuest quest in locationQuests) {
            if (!quest.HasNotStarted()) {
                CreateLocationQuestDisplay(quest);
            }
        }

        ToggleBasicQuestsDisplay();
    }
}
