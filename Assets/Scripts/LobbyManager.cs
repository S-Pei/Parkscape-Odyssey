using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    private string roomCode;
    public GameObject lobbyPopUp;
    public TMP_Text roomCodeText;

    // Called when code is submitted in the room selection pop up
    public void SetUpLobby(string roomCode) {
        this.roomCode = roomCode;
        roomCodeText.text = roomCode;
        PopUpManager lobbyPopUpManager = (PopUpManager) lobbyPopUp.GetComponent(typeof(PopUpManager));
        lobbyPopUpManager.openPopUp();
    }

    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        // Spin and listen for any new connections or dropped connections
    }


}
