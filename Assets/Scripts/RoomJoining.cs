using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class RoomJoining : MonoBehaviour
{
    public GameObject lobbyPopUp;
    public GameObject roomSelectionPopUp;
    public TMP_InputField roomCodeInput;

    private PopUpManager roomSelectionPopUpManager;

    public void Start() {
        //  Potentially need some initialisation here for P2P.
        // Ensure pop up is closed at start.
        roomSelectionPopUpManager  = (PopUpManager) roomSelectionPopUp.GetComponent(typeof(PopUpManager));
        roomSelectionPopUpManager.closePopUp();
    }

    public void JoinRoom() {
        bool isLeader = false;
        if (roomCodeInput == null) {
            Debug.Log("No room code entered");
            return;
        }
        string roomCode = roomCodeInput.text;
        Debug.Log("Joining room: " + roomCode);
        if (foundRoom(roomCode)) {
            //  Join the room.
        } else {
            isLeader = true;
            CreateRoom(roomCode);
        }

        // Set Up Lobby
        LobbyManager lobbyManager = (LobbyManager) lobbyPopUp.GetComponent(typeof(LobbyManager));
        lobbyManager.SetUpLobby(roomCode, isLeader);
        
        // Disable Room Selection Pop Up
        roomSelectionPopUpManager.closePopUp();
    }

    private void CreateRoom(string roomCode) {
        //  Create the room.
    }

    private bool foundRoom(string roomCode) {
        return roomCode == "123456";
    }
}
