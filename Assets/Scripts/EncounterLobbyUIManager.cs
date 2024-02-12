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
    
    private string encounterId;
    private List<Monster> monsters;

    public void ListPartyMembers(List<string> members) {
        for (int i = 0; i < partyMemberSlots.Count; i++) {
            if (i < members.Count)
                partyMemberSlots[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = members[i];
            else
                partyMemberSlots[i].GetComponent<Image>().enabled = false;
        }
    }

    public void LeaderEncounterLobbyInit(string encounterId, List<Monster> monsters) {
        this.encounterId = encounterId;
        this.monsters = monsters;

        // Add self to party members
        partyMembers.Add(GameState.Instance.MyPlayer.Name);
        partyMemberSlots[0].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = GameState.Instance.MyPlayer.Name;
        
        // Set all other party member slots to inactive
        for (int i = 1; i < partyMemberSlots.Count; i++) {
            partyMemberSlots[i].GetComponent<Image>().enabled = false;
        }
    }

    public void MemberJoinedParty(string member) {
        partyMembers.Add(member);
        partyMemberSlots[partyMembers.Count - 1].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = member;
        partyMemberSlots[partyMembers.Count - 1].GetComponent<Image>().enabled = true;
    }

    public void StartEncounter() {
        SceneManager.LoadScene("Battle", LoadSceneMode.Additive);
        Destroy(gameObject);
    }
}
