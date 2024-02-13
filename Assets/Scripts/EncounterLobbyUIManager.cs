using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EncounterLobbyUIManager : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> partyMemberSlots;

    [SerializeField]
    private List<string> partyMembers;

    [SerializeField]
    private GameObject enemyDetailsPanel;

    [SerializeField]
    private GameObject startEncounterButton;
    
    private string encounterId;
    private List<Monster> monsters;

    private EncounterController encounterController;

    public void ListPartyMembers(List<string> members) {
        for (int i = 0; i < partyMemberSlots.Count; i++) {
            if (i < members.Count)
                partyMemberSlots[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = members[i];
            else
                partyMemberSlots[i].GetComponent<Image>().enabled = false;
        }
    }

    public void EncounterLobbyInit(string encounterId, List<Monster> monsters, bool isLeader) {
        this.encounterId = encounterId;
        this.monsters = monsters;

        encounterController = GameObject.FindGameObjectWithTag("EncounterManager").GetComponent<EncounterController>();

        DisplayMonsterDetails();

        // Add self to party members
        if (isLeader) {
            partyMembers.Add(GameState.Instance.MyPlayer.Name);
            partyMemberSlots[0].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = GameState.Instance.MyPlayer.Name;
        } else {
            startEncounterButton.SetActive(false);
        }

        // Set all other party member slots to inactive
        for (int i = partyMembers.Count; i < partyMemberSlots.Count; i++) {
            partyMemberSlots[i].GetComponent<Image>().enabled = false;
        }
    }

    private void DisplayMonsterDetails() {
        enemyDetailsPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"Enemy Level: {monsters[0].level}";
        enemyDetailsPanel.transform.GetChild(1).GetComponent<Image>().sprite = monsters[0].img;
    }

    public void MemberJoinedParty(string member) {
        partyMembers.Add(member);
        partyMemberSlots[partyMembers.Count - 1].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = member;
        partyMemberSlots[partyMembers.Count - 1].GetComponent<Image>().enabled = true;
    }

    public void StartEncounter() {
        encounterController.LeaderStartEncounter();
        SceneManager.LoadScene("Battle", LoadSceneMode.Additive);
        Destroy(gameObject);
    }
}
