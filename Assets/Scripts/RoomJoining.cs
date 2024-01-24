using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class RoomJoining : MonoBehaviour
{
    public GameObject lobbyPopUp;
    public GameObject roomSelectionPopUp;
    public TMP_InputField roomCodeInput;

    public void Start() {
        //  Potentially need some initialisation here for P2P.
    }

    public void JoinRoom() {
        bool isLeader = false;
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
        PopUpManager roomSelectionPopUpManager = (PopUpManager) roomSelectionPopUp.GetComponent(typeof(PopUpManager));
        roomSelectionPopUpManager.closePopUp();
    }

    private void CreateRoom(string roomCode) {
        //  Create the room.
    }

    private bool foundRoom(string roomCode) {
        return roomCode == "123456";
    }
}
