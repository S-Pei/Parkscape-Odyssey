using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class RoomJoining : MonoBehaviour
{
    [SerializeField]
    private GameObject lobbyPopUp;
    [SerializeField]
    private GameObject roomSelectionPopUp;
    [SerializeField]
    private TMP_InputField roomCodeInput;
    [SerializeField]
    private TMP_InputField nameInput;

    private PopUpManager roomSelectionPopUpManager;

    public void Start() {
        //  Potentially need some initialisation here for P2P.
        // Ensure pop up is closed at start.
        roomSelectionPopUpManager  = (PopUpManager) roomSelectionPopUp.GetComponent(typeof(PopUpManager));
        roomSelectionPopUpManager.closePopUp();

        // Pull in name from player prefs.
        string name = PlayerPrefs.GetString("name");
        nameInput.text = name;
    }

    public void JoinRoom() {
        // Set name.
        string name = nameInput.text;
        PlayerPrefs.SetString("name", name);
        if (name == "") {
            Debug.Log("No name entered");
            return;
        }

        bool isLeader = false;
        if (roomCodeInput.text == "") {
            Debug.Log("No room code entered");
            return;
        }
        string roomCode = roomCodeInput.text;
        Debug.Log("Joining room " + roomCode);

        // Set Up Lobby
        LobbyManager lobbyManager = (LobbyManager) lobbyPopUp.GetComponent(typeof(LobbyManager));
        lobbyManager.SetUpLobby(roomCode);
        
        // Disable Room Selection Pop Up
        roomSelectionPopUpManager.closePopUp();
    }
}
