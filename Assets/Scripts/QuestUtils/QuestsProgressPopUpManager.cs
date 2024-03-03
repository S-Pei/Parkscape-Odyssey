using UnityEngine;
using TMPro;

public class QuestsProgressPopUpManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI questTitle;
    [SerializeField]
    private TextMeshProUGUI questStatus;
    [SerializeField]
    private GameObject basicQuestPrefab;
    [SerializeField]
    private GameObject locationQuestPrefab;

    private readonly string COMPLETED_QUEST_MSG = "Quest Completed!";
    private readonly string PROGRESS_QUEST_MSG = "You've made progress on a quest!";
    private readonly string FAILED_QUEST_MSG = "Hmm... This doesn't seem like something you're looking for. Try again!";
    private readonly string SUCCESS_FOUND_OBJ_MSG = "You have found ";


    private static QuestsProgressPopUpManager instance;

    
    public static QuestsProgressPopUpManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new();
            }
            return instance;
        }
    }

    
    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowQuestResultPopUp(Quest quest)
    {
        bool success = quest != null;
        gameObject.SetActive(true);
        questTitle.text = FoundObjString(quest, success);
        questStatus.text = QuestStatusString(quest, success);
        if (success) {
            GameObject questTile;
            if (quest is LocationQuest) {
                questTile = Instantiate(locationQuestPrefab, gameObject.transform);
                questTile.GetComponent<LocationQuestUISetter>().Set((LocationQuest) quest);
            } else {
                questTile = Instantiate(basicQuestPrefab, gameObject.transform);
                questTile.GetComponent<BasicQuestUISetter>().Set((BasicQuest) quest);
            }
            questTile.transform.localPosition = new Vector3(0, 0, 0);
        }
    }
    
    public void CloseQuestResultPopUp()
    {
        gameObject.SetActive(false);
    }

    private string FoundObjString(Quest quest, bool success) {
        if (success) {
            if (quest is LocationQuest) {
                return SUCCESS_FOUND_OBJ_MSG + "the " + quest.Label + "!";
            } else {
                return SUCCESS_FOUND_OBJ_MSG + "a " + quest.Label + "!";
            }
        }
        return FAILED_QUEST_MSG;
    }

    private string QuestStatusString(Quest quest, bool success) {
        if (success) {
            return quest.IsCompleted() ? COMPLETED_QUEST_MSG : PROGRESS_QUEST_MSG;
        }
        return "";
    }


}
